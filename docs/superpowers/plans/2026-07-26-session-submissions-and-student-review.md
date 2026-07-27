# Bài nộp theo kỳ thi + Học sinh xem lại kết quả

> Kế hoạch triển khai. Thực thi theo superpowers:executing-plans, mỗi task 1 checkbox.

**Goal:**
1. **Giáo viên** xem "bài làm của học sinh" theo **kỳ thi** (thay vì theo từng đề như hiện tại).
2. **Học sinh** xem lại **kết quả các lần thi của bản thân** trong một kỳ thi.

## Quyết định thiết kế (YAGNI — có thể chỉnh)
- `ExamSubmission` đã có `SessionId` (nullable) → truy vấn theo kỳ thi khả thi, chỉ cần thêm endpoint.
- **GV:** thêm nút **"Bài nộp"** ở mỗi dòng trong `ExamSessionListPage` → mở **Drawer chấm bài** (tái sử dụng `SubmissionsDrawer` hiện có, tổng quát hoá để nhận `sessionId`). **Bỏ** nút "Bài nộp" theo từng đề ở `ExamListPage` (đúng nghĩa "chuyển sang kỳ thi").
- Một kỳ thi có nhiều đề → mỗi bài nộp có `examId` riêng; `SubmissionCard` tự nạp đề của chính nó (`useExamWithQuestionsQuery(sub.examId)`) để hiển thị nội dung câu hỏi khi chấm tự luận.
- **HS:** ở mỗi thẻ kỳ thi trong `StudentSessionListPage`, nếu `usedAttempts > 0` hiện nút **"Xem kết quả"** → mở **Modal** liệt kê các lần nộp (điểm, trạng thái, thời gian) → bấm 1 dòng mở lại trang kết quả `/student/exam/result?submissionId=...` (đã có).

**Tech:** .NET (EF Core, controller/service/repo) + React 19 + AntD 6 + TS. Verify: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj` (API dev khoá DLL → build riêng Core); `pnpm -C exam_hub_web exec tsc -b` (còn lỗi pre-existing `RichTextEditor.tsx`); `eslint <file>`; `vite build`.

---

## Task 1: Backend — repository truy vấn theo session
**Files:** `.../Domain/Interfaces/**/IExamSubmissionRepository.cs`, `.../Repositories/Implementations/Category/ExamSubmissionRepository.cs`
- [x] Thêm `Task<IReadOnlyList<ExamSubmission>> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)` — `Where(x => x.SessionId == sessionId).OrderByDescending(x => x.Created)`.
- [x] Thêm `Task<IReadOnlyList<ExamSubmission>> GetBySessionAndStudentAsync(Guid sessionId, Guid studentId, CancellationToken ct = default)` — thêm điều kiện `x.StudentId == studentId`.
- [x] Verify: `dotnet build ExamHub.Core`.

## Task 2: Backend — service
**Files:** `.../Domain/Interfaces/**/IExamSubmissionService.cs`, `.../Services/Implementations/ExamSubmissionService.cs`
- [x] Thêm `GetBySessionAsync` và `GetBySessionAndStudentAsync` uỷ quyền xuống repo.

## Task 3: Backend — controller endpoints
**File:** `ExamHub.API/Controllers/Exam/ExamSubmissionController.cs`
- [x] `GET by-session/{sessionId:guid}` `[Authorize(Roles="Admin,Teacher")]` → list (không kèm answers).
- [x] `GET by-session/{sessionId:guid}/student/{studentId:guid}` → list các lần nộp của HS (không kèm answers).
- [x] Verify build + kiểm tra `ExamHub.API.json` (Swagger) không lỗi.

## Task 4: Frontend — service + hooks
**Files:** `exam_hub_web/src/services/submissionService.ts`, `exam_hub_web/src/hooks/queries/useSubmissions.ts`
- [x] `submissionService.getBySession(sessionId)`, `getBySessionAndStudent(sessionId, studentId)`.
- [x] `SUBMISSION_KEYS.bySession`, `bySessionStudent`; hooks `useSubmissionsBySessionQuery(sessionId)`, `useMySessionSubmissionsQuery(sessionId, studentId)`.

## Task 5: Frontend — tổng quát hoá `SubmissionsDrawer` theo session
**File:** `exam_hub_web/src/pages/exams/SubmissionsDrawer.tsx`
- [x] Props đổi thành `{sessionId?: string; onClose}` (bỏ nhánh examId, hoặc nhận cả hai). Dùng `useSubmissionsBySessionQuery`.
- [x] `SubmissionCard` tự nạp đề bằng `useExamWithQuestionsQuery(sub.examId)` (bỏ prop `questionContent` truyền từ ngoài); tự tính `questionContent`.
- [x] Tiêu đề Drawer: "Bài nộp kỳ thi".

## Task 6: Frontend — bỏ nút "Bài nộp" ở `ExamListPage`
**File:** `exam_hub_web/src/pages/exams/ExamListPage.tsx`
- [x] Gỡ state `submissionsExamId`, nút mở drawer, và `<SubmissionsDrawer/>` (đã chuyển sang kỳ thi).

## Task 7: Frontend — nút "Bài nộp" ở `ExamSessionListPage`
**File:** `exam_hub_web/src/pages/exams/ExamSessionListPage.tsx`
- [x] Thêm action "Bài nộp" mỗi kỳ thi → set `submissionsSessionId` → render `<SubmissionsDrawer sessionId=…/>`.

## Task 8: Frontend — HS xem lại kết quả (`StudentSessionListPage` + Modal)
**Files:** `exam_hub_web/src/pages/student/StudentSessionListPage.tsx`, mới `.../student/SessionResultsModal.tsx`
- [x] Thêm nút **"Xem kết quả"** vào chân thẻ khi `usedAttempts > 0` (cạnh nút hành động).
- [x] `SessionResultsModal`: dùng `useMySessionSubmissionsQuery(sessionId, user.id)` liệt kê lần nộp (STT, điểm, trạng thái Tag, thời gian nộp); bấm dòng → `navigate('/student/exam/result?submissionId='+id)`.

## Task 9: Verify tổng thể + commit
- [x] Build Core, tsc, eslint, vite build sạch (trừ lỗi pre-existing).
- [x] Commit backend và frontend theo nhóm task.
- [x] Cần **khởi động lại API** để nạp endpoint mới.

## Ghi chú mở rộng (KHÔNG làm trong lần này)
- Hiển thị **tên học sinh** thay cho `studentId` rút gọn (cần enrich response bằng join `app_users`).
- Lọc/nhóm bài nộp theo từng đề trong kỳ thi.
