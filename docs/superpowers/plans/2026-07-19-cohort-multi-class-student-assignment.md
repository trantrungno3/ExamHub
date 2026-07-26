# Multi-Class Per Cohort + Student Class Assignment — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cho phép một khoá (Cohort) có nhiều lớp song song (A, B, C…) và gán mỗi học sinh vào một lớp cụ thể, ổn định suốt khoá.

**Architecture:** Thêm `section` (ban/lớp) làm thuộc tính chuỗi ổn định. `cohorts.num_classes` thay `class_suffix`; DB trigger sinh `num_classes × số_năm` lớp, mỗi lớp có `section` A.. `cohort_members.section` (nullable) gắn học sinh vào lớp; validate ở tầng service. Exam session resolve học sinh cấp lớp theo section khớp.

**Tech Stack:** .NET (EF Core, PostgreSQL, controller/service/repository), React 19 + Ant Design 6 + TanStack Query + TypeScript.

## 🔖 Trạng thái tiến độ (cập nhật 2026-07-26)

- ✅ Brainstorming xong, spec đã duyệt: `docs/superpowers/specs/2026-07-19-cohort-multi-class-student-assignment-design.md`
- ✅ Plan này đã viết xong & commit.
- ✅ **Task 1–10 ĐÃ CODE + COMMIT** trên nhánh `feat/exam-session`. Backend `dotnet build` (ExamHub.Core) 0 lỗi; frontend `tsc`/`lint` sạch trên các file thuộc feature (các lỗi build/lint còn lại là PRE-EXISTING ở file không liên quan: RichTextEditor.tsx, UserPage.tsx, routes/index.tsx…).
- ⏭️ **Còn lại: Task 11 — kiểm thử thủ công E2E** (dựng lại DB dev bằng `database_schema.sql` mới, chạy API+web, đăng nhập Admin). Chưa chạy vì cần hệ thống thật + đăng nhập.
- Cách thực thi đề xuất: subagent-driven-development (mỗi task 1 subagent + review giữa các task), hoặc executing-plans (inline theo lô).
- Nhánh git hiện tại: `feat/exam-session`. Repo root: `D:/My-Project/ExamHub`.
- Lưu ý: repo KHÔNG có test tự động → verify bằng `dotnet build exam_hub_api/ExamHub.API.slnx` và `pnpm -C exam_hub_web build` + `pnpm -C exam_hub_web lint`.

## Global Constraints

- Backend persistence dùng **EF Core** (`BaseRepository.AddAsync` = `Set.AddAsync`+`SaveChangesAsync`); cột mới map qua `[Column("…")]` trên entity + (tùy chọn) Fluent trong `AppDbContext`.
- Lỗi nghiệp vụ/validation: **`throw new InvalidOperationException("thông điệp tiếng Việt")`** (pattern hiện có trong `ExamSessionService`).
- DB tạo từ `exam_hub_api/database_schema.sql` (KHÔNG có EF migrations). Mọi thay đổi cột phải sửa file SQL này + kèm đoạn `ALTER TABLE` để nâng cấp DB dev.
- `section` chuẩn hoá: `null` nếu rỗng, ngược lại `Trim().ToUpperInvariant()`. Dải hợp lệ của khoá: `A` … `chr('A' + num_classes - 1)`, `num_classes ∈ [1,26]`.
- **Không có test tự động trong repo.** Verification mỗi task = biên dịch/lint sạch:
  - Backend: `dotnet build exam_hub_api/ExamHub.API.slnx` (chạy từ repo root `D:/My-Project/ExamHub`).
  - Frontend: `pnpm -C exam_hub_web build` (tsc + vite) và `pnpm -C exam_hub_web lint`.
- Đường dẫn tương đối tính từ repo root `D:/My-Project/ExamHub`.

---

## Task 1: DB schema, trigger & seed

**Files:**
- Modify: `exam_hub_api/database_schema.sql` (cohorts ~179–196, cohort_classes ~201–216, cohort_members ~221–234, trigger fn ~260–294, seed ~713–723)

**Interfaces:**
- Produces (cho DB & mọi task sau): cột `cohorts.num_classes SMALLINT`, `cohort_classes.section VARCHAR(10)`, `cohort_members.section VARCHAR(10) NULL`; trigger sinh lớp theo `num_classes`.

- [ ] **Step 1: `cohorts` — thay `class_suffix` bằng `num_classes`**

