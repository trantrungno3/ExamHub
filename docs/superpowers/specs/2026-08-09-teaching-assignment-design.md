# Phân công GV giảng dạy cho lớp (SD14) — Design Spec

**Ngày:** 2026-08-09
**Nhánh (đề xuất):** `feat/teaching-assignment`
**Nguồn thiết kế:** `uml/activity_diagrams/14-phan-cong-giao-vien.puml`, `uml/seq_diagrams/SD14_phan_cong_giao_vien.puml`

## 1. Mục tiêu

Cho phép Admin phân công **giáo viên bộ môn** giảng dạy cho từng **lớp** (cohort_class) theo từng **môn**. Khác với: GVCN (`cohort_classes.homeroom_teacher_id` — đã có) và teacher_subjects (GV dạy môn gì ở cấp toàn cục — đã có).

Ngoài phạm vi: xếp thời khoá biểu, tiết học, điểm danh.

## 2. Quyết định thiết kế (đã chốt)

- **UI:** nút "Phân công" trên từng dòng lớp (tab "Lớp học" của `CohortDetailPage`) → mở drawer quản lý phân công của lớp đó.
- **Lọc GV:** chỉ GV là thành viên trường (school_members role Teacher, is_active) của trường sở hữu khoá **VÀ** có môn đó trong teacher_subjects.
- **Ràng buộc trùng:** UNIQUE (cohort_class_id, subject_id) — 1 môn/lớp = 1 GV.

## 3. Ràng buộc xuyên suốt (GLOBAL — áp cho mọi thao tác thêm/sửa/xoá)

> **Bổ sung theo yêu cầu (2026-08-09).** Mọi endpoint thay đổi dữ liệu PHẢI theo đúng trình tự, KHÔNG dựa vào lỗi FK thô của DB:
> 1. **Validate dữ liệu đầu vào:** tồn tại, đúng kiểu, trường bắt buộc.
> 2. **Kiểm tra ràng buộc nghiệp vụ & tham chiếu:** trùng lặp; khoá ngoại còn liên kết (không cho xoá bản ghi cha còn con).
> 3. **Chỉ khi hợp lệ** mới ghi DB.
> 4. **Trả message kết quả rõ ràng** (thành công / lỗi cụ thể, tiếng Việt) — FE hiển thị qua `message`.

Áp dụng cụ thể:
- **assign (feature này):** validate class/subject/teacher tồn tại; teacher eligible (điều kiện §2); check trùng (class, subject) → lỗi `"Môn '{subject}' đã được phân công cho GV '{teacher}' trong lớp này."`.
- **remove:** validate bản ghi phân công tồn tại.
- **Ví dụ được nêu — xoá trường:** hiện `cohorts.school_id ON DELETE CASCADE` + `SchoolService.DeleteAsync` không kiểm → xoá trường sẽ xoá luôn khoá. Sửa: trước khi xoá kiểm số khoá liên kết, nếu > 0 → chặn, trả `"Không thể xoá: trường còn {n} khoá học liên kết."`. (Thiết lập pattern guard tái dùng cho các entity cha khác — làm dần, task này chỉ hiện thực ví dụ trường.)

## 4. Data model

Bảng mới `cohort_class_teachers` (thêm vào `database_schema.sql`, sau `teacher_subjects`):

```sql
CREATE TABLE public.cohort_class_teachers
(
    id              SERIAL PRIMARY KEY,
    cohort_class_id INT  NOT NULL REFERENCES cohort_classes (id) ON DELETE CASCADE,
    subject_id      INT  NOT NULL REFERENCES subjects (id)       ON DELETE CASCADE,
    teacher_id      UUID NOT NULL REFERENCES app_users (id)      ON DELETE CASCADE,
    UNIQUE (cohort_class_id, subject_id)
);
```

Kèm 1 EF migration khớp bảng (xác minh cách provisioning DB khi viết plan: `database_schema.sql` thủ công vs `dotnet ef` migration — cập nhật cả hai nếu cần).

## 5. Backend (theo pattern `TeacherSubject`)

Stack: ASP.NET, entity `IModelBaseSql<int>` + `[SqlBuilderProperty]` + FieldTable + `ToInsertObject/ToUpdateObject`; controller `[ApiController]`.

- `FieldTables/CohortClassTeacherTable.cs` — hằng tên bảng/cột.
- `Domain/Entities/CohortClassTeacher.cs` — Id, CohortClassId, SubjectId, TeacherId (+ helpers).
- `DataTransferObjects/School/CohortClassTeacherDto.cs` — kèm `SubjectName`, `TeacherName`, `ClassName` (join hiển thị) + request DTO (assign).
- `Domain/Interfaces/.../ICohortClassTeacherRepository.cs` + Impl: `GetByClassAsync(classId)`, `ExistsAsync(classId, subjectId)`, `GetEligibleTeachersAsync(classId, subjectId)`, `InsertAsync`, `DeleteByIdAsync`.
- `Domain/Interfaces/.../ICohortClassTeacherService.cs` + Impl: bọc repo, thực thi §3 (validate + constraint → message).
- `Controllers/School/CohortClassTeacherController.cs` route `api/cohort-class-teachers`:
  - `GET by-class/{cohortClassId:int}` → danh sách phân công.
  - `GET eligible-teachers?cohortClassId=&subjectId=` → GV hợp lệ (§2).
  - `POST assign` → tạo (validate + check trùng).
  - `DELETE remove/{id:int}` → xoá.
- Đăng ký DI trong `DependencyContainer.cs`.
- Guard xoá trường: sửa `SchoolService.DeleteAsync` kiểm cohorts liên kết (repo đã có `GetWithCohortsAsync`).

## 6. Frontend

- `types/school.d.ts` (hoặc mới): `CohortClassTeacher`, `EligibleTeacher`.
- `services/cohortClassTeacherService.ts`: getByClass, getEligibleTeachers, assign, remove.
- `hooks/queries/useCohortClassTeachers.ts`: query + mutations (invalidate).
- `pages/school/TeachingAssignmentDrawer.tsx`: mở từ nút "Phân công" trên dòng lớp; form (Select Môn → Select GV lọc eligible → Thêm) + bảng phân công hiện tại (Môn · GV · Xoá); lỗi trùng/validate → `message.error(<message BE>)`, thành công → `message.success`.
- `pages/school/CohortDetailPage.tsx`: thêm nút "Phân công" vào cột Thao tác của bảng lớp (tab "Lớp học") + state mở drawer.

## 7. Kiểm thử

Không có test project (BE) / test UI. Verify:
1. `dotnet build` (BE) không lỗi; `.http` gọi thử 4 endpoint (assign OK, assign trùng → lỗi message, eligible lọc đúng, remove OK; school delete khi còn khoá → chặn).
2. `npx tsc --noEmit` + `npm run build` (FE); thao tác thủ công trên drawer.

## 8. Rủi ro

- Cách provisioning DB (schema.sql thủ công vs EF migration) — xác minh để thêm bảng đúng chỗ, tránh lệch model snapshot.
- Query eligible teachers join nhiều bảng (cohort_class→cohort→school_members ∩ teacher_subjects) — kiểm bằng `.http`.
- Guard xoá trường đổi hành vi hiện tại (đang cascade) — cần rõ ràng message, không phá luồng khác.
