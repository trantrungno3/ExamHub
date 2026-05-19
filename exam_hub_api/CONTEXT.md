# ExamHub — Hệ Thống Tạo Sinh Đề Thi

Hệ thống ASP.NET Core (.NET 10) cho phép giáo viên cấu hình ngân hàng câu hỏi và sinh đề thi tự động theo Bloom's Taxonomy, độ khó, môn học, và lớp học. Phục vụ ba vai trò: Admin, Teacher, Student.

---

## Language

### Content Hierarchy

**GradeLevel (Lớp học)**: Cấp lớp trừu tượng 1–12 trong hệ thống giáo dục Việt Nam.
_Avoid_: `Class`, `Grade`, `Level`.

**Subject (Môn học)**: Môn học được dạy tại một **GradeLevel** cụ thể (VD: Toán lớp 10).
_Avoid_: `Course`, `Discipline`.

**Topic (Chủ đề)**: Đơn vị phân loại nội dung bên trong một **Subject**, tự tham chiếu qua `parent_id` để tạo cây chương → bài.
_Avoid_: `Chapter`, `Unit`, `Category`, `Section`.

**Question (Câu hỏi)**: Đơn vị ngân hàng câu hỏi gắn với một **Topic**, lưu nội dung HTML/Markdown và `content_plain` cho full-text search.
_Avoid_: `Item`, `Problem`, `Task`.

**QuestionAnswer (Đáp án)**: Một lựa chọn đáp án thuộc một **Question**, có cờ `is_correct`.
_Avoid_: `Option`, `Choice`, `Answer` (xung đột với SubmissionAnswer).

### Question Classification

**DifficultyLevel (Mức độ khó)**: Phân loại độ khó của **Question** — `easy` / `medium` / `hard` / `very_hard`.
_Avoid_: `Level`, `Complexity`, `Hardness`.

**QuestionType (Loại câu hỏi)**: Hình thức trình bày của **Question** — `multiple_choice`, `essay`, ...
_Avoid_: `Format`, `Kind`, `Type` (mơ hồ).

**CognitiveLevel (Cấp độ nhận thức)**: Một trong 6 cấp Bloom's Taxonomy (Anderson & Krathwohl 2001) — `remember` → `understand` → `apply` → `analyze` → `evaluate` → `create`; nullable trên **Question** nghĩa là "chưa phân loại".
_Avoid_: `BloomLevel`, `Bloom`, `SkillLevel`, `KnowledgeLevel`.

### Exam Authoring

**ExamTemplate (Mẫu đề thi)**: Cấu hình tái sử dụng để sinh **Exam**, gắn một **GradeLevel** và một **Subject**.
_Avoid_: `Template`, `Blueprint`, `ExamConfig`, `ExamSpec`.

**ExamTemplateSection (Phần thi)**: Một section trong **ExamTemplate** định nghĩa **Topic** + **QuestionType** + số câu + tỉ lệ **DifficultyLevel** (%) + **CognitiveLevel** filter (nullable).
_Avoid_: `Part`, `Block`, `Section`, `Component`.

### Exam Artifacts

**Exam (Đề thi)**: Đề thi cụ thể — kết quả sinh từ **ExamTemplate** hoặc tạo thủ công — với `ExamCode`, `DurationMinutes`, `Status` (Draft/Published/Archived), `SchoolYear`, `Semester`.
_Avoid_: `Test`, `Paper`, `ExamInstance`, `Quiz`.

**ExamQuestion (Câu hỏi trong đề)**: Bản snapshot bất biến của một **Question** tại thời điểm sinh đề, lưu `content_snapshot` và `answers_snapshot` (JSONB).
_Avoid_: `QuestionSnapshot`, `ExamItem`, `Item`.

**Batch (Lô đề)**: Tập **Exam** được sinh cùng lúc từ một **ExamTemplate** chia sẻ `batch_id`; mỗi biến thể có `variant_index` và trỏ về `parent_exam_id`.
_Avoid_: `Set`, `Group`, `Series`, `Pool`.

### Submissions

