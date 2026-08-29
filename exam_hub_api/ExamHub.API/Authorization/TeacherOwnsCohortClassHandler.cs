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
