namespace ExamHub.Core.Application.Services;

/// <summary>
/// Interface cho dịch vụ xuất đề thi ra PDF / Word.
/// </summary>
public interface IExportService
{
    /// <summary>Xuất đề thi ra file PDF (QuestPDF) và lưu lên MinIO. Trả về presigned URL.</summary>
    Task<string> ExportPdfAsync(Guid examId, CancellationToken ct = default);

    /// <summary>Xuất đề thi ra file Word (ClosedXML) và lưu lên MinIO. Trả về presigned URL.</summary>
    Task<string> ExportDocxAsync(Guid examId, CancellationToken ct = default);
}