**ExamSubmission (Bài nộp)**: Bản ghi lần làm bài của một Student trên một **Exam**, lưu `Status` (InProgress/Submitted/Graded), `total_score`, `is_passed`.
_Avoid_: `Attempt`, `Response`, `ExamResult`, `Submission`.

**SubmissionAnswer (Câu trả lời)**: Câu trả lời của Student cho một **ExamQuestion** bên trong một **ExamSubmission**.
_Avoid_: `Answer` (xung đột với QuestionAnswer), `Response`, `UserAnswer`.

### Organization

**School (Trường học)**: Tổ chức giáo dục có mã `code` duy nhất (VD: `THPT-NGUYEN-DU`).
_Avoid_: `Organization`, `Institution`, `Tenant`.

**Cohort (Khoá học)**: Đơn vị tuyển sinh theo năm thuộc một **School** (VD: "Khoá 2020-2025"); DB trigger tự tạo các **CohortClass** tương ứng khi INSERT.
_Avoid_: `Batch` (xung đột với lô đề thi), `Year`, `Generation`, `Intake`.

**CohortClass (Lớp học cụ thể)**: Lớp thực tế bên trong một **Cohort**, định danh bằng **GradeLevel** + `suffix` (VD: "10A").
_Avoid_: `Class`, `Classroom`, `Section`.

**SchoolMember (Thành viên trường)**: Liên kết một **AppUser** (Teacher/Admin) với một **School**.
_Avoid_: `Staff`, `Employee`, `Teacher` (xung đột với role).

**CohortMember (Học sinh trong khoá)**: Liên kết một **AppUser** (Student) với một **Cohort**.
_Avoid_: `Student` (xung đột với role), `Enrollment`, `Learner`.

**TeacherSubject (Phân công giảng dạy)**: Liên kết một **AppUser** (Teacher) với một **Subject** mà giáo viên đó phụ trách.
_Avoid_: `Assignment` (mơ hồ), `Teaching`, `TeacherAssignment`.

### Identity

**AppUser (Người dùng)**: Tài khoản hệ thống (UUID) với mảng `roles[]` gồm `Admin` / `Teacher` / `Student`, xác thực qua JWT Bearer + Refresh Token.
_Avoid_: `User` (xung đột ASP.NET Identity), `Account`, `Member`, `Principal`.

---

## Relationships

**Content tree**
- **GradeLevel** 1..N **Subject**
- **Subject** 1..N **Topic** (Topic là cây, **Topic** 1..N **Topic** qua `parent_id`)
- **Topic** 1..N **Question**
- **Question** 1..N **QuestionAnswer** (≥ 1 có `is_correct = true`)

**Question classification**
- **Question** N..1 **DifficultyLevel**
- **Question** N..1 **QuestionType**
- **Question** N..0..1 **CognitiveLevel** (nullable — "chưa phân loại")

**Exam authoring**
- **ExamTemplate** N..1 **GradeLevel**, N..1 **Subject**
- **ExamTemplate** 1..N **ExamTemplateSection**
- **ExamTemplateSection** N..1 **Topic**, N..1 **QuestionType**, N..0..1 **CognitiveLevel**

**Exam artifacts**
- **Exam** N..0..1 **ExamTemplate** (nullable — đề có thể được tạo thủ công)
- **Exam** 1..N **ExamQuestion** (snapshot, bất biến)
- **Exam** N..0..1 **Exam** qua `parent_exam_id` (variant trong **Batch**)
- **Batch** không phải entity — nó là tập **Exam** dùng chung `batch_id`

**Submissions**
- **ExamSubmission** N..1 **Exam**, N..1 **AppUser** (Student)
- **ExamSubmission** 1..N **SubmissionAnswer**
- **SubmissionAnswer** N..1 **ExamQuestion**

**Organization & identity**
- **School** 1..N **Cohort**, 1..N **SchoolMember**
- **Cohort** 1..N **CohortClass**, 1..N **CohortMember**
- **AppUser** N..N **School** qua **SchoolMember** (role Teacher/Admin)
- **AppUser** N..N **Cohort** qua **CohortMember** (role Student)
- **AppUser** N..N **Subject** qua **TeacherSubject** (role Teacher)

---

## Example dialogue

