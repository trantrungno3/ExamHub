# Cập nhật hệ thống khớp đặc tả ca sử dụng — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đóng 6 điểm lệch giữa code và `uml/dac-ta-ca-su-dung.md`: trạng thái "Chờ chấm tay" (UC34), tự lưu tạm bài làm server-side (UC33), chặn học sinh trùng lớp cùng khối (UC13), và cảnh báo xoá-khi-đang-dùng + xoá bắt buộc (UC5/10/18/24/43).

**Architecture:** Backend .NET nhiều tầng (Controller → Service → Repository, EF Core + Postgres) trong `exam_hub_api`; frontend React + AntD trong `exam_hub_web`. Mỗi thay đổi đi xuyên các tầng tương ứng. Logic quyết định trạng thái chấm được tách thành hàm thuần để unit-test không cần DB.

**Tech Stack:** C# / .NET, EF Core, PostgreSQL, xUnit (mới thêm); React + TypeScript + AntD + Vite; axios services.

**Spec:** `docs/superpowers/specs/2026-08-21-usecase-alignment-design.md`

## Global Constraints

- Enum `SubmissionStatusEnum` là `byte`; giá trị mới PHẢI = `4` (giữ nguyên `InProgress=1, Submitted=2, Graded=3` để không phá dữ liệu cũ).
- KHÔNG backfill dữ liệu submission cũ.
- Thông báo lỗi cho người dùng bằng tiếng Việt, đi qua `RequestResponse<T>.Error(...)`.
- Exception nghiệp vụ được ném ở tầng Service, bắt ở Controller và map sang mã HTTP cụ thể (theo pattern `InsufficientQuestionsException` trong `ExamGeneratorController`).
- Autosave KHÔNG đổi trạng thái và KHÔNG chấm điểm.
- Câu tự luận nhận diện bằng snapshot: `AnswersSnapshot` không có đáp án đúng nào (rỗng/null) ⇒ câu chấm tay.
- Mỗi task kết thúc bằng deliverable test/verify được độc lập; commit sau mỗi task.

---

## Phase 0 — Hạ tầng test

### Task 0: Tạo test project xUnit cho ExamHub.Core

**Files:**
- Create: `exam_hub_api/ExamHub.Tests/ExamHub.Tests.csproj`
- Create: `exam_hub_api/ExamHub.Tests/GradingStatusTests.cs` (placeholder rỗng, điền ở Task A2)
- Modify: solution file `exam_hub_api/*.sln` hoặc `.slnx` (thêm project reference)

**Interfaces:**
- Produces: test project chạy được bằng `dotnet test`, tham chiếu `ExamHub.Core`.

- [ ] **Step 1: Tạo project và tham chiếu**

```bash
cd exam_hub_api
dotnet new xunit -n ExamHub.Tests -o ExamHub.Tests
dotnet add ExamHub.Tests/ExamHub.Tests.csproj reference ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj
```

(Nếu đường dẫn csproj của Core khác, dùng đường dẫn thực tế: chạy `find . -name ExamHub.Core.csproj -not -path "*obj*"` để xác định.)

- [ ] **Step 2: Thêm project vào solution**

```bash
dotnet sln list
dotnet sln add ExamHub.Tests/ExamHub.Tests.csproj
```

- [ ] **Step 3: Xoá file mẫu và verify build**

```bash
rm -f ExamHub.Tests/UnitTest1.cs
dotnet test ExamHub.Tests/ExamHub.Tests.csproj
```

Expected: build thành công, 0 test chạy (chưa có test).

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.Tests
git commit -m "test: add ExamHub.Tests xUnit project"
```

---

## Phase A — UC34: Trạng thái "Chờ chấm tay"

### Task A1: Thêm giá trị enum PendingManualGrade

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Enums/SubmissionStatusEnum.cs`

**Interfaces:**
- Produces: `SubmissionStatusEnum.PendingManualGrade = 4`.

- [ ] **Step 1: Thêm giá trị enum**

Trong `SubmissionStatusEnum.cs`, thêm sau `Submitted`:

```csharp
    /// <summary>Đã nộp</summary>
    Submitted = 2,

    /// <summary>Đã chấm điểm</summary>
    Graded = 3,

    /// <summary>Chờ giáo viên chấm tay (có câu tự luận)</summary>
    PendingManualGrade = 4
```

- [ ] **Step 2: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj`
Expected: build thành công.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Enums/SubmissionStatusEnum.cs
git commit -m "feat: add PendingManualGrade submission status"
```

### Task A2: Hàm thuần quyết định trạng thái sau chấm tự động (TDD)

**Files:**
- Create: `exam_hub_api/ExamHub.Core/Application/Grading/SubmissionGrading.cs`
- Test: `exam_hub_api/ExamHub.Tests/GradingStatusTests.cs`

**Interfaces:**
- Produces:
  - `static bool SubmissionGrading.HasManualGradeQuestion(IEnumerable<ExamQuestion> examQuestions)` — true nếu tồn tại câu không có đáp án đúng nào trong snapshot (câu tự luận).
  - `static HashSet<Guid> SubmissionGrading.CorrectAnswerIds(string? answersSnapshotJson)` — trích tập UUID đáp án đúng (chuyển từ private của `ExamSubmissionService`).
  - `static SubmissionStatusEnum SubmissionGrading.DecideStatus(IEnumerable<ExamQuestion> examQuestions)` — `PendingManualGrade` nếu có câu tự luận, ngược lại `Graded`.

- [ ] **Step 1: Viết test thất bại**

`GradingStatusTests.cs`:

```csharp
using ExamHub.Core.Application.Grading;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;
using Xunit;

public class GradingStatusTests
{
    private static ExamQuestion Q(string? answersSnapshot) => new()
    {
        ContentSnapshot = "x",
        AnswersSnapshot = answersSnapshot
    };

    private const string ObjectiveSnap =
        "[{\"id\":\"11111111-1111-1111-1111-111111111111\",\"is_correct\":true}]";

    [Fact]
    public void CorrectAnswerIds_parses_only_correct()
    {
        var ids = SubmissionGrading.CorrectAnswerIds(ObjectiveSnap);
        Assert.Single(ids);
    }

    [Fact]
    public void DecideStatus_all_objective_is_Graded()
    {
        var status = SubmissionGrading.DecideStatus(new[] { Q(ObjectiveSnap), Q(ObjectiveSnap) });
        Assert.Equal(SubmissionStatusEnum.Graded, status);
    }

    [Fact]
    public void DecideStatus_with_essay_is_PendingManualGrade()
    {
        var status = SubmissionGrading.DecideStatus(new[] { Q(ObjectiveSnap), Q(null) });
        Assert.Equal(SubmissionStatusEnum.PendingManualGrade, status);
    }
}
```

- [ ] **Step 2: Chạy test để xác nhận fail**

Run: `dotnet test exam_hub_api/ExamHub.Tests/ExamHub.Tests.csproj`
Expected: FAIL (không compile — `SubmissionGrading` chưa tồn tại).

- [ ] **Step 3: Tạo hàm thuần**

`SubmissionGrading.cs`:

```csharp
using System.Text.Json;
using ExamHub.Core.Domain.Entities;
using ExamHub.Core.Domain.Enums;

namespace ExamHub.Core.Application.Grading;

/// <summary>Logic thuần quyết định chấm điểm — không phụ thuộc DB, unit-test được.</summary>
public static class SubmissionGrading
{
    /// <summary>Trích tập UUID đáp án đúng từ snapshot JSON [{id, is_correct, ...}].</summary>
    public static HashSet<Guid> CorrectAnswerIds(string? answersSnapshotJson)
    {
        var result = new HashSet<Guid>();
        if (string.IsNullOrWhiteSpace(answersSnapshotJson)) return result;

        using var doc = JsonDocument.Parse(answersSnapshotJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Array) return result;

        foreach (var el in doc.RootElement.EnumerateArray())
            if (el.TryGetProperty("is_correct", out var ic) && ic.ValueKind == JsonValueKind.True &&
                el.TryGetProperty("id", out var idEl) && idEl.TryGetGuid(out var id))
                result.Add(id);
        return result;
    }

    /// <summary>Câu chấm tay = không có đáp án đúng nào trong snapshot (tự luận).</summary>
    public static bool HasManualGradeQuestion(IEnumerable<ExamQuestion> examQuestions)
        => examQuestions.Any(eq => CorrectAnswerIds(eq.AnswersSnapshot).Count == 0);

    /// <summary>PendingManualGrade nếu có câu tự luận, ngược lại Graded.</summary>
    public static SubmissionStatusEnum DecideStatus(IEnumerable<ExamQuestion> examQuestions)
        => HasManualGradeQuestion(examQuestions)
            ? SubmissionStatusEnum.PendingManualGrade
            : SubmissionStatusEnum.Graded;
}
```

- [ ] **Step 4: Chạy test để xác nhận pass**

Run: `dotnet test exam_hub_api/ExamHub.Tests/ExamHub.Tests.csproj`
Expected: PASS (3 test).

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Application/Grading/SubmissionGrading.cs exam_hub_api/ExamHub.Tests/GradingStatusTests.cs
git commit -m "feat: add pure grading-status decision helper with tests"
```

### Task A3: Áp dụng trạng thái mới trong ExamSubmissionService

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs`

**Interfaces:**
- Consumes: `SubmissionGrading.CorrectAnswerIds`, `SubmissionGrading.DecideStatus`, `IExamQuestionRepository.GetByExamAsync`.
- Produces: sau `SubmitAsync`/`SubmitInProgressAsync`, `submission.Status` = `Graded` (toàn trắc nghiệm) hoặc `PendingManualGrade` (có tự luận).

- [ ] **Step 1: Refactor chấm tự động dùng danh sách examQuestions nạp sẵn**

Thay `AutoGradeObjectiveAsync(examId, answers, ct)` bằng: nạp examQuestions một lần rồi dùng cho cả chấm và quyết định trạng thái. Trong `SubmitAsync`, thay đoạn:

```csharp
        await AutoGradeObjectiveAsync(submission.ExamId, answerList, ct);

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

        return submission;
```

bằng:

```csharp
        var examQuestions = await _examQuestionRepo.GetByExamAsync(submission.ExamId, ct);
        ApplyAutoGrade(examQuestions, answerList);
        submission.TotalScore = answerList.Sum(a => a.ScoreEarned);
        submission.Status     = SubmissionGrading.DecideStatus(examQuestions);
        await _submissionRepo.UpdateAsync(submission, ct);

        if (answerList.Count > 0)
            await _answerRepo.AddRangeAsync(answerList, ct);

        return submission;
```

Lưu ý: `SubmitAsync` đã `AddAsync(submission)` với `Status = Submitted` trước đó — giữ AddAsync, sau đó UpdateAsync để set trạng thái đã quyết định (đơn giản, ít rủi ro). Thêm `using ExamHub.Core.Application.Grading;`.

- [ ] **Step 2: Cập nhật SubmitInProgressAsync**

Thay đoạn:

```csharp
        await AutoGradeObjectiveAsync(existing.ExamId, answerList, ct);
        existing.TotalScore = answerList.Sum(a => a.ScoreEarned);

        await _submissionRepo.UpdateAsync(existing, ct);
```

bằng:

```csharp
        var examQuestions = await _examQuestionRepo.GetByExamAsync(existing.ExamId, ct);
        ApplyAutoGrade(examQuestions, answerList);
        existing.TotalScore = answerList.Sum(a => a.ScoreEarned);
        existing.Status     = SubmissionGrading.DecideStatus(examQuestions);

        await _submissionRepo.UpdateAsync(existing, ct);
```

