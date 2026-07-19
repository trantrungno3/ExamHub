# Thiết kế: Nhiều lớp song song trong một khoá + gán học sinh vào lớp

**Ngày:** 2026-07-19
**Phạm vi:** `exam_hub_api` (backend .NET) + `exam_hub_web` (frontend React)

## 1. Bối cảnh & vấn đề

Hệ thống quản lý theo cấu trúc: **Trường (School) → Khoá (Cohort) → Lớp học (CohortClass)**.

Mô hình hiện tại:
- Mỗi khoá chỉ khai báo **một** hậu tố lớp (`cohorts.class_suffix`, mặc định `'A'`).
- DB trigger `generate_cohort_classes` sinh **đúng một lớp mỗi năm** (10A → 11A → 12A). Ràng buộc `UNIQUE (cohort_id, year_index)` chặn nhiều lớp song song.
- `cohort_members` gắn học sinh với **khoá**, **không** có liên kết tới lớp cụ thể.

Cần bổ sung:
1. Một khoá có **nhiều lớp song song** (A, B, C, D, E…).
2. Gán mỗi học sinh vào **một lớp cụ thể**.

## 2. Quyết định thiết kế (đã chốt)

- **Học sinh ở nguyên một ban/lớp (section) suốt khoá** (10A → 11A → 12A). Không đổi lớp theo từng năm ⇒ lưu section ở cấp khoá (trên `cohort_members`), không cần bảng liên kết theo năm.
- **Khai báo lớp bằng số lượng**: khi tạo khoá nhập `num_classes` → hệ thống tự sinh hậu tố A, B, C… (`chr(64 + n)`).
- **Section của học sinh nullable**: học sinh có thể vào khoá trước (chưa xếp lớp), xếp lớp sau.
- **Validate**: section gán cho học sinh phải nằm trong dải hợp lệ của khoá (A .. `chr(64 + num_classes)`); kiểm ở backend service.

## 3. Mô hình dữ liệu (`exam_hub_api/database_schema.sql`)

### `cohorts`
- **Bỏ** `class_suffix VARCHAR(10) NOT NULL DEFAULT 'A'`.
- **Thêm** `num_classes SMALLINT NOT NULL DEFAULT 1` — số lớp song song, hợp lệ 1..26 (CHECK `num_classes BETWEEN 1 AND 26`).

### `cohort_classes`
- **Thêm** `section VARCHAR(10) NOT NULL DEFAULT 'A'` — ban/lớp: A, B, C…
- **Đổi** `UNIQUE (cohort_id, year_index)` → `UNIQUE (cohort_id, year_index, section)`.
- `class_name` giữ nguyên format `grade || section` (ví dụ `10A`, `10B`).

### `cohort_members`
- **Thêm** `section VARCHAR(10) NULL` — lớp của học sinh (ổn định cả khoá). NULL = chưa xếp lớp.

### Trigger `generate_cohort_classes`
Vòng lặp lồng: cho mỗi năm `i` (1..duration) × mỗi lớp `j` (0..num_classes-1):
- `section = chr(65 + j)` (A, B, C…)
- `class_name = (grade_start + i - 1) || section`
- insert dòng `cohort_classes` với `section` tương ứng.

Tổng số dòng sinh ra = `num_classes × duration`.

### Migration cho DB dev đang chạy
Repo dùng một file `database_schema.sql` (không có hệ migration versioned). Cập nhật file này **và** cung cấp đoạn `ALTER TABLE` kèm theo để nâng cấp DB dev không phải tạo lại:
- `ALTER TABLE cohorts ADD num_classes ...; UPDATE ...; ALTER TABLE cohorts DROP class_suffix;`
- `ALTER TABLE cohort_classes ADD section ...;` + đổi unique constraint.
- `ALTER TABLE cohort_members ADD section ...;`
- `CREATE OR REPLACE FUNCTION generate_cohort_classes ...` (bản mới).
- Cập nhật seed data trong `database_schema.sql` (các INSERT cohort dùng `num_classes` thay `class_suffix`).

## 4. Backend (.NET — `exam_hub_api`)

### `Cohort` (entity + `CohortTable` FieldTable + DTO)
- Đổi `ClassSuffix` (string) → `NumClasses` (int/short).
- Cập nhật `[Column]`/`[SqlBuilderProperty]`, `ToInsertObject`, DTO request/response.

