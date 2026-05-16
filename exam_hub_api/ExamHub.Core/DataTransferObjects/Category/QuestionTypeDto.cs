using ExamHub.Core.Domain.Entities;

namespace ExamHub.Core.DataTransferObjects.Category;

/// <summary>Request DTO để tạo / cập nhật QuestionType</summary>
public record QuestionTypeRequest(
    string Code,
    string Name,
    string? Description,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public QuestionType ToEntity() => new()
    {
        Code        = Code,
        Name        = Name,
        Description = Description,
        IsActive    = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public QuestionType ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho QuestionType</summary>
public record QuestionTypeResponse(
    int Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static QuestionTypeResponse FromEntity(QuestionType e) =>
        new(e.Id, e.Code, e.Name, e.Description, e.IsActive);
}