- [ ] **Step 3: Thay AutoGradeObjectiveAsync bằng ApplyAutoGrade (đồng bộ, nhận examQuestions)**

Thay method `AutoGradeObjectiveAsync` và giữ private helper CorrectAnswerIds bằng cách gọi `SubmissionGrading.CorrectAnswerIds`:

```csharp
    /// <summary>Chấm tự động câu trắc nghiệm dựa trên danh sách examQuestions đã nạp.</summary>
    private static void ApplyAutoGrade(
        IReadOnlyList<ExamQuestion> examQuestions, IReadOnlyList<SubmissionAnswer> answers)
    {
        var byId = examQuestions.ToDictionary(eq => eq.Id);
        foreach (var answer in answers)
        {
            if (answer.SelectedAnswerIds is not { Length: > 0 } selected) continue;
            if (!byId.TryGetValue(answer.ExamQuestionId, out var examQuestion)) continue;

            var correctIds = SubmissionGrading.CorrectAnswerIds(examQuestion.AnswersSnapshot);
            var isCorrect  = correctIds.Count > 0 && correctIds.SetEquals(selected);
            answer.IsCorrect   = isCorrect;
            answer.ScoreEarned = isCorrect ? examQuestion.Score ?? 1m : 0m;
        }
    }
```

Xoá method `AutoGradeObjectiveAsync` và private `CorrectAnswerIdsFromSnapshot` cũ (đã chuyển sang `SubmissionGrading`).

- [ ] **Step 4: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj`
Expected: build thành công, không còn tham chiếu tới method đã xoá.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs
git commit -m "feat: set PendingManualGrade/Graded after auto-grading"
```

### Task A4: Cập nhật frontend hiển thị trạng thái mới

**Files:**
- Modify: `exam_hub_web/src/types/submission.d.ts:3`
- Modify: `exam_hub_web/src/constants/index.ts` (4 map SUBMISSION_STATUS_*)
- Modify: `exam_hub_web/src/pages/exams/SubmissionListPage.tsx` (bộ lọc "cần chấm")

**Interfaces:**
- Consumes: chuỗi `status` từ API (`"PendingManualGrade"`).
- Produces: nhãn/màu cho trạng thái mới; filter danh sách chấm bài dùng `PendingManualGrade`.

- [ ] **Step 1: Mở rộng type**

`submission.d.ts` dòng 3:

```ts
type SubmissionStatus = 'InProgress' | 'Submitted' | 'PendingManualGrade' | 'Graded'
```

- [ ] **Step 2: Cập nhật nhãn/màu trong constants/index.ts**

```ts
export const SUBMISSION_STATUS_LABEL: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Đã nộp', PendingManualGrade: 'Chờ chấm tay', Graded: 'Đã chấm',
}
export const SUBMISSION_STATUS_LABEL_STUDENT: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Đã nộp', PendingManualGrade: 'Đã nộp (chờ chấm)', Graded: 'Đã chấm',
}
export const SUBMISSION_STATUS_VARIANT: Record<SubmissionStatus, StatusVariant> = {
    InProgress: 'default', Submitted: 'default', PendingManualGrade: 'warning', Graded: 'success',
}
export const SUBMISSION_STATUS_TAG_COLOR: Record<SubmissionStatus, string> = {
    InProgress: 'default', Submitted: 'default', PendingManualGrade: 'gold', Graded: 'green',
}
```

- [ ] **Step 3: Bộ lọc "cần chấm" dùng PendingManualGrade**

Trong `SubmissionListPage.tsx`, tìm nơi lọc bài chờ chấm (hiện dùng `'Submitted'`) và đổi sang `'PendingManualGrade'`. Nếu có filter option list, thêm nhãn "Chờ chấm tay". (Chạy `grep -n "Submitted" exam_hub_web/src/pages/exams/SubmissionListPage.tsx` để tìm.)

- [ ] **Step 4: Verify build & lint**

Run: `cd exam_hub_web && npm run build`
Expected: build TS thành công, không lỗi type (Record phải phủ hết 4 nhánh).

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/src/types/submission.d.ts exam_hub_web/src/constants/index.ts exam_hub_web/src/pages/exams/SubmissionListPage.tsx
git commit -m "feat(web): show PendingManualGrade submission status"
```

---

## Phase B — UC33: Tự lưu tạm bài làm (server-side)

### Task B1: Repository upsert đáp án tạm (replace-all)

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamRepositories.cs` (interface `ISubmissionAnswerRepository`)
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/SubmissionAnswerRepository.cs`

**Interfaces:**
- Consumes: `DeleteBySubmissionAsync` (đã có).
- Produces: `Task ReplaceForSubmissionAsync(Guid submissionId, IReadOnlyList<SubmissionAnswer> answers, CancellationToken ct)` — xoá toàn bộ đáp án cũ của submission rồi thêm mới, trong 1 transaction.

- [ ] **Step 1: Khai báo interface**

Trong `ISubmissionAnswerRepository`, thêm:

```csharp
    /// <summary>Thay toàn bộ đáp án của một bài nộp (dùng cho lưu tạm định kỳ).</summary>
    Task ReplaceForSubmissionAsync(Guid submissionId, IReadOnlyList<SubmissionAnswer> answers, CancellationToken ct = default);
```

- [ ] **Step 2: Triển khai**

Trong `SubmissionAnswerRepository.cs` (dùng EF `Db`, `Set`, `ExecuteDeleteAsync` như `ExamQuestionRepository`):

```csharp
    /// <inheritdoc/>
    public async Task ReplaceForSubmissionAsync(
        Guid submissionId, IReadOnlyList<SubmissionAnswer> answers, CancellationToken ct = default)
    {
        await Set.Where(a => a.SubmissionId == submissionId).ExecuteDeleteAsync(ct);
        if (answers.Count > 0)
        {
            foreach (var a in answers) a.SubmissionId = submissionId;
            await Set.AddRangeAsync(answers, ct);
            await Db.SaveChangesAsync(ct);
        }
    }
