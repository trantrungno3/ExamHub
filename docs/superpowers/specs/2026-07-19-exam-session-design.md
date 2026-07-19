# Kỳ thi (Exam Session) — Design Spec

**Ngày:** 2026-07-19
**Phạm vi:** Full-stack (exam_hub_api + exam_hub_web)
**Trạng thái:** Đã duyệt thiết kế, chờ viết implementation plan

## 1. Mục tiêu

Thêm khái niệm **Kỳ thi** (Exam Session): một cấu hình thi theo **môn + cấp lớp**, được **giao cho 1 lớp hoặc 1 khoá**, chứa **nhiều đề thi**. Khi học sinh vào làm bài, hệ thống **bốc ngẫu nhiên 1 đề** trong kỳ thi và **khoá đề đó cho lượt làm hiện tại**.

Bốn mục quản lý đề (Mẫu đề thi, Sinh đề thi, Đề thi, Kỳ thi) được gom thành **1 nhóm menu cha "Quản lý đề thi"**. Phía học sinh chuyển sang danh sách **"Kỳ thi của tôi"** (thay danh sách duyệt toàn bộ đề published cũ).

## 2. Quyết định thiết kế (đã chốt)

| # | Quyết định |
|---|---|
| 1 | Bốc đề: **1 lần rồi khoá theo từng lượt làm**. Vào lại lượt đang làm = đúng đề; lượt mới = bốc lại. |
| 1b | **Chế độ chọn đề cấu hình được** (`pick_mode`): `random` = hệ thống bốc ngẫu nhiên hoàn toàn (có thể trùng đề giữa các lượt); `student_choice` = học sinh tự chọn đề từ pool cho mỗi lượt (làm xong 1 đề → hiện danh sách để chọn đề tiếp). |
| 2 | Giao cho **cả lớp (cohort_class) và khoá (cohort)**. |
| 3 | **Có khung giờ mở/đóng** (open_at, close_at) — bắt buộc. |
| 4 | Đề thêm vào kỳ thi: **chọn tay** từ đề có sẵn. |
| 5 | **Số lượt làm cấu hình được** (max_attempts, mặc định 1). |
| 6 | Menu nhóm cha **"Quản lý đề thi"**. |
| 7 | Học sinh: chuyển sang **"Kỳ thi của tôi"**, bỏ danh sách duyệt toàn bộ đề published. |
| A | Giao **khoá** → lấy học sinh qua `cohort_members` (không phân biệt lớp/năm). |
| B | Pool đề **chỉ nhận đề đúng `subject_id` + `grade_level_id`** của kỳ thi. |
| C | Đề thêm vào pool **bắt buộc `status='published'`**. |

## 3. Data model

### 3.1 Bảng mới

**`exam_sessions`** — cấu hình kỳ thi
```
id              UUID PK        DEFAULT gen_random_uuid()
title           VARCHAR(300)   NOT NULL
description     TEXT
subject_id      INT            NOT NULL REFERENCES subjects(id)
grade_level_id  INT            NOT NULL REFERENCES grade_levels(id)
open_at         TIMESTAMPTZ    NOT NULL
close_at        TIMESTAMPTZ    NOT NULL
max_attempts    SMALLINT       NOT NULL DEFAULT 1   CHECK (max_attempts >= 1)
pick_mode       VARCHAR(20)    NOT NULL DEFAULT 'random'
                CHECK (pick_mode IN ('random','student_choice'))
status          VARCHAR(20)    NOT NULL DEFAULT 'draft'
                CHECK (status IN ('draft','published','closed'))
created/created_by/modified/modified_by  (chuẩn ModifyModelBase)
CHECK (close_at > open_at)
```

**`exam_session_exams`** — pool đề của kỳ thi (nhiều-nhiều với exams)
```
id          UUID PK
session_id  UUID NOT NULL REFERENCES exam_sessions(id) ON DELETE CASCADE
exam_id     UUID NOT NULL REFERENCES exams(id)
UNIQUE (session_id, exam_id)
```

**`exam_session_assignments`** — giao cho lớp/khoá
```
id               UUID PK
session_id       UUID NOT NULL REFERENCES exam_sessions(id) ON DELETE CASCADE
cohort_id        INT  NULL REFERENCES cohorts(id) ON DELETE CASCADE
cohort_class_id  INT  NULL REFERENCES cohort_classes(id) ON DELETE CASCADE
CHECK ( (cohort_id IS NOT NULL)::int + (cohort_class_id IS NOT NULL)::int = 1 )  -- đúng 1
UNIQUE (session_id, cohort_id, cohort_class_id)
```

