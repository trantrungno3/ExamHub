using ExamHub.Core.DataTransferObjects.User;

namespace ExamHub.Core.Application.Services;

/// <summary>
/// Dịch vụ import người dùng hàng loạt từ file Excel (.xlsx).
/// </summary>
/// <remarks>
/// Định dạng cột (dòng 1 là tiêu đề, dữ liệu từ dòng 2):
/// <list type="number">
/// <item>UserName (bắt buộc, duy nhất)</item>
/// <item>DisplayName (bắt buộc)</item>
/// <item>Email (tuỳ chọn)</item>
/// <item>PhoneNumber (tuỳ chọn)</item>
/// <item>Sex (tuỳ chọn — "Nam"/"Nữ" hoặc "0"/"1"; trống = Nam)</item>
/// <item>Role (tuỳ chọn — Admin/Teacher/Student, nhiều vai trò cách nhau bởi dấu phẩy)</item>
/// </list>
/// Mỗi dòng xử lý độc lập; dòng lỗi được báo cáo, dòng hợp lệ vẫn được lưu (import một phần).
/// Mật khẩu lấy từ <c>DefaultPassword</c> — áp cho mọi tài khoản.
/// </remarks>
public interface IUserBulkImportService
{
    /// <summary>Phân tích file Excel và tạo các tài khoản hợp lệ.</summary>
    Task<BulkUserImportResponse> ImportAsync(BulkUserImportRequest request, CancellationToken ct = default);

    /// <summary>Tạo file Excel mẫu (chỉ có dòng tiêu đề và một dòng ví dụ).</summary>
    byte[] BuildTemplate();
}
