using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>
/// Lưu kết quả sinh đề nguyên tử: INSERT exam → INSERT exam_questions → UPDATE usage_count.
/// Dùng EF Core transaction để đảm bảo rollback toàn bộ nếu có lỗi.
/// </summary>
public class ExamGeneratorRepository(AppDbContext db, IQuestionRepository questionRepo) : IExamGeneratorRepository
{
    /// <inheritdoc/>
    public async Task<Guid> SaveExamAsync(
        Exam exam,
        IReadOnlyList<ExamQuestion> questions,
        IReadOnlySet<Guid> usedQuestionIds,
        CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            db.Set<Exam>().Add(exam);
            db.Set<ExamQuestion>().AddRange(questions);
            await db.SaveChangesAsync(ct);

            if (usedQuestionIds.Count > 0)
                await questionRepo.IncrementUsageCountAsync(usedQuestionIds, ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        return exam.Id;
    }

    /// <inheritdoc/>
    public async Task<Guid> SaveBatchExamsAsync(
        IReadOnlyList<Exam> exams,
        IReadOnlyList<ExamQuestion> questions,
        IReadOnlySet<Guid> usedQuestionIds,
        CancellationToken ct = default)
    {
        var batchId = exams[0].BatchId ?? Guid.NewGuid();
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            db.Set<Exam>().AddRange(exams);
            db.Set<ExamQuestion>().AddRange(questions);
            await db.SaveChangesAsync(ct);

            if (usedQuestionIds.Count > 0)
                await questionRepo.IncrementUsageCountAsync(usedQuestionIds, ct);

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        return batchId;
    }
}
