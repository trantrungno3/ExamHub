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
    /// <summary>Evaluates the requirement against the current user's claims and the TeacherSubject table.</summary>
    /// <param name="context">Authorization context; <see cref="AuthorizationHandlerContext.Succeed"/> is called on success.</param>
    /// <param name="requirement">The requirement instance being evaluated.</param>
    /// <param name="subjectId">Id of the subject the caller must teach (or be Admin) to access.</param>
    /// <returns>A task that completes once the outcome has been reported via <paramref name="context"/>.</returns>
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

    /// <summary>Extracts the caller's user id from the standard user-id claim.</summary>
    /// <param name="user">Principal to read the claim from.</param>
    /// <returns>The parsed user id, or <see cref="Guid.Empty"/> if the claim is missing or invalid.</returns>
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ConstClaim.UserId);
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}