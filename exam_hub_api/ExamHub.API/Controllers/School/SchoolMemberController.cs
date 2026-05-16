using ExamHub.Core.DataTransferObjects.School;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.School;

/// <summary>Controller quản lý thành viên trường học</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchoolMemberController(ISchoolMemberService service) : ControllerBase
{
    /// <summary>Lấy theo ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SchoolMemberResponse>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        return Ok(SchoolMemberResponse.FromEntity(result));
    }

    /// <summary>Lấy danh sách thành viên theo trường</summary>
    [HttpGet("by-school/{schoolId:int}")]
    public async Task<ActionResult<IReadOnlyList<SchoolMemberResponse>>> GetBySchool(int schoolId, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAsync(schoolId, ct);
        return Ok(result.Select(SchoolMemberResponse.FromEntity).ToList());
    }

    /// <summary>Lấy danh sách thành viên theo trường và vai trò</summary>
    [HttpGet("by-school/{schoolId:int}/role/{role}")]
    public async Task<ActionResult<IReadOnlyList<SchoolMemberResponse>>> GetBySchoolAndRole(int schoolId, string role, CancellationToken ct = default)
    {
        var result = await service.GetBySchoolAndRoleAsync(schoolId, role, ct);
        return Ok(result.Select(SchoolMemberResponse.FromEntity).ToList());
    }

    /// <summary>Lấy tất cả trường mà một người dùng thuộc vào</summary>
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SchoolMemberResponse>>> GetByUser(Guid userId, CancellationToken ct = default)
    {
        var result = await service.GetByUserAsync(userId, ct);
        return Ok(result.Select(SchoolMemberResponse.FromEntity).ToList());
    }

    /// <summary>Thêm thành viên vào trường</summary>
    [HttpPost("")]
    public async Task<ActionResult<SchoolMemberResponse>> AddMember([FromBody] SchoolMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        var result = await service.AddMemberAsync(entity, ct);
        return Ok(SchoolMemberResponse.FromEntity(result));
    }

    /// <summary>Cập nhật vai trò thành viên</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SchoolMemberResponse>> Update(Guid id, [FromBody] SchoolMemberRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity();
        entity.Id = id;
        var result = await service.UpdateAsync(entity, ct);
        return Ok(SchoolMemberResponse.FromEntity(result));
    }

    /// <summary>Xóa thành viên khỏi trường</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, CancellationToken ct = default)
    {
        await service.RemoveMemberAsync(id, ct);
        return NoContent();
    }

    /// <summary>Bật/tắt trạng thái thành viên</summary>
    [HttpPatch("{id:guid}/active")]
    public async Task<ActionResult<bool>> SetActive(Guid id, [FromBody] bool isActive, CancellationToken ct = default)
    {
        var result = await service.SetActiveAsync(id, isActive, ct);
        return Ok(result);
    }
}
