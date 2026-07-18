using System.Security.Claims;
using ExamHub.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using TVT.Core.Claims;

namespace ExamHub.API.Authorization;

/// <summary>
/// Succeeds when the current user is Admin OR is assigned to the subject in the TeacherSubject table.
/// The resource passed by the controller is the int subjectId.
/// </summary>
public sealed class TeacherOwnsSubjectHandler(ITeacherSubjectRepository teacherSubjectRepo)
    : AuthorizationHandler<TeacherOwnsSubjectRequirement, int>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TeacherOwnsSubjectRequirement requirement,
        int subjectId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = GetUserId(context.User);
        if (userId == Guid.Empty) return;

        if (await teacherSubjectRepo.IsTeacherOfSubjectAsync(userId, subjectId))
            context.Succeed(requirement);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ConstClaim.UserId);
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}