```

- [ ] **Step 3: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj`
Expected: build thành công.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamRepositories.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/SubmissionAnswerRepository.cs
git commit -m "feat: add ReplaceForSubmissionAsync for autosave"
```

### Task B2: Service SaveProgressAsync (không đổi trạng thái, không chấm)

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamServices.cs` (interface `IExamSubmissionService`)
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs`

**Interfaces:**
- Consumes: `IExamSubmissionRepository.GetByIdAsync`, `ISubmissionAnswerRepository.ReplaceForSubmissionAsync`.
- Produces: `Task SaveProgressAsync(Guid submissionId, IEnumerable<SubmissionAnswer> answers, CancellationToken ct)` — chỉ áp dụng cho bản `InProgress`; ném `InvalidOperationException` nếu không tồn tại/không phải InProgress.

- [ ] **Step 1: Khai báo interface**

Trong `IExamSubmissionService`, thêm:

```csharp
    /// <summary>Lưu tạm đáp án cho bài đang làm (InProgress) — không đổi trạng thái, không chấm.</summary>
    Task SaveProgressAsync(Guid submissionId, IEnumerable<SubmissionAnswer> answers, CancellationToken ct = default);
```

- [ ] **Step 2: Triển khai**

Trong `ExamSubmissionService`:

```csharp
    public async Task SaveProgressAsync(
        Guid submissionId, IEnumerable<SubmissionAnswer> answers, CancellationToken ct = default)
    {
        var existing = await _submissionRepo.GetByIdAsync(submissionId, ct)
            ?? throw new InvalidOperationException("Không tìm thấy bài làm.");
        if (existing.Status != SubmissionStatusEnum.InProgress)
            throw new InvalidOperationException("Bài làm đã nộp, không thể lưu tạm.");

        var list = answers.Select(a =>
        {
            a.Id           = Guid.NewGuid();
            a.SubmissionId = submissionId;
            a.EssayContent = a.EssayContent?.Trim();
            return a;
        }).ToList();

        await _answerRepo.ReplaceForSubmissionAsync(submissionId, list, ct);
    }
```

- [ ] **Step 3: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj`
Expected: build thành công.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamServices.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSubmissionService.cs
git commit -m "feat: add SaveProgressAsync service method"
```

### Task B3: Endpoint PUT /api/exam-submissions/{id}/progress

**Files:**
- Modify: `exam_hub_api/ExamHub.API/Controllers/Exam/ExamSubmissionController.cs`

**Interfaces:**
- Consumes: `IExamSubmissionService.SaveProgressAsync`; DTO `ExamSubmissionRequest.ToAnswers()` (đã có) — nhưng body chỉ cần danh sách answers; tái dùng `SubmissionAnswerRequest`.
- Produces: `PUT api/exam-submissions/{id:guid}/progress` — 200 khi lưu; 409 khi bài không ở InProgress.

- [ ] **Step 1: Thêm action**

```csharp
    /// <summary>Lưu tạm bài làm (autosave) cho bản InProgress — không đổi trạng thái, không chấm.</summary>
    [HttpPut("{id:guid}/progress")]
    public async Task<ActionResult<RequestResponse<bool>>> SaveProgress(
        Guid id, [FromBody] IEnumerable<SubmissionAnswerRequest> answers, CancellationToken ct)
    {
        try
        {
            await service.SaveProgressAsync(id, answers.Select(a => a.ToEntity()), ct);
            return Ok(RequestResponse<bool>.Success("Đã lưu tạm bài làm.", true, 1));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
    }
```

- [ ] **Step 2: Verify build API**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`
Expected: build thành công.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.API/Controllers/Exam/ExamSubmissionController.cs
git commit -m "feat: add autosave progress endpoint"
```

### Task B4: Frontend gọi autosave định kỳ + khôi phục đáp án

**Files:**
- Modify: `exam_hub_web/src/services/submissionService.ts`
- Modify: `exam_hub_web/src/pages/student/ExamTakingPage.tsx`

**Interfaces:**
- Consumes: `PUT /exam-submissions/{id}/progress`.
- Produces: `submissionService.saveProgress(submissionId, answers)`; autosave mỗi ~20s + khôi phục đáp án khi vào lại.

- [ ] **Step 1: Thêm hàm service**

Trong `submissionService.ts` (theo pattern các hàm hiện có, dùng axios instance của dự án):

```ts
export function saveProgress(submissionId: string, answers: SubmissionAnswerInput[]) {
    return http.put(`/exam-submissions/${submissionId}/progress`, answers)
}
```

(Điều chỉnh tên axios instance/`SubmissionAnswerInput` theo type đã có trong file; nếu chưa có type input, dùng dạng `{ examQuestionId: string; selectedAnswerIds?: string[]; essayContent?: string }[]`.)

- [ ] **Step 2: Autosave định kỳ trong ExamTakingPage**

Trong `ExamTakingPage.tsx`, thêm `useEffect` chạy khi có `submissionId` (bản InProgress của kỳ thi), gọi `saveProgress` mỗi 20s với đáp án hiện tại. Chỉ bật khi luồng kỳ thi có `submissionId` (luồng đề trực tiếp không có bản InProgress thì bỏ qua):

```tsx
useEffect(() => {
    if (!submissionId) return
    const id = setInterval(() => {
        saveProgress(submissionId, toAnswerPayload(answersState)).catch(() => {})
    }, 20000)
    return () => clearInterval(id)
}, [submissionId, answersState])
```

`toAnswerPayload` map state đáp án hiện tại sang mảng payload (tái dùng logic đã dùng khi Nộp bài).

- [ ] **Step 3: Khôi phục đáp án khi vào lại**

Khi bắt đầu làm bài, nếu submission InProgress trả về đã có `answers` (từ `GetById`/pool), nạp vào `answersState`. Nếu API start hiện chưa trả answers, gọi `submissionService.getById(submissionId)` để lấy `answers` rồi seed state. (Kiểm tra response `StartSessionResponse` xem có sẵn answers không; nếu không, thêm 1 lần GET.)

- [ ] **Step 4: Verify build web**

Run: `cd exam_hub_web && npm run build`
Expected: build thành công.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/src/services/submissionService.ts exam_hub_web/src/pages/student/ExamTakingPage.tsx
git commit -m "feat(web): periodic autosave and answer restore during exam"
```

### Task B5: Verify E2E autosave (thủ công)

- [ ] **Step 1: Chạy hệ thống**

Khởi động API + web (theo README). Đăng nhập học sinh, vào một kỳ thi đang mở, bắt đầu làm bài.

- [ ] **Step 2: Kiểm tra autosave**

Trả lời vài câu, chờ >20s, mở DevTools Network xác nhận có `PUT .../progress` trả 200. Reload trang, vào lại bài — đáp án đã trả lời được khôi phục.

- [ ] **Step 3: Ghi nhận**

Nếu đạt, đánh dấu hoàn tất. Nếu không, quay lại Task B4.

---

## Phase C — UC13: Chặn học sinh trùng lớp cùng khối

### Task C1: Repository kiểm tra thành viên active theo khối

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberRepository.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/CohortMemberRepository.cs`

**Interfaces:**
- Produces: `Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct)` — true nếu HS đã là thành viên active của cohort.

- [ ] **Step 1: Khai báo interface**

```csharp
    /// <summary>HS đã là thành viên (active) của khoá/khối này chưa?</summary>
    Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct = default);
