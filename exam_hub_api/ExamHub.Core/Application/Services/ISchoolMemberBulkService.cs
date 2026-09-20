using ExamHub.Core.DataTransferObjects.School;

namespace ExamHub.Core.Application.Services;

public interface ISchoolMemberBulkService
{
    Task<SchoolMemberImportPreviewResponse> PreviewAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default);
    Task<SchoolMemberBulkResult> ImportAsync(
        SchoolMemberImportRequest request, CancellationToken ct = default);
    Task<SchoolMemberBulkResult> AddAsync(
        SchoolMemberBulkAddRequest request, CancellationToken ct = default);
    byte[] BuildTemplate();
}
