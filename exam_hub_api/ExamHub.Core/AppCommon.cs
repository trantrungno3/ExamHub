using TVT.Core.Models;

namespace ExamHub.Core;

/// <summary>
/// </summary>
public static class AppCommon
{
    /// <summary>
    /// </summary>
    public const int DefaultGuidLength = 36;

    /// <summary>
    /// </summary>
    public static ConfigAudience Audience { get; } = new();

    /// <summary>
    /// </summary>
    public static ConfigAudience AudienceRefresh { get; } = new();

    /// <summary>
    /// </summary>
    public static string? SaltPassHash { get; set; }
}