```

- [ ] **Step 2: Triển khai (dùng ExistsAsync của base + cột IsActive)**

Kiểm tra tên cột trạng thái active trên entity `CohortMember` (chạy `grep -n "IsActive\|Active" exam_hub_api/ExamHub.Core/Domain/Entities/CohortMember.cs`). Giả định `IsActive`:

```csharp
    /// <inheritdoc/>
    public Task<bool> ExistsActiveMembershipAsync(int cohortId, Guid studentId, CancellationToken ct = default)
        => ExistsAsync(m => m.CohortId == cohortId && m.StudentId == studentId && m.IsActive, ct);
```

(Nếu entity không có `IsActive`, dùng điều kiện tồn tại bất kỳ: `m.CohortId == cohortId && m.StudentId == studentId`.)

- [ ] **Step 3: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj`
Expected: build thành công.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/Category/ICohortMemberRepository.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/CohortMemberRepository.cs
git commit -m "feat: add ExistsActiveMembershipAsync check"
```

### Task C2: Service chặn thêm HS trùng khối

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs`

**Interfaces:**
- Consumes: `ICohortMemberRepository.ExistsActiveMembershipAsync`.
- Produces: `AddStudentAsync` ném `InvalidOperationException("Học sinh đã thuộc lớp khác trong khối này.")` nếu đã là thành viên active.

- [ ] **Step 1: Thêm kiểm tra vào AddStudentAsync**

Ở đầu `AddStudentAsync`, trước khi tạo entity:

```csharp
        if (await _repo.ExistsActiveMembershipAsync(entity.CohortId, entity.StudentId, ct))
            throw new InvalidOperationException("Học sinh đã thuộc lớp khác trong khối này.");
```

(Xác nhận field `_repo` là `ICohortMemberRepository`; nếu tên khác thì dùng đúng.)

- [ ] **Step 2: Đảm bảo controller trả lỗi rõ ràng**

`CohortMemberController.AddStudent` hiện không bắt exception. Bọc try/catch trả 409:

```csharp
        try
        {
            var entity = request.ToEntity();
            var result = await service.AddStudentAsync(entity, ct);
            return Ok(RequestResponse<CohortMemberResponse>.Success("Thêm học sinh thành công!", CohortMemberResponse.FromEntity(result), 1));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
```

- [ ] **Step 3: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`
Expected: build thành công.

- [ ] **Step 4: Verify thủ công**

Thêm 1 HS đã ở lớp A của khoá vào lớp B cùng khoá → nhận 409 "Học sinh đã thuộc lớp khác trong khối này."; FE `CohortDetailPage` hiển thị message.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/Category/CohortMemberService.cs exam_hub_api/ExamHub.API/Controllers/School/CohortMemberController.cs
git commit -m "feat: block adding student already enrolled in cohort"
```

---

## Phase D — UC5/10/18/24/43: Cảnh báo xoá khi đang dùng + xoá bắt buộc

> **Ghi chú thiết kế (đọc trước khi làm):** "Xoá bắt buộc" chỉ AN TOÀN cho các entity sở hữu con có thể dọn (School→Cohort, Cohort→lớp/thành viên, User→bài nộp). Với **Câu hỏi (UC24)** và **Danh mục (UC18)**, xoá bắt buộc sẽ phá toàn vẹn tham chiếu (đề đã snapshot / câu hỏi đang gắn danh mục) nên **chỉ cảnh báo (409), không cho force** — thông báo hướng dẫn người dùng gỡ liên kết trước. Đây là điều chỉnh có chủ đích so với "force mọi nơi", vì lý do toàn vẹn dữ liệu.

### Task D1: Exception nghiệp vụ EntityInUseException

**Files:**
- Create: `exam_hub_api/ExamHub.Core/Application/Services/EntityInUseException.cs`

**Interfaces:**
- Produces: `sealed class EntityInUseException(string message) : Exception(message)`.

