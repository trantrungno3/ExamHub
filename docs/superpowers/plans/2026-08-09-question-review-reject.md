# Duyệt / Từ chối câu hỏi (kèm lý do) — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Steps dùng checkbox `- [ ]`.

**Goal:** Bổ sung luồng **duyệt / từ chối (kèm lý do)** cho câu hỏi: QTV/GV xem câu chờ duyệt → **Duyệt** hoặc **Từ chối kèm lý do**. Chỉ câu đã duyệt mới dùng sinh đề (đã có sẵn — pool lọc `is_verified=true`).

**Architecture:** Giữ `is_verified` (bool) + thêm cột `rejection_reason`. 3 trạng thái suy ra, không cần enum mới, không migrate 5200 câu.

**Tech Stack:** ASP.NET Core + EF, PostgreSQL, React 19, AntD 6, TanStack Query.

## Global Constraints

- **Mô hình trạng thái (suy ra, không thêm enum):**
  - **Đã duyệt** = `is_verified = true`
  - **Bị từ chối** = `is_verified = false AND rejection_reason IS NOT NULL`
  - **Chờ duyệt** = `is_verified = false AND rejection_reason IS NULL`
- **Bất biến:** Duyệt ⇒ `is_verified=true`, `rejection_reason=NULL`. Bỏ duyệt ⇒ `is_verified=false`, `rejection_reason=NULL` (về *chờ duyệt*). Từ chối ⇒ `is_verified=false`, `rejection_reason=<reason>`, ghi `verified_by`/`verified_at` = người xử lý.
- Pool sinh đề KHÔNG đổi (đã lọc `is_verified=true`).
- Sửa câu hỏi qua form: `ToEntity` đặt `rejection_reason=NULL` (mặc định) ⇒ chỉnh sửa đưa câu *bị từ chối* về *chờ duyệt* (hợp lý: đã sửa để nộp lại).
- Verify BE bằng `dotnet build`; FE `npx tsc --noEmit` + `npm run build`. Không test tự động → smoke `.http` + thủ công.
- Nhánh `feat/question-bank-figma`. Commit kết `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`.

---

## Task 1: DB — cột `rejection_reason` — ✅ ĐÃ LÀM

- [x] `database_schema.sql`: thêm `rejection_reason TEXT` vào bảng `questions` (sau `verified_at`).
- [x] Áp DB đang chạy: `ALTER TABLE public.questions ADD COLUMN IF NOT EXISTS rejection_reason TEXT;` (đã verify cột tồn tại).

---

## Task 2: BE — Entity + FieldTable + Response DTO expose reason

**Files:** `FieldTables/QuestionTable.cs`; `Domain/Entities/Question.cs`; `DataTransferObjects/Question/QuestionDto.cs` (QuestionResponse).

- [ ] **Step 1: FieldTable** — thêm `public const string RejectionReason = "rejection_reason";` vào `QuestionTable`.
- [ ] **Step 2: Entity** — thêm property vào `Question.cs`:
```csharp
[Column(QuestionTable.RejectionReason)]
[SqlBuilderProperty(QuestionTable.RejectionReason, Insert = true, Update = true)]
public string? RejectionReason { get; set; }
```
và thêm `rejection_reason = RejectionReason` vào cả `ToInsertObject()` lẫn `ToUpdateObject()`.
- [ ] **Step 3: QuestionResponse** — thêm field `string? RejectionReason` vào record `QuestionResponse` và map trong `FromEntity`.
- [ ] **Step 4: Build** — `dotnet build ExamHub.Core/ExamHub.Core.csproj`. Expected 0 error.
- [ ] **Step 5: Commit** — `git commit -m "feat(be): Question.RejectionReason + expose ở response"`.

---

## Task 3: BE — Repo + Service (reject, clear reason, stats theo trạng thái)

**Files:** `Domain/Interfaces/IQuestionRepositories.cs` + `QuestionRepository.cs`; `Domain/Interfaces/IExamServices.cs` + `QuestionService.cs`; `DataTransferObjects/Question/QuestionStatsDto.cs`.

- [ ] **Step 1: QuestionStatsResponse** — mở rộng: `record QuestionStatsResponse(int Total, int Verified, int Pending, int Rejected, int Inactive)` (bỏ `Unverified`, tách Pending/Rejected).
- [ ] **Step 2: Repo — Verify/Unverify clear reason + Reject + Stats**
```csharp
// VerifyAsync: thêm .SetProperty(x => x.RejectionReason, (string?)null)
// UnverifyAsync: thêm .SetProperty(x => x.RejectionReason, (string?)null)

public async Task RejectAsync(Guid id, Guid reviewedBy, string reason, CancellationToken ct = default)
    => await Set.Where(x => x.Id == id)
        .ExecuteUpdateAsync(s => s
            .SetProperty(x => x.IsVerified, false)
            .SetProperty(x => x.RejectionReason, reason)
            .SetProperty(x => x.VerifiedBy, reviewedBy)
            .SetProperty(x => x.VerifiedAt, DateTime.UtcNow), ct);

public async Task<QuestionStatsResponse> GetStatsAsync(CancellationToken ct = default)
{
    var total    = await Set.CountAsync(ct);
    var verified = await Set.CountAsync(x => x.IsVerified, ct);
    var rejected = await Set.CountAsync(x => !x.IsVerified && x.RejectionReason != null, ct);
    var pending  = await Set.CountAsync(x => !x.IsVerified && x.RejectionReason == null, ct);
    var inactive = await Set.CountAsync(x => !x.IsActive, ct);
    return new QuestionStatsResponse(total, verified, pending, rejected, inactive);
}
```
Thêm `RejectAsync` vào `IQuestionRepository`.
- [ ] **Step 3: Service** — thêm `RejectAsync(Guid id, Guid reviewedBy, string reason, ct)` vào `IQuestionService` + impl (invalidate pool của câu trước khi đổi, giống Unverify), passthrough repo.
- [ ] **Step 4: (tuỳ) Filter theo trạng thái** — nếu làm filter "Bị từ chối" (Task 6): `GetPagedAsync` nhận thêm `string? reviewStatus` (pending/approved/rejected) và dịch sang điều kiện `is_verified`/`rejection_reason`. Nếu chưa cần, bỏ qua.
- [ ] **Step 5: Build** — `dotnet build`. Expected 0 error.
- [ ] **Step 6: Commit** — `git commit -m "feat(be): reject câu hỏi + stats theo trạng thái (pending/rejected)"`.