Trong `CREATE TABLE public.cohorts`, đổi dòng:
```sql
    class_suffix VARCHAR(10)  NOT NULL DEFAULT 'A', -- Hậu tố: "A" → 1A, 2A, 3A, ...
```
thành:
```sql
    num_classes  SMALLINT     NOT NULL DEFAULT 1,   -- Số lớp song song → A, B, C, ...
```
và thêm constraint (ngay sau `CONSTRAINT chk_cohort_years ...`):
```sql
    CONSTRAINT chk_cohort_num_classes CHECK (num_classes BETWEEN 1 AND 26),
```

- [ ] **Step 2: `cohort_classes` — thêm `section`, đổi UNIQUE**

Thêm cột (ngay sau dòng `class_name ...`):
```sql
    section             VARCHAR(10) NOT NULL DEFAULT 'A', -- Ban/lớp: A, B, C, ...
```
Đổi:
```sql
    UNIQUE (cohort_id, year_index)
```
thành:
```sql
    UNIQUE (cohort_id, year_index, section)
```

- [ ] **Step 3: `cohort_members` — thêm `section` nullable**

Thêm cột (ngay sau dòng `student_id ...`):
```sql
    section    VARCHAR(10),                      -- Lớp của HS (A, B, ...); NULL = chưa xếp lớp
```

- [ ] **Step 4: Viết lại hàm `generate_cohort_classes` (vòng lặp lồng năm × section)**

Thay toàn bộ thân hàm bằng:
```sql
CREATE
OR REPLACE FUNCTION public.generate_cohort_classes(p_cohort_id INT)
RETURNS VOID AS $$
DECLARE
    v_cohort   public.cohorts%ROWTYPE;
    v_duration SMALLINT;
    i          SMALLINT;
    j          SMALLINT;
    v_section  VARCHAR(10);
BEGIN
    SELECT * INTO v_cohort FROM public.cohorts WHERE id = p_cohort_id;
    v_duration := v_cohort.end_year - v_cohort.start_year;

    FOR i IN 1..v_duration LOOP
        FOR j IN 1..v_cohort.num_classes LOOP
            v_section := chr(64 + j);   -- 1→A, 2→B, 3→C, ...
            INSERT INTO public.cohort_classes (
                cohort_id, grade_level_id, class_name, school_year, year_index, section
            ) VALUES (
                p_cohort_id,
                v_cohort.grade_start + i - 1,
                (v_cohort.grade_start + i - 1)::TEXT || v_section,
                (v_cohort.start_year + i - 1)::TEXT || '-' || (v_cohort.start_year + i)::TEXT,
                i,
                v_section
            );
        END LOOP;
    END LOOP;
END;
$$
LANGUAGE plpgsql;
```

- [ ] **Step 5: Cập nhật seed (3 INSERT cohorts)**

Đổi 3 câu INSERT (dùng `class_suffix`/`'A'`) sang `num_classes`. Ví dụ khoá THPT cho 2 lớp để test:
```sql
INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, num_classes)
VALUES (1, 'Khoá 2020-2025', 2020, 2025, 1, 1);

INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, num_classes)
VALUES (1, 'Khoá 2021-2026', 2021, 2026, 1, 1);

-- Trường THPT: Khoá 2021-2024 (lớp 10→12), 3 lớp A/B/C
INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, num_classes)
VALUES (3, 'Khoá 2021-2024', 2021, 2024, 10, 3);
```

- [ ] **Step 6: Đoạn ALTER cho DB dev đang chạy**

Thêm khối này vào cuối `database_schema.sql` trong một comment block "MIGRATION — chạy tay trên DB dev đã tồn tại" (để không phá schema gốc; ai cần nâng cấp thì chạy tay):
```sql
-- ============================================================
-- MIGRATION [2026-07-19]: multi-class per cohort + student section
-- Chạy TAY trên DB dev đã tồn tại (schema gốc phía trên đã có sẵn cột mới).
-- ============================================================
-- ALTER TABLE public.cohorts ADD COLUMN num_classes SMALLINT NOT NULL DEFAULT 1;
-- ALTER TABLE public.cohorts ADD CONSTRAINT chk_cohort_num_classes CHECK (num_classes BETWEEN 1 AND 26);
-- ALTER TABLE public.cohorts DROP COLUMN class_suffix;
-- ALTER TABLE public.cohort_classes ADD COLUMN section VARCHAR(10) NOT NULL DEFAULT 'A';
-- ALTER TABLE public.cohort_classes DROP CONSTRAINT cohort_classes_cohort_id_year_index_key;
-- ALTER TABLE public.cohort_classes ADD UNIQUE (cohort_id, year_index, section);
-- ALTER TABLE public.cohort_members ADD COLUMN section VARCHAR(10);
-- (sau đó CREATE OR REPLACE FUNCTION generate_cohort_classes bản mới ở trên)
```

