using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.DataTransferObjects.Category;

/// <summary>Request DTO để tạo / cập nhật DifficultyLevel</summary>
public record DifficultyLevelRequest(
    string Code,
    string Name,
    decimal ScoreWeight = 1.0m,
    short SortOrder = 0,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public DifficultyLevel ToEntity() => new()
    {
        Code        = Code,
        Name        = Name,
        ScoreWeight = ScoreWeight,
        SortOrder   = SortOrder,
        IsActive    = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public DifficultyLevel ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho DifficultyLevel</summary>
public record DifficultyLevelResponse(
    int Id,
    string Code,
    string Name,
    decimal ScoreWeight,
    short SortOrder,
    bool IsActive
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static DifficultyLevelResponse FromEntity(DifficultyLevel e) =>
        new(e.Id, e.Code, e.Name, e.ScoreWeight, e.SortOrder, e.IsActive);
}
