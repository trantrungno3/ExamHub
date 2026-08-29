namespace ExamHub.Core.Application.Services;

/// <summary>Tên các custom claim được nhúng vào JWT access token (ngoài claim chuẩn của TVT.Core).</summary>
public static class TokenClaimTypes
{
    /// <summary>Id trường — Teacher (nhiều) và Student (theo khoá đang học).</summary>
    public const string SchoolId = "SchoolId";
    /// <summary>Id lớp (cohort_classes) — Teacher (dạy + chủ nhiệm) và Student (lớp hiện tại).</summary>
    public const string CohortClassId = "CohortClassId";
    /// <summary>Id môn phụ trách — chỉ Teacher.</summary>
    public const string SubjectId = "SubjectId";
}