- [ ] **Step 7: Verify — SQL hợp lệ về mặt cú pháp (rà soát thủ công)**

Không có DB trong pipeline plan; rà lại: không còn chuỗi `class_suffix` nào trong file (chỉ còn trong comment migration). Grep xác nhận.
Run: `grep -n "class_suffix" exam_hub_api/database_schema.sql`
Expected: chỉ xuất hiện trong khối comment MIGRATION (dòng bị `--`).

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/database_schema.sql
git commit -m "feat(db): num_classes per cohort + section on classes/members"
```

---

## Task 2: Backend — `Cohort` đổi `ClassSuffix` → `NumClasses`

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/FieldTables/CohortTable.cs:26-27`
- Modify: `exam_hub_api/ExamHub.Core/Domain/Entities/Cohort.cs:48-51,77,86`
- Modify: `exam_hub_api/ExamHub.Core/DataTransferObjects/School/CohortDto.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs:362`

**Interfaces:**
- Produces: `Cohort.NumClasses` (short); `CohortRequest(… short NumClasses = 1 …)`; `CohortResponse(… short NumClasses …)`.

- [ ] **Step 1: `CohortTable` — đổi hằng cột**

Đổi:
```csharp
    /// <summary>Hậu tố lớp: A → 1A, 2A</summary>
    public const string ClassSuffix = "class_suffix";
```
thành:
```csharp
    /// <summary>Số lớp song song: 1 → A, 2 → A,B, ...</summary>
    public const string NumClasses = "num_classes";
```

- [ ] **Step 2: `Cohort` entity — property + insert/update objects**

Đổi khối property `ClassSuffix`:
```csharp
    /// <summary>Hậu tố tên lớp (VD: "A" → 1A, 2A, 3A)</summary>
    [Column(CohortTable.ClassSuffix)]
    [SqlBuilderProperty(CohortTable.ClassSuffix, Insert = true, Update = true)]
    public string ClassSuffix { get; set; } = "A";
```
thành:
```csharp
    /// <summary>Số lớp song song trong khoá (1..26 → A, B, C, ...)</summary>
    [Column(CohortTable.NumClasses)]
    [SqlBuilderProperty(CohortTable.NumClasses, Insert = true, Update = true)]
    public short NumClasses { get; set; } = 1;
```
Trong `ToInsertObject()` đổi `class_suffix = ClassSuffix,` → `num_classes  = NumClasses,`.
Trong `ToUpdateObject()` đổi `class_suffix = ClassSuffix,` → `num_classes  = NumClasses,`.

- [ ] **Step 3: `CohortDto` — request + response**

Trong `CohortRequest`: đổi tham số `string ClassSuffix = "A"` → `short NumClasses = 1`; trong `ToEntity()` đổi `ClassSuffix = ClassSuffix,` → `NumClasses = NumClasses,`.
Trong `CohortResponse`: đổi field `string ClassSuffix` → `short NumClasses`; trong `FromEntity` đổi `e.ClassSuffix` → `e.NumClasses`.

- [ ] **Step 4: `AppDbContext` — Fluent property**

Đổi:
```csharp
            e.Property(x => x.ClassSuffix).HasMaxLength(10).HasDefaultValue("A");
```
thành:
```csharp
            e.Property(x => x.NumClasses).HasDefaultValue((short)1);
```

- [ ] **Step 5: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API.slnx`
Expected: Build succeeded, 0 lỗi (không còn tham chiếu `ClassSuffix`).

- [ ] **Step 6: Commit**

```bash
git add exam_hub_api/ExamHub.Core/FieldTables/CohortTable.cs exam_hub_api/ExamHub.Core/Domain/Entities/Cohort.cs exam_hub_api/ExamHub.Core/DataTransferObjects/School/CohortDto.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs
git commit -m "feat(api): replace Cohort.ClassSuffix with NumClasses"
```

---

## Task 3: Backend — `CohortClass` thêm `Section`

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/FieldTables/CohortClassTable.cs`
- Modify: `exam_hub_api/ExamHub.Core/Domain/Entities/CohortClass.cs`
- Modify: `exam_hub_api/ExamHub.Core/DataTransferObjects/School/CohortClassDto.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs:381,387`

**Interfaces:**
- Consumes: `CohortClassTable` (Task nội bộ).
- Produces: `CohortClass.Section` (string); `CohortClassResponse(… string Section …)`.

- [ ] **Step 1: `CohortClassTable` — thêm hằng cột**

Thêm sau `ClassName`:
```csharp
    /// <summary>Ban/lớp: A, B, C, ...</summary>
    public const string Section = "section";
```

- [ ] **Step 2: `CohortClass` entity — property + insert object**

