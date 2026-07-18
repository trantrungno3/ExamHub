using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho ExamTemplate</summary>
public class ExamTemplateService : IExamTemplateService
{
    private readonly IExamTemplateRepository _templateRepo;
    private readonly IExamTemplateSectionRepository _sectionRepo;

    public ExamTemplateService(
        IExamTemplateRepository templateRepo,
        IExamTemplateSectionRepository sectionRepo)
    {
        _templateRepo = templateRepo;
        _sectionRepo  = sectionRepo;
    }

    public Task<ExamTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _templateRepo.GetByIdAsync(id, ct);

    public Task<ExamTemplate?> GetWithSectionsAsync(Guid id, CancellationToken ct = default)
        => _templateRepo.GetWithSectionsAsync(id, ct);

    public Task<IReadOnlyList<ExamTemplate>> GetBySubjectAsync(int subjectId, CancellationToken ct = default)
        => _templateRepo.GetBySubjectAsync(subjectId, ct);

    public Task<IReadOnlyList<ExamTemplate>> GetByGradeLevelAsync(int gradeLevelId, CancellationToken ct = default)
        => _templateRepo.GetByGradeLevelAsync(gradeLevelId, ct);

    public async Task<ExamTemplate> CreateAsync(
        ExamTemplate entity, IEnumerable<ExamTemplateSection> sections, CancellationToken ct = default)
    {
        entity.Id        = Guid.NewGuid();
        entity.Created = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;
        await _templateRepo.AddAsync(entity, ct);

        var sectionList = sections.Select((s, i) =>
        {
            s.Id             = Guid.NewGuid();
            s.ExamTemplateId = entity.Id;
            s.SortOrder      = (short)i;
            s.Created      = DateTime.UtcNow;
            return s;
        }).ToList();

        if (sectionList.Count > 0)
            await _sectionRepo.AddRangeAsync(sectionList, ct);

        return entity;
    }

    public async Task<ExamTemplate> UpdateAsync(
        ExamTemplate entity, IEnumerable<ExamTemplateSection>? sections = null, CancellationToken ct = default)
    {
        entity.Modified = DateTime.UtcNow;
        await _templateRepo.UpdateAsync(entity, ct);

        if (sections is not null)
        {
            await _sectionRepo.DeleteByTemplateAsync(entity.Id, ct);
            var sectionList = sections.Select((s, i) =>
            {
                s.Id             = Guid.NewGuid();
                s.ExamTemplateId = entity.Id;
                s.SortOrder      = (short)i;
                s.Created      = DateTime.UtcNow;
                return s;
            }).ToList();
            if (sectionList.Count > 0)
                await _sectionRepo.AddRangeAsync(sectionList, ct);
        }

        return entity;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _templateRepo.DeleteByIdAsync(id, ct);
}
