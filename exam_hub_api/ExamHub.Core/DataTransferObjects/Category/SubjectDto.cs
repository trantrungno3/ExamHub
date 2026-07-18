using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Category;

/// <summary>Request DTO để tạo / cập nhật Subject</summary>
public record SubjectRequest(
    int GradeLevelId,
    string Name,
    string Code,
    string? Description,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public Subject ToEntity() => new()
    {
        GradeLevelId = GradeLevelId,
        Name         = Name,
        Code         = Code,
        Description  = Description,
        IsActive     = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public Subject ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho Subject</summary>
public record SubjectResponse(
    int Id,
    int GradeLevelId,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    long Created,
    long Modified
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static SubjectResponse FromEntity(Subject e) =>
        new(e.Id, e.GradeLevelId, e.Name, e.Code, e.Description, e.IsActive,
            e.Created.ToTimestamp(), e.Modified.ToTimestamp());
}
