using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExamHub.Core.Application.Services;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TVT.Core.MinioStorage;
using QuestDocument = QuestPDF.Fluent.Document;
using WordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>
/// Xuất đề thi ra PDF (QuestPDF) / Word (DocumentFormat.OpenXml) và lưu lên MinIO.
/// Render từ <see cref="Exam"/> kèm <see cref="ExamQuestion"/> snapshot, trả về URL tải về.
/// </summary>
public class ExportService(IExamService examService, IMinioStorageService storage) : IExportService
{
    private const string DocxContentType =
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    static ExportService()
    {
        // QuestPDF Community license (miễn phí, không cần key cho dự án doanh thu < $1M).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc/>
    public async Task<string> ExportPdfAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await LoadExamAsync(examId, ct);
        var bytes = RenderPdf(exam);
        return await UploadAsync(bytes, $"exports/{examId}.pdf", "application/pdf");
    }

    /// <inheritdoc/>
    public async Task<string> ExportDocxAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await LoadExamAsync(examId, ct);
        var bytes = RenderDocx(exam);
        return await UploadAsync(bytes, $"exports/{examId}.docx", DocxContentType);
    }

    private async Task<Exam> LoadExamAsync(Guid examId, CancellationToken ct)
        => await examService.GetWithQuestionsAsync(examId, ct)
           ?? throw new InvalidOperationException($"Đề thi {examId} không tồn tại.");

    private async Task<string> UploadAsync(byte[] bytes, string objectName, string contentType)
    {
        using var ms = new MemoryStream(bytes);
        var (ok, url) = await storage.UploadStreamAsync(ms, objectName, contentType);
        if (!ok || string.IsNullOrEmpty(url))
            throw new InvalidOperationException("Tải file đề thi lên MinIO thất bại.");
        return url;
    }

    // ── PDF rendering ────────────────────────────────────────────
    private static byte[] RenderPdf(Exam exam)
    {
        var questions = exam.Questions.OrderBy(q => q.SortOrder).ToList();

        var document = QuestDocument.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(11));

            page.Header().Column(col =>
            {
                col.Item().AlignCenter().Text(exam.Title).Bold().FontSize(16);
                col.Item().AlignCenter().Text(MetaLine(exam)).FontSize(10).Italic();
                if (!string.IsNullOrWhiteSpace(exam.Instructions))
                    col.Item().PaddingTop(4).Text(StripHtml(exam.Instructions)).FontSize(10);
            });

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Spacing(10);
                for (var i = 0; i < questions.Count; i++)
                {
                    var q = questions[i];
                    col.Item().Column(qcol =>
                    {
                        qcol.Item().Text($"Câu {i + 1}{ScoreSuffix(q.Score)}: {StripHtml(q.ContentSnapshot)}").Bold();
                        foreach (var (letter, text) in ParseAnswers(q.AnswersSnapshot))
                            qcol.Item().PaddingLeft(15).Text($"{letter}. {text}");
                    });
                }
            });

            page.Footer().AlignCenter().Text(txt =>
            {
                txt.CurrentPageNumber();
                txt.Span(" / ");
                txt.TotalPages();
            });
        }));

        return document.GeneratePdf();
    }

    // ── Word (.docx) rendering ───────────────────────────────────
    private static byte[] RenderDocx(Exam exam)
    {
        var questions = exam.Questions.OrderBy(q => q.SortOrder).ToList();

        using var ms = new MemoryStream();
        using (var word = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
        {
            var main = word.AddMainDocumentPart();
            main.Document = new WordDocument();
            var body = main.Document.AppendChild(new Body());

            body.AppendChild(HeadingParagraph(exam.Title));
            body.AppendChild(Paragraph(MetaLine(exam)));
            if (!string.IsNullOrWhiteSpace(exam.Instructions))
                body.AppendChild(Paragraph(StripHtml(exam.Instructions)));

            for (var i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                body.AppendChild(BoldParagraph($"Câu {i + 1}{ScoreSuffix(q.Score)}: {StripHtml(q.ContentSnapshot)}"));
                foreach (var (letter, text) in ParseAnswers(q.AnswersSnapshot))
                    body.AppendChild(Paragraph($"{letter}. {text}"));
            }

            main.Document.Save();
        }

        return ms.ToArray();
    }

    // ── Shared helpers ───────────────────────────────────────────
    private static string MetaLine(Exam exam)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(exam.ExamCode)) parts.Add($"Mã đề: {exam.ExamCode}");
        parts.Add($"Thời gian: {exam.DurationMinutes} phút");
        parts.Add($"Tổng điểm: {exam.TotalScore}");
        if (!string.IsNullOrWhiteSpace(exam.SchoolYear)) parts.Add($"Năm học: {exam.SchoolYear}");
        return string.Join("  |  ", parts);
    }

    private static string ScoreSuffix(decimal? score)
        => score.HasValue ? $" ({score.Value:0.##} điểm)" : string.Empty;

    /// <summary>Parse answers_snapshot JSON [{content, sort_order, ...}] → (A/B/C…, plain text).</summary>
    private static List<(string Letter, string Text)> ParseAnswers(string? snapshotJson)
    {
        var result = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(snapshotJson)) return result;

        try
        {
            using var doc = JsonDocument.Parse(snapshotJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return result;

            var items = new List<(int Sort, string Content)>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var content = el.TryGetProperty("content", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                var sort = el.TryGetProperty("sort_order", out var s) && s.TryGetInt32(out var so) ? so : items.Count;
                items.Add((sort, StripHtml(content)));
            }

            var ordered = items.OrderBy(x => x.Sort).ToList();
            for (var i = 0; i < ordered.Count && i < 26; i++)
                result.Add((((char)('A' + i)).ToString(), ordered[i].Content));
        }
        catch (JsonException)
        {
            // Snapshot hỏng → bỏ qua phần đáp án, vẫn xuất nội dung câu hỏi.
        }

        return result;
    }

    /// <summary>Loại bỏ thẻ HTML, decode entity, gộp khoảng trắng — để render plaintext.</summary>
    private static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var text = Regex.Replace(html, "<.*?>", " ");
        text = WebUtility.HtmlDecode(text);
        return Regex.Replace(text, "\\s+", " ").Trim();
    }

    private static Paragraph Paragraph(string text)
        => new(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));

    private static Paragraph BoldParagraph(string text)
    {
        var run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })
        {
            RunProperties = new RunProperties(new Bold())
        };
        return new Paragraph(run);
    }

    private static Paragraph HeadingParagraph(string text)
    {
        var run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve })
        {
            RunProperties = new RunProperties(new Bold(), new FontSize { Val = "32" })
        };
        return new Paragraph(run)
        {
            ParagraphProperties = new ParagraphProperties(new Justification { Val = JustificationValues.Center })
        };
    }
}
