# ExamHub — Next-Phase Plan (Backend + Frontend Gap Closure to Spec)

> Covers two repos: **`exam_hub_api`** (.NET 10 backend) and **`exam_hub_web`**
> (React 19 + Vite + TypeScript frontend). Backend phases are numbered `Phase 1..7`;
> frontend phases are numbered `FE-1..FE-6` and depend on the matching backend endpoints.

## Progress (updated 2026-06-13)

> **Status: Phases 1–7 + FE-1–FE-6 complete; Phase 8, FE-7, FE-8 newly added (not started).**
> Backend Phases 1–7 + `IBaseRepository` migration of `PickRandomAsync` + `context.md` doc-drift
> fixes done; `dotnet build` clean. Frontend FE-1–FE-6 wired to the real API; `tsc -b` + `vite build`
> clean, no new lint errors, `__mocks__` removed. Testing was removed from scope per request.
>
> **New scope (2026-06-13):** the spec was missing a **School Management UI** (backend is fully
> built but the frontend has no pages) and a **role-based navigation menu**. Added below as backend
> **Phase 8 — Role-based menu endpoint**, frontend **FE-7 — School Management UI** (separate routes
> per level), and **FE-8 — Role-based menu consumption**. These are not yet started.
>
> Remaining: implement Phase 8 / FE-7 / FE-8, commit the working-tree changes (both repos are
> uncommitted; API on `feature/phase-1-foundation`), and run the manual end-to-end smoke against a
> live Postgres + Redis + MinIO stack.

### Backend (`exam_hub_api`)

- [x] **Phase 1 — Bloom filter wiring.** `cognitiveLevelId` threaded through
  `IQuestionService`/`QuestionService`/`IQuestionRepository`/`QuestionRepository` and
  `QuestionController.GetPaged`. Build clean.
- [x] **Phase 2 — Submission grading loop.** MC auto-grading at submit (set-equality of
  `SelectedAnswerIds` vs correct ids in snapshot) + `FinalizeAsync` + `POST .../finalize`.
  **Design fix:** snapshot JSON now includes the answer `id` (generator SQL
  `QuestionRepository.BuildPickSql`) — the submission flow requires it to reference answers.
- [x] **Phase 3 — Exam analytics.** `GetAnalyticsAsync` + `GET /api/exams/{id}/analytics`;
  added `IExamQuestionRepository.GetByExamWithClassificationAsync`. Distributions by
  Bloom/difficulty/topic per the existing `ExamAnalyticsResponse` DTO (no submission stats — DTO
  has none).
- [x] **Phase 4 — Question attachment.** `POST /api/questions/{id}/attachment` reusing the
  `TVT.Core` `IMinioStorageService` (singleton, default bucket pre-set); validates type/size;
  persists URL via new `SetImageUrlAsync` (ExecuteUpdate pattern).
- [x] **Phase 5 — Bulk Excel import.** Added `ClosedXML` (MIT — chosen over EPPlus to avoid the
  noncommercial license prompt). `IBulkImportService`/`BulkImportService` parse + per-row
  validation + partial import; `POST /api/questions/bulk-import` (`[FromForm]`). Build clean.
