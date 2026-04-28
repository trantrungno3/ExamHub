using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho Question</summary>
public class QuestionService : IQuestionService
{
    private readonly IQuestionRepository _questionRepo;
    private readonly IQuestionAnswerRepository _answerRepo;

    public QuestionService(IQuestionRepository questionRepo, IQuestionAnswerRepository answerRepo)
    {
        _questionRepo = questionRepo;
        _answerRepo   = answerRepo;
    }

    public Task<Question?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _questionRepo.GetByIdAsync(id, ct);

    public Task<Question?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => _questionRepo.GetWithAnswersAsync(id, ct);

    public Task<IReadOnlyList<Question>> GetByTopicAsync(int topicId, CancellationToken ct = default)
        => _questionRepo.GetByTopicAsync(topicId, ct: ct);

    public Task<(IReadOnlyList<Question> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        int? topicId = null, int? questionTypeId = null,
        int? difficultyLevelId = null, string? keyword = null,
        bool? isVerified = null, CancellationToken ct = default)
        => _questionRepo.GetPagedAsync(page, pageSize, topicId, questionTypeId, difficultyLevelId, keyword, isVerified, ct);

    public async Task<Question> CreateAsync(Question entity, IEnumerable<QuestionAnswer> answers, CancellationToken ct = default)
    {
        entity.Id        = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _questionRepo.AddAsync(entity, ct);

        var answerList = answers.Select((a, i) =>
        {
            a.Id         = Guid.NewGuid();
            a.QuestionId = entity.Id;
            a.SortOrder  = (short)i;
            return a;
        }).ToList();

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

        return entity;
    }

    public async Task<Question> UpdateAsync(Question entity, IEnumerable<QuestionAnswer>? answers = null, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _questionRepo.UpdateAsync(entity, ct);

        if (answers is not null)
        {
            await _answerRepo.DeleteByQuestionAsync(entity.Id, ct);
            var answerList = answers.Select((a, i) =>
            {
                a.Id         = Guid.NewGuid();
                a.QuestionId = entity.Id;
                a.SortOrder  = (short)i;
                return a;
            }).ToList();
            if (answerList.Count > 0)
                await _answerRepo.AddRangeAsync(answerList, ct);
        }

        return entity;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _questionRepo.DeleteByIdAsync(id, ct);

    public Task VerifyAsync(Guid id, Guid verifiedBy, CancellationToken ct = default)
        => _questionRepo.VerifyAsync(id, verifiedBy, ct);
}

/// <summary>Triển khai service cho TeacherSubject</summary>
public class TeacherSubjectService : ITeacherSubjectService
{
    private readonly ITeacherSubjectRepository _repo;
    public TeacherSubjectService(ITeacherSubjectRepository repo) => _repo = repo;

    public Task<IReadOnlyList<TeacherSubject>> GetByTeacherAsync(Guid userId, CancellationToken ct = default)
        => _repo.GetByTeacherAsync(userId, ct);

    public Task<bool> IsTeacherOfSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => _repo.IsTeacherOfSubjectAsync(userId, subjectId, ct);

    public Task AssignSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => _repo.AssignSubjectAsync(userId, subjectId, ct);

    public Task RemoveSubjectAsync(Guid userId, int subjectId, CancellationToken ct = default)
        => _repo.RemoveSubjectAsync(userId, subjectId, ct);
}

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
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _templateRepo.AddAsync(entity, ct);

        var sectionList = sections.Select((s, i) =>
        {
            s.Id             = Guid.NewGuid();
            s.ExamTemplateId = entity.Id;
            s.SortOrder      = (short)i;
            s.CreatedAt      = DateTime.UtcNow;
            return s;
        }).ToList();

        if (sectionList.Count > 0)
            await _sectionRepo.AddRangeAsync(sectionList, ct);

