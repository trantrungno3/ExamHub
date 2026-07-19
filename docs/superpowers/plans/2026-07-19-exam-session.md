# Kỳ thi (Exam Session) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Thêm tính năng "Kỳ thi" — cấu hình thi theo môn+cấp lớp, giao cho lớp/khoá, chứa nhiều đề; học sinh vào thi được bốc/chọn 1 đề (khoá theo lượt).

**Architecture:** Backend .NET (EF Core + Dapper) thêm 3 bảng (`exam_sessions`, `exam_session_exams`, `exam_session_assignments`) + 2 cột vào `exam_submissions`; service `ExamSessionService` giữ logic bốc-và-khoá đề trong transaction; controller REST. Frontend React (antd + react-query) thêm trang quản lý + trang học sinh, và nâng menu sidebar thành nhóm cha "Quản lý đề thi".

**Tech Stack:** .NET 10, EF Core (Npgsql, snake_case), Dapper; React 18 + TypeScript, Ant Design, @tanstack/react-query, react-router-dom.

---

## ⏳ TIẾN ĐỘ THỰC THI (cập nhật 2026-07-19)

**Chế độ:** Subagent-Driven Development. **Nhánh:** `feat/exam-session` (tách từ `main` sau commit `f106cda`). **Ledger:** `.superpowers/sdd/progress.md`.

**Ràng buộc môi trường đã chốt với chủ dự án (áp cho MỌI task khi resume):**
- Subagent CHỈ build `ExamHub.Core` (`dotnet build ExamHub.Core/ExamHub.Core.csproj --no-dependencies`) và chạy `npx tsc --noEmit` cho web.
- KHÔNG build project `ExamHub.API` (đang bị debugger khoá DLL) → Task 8–9 viết code nhưng **hoãn build API + chạy thử** cho chủ dự án.
- KHÔNG chạy `dotnet ef migrations add` / `database update` → **Task 3 chỉ cấu hình DbContext + cập nhật `database_schema.sql`**; việc tạo & áp migration để chủ dự án tự làm sau (khi dừng app).
- Không có test project → verify = build/tsc thành công + review diff.
- WIP khác của chủ dự án (CurrentUserInfo.cs, Converters/, ExamSubmission*, topic/index.tsx, seed/...) đang để nguyên chưa commit — KHÔNG đụng.

**Trạng thái task:**
- [x] **Task 1** — Enums + FieldTables + Entities. DONE, commit `caa83f3`, Core build 0 errors. *(task-review chưa chạy)*
- [ ] Task 2 — ExamSubmission +SessionId/AttemptNo *(← RESUME TẠI ĐÂY)*
- [ ] Task 3 — DbContext config + schema.sql (KHÔNG chạy ef; hoãn migration)
- [ ] Task 4 — DTOs
- [ ] Task 5 — Repository
- [ ] Task 6 — ExamSessionService (pick/lock)
- [ ] Task 7 — Submit adaptation (đụng ExamSubmissionService/Controller — lưu ý WIP hiện có)
- [ ] Task 8 — Controller + DI (hoãn build API)
- [ ] Task 9 — Menu backend (hoãn build API)
- [ ] Task 10 — Sidebar nhóm cha (web)
- [ ] Task 11 — Types + service + hooks (web)
- [ ] Task 12 — ExamSessionListPage
- [ ] Task 13 — ExamSessionEditPage
- [ ] Task 14 — Student session pages
- [ ] Task 15 — Taking flow mang session/submission

**Cách resume:** đọc `.superpowers/sdd/progress.md`, tiếp tục Subagent-Driven từ Task 2. (Cân nhắc chạy task-review cho Task 1 trước, hoặc gộp vào review tổng cuối.)

## Global Constraints

- Spec nguồn: `docs/superpowers/specs/2026-07-19-exam-session-design.md`. Mọi quyết định (pick_mode, giao khoá→cohort_members, pool chỉ đề published cùng môn+cấp lớp) theo spec.
- Backend đang **chạy dưới debugger** khi phát triển → build project `ExamHub.Core` bằng `dotnet build ExamHub.Core/ExamHub.Core.csproj --no-dependencies` để tránh khoá DLL của `ExamHub.API`.
- **Không có test project** trong repo → verification = build + `dotnet ef` + typecheck (`npx tsc --noEmit`) + kiểm thử endpoint/UI thủ công. Không tạo test harness mới.
- Entities theo pattern hiện có: kế thừa `ModifyModelBase, IModelBaseSql<Guid|int>`, dùng `[Table]`/`[Column]`/`[SqlBuilderProperty]`, có `ToInsertObject()`/`ToUpdateObject()`, cấu hình trong `AppDbContext.OnModelCreating`.
- Controllers kế thừa `AuthorizeControllerBase`, trả `RequestResponse<T>`; lấy user qua `User.GetTag()` / `CurrentUser.UserId`. Endpoint quản lý gắn `[Authorize(Roles = "Admin,Teacher")]`.
- Đăng ký DI trong `ExamHub.Core/DependencyContainer.cs` (`AddRepositories`, `AddAppServices`).
- Cập nhật đồng bộ `exam_hub_api/database_schema.sql` với migration EF.
- Repo là monorepo, gốc `D:/My-Project/ExamHub`. Commit theo từng task.
- Tất cả chuỗi hiển thị bằng tiếng Việt, khớp giọng văn hiện có.

---

## FILE STRUCTURE

**Backend — tạo mới:**
- `ExamHub.Core/FieldTables/ExamSessionTable.cs`, `ExamSessionExamTable.cs`, `ExamSessionAssignmentTable.cs`
- `ExamHub.Core/Domain/Entities/ExamSession.cs`, `ExamSessionExam.cs`, `ExamSessionAssignment.cs`
- `ExamHub.Core/Domain/Enums/ExamSessionStatusEnum.cs`, `ExamSessionPickModeEnum.cs`
- `ExamHub.Core/Domain/Interfaces/IExamSessionRepositories.cs`
- `ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs`
- `ExamHub.Core/Application/Services/IExamSessionService.cs`
- `ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs`
- `ExamHub.Core/DataTransferObjects/ExamSession/ExamSessionDtos.cs`
- `ExamHub.API/Controllers/Exam/ExamSessionController.cs`
- EF migration dưới `ExamHub.Core/Infrastructure/Persistence/Migrations/`

**Backend — sửa:**
- `ExamHub.Core/Domain/Entities/ExamSubmission.cs` (+ `SessionId`, `AttemptNo`)
- `ExamHub.Core/FieldTables/ExamSubmissionTable.cs` (+ cột)
- `ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs` (+ DbSet + config)
- `ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs` (submit cập nhật submission in_progress khi có session)
- `ExamHub.Core/DependencyContainer.cs` (DI)
- `ExamHub.API/Controllers/Menu/MenuItemResponse.cs`, `MenuRegistry.cs` (nhóm cha)
- `exam_hub_api/database_schema.sql`

**Frontend — tạo mới:**
- `src/types/examSession.d.ts`
- `src/services/examSessionService.ts`
- `src/hooks/queries/useExamSessions.ts`
- `src/pages/exams/ExamSessionListPage.tsx`, `ExamSessionEditPage.tsx`
- `src/pages/student/StudentSessionListPage.tsx`, `StudentSessionPoolPage.tsx`

**Frontend — sửa:**
- `src/services/menuService.ts` types + `src/types/*` (MenuItem children)
- `src/layouts/AppLayout.tsx` (render nhóm), `src/routes/index.tsx`, `src/routes/paths.ts`
- `src/pages/student/ExamCoverPage.tsx`, `ExamTakingPage.tsx` + submit hook (truyền sessionId/submissionId)

---

## PHASE 1 — BACKEND DATA LAYER

### Task 1: Enums + FieldTables + Entities cho Exam Session

**Files:**
- Create: `ExamHub.Core/Domain/Enums/ExamSessionStatusEnum.cs`
- Create: `ExamHub.Core/Domain/Enums/ExamSessionPickModeEnum.cs`
- Create: `ExamHub.Core/FieldTables/ExamSessionTable.cs`
- Create: `ExamHub.Core/FieldTables/ExamSessionExamTable.cs`
- Create: `ExamHub.Core/FieldTables/ExamSessionAssignmentTable.cs`
- Create: `ExamHub.Core/Domain/Entities/ExamSession.cs`
- Create: `ExamHub.Core/Domain/Entities/ExamSessionExam.cs`
- Create: `ExamHub.Core/Domain/Entities/ExamSessionAssignment.cs`

