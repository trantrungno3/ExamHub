using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>
/// Response DTO cho CohortClass.
/// CohortClass được sinh tự động bởi DB trigger khi tạo Cohort — không có Create request.
/// </summary>
public record CohortClassResponse(
    int Id,
    int CohortId,
    int GradeLevelId,
    string ClassName,
    string SchoolYear,
    short YearIndex,
    Guid? HomeroomTeacherId,
    long Created
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static CohortClassResponse FromEntity(CohortClass e) =>
        new(e.Id, e.CohortId, e.GradeLevelId, e.ClassName, e.SchoolYear, e.YearIndex, e.HomeroomTeacherId,
            e.Created.ToTimestamp());
}

/// <summary>Request DTO để cập nhật giáo viên chủ nhiệm</summary>
public record SetHomeroomTeacherRequest(Guid? TeacherId);
