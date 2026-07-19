namespace ExamHub.Core.Application.Services;

/// <summary>
/// Thrown when the question pool does not have enough questions for a specific topic/difficulty/cognitive combination.
/// </summary>
public sealed class InsufficientQuestionsException(
    int? topicId, int difficultyId, int? cognitiveLevelId, int requested, int available)
    : Exception(BuildMessage(topicId, difficultyId, cognitiveLevelId, requested, available))
{
    public int? TopicId { get; } = topicId;
    public int DifficultyId { get; } = difficultyId;
    public int? CognitiveLevelId { get; } = cognitiveLevelId;
    public int Requested { get; } = requested;
    public int Available { get; } = available;

    private static string BuildMessage(int? topicId, int difficultyId, int? cognitiveLevelId, int requested, int available)
    {
        var topicPart = topicId.HasValue ? topicId.Value.ToString() : "mọi chủ đề";
        var cogPart = cognitiveLevelId.HasValue ? $", cognitiveLevel={cognitiveLevelId}" : "";
        return $"Không đủ câu hỏi: topic={topicPart}, difficulty={difficultyId}{cogPart}. Cần {requested}, tìm được {available}.";
    }
}