Thêm property (sau khối `ClassName`):
```csharp
    /// <summary>Ban/lớp song song: A, B, C, ...</summary>
    [Column(CohortClassTable.Section)]
    [SqlBuilderProperty(CohortClassTable.Section, Insert = true, Update = false)]
    public string Section { get; set; } = "A";
```
Trong `ToInsertObject()` thêm dòng `section = Section,` (sau `class_name = ClassName,`).

- [ ] **Step 3: `CohortClassResponse` — thêm field**

Đổi record thành (thêm `string Section` sau `ClassName`):
```csharp
public record CohortClassResponse(
    int Id,
    int CohortId,
    int GradeLevelId,
    string ClassName,
    string Section,
    string SchoolYear,
    short YearIndex,
    Guid? HomeroomTeacherId,
    long Created
)
{
    public static CohortClassResponse FromEntity(CohortClass e) =>
        new(e.Id, e.CohortId, e.GradeLevelId, e.ClassName, e.Section, e.SchoolYear, e.YearIndex, e.HomeroomTeacherId,
            e.Created.ToTimestamp());
}
```

- [ ] **Step 4: `AppDbContext` — property + đổi unique index**

Trong khối `Entity<CohortClass>`, thêm sau dòng `SchoolYear`:
```csharp
            e.Property(x => x.Section).HasMaxLength(10).IsRequired();
```
Đổi:
```csharp
            e.HasIndex(x => new { x.CohortId, x.YearIndex }).IsUnique();
```
thành:
```csharp
            e.HasIndex(x => new { x.CohortId, x.YearIndex, x.Section }).IsUnique();
```

- [ ] **Step 5: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API.slnx`
Expected: Build succeeded, 0 lỗi.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_api/ExamHub.Core/FieldTables/CohortClassTable.cs exam_hub_api/ExamHub.Core/Domain/Entities/CohortClass.cs exam_hub_api/ExamHub.Core/DataTransferObjects/School/CohortClassDto.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs
git commit -m "feat(api): add Section to CohortClass"
```

---

## Task 4: Backend — `CohortMember` thêm `Section` (entity, DTO, repo)

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/FieldTables/CohortMemberTable.cs`
- Modify: `exam_hub_api/ExamHub.Core/Domain/Entities/CohortMember.cs`
- Modify: `exam_hub_api/ExamHub.Core/DataTransferObjects/School/CohortMemberDto.cs`
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberRepository.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/CohortMemberRepository.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs:399-415`

**Interfaces:**
- Produces (cho Task 5): `CohortMember.Section` (string?); `ICohortMemberRepository.SetSectionAsync(Guid id, string? section, CancellationToken) → Task<bool>`; `CohortMemberRequest(… string? Section = null …)`; `CohortMemberResponse(… string? Section …)`.

- [ ] **Step 1: `CohortMemberTable` — thêm hằng cột**

Thêm sau `StudentId`:
```csharp
    /// <summary>Ban/lớp của học sinh: A, B, ...; NULL = chưa xếp lớp</summary>
    public const string Section = "section";
```

- [ ] **Step 2: `CohortMember` entity — property + insert/update objects**

Thêm property (sau khối `StudentId`):
```csharp
    /// <summary>Ban/lớp của học sinh trong khoá (A, B, ...); NULL = chưa xếp lớp</summary>
    [Column(CohortMemberTable.Section)]
    [SqlBuilderProperty(CohortMemberTable.Section, Insert = true, Update = true)]
    public string? Section { get; set; }
```
Trong `ToInsertObject()` thêm `section = Section,` (sau `student_id = StudentId,`).
Trong `ToUpdateObject()` thêm `section = Section,` (sau `id = Id,`).

- [ ] **Step 3: `CohortMemberDto` — request + response**

Đổi `CohortMemberRequest` thành:
```csharp
public record CohortMemberRequest(
    int CohortId,
    Guid StudentId,
    string? Section = null,
    long? JoinedAt = null,
    bool IsActive = true
)
{
    public CohortMember ToEntity() => new()
    {
        CohortId  = CohortId,
        StudentId = StudentId,
        Section   = Section,
        JoinedAt  = JoinedAt.HasValue
            ? DateOnly.FromDateTime(JoinedAt.Value.ToDateTime())
            : DateOnly.FromDateTime(DateTime.UtcNow),
        IsActive  = IsActive
    };
}
```
Đổi `CohortMemberResponse` thành (thêm `string? Section`):
```csharp
public record CohortMemberResponse(
    Guid Id,
    int CohortId,
    Guid StudentId,
    string? Section,
    long JoinedAt,
    bool IsActive
)
{
    public static CohortMemberResponse FromEntity(CohortMember e) =>
        new(e.Id, e.CohortId, e.StudentId, e.Section,
            e.JoinedAt.ToDateTime(TimeOnly.MinValue).ToTimestamp(),
            e.IsActive);
}
```

