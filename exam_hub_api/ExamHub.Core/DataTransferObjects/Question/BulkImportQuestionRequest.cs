using ExamHub.Core.DataTransferObjects.Common;
using Microsoft.AspNetCore.Http;

namespace ExamHub.Core.DataTransferObjects.Question;

/// <summary>Request bulk import câu hỏi từ file XLSX.</summary>
public record BulkImportQuestionRequest(
    IFormFile File,
    int DefaultTopicId,
    int DefaultDifficultyLevelId,
    int? DefaultCognitiveLevelId);

/// <summary>Kết quả tổng hợp sau bulk import.</summary>
public record BulkImportQuestionResponse(
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<BulkImportRowError> Errors);