- [ ] **Step 1: Tạo exception**

```csharp
namespace ExamHub.Core.Application.Services;

/// <summary>Ném khi cố xoá một đối tượng đang được đối tượng khác tham chiếu.</summary>
public sealed class EntityInUseException(string message) : Exception(message);
```

- [ ] **Step 2: Verify build & commit**

```bash
dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core/ExamHub.Core.csproj
git add exam_hub_api/ExamHub.Core/Application/Services/EntityInUseException.cs
git commit -m "feat: add EntityInUseException"
```

### Task D2: UC24 — Cảnh báo xoá câu hỏi đang dùng trong đề

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamRepositories.cs` (`IExamQuestionRepository`)
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/ExamQuestionRepository.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/QuestionService.cs`
- Modify: `exam_hub_api/ExamHub.API/Controllers/Question/QuestionController.cs`

**Interfaces:**
- Produces:
  - `Task<bool> IExamQuestionRepository.ExistsByQuestionAsync(Guid questionId, CancellationToken ct)`.
  - `QuestionService.DeleteAsync` ném `EntityInUseException` nếu câu hỏi đang nằm trong đề.
  - `DELETE api/questions/{id}` trả 409 khi đang dùng.

- [ ] **Step 1: Repo ExistsByQuestionAsync**

Interface:

```csharp
    /// <summary>Câu hỏi có đang được dùng trong đề thi nào không (snapshot).</summary>
    Task<bool> ExistsByQuestionAsync(Guid questionId, CancellationToken ct = default);
```

Impl (`ExamQuestionRepository`):

```csharp
    /// <inheritdoc/>
    public Task<bool> ExistsByQuestionAsync(Guid questionId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(x => x.QuestionId == questionId, ct);
```

- [ ] **Step 2: Inject repo vào QuestionService và chặn xoá**

Thêm `IExamQuestionRepository` vào constructor `QuestionService` (theo cách khai báo field hiện có). Sửa `DeleteAsync`:

```csharp
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _questionRepo.GetByIdAsync(id, ct);
        if (existing is null) return;
        if (await _examQuestionRepo.ExistsByQuestionAsync(id, ct))
            throw new EntityInUseException("Câu hỏi đang được dùng trong đề thi đã sinh, không thể xoá.");
        await _questionRepo.DeleteByIdAsync(id, ct);
        await InvalidatePoolAsync(existing, ct);
    }
```

Thêm `using ExamHub.Core.Application.Services;`.

- [ ] **Step 3: Controller trả 409**

Trong `QuestionController.Delete`:

```csharp
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var existing = await service.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();
        try
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (EntityInUseException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
    }
```

Thêm `using ExamHub.Core.Application.Services;`.

- [ ] **Step 4: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`
Expected: build thành công (kiểm tra DI: `QuestionService` nhận thêm `IExamQuestionRepository` — repo này đã đăng ký trong `DependencyContainer.cs`; nếu chưa, thêm đăng ký).

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamRepositories.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/Category/ExamQuestionRepository.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/QuestionService.cs exam_hub_api/ExamHub.API/Controllers/Question/QuestionController.cs
git commit -m "feat: warn when deleting a question used in an exam"
```

### Task D3: UC18 — Cảnh báo xoá danh mục đang dùng (theo câu hỏi)

**Files:**
- Modify: `exam_hub_api/ExamHub.API/Controllers/CategoryBaseController.cs` (bắt `EntityInUseException` → 409)
- Modify: các category service có tham chiếu từ câu hỏi: `SubjectService`, `TopicService`, `DifficultyLevelService`, `CognitiveLevelService`, `QuestionTypeService`, `GradeLevelService` — chặn xoá nếu còn câu hỏi tham chiếu.
- Modify: `IQuestionRepository` + impl — thêm đếm/kiểm tra tham chiếu theo từng khoá danh mục.

**Interfaces:**
- Produces:
  - `IQuestionRepository.ExistsByTopicAsync(int topicId)`, `ExistsBySubjectAsync(int subjectId)`, `ExistsByDifficultyAsync(int)`, `ExistsByCognitiveAsync(int)`, `ExistsByQuestionTypeAsync(int)`, `ExistsByGradeLevelAsync(int)` (bổ sung theo cột thực có trên entity `Question`).
  - `CategoryBaseController.Delete` trả 409 khi service ném `EntityInUseException`.

> Trước khi làm: `grep -n "public .* {" exam_hub_api/ExamHub.Core/Domain/Entities/Question.cs` để xác nhận các cột FK có trên `Question` (TopicId, SubjectId, DifficultyLevelId, CognitiveLevelId, QuestionTypeId, GradeLevelId). Chỉ thêm kiểm tra cho những cột thực sự tồn tại.

- [ ] **Step 1: Controller base bắt exception**

Sửa `CategoryBaseController.Delete`:

```csharp
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(TKey id, CancellationToken ct = default)
    {
        try
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (ExamHub.Core.Application.Services.EntityInUseException ex)
        {
            return Conflict(RequestResponse<object>.Error(ex.Message));
        }
    }
```

- [ ] **Step 2: Repo Question kiểm tra tham chiếu**

Thêm vào `IQuestionRepository` + impl (dùng `Set.AsNoTracking().AnyAsync(...)`), ví dụ Topic & Subject (lặp tương tự cho các khoá còn lại thực có):

```csharp
    Task<bool> ExistsByTopicAsync(int topicId, CancellationToken ct = default);
    Task<bool> ExistsBySubjectAsync(int subjectId, CancellationToken ct = default);
    Task<bool> ExistsByDifficultyAsync(int difficultyLevelId, CancellationToken ct = default);
    Task<bool> ExistsByCognitiveAsync(int cognitiveLevelId, CancellationToken ct = default);
    Task<bool> ExistsByQuestionTypeAsync(int questionTypeId, CancellationToken ct = default);
```

