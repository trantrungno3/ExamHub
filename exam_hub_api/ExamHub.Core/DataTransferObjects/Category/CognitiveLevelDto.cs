using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.DataTransferObjects.Category;

/// <summary>Request DTO để tạo / cập nhật CognitiveLevel</summary>
public record CognitiveLevelRequest(
    string Code,
    string Name,
    string NameEn,
    short LevelOrder,
    string? Description,
    string? ColorCode,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public CognitiveLevel ToEntity() => new()
    {
        Code        = Code,
        Name        = Name,
        NameEn      = NameEn,
        LevelOrder  = LevelOrder,
        Description = Description,
        ColorCode   = ColorCode,
        IsActive    = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public CognitiveLevel ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho CognitiveLevel</summary>
public record CognitiveLevelResponse(
    int Id,
    string Code,
    string Name,
    string NameEn,
    short LevelOrder,
    string? Description,
    string? ColorCode,
    bool IsActive
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static CognitiveLevelResponse FromEntity(CognitiveLevel e) =>
        new(e.Id, e.Code, e.Name, e.NameEn, e.LevelOrder, e.Description, e.ColorCode, e.IsActive);
}
