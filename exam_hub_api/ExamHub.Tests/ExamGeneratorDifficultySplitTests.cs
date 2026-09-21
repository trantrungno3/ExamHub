using System.Reflection;
using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.Infrastructure.Persistence.Services.Implementations;
using Xunit;

namespace ExamHub.Tests;

public class ExamGeneratorDifficultySplitTests
{
    private static List<(DifficultyLevelEnum Diff, int Count)> SplitByDifficulty(SectionConfig s)
    {
        var method = typeof(ExamGeneratorService).GetMethod(
            "SplitByDifficulty", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (List<(DifficultyLevelEnum, int)>)method.Invoke(null, [s])!;
    }

    [Fact]
    public void ZeroPercentVeryHard_NeverReceivesLeftoverQuestions()
    {
        // n=7, 40/30/30/0 -> floor 3 mức đầu dư 1 câu; bug cũ dồn hết dư vào VeryHard dù Pct=0
        var section = new SectionConfig
        {
            QuestionCount = 7, PctEasy = 40, PctMedium = 30, PctHard = 30, PctVeryHard = 0
        };
        var result = SplitByDifficulty(section);

        Assert.Equal(0, result.Single(r => r.Diff == DifficultyLevelEnum.VeryHard).Count);
        Assert.Equal(7, result.Sum(r => r.Count));
    }

    [Fact]
    public void Total_AlwaysMatchesQuestionCount()
    {
        var section = new SectionConfig
        {
            QuestionCount = 13, PctEasy = 25, PctMedium = 25, PctHard = 25, PctVeryHard = 25
        };
        var result = SplitByDifficulty(section);

        Assert.Equal(13, result.Sum(r => r.Count));
    }
}
