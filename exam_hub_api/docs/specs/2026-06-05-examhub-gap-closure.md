# ExamHub — Next-Phase Plan (Backend + Frontend Gap Closure to Spec)

> Covers two repos: **`exam_hub_api`** (.NET 10 backend) and **`exam_hub_web`**
> (React 19 + Vite + TypeScript frontend). Backend phases are numbered `Phase 1..7`;
> frontend phases are numbered `FE-1..FE-6` and depend on the matching backend endpoints.

## Progress (updated 2026-06-06)

> **Status: all phases complete.** Backend Phases 1–7 + `IBaseRepository` migration of
> `PickRandomAsync` + `context.md` doc-drift fixes done; `dotnet build` clean. Frontend FE-1–FE-6
> wired to the real API; `tsc -b` + `vite build` clean, no new lint errors, `__mocks__` removed.
> Testing was removed from scope per request. Remaining: commit the working-tree changes (both repos
> are uncommitted; API on `feature/phase-1-foundation`) and run the manual end-to-end smoke against a
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

## Global verification

**Backend:** after each phase, `dotnet build` clean, then exercise the new endpoint(s) via `/docs`
(Scalar, dev only) or the listed `GET/POST` calls against a running stack (Postgres + Redis +
MinIO from `compose.yaml`).

**Frontend:** `pnpm build` + `pnpm lint` clean, then smoke the wired pages against a running API.
Full end-to-end: login → create question (Bloom filter) → build template → generate exam → take +
submit as student → finalize → view analytics.
