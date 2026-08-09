# Phân công GV giảng dạy cho lớp — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cho phép Admin phân công giáo viên bộ môn giảng dạy cho từng lớp (theo môn), với ràng buộc 1 môn/lớp = 1 GV, GV lọc theo trường + đúng môn.

**Architecture:** Full-stack. Backend (ASP.NET, TVT.Core SqlBuilder) thêm bảng + entity + repo + service + controller theo pattern `TeacherSubject`. Frontend (React/AntD) thêm service + hook + drawer mở từ dòng lớp trong `CohortDetailPage`. Làm BE trước, FE sau.

**Tech Stack:** ASP.NET Core, PostgreSQL, TVT.Core (SqlBuilderProperty), React 19, AntD 6, TanStack Query.

## Global Constraints

- **Ràng buộc xuyên suốt (mọi thao tác thêm/sửa/xoá):** (1) validate dữ liệu đầu vào; (2) kiểm ràng buộc nghiệp vụ & tham chiếu (trùng lặp; cha còn con thì không xoá); (3) chỉ khi hợp lệ mới ghi DB; (4) trả message kết quả rõ ràng (tiếng Việt). KHÔNG để lỗi FK thô của DB nổi lên.
- Unique nghiệp vụ: `(cohort_class_id, subject_id)` — 1 môn/lớp = 1 GV.
- GV hợp lệ: `school_members`(role Teacher, is_active) của trường sở hữu khoá ∩ `teacher_subjects`(subject_id).
- Không có test project → verify bằng `dotnet build` + file `.http` (BE) và `npx tsc --noEmit`/`npm run build` + thao tác thủ công (FE).
- Nhánh `feat/teaching-assignment` (base `main`). Commit message kết `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`.

---

## File Structure

**Backend** (`exam_hub_api/ExamHub.Core` & `ExamHub.API`):
- `database_schema.sql` — thêm bảng `cohort_class_teachers`.
- Migration EF (nếu dự án dùng) — mirror bảng.
- `FieldTables/CohortClassTeacherTable.cs` — hằng tên bảng/cột.
- `Domain/Entities/CohortClassTeacher.cs` — entity.
- `DataTransferObjects/School/CohortClassTeacherDto.cs` — DTO đọc + request assign + eligible-teacher DTO.
- `Domain/Interfaces/Category/ICohortClassTeacherRepository.cs` + Impl.
- `Domain/Interfaces/Category/ICohortClassTeacherService.cs` + Impl.
- `Controllers/School/CohortClassTeacherController.cs`.
- `DependencyContainer.cs` — đăng ký DI.
- `SchoolService.cs` — guard xoá (ví dụ ràng buộc).
- `ExamHub.API/*.http` — smoke test.

**Frontend** (`exam_hub_web/src`):
- `types/school.d.ts` — thêm type.
- `services/cohortClassTeacherService.ts`.
- `hooks/queries/useCohortClassTeachers.ts`.
- `pages/school/TeachingAssignmentDrawer.tsx`.
- `pages/school/CohortDetailPage.tsx` — nút "Phân công" + state drawer.

---

## Task 1: DB — bảng `cohort_class_teachers`

**Files:** Modify `exam_hub_api/database_schema.sql`; (nếu có) thêm migration.

- [ ] **Step 1: Xác minh cách provisioning DB**

Run: `ls exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Migrations` và đọc `README`/`CONTEXT.md` để biết DB dựng bằng `database_schema.sql` thủ công hay `dotnet ef database update`. Ghi lại kết luận.

- [ ] **Step 2: Thêm bảng vào `database_schema.sql`** (sau `teacher_subjects`)

```sql
-- Phân công GV giảng dạy cho lớp (1 môn/lớp = 1 GV)
CREATE TABLE public.cohort_class_teachers
(
    id              SERIAL PRIMARY KEY,
    cohort_class_id INT  NOT NULL REFERENCES cohort_classes (id) ON DELETE CASCADE,
    subject_id      INT  NOT NULL REFERENCES subjects (id)       ON DELETE CASCADE,
    teacher_id      UUID NOT NULL REFERENCES app_users (id)      ON DELETE CASCADE,
    UNIQUE (cohort_class_id, subject_id)
);
```

- [ ] **Step 3: Migration (nếu dùng EF)** — nếu Step 1 kết luận dùng EF: `dotnet ef migrations add AddCohortClassTeachers` trong `ExamHub.Core`, kiểm file sinh ra khớp bảng. Nếu dùng schema.sql thủ công: bỏ qua, ghi chú chạy lại script.

