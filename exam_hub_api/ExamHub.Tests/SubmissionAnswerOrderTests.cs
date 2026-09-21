using ExamHub.Core.DataTransferObjects.Exam;
using ExamHub.Core.Domain.Entities;
using Xunit;

/// <summary>
/// FE đánh số "Câu i+1" theo vị trí mảng answers, nên response phải trả đúng thứ tự câu trong đề
/// dù DB trả dòng theo thứ tự nào.
/// </summary>
public class SubmissionAnswerOrderTests
{
    private static SubmissionAnswer A(int sortOrder, bool? isCorrect) => new()
    {
        ExamQuestion = new ExamQuestion { ContentSnapshot = "x", SortOrder = sortOrder },
        IsCorrect    = isCorrect
    };

    [Fact]
    public void Answers_are_ordered_by_exam_question_sort_order()
    {
        var submission = new ExamSubmission
        {
            Answers = { A(3, null), A(1, true), A(2, false) }
        };

        var res = ExamSubmissionResponse.FromEntity(submission, includeAnswers: true);

        Assert.Equal(new bool?[] { true, false, null }, res.Answers!.Select(a => a.IsCorrect));
    }
}
