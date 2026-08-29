using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Application.Services;

/// <inheritdoc/>
public sealed class TokenClaimsResolver(
    ISchoolMemberService schoolMemberService,
    ICohortClassTeacherService cohortClassTeacherService,
    ICohortClassService cohortClassService,
    ITeacherSubjectService teacherSubjectService,
    ICohortMemberService cohortMemberService,
    ICohortService cohortService) : ITokenClaimsResolver
{
    public async Task<List<KeyValuePair<string, string>>> ResolveAsync(
        Guid userId, IReadOnlyList<string> roles, CancellationToken ct = default)
    {
        var claims = new List<KeyValuePair<string, string>>();

        if (roles.Contains("Teacher"))
            await AddTeacherClaimsAsync(userId, claims, ct);

        if (roles.Contains("Student"))
            await AddStudentClaimsAsync(userId, claims, ct);

        return claims;
    }

    private async Task AddTeacherClaimsAsync(Guid teacherId, List<KeyValuePair<string, string>> claims, CancellationToken ct)
    {
        var schools = await schoolMemberService.GetByUserAsync(teacherId, ct);
        foreach (var schoolId in schools.Where(x => x.IsActive).Select(x => x.SchoolId).Distinct())
            claims.Add(new(TokenClaimTypes.SchoolId, schoolId.ToString()));

        var taught = await cohortClassTeacherService.GetByTeacherAsync(teacherId, ct);
        var homeroom = await cohortClassService.GetByHomeroomTeacherAsync(teacherId, ct);
        var classIds = taught.Select(x => x.CohortClassId).Concat(homeroom.Select(x => x.Id)).Distinct();
        foreach (var classId in classIds)
            claims.Add(new(TokenClaimTypes.CohortClassId, classId.ToString()));

        var subjects = await teacherSubjectService.GetByTeacherAsync(teacherId, ct);
        foreach (var subjectId in subjects.Select(x => x.SubjectId).Distinct())
            claims.Add(new(TokenClaimTypes.SubjectId, subjectId.ToString()));
    }

    private async Task AddStudentClaimsAsync(Guid studentId, List<KeyValuePair<string, string>> claims, CancellationToken ct)
    {
        var memberships = await cohortMemberService.GetByStudentAsync(studentId, ct);
        foreach (var member in memberships.Where(x => x.IsActive))
        {
            var cohort = await cohortService.GetByIdAsync(member.CohortId, ct);
            if (cohort is null) continue;

            claims.Add(new(TokenClaimTypes.SchoolId, cohort.SchoolId.ToString()));

            if (member.Section is null) continue;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var currentYearIndex = SchoolYearCalculator.GetCurrentYearIndex(cohort.StartYear, today);
            var classesInCohort = await cohortClassService.GetByCohortAsync(member.CohortId, ct);
            var match = classesInCohort.FirstOrDefault(x => x.Section == member.Section && x.YearIndex == currentYearIndex);
            if (match is not null)
                claims.Add(new(TokenClaimTypes.CohortClassId, match.Id.ToString()));
        }
    }
}
