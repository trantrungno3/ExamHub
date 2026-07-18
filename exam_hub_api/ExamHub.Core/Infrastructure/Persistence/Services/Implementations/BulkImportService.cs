using ClosedXML.Excel;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai import câu hỏi hàng loạt từ Excel bằng ClosedXML.</summary>
public class BulkImportService(IQuestionService questionService) : IBulkImportService
{
    private const int HeaderRows = 1;
    private static readonly char[] AnswerLetters = ['A', 'B', 'C', 'D'];

    /// <inheritdoc/>
    public async Task<BulkImportQuestionResponse> ImportAsync(
        BulkImportQuestionRequest request, string createdBy, CancellationToken ct = default)
    {
        var errors  = new List<BulkImportRowError>();
        var success = 0;

        await using var stream = request.File.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        for (int row = HeaderRows + 1; row <= lastRow; row++)
        {
            ct.ThrowIfCancellationRequested();

            var content = Cell(sheet, row, 1);
            // Bỏ qua dòng trống hoàn toàn
            if (string.IsNullOrWhiteSpace(content) && RowIsEmpty(sheet, row))
                continue;

            try
            {
                var (question, answers) = ParseRow(sheet, row, request, createdBy);
                await questionService.CreateAsync(question, answers, ct);
                success++;
            }
            catch (BulkImportRowException ex)
            {
                errors.Add(new BulkImportRowError(row, ex.Message));
            }
        }

        return new BulkImportQuestionResponse(success, errors.Count, errors);
    }

    private static (Question, List<QuestionAnswer>) ParseRow(
        IXLWorksheet sheet, int row, BulkImportQuestionRequest request, string createdBy)
    {
        var content = Cell(sheet, row, 1);
        if (string.IsNullOrWhiteSpace(content))
            throw new BulkImportRowException("Cột Content không được để trống.");

        if (!TryInt(Cell(sheet, row, 2), out var questionTypeId) || questionTypeId <= 0)
            throw new BulkImportRowException("QuestionTypeId không hợp lệ.");

        var difficultyLevelId = OptionalInt(Cell(sheet, row, 3)) ?? request.DefaultDifficultyLevelId;
        var topicId           = OptionalInt(Cell(sheet, row, 4)) ?? request.DefaultTopicId;
        var cognitiveLevelId  = OptionalInt(Cell(sheet, row, 5)) ?? request.DefaultCognitiveLevelId;
        var explanation       = Cell(sheet, row, 6);

        var correctSet = ParseCorrectLetters(Cell(sheet, row, 11));
        var answers    = new List<QuestionAnswer>();
        for (int i = 0; i < AnswerLetters.Length; i++)
        {
            var text = Cell(sheet, row, 7 + i);
            if (string.IsNullOrWhiteSpace(text)) continue;
            answers.Add(new QuestionAnswer
            {
                Content   = text,
                IsCorrect = correctSet.Contains(AnswerLetters[i])
            });
        }

        if (answers.Count > 0 && !answers.Any(a => a.IsCorrect))
            throw new BulkImportRowException("Câu trắc nghiệm phải có ít nhất một đáp án đúng (cột CorrectAnswers).");

        var question = new Question
        {
            TopicId           = topicId,
            QuestionTypeId    = questionTypeId,
            DifficultyLevelId = difficultyLevelId,
            CognitiveLevelId  = cognitiveLevelId,
            CreatedBy         = createdBy,
            Content           = content,
            ContentPlain      = content,
            Explanation       = string.IsNullOrWhiteSpace(explanation) ? null : explanation,
            Tags              = []
        };

        return (question, answers);
    }

    private static HashSet<char> ParseCorrectLetters(string? raw)
    {
        var set = new HashSet<char>();
        if (string.IsNullOrWhiteSpace(raw)) return set;
        foreach (var part in raw.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries))
        {
            var c = char.ToUpperInvariant(part.Trim()[0]);
            if (AnswerLetters.Contains(c)) set.Add(c);
        }
        return set;
    }

    private static string Cell(IXLWorksheet sheet, int row, int col)
        => sheet.Cell(row, col).GetString().Trim();

    private static bool RowIsEmpty(IXLWorksheet sheet, int row)
    {
        for (int c = 1; c <= 11; c++)
            if (!string.IsNullOrWhiteSpace(Cell(sheet, row, c)))
                return false;
        return true;
    }

    private static bool TryInt(string? s, out int value)
        => int.TryParse(s, out value);

    private static int? OptionalInt(string? s)
        => int.TryParse(s, out var v) ? v : null;

    /// <summary>Lỗi cấp dòng — được bắt và gom vào báo cáo, không làm dừng cả import.</summary>
    private sealed class BulkImportRowException(string message) : Exception(message);
}
