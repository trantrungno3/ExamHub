using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>Request DTO để thêm học sinh vào khoá học</summary>
public record CohortMemberRequest(
    int CohortId,
    Guid StudentId,
    long? JoinedAt = null,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity</summary>
    public CohortMember ToEntity() => new()
    {
        CohortId  = CohortId,
        StudentId = StudentId,
        JoinedAt  = JoinedAt.HasValue
            ? DateOnly.FromDateTime(JoinedAt.Value.ToDateTime())
            : DateOnly.FromDateTime(DateTime.UtcNow),
        IsActive  = IsActive
    };
}

/// <summary>Response DTO cho CohortMember</summary>
public record CohortMemberResponse(
    Guid Id,
    int CohortId,
    Guid StudentId,
    long JoinedAt,
    bool IsActive
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static CohortMemberResponse FromEntity(CohortMember e) =>
        new(e.Id, e.CohortId, e.StudentId,
            e.JoinedAt.ToDateTime(TimeOnly.MinValue).ToTimestamp(),
            e.IsActive);
}