- [ ] **Step 4: Áp DB local** và verify bảng tồn tại: `psql ... -c "\d cohort_class_teachers"` (hoặc script tạo DB dự án dùng). Expected: bảng + unique index hiện ra.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/database_schema.sql exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Migrations
git commit -m "feat(db): bảng cohort_class_teachers (phân công GV giảng dạy)"
```

---

## Task 2: Entity + FieldTable + DTO

**Files:** Create `FieldTables/CohortClassTeacherTable.cs`, `Domain/Entities/CohortClassTeacher.cs`, `DataTransferObjects/School/CohortClassTeacherDto.cs`.

**Interfaces:**
- Produces: `CohortClassTeacher` entity (Id:int, CohortClassId:int, SubjectId:int, TeacherId:Guid); `CohortClassTeacherDto`; `AssignTeacherRequest`; `EligibleTeacherDto`.

- [ ] **Step 1: FieldTable** — mở `FieldTables/TeacherSubjectTable.cs` làm mẫu, tạo `CohortClassTeacherTable` với `TableName = "cohort_class_teachers"` và các cột `Id`, `CohortClassId="cohort_class_id"`, `SubjectId="subject_id"`, `TeacherId="teacher_id"`.

- [ ] **Step 2: Entity** — mở `Domain/Entities/TeacherSubject.cs` làm mẫu; tạo `CohortClassTeacher : IModelBaseSql<int>` với 3 cột + `ToInsertObject()` (cohort_class_id, subject_id, teacher_id), `ToUpdateObject()` trả `{ id = Id }` (không hỗ trợ update — xoá & tạo mới).

- [ ] **Step 3: DTO** — mở `DataTransferObjects/School/CohortClassDto.cs` làm mẫu; tạo:
  - `CohortClassTeacherDto { int Id; int CohortClassId; int SubjectId; string SubjectName; Guid TeacherId; string TeacherName; }`
  - `AssignTeacherRequest { int CohortClassId; int SubjectId; Guid TeacherId; }`
  - `EligibleTeacherDto { Guid Id; string Name; }`

- [ ] **Step 4: Build** — `cd exam_hub_api && dotnet build`. Expected: build succeeded.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core
git commit -m "feat(be): entity/DTO CohortClassTeacher"
```

---

## Task 3: Repository

**Files:** Create `Domain/Interfaces/Category/ICohortClassTeacherRepository.cs`; `Infrastructure/Persistence/Repositories/Implementations/Category/CohortClassTeacherRepository.cs`.

**Interfaces:**
- Consumes: entity/DTO (Task 2).
- Produces: `ICohortClassTeacherRepository` với:
  - `Task<IReadOnlyList<CohortClassTeacherDto>> GetByClassAsync(int cohortClassId, CancellationToken ct)`
  - `Task<bool> ExistsAsync(int cohortClassId, int subjectId, CancellationToken ct)`
  - `Task<IReadOnlyList<EligibleTeacherDto>> GetEligibleTeachersAsync(int cohortClassId, int subjectId, CancellationToken ct)`
  - `Task<int> InsertAsync(CohortClassTeacher e, CancellationToken ct)`
  - `Task DeleteByIdAsync(int id, CancellationToken ct)`

- [ ] **Step 1: Interface** — tạo file interface với 5 method trên.

- [ ] **Step 2: Impl** — mở `Repositories/Implementations/TeacherSubjectRepository.cs` + `Category/CohortClassRepository.cs` làm mẫu (cùng cách dùng SqlBuilder/Dapper của dự án). Viết SQL:
  - `GetByClassAsync`: JOIN `cohort_class_teachers cct` × `subjects s` × `app_users u` WHERE `cct.cohort_class_id=@id`, select id, cohort_class_id, subject_id, s.name, teacher_id, u.display_name/user_name.
  - `ExistsAsync`: `SELECT EXISTS(SELECT 1 FROM cohort_class_teachers WHERE cohort_class_id=@c AND subject_id=@s)`.
  - `GetEligibleTeachersAsync`: từ `cohort_classes cc` JOIN `cohorts co` (lấy school_id) JOIN `school_members sm`(role='Teacher' AND is_active) JOIN `app_users u` JOIN `teacher_subjects ts`(ts.user_id=sm.user_id AND ts.subject_id=@subjectId) WHERE cc.id=@classId → distinct (u.id, name).
  - `InsertAsync`/`DeleteByIdAsync`: theo pattern repo hiện có.