        return entity;
    }

    public async Task<ExamTemplate> UpdateAsync(
        ExamTemplate entity, IEnumerable<ExamTemplateSection>? sections = null, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _templateRepo.UpdateAsync(entity, ct);

        if (sections is not null)
        {
            await _sectionRepo.DeleteByTemplateAsync(entity.Id, ct);
            var sectionList = sections.Select((s, i) =>
            {
                s.Id             = Guid.NewGuid();
                s.ExamTemplateId = entity.Id;
                s.SortOrder      = (short)i;
                s.CreatedAt      = DateTime.UtcNow;
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

/// <summary>Triển khai service cho Exam</summary>
public class ExamService : IExamService
{
    private readonly IExamRepository _examRepo;
    private readonly IExamQuestionRepository _questionRepo;

    public ExamService(IExamRepository examRepo, IExamQuestionRepository questionRepo)
    {
        _examRepo     = examRepo;
        _questionRepo = questionRepo;
    }

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _examRepo.GetByIdAsync(id, ct);

    public Task<Exam?> GetWithQuestionsAsync(Guid id, CancellationToken ct = default)
        => _examRepo.GetWithQuestionsAsync(id, ct);

    public Task<(IReadOnlyList<Exam> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        int? gradeLevelId = null, int? subjectId = null,
        ExamStatusEnum? status = null, string? keyword = null,
        CancellationToken ct = default)
        => _examRepo.GetPagedAsync(page, pageSize, gradeLevelId, subjectId, status, keyword, ct);

    public Task<IReadOnlyList<Exam>> GetVariantsAsync(Guid parentExamId, CancellationToken ct = default)
        => _examRepo.GetVariantsAsync(parentExamId, ct);

    public async Task<Exam> CreateAsync(Exam entity, IEnumerable<ExamQuestion> questions, CancellationToken ct = default)
    {
        entity.Id        = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _examRepo.AddAsync(entity, ct);

        var questionList = questions.Select((q, i) =>
        {
            q.Id       = Guid.NewGuid();
            q.ExamId   = entity.Id;
            q.SortOrder = i;
            return q;
        }).ToList();

        if (questionList.Count > 0)
            await _questionRepo.AddRangeAsync(questionList, ct);

        return entity;
    }

    public Task<bool> PublishAsync(Guid id, CancellationToken ct = default)
        => _examRepo.UpdateStatusAsync(id, ExamStatusEnum.Published, ct);

    public Task<bool> ArchiveAsync(Guid id, CancellationToken ct = default)
        => _examRepo.UpdateStatusAsync(id, ExamStatusEnum.Archived, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
        => _examRepo.DeleteByIdAsync(id, ct);
}

/// <summary>Triển khai service cho ExamSubmission</summary>
public class ExamSubmissionService : IExamSubmissionService
{
    private readonly IExamSubmissionRepository _submissionRepo;
    private readonly ISubmissionAnswerRepository _answerRepo;

    public ExamSubmissionService(
        IExamSubmissionRepository submissionRepo,
        ISubmissionAnswerRepository answerRepo)
    {
        _submissionRepo = submissionRepo;
        _answerRepo     = answerRepo;
    }

    public Task<ExamSubmission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _submissionRepo.GetByIdAsync(id, ct);

    public Task<ExamSubmission?> GetWithAnswersAsync(Guid id, CancellationToken ct = default)
        => _submissionRepo.GetWithAnswersAsync(id, ct);

    public Task<IReadOnlyList<ExamSubmission>> GetByExamAsync(Guid examId, CancellationToken ct = default)
        => _submissionRepo.GetByExamAsync(examId, ct);

    public Task<IReadOnlyList<ExamSubmission>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => _submissionRepo.GetByStudentAsync(studentId, ct);

    public Task<ExamSubmission?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default)
        => _submissionRepo.GetByExamAndStudentAsync(examId, studentId, ct);

    public async Task<ExamSubmission> SubmitAsync(
        ExamSubmission submission,
        IEnumerable<SubmissionAnswer> answers,
        CancellationToken ct = default)
    {
        submission.Id          = Guid.NewGuid();
        submission.SubmittedAt = DateTime.UtcNow;
        submission.Status      = SubmissionStatusEnum.Submitted;
        submission.CreatedAt   = DateTime.UtcNow;

        await _submissionRepo.AddAsync(submission, ct);

        var answerList = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = submission.Id;
            return a;
        }).ToList();

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);


        return submission;
    }

    public async Task GradeAnswerAsync(
        Guid submissionAnswerId,
        decimal scoreEarned,
        bool isCorrect,
        string? feedback,
        Guid gradedBy,
        CancellationToken ct = default)
    {
        var answer = await _answerRepo.GetByIdAsync(submissionAnswerId, ct)
            ?? throw new KeyNotFoundException($"SubmissionAnswer '{submissionAnswerId}' not found.");

        answer.ScoreEarned = scoreEarned;
        answer.IsCorrect   = isCorrect;
        answer.Feedback    = feedback;
        answer.GradedBy    = gradedBy;

        await _answerRepo.UpdateAsync(answer, ct);
    }
}