**Interfaces:**
- Produces: entity `ExamSession` (Guid Id, string Title, string? Description, int SubjectId, int GradeLevelId, DateTime OpenAt, DateTime CloseAt, short MaxAttempts, `ExamSessionPickModeEnum` PickMode, `ExamSessionStatusEnum` Status); `ExamSessionExam` (Guid Id, Guid SessionId, Guid ExamId); `ExamSessionAssignment` (Guid Id, Guid SessionId, int? CohortId, int? CohortClassId).

- [ ] **Step 1: Tạo enums**

`ExamSessionStatusEnum.cs`:
```csharp
namespace ExamHub.Core.Domain.Enums;

/// <summary>Trạng thái kỳ thi.</summary>
public enum ExamSessionStatusEnum
{
    Draft,
    Published,
    Closed
}
```

`ExamSessionPickModeEnum.cs`:
```csharp
namespace ExamHub.Core.Domain.Enums;

/// <summary>Cách chọn đề khi học sinh vào thi.</summary>
public enum ExamSessionPickModeEnum
{
    /// <summary>Hệ thống bốc ngẫu nhiên.</summary>
    Random,
    /// <summary>Học sinh tự chọn đề trong pool.</summary>
    StudentChoice
}
```

- [ ] **Step 2: Tạo FieldTables** (theo mẫu `ExamTable.cs`)

`ExamSessionTable.cs`:
```csharp
namespace ExamHub.Core.FieldTables;

/// <summary>Tên bảng và cột cho bảng exam_sessions.</summary>
public readonly struct ExamSessionTable
{
    public const string TableName = "public.exam_sessions";
    public const string Title = "title";
    public const string Description = "description";
    public const string SubjectId = "subject_id";
    public const string GradeLevelId = "grade_level_id";
    public const string OpenAt = "open_at";
    public const string CloseAt = "close_at";
    public const string MaxAttempts = "max_attempts";
    public const string PickMode = "pick_mode";
    public const string Status = "status";
}
```

`ExamSessionExamTable.cs`:
```csharp
namespace ExamHub.Core.FieldTables;

public readonly struct ExamSessionExamTable
{
    public const string TableName = "public.exam_session_exams";
    public const string SessionId = "session_id";
    public const string ExamId = "exam_id";
}
```

`ExamSessionAssignmentTable.cs`:
```csharp
namespace ExamHub.Core.FieldTables;

public readonly struct ExamSessionAssignmentTable
{
    public const string TableName = "public.exam_session_assignments";
    public const string SessionId = "session_id";
    public const string CohortId = "cohort_id";
    public const string CohortClassId = "cohort_class_id";
}
```

- [ ] **Step 3: Tạo entity `ExamSession.cs`** (theo mẫu `Exam.cs`; status/pick_mode lưu chuỗi thường)

```csharp
using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.Domain.Enums;
using ExamHub.Core.FieldTables;
using TVT.Core.Models;

namespace ExamHub.Core.Domain.Entities;

/// <summary>Kỳ thi — cấu hình thi theo môn + cấp lớp, giao cho lớp/khoá.</summary>
[Table(ExamSessionTable.TableName)]
[SqlBuilderProperty(ExamSessionTable.TableName)]
public class ExamSession : ModifyModelBase, IModelBaseSql<Guid>
{
    [Column(CommonFieldTable.Id)]
    [SqlBuilderProperty(CommonFieldTable.Id, Insert = true, Update = false)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(ExamSessionTable.Title)]
    [SqlBuilderProperty(ExamSessionTable.Title, Insert = true, Update = true)]
    public required string Title { get; set; }

    [Column(ExamSessionTable.Description)]
    [SqlBuilderProperty(ExamSessionTable.Description, Insert = true, Update = true)]
    public string? Description { get; set; }

    [Column(ExamSessionTable.SubjectId)]
    [SqlBuilderProperty(ExamSessionTable.SubjectId, Insert = true, Update = true)]
    public int SubjectId { get; set; }

    [Column(ExamSessionTable.GradeLevelId)]
    [SqlBuilderProperty(ExamSessionTable.GradeLevelId, Insert = true, Update = true)]
    public int GradeLevelId { get; set; }

    [Column(ExamSessionTable.OpenAt)]
    [SqlBuilderProperty(ExamSessionTable.OpenAt, Insert = true, Update = true)]
    public DateTime OpenAt { get; set; }

    [Column(ExamSessionTable.CloseAt)]
    [SqlBuilderProperty(ExamSessionTable.CloseAt, Insert = true, Update = true)]
    public DateTime CloseAt { get; set; }

    [Column(ExamSessionTable.MaxAttempts)]
    [SqlBuilderProperty(ExamSessionTable.MaxAttempts, Insert = true, Update = true)]
    public short MaxAttempts { get; set; } = 1;

    [Column(ExamSessionTable.PickMode)]
    [SqlBuilderProperty(ExamSessionTable.PickMode, Insert = true, Update = true)]
    public ExamSessionPickModeEnum PickMode { get; set; } = ExamSessionPickModeEnum.Random;

    [Column(ExamSessionTable.Status)]
    [SqlBuilderProperty(ExamSessionTable.Status, Insert = true, Update = true)]
    public ExamSessionStatusEnum Status { get; set; } = ExamSessionStatusEnum.Draft;

    // ── Navigation ──
    public Subject? Subject { get; set; }
    public GradeLevel? GradeLevel { get; set; }
    public List<ExamSessionExam> Exams { get; set; } = [];
    public List<ExamSessionAssignment> Assignments { get; set; } = [];

    public object ToInsertObject() => new
    {
        id = Id, title = Title, description = Description,
        subject_id = SubjectId, grade_level_id = GradeLevelId,
        open_at = OpenAt, close_at = CloseAt, max_attempts = MaxAttempts,
        pick_mode = PickMode.ToString(), status = Status.ToString().ToLower(),
        created = Created, created_by = CreatedBy, modified = Modified, modified_by = ModifiedBy
    };

    public object ToUpdateObject() => new
    {
        id = Id, title = Title, description = Description,
        subject_id = SubjectId, grade_level_id = GradeLevelId,
        open_at = OpenAt, close_at = CloseAt, max_attempts = MaxAttempts,
        pick_mode = PickMode.ToString(), status = Status.ToString().ToLower(),
        modified = DateTime.UtcNow, modified_by = ModifiedBy
    };
}
```

> Lưu ý: `pick_mode` lưu PascalCase (`Random`/`StudentChoice`) qua `PickMode.ToString()`; DbContext sẽ dùng converter khớp (Task 3). `status` lưu lowercase khớp CHECK trong migration.

- [ ] **Step 4: Tạo entity `ExamSessionExam.cs` và `ExamSessionAssignment.cs`**

`ExamSessionExam.cs`:
```csharp
using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>Đề thi thuộc pool của một kỳ thi.</summary>
[Table(ExamSessionExamTable.TableName)]
[SqlBuilderProperty(ExamSessionExamTable.TableName)]
public class ExamSessionExam
{
    [Column(CommonFieldTable.Id)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(ExamSessionExamTable.SessionId)]
    public Guid SessionId { get; set; }

    [Column(ExamSessionExamTable.ExamId)]
    public Guid ExamId { get; set; }

    public Exam? Exam { get; set; }
}
```

`ExamSessionAssignment.cs`:
```csharp
using System.ComponentModel.DataAnnotations.Schema;
using TVT.Core.Attributes;
using TVT.Core.Models.PostgreSql.FieldTables;
using ExamHub.Core.FieldTables;

namespace ExamHub.Core.Domain.Entities;

/// <summary>Giao kỳ thi cho một lớp (cohort_class) hoặc một khoá (cohort).</summary>
[Table(ExamSessionAssignmentTable.TableName)]
[SqlBuilderProperty(ExamSessionAssignmentTable.TableName)]
public class ExamSessionAssignment
{
    [Column(CommonFieldTable.Id)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column(ExamSessionAssignmentTable.SessionId)]
    public Guid SessionId { get; set; }

    [Column(ExamSessionAssignmentTable.CohortId)]
    public int? CohortId { get; set; }

    [Column(ExamSessionAssignmentTable.CohortClassId)]
    public int? CohortClassId { get; set; }
}
```