- [ ] **Step 3: Build** — `dotnet build`. Expected: succeeded.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.Core
git commit -m "feat(be): repository CohortClassTeacher (getByClass, eligible, exists, insert, delete)"
```

---

## Task 4: Service (validate + ràng buộc) + DI

**Files:** Create `Domain/Interfaces/Category/ICohortClassTeacherService.cs`; `Infrastructure/Persistence/Services/Implementations/Category/CohortClassTeacherService.cs`; Modify `DependencyContainer.cs`.

**Interfaces:**
- Produces: `ICohortClassTeacherService`:
  - `Task<IReadOnlyList<CohortClassTeacherDto>> GetByClassAsync(int classId, CancellationToken ct)`
  - `Task<IReadOnlyList<EligibleTeacherDto>> GetEligibleTeachersAsync(int classId, int subjectId, CancellationToken ct)`
  - `Task<RequestResponse<int>> AssignAsync(AssignTeacherRequest req, CancellationToken ct)`
  - `Task<RequestResponse<bool>> RemoveAsync(int id, CancellationToken ct)`

- [ ] **Step 1: Interface** — tạo với 4 method.

- [ ] **Step 2: Impl** — mở `Services/Implementations/TeacherSubjectService.cs` làm mẫu cho kiểu trả `RequestResponse`. Trong `AssignAsync` áp Global Constraint:
  1. Validate req (cohortClassId>0, subjectId>0, teacherId≠empty) → nếu sai trả `RequestResponse.Error("Dữ liệu không hợp lệ.")`.
  2. Kiểm eligible: teacherId ∈ `GetEligibleTeachersAsync(classId, subjectId)` → nếu không, `Error("Giáo viên không hợp lệ cho môn/ trường này.")`.
  3. Kiểm trùng: `ExistsAsync(classId, subjectId)` → nếu có, `Error("Môn đã được phân công cho GV khác trong lớp này.")`.
  4. `InsertAsync` → `RequestResponse.Success(newId, "Phân công thành công.")`.
  `RemoveAsync`: kiểm tồn tại (nếu repo cần) → xoá → `Success(true, "Đã xoá phân công.")`.

- [ ] **Step 3: DI** — trong `DependencyContainer.cs` đăng ký `ICohortClassTeacherRepository`/`Service` (theo dòng đăng ký TeacherSubject hiện có).

- [ ] **Step 4: Build** — `dotnet build`. Expected: succeeded.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core
git commit -m "feat(be): service CohortClassTeacher — validate + ràng buộc trùng/eligible"
```

---

## Task 5: Controller + smoke test `.http`

**Files:** Create `Controllers/School/CohortClassTeacherController.cs`; thêm block vào `ExamHub.API/*.http`.

- [ ] **Step 1: Controller** — mở `Controllers/Teacher/TeacherSubjectController.cs` làm mẫu. Route `api/cohort-class-teachers`, `[ApiController]`, endpoints:
  - `GET by-class/{cohortClassId:int}` → `service.GetByClassAsync`.
  - `GET eligible-teachers` (query `cohortClassId`, `subjectId`) → `service.GetEligibleTeachersAsync`.
  - `POST assign` (body `AssignTeacherRequest`) → `service.AssignAsync`; trả `Ok(resp)` hoặc `BadRequest(resp)` theo `resp.Status`.
  - `DELETE remove/{id:int}` → `service.RemoveAsync`.
  Áp `[Authorize]` khớp các controller School khác.

- [ ] **Step 2: `.http` smoke** — thêm request: assign (OK) → assign trùng (kỳ vọng lỗi message) → eligible-teachers (kỳ vọng danh sách đúng) → by-class → remove.

- [ ] **Step 3: Verify** — `dotnet run` API, chạy `.http`. Expected: assign đầu OK; assign trùng trả message lỗi; eligible lọc đúng; remove OK.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.API
git commit -m "feat(be): controller api/cohort-class-teachers + smoke .http"
```

---

## Task 6: Guard xoá trường (ví dụ ràng buộc tham chiếu)

**Files:** Modify `Infrastructure/Persistence/Services/Implementations/Category/SchoolService.cs` (`DeleteAsync`).

- [ ] **Step 1: Chặn xoá khi còn khoá** — trong `DeleteAsync(int id, ct)`: gọi `GetWithCohortsAsync(id, ct)` (đã có); nếu `school.Cohorts?.Count > 0` → trả/ném lỗi nghiệp vụ `"Không thể xoá: trường còn {n} khoá học liên kết."` thay vì xoá. Nếu chữ ký hiện tại là `Task` không trả `RequestResponse`, đổi sang trả `RequestResponse<bool>` và cập nhật controller `SchoolController.Delete` hiển thị message (kiểm nơi gọi).

- [ ] **Step 2: Build** — `dotnet build`. Expected: succeeded.

- [ ] **Step 3: Verify `.http`** — xoá 1 trường còn khoá → kỳ vọng lỗi message; xoá trường rỗng → OK.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api
git commit -m "feat(be): chặn xoá trường còn khoá liên kết (validate ràng buộc tham chiếu)"
```

