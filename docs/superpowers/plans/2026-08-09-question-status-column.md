# Chuyển `is_verified` (bool) → cột `status` (enum) cho câu hỏi — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Steps dùng checkbox `- [ ]`.

**Goal:** Thay cột nhị phân `is_verified` bằng cột `status` (`pending`|`approved`|`rejected`) để check trạng thái duyệt trực tiếp, cập nhật MỌI nơi đang query `is_verified` (BE + FE + SQL). **Giữ `is_active`, `rejection_reason`, `verified_by/at`.**

**Architecture:** `status` = nguồn chân lý 3 trạng thái. `approved` ⇔ (cũ) `is_verified=true`. `rejected` kèm `rejection_reason`. Mặc định `pending`.

**Tech Stack:** PostgreSQL, ASP.NET Core + EF, React 19, AntD 6.

## Global Constraints

- Bản đồ trạng thái: **approved** (dùng sinh đề), **pending** (chờ), **rejected** (kèm `rejection_reason`).
- Bất biến: Duyệt⇒`status='approved'`,`rejection_reason=NULL`. Bỏ duyệt⇒`status='pending'`,`rejection_reason=NULL`. Từ chối⇒`status='rejected'`,`rejection_reason=reason`. Đều ghi `verified_by/at` = người xử lý.
- Pool sinh đề: `status='approved' AND is_active`.
- Verify: BE `dotnet build`; FE `npx tsc --noEmit`+`npm run build`. Nhánh `feat/question-bank-figma`. Commit kết `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`.

## Danh mục nơi dùng `is_verified` (từ grep — plan phải phủ hết)

- **SQL** `database_schema.sql`: cột (348), comment (351), index `idx_questions_active` (589), partial index `idx_q_pool` (600) + `idx_q_pool_cognitive` (604).
- **BE**: `QuestionTable.cs`(51); `Question.cs`(88–90 prop, 141+166 ToInsert/ToUpdate); `QuestionDto.cs` QuestionRequest(54,74), QuestionResponse(108,136), QuestionPagedRequest(153); `IExamServices.cs`(20 param); `IQuestionRepositories.cs`(36 param); `AppDbContext.cs`(225 config); `QuestionRepository.cs`(53 pool, 83 param, 111–118 filter, 227/237/247 verify/unverify/reject, 256–258 stats, 299 pool SQL); `QuestionService.cs`(39–40 param, 102/109 comment).
- **FE**: `question.d.ts`(38,58,70); `QuestionBankPage.tsx`(64 reviewState, 197 column); `AddQuestionPage.tsx`(42,49,99,140,367 toggle).
- **Migrations EF** (InitialCreate + snapshot): runtime KHÔNG áp (DB qua schema.sql) → để nguyên như lịch sử; snapshot lệch model không ảnh hưởng build/run.

---

## Task 1: DB — thêm `status`, backfill, bỏ `is_verified`, sửa index

**Files:** `database_schema.sql` + DB đang chạy.

- [ ] **Step 1: schema.sql — cột** (dòng 348): thay
  `is_verified BOOLEAN NOT NULL DEFAULT FALSE,` →
  `status VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','approved','rejected')),`
  và sửa comment dòng 351 (`is_verified=false` → `status<>'approved'`).
- [ ] **Step 2: schema.sql — index** (589/600/604): đổi mọi `is_verified` → `status`:
  - `idx_questions_active ... (is_active, status)`
  - `idx_q_pool ... WHERE is_active = true AND status = 'approved'`
  - `idx_q_pool_cognitive ... WHERE is_active = true AND status = 'approved'`