- [ ] **Step 4: `ICohortMemberRepository` — thêm SetSectionAsync**

Thêm vào interface (sau `SetActiveAsync`):
```csharp
    /// <summary>Đổi lớp (section) của học sinh trong khoá</summary>
    Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default);
```

- [ ] **Step 5: `CohortMemberRepository` — cài SetSectionAsync**

Thêm method (sau `SetActiveAsync`):
```csharp
    public async Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
        => await Set
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Section, section), ct) > 0;
```

- [ ] **Step 6: `AppDbContext` — Fluent property**

Trong khối `Entity<CohortMember>`, thêm (sau `e.Property(x => x.IsActive)...`):
```csharp
            e.Property(x => x.Section).HasMaxLength(10);
```

- [ ] **Step 7: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API.slnx`
Expected: Build succeeded, 0 lỗi.

- [ ] **Step 8: Commit**

```bash
git add exam_hub_api/ExamHub.Core/FieldTables/CohortMemberTable.cs exam_hub_api/ExamHub.Core/Domain/Entities/CohortMember.cs exam_hub_api/ExamHub.Core/DataTransferObjects/School/CohortMemberDto.cs exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberRepository.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/CohortMemberRepository.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs
git commit -m "feat(api): add Section to CohortMember + SetSectionAsync repo"
```

---

## Task 5: Backend — Service validate section + endpoint đổi lớp

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberService.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs`
- Modify: `exam_hub_api/ExamHub.API/Controllers/School/CohortMemberController.cs`

**Interfaces:**
- Consumes: `ICohortMemberRepository.SetSectionAsync` (Task 4), `ICohortRepository.GetByIdAsync` (base repo → `Cohort?`), `Cohort.NumClasses` (Task 2).
- Produces: `ICohortMemberService.SetSectionAsync(Guid, string?, CancellationToken) → Task<bool>`; endpoint `PATCH /api/cohortmember/{id:guid}/section`.

- [ ] **Step 1: `ICohortMemberService` — thêm SetSectionAsync**

Thêm (sau `SetActiveAsync`):
```csharp
    /// <summary>Đổi lớp (section) của học sinh; validate thuộc dải lớp của khoá</summary>
    Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default);
```

- [ ] **Step 2: `CohortMemberService` — inject ICohortRepository, validate + normalize + SetSection**

Thay toàn bộ class thành:
```csharp
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Interfaces;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai service cho CohortMember</summary>
public class CohortMemberService : ICohortMemberService
{
    private readonly ICohortMemberRepository _repo;
    private readonly ICohortRepository _cohortRepo;

    public CohortMemberService(ICohortMemberRepository repo, ICohortRepository cohortRepo)
    {
        _repo = repo;
        _cohortRepo = cohortRepo;
    }

    public Task<IReadOnlyList<CohortMember>> GetByCohortAsync(int cohortId, CancellationToken ct = default)
        => _repo.GetByCohortAsync(cohortId, ct);

    public Task<IReadOnlyList<CohortMember>> GetByStudentAsync(Guid studentId, CancellationToken ct = default)
        => _repo.GetByStudentAsync(studentId, ct);

    public Task<CohortMember?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _repo.GetByIdAsync(id, ct);

    public async Task<CohortMember> AddStudentAsync(CohortMember entity, CancellationToken ct = default)
    {
        entity.Section = NormalizeSection(entity.Section);
        await ValidateSectionAsync(entity.CohortId, entity.Section, ct);
        entity.Id       = Guid.NewGuid();
        entity.JoinedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        entity.Created  = DateTime.UtcNow;
        entity.Modified = DateTime.UtcNow;
        return await _repo.AddAsync(entity, ct);
    }

    public Task RemoveStudentAsync(Guid id, CancellationToken ct = default)
        => _repo.DeleteByIdAsync(id, ct);

    public Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken ct = default)
        => _repo.SetActiveAsync(id, isActive, ct);

    public async Task<bool> SetSectionAsync(Guid id, string? section, CancellationToken ct = default)
    {
        var member = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException("Không tìm thấy học sinh trong khoá.");
        section = NormalizeSection(section);
        await ValidateSectionAsync(member.CohortId, section, ct);
        return await _repo.SetSectionAsync(id, section, ct);
    }

    // ── Helpers ─────────────────────────────────────────────────
    private static string? NormalizeSection(string? section)
        => string.IsNullOrWhiteSpace(section) ? null : section.Trim().ToUpperInvariant();

    private async Task ValidateSectionAsync(int cohortId, string? section, CancellationToken ct)
    {
        if (section is null) return; // chưa xếp lớp — hợp lệ
        var cohort = await _cohortRepo.GetByIdAsync(cohortId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy khoá học.");
        var allowed = Enumerable.Range(0, cohort.NumClasses)
            .Select(i => ((char)('A' + i)).ToString());
        if (!allowed.Contains(section))
            throw new InvalidOperationException(
                $"Lớp '{section}' không hợp lệ cho khoá này (chỉ A..{(char)('A' + cohort.NumClasses - 1)}).");
    }
}
```

