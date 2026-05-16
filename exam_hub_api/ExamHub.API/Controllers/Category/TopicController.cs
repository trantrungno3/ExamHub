using ExamHub.Core.DataTransferObjects.Category;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamHub.API.Controllers.Category;

/// <summary>Controller quản lý chủ đề</summary>
[ApiController]
[Route("api/[controller]")]
public class TopicController(ITopicService service)
    : CategoryBaseController<Topic, int, TopicRequest, TopicResponse>(service)
{
    /// <inheritdoc/>
    protected override Topic ToEntity(TopicRequest request) => request.ToEntity();
    /// <inheritdoc/>
    protected override Topic ToEntityForUpdate(int id, TopicRequest request) => request.ToEntity(id);
    /// <inheritdoc/>
    protected override TopicResponse ToResponse(Topic entity) => TopicResponse.FromEntity(entity);

    /// <summary>Lấy theo môn học</summary>
    [HttpGet("by-subject/{subjectId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> GetBySubject(int subjectId, CancellationToken ct = default)
    {
        var result = await service.GetBySubjectAsync(subjectId, ct);
        return Ok(result.Select(ToResponse).ToList());
    }

    /// <summary>Lấy chủ đề gốc theo môn học</summary>
    [HttpGet("root/by-subject/{subjectId:int}")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> GetRootTopics(int subjectId, CancellationToken ct = default)
    {
        var result = await service.GetRootTopicsAsync(subjectId, ct);
        return Ok(result.Select(ToResponse).ToList());
    }

    /// <summary>Lấy chủ đề con</summary>
    [HttpGet("{parentId:int}/children")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<TopicResponse>>> GetChildren(int parentId, CancellationToken ct = default)
    {
        var result = await service.GetChildrenAsync(parentId, ct);
        return Ok(result.Select(ToResponse).ToList());
    }
}
