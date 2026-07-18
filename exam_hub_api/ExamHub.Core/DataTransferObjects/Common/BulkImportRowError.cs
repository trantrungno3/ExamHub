namespace ExamHub.Core.DataTransferObjects.Common;

/// <summary>Kết quả lỗi của một hàng khi import hàng loạt (dùng chung cho mọi loại import).</summary>
public record BulkImportRowError(int RowNumber, string Message);
