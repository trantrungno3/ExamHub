using Microsoft.AspNetCore.Authorization;

namespace ExamHub.API.Authorization;

/// <summary>Authorization requirement: caller must be assigned to the target cohort class (or be Admin).</summary>
public sealed class TeacherOwnsCohortClassRequirement : IAuthorizationRequirement { }