- [ ] **Step 3: `CohortMemberController` — thêm endpoint SetSection**

Thêm action (sau `SetActive`):
```csharp
    /// <summary>Đổi lớp (section) của học sinh trong khoá</summary>
    [HttpPatch("{id:guid}/section")]
    public async Task<ActionResult<RequestResponse<bool>>> SetSection(Guid id, [FromBody] string? section, CancellationToken ct = default)
    {
        var result = await service.SetSectionAsync(id, section, ct);
        return Ok(RequestResponse<bool>.Success("Cập nhật lớp thành công!", result, 1));
    }
```

- [ ] **Step 4: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API.slnx`
Expected: Build succeeded, 0 lỗi. (Nếu DI báo thiếu `ICohortRepository`, kiểm tra nó đã đăng ký — đã dùng bởi `CohortController` nên có sẵn.)

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberService.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs exam_hub_api/ExamHub.API/Controllers/School/CohortMemberController.cs
git commit -m "feat(api): validate member section + PATCH section endpoint"
```

---

## Task 6: Backend — Exam session resolve học sinh theo section

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs:142-174`

**Interfaces:**
- Consumes: `CohortMember.Section` (Task 4).
- Produces: hành vi — assignment cấp lớp (`CohortClassId`) chỉ gồm HS có `section` khớp lớp đó.

- [ ] **Step 1: Sửa `GetAssignedToStudentAsync`**

Thay thân method thành:
```csharp
    public async Task<IReadOnlyList<ExamSession>> GetAssignedToStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        var cohortIds = await _db.Set<CohortMember>()
            .Where(m => m.StudentId == studentId && m.IsActive)
            .Select(m => m.CohortId)
            .ToListAsync(ct);

        // Lớp cụ thể HS thuộc về: cùng cohort và section khớp
        var classIds = await _db.Set<CohortClass>()
            .Where(cc => _db.Set<CohortMember>().Any(m =>
                m.StudentId == studentId && m.IsActive &&
                m.CohortId == cc.CohortId && m.Section != null && m.Section == cc.Section))
            .Select(cc => cc.Id)
            .ToListAsync(ct);

        return await _db.Set<ExamSession>()
            .Include(s => s.Subject).Include(s => s.GradeLevel).Include(s => s.Assignments)
            .Where(s => s.Status == ExamSessionStatusEnum.Published)
            .Where(s => s.Assignments.Any(a =>
                (a.CohortId != null && cohortIds.Contains(a.CohortId.Value)) ||
                (a.CohortClassId != null && classIds.Contains(a.CohortClassId.Value))))
            .OrderByDescending(s => s.OpenAt)
            .ToListAsync(ct);
    }
```

- [ ] **Step 2: Sửa `IsStudentAssignedAsync`**

Thay thân method thành:
```csharp
    public async Task<bool> IsStudentAssignedAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)
    {
        var cohortIds = await _db.Set<CohortMember>()
            .Where(m => m.StudentId == studentId && m.IsActive)
            .Select(m => m.CohortId)
            .ToListAsync(ct);

        var classIds = await _db.Set<CohortClass>()
            .Where(cc => _db.Set<CohortMember>().Any(m =>
                m.StudentId == studentId && m.IsActive &&
                m.CohortId == cc.CohortId && m.Section != null && m.Section == cc.Section))
            .Select(cc => cc.Id)
            .ToListAsync(ct);

        return await _db.Set<ExamSession>()
            .Where(s => s.Id == sessionId)
            .AnyAsync(s => s.Assignments.Any(a =>
                (a.CohortId != null && cohortIds.Contains(a.CohortId.Value)) ||
                (a.CohortClassId != null && classIds.Contains(a.CohortClassId.Value))), ct);
    }