- [ ] **Step 3: Áp DB đang chạy** (docker exec psql):
```sql
ALTER TABLE public.questions ADD COLUMN IF NOT EXISTS status VARCHAR(20) NOT NULL DEFAULT 'pending'
    CHECK (status IN ('pending','approved','rejected'));
UPDATE public.questions SET status = CASE
    WHEN is_verified THEN 'approved'
    WHEN rejection_reason IS NOT NULL THEN 'rejected'
    ELSE 'pending' END;
DROP INDEX IF EXISTS idx_questions_active;
DROP INDEX IF EXISTS idx_q_pool;
DROP INDEX IF EXISTS idx_q_pool_cognitive;
CREATE INDEX idx_questions_active ON public.questions (is_active, status);
CREATE INDEX idx_q_pool ON public.questions (topic_id, difficulty_level_id, question_type_id)
    INCLUDE (id, cognitive_level_id) WHERE is_active = true AND status = 'approved';
CREATE INDEX idx_q_pool_cognitive ON public.questions (topic_id, cognitive_level_id, difficulty_level_id)
    INCLUDE (id) WHERE is_active = true AND status = 'approved';
ALTER TABLE public.questions DROP COLUMN is_verified;
```
  Verify: `\d questions` có `status`, không còn `is_verified`.
- [ ] **Step 4: Commit** — `git commit -m "db: chuyển is_verified sang cột status (schema.sql + DB)"`.

---

## Task 2: BE — FieldTable + Entity + AppDbContext

**Files:** `QuestionTable.cs`; `Question.cs`; `AppDbContext.cs`.

- [ ] **Step 1: FieldTable** — đổi `IsVerified = "is_verified"` → `Status = "status"`.
- [ ] **Step 2: Entity** — thay property:
```csharp
[Column(QuestionTable.Status)]
[SqlBuilderProperty(QuestionTable.Status, Insert = true, Update = true)]
public string Status { get; set; } = "pending";
```
và trong ToInsertObject/ToUpdateObject đổi `is_verified = IsVerified` → `status = Status`.
- [ ] **Step 3: AppDbContext** (225) — thay `e.Property(x => x.IsVerified).HasDefaultValue(false);` → `e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("pending");`.
- [ ] **Step 4: Build** `dotnet build ExamHub.Core` (sẽ lỗi ở nơi còn dùng IsVerified — sửa ở Task 3/4). Có thể build sau Task 4.
- [ ] **Step 5: Commit** (gộp với Task 3/4 nếu build chưa xanh).

---

## Task 3: BE — DTO (Request/Response/PagedRequest)

**Files:** `QuestionDto.cs`.

- [ ] **Step 1: QuestionRequest** — thay `public bool IsVerified { get; set; }` → `public string Status { get; set; } = "pending";`; trong ToEntity đổi `IsVerified = IsVerified` → `Status = Status`. (`IsActive` giữ nguyên.)
- [ ] **Step 2: QuestionResponse** — thay field `bool IsVerified` → `string Status`; trong FromEntity `e.IsVerified` → `e.Status`.
- [ ] **Step 3: QuestionPagedRequest** — bỏ `bool? IsVerified = null` (giữ `ReviewStatus`).
- [ ] **Step 4: Commit** (gộp Task 2–5).

---

## Task 4: BE — Repo + Service

**Files:** `IQuestionRepositories.cs`; `QuestionRepository.cs`; `IExamServices.cs`; `QuestionService.cs`.

- [ ] **Step 1: Pool** (repo 53): `.Where(x => x.IsActive && x.Status == "approved")`.
- [ ] **Step 2: Verify/Unverify/Reject** (227/237/247): thay `.SetProperty(x => x.IsVerified, true/false)` →
  Verify `.SetProperty(x => x.Status, "approved")`; Unverify `.SetProperty(x => x.Status, "pending")`; Reject `.SetProperty(x => x.Status, "rejected")` (giữ set rejection_reason/verified_by/at như hiện tại).
