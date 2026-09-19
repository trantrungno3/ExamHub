using ExamHub.Core.DataTransferObjects.Common;
using Microsoft.AspNetCore.Http;

namespace ExamHub.Core.DataTransferObjects.School;

public record SchoolMemberImportRequest(int SchoolId, IFormFile File);
public record SchoolMemberImportRowResult(
    int RowNumber, string UserName, string Role, string? CohortName,
    string? Section, bool IsValid, IReadOnlyList<string> Errors);
public record SchoolMemberImportPreviewResponse(
    int ValidCount, int ErrorCount, IReadOnlyList<SchoolMemberImportRowResult> Rows);
public record SchoolMemberBulkAddRequest(
    int SchoolId, string Role, IReadOnlyList<Guid> UserIds,
    int? CohortId = null, string? Section = null);
public record SchoolMemberBulkResult(
    int SuccessCount, int ErrorCount, IReadOnlyList<BulkImportRowError> Errors);
