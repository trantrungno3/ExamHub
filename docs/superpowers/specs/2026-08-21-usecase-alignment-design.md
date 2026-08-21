# Thiết kế: Cập nhật hệ thống khớp đặc tả ca sử dụng

> Ngày: 2026-08-21
> Nguồn đối chiếu: `uml/dac-ta-ca-su-dung.md` (45 use case)
> Mục tiêu: đóng 6 điểm lệch giữa code hiện tại và đặc tả, không đụng tới các chức năng ngoài đặc tả.

## Bối cảnh

Kết quả đối chiếu 45 use case cho thấy phần lớn chức năng đã theo đúng luồng.
Tồn tại 6 điểm lệch cần xử lý (thứ tự ưu tiên theo tác động):

1. **UC33** — Làm bài: không có tự lưu tạm định kỳ → rủi ro mất bài khi reload/mất mạng.
2. **UC34** — Chấm điểm: thiếu trạng thái "Chờ chấm tay" (enum chỉ có InProgress/Submitted/Graded).
3. **UC13** — Thêm học sinh vào lớp: không chặn HS đã thuộc lớp khác cùng khối.
4. **UC24** — Xoá câu hỏi: không cảnh báo khi câu hỏi đang dùng trong đề thi.
5. **UC5/10/18** — Xoá trường/khoá học/danh mục: không cảnh báo "đang được sử dụng".
6. **UC43** — Xoá người dùng: không cảnh báo "đang có dữ liệu liên quan".

Quyết định thiết kế đã chốt với chủ dự án:
- Phạm vi: xử lý **cả 6 điểm lệch**.
- Autosave: **server-side** (API save-progress).
- Trạng thái chấm tay: **thêm enum `PendingManualGrade`**.
- Xoá khi đang dùng: **cảnh báo (409) + tuỳ chọn xoá bắt buộc `force=true`**.

## Kiến trúc & thành phần

Hệ thống gồm backend .NET (`exam_hub_api`, kiến trúc Domain/Application/Infrastructure)
và frontend React + AntD (`exam_hub_web`). Mỗi thay đổi đi xuyên các tầng
Controller → Service → Repository (BE) và Service/Page/Constants (FE).

---

### A. UC34 — Trạng thái "Chờ chấm tay"

**Domain**
- `ExamHub.Core/Domain/Enums/SubmissionStatusEnum.cs`: thêm `PendingManualGrade = 4`
  (đặt sau `Submitted = 2`, giữ `Graded = 3` để không phá dữ liệu cũ; giá trị mới = 4).

**Application/Infrastructure**
- `ExamSubmissionService`:
  - `SubmitAsync` và `SubmitInProgressAsync`: sau khi `AutoGradeObjectiveAsync`,
    quyết định trạng thái cuối:
    - Nếu bài **có ít nhất một câu tự luận** (answer có `EssayContent` hoặc
      ExamQuestion loại tự luận và chưa có điểm) → `PendingManualGrade`.
    - Nếu **toàn trắc nghiệm** → `Graded` (chấm tự động xong là chốt luôn).
  - `FinalizeAsync`: cho phép chuyển từ `Submitted`/`PendingManualGrade` → `Graded`.
- Cách phát hiện câu tự luận: dựa vào `SubmissionAnswer.EssayContent != null`
  hoặc `SelectedAnswerIds` rỗng nhưng thuộc ExamQuestion loại tự luận
  (`QuestionTypeEnum`). Chọn tiêu chí: **answer có EssayContent** (đơn giản, đủ dùng
  vì FE luôn gửi EssayContent cho câu tự luận).

**Frontend**
- `types/submission.d.ts`: `SubmissionStatus` thêm `'PendingManualGrade'`.
- `constants/index.ts`: cập nhật 4 map nhãn/màu:
  - `SUBMISSION_STATUS_LABEL`: `Submitted='Đã nộp'`, `PendingManualGrade='Chờ chấm tay'`, `Graded='Đã chấm'`.
  - `SUBMISSION_STATUS_LABEL_STUDENT`: `PendingManualGrade='Đã nộp (chờ chấm)'`.
  - `SUBMISSION_STATUS_VARIANT` / `SUBMISSION_STATUS_TAG_COLOR`: PendingManualGrade dùng `warning`/`gold`.
- `SubmissionListPage.tsx`: bộ lọc "cần chấm" dùng `PendingManualGrade`.

**Lưu ý dữ liệu cũ**: submission cũ đang `Submitted` (có tự luận) sẽ vẫn hiển thị
"Đã nộp" thay vì "Chờ chấm tay" — chấp nhận được, không cần backfill.

---

### B. UC33 — Tự lưu tạm bài làm (server-side)

**API**
- `ExamSubmissionController`: `PUT /api/exam-submissions/{id}/progress`
  - Body: danh sách đáp án tạm (giống `ExamSubmissionRequest.Answers`).
  - Chỉ áp dụng cho bản `InProgress`; **không đổi trạng thái, không chấm điểm**.
  - Trả 200 (idempotent). Nếu bản không tồn tại/không phải InProgress → 409/404.

**Service/Repository**
- `IExamSubmissionService.SaveProgressAsync(Guid submissionId, IEnumerable<SubmissionAnswer>, ct)`.
- `ISubmissionAnswerRepository.UpsertRangeAsync(...)`: xoá đáp án cũ của submission
  rồi thêm mới (đơn giản, tránh phức tạp diff), hoặc upsert theo `ExamQuestionId`.
  Chọn: **replace-all theo submissionId** trong một transaction.

