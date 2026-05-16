using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>Request DTO để thêm / cập nhật thành viên trường</summary>
public record SchoolMemberRequest(
    int SchoolId,
    Guid UserId,
    string Role,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity</summary>
    public SchoolMember ToEntity() => new()
    {
        SchoolId = SchoolId,
        UserId   = UserId,
        Role     = Role,
        IsActive = IsActive
    };
}

/// <summary>Response DTO cho SchoolMember</summary>
public record SchoolMemberResponse(
    Guid Id,
    int SchoolId,
    Guid UserId,
    string Role,
    bool IsActive,
    long JoinedAt
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static SchoolMemberResponse FromEntity(SchoolMember e) =>
        new(e.Id, e.SchoolId, e.UserId, e.Role, e.IsActive,
            e.JoinedAt.UtcDateTime.ToTimestamp());
}
