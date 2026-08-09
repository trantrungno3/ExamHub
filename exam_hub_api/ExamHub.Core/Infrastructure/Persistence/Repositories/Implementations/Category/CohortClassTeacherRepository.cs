using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;
using ExamHub.Core.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ExamHub.Core.Infrastructure.Persistence.Repositories.Implementations;

/// <summary>Triển khai repository cho CohortClassTeacher</summary>
public class CohortClassTeacherRepository : BaseRepository<CohortClassTeacher, int>, ICohortClassTeacherRepository
{
    public CohortClassTeacherRepository(AppDbContext db) : base(db) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guid>> GetEligibleTeacherIdsAsync(int cohortClassId, int subjectId, CancellationToken ct = default)
    {
        // Trường sở hữu khoá của lớp
        var schoolId = await Db.CohortClasses.AsNoTracking()
            .Where(cc => cc.Id == cohortClassId)
            .Join(Db.Cohorts, cc => cc.CohortId, c => c.Id, (cc, c) => c.SchoolId)
            .FirstOrDefaultAsync(ct);

        if (schoolId == 0) return Array.Empty<Guid>();

        // GV = thành viên trường (Teacher, active) ∩ có môn trong teacher_subjects
        return await Db.SchoolMembers.AsNoTracking()
            .Where(sm => sm.SchoolId == schoolId && sm.Role == "Teacher" && sm.IsActive)
            .Where(sm => Db.TeacherSubjects.Any(ts => ts.UserId == sm.UserId && ts.SubjectId == subjectId))
            .Select(sm => sm.UserId)
            .Distinct()
            .ToListAsync(ct);
    }
}
