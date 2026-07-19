using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExamHub.Core.Infrastructure.Persistence.Converters;

/// <summary>
/// Chuyển enum ↔ chuỗi snake_case chữ thường để khớp với CHECK constraint trong PostgreSQL.
/// VD: SubmissionStatusEnum.InProgress ↔ "in_progress"; ExamStatusEnum.Draft ↔ "draft".
/// </summary>
public sealed class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public SnakeCaseEnumConverter()
        : base(
            v => ToSnakeCase(v.ToString()!),
            v => Enum.Parse<TEnum>(v.Replace("_", string.Empty), true))
    {
    }

    private static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c) && i > 0)
                sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
