using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using TVT.Core.Enums;
using TVT.Core.MinioStorage;
using Xunit;

namespace ExamHub.Tests;

file sealed class FakeExamServiceForExport : IExamService
{
    public Exam? WithQuestionsResult { get; set; }

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default) => Task.FromResult(WithQuestionsResult);
    public Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(int page, int pageSize, int? gradeLevelId = null, int? subjectId = null, ExamStatusEnum? status = null, string? keyword = null, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<Exam> CreateAsync(Exam entity, IEnumerable<ExamQuestion> questions, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> PublishAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
    public Task<ExamHub.Core.DataTransferObjects.Exam.ExamAnalyticsResponse?> GetAnalyticsAsync(Guid examId, CancellationToken ct = default) => throw new NotSupportedException();
}

file sealed class FakeMinioStorageService : IMinioStorageService
{
    public bool UploadSucceeds { get; set; } = true;
    public string? UploadedUrl { get; set; } = "https://minio.local/exports/fake.pdf";

    public Task SetBucket(string bucketName) => throw new NotSupportedException();
    public Task RemoveBucket(string bucketName) => throw new NotSupportedException();
    public Task<(bool, string?)> UploadFileAsync(string filePath, string objectName, string contentType = "application/octet-stream") => throw new NotSupportedException();
    public Task<(bool, string?)> UploadStreamAsync(Stream stream, string objectName, string contentType = "application/octet-stream")
        => Task.FromResult((UploadSucceeds, UploadSucceeds ? UploadedUrl : null));
}

public class ExportServiceTests
{
    private static Exam SampleExam(Guid id) => new()
    {
        Id = id, Title = "Đề kiểm tra", SubjectId = 1, GradeLevelId = 1,
        Questions = [],
    };

    [Fact]
    public async Task ExportPdfAsync_ExamNotFound_ReturnsError()
    {
        var examService = new FakeExamServiceForExport { WithQuestionsResult = null };
        var service = new ExportService(examService, new FakeMinioStorageService());
        var examId = Guid.NewGuid();

        var result = await service.ExportPdfAsync(examId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal($"Đề thi {examId} không tồn tại.", result.Message);
    }

    [Fact]
    public async Task ExportPdfAsync_UploadFails_ReturnsError()
    {
        var examId = Guid.NewGuid();
        var examService = new FakeExamServiceForExport { WithQuestionsResult = SampleExam(examId) };
        var storage = new FakeMinioStorageService { UploadSucceeds = false };
        var service = new ExportService(examService, storage);

        var result = await service.ExportPdfAsync(examId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal("Tải file đề thi lên MinIO thất bại.", result.Message);
    }

    [Fact]
    public async Task ExportPdfAsync_ValidExam_ReturnsSuccessWithUrl()
    {
        var examId = Guid.NewGuid();
        var examService = new FakeExamServiceForExport { WithQuestionsResult = SampleExam(examId) };
        var storage = new FakeMinioStorageService { UploadedUrl = "https://minio.local/exports/abc.pdf" };
        var service = new ExportService(examService, storage);

        var result = await service.ExportPdfAsync(examId);

        Assert.Equal(RequestResponseStatus.Success, result.Status);
        Assert.Equal("https://minio.local/exports/abc.pdf", result.Data);
    }

    [Fact]
    public async Task ExportDocxAsync_ExamNotFound_ReturnsError()
    {
        var examService = new FakeExamServiceForExport { WithQuestionsResult = null };
        var service = new ExportService(examService, new FakeMinioStorageService());
        var examId = Guid.NewGuid();

        var result = await service.ExportDocxAsync(examId);

        Assert.Equal(RequestResponseStatus.Error, result.Status);
        Assert.Equal($"Đề thi {examId} không tồn tại.", result.Message);
    }
}