- [ ] **Step 5: Build Core**

Run: `cd /d/My-Project/ExamHub/exam_hub_api && dotnet build ExamHub.Core/ExamHub.Core.csproj --no-dependencies -nologo -clp:ErrorsOnly`
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 6: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/Domain/Enums exam_hub_api/ExamHub.Core/FieldTables exam_hub_api/ExamHub.Core/Domain/Entities/ExamSession*.cs
git commit -m "feat(api): add exam session entities, enums, field tables"
```

---

### Task 2: Sửa `ExamSubmission` — thêm SessionId + AttemptNo

**Files:**
- Modify: `ExamHub.Core/FieldTables/ExamSubmissionTable.cs`
- Modify: `ExamHub.Core/Domain/Entities/ExamSubmission.cs`

**Interfaces:**
- Produces: `ExamSubmission.SessionId (Guid?)`, `ExamSubmission.AttemptNo (short)`.

- [ ] **Step 1: Thêm hằng cột** vào `ExamSubmissionTable.cs` (thêm 2 dòng trong struct):

```csharp
public const string SessionId = "session_id";
public const string AttemptNo = "attempt_no";
```

- [ ] **Step 2: Thêm property vào `ExamSubmission.cs`** (sau `StudentId`):

```csharp
/// <summary>Kỳ thi (null = đề trực tiếp, luồng cũ)</summary>
[Column(ExamSubmissionTable.SessionId)]
[SqlBuilderProperty(ExamSubmissionTable.SessionId, Insert = true, Update = false)]
public Guid? SessionId { get; set; }

/// <summary>Số thứ tự lượt làm trong kỳ thi</summary>
[Column(ExamSubmissionTable.AttemptNo)]
[SqlBuilderProperty(ExamSubmissionTable.AttemptNo, Insert = true, Update = false)]
public short AttemptNo { get; set; } = 1;
```

- [ ] **Step 3: Thêm vào `ToInsertObject()`** (bổ sung 2 khóa vào object literal):

```csharp
session_id = SessionId,
attempt_no = AttemptNo,
```

- [ ] **Step 4: Build Core** — `dotnet build ExamHub.Core/ExamHub.Core.csproj --no-dependencies -nologo -clp:ErrorsOnly` → 0 errors.

- [ ] **Step 5: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/Domain/Entities/ExamSubmission.cs exam_hub_api/ExamHub.Core/FieldTables/ExamSubmissionTable.cs
git commit -m "feat(api): add session_id and attempt_no to exam_submissions entity"
```

---

### Task 3: Cấu hình EF (DbContext) + tạo migration + cập nhật schema.sql

**Files:**
- Modify: `ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs`
- Create: migration files (sinh bởi `dotnet ef`)
- Modify: `exam_hub_api/database_schema.sql`

**Interfaces:**
- Consumes: entities từ Task 1–2.
- Produces: bảng `exam_sessions`, `exam_session_exams`, `exam_session_assignments`, cột mới trên `exam_submissions`; `DbSet<ExamSession> ExamSessions`.

- [ ] **Step 1: Thêm DbSet** vào `AppDbContext.cs` (khu vực khai báo DbSet, cạnh `Exams`):

```csharp
/// <summary>Kỳ thi</summary>
public DbSet<ExamSession> ExamSessions { get; set; }
public DbSet<ExamSessionExam> ExamSessionExams { get; set; }
public DbSet<ExamSessionAssignment> ExamSessionAssignments { get; set; }
```

- [ ] **Step 2: Cấu hình model** — thêm vào cuối `OnModelCreating` (trước dấu `}` đóng phương thức), theo mẫu block `ExamQuestion`:

```csharp
// ── ExamSession ────────────────────────────────────────────────────
modelBuilder.Entity<ExamSession>(e =>
{
    e.ToTable("exam_sessions");
    e.HasKey(x => x.Id);
    e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
    e.Property(x => x.Title).HasMaxLength(300).IsRequired();
    e.Property(x => x.Status)
        .HasConversion(new SnakeCaseEnumConverter<ExamSessionStatusEnum>())
        .HasMaxLength(20)
        .HasDefaultValue(ExamSessionStatusEnum.Draft);
    e.Property(x => x.PickMode)
        .HasConversion<string>()
        .HasMaxLength(20)
        .HasDefaultValue(ExamSessionPickModeEnum.Random);
    e.Property(x => x.MaxAttempts).HasDefaultValue((short)1);
    e.HasIndex(x => new { x.SubjectId, x.GradeLevelId, x.Status });
    e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
    e.HasOne(x => x.GradeLevel).WithMany().HasForeignKey(x => x.GradeLevelId).OnDelete(DeleteBehavior.Restrict);
});

// ── ExamSessionExam ────────────────────────────────────────────────
modelBuilder.Entity<ExamSessionExam>(e =>
{
    e.ToTable("exam_session_exams");
    e.HasKey(x => x.Id);
    e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
    e.HasIndex(x => new { x.SessionId, x.ExamId }).IsUnique();
    e.HasOne<ExamSession>().WithMany(s => s.Exams).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
    e.HasOne(x => x.Exam).WithMany().HasForeignKey(x => x.ExamId).OnDelete(DeleteBehavior.Restrict);
});

// ── ExamSessionAssignment ──────────────────────────────────────────
modelBuilder.Entity<ExamSessionAssignment>(e =>
{
    e.ToTable("exam_session_assignments");
    e.HasKey(x => x.Id);
    e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
    e.HasIndex(x => x.SessionId);
    e.HasIndex(x => x.CohortId);
    e.HasIndex(x => x.CohortClassId);
    e.HasOne<ExamSession>().WithMany(s => s.Assignments).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
});

// ── ExamSubmission: liên kết kỳ thi ────────────────────────────────
modelBuilder.Entity<ExamSubmission>(e =>
{
    e.Property(x => x.AttemptNo).HasDefaultValue((short)1);
    e.HasIndex(x => new { x.SessionId, x.StudentId });
    e.HasOne<ExamSession>().WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
});
```

> Kiểm tra `SnakeCaseEnumConverter` đã được import/khả dụng (đã dùng cho `ExamSubmission.Status` trong file). `PickMode` dùng `HasConversion<string>()` để lưu `Random`/`StudentChoice`.

- [ ] **Step 3: Build Core** → 0 errors.

- [ ] **Step 4: Tạo migration EF**

Run (cần dừng app đang chạy để không khoá DLL; chạy tại thư mục solution):
```bash
cd /d/My-Project/ExamHub/exam_hub_api && dotnet ef migrations add AddExamSessions --project ExamHub.Core --startup-project ExamHub.API
```
Expected: sinh file migration `*_AddExamSessions.cs` trong `ExamHub.Core/Infrastructure/Persistence/Migrations/`, không lỗi.

- [ ] **Step 5: Kiểm tra migration** — mở file `*_AddExamSessions.cs`, xác nhận: tạo 3 bảng, thêm cột `session_id`+`attempt_no` vào `exam_submissions`, có unique index `(session_id, exam_id)`. Thêm thủ công 2 CHECK constraint mà EF không tự sinh, vào cuối phương thức `Up` (dùng `migrationBuilder.Sql`):

```csharp
migrationBuilder.Sql(@"ALTER TABLE public.exam_sessions
    ADD CONSTRAINT chk_exam_sessions_close_after_open CHECK (close_at > open_at);");
migrationBuilder.Sql(@"ALTER TABLE public.exam_session_assignments
    ADD CONSTRAINT chk_assignment_target CHECK (
        (cohort_id IS NOT NULL)::int + (cohort_class_id IS NOT NULL)::int = 1);");
```
Và trong `Down`, drop 2 constraint tương ứng trước khi drop bảng (EF thường tự lo drop bảng; chỉ cần thêm nếu `Down` giữ bảng — nếu `Down` drop bảng thì bỏ qua).

- [ ] **Step 6: Áp migration vào DB dev**

Run: `cd /d/My-Project/ExamHub/exam_hub_api && dotnet ef database update --project ExamHub.Core --startup-project ExamHub.API`
Expected: `Done.` không lỗi.

- [ ] **Step 7: Cập nhật `database_schema.sql`** — thêm 3 khối `CREATE TABLE` (đặt sau bảng `exams`/`exam_questions`, trước phần INDEXES) và 2 cột vào `exam_submissions`:

