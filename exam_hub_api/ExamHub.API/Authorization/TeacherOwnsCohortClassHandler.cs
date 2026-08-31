using ExamHub.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace ExamHub.API.Authorization;

/// <summary>
/// Succeeds when the current user is Admin OR their JWT carries a CohortClassId claim
/// matching the target class. The resource passed by the controller is the int cohortClassId.
/// Reads the claim directly (no DB round trip) — the claim is resolved once at login by
/// ITokenClaimsResolver (see the 2026-08-25 access-token plan).
/// </summary>
public sealed class TeacherOwnsCohortClassHandler : AuthorizationHandler<TeacherOwnsCohortClassRequirement, int>
{
    /// <summary>Evaluates the requirement against the current user's claims.</summary>
    /// <param name="context">Authorization context; <see cref="AuthorizationHandlerContext.Succeed"/> is called on success.</param>
    /// <param name="requirement">The requirement instance being evaluated.</param>
    /// <param name="cohortClassId">Id of the cohort class the caller must own (or be Admin) to access.</param>
    /// <returns>A completed task; the outcome is reported via <paramref name="context"/>.</returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TeacherOwnsCohortClassRequirement requirement,
        int cohortClassId)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (new CurrentUserInfo(context.User).CohortClassIds.Contains(cohortClassId))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
