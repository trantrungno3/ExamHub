# Error response normalization (business-rule validation)

## Problem

`AuthService` (reference pattern) never throws for expected business-rule
failures — every method returns `RequestResponse<T>` directly
(`RequestResponse<T>.Error("message")` on failure), and `AuthController`
just does `return Ok(await service.X(dto))`. This serializes to
`{status, message, data, totalRecord}`, which the frontend's
`ApiResponse<T>` (`{status, message, data, total}`) parses correctly.

Elsewhere in the backend (`ExamSessionService`, `CohortMemberService`,
`CohortClassTeacherService`, `ExportService`), business-rule violations are
raised as `throw new InvalidOperationException(...)`. These are caught by
the global `CustomExceptionHandler` (shared `trungtv_core` middleware),
which writes a *different* JSON shape: `{statusCode, message, details,
traceId, timestamp, errors}`.

The frontend's `handleResponse` does `JSON.parse(text) as ApiResponse<T>`
with no shape check. For an exception-shaped body, `res.status` comes back
`undefined` (the field is named `statusCode`, not `status`):

- Hooks that also check `!res.data` (e.g. `startAndGo` in
  `StudentSessionListPage`) still land in the error branch, but show a
  mangled message (`"Invalid operation: <msg>"` — English prefix baked in
  by `CustomExceptionHandler.MapInvalidOperationException`).
- Hooks that check **only** `res.status === statusCode.Error` (the
  majority — `useCreateExamSessionMutation`, `usePublishSessionMutation`,
  `useCloseSessionMutation`, `useSetSessionExamsMutation`,
  `useAddAssignmentMutation`, `useRemoveAssignmentMutation`,
  `useDeleteExamSessionMutation`, etc.) treat `undefined !== 0` as
  **success** and show a false "Đã tạo/Đã cập nhật..." toast even though
  the backend rejected the request. Silent-false-success is the worst
  instance of this bug.

## Decision

Rewrite the throwing methods to follow the `AuthService` pattern: return
`RequestResponse<T>.Error(...)` instead of throwing, for every
**expected/business-rule** violation (not found, invalid state, validation
failure). Keep exceptions only for truly exceptional conditions (already
the case — none identified in scope need to stay exceptional; see
Out-of-scope below for the one deliberately-excluded case).

Rejected alternative: normalize the exception JSON shape once in the
frontend's `handleResponse` (smaller diff, zero backend risk). Not chosen —
the user wants the codebase consistently following the `AuthService`
contract rather than tolerating two response shapes at the HTTP boundary.

## Scope

| Service / Interface | Methods to convert | Shared private helpers to convert |
|---|---|---|
| `IExamSessionService` / `ExamSessionService` | `CreateAsync`, `UpdateAsync`, `PublishAsync`, `SetExamsAsync`, `AddAssignmentAsync`, `StartAsync` (~25 throw sites total) | — |
| `ICohortMemberService` / `CohortMemberService` | `AddStudentAsync`, `SetSectionAsync` | `ValidateSectionAsync` (called by both) |
| `ICohortClassTeacherService` / `CohortClassTeacherService` | `AssignAsync` | — |
| `IExportService` / `ExportService` | `ExportPdfAsync`, `ExportDocxAsync` | `LoadExamAsync`, `UploadAsync` (called by both) |

Controllers to update to drop try/catch (where present) or simplify to a
`RequestResponse` pass-through (where absent): `ExamSessionController`
(Create/Update/Publish/SetExams/AddAssignment/Start — 6 actions),
`CohortMemberController` (AddStudent/SetSection), `CohortClassTeacherController`
(Assign), `ExamController` (Export — the `switch` expression needs
restructuring since the branches now each return `RequestResponse<string>`
instead of `string`).

### Out of scope

- `ExamSubmissionService.SaveProgressAsync` — throws `InvalidOperationException`
  / `UnauthorizedAccessException`, but `ExamSubmissionController.SaveProgress`
  already wraps the call in try/catch and maps to the correct
  `RequestResponse` + HTTP status (409/403). Not broken today; explicitly
  left as-is per user decision (avoid unnecessary diff).
- `DependencyContainer.cs:59` (`?? throw new InvalidOperationException(...)`)
  — startup-time configuration validation, not part of a request pipeline.
  Leave as a fail-fast throw.
- `ExamHub.Tests/SubmissionFlowTests.cs:85` — test helper code, not part of
  the API surface.

## Conversion pattern

Applied uniformly to every throw site in scope:

- Return type: `Task<X>` → `Task<RequestResponse<X>>`. A currently-`void`
  `Task` method (e.g. `UpdateAsync`, `SetExamsAsync`) becomes
  `Task<RequestResponse<bool>>`, returning `RequestResponse<bool>.Success(msg, true, 1)`
  on the happy path.
- `throw new InvalidOperationException(msg);` → `return RequestResponse<X>.Error(msg);`
- `x ?? throw new InvalidOperationException(msg)` → split into an explicit
  null check with an early `return RequestResponse<X>.Error(msg);`.
- A private helper called from multiple public methods and currently
  throwing (`ValidateSectionAsync`, `LoadExamAsync`, `UploadAsync`) is
  converted to return `RequestResponse<X>` (or `(bool Ok, string? Error, X?
  Value)` if returning a non-`RequestResponse` value is more convenient);
  every caller propagates the error with an early return instead of letting
  an exception unwind.
- Controllers: once the service returns `RequestResponse<T>` on both paths,
  the action body collapses to `return Ok(await service.X(...));` — same
  as `AuthController`. Drop any existing try/catch around the call (only
  `CohortMemberController.AddStudent` has one today). `ExamController.Export`'s
  `switch` needs its branches adjusted to unwrap `RequestResponse<string>.Data`
  on success / pass the `RequestResponse<object>.Error` through on failure.
- HTTP status stays 200 for these business-rule failures (matching
  `AuthController`'s convention — the frontend already reads `status` from
  the JSON body, not the HTTP status code, for this class of error).
  Existing explicit `NotFound()`/`Conflict()`/`StatusCode(403/401)` results
  that controllers already return *before* calling the service (ownership
  / existence checks done in the controller itself) are untouched.

## Frontend impact

None required. `useMySessionsQuery`, `useCreateExamSessionMutation`, etc.
already check `res.status === statusCode.Error`; once the backend actually
returns that shape instead of an exception-shaped body, the existing
(currently dead) error branches start firing correctly.

## Test impact

Grep `ExamHub.Tests` for `ThrowsAsync<InvalidOperationException>` (or
similar) against any of the converted methods and update those assertions
to check `result.Status == RequestResponseStatus.Error` instead. Exact
list to be confirmed during implementation (`SubmissionFlowTests.cs` is
known to reference `ExamSessionService`/`SubmissionAttempts` behavior and
must be checked).

## Risks

- Mechanical but has ~35 call sites across 4 files — highest risk is
  missing a call site or a private-helper caller that still assumes an
  exception (compiler will catch signature mismatches; logic mismatches
  won't be caught by the compiler, so each converted method needs a
  read-through against its original throw conditions).
- Loses `CustomExceptionHandler`'s centralized logging (RabbitMQ/Kafka) for
  the converted call sites specifically — these were "expected" business
  rejections, not real errors, so this is an accepted trade-off, not a
  regression (the AuthService reference pattern has always worked this way).