```

- [ ] **Step 3: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API.slnx`
Expected: Build succeeded, 0 lỗi.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs
git commit -m "feat(api): resolve class-level exam assignment by member section"
```

---

## Task 7: Frontend — types

**Files:**
- Modify: `exam_hub_web/src/types/school.d.ts`

**Interfaces:**
- Produces: `Cohort.numClasses`, `CohortBody.numClasses`, `CohortClass.section`, `CohortMember.section?`, `CohortMemberBody.section?`.

- [ ] **Step 1: Sửa các interface**

- Trong `Cohort`: đổi `classSuffix: string` → `numClasses: number`.
- Trong `CohortBody`: đổi `classSuffix?: string` → `numClasses: number`.
- Trong `CohortClass`: thêm `section: string` (sau `className: string`).
- Trong `CohortMember`: thêm `section?: string | null` (sau `studentId: string`).
- Trong `CohortMemberBody`: thêm `section?: string | null` (sau `studentId: string`).

- [ ] **Step 2: Verify typecheck**

Run: `pnpm -C exam_hub_web build`
Expected: FAIL — `SchoolDetailPage.tsx` còn dùng `classSuffix` (sẽ sửa Task 9). Ghi nhận đây là lỗi dự kiến, chưa commit riêng; gộp verify vào Task 9. Có thể tạm chỉ chạy để xác nhận lỗi đúng chỗ `classSuffix`.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_web/src/types/school.d.ts
git commit -m "feat(web): cohort/class/member section types"
```

---

## Task 8: Frontend — service + hook `setSection`

**Files:**
- Modify: `exam_hub_web/src/services/cohortMemberService.ts`
- Modify: `exam_hub_web/src/hooks/queries/useCohortMembers.ts`

**Interfaces:**
- Consumes: `CohortMemberBody.section` (Task 7).
- Produces: `cohortMemberService.setSection(id, section)`, `useSetCohortMemberSectionMutation(cohortId)`.

- [ ] **Step 1: `cohortMemberService` — thêm setSection**

Thêm method (sau `setActive`):
```typescript
    setSection(id: string, section: string | null) {
        return AuthHttp.patch<boolean>(`/cohortmember/${id}/section`, section)
    }
```

- [ ] **Step 2: `useCohortMembers` — thêm hook**

Thêm (sau `useSetCohortMemberActiveMutation`):
```typescript
export function useSetCohortMemberSectionMutation(cohortId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, section}: {id: string; section: string | null}) =>
            cohortMemberService.setSection(id, section),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Cập nhật lớp thành công')
            void qc.invalidateQueries({queryKey: COHORT_MEMBER_KEYS.byCohort(cohortId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}
```

- [ ] **Step 3: Verify lint**

Run: `pnpm -C exam_hub_web lint`
Expected: 0 lỗi ở 2 file này.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_web/src/services/cohortMemberService.ts exam_hub_web/src/hooks/queries/useCohortMembers.ts
git commit -m "feat(web): cohortMember setSection service + hook"
```

---

## Task 9: Frontend — form tạo khoá dùng "Số lớp"

**Files:**
- Modify: `exam_hub_web/src/pages/school/SchoolDetailPage.tsx:156-158`

**Interfaces:**
- Consumes: `CohortBody.numClasses` (Task 7).

- [ ] **Step 1: Đổi Form.Item classSuffix → numClasses**

Đổi:
```tsx
                        <Form.Item name="classSuffix" label="Hậu tố lớp" className="flex-1" initialValue="A">
                            <Input placeholder="A"/>
                        </Form.Item>
```
thành:
```tsx
                        <Form.Item name="numClasses" label="Số lớp" className="flex-1" initialValue={1}
                            rules={[{required: true}]}>
                            <Input type="number" min={1} max={26} placeholder="1"/>
                        </Form.Item>
```

- [ ] **Step 2: Verify build (typecheck toàn bộ, gồm Task 7)**

Run: `pnpm -C exam_hub_web build`
Expected: Build succeeded — không còn lỗi `classSuffix`.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_web/src/pages/school/SchoolDetailPage.tsx
git commit -m "feat(web): cohort create uses numClasses"
```

---

## Task 10: Frontend — CohortDetailPage: cột Lớp + xếp/đổi lớp

**Files:**
- Modify: `exam_hub_web/src/pages/school/CohortDetailPage.tsx`

**Interfaces:**
- Consumes: `CohortClass.section`, `CohortMember.section` (Task 7); `useSetCohortMemberSectionMutation` (Task 8).

- [ ] **Step 1: Import hook mới**

Đổi dòng import `useCohortMembers` để thêm `useSetCohortMemberSectionMutation`:
```tsx
import {useCohortMembersQuery, useAddCohortMemberMutation, useRemoveCohortMemberMutation, useSetCohortMemberActiveMutation, useSetCohortMemberSectionMutation} from '../../hooks/queries/useCohortMembers'
```