Impl:

```csharp
    public Task<bool> ExistsByTopicAsync(int topicId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(q => q.TopicId == topicId, ct);
    public Task<bool> ExistsBySubjectAsync(int subjectId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(q => q.SubjectId == subjectId, ct);
    public Task<bool> ExistsByDifficultyAsync(int difficultyLevelId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(q => q.DifficultyLevelId == difficultyLevelId, ct);
    public Task<bool> ExistsByCognitiveAsync(int cognitiveLevelId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(q => q.CognitiveLevelId == cognitiveLevelId, ct);
    public Task<bool> ExistsByQuestionTypeAsync(int questionTypeId, CancellationToken ct = default)
        => Set.AsNoTracking().AnyAsync(q => q.QuestionTypeId == questionTypeId, ct);
```

(Bỏ hàm nào nếu cột tương ứng không tồn tại trên `Question`.)

- [ ] **Step 3: Chặn xoá trong từng category service**

Với mỗi service, inject `IQuestionRepository` và sửa `DeleteAsync`. Ví dụ `TopicService`:

```csharp
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (await _questionRepo.ExistsByTopicAsync(id, ct))
            throw new EntityInUseException("Chủ đề đang được dùng bởi câu hỏi, không thể xoá.");
        await _repo.DeleteByIdAsync(id, ct);
    }
```

Lặp tương tự: `SubjectService` (`ExistsBySubjectAsync`, "Môn học đang được dùng…"), `DifficultyLevelService` (`ExistsByDifficultyAsync`, "Độ khó đang được dùng…"), `CognitiveLevelService` (`ExistsByCognitiveAsync`, "Cấp độ Bloom đang được dùng…"), `QuestionTypeService` (`ExistsByQuestionTypeAsync`, "Loại câu hỏi đang được dùng…"). `GradeLevelService`: kiểm tra tham chiếu từ `IExamRepository`/`ICohortRepository` nếu có cột GradeLevelId (dùng `ExistsAsync(e => e.GradeLevelId == id)`), thông báo "Khối/lớp đang được dùng…". Thêm `using ExamHub.Core.Application.Services;` vào mỗi file.

- [ ] **Step 4: Verify build**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`
Expected: build thành công; kiểm tra DI cho các service đã inject thêm repo.

- [ ] **Step 5: Verify thủ công**

Thử xoá một môn học đang có câu hỏi → 409 với message rõ ràng.

- [ ] **Step 6: Commit**

```bash
git add -A exam_hub_api
git commit -m "feat: warn when deleting a category still referenced by questions"
```

### Task D4: UC5/UC10 — Xoá trường/khoá học: cảnh báo + force

**Files:**
- Modify: `exam_hub_api/ExamHub.API/Controllers/School/SchoolController.cs`, `CohortController.cs` (không kế thừa Delete từ base thì thêm override có `?force`)
- Modify: `SchoolService`, `CohortService` (thêm `DeleteAsync(id, force)`)
- Modify: interface `ISchoolService`, `ICohortService`

**Interfaces:**
- Produces:
  - `Task DeleteAsync(int id, bool force, CancellationToken ct)` cho School/Cohort.
  - Không force + còn con → `EntityInUseException`; force → xoá con rồi xoá cha.
  - `DELETE api/cohort/{id}?force=bool`, `DELETE api/school/{id}?force=bool` (đường route theo controller thực tế).

> `CohortController`/`SchoolController` kế thừa `CategoryBaseController` (Delete generic không nhận force). Thêm **override Delete có tham số `force`** ở controller con để không đụng base.

- [ ] **Step 1: Interface + service Cohort**

`ICohortService`: thêm `Task DeleteAsync(int id, bool force, CancellationToken ct = default);`

`CohortService` (dùng `ICohortMemberRepository`, `ICohortClassRepository` nếu có — xác nhận tên; nếu lớp sinh bằng trigger và có bảng `cohort_classes`, dùng repo tương ứng):

```csharp
    public async Task DeleteAsync(int id, bool force, CancellationToken ct = default)
    {
        var hasMembers = await _memberRepo.CountAsync(m => m.CohortId == id, ct) > 0;
        if (hasMembers && !force)
            throw new EntityInUseException("Khoá học còn học sinh/lớp đang hoạt động. Xoá bắt buộc để xoá luôn dữ liệu liên quan.");
        if (force)
        {
            // xoá thành viên (và lớp nếu có bảng riêng) trước
            var members = await _memberRepo.GetByCohortAsync(id, ct);
            foreach (var m in members) await _memberRepo.DeleteByIdAsync(m.Id, ct);
        }
        await _repo.DeleteByIdAsync(id, ct);
    }
```

Giữ `DeleteAsync(int id, ct)` cũ (interface `ICohortService` hiện có) ủy quyền: `=> DeleteAsync(id, false, ct);`

- [ ] **Step 2: Override Delete ở CohortController**

```csharp
    /// <summary>Xoá khoá học; ?force=true để xoá kèm dữ liệu liên quan.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCohort(int id, [FromQuery] bool force = false, CancellationToken ct = default)
    {
        try { await service.DeleteAsync(id, force, ct); return NoContent(); }
        catch (EntityInUseException ex) { return Conflict(RequestResponse<object>.Error(ex.Message)); }
    }
```

(Tên route trùng base `[HttpDelete("{id}")]` — dùng `{id:int}` để ưu tiên; nếu xung đột routing, đặt `[HttpDelete("{id:int}/delete")]` hoặc ẩn base bằng `new`. Xác nhận khi build/chạy.)

- [ ] **Step 3: Lặp tương tự cho School**

`ISchoolService`/`SchoolService.DeleteAsync(id, force)`: kiểm tra còn Cohort (`ICohortRepository.GetBySchoolAsync`); force → xoá các cohort (gọi `cohortService.DeleteAsync(c.Id, true, ct)`), rồi xoá school. Thông báo: "Trường đang có khoá học/lớp liên kết. Xoá bắt buộc để xoá luôn." Override `Delete` ở `SchoolController` giống Step 2.

- [ ] **Step 4: Verify build & routing**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj` rồi chạy API, thử `DELETE` cả 2 nhánh (force / không force).
Expected: không force + còn con → 409; force → 204.

