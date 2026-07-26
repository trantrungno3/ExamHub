using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>Request DTO để tạo / cập nhật Cohort</summary>
public record CohortRequest(
    int SchoolId,
    string Name,
    short StartYear,
    short EndYear,
    short GradeStart,
    short NumClasses = 1,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public Cohort ToEntity() => new()
    {
        SchoolId    = SchoolId,
        Name        = Name,
        StartYear   = StartYear,
        EndYear     = EndYear,
        GradeStart  = GradeStart,
        NumClasses  = NumClasses,
        IsActive    = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public Cohort ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho Cohort</summary>
public record CohortResponse(
    int Id,
    int SchoolId,
    string Name,
    short StartYear,
    short EndYear,
    short GradeStart,
    short NumClasses,
    bool IsActive,
    long Created
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static CohortResponse FromEntity(Cohort e) =>
        new(e.Id, e.SchoolId, e.Name, e.StartYear, e.EndYear, e.GradeStart, e.NumClasses, e.IsActive,
            e.Created.ToTimestamp());
}