---

## Task 7: Frontend — types + service + hook

**Files:** Modify `types/school.d.ts`; Create `services/cohortClassTeacherService.ts`, `hooks/queries/useCohortClassTeachers.ts`.

**Interfaces:**
- Produces: `cohortClassTeacherService` { getByClass(classId), getEligibleTeachers(classId, subjectId), assign(body), remove(id) }; hooks `useClassTeachersQuery(classId)`, `useEligibleTeachersQuery(classId, subjectId)`, `useAssignTeacherMutation(classId)`, `useRemoveTeacherMutation(classId)`.

- [ ] **Step 1: Types** — thêm `CohortClassTeacher { id; cohortClassId; subjectId; subjectName; teacherId; teacherName }`, `EligibleTeacher { id; name }`, `AssignTeacherBody { cohortClassId; subjectId; teacherId }`.

- [ ] **Step 2: Service** — mở `services/teacherSubjectService.ts` + `cohortClassService.ts` làm mẫu (dùng `requestService`). 4 hàm gọi `api/cohort-class-teachers/...`.

- [ ] **Step 3: Hook** — mở `hooks/queries/useCohortClasses.ts` làm mẫu. Query getByClass + eligible; mutation assign/remove với `onSuccess` invalidate query by-class và `message.success`/`message.error` theo response.

- [ ] **Step 4: Build** — `cd exam_hub_web && npx tsc --noEmit`. Expected: no error.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/src/types exam_hub_web/src/services exam_hub_web/src/hooks
git commit -m "feat(fe): service + hook cohortClassTeacher"
```

---

## Task 8: Frontend — Drawer + nút trong CohortDetailPage

**Files:** Create `pages/school/TeachingAssignmentDrawer.tsx`; Modify `pages/school/CohortDetailPage.tsx`.

- [ ] **Step 1: Drawer** — `TeachingAssignmentDrawer({cohortClass, open, onClose})`:
  - Header: `Phân công giảng dạy — Lớp {className}`.
  - Form: `Select` Môn (mọi môn) → khi chọn môn, gọi `useEligibleTeachersQuery(classId, subjectId)` → `Select` GV (options eligible) → nút "Thêm" gọi `assign`. Thành công/lỗi: message từ hook.
  - Bảng phân công hiện tại (`useClassTeachersQuery`): cột Môn, GV, Thao tác (Popconfirm Xoá → `remove`).

- [ ] **Step 2: Nút trong bảng lớp** — trong `CohortDetailPage` `classColumns`, thêm nút `Phân công` (mở drawer với `record`) cạnh Select GVCN ở cột Thao tác; thêm state `assignClass` + render `<TeachingAssignmentDrawer .../>`.

- [ ] **Step 3: Build** — `npx tsc --noEmit` + `npm run build`. Expected: no error, exit 0.

- [ ] **Step 4: Verify thủ công** — `npm run dev`: mở khoá → tab Lớp học → Phân công → chọn môn (GV lọc đúng), Thêm; thử thêm trùng môn → message lỗi; xoá phân công.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/src/pages/school
git commit -m "feat(fe): drawer phân công GV giảng dạy trong CohortDetailPage"
```

---

## Self-Review

- **Spec coverage:** §4 model→T1; §5 backend→T2-5; §3 global constraint→T4 (assign) + T5 (controller trả BadRequest) + T6 (ví dụ xoá trường); §6 frontend→T7-8. Đủ.
- **Placeholder:** SQL/endpoints/DTO cụ thể; phần SqlBuilder repo chỉ ra file template chính xác để mirror (không có sẵn code trong ngữ cảnh) — chấp nhận trong codebase pattern-based, executor đọc template.
- **Type consistency:** `AssignTeacherRequest`(BE)/`AssignTeacherBody`(FE), `EligibleTeacherDto`/`EligibleTeacher`, tên method service khớp giữa Task 3/4/5/7/8.
- **Rủi ro:** provisioning DB (T1 Step 1 xác minh trước); chữ ký `SchoolService.DeleteAsync` có thể cần đổi kiểu trả (T6 Step 1 nêu rõ).
