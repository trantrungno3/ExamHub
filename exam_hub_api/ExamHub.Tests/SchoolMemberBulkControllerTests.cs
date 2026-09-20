using System.Reflection;
using ExamHub.API.Controllers.School;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Common;
using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TVT.Core;

namespace ExamHub.Tests;

/// <summary>Fake bulk service with call counters — the controller must not delegate
/// when file metadata is already invalid.</summary>
sealed class FakeBulkService : ISchoolMemberBulkService
{
    public int PreviewCalls { get; private set; }
    public int ImportCalls { get; private set; }
    public int AddCalls { get; private set; }
    public int TemplateCalls { get; private set; }
    public string? ThrowInvalidData { get; set; }

    public Task<SchoolMemberImportPreviewResponse> PreviewAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default)
    {
        PreviewCalls++;
        if (ThrowInvalidData is not null) throw new InvalidDataException(ThrowInvalidData);
        return Task.FromResult(new SchoolMemberImportPreviewResponse(1, 0,
            [new(2, "teacher1", "Teacher", null, null, true, [])]));
    }

    public Task<SchoolMemberBulkResult> ImportAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default)
    {
        ImportCalls++;
        if (ThrowInvalidData is not null) throw new InvalidDataException(ThrowInvalidData);
        return Task.FromResult(new SchoolMemberBulkResult(1, 0, []));
    }

    public Task<SchoolMemberBulkResult> AddAsync(
        SchoolMemberBulkAddRequest request, CancellationToken ct = default)
    {
        AddCalls++;
        return Task.FromResult(new SchoolMemberBulkResult(request.UserIds.Count, 0, []));
    }

    public byte[] BuildTemplate()
    {
        TemplateCalls++;
        return [1, 2, 3];
    }
}

public class SchoolMemberBulkControllerTests
{
    private const long MaxImportBytes = 10 * 1024 * 1024;

    private static FormFile File(string name, long length) => new(Stream.Null, 0, length, "file", name);

    private static (SchoolMemberController Controller, FakeBulkService Bulk) Build()
    {
        var bulk = new FakeBulkService();
        return (new SchoolMemberController(null!, bulk), bulk);
    }

    [Fact]
    public void BulkEndpointsRequireAdminRole()
    {
        var methods = new[] { "Preview", "BulkImport", "BulkAdd", "DownloadImportTemplate" };
        foreach (var name in methods)
        {
            var authorize = typeof(SchoolMemberController).GetMethod(name)!
                .GetCustomAttribute<AuthorizeAttribute>();
            Assert.Equal("Admin", authorize!.Roles);
        }
    }

    [Fact]
    public void UploadEndpointsUseWriteHeavyRateLimiting()
    {
        foreach (var name in new[] { "Preview", "BulkImport" })
        {
            var limiting = typeof(SchoolMemberController).GetMethod(name)!
                .GetCustomAttributes()
                .Single(x => x.GetType().Name == "EnableRateLimitingAttribute");
            Assert.Equal("write-heavy", limiting.GetType().GetProperty("PolicyName")!.GetValue(limiting));
        }
    }

    [Theory]
    [InlineData("members.xlsx", 0, "File import không được để trống.")]
    [InlineData("members.csv", 10, "Chỉ chấp nhận file Excel (.xlsx).")]
    [InlineData("members.xlsx", MaxImportBytes + 1, "File import không được vượt quá 10 MB.")]
    public async Task PreviewRejectsInvalidFileWithoutDelegating(string name, long length, string message)
    {
        var (controller, bulk) = Build();

        var result = await controller.Preview(new(1, File(name, length)), default);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(message, Assert.IsType<RequestResponse<object>>(bad.Value).Message);
        Assert.Equal(0, bulk.PreviewCalls);
    }

    [Theory]
    [InlineData("members.xlsx", 0)]
    [InlineData("members.csv", 10)]
    [InlineData("members.xlsx", MaxImportBytes + 1)]
    public async Task BulkImportRejectsInvalidFileWithoutDelegating(string name, long length)
    {
        var (controller, bulk) = Build();

        var result = await controller.BulkImport(new(1, File(name, length)), default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(0, bulk.ImportCalls);
    }

    [Fact]
    public async Task PreviewAcceptsFileAtExactLimitAndDelegates()
    {
        var (controller, bulk) = Build();

        var result = await controller.Preview(new(1, File("MEMBERS.XLSX", MaxImportBytes)), default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RequestResponse<SchoolMemberImportPreviewResponse>>(ok.Value);
        Assert.Equal(1, response.Data!.ValidCount);
        Assert.Equal(1, bulk.PreviewCalls);
    }

    [Fact]
    public async Task BulkImportDelegatesAndReportsCounts()
    {
        var (controller, bulk) = Build();

        var result = await controller.BulkImport(new(1, File("members.xlsx", 10)), default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RequestResponse<SchoolMemberBulkResult>>(ok.Value);
        Assert.Equal(1, response.Data!.SuccessCount);
        Assert.Equal(1, bulk.ImportCalls);
    }

    [Fact]
    public async Task InvalidWorkbookBecomesBadRequest()
    {
        var (controller, bulk) = Build();
        bulk.ThrowInvalidData = "File Excel không có worksheet.";

        var preview = await controller.Preview(new(1, File("members.xlsx", 10)), default);
        var import = await controller.BulkImport(new(1, File("members.xlsx", 10)), default);

        var bad = Assert.IsType<BadRequestObjectResult>(preview.Result);
        Assert.Equal("File Excel không có worksheet.", Assert.IsType<RequestResponse<object>>(bad.Value).Message);
        Assert.IsType<BadRequestObjectResult>(import.Result);
    }

    [Fact]
    public async Task BulkAddDelegatesToService()
    {
        var (controller, bulk) = Build();

        var result = await controller.BulkAdd(new(1, "Teacher", [Guid.NewGuid(), Guid.NewGuid()]), default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RequestResponse<SchoolMemberBulkResult>>(ok.Value);
        Assert.Equal(2, response.Data!.SuccessCount);
        Assert.Equal(1, bulk.AddCalls);
    }

    [Fact]
    public void TemplateReturnsXlsxFile()
    {
        var (controller, bulk) = Build();

        var file = Assert.IsType<FileContentResult>(controller.DownloadImportTemplate());

        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
        Assert.Equal("school-member-import-template.xlsx", file.FileDownloadName);
        Assert.Equal(1, bulk.TemplateCalls);
    }
}