**Frontend**
- `ExamTakingPage.tsx`:
  - `setInterval` ~20s (và khi đổi đáp án, debounce) gọi save-progress với đáp án hiện tại.
  - Khi vào lại bài (start trả submission InProgress đã có đáp án, hoặc GET submission)
    → nạp đáp án đã lưu vào state để khôi phục.
- `submissionService.ts`: thêm `saveProgress(submissionId, answers)`.

---

### C. UC13 — Chặn học sinh trùng lớp cùng khối

**Service/Repository**
- `CohortMemberService.AddStudentAsync`: trước khi thêm, kiểm tra HS đã là thành viên
  **active** của cùng cohort → ném `InvalidOperationException`
  *"Học sinh đã thuộc lớp khác trong khối này."*
- `ICohortMemberRepository.ExistsActiveMembershipAsync(int cohortId, Guid studentId, ct)`.
- Diễn giải "cùng khối": trong mô hình hiện tại một cohort = một khối/khoá; các
  section (A/B/…) là lớp con. Ràng buộc = 1 HS chỉ thuộc 1 section active trong 1 cohort.

**Frontend**
- `CohortDetailPage.tsx`: hiển thị message lỗi trả về (cơ chế hiện có).

---

### D. UC5/10/18/24/43 — Cảnh báo xoá khi đang dùng + xoá bắt buộc

**Nguyên tắc chung**
- Controller Delete nhận thêm `?force=bool` (mặc định false).
- Service kiểm tra tham chiếu:
  - Đang dùng và `force=false` → ném exception nghiệp vụ → controller trả **409 Conflict**
    kèm thông báo rõ nội dung ("đang được sử dụng bởi …").
  - `force=true` → xoá bắt buộc (xoá theo thứ tự phụ thuộc hoặc dựa FK cascade nếu có).

**Điểm áp dụng**
- **Câu hỏi (UC24)** — `QuestionService.DeleteAsync(id, force)`: check còn `ExamQuestion`
  tham chiếu `QuestionId`. Cần `IExamQuestionRepository.ExistsByQuestionAsync(questionId)`.
  (An toàn về nội dung do đề đã snapshot; force chỉ xoá bản ghi câu hỏi gốc.)
- **Trường (UC5)** — `SchoolService.DeleteAsync(id, force)`: check còn Cohort/lớp/thành viên.
- **Khoá học (UC10)** — `CohortService.DeleteAsync(id, force)`: check còn lớp active/thành viên.
- **Danh mục (UC18)** — các category service (Subject/Topic/DifficultyLevel/CognitiveLevel/
  QuestionType/GradeLevel): check còn câu hỏi/đề/… tham chiếu. `CategoryBaseController.Delete`
  bổ sung tham số `force` và uỷ quyền cho service kiểm tra.
- **Người dùng (UC43)** — `UserController.Delete`/service: check còn submission/đề thi liên quan.

**Controller layer**
- `CategoryBaseController.Delete(TKey id, bool force=false, ct)`: gọi `service.DeleteAsync(id, force, ct)`.
  Bắt exception nghiệp vụ → `Conflict(RequestResponse.Error(...))`.
- Các controller riêng (Question, School, Cohort, User) tương tự.

**Frontend**
- Tầng service bắt HTTP 409 → trả về flag/nội dung để trang hiển thị Modal.confirm
  "… đang được sử dụng. Xoá bắt buộc?" → gọi lại API kèm `force=true`.
- Các trang: `QuestionBankPage`, `SchoolListPage`/`SchoolDetailPage`, `CohortDetailPage`,
  `CategoryPage`, `UserPage`.

---

## Luồng dữ liệu (tóm tắt)

- **Làm bài**: Start → tạo/lấy InProgress → (định kỳ) PUT progress upsert answers →
  Submit → AutoGrade → status = PendingManualGrade|Graded → (nếu pending) Finalize → Graded.
- **Xoá**: FE Delete → BE check tham chiếu → 409 nếu đang dùng → FE confirm →
  Delete?force=true → xoá.

## Xử lý lỗi

- Save-progress trên bản không InProgress: 409, không ghi.
- Add-student trùng khối: 400/409 với message tiếng Việt rõ ràng.
- Delete-in-use không force: 409 với danh mục/số lượng đối tượng liên quan (nếu rẻ để đếm).

## Kiểm thử

**Backend (unit, theo pattern sẵn có)**
- Submit: bài toàn trắc nghiệm → Graded; bài có tự luận → PendingManualGrade; Finalize → Graded.
- SaveProgress: giữ InProgress, thay đáp án, không chấm.
- AddStudent: HS đã active trong cohort → ném lỗi; HS mới → OK.
- Delete: đang dùng + force=false → exception/409; force=true → xoá; không dùng → xoá.

**Thủ công E2E**
- Làm bài → chờ autosave → reload → đáp án khôi phục → nộp → (có tự luận) trạng thái
  "Chờ chấm tay" → GV chấm tay → chốt điểm → "Đã chấm".
- Thử xoá câu hỏi/danh mục đang dùng → thấy cảnh báo → xoá bắt buộc.

## Ngoài phạm vi (YAGNI)

- Không backfill dữ liệu submission cũ.
- Không đồng bộ autosave đa thiết bị (chỉ lưu server theo submission hiện tại).
- Không thêm revoke token phía server cho UC2 (JWT stateless — logout client-side là hợp lệ).
- Không refactor ngoài các điểm nêu trên.
