using System.Security.Claims;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TVT.Core.Claims;

namespace ExamHub.Tests;

public class AuditStampTests
{
    [Fact]
    public void Added_entity_gets_creation_and_modification_audit()
    {
        var now = new DateTime(2026, 9, 19, 12, 0, 0, DateTimeKind.Utc);
        using var db = ContextFor("admin1");
        var entity = new GradeLevel
        {
            Name = "Lớp 10",
            GradeNumber = 10,
            Created = null,
            CreatedBy = null,
            Modified = null,
            ModifiedBy = null,
        };
        db.Add(entity);

        db.ApplyAuditFields(now);

        Assert.Equal(now, entity.Created);
        Assert.Equal("admin1", entity.CreatedBy);
        Assert.Equal(now, entity.Modified);
        Assert.Equal("admin1", entity.ModifiedBy);
    }

    [Fact]
    public void Modified_entity_preserves_creation_audit_and_stamps_current_editor()
    {
        var created = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var now = new DateTime(2026, 9, 19, 12, 0, 0, DateTimeKind.Utc);
        using var db = ContextFor("editor1");
        var entity = new GradeLevel
        {
            Id = 1,
            Name = "Lớp 10",
            GradeNumber = 10,
            Created = created,
            CreatedBy = "creator1",
            Modified = created,
            ModifiedBy = "old-editor",
        };
        db.Update(entity);

        db.ApplyAuditFields(now);

        Assert.False(db.Entry(entity).Property(x => x.Created).IsModified);
        Assert.False(db.Entry(entity).Property(x => x.CreatedBy).IsModified);
        Assert.Equal(created, entity.Created);
        Assert.Equal("creator1", entity.CreatedBy);
        Assert.Equal(now, entity.Modified);
        Assert.Equal("editor1", entity.ModifiedBy);
    }

    private static AppDbContext ContextFor(string userName)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ConstClaim.UserName, userName)], "test"));
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new AppDbContext(options, accessor);
    }
}