---

## Task 4: BE — Controller endpoint reject + `.http`

**Files:** `Controllers/Question/QuestionController.cs`; `ExamHub.API/*.http`.

- [ ] **Step 1: Endpoint** (sau Unverify):
```csharp
public record RejectQuestionRequest(string Reason);

[HttpPost("{id:guid}/reject")]
public async Task<IActionResult> Reject(Guid id, [FromBody] RejectQuestionRequest req, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(req.Reason))
        return BadRequest(RequestResponse<object>.Error("Vui lòng nhập lý do từ chối."));
    var existing = await service.GetByIdAsync(id, ct);
    if (existing is null) return NotFound();
    await service.RejectAsync(id, CurrentUser.UserId!.Value, req.Reason.Trim(), ct);
    return NoContent();
}
```
(Đặt `RejectQuestionRequest` trong DTO Question hoặc trên controller.)
- [ ] **Step 2: `.http`** — thêm request reject (kỳ vọng NoContent; reason rỗng → 400).
- [ ] **Step 3: Build** (ra OutDir khác nếu API đang chạy khoá bin) — 0 error.
- [ ] **Step 4: Commit** — `git commit -m "feat(be): endpoint POST questions/{id}/reject"`.

---

## Task 5: FE — types + service + hook

**Files:** `types/question.d.ts`; `services/questionService.ts`; `hooks/queries/useQuestions.ts`.

- [ ] **Step 1: Types** — `Question` thêm `rejectionReason?: string | null`. `QuestionStats` đổi thành `{total; verified; pending; rejected; inactive}`.
- [ ] **Step 2: Service** — `reject(id: string, reason: string) => AuthHttp.post<void>('/questions/${id}/reject', {reason})`.
- [ ] **Step 3: Hook** — `useRejectQuestionMutation()` (mutationFn nhận `{id, reason}`; onSuccess message + invalidate `all` + `stats`).
- [ ] **Step 4: Build** — `npx tsc --noEmit`. Expected 0 error.
- [ ] **Step 5: Commit** — `git commit -m "feat(fe): service/hook reject câu hỏi + type"`.

---

## Task 6: FE — QuestionBank: badge 3 trạng thái + hành động + modal lý do + stat

**Files:** `pages/questions/QuestionBankPage.tsx`.

- [ ] **Step 1: Suy ra trạng thái** — helper:
```ts
const reviewState = (q: Question) =>
    q.isVerified ? 'approved' : (q.rejectionReason ? 'rejected' : 'pending')
```
- [ ] **Step 2: Cột "Duyệt"** — dùng `StatusTag`: approved→`success`"Đã duyệt", pending→`warning`"Chờ duyệt", rejected→`danger`"Bị từ chối". Với rejected bọc `Tooltip title={q.rejectionReason}`.
- [ ] **Step 3: Hành động theo trạng thái** —
  - pending: **Duyệt** (verify) · **Từ chối** (mở modal) · Sửa · Xóa
  - approved: **Bỏ duyệt** (unverify) · Sửa · Xóa
  - rejected: **Duyệt** (verify lại) · **Xem lý do** (Tooltip/Popover) · Sửa · Xóa
- [ ] **Step 4: Modal Từ chối** — state `rejectTarget?: Question`; `Modal` (hoặc `Modal.confirm` với `Input.TextArea`) nhập lý do → `rejectMutation.mutate({id, reason})`. Nút "Từ chối" disabled khi reason rỗng.
- [ ] **Step 5: Stat card + filter** — thêm thẻ **"Bị từ chối"** (đỏ) dùng `stats.rejected`; "Chờ duyệt" dùng `stats.pending`. Filter "Trạng thái" thêm mục "Bị từ chối" (nếu làm BE filter ở Task 3 Step 4; nếu không, để badge + stat là đủ).
- [ ] **Step 6: Build** — `npx tsc --noEmit` + `npm run build`. Expected exit 0.
- [ ] **Step 7: Commit** — `git commit -m "feat(fe): question bank — duyệt/từ chối kèm lý do + badge 3 trạng thái"`.

---

## Self-Review

- **Yêu cầu mô tả:** xem xét→Task 6 (badge/filter); phê duyệt→verify (đã có); **từ chối kèm lý do**→Task 3/4/6; chỉ câu duyệt mới sinh đề→đã có (pool `is_verified=true`), không đụng.
- **Bất biến:** verify/unverify clear `rejection_reason` (Task 3 Step 2) tránh trạng thái mập mờ.
- **Type nhất quán:** `RejectAsync(id, reviewedBy, reason)` BE ↔ `reject(id, reason)` FE; `QuestionStatsResponse(Total,Verified,Pending,Rejected,Inactive)` ↔ `QuestionStats`.
- **Rủi ro:** stats đổi cấu trúc (`Unverified`→`Pending`+`Rejected`) — cập nhật đồng bộ FE (Task 5) để không vỡ stat card. Phân quyền reject dùng `[Authorize]` như verify (cân nhắc giới hạn Admin/Teacher nếu cần).
```