### 3.2 Sửa bảng có sẵn

**`exam_submissions`** — thêm 2 cột (đều nullable để giữ tương thích đề trực tiếp cũ):
```
session_id  UUID     NULL REFERENCES exam_sessions(id)
attempt_no  SMALLINT NOT NULL DEFAULT 1
```
`session_id` NULL = submission cho đề trực tiếp (luồng cũ). NOT NULL = thuộc một kỳ thi.

### 3.3 Index gợi ý
- `exam_sessions (subject_id, grade_level_id, status)`
- `exam_session_exams (session_id)`
- `exam_session_assignments (session_id)`, `(cohort_id)`, `(cohort_class_id)`
- `exam_submissions (session_id, student_id)`

## 4. Logic "bốc đề + khoá" (mấu chốt)

Endpoint `POST /exam-sessions/{id}/start` (học sinh), body `{ examId? }`, chạy trong transaction:

1. **Validate**: kỳ thi tồn tại & `status='published'`; `now ∈ [open_at, close_at]`; học sinh nằm trong tập được giao (xem §5.3).
2. Tìm submission `in_progress` của `(session_id, student_id)`:
   - Có → trả lại submission đó (đúng đề đang làm) — hành vi **"Tiếp tục"**. (Bỏ qua `examId` truyền vào.)
3. Chưa có in_progress → đếm số submission đã nộp (`submitted` + `graded`) của `(session, student)` = `usedAttempts`:
   - `usedAttempts >= max_attempts` → lỗi 409 "Đã hết lượt làm bài".
   - Xác định `exam_id` cho lượt mới theo **`pick_mode`**:
     - `random`: **bốc ngẫu nhiên 1 exam_id** từ pool (`exam_session_exams`). Có thể trùng đề đã làm ở lượt trước.
     - `student_choice`: **bắt buộc** có `examId` trong body; validate `examId ∈ pool`. (Khuyến nghị: chặn chọn đề mà học sinh đã hoàn thành trong kỳ thi này — trả 409 nếu đã làm.)
   - Tạo `exam_submissions` (`session_id`, `exam_id`, `student_id`, `status='in_progress'`, `attempt_no = usedAttempts + 1`, `started_at=now`); trả về `{ submissionId, examId }`.

→ Đề khoá theo **từng lượt**. Kết quả trả về đủ để phía web mở luồng làm bài sẵn có.

## 5. Backend API

### 5.1 Quản lý (Admin/Teacher) — `/exam-sessions`
- `GET /exam-sessions` — danh sách (lọc subject/grade/status/keyword, phân trang).
- `GET /exam-sessions/{id}` — chi tiết kèm pool đề + assignments.
- `POST /exam-sessions` — tạo (title, description, subjectId, gradeLevelId, openAt, closeAt, maxAttempts).
- `PUT /exam-sessions/{id}` — sửa cấu hình.
- `DELETE /exam-sessions/{id}`.
- `POST /exam-sessions/{id}/exams` `{ examIds: [] }` / `DELETE /exam-sessions/{id}/exams/{examId}` — quản lý pool.
  - **Validate B+C**: mỗi exam phải cùng `subject_id`+`grade_level_id` với kỳ thi và `status='published'`.
- `POST /exam-sessions/{id}/assignments` `{ cohortId? , cohortClassId? }` / `DELETE .../assignments/{assignmentId}` — giao/bỏ giao.
- `POST /exam-sessions/{id}/publish` — chuyển `published` (validate: có ≥1 đề trong pool, có ≥1 assignment, close_at còn ở tương lai).
- `POST /exam-sessions/{id}/close` — chuyển `closed`.

### 5.2 Học sinh
- `GET /exam-sessions/my` — kỳ thi được giao cho học sinh hiện tại; mỗi item kèm: `pickMode`, trạng thái theo thời gian (sắp mở / đang mở / đã đóng), số lượt đã dùng, số lượt còn lại, và (nếu đang có lượt in_progress) submissionId+examId để "Tiếp tục".
- `GET /exam-sessions/{id}/pool` — (dùng cho `student_choice`) danh sách đề trong pool kèm trạng thái của học sinh với từng đề: `notStarted` / `inProgress` / `completed` (+ submissionId nếu có). Chỉ trả khi HS được giao và kỳ thi đang mở.
- `POST /exam-sessions/{id}/start` — §4, body `{ examId? }`.

