# Bảng "Giao cho" của kỳ thi — hiển thị Trường / Khoá / Lớp / Phạm vi

**Goal:** Ở trang tạo/sửa kỳ thi (`ExamSessionEditPage`), phần **Giao cho** hiển thị dạng **bảng** với 4 cột **Trường · Khoá · Lớp · Phạm vi** (giống bộ chọn phía trên), thay vì chỉ có tên lớp/khoá + "#id".

**Bối cảnh:** `AssignmentResponse` backend hiện trả `CohortName`/`CohortClassName` = `null`; entity `ExamSessionAssignment` không có navigation → không lấy được tên trường/khoá/lớp. Phải enrich backend rồi mới render đủ cột ở FE.

**Tech:** .NET (EF Core, Npgsql) + React 19 + AntD 6 + TS. DB tạo từ `database_schema.sql` (không EF migrations) — quan hệ FK đã có sẵn trong DB, chỉ cần khai báo navigation để `Include`.

**Verify:** `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj` (API dev đang chạy khoá DLL nên build cả solution sẽ lỗi copy — build riêng Core để kiểm biên dịch); `pnpm -C exam_hub_web exec tsc -b` (chỉ còn lỗi pre-existing `RichTextEditor.tsx`); `pnpm -C exam_hub_web exec eslint <file>`; `pnpm -C exam_hub_web exec vite build`.

---

## Task 1: Backend — navigation cho `ExamSessionAssignment`  ✅
**File:** `exam_hub_api/ExamHub.Core/Domain/Entities/ExamSessionAssignment.cs`
- [x] Thêm `public Cohort? Cohort { get; set; }` và `public CohortClass? CohortClass { get; set; }`.

## Task 2: Backend — cấu hình quan hệ trong `AppDbContext`  ✅
**File:** `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/AppDbContext.cs` (khối `Entity<ExamSessionAssignment>`)
- [x] `HasOne(x => x.Cohort).WithMany().HasForeignKey(x => x.CohortId).OnDelete(NoAction)`.
- [x] `HasOne(x => x.CohortClass).WithMany().HasForeignKey(x => x.CohortClassId).OnDelete(NoAction)`.

## Task 3: Backend — `Include` trong `GetDetailAsync`
**File:** `.../Repositories/Implementations/ExamSessionRepository.cs` (~dòng 13-19)
- [x] Thêm:
  `.Include(s => s.Assignments).ThenInclude(a => a.Cohort!).ThenInclude(c => c.School)`
  `.Include(s => s.Assignments).ThenInclude(a => a.CohortClass!).ThenInclude(cc => cc.Cohort!).ThenInclude(c => c.School)`

## Task 4: Backend — thêm `SchoolName` vào `AssignmentResponse`
**File:** `.../DataTransferObjects/ExamSession/ExamSessionDtos.cs` (~dòng 69)
- [x] `AssignmentResponse(Guid Id, int? CohortId, string? CohortName, int? CohortClassId, string? CohortClassName, string? SchoolName)`.

## Task 5: Backend — map tên trong `GetDetailAsync` (service)
**File:** `.../Services/Implementations/ExamSessionService.cs` (~dòng 62-64)
- [x] Map: `CohortName = a.CohortClass?.Cohort?.Name ?? a.Cohort?.Name`, `CohortClassName = a.CohortClass?.ClassName`, `SchoolName = a.CohortClass?.Cohort?.School?.Name ?? a.Cohort?.School?.Name`.

## Task 6: Frontend — type `SessionAssignment`
**File:** `exam_hub_web/src/types/examSession.d.ts`
- [x] Thêm `schoolName?: string`.

## Task 7: Frontend — cột bảng Trường/Khoá/Lớp/Phạm vi
**File:** `exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx` (`AssignmentSection`)
- [x] Cột: **Trường** (`schoolName`), **Khoá** (`cohortName`), **Lớp** (`cohortClassName` / "Cả khoá"), **Phạm vi** (Tag Một lớp / Cả khoá), + cột **Gỡ**.

## Task 8: Verify + commit
- [x] Build Core + tsc + eslint + vite build sạch (trừ lỗi pre-existing).
- [x] Commit backend và frontend riêng.
