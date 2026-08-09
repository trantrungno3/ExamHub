namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>
/// Response DTO cho một phân công GV giảng dạy (kèm tên môn/GV để hiển thị).
/// </summary>
public record CohortClassTeacherResponse(
    int Id,
    int CohortClassId,
    int SubjectId,
    string SubjectName,
    Guid TeacherId,
    string TeacherName
);

/// <summary>Request phân công GV giảng dạy cho lớp theo môn.</summary>
public record AssignTeacherRequest(int CohortClassId, int SubjectId, Guid TeacherId);

/// <summary>GV hợp lệ để phân công (đã lọc theo trường + đúng môn).</summary>
public record EligibleTeacherResponse(Guid Id, string Name);