### 5.3 Xác định học sinh được giao
Học sinh `S` được giao kỳ thi `E` nếu tồn tại assignment của `E` mà:
- `cohort_id = C` và `S ∈ cohort_members(C, is_active)`, **hoặc**
- `cohort_class_id = K` và `S ∈ cohort_members(cohort_of(K), is_active)` (lớp thuộc khoá nào thì lấy học sinh khoá đó).

### 5.4 Sửa luồng nộp bài
`submit` hiện tạo submission mới lúc nộp. Cho nhánh **session**: submit phải **cập nhật** submission `in_progress` đang tồn tại (do `start` tạo) — chấm trắc nghiệm tự động, set `submitted_at`, `duration_seconds`, `status='submitted'`. Truyền `submissionId` trong body submit. **Nhánh đề trực tiếp cũ (session_id NULL) giữ nguyên** hành vi tạo-lúc-nộp để không phá vỡ tính năng hiện có.

## 6. Menu (gom nhóm)

- `MenuItemResponse`: thêm `Children: MenuItemResponse[]?` (nhóm cha có con, `Path` có thể null với nhóm chỉ để mở/thu).
- `MenuRegistry`: mô tả nhóm cha **"Quản lý đề thi"** (key `exam-mgmt`) chứa các con: `exams` (Mẫu đề thi), `generate` (Sinh đề thi), `exam-list` (Đề thi), `exam-sessions` (**Kỳ thi**). Roles: Admin, Teacher.
- Frontend `AppLayout`: render nhóm cha thu/mở được; mục con active theo `location.pathname`. `FALLBACK_NAV` cập nhật tương ứng. Thêm icon cho "Kỳ thi".

## 7. Frontend

### 7.1 Quản lý (Admin/Teacher)
- **`ExamSessionListPage`** (`/app/exam-sessions`): bảng danh sách (môn, cấp lớp, khung giờ, trạng thái, số đề, số lớp/khoá được giao), nút Tạo, Sửa, Xoá, Publish/Close.
- **`ExamSessionEditPage`** (`/app/exam-sessions/create`, `/app/exam-sessions/:id/edit`):
  - Thông tin: title, description, môn, cấp lớp, open_at, close_at, max_attempts, **pick_mode** (Ngẫu nhiên / Học sinh tự chọn).
  - **Chọn đề vào pool**: bảng/transfer chọn từ đề `published` cùng môn+cấp lớp.
  - **Giao lớp/khoá**: chọn cohort hoặc cohort_class (multi).
  - Nút Publish.

### 7.2 Học sinh
- **`StudentSessionListPage`** = "Kỳ thi của tôi" (route hiện `ROUTES.STUDENT_EXAMS`): danh sách kỳ thi được giao + trạng thái + lượt còn lại.
  - Nút "Tiếp tục" (khi có lượt in_progress) → `POST /start` → nhận `{ examId, submissionId }` → vào luồng làm bài.
  - Kỳ thi `pick_mode='random'`: nút "Vào thi" → `POST /start` (không kèm examId) → bốc ngẫu nhiên → vào làm.
  - Kỳ thi `pick_mode='student_choice'`: nút "Chọn đề" → trang **chọn đề** (`GET /{id}/pool`): hiện danh sách đề, đề đã hoàn thành được đánh dấu; chọn 1 đề chưa làm → `POST /start { examId }` → vào làm. Làm xong quay lại trang này để chọn đề tiếp (tới khi hết lượt).
- **Tái dùng** `ExamCoverPage` + `ExamTakingPage`. Truyền thêm `sessionId`/`submissionId` để submit cập nhật đúng submission (§5.4). `StudentExamListPage` cũ **giữ lại file** (không xoá), chỉ đổi entry chính sang `StudentSessionListPage`.

## 8. Ngoài phạm vi (YAGNI)
Chống gian lận nâng cao (đổi tab/camera), chấm lại hàng loạt, thông báo/nhắc lịch, thống kê kết quả theo kỳ thi. Có thể làm ở giai đoạn sau.

## 9. Rủi ro & điểm cần lưu ý
- **Sửa luồng submit** (§5.4) chạm vào tính năng đang chạy → cần giữ tương thích nhánh đề trực tiếp và test kỹ.
- **Bốc ngẫu nhiên** cần transaction + kiểm tra lượt để tránh tạo trùng nhiều submission khi double-click.
- **Migration** thêm cột `exam_submissions.session_id/attempt_no` phải nullable/có default để không vỡ dữ liệu cũ.
- Đảm bảo `exam_hub_api/database_schema.sql` được cập nhật đồng bộ với migration EF.
