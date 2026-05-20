using Microsoft.AspNetCore.Authorization;

namespace ExamHub.API.Authorization;

/// <summary>Authorization requirement: caller must be assigned to the target subject (or be Admin).</summary>
public sealed class TeacherOwnsSubjectRequirement : IAuthorizationRequirement { }
