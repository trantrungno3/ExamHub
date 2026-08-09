namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>
/// Response DTO cho một phân công GV giảng dạy.
/// Tên môn/GV được FE resolve từ danh sách môn + người dùng đã tải (theo pattern CohortDetailPage).
/// </summary>
public record CohortClassTeacherResponse(
    int Id,
    int CohortClassId,
    int SubjectId,
    Guid TeacherId
);

/// <summary>Request phân công GV giảng dạy cho lớp theo môn.</summary>
public record AssignTeacherRequest(int CohortClassId, int SubjectId, Guid TeacherId);