- [ ] **Step 2: Khởi tạo mutation + danh sách sections**

Sau dòng `const setActiveMutation = useSetCohortMemberActiveMutation(cohortId)`:
```tsx
    const setSectionMutation = useSetCohortMemberSectionMutation(cohortId)
```
Sau khối `const {data: allUsers = []} = useQuery(...)` (trước `const [memberModal...]`):
```tsx
    const sections = [...new Set(classes.map(c => c.section))].sort()
```

- [ ] **Step 3: Thêm cột "Lớp" vào `classColumns`**

Thêm mục này vào mảng `classColumns` (sau cột `className`):
```tsx
        {title: 'Lớp', dataIndex: 'section', key: 'section', width: 80},
```

- [ ] **Step 4: Thêm cột "Lớp" (Select đổi lớp) vào `memberColumns`**

Thêm mục này vào mảng `memberColumns` (sau cột `studentId`, trước cột 'Trạng thái'):
```tsx
        {
            title: 'Lớp', dataIndex: 'section', key: 'section', width: 130,
            render: (v, record) => (
                <Select
                    style={{width: 110}} allowClear placeholder="Chưa xếp"
                    value={v ?? undefined}
                    options={sections.map(s => ({value: s, label: s}))}
                    onChange={(val) => setSectionMutation.mutate({id: record.id, section: val ?? null})}
                />
            ),
        },
```

- [ ] **Step 5: Thêm Select "Lớp" vào modal thêm học sinh**

Trong `<Form form={memberForm} ...>`, sau `Form.Item name="studentId"`, thêm:
```tsx
                    <Form.Item name="section" label="Lớp">
                        <Select allowClear placeholder="Chưa xếp lớp"
                            options={sections.map(s => ({value: s, label: s}))}/>
                    </Form.Item>
```
(`handleAddMember` đã `...values` nên `section` được gửi kèm; không đổi.)

- [ ] **Step 6: Verify build + lint**

Run: `pnpm -C exam_hub_web build`
Expected: Build succeeded.
Run: `pnpm -C exam_hub_web lint`
Expected: 0 lỗi.

- [ ] **Step 7: Commit**

```bash
git add exam_hub_web/src/pages/school/CohortDetailPage.tsx
git commit -m "feat(web): show/assign student class (section) in cohort detail"
```

---

## Task 11: Kiểm thử thủ công (E2E)

**Files:** none (chạy hệ thống thật).

- [ ] **Step 1: Dựng lại DB dev** với `database_schema.sql` mới (hoặc chạy khối MIGRATION đã bỏ comment).

- [ ] **Step 2: Chạy API + web**, đăng nhập Admin.

- [ ] **Step 3:** Vào Trường THPT Lê Quý Đôn → khoá "Khoá 2021-2024" → tab **Lớp học**: thấy mỗi năm 3 lớp `10A/10B/10C`, `11A/…`, `12A/…` với cột **Lớp** = A/B/C.

- [ ] **Step 4:** Tab **Học sinh** → "Thêm học sinh": thêm 2 HS chọn lớp **A**, 1 HS **không chọn lớp** (Chưa xếp). Bảng hiển thị đúng cột Lớp; đổi lớp HS chưa xếp sang **B** qua Select → toast "Cập nhật lớp thành công".

- [ ] **Step 5:** Thử gán lớp ngoài dải (qua API trực tiếp `PATCH /cohortmember/{id}/section` body `"D"`) → nhận lỗi "Lớp 'D' không hợp lệ…".

- [ ] **Step 6:** Tạo Exam Session, thêm assignment **cấp lớp** = `10A`, publish. Đăng nhập HS lớp A → thấy kỳ thi; HS lớp B / chưa xếp → **không** thấy. (Xác nhận Task 6.)

---

## Self-Review Notes

- **Spec coverage:** §3 DB→Task 1; §4 Cohort→T2, CohortClass→T3, CohortMember→T4, Service validate+endpoint→T5, ExamSessionRepository→T6; §5 types→T7, service/hook→T8, SchoolDetailPage→T9, CohortDetailPage→T10; §7 kiểm thử→T11. Đủ.
- **Section list nguồn:** suy ra từ `distinct(cohort_classes.section)` đã tải sẵn (không cần fetch `cohort.numClasses` ở CohortDetailPage) — nhất quán giữa T10 và dữ liệu trigger sinh (T1).
- **Type/tên nhất quán:** `NumClasses`/`numClasses`, `Section`/`section`, `SetSectionAsync`/`setSection`/`useSetCohortMemberSectionMutation` khớp xuyên suốt.
- **Không placeholder.** Mọi step có code/lệnh cụ thể.
