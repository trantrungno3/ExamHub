using ExamHub.Core.DataTransferObjects.Common;
using Microsoft.AspNetCore.Http;

namespace ExamHub.Core.DataTransferObjects.User;

/// <summary>Request bulk import người dùng từ file XLSX.</summary>
public record BulkUserImportRequest(
    IFormFile File,
    string DefaultPassword);

/// <summary>Kết quả tổng hợp sau khi import người dùng.</summary>
public record BulkUserImportResponse(
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<BulkImportRowError> Errors);