### `CohortClass` (entity + `CohortClassTable` + DTO)
- Thêm `Section` (string). Column `section`, Insert=true, Update=false.
- Thêm vào `CohortClassResponse.FromEntity`.

### `CohortMember` (entity + `CohortMemberTable` + DTO)
- Thêm `Section` (nullable string). Insert=true **và** Update=true (cho phép đổi lớp).
- Cập nhật `ToInsertObject`/`ToUpdateObject`, `CohortMemberRequest.ToEntity`, `CohortMemberResponse.FromEntity`.
- Cập nhật SQL trong `CohortMemberRepository` (insert/update có cột `section`).

### `CohortMemberService` — validate + đổi lớp
- Khi `AddStudentAsync` và khi đổi section: nếu `section` không NULL, kiểm tra nằm trong dải hợp lệ của khoá (`A .. chr(64 + cohort.NumClasses)`); nếu sai → trả lỗi (ném exception nghiệp vụ / RequestResponse lỗi theo pattern hiện có).
- Thêm `SetSectionAsync(Guid id, string? section)`.

### `CohortMemberController`
- Thêm endpoint `PATCH /api/cohortmember/{id}/section` nhận body section (nullable) → `SetSectionAsync`.
- `AddStudent` nhận thêm `section` trong request (đã có sẵn body → mở rộng DTO).

### `ExamSessionRepository` (bắt buộc — tính đúng học sinh của lớp)
Hiện `GetAssignedToStudentAsync` và `IsStudentAssignedAsync` coi assignment cấp lớp (`CohortClassId`) = cả khoá (chỉ so `cc.CohortId`). Sửa lại: học sinh chỉ thuộc lớp khi
`cohort_members.section == cohort_classes.section` của lớp được giao (cùng cohort). Nếu `cohort_members.section` NULL → không thuộc lớp cụ thể nào.

## 5. Frontend (React — `exam_hub_web`)

### Types (`src/types/school.d.ts`)
- `Cohort.numClasses`, `CohortBody.numClasses` (bỏ `classSuffix`).
- `CohortClass.section`.
- `CohortMember.section`, `CohortMemberBody.section`.

### `SchoolDetailPage.tsx` — modal tạo khoá
- Thay ô "Hậu tố lớp" (`classSuffix`) → **"Số lớp"** (`numClasses`, `Input type=number`, mặc định 1).

### `CohortDetailPage.tsx`
- **Tab Lớp học**: hiển thị nhiều lớp/năm; thêm cột **Lớp (section)**. (Bảng đã liệt kê cohort_classes; chỉ thêm cột.)
- **Tab Học sinh**:
  - Thêm cột **Lớp** hiển thị section.
  - Modal "Thêm học sinh": thêm `Select` **Lớp** với options là các section của khoá (A .. theo `numClasses`), **cho phép để trống** (chưa xếp lớp).
  - Cho **đổi lớp trực tiếp trên bảng** bằng `Select` (giống cách đổi GVCN ở tab Lớp học) → gọi mutation setSection.
- Danh sách section suy ra từ `numClasses` của khoá (helper sinh `['A','B',...]`).

### `cohortMemberService.ts` + `useCohortMembers.ts`
- `add(body)` gửi kèm `section`.
- Thêm `setSection(id, section)` + hook `useSetCohortMemberSectionMutation` (invalidate `byCohort`).

## 6. Ngoài phạm vi (YAGNI)
- Đổi lớp theo từng năm học (student chuyển section giữa các năm).
- Import học sinh hàng loạt / phân lớp tự động.
- Ràng buộc FK cứng giữa `cohort_members.section` và `cohort_classes.section` (validate ở tầng service là đủ; section là thuộc tính chuỗi ổn định của khoá).

## 7. Kiểm thử
- **Backend**: unit test cho `generate_cohort_classes` (số dòng = num_classes × duration, section đúng); validate section ngoài dải bị từ chối; `ExamSessionRepository` chỉ trả HS đúng section khi assignment cấp lớp.
- **Frontend**: tạo khoá với numClasses=5 → tab Lớp học hiển thị A–E mỗi năm; thêm HS không chọn lớp (section trống) rồi đổi lớp; hiển thị cột Lớp đúng.
- **Thủ công (E2E)**: tạo khoá 5 lớp → xếp 2 HS vào 10A, 1 HS vào 10B → giao đề cho lớp 10A → chỉ 2 HS lớp A thấy đề.
