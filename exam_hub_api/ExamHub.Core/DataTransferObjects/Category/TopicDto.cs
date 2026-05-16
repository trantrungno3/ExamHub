using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Category;

/// <summary>Request DTO để tạo / cập nhật Topic</summary>
public record TopicRequest(
    int SubjectId,
    int? ParentId,
    string Name,
    string? Code,
    int SortOrder = 0,
    string? Description = null,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public Topic ToEntity() => new()
    {
        SubjectId   = SubjectId,
        ParentId    = ParentId,
        Name        = Name,
        Code        = Code,
        SortOrder   = SortOrder,
        Description = Description,
        IsActive    = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public Topic ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho Topic</summary>
public record TopicResponse(
    int Id,
    int SubjectId,
    int? ParentId,
    string Name,
    string? Code,
    int SortOrder,
    string? Description,
    bool IsActive,
    long CreatedAt,
    long UpdatedAt
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static TopicResponse FromEntity(Topic e) =>
        new(e.Id, e.SubjectId, e.ParentId, e.Name, e.Code, e.SortOrder, e.Description, e.IsActive,
            e.CreatedAt.ToTimestamp(), e.UpdatedAt.ToTimestamp());
}
