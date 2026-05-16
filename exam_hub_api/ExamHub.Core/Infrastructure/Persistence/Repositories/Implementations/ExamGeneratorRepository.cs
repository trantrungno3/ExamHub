using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using TVT.Core.Db.PostgreSql.Infrastructures;
using TVT.Core.Db.PostgreSql.SqlBuilder;
using TVT.Core.Db.PostgreSql.SqlBuilder.Models;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>
/// Triển khai repository sinh đề thi bằng Dapper + TVT.Core IBaseRepository.
/// <list type="bullet">
/// <item><see cref="PickRandomAsync"/> — ORDER BY RANDOM() để lấy câu hỏi ngẫu nhiên.</item>
/// <item><see cref="SaveExamAsync"/> — ExecuteInTransactionAsync để ghi nguyên tử.</item>
/// </list>
/// </summary>
public class ExamGeneratorRepository(IBaseRepository db) : IExamGeneratorRepository
{
    // SQL sinh từ annotations [SqlBuilderProperty(Insert=true)] trên entities — cache tĩnh.
    // answers_snapshot cần cast ::jsonb vì Npgsql gửi string dưới dạng text.
    private static readonly string InsertExamSql =
        new InsertTable<Exam>(null!).GetInsertQuery();

    private static readonly string InsertExamQuestionSql =
        new InsertTable<ExamQuestion>(null!).GetInsertQuery()
            .Replace("@answers_snapshot", "@answers_snapshot::jsonb");

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PickedQuestion>> PickRandomAsync(
        int topicId,
        int? questionTypeId,
        int difficultyId,
        int count,
        IReadOnlySet<Guid> excludeIds,
        CancellationToken ct = default)
    {
        var sql  = BuildPickSql(questionTypeId.HasValue);
        var rows = await db.QueryAsync<PickedQuestion>(sql, new
        {
            TopicId        = topicId,
            QuestionTypeId = questionTypeId,
            DifficultyId   = difficultyId,
            ExcludedIds    = excludeIds.Count > 0 ? excludeIds.ToArray() : [],
            Count          = count
        }, cancellationToken: ct);

        return rows.ToList();
    }

    /// <inheritdoc/>
    public async Task<Guid> SaveExamAsync(
        Exam exam,
        IReadOnlyList<ExamQuestion> questions,
        IReadOnlySet<Guid> usedQuestionIds,
        CancellationToken ct = default)
    {
        await db.ExecuteInTransactionAsync(async tx =>
        {
            await db.ExecuteQueryAsync(InsertExamSql, exam.ToInsertObject(), tx, cancellationToken: ct);

            foreach (var q in questions)
                await db.ExecuteQueryAsync(InsertExamQuestionSql, q.ToInsertObject(), tx, cancellationToken: ct);

            if (usedQuestionIds.Count > 0)
                await db.ExecuteQueryAsync(IncrUsageSql, new { ids = usedQuestionIds.ToArray() }, tx, cancellationToken: ct);
        }, ct);

        return exam.Id;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Tách 2 biến thể SQL để tránh truyền tham số NULL kiểu int
    /// gây lỗi type-inference trong Npgsql.
    /// </summary>
    private static string BuildPickSql(bool filterByQuestionType)
    {
        var qtFilter = filterByQuestionType
            ? "AND q.question_type_id = @QuestionTypeId"
            : "";

        return $"""
            SELECT
                q.id      AS QuestionId,
                q.content AS Content,
                (
                    SELECT jsonb_agg(
                        jsonb_build_object(
                            'content',     a.content,
                            'is_correct',  a.is_correct,
                            'sort_order',  a.sort_order,
                            'explanation', a.explanation
                        ) ORDER BY a.sort_order
                    )::text
                    FROM public.question_answers a
                    WHERE a.question_id = q.id
                ) AS AnswersJson
            FROM public.questions q
            WHERE q.is_active           = true
              AND q.is_verified         = true
              AND q.topic_id            = @TopicId
              AND q.difficulty_level_id = @DifficultyId
              {qtFilter}
              AND NOT (q.id = ANY(@ExcludedIds))
            ORDER BY RANDOM()
            LIMIT @Count
            """;
    }

    // ── SQL ───────────────────────────────────────────────────────────────

    private const string IncrUsageSql = """
        UPDATE public.questions
        SET usage_count = usage_count + 1,
            updated_at  = now()
        WHERE id = ANY(@ids)
        """;
}
