namespace ExamHub.Core.Application.Services;

/// <summary>Resolve các custom claim (trường/lớp/môn) để nhúng vào JWT access token theo vai trò người dùng.</summary>
public interface ITokenClaimsResolver
{
    Task<List<KeyValuePair<string, string>>> ResolveAsync(Guid userId, IReadOnlyList<string> roles, CancellationToken ct = default);
}
