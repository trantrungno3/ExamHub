using TVT.Core.Extensions;

namespace ExamHub.Core.DataTransferObjects.School;

/// <summary>Request DTO để tạo / cập nhật School</summary>
public record SchoolRequest(
    string Name,
    string Code,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive = true
)
{
    /// <summary>Chuyển sang entity để tạo mới</summary>
    public Domain.Entities.School ToEntity() => new()
    {
        Name     = Name,
        Code     = Code,
        Address  = Address,
        Phone    = Phone,
        Email    = Email,
        IsActive = IsActive
    };

    /// <summary>Chuyển sang entity để cập nhật (gán thêm Id)</summary>
    public Domain.Entities.School ToEntity(int id)
    {
        var entity = ToEntity();
        entity.Id = id;
        return entity;
    }
}

/// <summary>Response DTO cho School</summary>
public record SchoolResponse(
    int Id,
    string Name,
    string Code,
    string? Address,
    string? Phone,
    string? Email,
    bool IsActive,
    long CreatedAt,
    long UpdatedAt
)
{
    /// <summary>Chuyển từ entity sang response DTO</summary>
    public static SchoolResponse FromEntity(Domain.Entities.School e) =>
        new(e.Id, e.Name, e.Code, e.Address, e.Phone, e.Email, e.IsActive,
            e.CreatedAt.UtcDateTime.ToTimestamp(), e.UpdatedAt.UtcDateTime.ToTimestamp());
}
