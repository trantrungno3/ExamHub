namespace ExamHub.Core.DataTransferObjects.Exam;

/// <summary>Phân bổ theo một tiêu chí (Bloom / Độ khó / Chủ đề).</summary>
public record DistributionItem(string Label, int Count, double Percentage);

/// <summary>Response thống kê phân tích đề thi.</summary>
public record ExamAnalyticsResponse(
    Guid ExamId,
    int TotalQuestions,
    IReadOnlyList<DistributionItem> BloomDistribution,
    IReadOnlyList<DistributionItem> DifficultyDistribution,
    IReadOnlyList<DistributionItem> TopicDistribution);