```sql
-- ============================================================
-- PHẦN 6b: KỲ THI (Exam Sessions)
-- ============================================================
CREATE TABLE public.exam_sessions
(
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title          VARCHAR(300) NOT NULL,
    description    TEXT,
    subject_id     INT NOT NULL REFERENCES subjects (id),
    grade_level_id INT NOT NULL REFERENCES grade_levels (id),
    open_at        TIMESTAMPTZ NOT NULL,
    close_at       TIMESTAMPTZ NOT NULL,
    max_attempts   SMALLINT NOT NULL DEFAULT 1 CHECK (max_attempts >= 1),
    pick_mode      VARCHAR(20) NOT NULL DEFAULT 'Random'
                   CHECK (pick_mode IN ('Random','StudentChoice')),
    status         VARCHAR(20) NOT NULL DEFAULT 'draft'
                   CHECK (status IN ('draft','published','closed')),
    created        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by     VARCHAR(150),
    modified       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by    VARCHAR(150),
    CHECK (close_at > open_at)
);

CREATE TABLE public.exam_session_exams
(
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES exam_sessions (id) ON DELETE CASCADE,
    exam_id    UUID NOT NULL REFERENCES exams (id),
    UNIQUE (session_id, exam_id)
);

CREATE TABLE public.exam_session_assignments
(
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id      UUID NOT NULL REFERENCES exam_sessions (id) ON DELETE CASCADE,
    cohort_id       INT REFERENCES cohorts (id) ON DELETE CASCADE,
    cohort_class_id INT REFERENCES cohort_classes (id) ON DELETE CASCADE,
    CHECK ((cohort_id IS NOT NULL)::int + (cohort_class_id IS NOT NULL)::int = 1),
    UNIQUE (session_id, cohort_id, cohort_class_id)
);
```
Và trong `CREATE TABLE public.exam_submissions`, thêm sau `student_id`:
```sql
    session_id       UUID REFERENCES exam_sessions (id),
    attempt_no       SMALLINT NOT NULL DEFAULT 1,
```

- [ ] **Step 8: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/Infrastructure/Persistence exam_hub_api/database_schema.sql
git commit -m "feat(api): EF config + migration + schema.sql for exam sessions"
```

---

## PHASE 2 — BACKEND DTOs + REPOSITORY

### Task 4: DTOs cho Exam Session

**Files:**
- Create: `ExamHub.Core/DataTransferObjects/ExamSession/ExamSessionDtos.cs`

**Interfaces:**
- Produces: `CreateExamSessionRequest`, `UpdateExamSessionRequest`, `SetSessionExamsRequest(IReadOnlyList<Guid> ExamIds)`, `CreateAssignmentRequest(int? CohortId, int? CohortClassId)`, `StartSessionRequest(Guid? ExamId)`; responses `ExamSessionResponse`, `ExamSessionDetailResponse`, `SessionExamResponse`, `MySessionResponse`, `SessionPoolItemResponse`, `StartSessionResponse(Guid SubmissionId, Guid ExamId)`.

- [ ] **Step 1: Tạo file DTO** (một file gom, theo phong cách `ExamTemplateDto.cs`). Đầy đủ:

```csharp
using System.ComponentModel.DataAnnotations;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.DataTransferObjects.ExamSession;

