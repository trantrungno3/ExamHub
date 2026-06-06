namespace ExamHub.Core.Infrastructure.Caching;

/// <summary>
/// Quy ước khóa Redis cho pool câu hỏi ứng viên dùng khi sinh đề (spec §10).
/// Khóa: <c>qpool:{topicId}:{difficultyId}:{typeId|"all"}:{cogId|"all"}</c> — chỉ cache danh sách ID.
/// </summary>
public static class QuestionPoolCache
{
    /// <summary>TTL của pool cache (spec §10: 2 phút).</summary>
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(2);

    private const string All = "all";

    /// <summary>Khóa pool cho một bộ lọc pick cụ thể (type/cog có thể null = không lọc).</summary>
    public static string PoolKey(int topicId, int difficultyId, int? questionTypeId, int? cognitiveLevelId)
        => $"qpool:{topicId}:{difficultyId}:{(questionTypeId?.ToString() ?? All)}:{(cognitiveLevelId?.ToString() ?? All)}";

    /// <summary>
    /// Mọi khóa pool mà một câu hỏi với phân loại này có thể tham gia (≤ 4 khóa):
    /// type ∈ {type, "all"}, cog ∈ {cog, "all"} (hoặc chỉ "all" nếu câu hỏi chưa phân loại Bloom).
    /// Dùng để invalidate khi câu hỏi thêm/sửa/xóa/duyệt.
    /// </summary>
    public static IEnumerable<string> KeysForQuestion(
        int topicId, int difficultyId, int questionTypeId, int? cognitiveLevelId)
    {
        int?[] typeVariants = [questionTypeId, null];
        int?[] cogVariants  = cognitiveLevelId.HasValue ? [cognitiveLevelId, null] : [null];

        foreach (var t in typeVariants)
            foreach (var c in cogVariants)
                yield return PoolKey(topicId, difficultyId, t, c);
    }
}
