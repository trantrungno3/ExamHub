namespace ExamHub.Core.Application.Services;

/// <summary>Ném khi cố xoá một đối tượng đang được đối tượng khác tham chiếu.</summary>
public sealed class EntityInUseException(string message) : Exception(message);