> **Dev:** "Khi Teacher tạo **Question**, có phải chọn **CognitiveLevel** không?"
> **Domain expert:** "Không — `cognitive_level_id` là nullable. Câu hỏi chưa phân loại Bloom vẫn dùng được trong **ExamTemplateSection** không bật filter Bloom."

> **Dev:** "**GradeLevel**, **Cohort**, **CohortClass** khác nhau thế nào?"
> **Domain expert:** "**GradeLevel** là cấp lớp trừu tượng (10, 11, 12). **Cohort** là khoá tuyển sinh thực tế tại một **School** (Khoá 2020-2025 của THPT Nguyễn Du). **CohortClass** là lớp cụ thể trong khoá đó (10A, 11A). Đừng dùng từ 'lớp' mơ hồ — chỉ rõ cái nào."

> **Dev:** "**ExamTemplate** và **Exam** khác nhau ra sao?"
> **Domain expert:** "**ExamTemplate** là cấu hình tái sử dụng — bao nhiêu câu, tỉ lệ độ khó, filter Topic. **Exam** là kết quả sinh ra, kèm theo **ExamQuestion** snapshot bất biến — sửa Question gốc không ảnh hưởng Exam đã sinh."

> **Dev:** "Một lượt sinh đề ra 10 biến thể cho 10 học sinh thì gọi là gì?"
> **Domain expert:** "**Batch**. 10 **Exam** dùng chung `batch_id`, có `variant_index` 0–9, và đều trỏ `parent_exam_id` về Exam đầu tiên."

> **Dev:** "Khi học sinh nộp, điểm lưu ở **ExamSubmission** hay **SubmissionAnswer**?"
> **Domain expert:** "**ExamSubmission** giữ `total_score` và `is_passed` tổng hợp. **SubmissionAnswer** lưu nội dung từng câu trả lời, để chấm hoặc audit."

> **Dev:** "Teacher A dạy Toán 10 và Lý 11 — quan hệ nào lưu chuyện đó?"
> **Domain expert:** "**TeacherSubject**: một row cho (Teacher A, Subject `Toán-10`), một row cho (Teacher A, Subject `Lý-11`). Còn quan hệ Teacher A với trường nằm ở **SchoolMember**."

---

## Flagged ambiguities

- **"Class" → GradeLevel**: khi nói đến cấp lớp trừu tượng (Lớp 10, Lớp 11). _Resolution_: luôn dùng **GradeLevel**; cấm dùng "Class" hoặc "Grade" trần.
- **"Class" → CohortClass**: khi nói đến lớp học thực tế trong một khoá (10A, 11A của THPT Nguyễn Du). _Resolution_: luôn dùng **CohortClass**; cấm dùng "Class" hoặc "Classroom" trần.
- **"Batch"** → có thể là **Batch** (lô đề thi sinh cùng lúc) hoặc **Cohort** (khoá tuyển sinh). _Resolution_: "Batch" chỉ dành cho lô đề; khoá tuyển sinh luôn là **Cohort**.
- **"Answer"** → có thể là **QuestionAnswer** (đáp án trong ngân hàng) hoặc **SubmissionAnswer** (câu trả lời của Student). _Resolution_: luôn dùng tên đầy đủ; không được dùng "Answer" trần.
- **"Teacher"** → có thể là `role` của **AppUser** hoặc **SchoolMember** (liên kết với trường). _Resolution_: "Teacher" chỉ là tên role; danh tính trong trường là **SchoolMember**; phân công môn là **TeacherSubject**.
- **"Student"** → có thể là `role` của **AppUser** hoặc **CohortMember**. _Resolution_: "Student" chỉ là tên role; danh tính trong khoá là **CohortMember**.
- **"Level"** → có thể là **GradeLevel**, **DifficultyLevel**, hoặc **CognitiveLevel**. _Resolution_: cấm dùng "Level" trần.
- **"Template"** → đôi khi bị nhầm là **Exam** đã sinh. _Resolution_: **ExamTemplate** = cấu hình; **Exam** = artifact đã materialize.
- **"Section"** → tránh dùng trần vì xung đột giữa **ExamTemplateSection** và một số nghĩa "phần trong lớp"; trong dự án này chỉ có **ExamTemplateSection**.