- [ ] **Step 5: Commit**

```bash
git add -A exam_hub_api
git commit -m "feat: warn+force delete for school and cohort"
```

### Task D5: UC43 — Xoá người dùng: cảnh báo + force

**Files:**
- Modify: `exam_hub_api/ExamHub.API/Controllers/UserController.cs`
- Modify: user service (nơi có `DeleteAsync`) — xác định qua `grep -rn "DeleteAsync" exam_hub_api/ExamHub.Core/**/UserManagementService*.cs`

**Interfaces:**
- Consumes: `IExamSubmissionRepository.GetByStudentAsync` (đã có) để phát hiện dữ liệu liên quan.
- Produces: `DELETE api/user/{id}?force=bool` — 409 nếu còn bài nộp/đề thi liên quan và không force.

- [ ] **Step 1: Controller kiểm tra tham chiếu**

Trong `UserController.Delete` (thêm tham số `force`, inject `IExamSubmissionRepository`):

```csharp
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] bool force = false, CancellationToken ct = default)
    {
        var user = await userService.GetByIdAsync(id); // theo API hiện có
        if (user is null) return NotFound();

        var hasData = (await submissionRepo.GetByStudentAsync(id, ct)).Count > 0;
        if (hasData && !force)
            return Conflict(RequestResponse<object>.Error(
                "Người dùng đang có dữ liệu liên quan (bài làm/đề thi). Dùng xoá bắt buộc nếu chắc chắn."));

        await userService.DeleteAsync(user);
        return NoContent();
    }
```

(Điều chỉnh cách lấy user/DeleteAsync theo chữ ký thực tế của `userService`. Nếu force cần dọn bài nộp trước do FK, gọi xoá submissions trước khi xoá user.)

- [ ] **Step 2: Verify build & thủ công**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`; thử xoá user có bài nộp → 409; `?force=true` → xoá.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.API/Controllers/UserController.cs
git commit -m "feat: warn+force delete for user with related data"
```

### Task D6: Frontend — bắt 409 và xác nhận xoá bắt buộc

**Files:**
- Modify: services & trang liên quan: `QuestionBankPage.tsx`, `CategoryPage.tsx` (chỉ hiển thị lỗi 409, không force), `SchoolListPage.tsx`/`SchoolDetailPage.tsx`, `CohortDetailPage.tsx`, `UserPage.tsx` (có nút xoá bắt buộc).

**Interfaces:**
- Consumes: HTTP 409 với `message` từ BE; API delete nhận `?force=true`.
- Produces: với entity hỗ trợ force (school/cohort/user) → `Modal.confirm` "… đang được sử dụng. Xoá bắt buộc?" gọi lại `?force=true`. Với question/category → chỉ hiển thị message lỗi.

- [ ] **Step 1: Chuẩn hoá xử lý xoá có force**

Trong service xoá của school/cohort/user, thêm tham số `force`:

```ts
export function deleteCohort(id: number, force = false) {
    return http.delete(`/cohort/${id}${force ? '?force=true' : ''}`)
}
```

(Tương tự cho school/user; đường path theo service hiện có.)

- [ ] **Step 2: Trang bắt 409 → confirm force**

Ví dụ mẫu dùng lại cho school/cohort/user:

```tsx
try {
    await deleteCohort(id)
    message.success('Đã xoá')
    refetch()
} catch (e: any) {
    if (e?.response?.status === 409) {
        Modal.confirm({
            title: 'Đang được sử dụng',
            content: e.response.data?.message ?? 'Đối tượng đang được sử dụng. Xoá bắt buộc?',
            okText: 'Xoá bắt buộc', okType: 'danger',
            onOk: async () => { await deleteCohort(id, true); message.success('Đã xoá'); refetch() },
        })
    } else { message.error('Xoá thất bại') }
}
```

- [ ] **Step 3: Question/Category chỉ hiển thị lỗi**

Với `QuestionBankPage.tsx` và `CategoryPage.tsx`, bắt 409 và `message.error(e.response.data?.message)` — KHÔNG có nút force.

- [ ] **Step 4: Verify build web**

Run: `cd exam_hub_web && npm run build`
Expected: build thành công.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/src
git commit -m "feat(web): handle 409 in-use on delete, confirm force where safe"
```

---

## Self-Review Notes (đã kiểm)

- **Spec coverage:** A→UC34, B→UC33, C→UC13, D2→UC24, D3→UC18, D4→UC5/UC10, D5→UC43. Đủ 6 điểm lệch.
- **Điều chỉnh có chủ đích:** force delete KHÔNG áp dụng cho câu hỏi/danh mục (toàn vẹn snapshot/tham chiếu) — nêu rõ ở đầu Phase D; cần chủ dự án xác nhận khi review.
- **Phụ thuộc tên thực tế:** nhiều task yêu cầu `grep` xác nhận tên cột/field (IsActive, các *Id trên Question, tên field repo trong service, đường route controller) — đã ghi chú tại chỗ để tránh giả định sai.
- **Type consistency:** `SubmissionGrading.CorrectAnswerIds`/`DecideStatus`/`HasManualGradeQuestion` dùng nhất quán ở A2–A3; `ReplaceForSubmissionAsync` (B1) dùng đúng ở B2; `ExistsByQuestionAsync` (D2), `ExistsActiveMembershipAsync` (C1) khớp consumer.
