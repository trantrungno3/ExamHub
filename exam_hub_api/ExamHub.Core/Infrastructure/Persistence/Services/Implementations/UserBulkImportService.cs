using ClosedXML.Excel;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Common;
using ExamHub.Core.DataTransferObjects.User;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai import người dùng hàng loạt từ Excel bằng ClosedXML.</summary>
public class UserBulkImportService(IUserManagementService userService) : IUserBulkImportService
{
    private const int HeaderRows = 1;
    private const int ColumnCount = 6;

    private static readonly string[] Headers =
        ["UserName", "DisplayName", "Email", "PhoneNumber", "Sex", "Role"];

    private static readonly IReadOnlyDictionary<string, string> AllowedRoles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = "Admin",
            ["teacher"] = "Teacher",
            ["student"] = "Student",
        };

    /// <inheritdoc/>
    public async Task<BulkUserImportResponse> ImportAsync(
        BulkUserImportRequest request, CancellationToken ct = default)
    {
        var errors = new List<BulkImportRowError>();
        var seenUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var success = 0;

        await using var stream = request.File.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        for (int row = HeaderRows + 1; row <= lastRow; row++)
        {
            ct.ThrowIfCancellationRequested();

            if (RowIsEmpty(sheet, row)) continue;

            try
            {
                var (createRequest, roles) = ParseRow(sheet, row, request.DefaultPassword, seenUserNames);

                if (await userService.CheckUserNameExistAsync(createRequest.UserName))
                    throw new BulkImportRowException($"Tên đăng nhập '{createRequest.UserName}' đã tồn tại.");

                var created = await userService.CreateAsync(createRequest)
                    ?? throw new BulkImportRowException("Không thể tạo tài khoản (lỗi lưu dữ liệu).");

                if (roles.Length > 0)
                    await userService.SetRolesAsync(created.Id, roles);

                success++;
            }
            catch (BulkImportRowException ex)
            {
                errors.Add(new BulkImportRowError(row, ex.Message));
            }
        }

        return new BulkUserImportResponse(success, errors.Count, errors);
    }

    /// <inheritdoc/>
    public byte[] BuildTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Users");

        for (int c = 0; c < Headers.Length; c++)
        {
            var cell = sheet.Cell(1, c + 1);
            cell.Value = Headers[c];
            cell.Style.Font.Bold = true;
        }

        // Dòng ví dụ
        sheet.Cell(2, 1).Value = "nguyenvana";
        sheet.Cell(2, 2).Value = "Nguyễn Văn A";
        sheet.Cell(2, 3).Value = "a@example.com";
        sheet.Cell(2, 4).Value = "0900000000";
        sheet.Cell(2, 5).Value = "Nam";
        sheet.Cell(2, 6).Value = "Student";

        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static (CreateUserRequest, string[]) ParseRow(
        IXLWorksheet sheet, int row, string defaultPassword, HashSet<string> seenUserNames)
    {
        var userName = Cell(sheet, row, 1);
        if (string.IsNullOrWhiteSpace(userName))
            throw new BulkImportRowException("Cột UserName không được để trống.");
        // In-file duplicate detection: Add returns false if this UserName was already seen.
        if (!seenUserNames.Add(userName))
            throw new BulkImportRowException($"Tên đăng nhập '{userName}' bị trùng trong file.");

        var displayName = Cell(sheet, row, 2);
        if (string.IsNullOrWhiteSpace(displayName))
            throw new BulkImportRowException("Cột DisplayName không được để trống.");

        var email = Cell(sheet, row, 3);
        var phone = Cell(sheet, row, 4);
        var sex = ParseSex(Cell(sheet, row, 5));
        var roles = ParseRoles(Cell(sheet, row, 6));

        var request = new CreateUserRequest
        {
            UserName = userName,
            Password = defaultPassword,
            DisplayName = displayName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            Sex = sex,
        };

        return (request, roles);
    }

    /// <summary>"Nữ"/"1"/"true" => true (Nữ); còn lại => false (Nam).</summary>
    private static bool ParseSex(string raw)
    {
        var v = raw.Trim().ToLowerInvariant();
        return v is "nữ" or "nu" or "1" or "true" or "female" or "f";
    }

    private static string[] ParseRoles(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        var result = new List<string>();
        foreach (var part in raw.Split([',', ';', '/'], StringSplitOptions.RemoveEmptyEntries))
        {
            var key = part.Trim();
            if (!AllowedRoles.TryGetValue(key, out var canonical))
                throw new BulkImportRowException($"Vai trò '{key}' không hợp lệ (chỉ Admin/Teacher/Student).");
            if (!result.Contains(canonical)) result.Add(canonical);
        }
        return [.. result];
    }

    private static string Cell(IXLWorksheet sheet, int row, int col)
        => sheet.Cell(row, col).GetString().Trim();

    private static bool RowIsEmpty(IXLWorksheet sheet, int row)
    {
        for (int c = 1; c <= ColumnCount; c++)
            if (!string.IsNullOrWhiteSpace(Cell(sheet, row, c)))
                return false;
        return true;
    }

    /// <summary>Lỗi cấp dòng — gom vào báo cáo, không dừng cả import.</summary>
    private sealed class BulkImportRowException(string message) : Exception(message);
}
