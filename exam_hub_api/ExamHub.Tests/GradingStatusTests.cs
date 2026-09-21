using ExamHub.Core.Application.Grading;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using Xunit;

public class GradingStatusTests
{
    private static ExamQuestion Q(string? answersSnapshot) => new()
    {
        ContentSnapshot = "x",
        AnswersSnapshot = answersSnapshot
    };

    private const string ObjectiveSnap =
        "[{\"id\":\"11111111-1111-1111-1111-111111111111\",\"is_correct\":true}]";

    private const string MixedSnap =
        "[{\"id\":\"11111111-1111-1111-1111-111111111111\",\"is_correct\":true}," +
         "{\"id\":\"22222222-2222-2222-2222-222222222222\",\"is_correct\":false}]";

    [Fact]
    public void CorrectAnswerIds_parses_only_correct()
    {
        var ids = SubmissionGrading.CorrectAnswerIds(MixedSnap);
        Assert.Single(ids);
        Assert.Contains(Guid.Parse("11111111-1111-1111-1111-111111111111"), ids);
    }

    [Fact]
    public void DecideStatus_all_objective_is_Graded()
    {
        var status = SubmissionGrading.DecideStatus(new[] { Q(ObjectiveSnap), Q(ObjectiveSnap) });
        Assert.Equal(SubmissionStatusEnum.Graded, status);
    }

    [Fact]
    public void DecideStatus_with_essay_is_PendingManualGrade()
    {
        var status = SubmissionGrading.DecideStatus(new[] { Q(ObjectiveSnap), Q(null) });
        Assert.Equal(SubmissionStatusEnum.PendingManualGrade, status);
    }
}
