using ExamHub.Core.Domain.Entities;
using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.Category;

/// <summary>Request DTO để tạo / cập nhật GradeLevel</summary>
public record GradeLevelRequest(
    string Name,
    short GradeNumber,
    string? Description,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public GradeLevel ToEntity() => new()
    {
        Name        = Name,
        GradeNumber = GradeNumber,
        Description = Description,
        IsActive    = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public GradeLevel ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho GradeLevel</summary>
public record GradeLevelResponse(
    int Id,
    string Name,
    short GradeNumber,
    string? Description,
    bool IsActive,
    long CreatedAt,
    long UpdatedAt
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static GradeLevelResponse FromEntity(GradeLevel e) =>
        new(e.Id, e.Name, e.GradeNumber, e.Description, e.IsActive,
            e.CreatedAt.ToTimestamp(), e.UpdatedAt.ToTimestamp());
}