- [ ] **Step 3: GetStats** (256–258):
```csharp
var verified = await Set.CountAsync(x => x.Status == "approved", ct);
var rejected = await Set.CountAsync(x => x.Status == "rejected", ct);
var pending  = await Set.CountAsync(x => x.Status == "pending", ct);
```
- [ ] **Step 4: GetPaged filter** (111–118): bỏ nhánh `isVerified`; thay switch reviewStatus bằng `if (!string.IsNullOrWhiteSpace(reviewStatus)) query = query.Where(x => x.Status == reviewStatus);` (giá trị approved/pending/rejected = status).
- [ ] **Step 5: Bỏ param `isVerified`** khỏi `GetPagedAsync` ở repo(83)+interface(36)+service(39–40)+interface IExamServices(20). Controller (Task 5) bỏ truyền IsVerified.
- [ ] **Step 6: Pool SQL** (299): `AND q.status = 'approved'` (thay `q.is_verified = true`).
- [ ] **Step 7: Comment** (service 102/109): "is_verified=true" → "status='approved'".
- [ ] **Step 8: Build** `dotnet build ExamHub.Core` — 0 error.

---

## Task 5: BE — Controller + build API + commit

**Files:** `QuestionController.cs`.

- [ ] **Step 1: GetPaged** — bỏ `request.IsVerified` khỏi lời gọi `service.GetPagedAsync(... request.ReviewStatus, ct)`.
- [ ] **Step 2: Build** Core + API (OutDir khác nếu API đang chạy) — 0 error.
- [ ] **Step 3: Commit** — `git commit -m "feat(be): thay is_verified bằng status ở entity/DTO/repo/service/controller"`.

---

## Task 6: FE — types

**Files:** `types/question.d.ts`.

- [ ] **Step 1: Question** — `isVerified: boolean` → `status: string` (giữ `rejectionReason`).
- [ ] **Step 2: QuestionBody** — `isVerified?: boolean` → `status?: string`.
- [ ] **Step 3: QuestionPagedQuery** — bỏ `isVerified?: boolean` (giữ `reviewStatus?: string`).

---

## Task 7: FE — QuestionBankPage

**Files:** `pages/questions/QuestionBankPage.tsx`.

- [ ] **Step 1: reviewState** (64) — `const reviewState = (q: Question): ReviewState => q.status as ReviewState`.
- [ ] **Step 2: Cột "Duyệt"** (197) — `dataIndex: 'status'`; render giữ nguyên (dùng `reviewState(q)`).
- [ ] **Step 3: Build** `npx tsc --noEmit`.

---

## Task 8: FE — AddQuestionPage (toggle "Đã xác minh" ↔ status)

**Files:** `pages/questions/AddQuestionPage.tsx`.

- [ ] **Step 1: Form vẫn dùng toggle boolean `isVerified`** (UI không đổi), nhưng:
  - `QuestionBody` gửi `status: v.isVerified ? 'approved' : 'pending'` (thay `isVerified: v.isVerified`).
  - Load khi sửa: `isVerified: existing.status === 'approved'` (thay `existing.isVerified`).
  - (QuestionForm type vẫn có `isVerified: boolean` cho form nội bộ — không đổi.)
- [ ] **Step 2: Build** `npx tsc --noEmit` + `npm run build` — exit 0.
- [ ] **Step 3: Commit** — `git commit -m "feat(fe): dùng status thay is_verified (question bank + add question)"`.

---

## Self-Review

- **Phủ hết grep:** SQL(T1), FieldTable/Entity/DbContext(T2), DTO(T3), Repo/Service(T4), Controller(T5), FE types(T6)/QuestionBank(T7)/AddQuestion(T8). Đủ.
- **Giữ:** `is_active`, `rejection_reason`, `verified_by/at`.
- **Type nhất quán:** BE `Status`(string) ↔ FE `status`(string); filter FE `reviewStatus` → BE `x.Status == reviewStatus` (giá trị trùng).
- **Rủi ro:** CHECK constraint chặn giá trị lạ — verify/reject/unverify chỉ dùng 3 giá trị hợp lệ. EF model snapshot lệch (không ảnh hưởng runtime vì DB qua schema.sql).
```