/// <summary>Request tạo kỳ thi.</summary>
public sealed record CreateExamSessionRequest
{
    [Required, MaxLength(300)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int SubjectId { get; set; }
    [Range(1, int.MaxValue)] public int GradeLevelId { get; set; }
    [Required] public DateTime OpenAt { get; set; }
    [Required] public DateTime CloseAt { get; set; }
    [Range(1, 100)] public short MaxAttempts { get; set; } = 1;
    [RegularExpression("^(Random|StudentChoice)$")] public string PickMode { get; set; } = "Random";

    public Domain.Entities.ExamSession ToEntity() => new()
    {
        Title = Title, Description = Description, SubjectId = SubjectId, GradeLevelId = GradeLevelId,
        OpenAt = OpenAt.ToUniversalTime(), CloseAt = CloseAt.ToUniversalTime(), MaxAttempts = MaxAttempts,
        PickMode = Enum.Parse<ExamSessionPickModeEnum>(PickMode),
        Status = ExamSessionStatusEnum.Draft
    };
}

/// <summary>Request cập nhật kỳ thi (không đổi trạng thái ở đây).</summary>
public sealed record UpdateExamSessionRequest
{
    [Required, MaxLength(300)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int SubjectId { get; set; }
    [Range(1, int.MaxValue)] public int GradeLevelId { get; set; }
    [Required] public DateTime OpenAt { get; set; }
    [Required] public DateTime CloseAt { get; set; }
    [Range(1, 100)] public short MaxAttempts { get; set; } = 1;
    [RegularExpression("^(Random|StudentChoice)$")] public string PickMode { get; set; } = "Random";
}

public sealed record SetSessionExamsRequest(IReadOnlyList<Guid> ExamIds);
public sealed record CreateAssignmentRequest(int? CohortId, int? CohortClassId);
public sealed record StartSessionRequest(Guid? ExamId);

/// <summary>Tóm tắt kỳ thi cho danh sách quản lý.</summary>
public sealed record ExamSessionResponse(
    Guid Id, string Title, int SubjectId, string? SubjectName,
    int GradeLevelId, string? GradeLevelName,
    long OpenAt, long CloseAt, short MaxAttempts, string PickMode, string Status,
    int ExamCount, int AssignmentCount)
{
    public static ExamSessionResponse FromEntity(Domain.Entities.ExamSession s) => new(
        s.Id, s.Title, s.SubjectId, s.Subject?.Name, s.GradeLevelId, s.GradeLevel?.Name,
        new DateTimeOffset(s.OpenAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
        new DateTimeOffset(s.CloseAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
        s.MaxAttempts, s.PickMode.ToString(), s.Status.ToString().ToLower(),
        s.Exams.Count, s.Assignments.Count);
}

public sealed record SessionExamResponse(Guid ExamId, string Title, string? ExamCode, decimal TotalScore);

/// <summary>Chi tiết kỳ thi kèm pool đề + assignments.</summary>
public sealed record ExamSessionDetailResponse(
    Guid Id, string Title, string? Description, int SubjectId, string? SubjectName,
    int GradeLevelId, string? GradeLevelName, long OpenAt, long CloseAt,
    short MaxAttempts, string PickMode, string Status,
    IReadOnlyList<SessionExamResponse> Exams,
    IReadOnlyList<AssignmentResponse> Assignments);

public sealed record AssignmentResponse(Guid Id, int? CohortId, string? CohortName, int? CohortClassId, string? CohortClassName);

/// <summary>Kỳ thi được giao — hiển thị phía học sinh.</summary>
public sealed record MySessionResponse(
    Guid Id, string Title, string? SubjectName, string? GradeLevelName,
    long OpenAt, long CloseAt, string PickMode, string Availability,
    short MaxAttempts, int UsedAttempts,
    Guid? InProgressSubmissionId, Guid? InProgressExamId);

/// <summary>Một đề trong pool + trạng thái của học sinh (dùng cho student_choice).</summary>
public sealed record SessionPoolItemResponse(
    Guid ExamId, string Title, string? ExamCode, decimal TotalScore,
    string StudentState, Guid? SubmissionId);

public sealed record StartSessionResponse(Guid SubmissionId, Guid ExamId);
```

- [ ] **Step 2: Build Core** → 0 errors.

- [ ] **Step 3: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/DataTransferObjects/ExamSession
git commit -m "feat(api): add exam session DTOs"
```

---

### Task 5: Repository — `IExamSessionRepository` + implementation

**Files:**
- Create: `ExamHub.Core/Domain/Interfaces/IExamSessionRepositories.cs`
- Create: `ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs`

**Interfaces:**
- Consumes: `AppDbContext`, entities.
- Produces: `IExamSessionRepository` với các phương thức:
  - `Task<ExamSession?> GetDetailAsync(Guid id, CancellationToken ct)` — kèm Subject, GradeLevel, Exams(+Exam), Assignments.
  - `Task<(IReadOnlyList<ExamSession> Items, int Total)> GetPagedAsync(int page, int pageSize, int? subjectId, int? gradeLevelId, ExamSessionStatusEnum? status, string? keyword, CancellationToken ct)` — kèm Subject/GradeLevel + Counts.
  - `Task<ExamSession?> GetByIdAsync(Guid id, CancellationToken ct)`
  - `Task AddAsync(ExamSession s, CancellationToken ct)`, `Task UpdateAsync(ExamSession s, CancellationToken ct)`, `Task DeleteAsync(Guid id, CancellationToken ct)`
  - `Task SetStatusAsync(Guid id, ExamSessionStatusEnum status, CancellationToken ct)`
  - `Task AddExamsAsync(Guid sessionId, IEnumerable<Guid> examIds, CancellationToken ct)` (bỏ trùng), `Task RemoveExamAsync(Guid sessionId, Guid examId, CancellationToken ct)`
  - `Task<IReadOnlyList<Exam>> GetPoolExamsAsync(Guid sessionId, CancellationToken ct)` (các Exam trong pool)
  - `Task AddAssignmentAsync(ExamSessionAssignment a, CancellationToken ct)`, `Task RemoveAssignmentAsync(Guid assignmentId, CancellationToken ct)`
  - `Task<IReadOnlyList<ExamSession>> GetAssignedToStudentAsync(Guid studentId, CancellationToken ct)` — kỳ thi `published` giao tới cohort chứa student (qua cohort_members) hoặc cohort_class thuộc cohort đó; kèm Subject/GradeLevel + Assignments.
  - `Task<int> CountSubmittedAttemptsAsync(Guid sessionId, Guid studentId, CancellationToken ct)` — đếm submission status submitted/graded.
  - `Task<ExamSubmission?> GetInProgressAsync(Guid sessionId, Guid studentId, CancellationToken ct)`
  - `Task<bool> PoolContainsAsync(Guid sessionId, Guid examId, CancellationToken ct)`
  - `Task<IReadOnlyList<ExamSubmission>> GetStudentSubmissionsAsync(Guid sessionId, Guid studentId, CancellationToken ct)` — để tính trạng thái pool + used attempts.
  - `Task<bool> IsStudentAssignedAsync(Guid sessionId, Guid studentId, CancellationToken ct)`

- [ ] **Step 1: Tạo interface** `IExamSessionRepositories.cs` với đúng các chữ ký trên trong `namespace ExamHub.Core.Domain.Interfaces;` (khai báo `public interface IExamSessionRepository { ... }`).

- [ ] **Step 2: Tạo implementation** `ExamSessionRepository.cs`. Dùng EF Core (`AppDbContext`), theo mẫu các repository EF hiện có. Điểm mấu chốt — truy vấn "kỳ thi giao cho học sinh" (giải thích §5.3 của spec):

```csharp
public async Task<IReadOnlyList<ExamSession>> GetAssignedToStudentAsync(Guid studentId, CancellationToken ct)
{
    // cohort của student (đang active)
    var cohortIds = await _db.Set<CohortMember>()
        .Where(m => m.StudentId == studentId && m.IsActive)
        .Select(m => m.CohortId)
        .ToListAsync(ct);

    return await _db.Set<ExamSession>()
        .Include(s => s.Subject).Include(s => s.GradeLevel).Include(s => s.Assignments)
        .Where(s => s.Status == ExamSessionStatusEnum.Published)
        .Where(s => s.Assignments.Any(a =>
            (a.CohortId != null && cohortIds.Contains(a.CohortId.Value)) ||
            (a.CohortClassId != null && _db.Set<CohortClass>()
                .Any(cc => cc.Id == a.CohortClassId && cohortIds.Contains(cc.CohortId)))))
        .OrderByDescending(s => s.OpenAt)
        .ToListAsync(ct);
}
```

`IsStudentAssignedAsync` dùng cùng logic nhưng thêm điều kiện `s.Id == sessionId` và trả `AnyAsync`.

`CountSubmittedAttemptsAsync`:
```csharp
public Task<int> CountSubmittedAttemptsAsync(Guid sessionId, Guid studentId, CancellationToken ct)
    => _db.Set<ExamSubmission>().CountAsync(
        x => x.SessionId == sessionId && x.StudentId == studentId
             && (x.Status == SubmissionStatusEnum.Submitted || x.Status == SubmissionStatusEnum.Graded), ct);
```

`GetInProgressAsync`:
```csharp
public Task<ExamSubmission?> GetInProgressAsync(Guid sessionId, Guid studentId, CancellationToken ct)
    => _db.Set<ExamSubmission>().FirstOrDefaultAsync(
        x => x.SessionId == sessionId && x.StudentId == studentId
             && x.Status == SubmissionStatusEnum.InProgress, ct);
```

`AddExamsAsync` (bỏ trùng với pool hiện có):
```csharp
public async Task AddExamsAsync(Guid sessionId, IEnumerable<Guid> examIds, CancellationToken ct)
{
    var existing = await _db.Set<ExamSessionExam>()
        .Where(x => x.SessionId == sessionId).Select(x => x.ExamId).ToListAsync(ct);
    var toAdd = examIds.Distinct().Where(id => !existing.Contains(id))
        .Select(id => new ExamSessionExam { SessionId = sessionId, ExamId = id });
    _db.Set<ExamSessionExam>().AddRange(toAdd);
    await _db.SaveChangesAsync(ct);
}
```

Các phương thức còn lại theo pattern EF chuẩn (Add/Update/Delete qua `_db.Set<>()` + `SaveChangesAsync`; `GetPagedAsync` dùng `Include` + `Skip/Take` + `CountAsync`; `GetDetailAsync` `Include(s => s.Exams).ThenInclude(e => e.Exam)`).

> Constructor: `public class ExamSessionRepository(AppDbContext _db) : IExamSessionRepository`. Import các entity/enum cần (`CohortMember`, `CohortClass`, `SubmissionStatusEnum`, ...).

- [ ] **Step 3: Build Core** → 0 errors.

- [ ] **Step 4: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamSessionRepositories.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs
git commit -m "feat(api): add exam session repository"
```

---

## PHASE 3 — BACKEND SERVICE (logic bốc/khoá đề)

### Task 6: `ExamSessionService` — quản lý + start

**Files:**
- Create: `ExamHub.Core/Application/Services/IExamSessionService.cs`
- Create: `ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs`

**Interfaces:**
- Consumes: `IExamSessionRepository`, `IExamRepository` (kiểm tra đề published/đúng môn+lớp).
- Produces: `IExamSessionService` với:
  - CRUD + `PublishAsync`/`CloseAsync`
  - `SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken)` — validate mỗi đề published + cùng subject/grade với session; ném `InvalidOperationException` nếu vi phạm.
  - assignment add/remove.
  - `GetMySessionsAsync(Guid studentId, CancellationToken)` → `IReadOnlyList<MySessionResponse>`
  - `GetPoolForStudentAsync(Guid sessionId, Guid studentId, CancellationToken)` → `IReadOnlyList<SessionPoolItemResponse>`
  - `StartAsync(Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken)` → `StartSessionResponse`.

- [ ] **Step 1: Tạo interface** `IExamSessionService.cs` với các chữ ký trên (trả DTO ở Task 4).

- [ ] **Step 2: Implement `StartAsync`** — phần lõi (transaction + chống double-submit). Trong `ExamSessionService.cs`:

```csharp
public async Task<StartSessionResponse> StartAsync(
    Guid sessionId, Guid studentId, Guid? chosenExamId, string by, CancellationToken ct)
{
    var session = await _repo.GetByIdAsync(sessionId, ct)
        ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
    if (session.Status != ExamSessionStatusEnum.Published)
        throw new InvalidOperationException("Kỳ thi chưa mở.");
    var now = DateTime.UtcNow;
    if (now < session.OpenAt) throw new InvalidOperationException("Kỳ thi chưa đến giờ mở.");
    if (now > session.CloseAt) throw new InvalidOperationException("Kỳ thi đã đóng.");
    if (!await _repo.IsStudentAssignedAsync(sessionId, studentId, ct))
        throw new InvalidOperationException("Bạn không được giao kỳ thi này.");

    // Đang có lượt dở → trả lại đúng đề đó (Tiếp tục)
    var inProgress = await _repo.GetInProgressAsync(sessionId, studentId, ct);
    if (inProgress is not null)
        return new StartSessionResponse(inProgress.Id, inProgress.ExamId);

    var used = await _repo.CountSubmittedAttemptsAsync(sessionId, studentId, ct);
    if (used >= session.MaxAttempts)
        throw new InvalidOperationException("Bạn đã hết lượt làm bài.");

    // Xác định đề theo pick_mode
    var pool = await _repo.GetPoolExamsAsync(sessionId, ct);
    if (pool.Count == 0) throw new InvalidOperationException("Kỳ thi chưa có đề.");

    Guid examId;
    if (session.PickMode == ExamSessionPickModeEnum.StudentChoice)
    {
        if (chosenExamId is null) throw new InvalidOperationException("Vui lòng chọn đề.");
        if (pool.All(e => e.Id != chosenExamId.Value))
            throw new InvalidOperationException("Đề không thuộc kỳ thi.");
        // Chặn chọn lại đề đã hoàn thành
        var done = await _repo.GetStudentSubmissionsAsync(sessionId, studentId, ct);
        if (done.Any(s => s.ExamId == chosenExamId.Value && s.Status != SubmissionStatusEnum.InProgress))
            throw new InvalidOperationException("Bạn đã làm đề này rồi.");
        examId = chosenExamId.Value;
    }
    else
    {
        examId = pool[Random.Shared.Next(pool.Count)].Id;
    }

    var submission = new ExamSubmission
    {
        Id = Guid.NewGuid(), SessionId = sessionId, ExamId = examId, StudentId = studentId,
        Status = SubmissionStatusEnum.InProgress, AttemptNo = (short)(used + 1),
        StartedAt = now, CreatedBy = by
    };
    await _repo.CreateSubmissionAsync(submission, ct); // insert; unique index (session_id, student_id, in_progress) khuyến nghị
    return new StartSessionResponse(submission.Id, examId);
}
```

> Thêm phương thức `CreateSubmissionAsync(ExamSubmission, ct)` vào repository (insert đơn qua `_db.Set<ExamSubmission>().Add` + save) — bổ sung vào interface/impl Task 5 nếu chưa có.
> Chống double-click: đặt trong transaction (`await using var tx = _db.Database.BeginTransactionAsync`) HOẶC tạo **partial unique index** `exam_submissions (session_id, student_id) WHERE status='in_progress'` trong migration (khuyến nghị — thêm `migrationBuilder.Sql` ở Task 3 Step 5). Ghi rõ trong plan: nếu thêm index này, khi double-submit lần 2 sẽ lỗi unique → bắt và trả về lượt in_progress hiện có.

- [ ] **Step 3: Implement `GetMySessionsAsync` + `GetPoolForStudentAsync`** — dựng DTO từ repo:
  - `Availability`: `now < OpenAt` → `"upcoming"`; `now > CloseAt` → `"closed"`; ngược lại `"open"`.
  - `UsedAttempts` = `CountSubmittedAttemptsAsync`; `InProgress*` từ `GetInProgressAsync`.
  - Pool item `StudentState`: có submission in_progress đề đó → `"inProgress"`; có submission submitted/graded → `"completed"`; else `"notStarted"`.

- [ ] **Step 4: Implement `SetExamsAsync`** — validate published + đúng môn/lớp:

```csharp
public async Task SetExamsAsync(Guid sessionId, IReadOnlyList<Guid> examIds, string by, CancellationToken ct)
{
    var session = await _repo.GetByIdAsync(sessionId, ct)
        ?? throw new InvalidOperationException("Không tìm thấy kỳ thi.");
    foreach (var examId in examIds.Distinct())
    {
        var exam = await _examRepo.GetByIdAsync(examId, ct)
            ?? throw new InvalidOperationException($"Không tìm thấy đề {examId}.");
        if (exam.Status != ExamStatusEnum.Published)
            throw new InvalidOperationException($"Đề '{exam.Title}' chưa phát hành.");
        if (exam.SubjectId != session.SubjectId || exam.GradeLevelId != session.GradeLevelId)
            throw new InvalidOperationException($"Đề '{exam.Title}' không cùng môn/cấp lớp với kỳ thi.");
    }
    await _repo.AddExamsAsync(sessionId, examIds, ct);
}
```

- [ ] **Step 5: Implement phần CRUD/publish/close/assignment còn lại** — mỏng, gọi repo. `PublishAsync`: validate có ≥1 đề trong pool và ≥1 assignment và `CloseAt > now`, rồi `SetStatusAsync(Published)`.

- [ ] **Step 6: Build Core** → 0 errors.

- [ ] **Step 7: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/Application/Services/IExamSessionService.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamSessionRepositories.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs
git commit -m "feat(api): exam session service with random/choice pick-and-lock logic"
```

---

## PHASE 4 — SUBMIT FLOW ADAPTATION

### Task 7: Submit cập nhật submission in_progress khi thuộc kỳ thi

**Files:**
- Modify: `ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs`
- Modify: `ExamHub.Core/DataTransferObjects/Exam/…` (DTO submit — thêm `SubmissionId? Guid`)

**Interfaces:**
- Consumes: `ExamSubmissionRequest` (thêm optional `SubmissionId`).
- Produces: `SubmitAsync` hỗ trợ 2 nhánh: có `SubmissionId` (session) → cập nhật bản in_progress; không có → tạo mới (giữ nguyên).

- [ ] **Step 1: Đọc** `ExamSubmissionService.SubmitAsync` hiện tại và DTO `ExamSubmissionRequest` để biết chữ ký chính xác trước khi sửa.

- [ ] **Step 2: Thêm `Guid? SubmissionId`** vào `ExamSubmissionRequest`.

- [ ] **Step 3: Sửa `SubmitAsync`**: nếu `submission.Id` (từ request `SubmissionId`) khác `Guid.Empty` và tồn tại bản in_progress → nạp bản đó, gán answers, chấm trắc nghiệm tự động (tái dùng logic chấm hiện có), set `SubmittedAt`, `DurationSeconds`, `Status=Submitted`, `TotalScore`, và **UPDATE** thay vì INSERT. Nhánh cũ (không có SubmissionId) giữ nguyên tạo mới. Không đổi hành vi đề trực tiếp.

- [ ] **Step 4: Build Core** → 0 errors.

- [ ] **Step 5: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs exam_hub_api/ExamHub.Core/DataTransferObjects/Exam
git commit -m "feat(api): submit updates in-progress submission for exam sessions"
```

---

## PHASE 5 — BACKEND CONTROLLER + DI + MENU

### Task 8: `ExamSessionController` + đăng ký DI

**Files:**
- Create: `ExamHub.API/Controllers/Exam/ExamSessionController.cs`
- Modify: `ExamHub.Core/DependencyContainer.cs`

**Interfaces:**
- Consumes: `IExamSessionService`.
- Produces: REST endpoints tại `api/exam-sessions`.

- [ ] **Step 1: Đăng ký DI** — thêm vào `AddRepositories()`: `.AddScoped<IExamSessionRepository, ExamSessionRepository>()`; vào `AddAppServices()`: `.AddScoped<IExamSessionService, ExamSessionService>()`.

- [ ] **Step 2: Tạo controller** (theo mẫu `ExamSubmissionController`), các action:

```csharp
[ApiController]
[Route("api/exam-sessions")]
public class ExamSessionController(IExamSessionService service) : AuthorizeControllerBase
{
    // Quản lý
    [HttpGet, Authorize(Roles = "Admin,Teacher")]                          // list (query: page,pageSize,subjectId,gradeLevelId,status,keyword)
    [HttpGet("{id:guid}"), Authorize(Roles = "Admin,Teacher")]            // detail
    [HttpPost, Authorize(Roles = "Admin,Teacher")]                        // create (CreateExamSessionRequest)
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin,Teacher")]           // update
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin,Teacher")]        // delete
    [HttpPost("{id:guid}/exams"), Authorize(Roles = "Admin,Teacher")]    // SetSessionExamsRequest
    [HttpDelete("{id:guid}/exams/{examId:guid}"), Authorize(Roles = "Admin,Teacher")]
    [HttpPost("{id:guid}/assignments"), Authorize(Roles = "Admin,Teacher")] // CreateAssignmentRequest
    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}"), Authorize(Roles = "Admin,Teacher")]
    [HttpPost("{id:guid}/publish"), Authorize(Roles = "Admin,Teacher")]
    [HttpPost("{id:guid}/close"), Authorize(Roles = "Admin,Teacher")]

    // Học sinh
    [HttpGet("my")]                                                       // GetMySessionsAsync(CurrentUser.UserId)
    [HttpGet("{id:guid}/pool")]                                           // GetPoolForStudentAsync
    [HttpPost("{id:guid}/start")]                                         // StartSessionRequest → StartSessionResponse
}
```
Mỗi action bọc kết quả trong `RequestResponse<T>` như các controller khác; lấy `by = User.GetTag()`, `studentId = CurrentUser.UserId!.Value`. Bắt `InvalidOperationException` → trả `RequestResponse.Error` (theo cách xử lý lỗi hiện có; nếu có global filter thì để ném).

- [ ] **Step 3: Build API** (dừng app trước nếu bị khoá DLL) — `dotnet build ExamHub.API/ExamHub.API.csproj -nologo -clp:ErrorsOnly` → 0 errors.

- [ ] **Step 4: Kiểm thử endpoint thủ công** — chạy API, dùng token Teacher: tạo session → thêm đề (đề published cùng môn/lớp) → thêm assignment (cohort) → publish. Dùng token Student thuộc cohort đó: `GET /api/exam-sessions/my` thấy kỳ thi; `POST /api/exam-sessions/{id}/start` trả `{submissionId, examId}`; gọi lại `start` trả đúng submission cũ.

- [ ] **Step 5: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.API/Controllers/Exam/ExamSessionController.cs exam_hub_api/ExamHub.Core/DependencyContainer.cs
git commit -m "feat(api): exam session controller + DI registration"
```

---

### Task 9: Menu nhóm cha "Quản lý đề thi" (backend)

**Files:**
- Modify: `ExamHub.API/Controllers/Menu/MenuItemResponse.cs`
- Modify: `ExamHub.API/Controllers/Menu/MenuRegistry.cs`

**Interfaces:**
- Produces: `MenuItemResponse` có `Children`; registry trả nhóm cha chứa 4 mục con.

- [ ] **Step 1: Thêm `Children` vào `MenuItemResponse`:**

```csharp
public record MenuItemResponse(
    string Key, string Label, string? Path, string Icon, int Order,
    IReadOnlyList<MenuItemResponse>? Children = null);
```

- [ ] **Step 2: Sửa `MenuRegistry`** — mô hình nhóm cha. Thêm khái niệm parent qua `Group` key; xây cây trong `GetForRoles`:

```csharp
private record Item(string Key, string Label, string? Path, string Icon, int Order, string[] Roles, string? Group = null);

private static readonly Item[] Items =
[
    new("dashboard", "Tổng quan", "/app/dashboard", "dashboard", 1, ["Admin","Teacher","Student"]),
    new("questions", "Câu hỏi", "/app/questions", "question", 2, ["Admin","Teacher"]),
    // Nhóm cha
    new("exam-mgmt", "Quản lý đề thi", null, "template", 3, ["Admin","Teacher"]),
    new("exams",        "Mẫu đề thi",  "/app/exams",         "template", 1, ["Admin","Teacher"], Group: "exam-mgmt"),
    new("generate",     "Sinh đề thi", "/app/generate",      "generate", 2, ["Admin","Teacher"], Group: "exam-mgmt"),
    new("exam-list",    "Đề thi",      "/app/exam-list",     "exam",     3, ["Admin","Teacher"], Group: "exam-mgmt"),
    new("exam-sessions","Kỳ thi",      "/app/exam-sessions", "session",  4, ["Admin","Teacher"], Group: "exam-mgmt"),
    new("schools", "Quản lý trường", "/app/schools", "school", 6, ["Admin"]),
    new("users", "Người dùng", "/app/users", "user", 7, ["Admin"]),
    new("category", "Danh mục", "/app/category", "category", 8, ["Admin"]),
];
```
`GetForRoles`: lọc theo role; các item có `Group=null` là gốc; item có `Group` gộp làm `Children` của item cha (sắp theo `Order`). Nhóm cha chỉ hiện nếu có ≥1 con hợp lệ.

- [ ] **Step 3: Build API** → 0 errors. Chạy `GET /api/menu` với token Teacher → thấy item `exam-mgmt` có `children` gồm 4 mục.

- [ ] **Step 4: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_api/ExamHub.API/Controllers/Menu
git commit -m "feat(api): group exam menu items under 'Quản lý đề thi' parent"
```

---

## PHASE 6 — FRONTEND: MENU + TYPES + SERVICE

### Task 10: Sidebar render nhóm cha thu/mở + MenuItem type

**Files:**
- Modify: `src/types/*` (nơi khai báo `MenuItem` — tìm bằng grep `interface MenuItem`)
- Modify: `src/layouts/AppLayout.tsx`
- Modify: `src/routes/paths.ts`

**Interfaces:**
- Consumes: `/api/menu` (item có `children`).
- Produces: sidebar hỗ trợ nhóm con.

- [ ] **Step 1: Tìm & sửa type `MenuItem`** — thêm `path?: string` (cho nhóm cha) và `children?: MenuItem[]`.

Run: `grep -rn "interface MenuItem" src/`

- [ ] **Step 2: Thêm route path** vào `paths.ts`: `EXAM_SESSIONS: '/app/exam-sessions'`, `EXAM_SESSIONS_CREATE: '/app/exam-sessions/create'`.

- [ ] **Step 3: Sửa `AppLayout.tsx`** — render đệ quy: item có `children` → hiện nút cha bấm để thu/mở (state `openGroups`), con thụt lề; item active theo `location.pathname.startsWith(child.path)`; nhóm cha auto-mở nếu có con đang active. Thêm icon `session` vào `ICON_MAP` (VD `<ScheduleOutlined/>`). Cập nhật `FALLBACK_NAV` khớp cây mới (nhóm "Quản lý đề thi" chứa 4 mục).

- [ ] **Step 4: Verify** — `npx tsc --noEmit` (0 lỗi); chạy web, đăng nhập Teacher → sidebar hiện nhóm "Quản lý đề thi" thu/mở, có mục "Kỳ thi".

- [ ] **Step 5: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_web/src/layouts/AppLayout.tsx exam_hub_web/src/routes/paths.ts exam_hub_web/src/types
git commit -m "feat(web): collapsible sidebar groups; exam menu group"
```

---

### Task 11: Types + service + query hooks cho Exam Session

**Files:**
- Create: `src/types/examSession.d.ts`
- Create: `src/services/examSessionService.ts`
- Create: `src/hooks/queries/useExamSessions.ts`

**Interfaces:**
- Produces: types `ExamSession`, `ExamSessionDetail`, `ExamSessionBody`, `MySession`, `SessionPoolItem`, `StartSessionResult`; `examSessionService`; hooks `useExamSessionsQuery`, `useExamSessionQuery`, `useMySessionsQuery`, `useSessionPoolQuery`, mutations create/update/remove/setExams/addAssignment/removeAssignment/publish/close/start.

- [ ] **Step 1: Tạo `examSession.d.ts`** — mirror DTO Task 4 (dùng `number` cho timestamp ms, `pickMode: 'Random'|'StudentChoice'`, `status: 'draft'|'published'|'closed'`).

- [ ] **Step 2: Tạo `examSessionService.ts`** (theo mẫu `examTemplateService.ts`, `basePath='exam-sessions'`) với các method: `list(query)`, `getDetail(id)`, `create(body)`, `update(id,body)`, `remove(id)`, `setExams(id, examIds)`, `removeExam(id, examId)`, `addAssignment(id, body)`, `removeAssignment(id, assignmentId)`, `publish(id)`, `close(id)`, `getMy()`, `getPool(id)`, `start(id, examId?)`.

- [ ] **Step 3: Tạo `useExamSessions.ts`** (theo mẫu `useExamTemplates.ts`) — query keys + hooks + invalidation.

- [ ] **Step 4: Verify** — `npx tsc --noEmit` → 0 lỗi.

- [ ] **Step 5: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_web/src/types/examSession.d.ts exam_hub_web/src/services/examSessionService.ts exam_hub_web/src/hooks/queries/useExamSessions.ts
git commit -m "feat(web): exam session types, service, query hooks"
```

---

## PHASE 7 — FRONTEND: TRANG QUẢN LÝ

### Task 12: `ExamSessionListPage` + route

**Files:**
- Create: `src/pages/exams/ExamSessionListPage.tsx`
- Modify: `src/routes/index.tsx`

**Interfaces:**
- Consumes: `useExamSessionsQuery`, mutations publish/close/remove.

- [ ] **Step 1: Tạo trang danh sách** (theo mẫu `ExamTemplatePage.tsx`): bảng cột Tiêu đề, Môn, Cấp lớp, Khung giờ (open→close), Số đề, Số lớp/khoá, Trạng thái (Tag), Thao tác (Sửa/Publish/Close/Xoá). Nút "Tạo kỳ thi" → `/app/exam-sessions/create`. Lọc theo môn/cấp lớp/trạng thái/keyword.

- [ ] **Step 2: Thêm route** vào `src/routes/index.tsx`:
```tsx
{ path: 'exam-sessions',            element: <ExamSessionListPage /> },
{ path: 'exam-sessions/create',     element: <ExamSessionEditPage /> },
{ path: 'exam-sessions/:id/edit',   element: <ExamSessionEditPage /> },
```
(import 2 page; `ExamSessionEditPage` tạo ở Task 13 — có thể tạm import sau khi Task 13 xong, hoặc tạo file rỗng trước.)

- [ ] **Step 3: Verify** — `npx tsc --noEmit` → 0 lỗi; điều hướng `/app/exam-sessions` hiển thị danh sách.

- [ ] **Step 4: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_web/src/pages/exams/ExamSessionListPage.tsx exam_hub_web/src/routes/index.tsx
git commit -m "feat(web): exam session list page + routes"
```

---

### Task 13: `ExamSessionEditPage` (cấu hình + chọn đề + giao lớp/khoá + publish)

**Files:**
- Create: `src/pages/exams/ExamSessionEditPage.tsx`

**Interfaces:**
- Consumes: `useExamSessionQuery`, create/update/setExams/removeExam/addAssignment/removeAssignment/publish; `useExamsQuery` (đề published cùng môn+lớp), `useSubjectsQuery`, `useGradeLevelsListQuery`, cohort/cohort-class hooks.

- [ ] **Step 1: Form cấu hình** — title, description, môn, cấp lớp, open_at/close_at (`DatePicker showTime`), max_attempts (`InputNumber`), pick_mode (`Select`: Ngẫu nhiên / Học sinh tự chọn). Lưu bằng create/update.

- [ ] **Step 2: Khối "Đề trong kỳ thi"** (chỉ bật sau khi đã lưu/tạo, cần sessionId) — bảng đề đã chọn + nút "Thêm đề" mở modal chọn từ `useExamsQuery({status:'Published', subjectId, gradeLevelId})`; gọi `setExams`/`removeExam`.

- [ ] **Step 3: Khối "Giao cho"** — chọn cohort hoặc cohort_class (2 Select nguồn từ hooks trường học); `addAssignment`/`removeAssignment`; hiển thị danh sách assignment hiện có.

- [ ] **Step 4: Nút Publish** — gọi `publish`, báo lỗi nếu BE trả lỗi (thiếu đề/assignment).

- [ ] **Step 5: Verify** — `npx tsc --noEmit` → 0 lỗi; tạo 1 kỳ thi hoàn chỉnh end-to-end trên UI.

- [ ] **Step 6: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx
git commit -m "feat(web): exam session edit page (config, pool, assignments, publish)"
```

---

## PHASE 8 — FRONTEND: PHÍA HỌC SINH

### Task 14: `StudentSessionListPage` + `StudentSessionPoolPage` + đổi entry

**Files:**
- Create: `src/pages/student/StudentSessionListPage.tsx`
- Create: `src/pages/student/StudentSessionPoolPage.tsx`
- Modify: `src/routes/index.tsx` (đổi `ROUTES.STUDENT_EXAMS` → `StudentSessionListPage`; thêm route pool)

**Interfaces:**
- Consumes: `useMySessionsQuery`, `useSessionPoolQuery`, `start` mutation.

- [ ] **Step 1: `StudentSessionListPage`** = "Kỳ thi của tôi": card/bảng kỳ thi được giao (`getMy`), hiện tên/môn/khung giờ/trạng thái (`upcoming/open/closed`)/lượt còn lại. Nút theo trạng thái:
  - có `inProgressSubmissionId` → "Tiếp tục" → `start(id)` → navigate `/student/exam?examId=<examId>&sessionId=<id>&submissionId=<sid>`.
  - `pickMode='Random'` & còn lượt & đang mở → "Vào thi" → `start(id)` → navigate như trên.
  - `pickMode='StudentChoice'` & còn lượt & đang mở → "Chọn đề" → navigate `/student/session/:id/pool`.

- [ ] **Step 2: `StudentSessionPoolPage`** — `getPool(id)`: danh sách đề, đề `completed` đánh dấu (disable), `inProgress` → nút Tiếp tục; chọn đề `notStarted` → `start(id, examId)` → navigate vào làm bài.

- [ ] **Step 3: Sửa route** — `ROUTES.STUDENT_EXAMS` element = `StudentSessionListPage`; thêm `{ path: '/student/session/:id/pool', element: <StudentSessionPoolPage /> }` trong `StudentLayout`. **Giữ file** `StudentExamListPage.tsx` (không xoá, không route tới nữa).

- [ ] **Step 4: Verify** — `npx tsc --noEmit` → 0 lỗi; đăng nhập Student → thấy kỳ thi được giao.

- [ ] **Step 5: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_web/src/pages/student/StudentSessionListPage.tsx exam_hub_web/src/pages/student/StudentSessionPoolPage.tsx exam_hub_web/src/routes/index.tsx
git commit -m "feat(web): student exam session pages; switch entry to sessions"
```

---

### Task 15: Luồng làm bài mang theo session/submission

**Files:**
- Modify: `src/pages/student/ExamCoverPage.tsx`
- Modify: `src/pages/student/ExamTakingPage.tsx`
- Modify: submit hook/`submissionService.ts` (truyền `submissionId`, `sessionId`)

**Interfaces:**
- Consumes: query param `examId`, `sessionId?`, `submissionId?`.
- Produces: submit gửi kèm `submissionId` để BE cập nhật bản in_progress (Task 7).

- [ ] **Step 1: `ExamCoverPage`** — đọc thêm `sessionId`/`submissionId` từ query; nút "Bắt đầu" chuyển query sang `/student/exam/take` giữ nguyên các param.

- [ ] **Step 2: `ExamTakingPage`** — đọc `submissionId`/`sessionId`; khi submit, đưa vào body (`ExamSubmissionBody.submissionId`, `sessionId`). Cập nhật type `ExamSubmissionBody` thêm 2 field optional.

- [ ] **Step 3: `submissionService`/type** — cho phép body có `submissionId?`, `sessionId?`.

- [ ] **Step 4: Verify E2E thủ công** — Student `Random`: Vào thi → làm → nộp; kiểm tra `exam_submissions` có `session_id`, `attempt_no`, `status=submitted`, đúng bản (không tạo trùng). Vào lại khi đang làm → đúng đề. Hết `max_attempts` → báo hết lượt. `StudentChoice`: chọn đề → làm → nộp → quay lại chọn đề khác.

- [ ] **Step 5: `npx tsc --noEmit`** → 0 lỗi.

- [ ] **Step 6: Commit**

```bash
cd /d/My-Project/ExamHub && git add exam_hub_web/src/pages/student/ExamCoverPage.tsx exam_hub_web/src/pages/student/ExamTakingPage.tsx exam_hub_web/src/services/submissionService.ts exam_hub_web/src/types
git commit -m "feat(web): carry session/submission through exam-taking + submit"
```

---

## SELF-REVIEW NOTES (đã kiểm)

- **Spec coverage:** data model (Task 1–3), pick_mode random/student_choice (Task 6), giao lớp/khoá + resolve qua cohort_members (Task 5 `GetAssignedToStudentAsync`), pool chỉ published cùng môn/lớp (Task 6 `SetExamsAsync`), khung giờ mở/đóng (Task 6 `StartAsync`), max_attempts (Task 6), submit adaptation (Task 7), menu nhóm (Task 9–10), trang quản lý (Task 12–13), phía HS (Task 14–15), giữ file cũ (Task 14). ✔
- **Lưu ý thực thi:** cần dừng app khi chạy `dotnet ef`/build API (khoá DLL). Cân nhắc **partial unique index** cho in_progress để chống double-start (Task 6 Step 2).
- **Type consistency:** `pick_mode` lưu PascalCase ('Random'/'StudentChoice') xuyên suốt entity/DTO/schema/web; `status` kỳ thi lowercase ('draft'/'published'/'closed').