- [x] **Phase 6 — Export PDF/Word.** `ExportService` (QuestPDF for PDF, `DocumentFormat.OpenXml`
  for Word — `context.md`'s "ClosedXML for Word" is wrong; ClosedXML is Excel-only) renders the
  `Exam` + `ExamQuestion` snapshots (HTML stripped to plaintext, answers A/B/C…), uploads to MinIO
  `exports/{examId}.{ext}` via `UploadStreamAsync`, returns the URL. `GET /api/exams/{id}/export?format=pdf|docx`. Build clean.
- [x] **Phase 7 — Redis pool cache.** `QuestionRepository.PickRandomAsync` now caches the candidate
  ID pool (IDs only, 2-min TTL) under `qpool:{topicId}:{diffId}:{typeId|"all"}:{cogId|"all"}` via
  `IRedisService`, then excludes/shuffles (Fisher-Yates)/picks in memory and fetches content for the
  picked IDs. `QuestionService` invalidates the ≤4 pool keys a question participates in on
  create/update (old+new classification)/delete/verify. Shared `QuestionPoolCache` key helper. Build clean.
- [x] **Phase 8 — Role-based navigation menu endpoint.** `GET /api/menu` returns the nav items the
  current user's roles (from JWT) are allowed to see. `MenuRegistry` (static config) + `MenuController`
  extending `AuthorizeControllerBase`. `dotnet build` clean.
- [x] **Phase 9 — Self-service profile endpoints.** `PUT /api/auth/profile` (update own
  name/email/phone) + `POST /api/auth/change-password` (verify old, set new) in `AuthController`/
  `AuthService` (`UpdateProfileDto`/`ChangePasswordDto` in `AuthDto.cs`). Reuses `GET /api/auth/info`.
  Also added `Email` field to `TVT.Core` `UserInfo` (mapped via `UserAdmin.GetEmailName()`) so
  `GET /api/auth/info` now returns email. `dotnet build` clean.
- [x] **Cross-cutting — `context.md` doc-drift fixes.** .NET 8→10, EF Core 8→10, "ClosedXML for
  Word"→DocumentFormat.OpenXml (ClosedXML is Excel-only), React 18→19, shadcn/ui→Ant Design v6,
  Axios→fetch wrapper, table count note (School module adds 5 tables). `CONTEXT.md` left authoritative.

### Frontend (`exam_hub_web`)

- [x] **FE-1 — API plumbing.** Added `postForm` (multipart, no forced Content-Type) + `cleanParams`
  to `requestService.ts`; typed services `questionService`/`examService`/`examTemplateService`/
  `examGeneratorService`/`submissionService` + `src/types/{question,exam,examTemplate,submission}.d.ts`
  and `PagedResult<T>` in `common.d.ts`. `tsc -b` + eslint clean.
- [x] **FE-2 — Question Bank wiring.** `QuestionBankPage` now AntD `Table` + TanStack Query via
  `useQuestions`/`useCategoryLists`: server pagination, filters (topic/type/difficulty/**Bloom**/verified/keyword),
  verify + delete, and a `BulkImportModal` (`questionService.bulkImport`). `AddQuestionPage` is a real
  create/edit form (`Form.List` answers, classification selects, attachment upload in edit mode) on
  new route `questions/:id/edit`. `tsc -b` + eslint clean.
- [x] **FE-3 — Exam Templates wiring.** `ExamTemplatePage` lists via `examTemplateService.getByGrade`
  (grade selector + subject/search filter, delete, "Sinh đề" link). `CreateExamTemplatePage` is a real
  create/edit form with a `Form.List` section editor (Topic + QuestionType + difficulty % + optional
  CognitiveLevel + count/score) on route `exams/:id/edit`. `tsc -b` + eslint clean.
- [x] **FE-4 — Exam generation + list/export.** `/generate` route now `GeneratePage` (single + batch
  via `examGeneratorService`, prefilled from `?templateId=`). New `ExamListPage` (route `exam-list`,
  added to sidebar) with filters/pagination, publish/delete, a snapshot-question preview drawer, and
  PDF/Word export buttons opening the MinIO URL. `useExams` hook. `tsc -b` clean; no new lint errors.
- [x] **FE-5 — Student flow + grading.** `ExamCoverPage`/`ExamTakingPage` load a real exam via
  `?examId=` (snapshot questions, answer ids), submit through `submissionService` (studentId from JWT
  `UserId` claim, now exposed on `user.id`); new `ExamResultPage`. Teacher grading via `SubmissionsDrawer`
  (per-essay grade + finalize) opened from `ExamListPage`. Shared `utils/snapshot.ts`. Build + lint clean.
- [x] **FE-6 — Dashboard/analytics.** `DashboardPage` now uses real aggregates (question/exam totals
  from paged `total`, recent-exams table, derived status pie via recharts) with wired quick actions.
  `AnalyticsDrawer` (recharts bar charts for Bloom/difficulty/topic from `GET /exams/{id}/analytics`)
  opened from `ExamListPage`. All three `__mocks__` files deleted. `tsc -b` + `vite build` clean; no new lint errors.
- [x] **FE-7 — School Management UI.** `SchoolListPage` / `SchoolDetailPage` (tabs: Khoá học +
  Thành viên) / `CohortDetailPage` (tabs: Lớp học + Học sinh). Services: `cohortService`,
  `cohortClassService`, `schoolMemberService`, `cohortMemberService`. Hooks: `useSchools`,
  `useCohorts`, `useCohortClasses`, `useSchoolMembers`, `useCohortMembers`. Routes registered;
  types extended in `school.d.ts`. `vite build` clean.
- [x] **FE-8 — Role-based menu consumption.** `menuService` + `useMenuQuery` (5-min stale).
  `AppLayout.tsx` replaced hardcoded `NAV_ITEMS` with API data + `ICON_MAP` + `FALLBACK_NAV`.
  `BankOutlined` added for `schools`. `vite build` clean.
- [x] **FE-10 — Profile / detail screens (3 separate per role).** `StudentProfilePage` (khoá/lớp +
  thống kê thi), `TeacherProfilePage` (môn phụ trách + trường), `AdminProfilePage` (thống kê hệ
  thống). Shared `ProfileCard` + `EditProfileModal` + `ChangePasswordModal` (`authService.getInfo/
  updateProfile/changePassword`). `AppProfilePage` chọn Admin/Teacher theo role tại `/app/profile`;
  `/student/profile` trong StudentLayout. Entry: nút "Tài khoản" ở sidebar footer + tên ở header HS.
  `tsc -b` + `vite build` clean.
- [x] **FE-9 — Student exam list / portal home.** `StudentExamListPage` (`/student/exams`) +
  `StudentLayout` (first layout for the student flow — header + logout). Client-side merge of
  published exams (`useExamsQuery`) and the student's submissions (`useMySubmissionsQuery`) →
  per-exam status (Chưa làm / Đang làm / Đã nộp / Đã chấm) + điểm; filters by subject/grade/status/
  keyword. Login redirects Student-only accounts here (`homePathForRoles`); `ExamResultPage` "Hoàn
  tất" now returns to `/student/exams`. `vite build` clean.

## Context

The ExamHub backend (`feature/phase-1-foundation`, .NET 10, ASP.NET Core + Dapper-style
repositories over `TVT.Core`) already implements the foundation described in `context.md`:
Config/Category CRUD (incl. Bloom `CognitiveLevel`), Question Bank CRUD with the
`TeacherOwnsSubject` authorization policy, Exam generation + batch generation (stratified
sampling by difficulty, Bloom filter, Fisher-Yates shuffle, variants/batches), Exam template
authoring, School/Cohort/Identity, JWT auth, and a submission *submit + manual essay grade*
skeleton.

A gap analysis against `context.md` (§7 endpoints, §8 algorithm, §10 cache, §13 NFRs) found
several spec features that are scaffolded but not wired, plus correctness gaps. This plan
closes them in dependency order so each phase is independently shippable. The intended outcome:
the API fully satisfies the `context.md` feature set end-to-end (Teacher authoring → exam
generation → export → Student take-exam → grading → analytics).

**Key reuse (do NOT rebuild):**
- **MinIO**: `TVT.Core.MinioStorage.IMinioStorageService.UploadStreamAsync(stream, objectName, contentType)` — already DI-registered via `AddMinioService(config)` in `ExamHub.Core/DependencyContainer.cs`. Config key `MinioStorageConfig` in `appsettings.json`. The empty local `Infrastructure/Storage` folder and the unused local `IExportService` should consume this, not a hand-rolled MinIO client.
- **Redis**: `TVT.Core.Db.Redis` registered via `AddRedisStorage("examhub-core", isDev)`; `IRedisService` available (see commit `7998b9e`).
- **Repository rule** (`CONTEXT.md`): all new DB access goes through `ICommonRepository<T,TId>` / `IBaseRepository` (`repo.GetBaseRepository().QueryAsync<T>(sql, …)`); never `new NpgsqlConnection`.
- **Response envelope**: `RequestResponse<T>.Success/Error` from `TVT.Core` (used by every controller).
- **Existing DTOs to fill in**: `BulkImportQuestionRequest`, `ExamAnalyticsResponse`, interface `IExportService` — all present, no implementation behind them.
- **Question picking**: `IQuestionRepository.PickRandomAsync(...)` + `PickedQuestion` record already used by the generator.

---

## Phase 1 — Correctness quick wins (Bloom filter wiring)

The `cognitiveLevel` question filter from spec §7 is dead: `QuestionPagedRequest.CognitiveLevelId`
(`DataTransferObjects/Question/QuestionDto.cs:146`) is never threaded to the query.

- Add `int? cognitiveLevelId = null` param to `IQuestionService.GetPagedAsync`
  (`Domain/Interfaces/IExamServices.cs:19`) and `IQuestionRepository.GetPagedAsync`
  (`Domain/Interfaces/IQuestionRepositories.cs:27`). Note: `PickRandomAsync`
  (`IQuestionRepositories.cs:46`) already takes `cognitiveLevelId` — only the paged-list path
  is missing it, so this is the single repo method to touch.
- Pass it through `QuestionService.GetPagedAsync`
  (`Infrastructure/.../Services/Implementations/QuestionService.cs:27`) and add the
  `cognitive_level_id` predicate in `QuestionRepository.GetPagedAsync`.
- Pass `request.CognitiveLevelId` from `QuestionController.GetPaged`
  (`ExamHub.API/Controllers/Question/QuestionController.cs:31`).

**Verify:** `GET /api/questions?cognitiveLevelId=3` returns only Bloom-`apply` questions.

## Phase 2 — Submission grading loop (Student flow correctness)

Spec: MC auto-graded 0/1 at submit; Teacher `finalize` recomputes `total_score` and sets
`Graded`. Current `ExamSubmissionService.SubmitAsync` stores answers without auto-grading and
there is no finalize step.

- In `ExamSubmissionService` (`Infrastructure/.../Services/Implementations/ExamSubmissionService.cs`):
  inject `IExamQuestionRepository`. In `SubmitAsync`, for each answer whose `ExamQuestion`
  question type is `multiple_choice`, compare `SelectedAnswerIds` against the correct answer ids
  parsed from the `answers_snapshot` JSONB → set `ScoreEarned` (0 or the question `Score`) and
  `IsCorrect`. Leave essay answers ungraded.
- Add `FinalizeAsync(Guid submissionId, Guid gradedBy, CancellationToken ct)` to
  `IExamSubmissionService` (`Domain/Interfaces/IExamServices.cs:104`) + impl: sum
  `SubmissionAnswer.ScoreEarned` → `ExamSubmission.TotalScore`, set `Status = Graded`.
- Add endpoint `POST /api/exam-submissions/{id}/finalize` in `ExamSubmissionController.cs`
  (`[Authorize(Roles="Admin,Teacher")]`).

**Verify:** submit a MC-only attempt → answers carry 0/1 scores immediately; call finalize →
`total_score` equals their sum and status is `Graded`.

## Phase 3 — Exam analytics endpoint

`ExamAnalyticsResponse` DTO exists; no service/endpoint (spec §7 `GET /exams/{id}/analytics`).

- Add `GetAnalyticsAsync(Guid examId, CancellationToken ct)` to `IExamService`
  (`Domain/Interfaces/IExamServices.cs:76`) → returns `ExamAnalyticsResponse`.
- Implement aggregation in `ExamService` using a raw `IBaseRepository.QueryAsync` over
  `exam_questions` joined to `questions` for Bloom + difficulty distribution, plus
  `exam_submissions` for count/avg/min/max score.
- Add endpoint `GET /api/exams/{id}/analytics` in `ExamController.cs`.

**Verify:** generate an exam, add submissions, `GET /api/exams/{id}/analytics` returns
per-Bloom and per-difficulty counts and submission score stats.

## Phase 4 — Question attachments (reuse MinIO)

Spec §7 `POST /questions/{id}/image`. `Question` already has `ImageUrl`/`AudioUrl`; MinIO is
wired but unused.

- Add `POST /api/questions/{id}/attachment` to `QuestionController` accepting `IFormFile`
  (follow `minimal-api-file-upload` constraints: validate content-type ∈ {image/*, application/pdf},
  size ≤ 10 MB). Reuse the `TeacherOwnsSubject` authz check already in `Create`/`Update`.
- Upload via `IMinioStorageService` (inject it; set bucket from `MinioStorageConfig:DefaultBucket`)
  using object name `questions/{id}/{guid}{ext}` → persist returned URL to `Question.ImageUrl`
  via `QuestionService.UpdateAsync`.
- Scope decision: ship the single-URL route first (writes `Question.ImageUrl`). The full
  `QuestionAttachment` table from `CONTEXT.md` is deferred — note as a follow-up; do NOT add a
  migration in this phase unless multi-file is required.

**Verify:** upload a PNG to a question owned by the teacher → 200 with URL; the URL is
reachable in MinIO and stored on the question; oversize / wrong-type → 400.

## Phase 5 — Bulk Excel import

`BulkImportQuestionRequest` DTO exists; no endpoint/service and no Excel package.

- Add `EPPlus` (or `ClosedXML`) to `Directory.Packages.props` + `ExamHub.Core.csproj`.
- New `IBulkImportService` + impl in `Infrastructure/.../Services/Implementations`: parse the
  uploaded `.xlsx` into rows, validate each (topic/type/difficulty/Bloom ids, ≥1 correct answer),
  collect per-row errors, then insert valid questions inside one transaction
  (`ICommonRepository.ExecuteInTransactionAsync`) reusing `QuestionService.CreateAsync`.
- Endpoint `POST /api/questions/bulk-import` (`IFormFile`, `[Authorize(Roles="Admin,Teacher")]`)
  returning a summary `{ imported, failed, errors[] }`.

**Verify:** upload a sample workbook with one bad row → response reports N imported, 1 failed
with row/reason; imported questions appear via `GET /api/questions`.

## Phase 6 — Export PDF / Word (reuse MinIO)

Only the `IExportService` interface exists (spec §7 `GET /exams/{id}/export?format=pdf|docx`).

- Add `QuestPDF` for PDF. For Word use `DocumentFormat.OpenXml` (note: `context.md` lists
  ClosedXML for Word, but ClosedXML is Excel-only — flag this doc error; confirm Word format
  during implementation).
- Implement `ExportService` (new file under `Infrastructure/.../Services/Implementations`,
  register in `DependencyContainer.AddAppServices`): load exam via
  `IExamService.GetWithQuestionsAsync`, render questions + answer snapshots, write to a stream,
  upload to MinIO `exports/{examId}.{ext}` via `IMinioStorageService.UploadStreamAsync`, return URL.
- Endpoint `GET /api/exams/{id}/export?format=pdf|docx` in `ExamController.cs`.

**Verify:** `GET /api/exams/{id}/export?format=pdf` returns a URL to a valid PDF containing the
exam's snapshot questions; `format=docx` returns a valid Word doc.

## Phase 7 — Redis question-pool cache (optimization, spec §10/§13)

Generation currently hits Postgres for every section. NFR is <2s on 10k+ questions.

- In `QuestionRepository.PickRandomAsync` (or a thin wrapper), cache the candidate ID pool keyed
  `qpool:{topicId}:{diffId}:{typeId}:{cogId|"all"}`, TTL 2 min, via `IRedisService`. Cache IDs only.
- Invalidate matching keys on question create/update/delete/verify in `QuestionService`.

**Verify:** repeated generation against the same template hits the cache (log/metric); editing a
question invalidates its pool keys.

## Phase 8 — Role-based navigation menu endpoint (NEW, 2026-06-13)

The frontend sidebar (`exam_hub_web/src/layouts/AppLayout.tsx`) hardcodes `NAV_ITEMS` and shows
every item to every authenticated user. Roles already ride in the JWT (parsed by
`CurrentUserInfo.Roles` / `User.GetRoles()`), but nothing filters navigation. Add a single endpoint
that returns the nav items the caller's roles are allowed to see — one authoritative menu the FE
renders dynamically and reuses for route guarding.

**Approach:** pure API-layer feature — the menu is static config, not domain data, so no
`ExamHub.Core` service or DB table. Keep it in `ExamHub.API`.

- **DTO** `MenuItemResponse` (new, `ExamHub.API/.../Menu/MenuItemResponse.cs`):
  `{ string Key, string Label, string Path, string Icon, int Order }`. `Icon` is a **string key**
  (e.g. `"dashboard"`, `"school"`) — React components can't serialize; the FE maps the key to an
  AntD icon (FE-8). Do **not** leak the allowed-roles list in the response.
- **Menu registry** (new static class `MenuRegistry` in the same folder): the full ordered item list,
  each tagged with `string[] Roles` (which roles may see it). Mirror the current `NAV_ITEMS` plus the
  new School entry. Suggested mapping (confirm during impl):
  | Key | Label | Path | Roles |
  |-----|-------|------|-------|
  | `dashboard` | Tổng quan | `/app/dashboard` | Admin, Teacher, Student |
  | `questions` | Câu hỏi | `/app/questions` | Admin, Teacher |
  | `exams` | Mẫu đề thi | `/app/exams` | Admin, Teacher |
  | `generate` | Sinh đề thi | `/app/generate` | Admin, Teacher |
  | `exam-list` | Đề thi | `/app/exam-list` | Admin, Teacher |
  | `schools` | Quản lý trường | `/app/schools` | Admin |
  | `users` | Người dùng | `/app/users` | Admin |
  | `category` | Danh mục | `/app/category` | Admin |
- **Controller** `MenuController` (new, `ExamHub.API/Controllers/MenuController.cs`) extending
  `AuthorizeControllerBase` so it can read `CurrentUser.Roles`:
  `GET /api/menu` → `RequestResponse<IReadOnlyList<MenuItemResponse>>` filtered to items whose
  `Roles` intersect `CurrentUser.Roles`, ordered by `Order`. `[Authorize]` (any authenticated user).
  Use the `RequestResponse<T>.Success(...)` envelope like every other controller.

**Verify:** log in as Admin → `GET /api/menu` returns all 8 items; log in as a Teacher → `schools`,
`users`, `category` are absent; as Student → only `dashboard` (and any student-visible items).

## Phase 9 — Self-service profile endpoints (NEW, 2026-06-13)

Profile screens (FE-10) need the logged-in user to read **and edit** their own info + change
password. `GET /api/auth/info` (UserInfo) already exists. `UserController` PUT/reset-password are
`[Authorize(Roles="Admin")]` only — not usable for self-service. Add two endpoints in
`AuthController` + `AuthService`/`IAuthService` (reuse `IUserService.FindByNameAsync`/`UpdateAsync`
and `GetPasswordHash(AppCommon.SaltPassHash!)` — same hashing as `Login`).

- **DTOs** (next to `LoginDto`/`RegisterDto` in `ExamHub.Core/DataAccessObjects`):
  `UpdateProfileDto(string DisplayName, string? Email, string? PhoneNumber)`,
  `ChangePasswordDto(string OldPassword, string NewPassword)`.
- **`PUT /api/auth/profile`** `[Authorize]` → `AuthService.UpdateProfile(User.GetUserName(), dto)`:
  `FindByNameAsync` → set `DisplayName`/`PhoneNumber`/`SetEmail` → `userService.UpdateAsync(user)` →
  return updated `UserInfo`.
- **`POST /api/auth/change-password`** `[Authorize]` → verify `user.PasswordHash.Contains(old.GetPasswordHash(...))`
  (same check as `Login`); if ok set new hash + `UpdateAsync`; else `RequestResponse.Error`.

**Verify:** logged in, `PUT /api/auth/profile` updates name/email/phone (re-fetch `info` shows new
values); `change-password` with wrong old → error, correct old → next login uses the new password.

---

## Cross-cutting: docs

- **No automated tests.** This project intentionally has no test project; verify each phase
  manually via the listed `GET/POST` calls / `/docs` (Scalar) against a running stack.
- **Doc drift to fix in `context.md`** (small, do alongside): says .NET 8 (repo is .NET 10);
  "ClosedXML for Word" is wrong; table count predates the School module. `CONTEXT.md` glossary is
  accurate — keep it authoritative for naming.

# Frontend (`exam_hub_web`) — Gap Closure

## FE Context

Stack (actual): **React 19 + Vite + TypeScript + Ant Design v6 + TanStack Query + Zustand +
react-hook-form + zod + TipTap + recharts + react-router 7 + i18next**. The API client is a
hand-rolled `fetch` wrapper in `src/services/requestService.ts` exposing `AuthHttp`/`Http` and an
`ApiResponse<T>` that mirrors the backend `RequestResponse<T>` (`{status, message, data, total}`).

> **Doc drift:** `context.md` §6/§14 say shadcn/ui + Axios — the repo uses **Ant Design + fetch**.
> Fix `context.md` alongside the backend doc-drift item; keep `CONTEXT.md` glossary authoritative.

**Working today (real API):** Auth (`authService`), all Category config pages
(grade/subject/topic/difficulty/questionType/cognitive via `categoryServiceBase` + per-entity
services), User management (`userService`).

**Mock-backed (need wiring):** `QuestionBankPage` + `AddQuestionPage` (`__mocks__/questions`),
`ExamTemplatePage` + `CreateExamTemplatePage` (`__mocks__/templates`), `DashboardPage`
(`__mocks__/dashboardStats`).

**Missing entirely:** exam generation (`/generate` route is a `Placeholder`), exam list/detail/
preview, export buttons, batch/variants view, Student submission wiring (`ExamCoverPage`/
`ExamTakingPage` exist but no `submissionService`), Teacher grading/finalize UI, School
management pages (`schoolService` exists, no page), and a multipart upload path in `requestService`.

## FE-1 — API plumbing (foundation for the rest)

- Add a multipart helper to `src/services/requestService.ts` (e.g. `postForm`) that sends
  `FormData` **without** forcing `Content-Type` (let the browser set the boundary), reusing the
  existing bearer-token logic. Needed by FE-2 (attachment + bulk import).
- Add typed services + `src/types/*.d.ts` mirroring backend DTOs: `questionService`,
  `examService`, `examTemplateService`, `examGeneratorService`, `submissionService`. Follow the
  existing `categoryServiceBase` / `gradeLevelService` pattern and `ApiResponse<T>` shape.

## FE-2 — Question Bank wiring (needs backend Phases 1, 4, 5)

- Replace `__mocks__/questions` in `QuestionBankPage.tsx` with `questionService.getPaged`
  (filters incl. **`cognitiveLevelId`** — backend Phase 1) and wire create/update/delete/verify in
  `AddQuestionPage.tsx`.
- Add attachment upload (backend Phase 4: `POST /api/questions/{id}/attachment`) via the FE-1
  multipart helper, and a bulk-import dialog (backend Phase 5: `POST /api/questions/bulk-import`)
  surfacing the `{successCount, errorCount, errors[]}` summary.

## FE-3 — Exam Templates wiring

- Replace `__mocks__/templates` with `examTemplateService` CRUD in `ExamTemplatePage.tsx` /
  `CreateExamTemplatePage.tsx`; section editor binds Topic + QuestionType + difficulty % +
  optional CognitiveLevel.

## FE-4 — Exam generation, list & export (needs backend Phase 6 for export)

- Implement the `/generate` route (currently `Placeholder` in `src/routes/index.tsx`): single
  generate + batch generate (variant count/naming) via `examGeneratorService`.
- Add exam list + detail/preview (snapshot questions) + variants view via `examService`.
- Add export buttons calling `GET /api/exams/{id}/export?format=pdf|docx` (backend Phase 6),
  opening the returned MinIO URL.

## FE-5 — Student flow + grading (needs backend Phase 2)

- Wire `ExamTakingPage` submit through `submissionService` (`POST /api/exam-submissions`),
  selecting answers by the snapshot answer `id` (the id added to `answers_snapshot` in backend
  Phase 2). Add a results view.
- Teacher grading UI: per-essay-answer grade (`POST .../answers/{id}/grade`) + finalize
  (`POST .../{id}/finalize`).

## FE-6 — Dashboard & analytics

- Replace `__mocks__/dashboardStats` with real aggregates; render exam analytics
  (`GET /api/exams/{id}/analytics`, backend Phase 3) as recharts Bloom/difficulty/topic charts.

## FE-7 — School Management UI (NEW, 2026-06-13) — separate routes

The backend School module is fully built; the frontend has only `schoolService` and **no pages**
(`School` listed under "Missing entirely" above). Build the admin UI as **separate routes per level**
(per decision 2026-06-13), reusing the existing AntD `Table` + `Popconfirm` + form-modal pattern
(`pages/category/grade/index.tsx`, `useCategoryTab.ts`) and the TanStack Query hook style
(`hooks/queries/*`).

**Services** (new under `src/services/`, mirror the controllers; follow `schoolService.ts` /
`categoryServiceBase.ts`):
- `cohortService` — `getBySchool(schoolId)`, `getWithClasses(id)`, `getWithMembers(id)` + CRUD
  (extends `CategoryServiceBase`, base `cohort`).
- `cohortClassService` — `getByCohort(cohortId)`, `getBySchoolYear(year)`,
  `setHomeroomTeacher(id, teacherId)` (`PATCH /api/cohortclass/{id}/homeroom-teacher`).
- `schoolMemberService` — GUID-keyed: `getBySchool(schoolId)`, `getBySchoolAndRole`, `getByUser`,
  `add(body)`, `update(id, body)`, `remove(id)`, `setActive(id, isActive)`.
- `cohortMemberService` — GUID-keyed: `getByCohort(cohortId)`, `getByStudent`, `add(body)`,
  `remove(id)`, `setActive(id, isActive)`.

  > Note: member endpoints are **Guid**-keyed and `cohortMember`/parts require `Admin`; keep the
  > existing `AuthHttp` bearer flow. `schoolMember`/`cohortMember` need a *user picker* — reuse
  > `userService.getAll()` (Admin-only) to choose the `AppUser` to enrol.

**Types** (extend `src/types/school.d.ts`): add `Cohort`/`CohortBody`, `CohortClass`/`CohortClassBody`,
`SchoolMember`/`SchoolMemberBody`, `CohortMember`/`CohortMemberBody` matching the API response DTOs
(`ExamHub.Core/DataTransferObjects/School/*`).

**Query hooks** (new under `src/hooks/queries/`): `useSchools`, `useCohorts(schoolId)`,
`useCohortClasses(cohortId)`, `useSchoolMembers(schoolId)`, `useCohortMembers(cohortId)` with the
matching create/update/delete mutations (model on `useGradeLevels.ts`).

**Routes** (register in `src/routes/index.tsx` under the protected `AppLayout` children; add path
constants to `src/routes/paths.ts`):
- `schools` → `SchoolListPage` — table of schools, CRUD via form modal, row → navigate to detail.
- `schools/:id` → `SchoolDetailPage` — school header + two tabs: **Khoá học** (Cohort list via
  `getBySchool`, CRUD, row → `cohorts/:id`) and **Thành viên trường** (SchoolMember list +
  add/remove/role-change/active toggle via the user picker).
- `cohorts/:id` → `CohortDetailPage` — cohort header + two tabs: **Lớp học** (CohortClass list via
  `getByCohort`, set homeroom teacher) and **Học sinh** (CohortMember list + enrol/remove/active).
- Add an AntD `Breadcrumb` (Trường → Khoá → Lớp) for cross-level navigation.

Guard all three routes with `<ProtectedRoute allowedRoles={['Admin']} />` (the component already
supports `allowedRoles`).

**Verify:** as Admin, create a school → open it → add a cohort → open the cohort → add a class and
enrol a student → set a homeroom teacher; all calls hit the live API and lists refresh. As a
non-Admin, navigating to `/app/schools` redirects to `/forbidden`.

## FE-8 — Role-based menu consumption (NEW, 2026-06-13) — needs backend Phase 8

`AppLayout.tsx` hardcodes `NAV_ITEMS` for everyone. Drive the sidebar from `GET /api/menu`.

- New `menuService.getMenu()` (`GET /api/menu` via `AuthHttp`) returning `MenuItemResponse[]`, and a
  `useMenu()` TanStack Query hook.
- In `AppLayout.tsx`, replace the static `NAV_ITEMS` render with the fetched menu. Map the backend
  `icon` **string key** → AntD icon component via a small lookup object (the icons currently imported:
  `AppstoreOutlined`, `UnorderedListOutlined`, `FileTextOutlined`, `ThunderboltOutlined`,
  `UserOutlined`, `TagsOutlined`, plus a new School icon e.g. `BankOutlined`). Keep the current
  static list as an offline **fallback** if the query errors, so the shell never renders empty.
- Add the new `schools` entry to whatever fallback list remains.
- Type: add `MenuItem` to `src/types/*.d.ts` matching `MenuItemResponse`.

**Verify:** log in as Admin → sidebar shows all items incl. "Quản lý trường"; log in as Teacher →
school/users/category hidden; the menu matches what `GET /api/menu` returns for that role.

## FE-9 — Student exam list / portal home (NEW, 2026-06-13) — no backend change

Students could previously only reach an exam via a direct `?examId=` link (cover → take → result);
there was **no list screen** and **no layout** for the student flow (only `AppLayout` for
admin/teacher existed). All accounts also landed on `/app/dashboard` after login. Add a student home
at `/student/exams` listing published exams with the student's own status + score and filters.

**No backend change** — both endpoints exist: `examService.getPaged({status:'Published'})` and
`submissionService.getByStudent(studentId)`. Merge client-side.

- **`StudentLayout`** (`src/layouts/StudentLayout.tsx`, new — first student-flow layout): header
  (ExamHub logo, student name from `useAuth().user`, logout → `/login`) + `<Outlet/>`; redirects to
  `/login` if not authenticated. Wraps `/student/exams`. The exam-taking pages (`/student/exam`,
  `/student/exam/take`, `/student/exam/result`) stay full-screen outside the layout.
- **`StudentExamListPage`** (`src/pages/student/StudentExamListPage.tsx`, new): reuses the
  `ExamListPage` table/filter pattern. Loads published exams (`pageSize` large) + the student's
  submissions (`useMySubmissionsQuery`, added to `useSubmissions.ts`), maps `examId → latest
  submission`, derives status: no sub → Chưa làm; `InProgress` → Đang làm; `Submitted` → Đã nộp;
  `Graded` → Đã chấm (+ score). Columns incl. **Điểm của tôi**; row action "Vào thi/Tiếp tục"
  (→ cover) or "Xem kết quả" (→ result). Filters: subject, grade, student-status (client-side),
  keyword.
- **Login redirect** (`LoginPage.tsx`): `homePathForRoles(roles)` → Student-only → `/student/exams`,
  else `/app/dashboard` (both `onFinish` and the already-authenticated `useEffect`).
- **`ExamResultPage`**: "Hoàn tất" now → `/student/exams` (was `/login`).
- Routes/paths: `STUDENT_EXAMS: '/student/exams'`; route group under `<StudentLayout/>`.

**Verify:** see the FE verification list below — log in as a Student-only account → lands on
`/student/exams` with status/score per exam; filter works; take an exam → status flips to Đã nộp;
Admin/Teacher still land on `/app/dashboard`; unauthenticated `/student/exams` → `/login`.

## FE-10 — Profile / detail screens, 3 separate per role (NEW, 2026-06-13) — needs Phase 9

Three **separate** profile components, each showing account info + role-specific data, with edit +
change-password. Reuse `useAuth().user` for identity and the existing read endpoints.

- **`authService`** (`src/services/authService.ts`): add `getInfo()` (`GET /Auth/info`),
  `updateProfile(body)` (`PUT /Auth/profile`), `changePassword(body)` (`POST /Auth/change-password`).
  Add a shared `ProfileInfoCard` + `EditProfileModal` + `ChangePasswordModal` (under
  `src/pages/profile/`) reused by all three pages.
- **`StudentProfilePage`** (`/student/profile`, inside `StudentLayout`): account info + enrolled
  cohort/class (`cohortMemberService.getByStudent(user.id)`) + exam stats derived from
  `submissionService.getByStudent` (số đề đã làm, điểm TB của bài đã chấm).
- **`TeacherProfilePage`** (`/app/profile`): account info + assigned subjects
  (`GET /api/teacher-subjects/teacher/{id}`, add `teacherSubjectService`) + schools
  (`schoolMemberService.getByUser(user.id)`).
- **`AdminProfilePage`** (`/app/profile`): account info + system stats (counts from
  `userService.getAll`, `examService.getPaged` total, `schoolService.getAll`, question total).
- **Routing/entry**: `/app/profile` renders `AdminProfilePage` if role Admin else `TeacherProfilePage`
  (single route, role switch — keeps 3 separate components). Add a "Tài khoản" button in the
  `AppLayout` sidebar footer (next to logout) → `/app/profile`; make `StudentLayout` header name/avatar
  clickable → `/student/profile`. Paths in `src/routes/paths.ts`.

**Verify:** each role opens its own profile, sees role-specific data; edit name/email/phone persists
(re-fetch shows new values); change-password with correct old works, wrong old shows error.

## FE verification

`pnpm install` then `pnpm build` (`tsc -b && vite build`) and `pnpm lint` clean. Manually smoke
each wired page against a running API (`globalConfig.apiBaseUrl`): login, list/create a question
with a Bloom filter, generate an exam, take + submit it as a student, finalize, view analytics.

# Suggested sequencing

**Backend:** Phases 1–3 are small, high-value, no new dependencies — land them first. Phase 4 is
quick. Phases 5 and 6 each add a package and are larger; Phase 6 depends only on MinIO (present).
Phase 7 is a pure optimization — do last, after correctness is proven and only if generation
latency warrants it. (Phases 1–5 are already done.)

**Frontend depends on backend endpoints**, so interleave: FE-1 (plumbing) anytime → FE-2 after
BE Phases 1/4/5 (done) → FE-3 anytime → FE-4 after BE Phase 6 → FE-5 after BE Phase 2 (done) →
FE-6 after BE Phase 3 (done). Practically: FE-1, FE-2, FE-3, FE-5, FE-6 are unblocked now; only
FE-4's export buttons wait on BE Phase 6.

**New work (2026-06-13):** **FE-7** (School Management UI) is unblocked now — the backend School
module is complete, so it needs no new backend. **Phase 8** (menu endpoint) is small/standalone and
should land before **FE-8** (menu consumption), which depends on it. Recommended order: Phase 8 →
FE-7 → FE-8 (FE-7 and Phase 8 are independent and can proceed in parallel).

## Global verification

**Backend:** after each phase, `dotnet build` clean, then exercise the new endpoint(s) via `/docs`
(Scalar, dev only) or the listed `GET/POST` calls against a running stack (Postgres + Redis +
MinIO from `compose.yaml`).

**Frontend:** `pnpm build` + `pnpm lint` clean, then smoke the wired pages against a running API.
Full end-to-end: login → create question (Bloom filter) → build template → generate exam → take +
submit as student → finalize → view analytics.
