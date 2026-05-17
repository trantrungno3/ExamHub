# Hệ Thống Tạo Sinh Đề Thi (ExamHub)
> Context file — paste vào đầu cuộc hội thoại mới để tiếp tục làm việc.
> Phiên bản: v3 — Cập nhật Bloom's Taxonomy (cognitive_levels) + Đổi tên dự án → ExamHub + School Management Module

---

## 1. Tổng Quan Hệ Thống

Hệ thống tạo sinh đề thi tự động cho phép giáo viên/admin cấu hình và sinh đề thi theo trình độ lớp học (1–12), môn học, chủ đề, độ khó và **cấp độ nhận thức theo Thang đo Bloom's Taxonomy**. Hệ thống quản lý ngân hàng câu hỏi và cho phép sinh đề thi đa dạng theo yêu cầu, đảm bảo chất lượng đề thi theo chuẩn giáo dục.

---

## 2. System Design

### 2.1 Kiến Trúc Tổng Thể

```
┌──────────────────────────────────────────────────────────────────┐
│                          CLIENT LAYER                            │
│         React + Vite + TypeScript + TailwindCSS                  │
│         (Admin Dashboard  |  Teacher Portal  |  Student Portal)  │
└─────────────────────┬────────────────────────────────────────────┘
                      │ HTTPS / REST API (JSON)
┌─────────────────────▼────────────────────────────────────────────┐
│                        NGINX Reverse Proxy                        │
│              CORS  |  Rate Limiting  |  SSL Termination           │
└─────────────────────┬────────────────────────────────────────────┘
                      │
┌─────────────────────▼────────────────────────────────────────────┐
│             BACKEND — ASP.NET Core Web API (.NET 8)               │
│                                                                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  Config         │  │  Question Bank  │  │  Exam Generator │  │
│  │  Module         │  │  Module         │  │  Module         │  │
│  │  Controllers +  │  │  Controllers +  │  │  Service +      │  │
│  │  Services       │  │  Repository     │  │  Algorithm      │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│                                                                  │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │  Auth / User    │  │  Result &       │  │  Export         │  │
│  │  Module         │  │  Analytics      │  │  Module         │  │
│  │  ASP.NET Core   │  │  Module         │  │  QuestPDF +     │  │
│  │  Identity + JWT │  │                 │  │  ClosedXML      │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘  │
│                                                                  │
│  ─────────────── Infrastructure Layer ──────────────────────── │
│  Entity Framework Core 8  |  Repository Pattern  |  CQRS/MediatR │
└─────────────────────┬────────────────────────────────────────────┘
                      │
┌─────────────────────▼────────────────────────────────────────────┐
│                         DATA LAYER                                │
│   PostgreSQL 16     │   Redis 7 (Cache)   │   MinIO (S3 Files)   │
└──────────────────────────────────────────────────────────────────┘
```

### 2.2 Các Module Chính

| Module | Chức năng | .NET Implementation |
|--------|-----------|---------------------|
| **Config Module** | Quản lý lớp học, môn học, chủ đề, độ khó, loại câu hỏi, **cấp độ nhận thức (Bloom)** | Controller + Service + EF Core |
| **Question Bank Module** | CRUD câu hỏi, lọc theo Bloom, tìm kiếm full-text, duyệt câu hỏi | Repository Pattern + Specification |
| **Exam Generator Module** | Thuật toán sinh đề theo điều kiện, hỗ trợ lọc Bloom | ExamGeneratorService + Strategy Pattern |
| **Auth / User Module** | Đăng nhập, phân quyền theo role | ASP.NET Core Identity + JWT Bearer |
| **Result & Analytics** | Lưu kết quả, thống kê theo cấp độ Bloom, báo cáo | Background Service + CQRS/MediatR |
| **Export Module** | Xuất PDF, Word, Excel | QuestPDF + ClosedXML |
| **File Storage** | Upload/download ảnh, file đề thi | MinIO SDK (S3-compatible) |
| **School Management Module** | Quản lý trường, khoá học, lớp học, phân công giáo viên, enroll học sinh | Controller + Service + EF Core |

### 2.3 Luồng Hoạt Động Chính

```
[Admin] → Cấu hình lớp học, môn học, chủ đề, độ khó, loại câu hỏi
        → Cấu hình cấp độ nhận thức (cognitive_levels — Bloom's Taxonomy)
    ↓
[Giáo viên] → Nhập câu hỏi vào ngân hàng
              (upload ảnh → MinIO)
              Gán: lớp / môn / chủ đề / độ khó / loại câu hỏi / cấp độ Bloom
    ↓
[Giáo viên] → Tạo mẫu đề thi: chọn môn, lớp, cấu hình từng phần thi
              (topic, loại câu hỏi, số câu, tỉ lệ độ khó, cấp độ Bloom tùy chọn)
    ↓
[ExamGeneratorService] → Query câu hỏi từ PostgreSQL (Redis cache pool)
                         Stratified Sampling theo độ khó + Bloom filter
                         Fisher-Yates Shuffle → Partition thành N mã đề
    ↓
[Output] → Đề thi lưu DB → Preview trên React
         → Export PDF (QuestPDF) / Word (ClosedXML) → MinIO
    ↓
[Học sinh] → Làm bài → Nộp → Chấm điểm tự động (trắc nghiệm)
                             / thủ công (tự luận)
```

---

## 3. Domain Model

```
GradeLevel (Lớp học 1–12)
    └── has many → Subject (Môn học)
                       └── has many → Topic (Chủ đề / Chương)
                                          └── has many → Question (Câu hỏi)
                                                             ├── has → DifficultyLevel
                                                             ├── has → QuestionType
                                                             ├── has → CognitiveLevel  ← [MỚI] Bloom's Taxonomy
                                                             └── has many → QuestionAnswer (Đáp án)

CognitiveLevel (Cấp độ nhận thức — Bloom's Taxonomy)  ← [MỚI]
    6 cấp: Remember → Understand → Apply → Analyze → Evaluate → Create

ExamTemplate (Mẫu đề thi)
    └── belongs to → GradeLevel, Subject
    └── has many  → ExamTemplateSection (Phần thi)
                        └── config → Topic, QuestionType,
                                     DifficultyDistribution (%),
                                     CognitiveLevel (tùy chọn),  ← [MỚI]
                                     QuestionCount, ScorePerQuestion

Exam (Đề thi cụ thể — sinh từ template)
    └── has many → ExamQuestion (snapshot nội dung tại thời điểm tạo)
                        └── answers_snapshot (JSONB)

ExamSubmission (Bài nộp)
    └── belongs to → Exam, Student
    └── has many → SubmissionAnswer
```

---

## 4. Database — 15 Bảng (PostgreSQL 16, schema: public)

> Cập nhật v2: Thêm bảng `cognitive_levels` + FK vào `questions` và `exam_template_sections`

### Nhóm Config (6 bảng)
| Bảng | Mô tả | PK |
|------|-------|-----|
| `grade_levels` | Lớp học 1–12 | SERIAL |
| `subjects` | Môn học, FK → grade_levels | SERIAL |
| `topics` | Chủ đề/chương, hỗ trợ self-reference parent_id | SERIAL |
| `difficulty_levels` | Độ khó: easy/medium/hard/very_hard | SERIAL |
| `question_types` | Loại câu hỏi: multiple_choice/essay/... | SERIAL |
| `cognitive_levels` | **[MỚI]** Cấp độ nhận thức Bloom's Taxonomy (6 cấp) | SERIAL |

### Nhóm Users (2 bảng)
| Bảng | Mô tả | PK |
|------|-------|-----|
| `app_users` | Người dùng (UUID), roles[], claims[], refreshtoken | UUID |
| `teacher_subjects` | Giáo viên phụ trách môn | SERIAL |

### Nhóm Question Bank (2 bảng)
| Bảng | Mô tả | PK |
|------|-------|-----|
| `questions` | Câu hỏi, FK → topics/question_types/difficulty_levels/**cognitive_levels**/app_users | UUID |
| `question_answers` | Đáp án, FK → questions CASCADE | UUID |

### Nhóm Exam Template (2 bảng)
| Bảng | Mô tả | PK |
|------|-------|-----|
| `exam_templates` | Mẫu đề thi, shuffle_questions/shuffle_answers/prevent_duplicate | UUID |
| `exam_template_sections` | Phần thi, pct_easy/medium/hard/very_hard (%), **cognitive_level_id** | UUID |

### Nhóm Exam (2 bảng)
| Bảng | Mô tả | PK |
|------|-------|-----|
| `exams` | Đề thi cụ thể, parent_exam_id/variant_index/batch_id | UUID |
| `exam_questions` | Snapshot câu hỏi tại thời điểm tạo đề, answers_snapshot JSONB | UUID |

### Nhóm Results (2 bảng)
| Bảng | Mô tả | PK |
|------|-------|-----|
| `exam_submissions` | Bài nộp, FK → exams + app_users | UUID |
| `submission_answers` | Chi tiết câu trả lời, selected_answer_ids UUID[] | UUID |

### Index quan trọng
```sql
-- Covering partial index cho sinh đề thi (quan trọng nhất) — CẬP NHẬT v2
CREATE INDEX idx_q_pool ON questions(topic_id, difficulty_level_id, question_type_id)
    INCLUDE (id, cognitive_level_id)          -- thêm cognitive_level_id vào INCLUDE
    WHERE is_active = true AND is_verified = true;

-- [MỚI] Index lọc câu hỏi theo Bloom
CREATE INDEX idx_questions_cognitive ON questions(cognitive_level_id);

-- [MỚI] Pool index kết hợp Bloom
CREATE INDEX idx_q_pool_cognitive ON questions(topic_id, cognitive_level_id, difficulty_level_id)
    INCLUDE (id)
    WHERE is_active = true AND is_verified = true;
```

---

## 5. Bloom's Taxonomy — Cấp Độ Nhận Thức

> Dựa theo Anderson & Krathwohl (2001) — phiên bản cải tiến của Benjamin Bloom

| # | Code | Tên VN | Tên EN | Hệ số | Màu | Động từ tiêu biểu |
|---|------|--------|--------|-------|-----|-------------------|
| 1 | `remember` | Nhớ | Remember | Thấp nhất | `#4CAF50` | Liệt kê, xác định, nhận ra, gọi tên |
| 2 | `understand` | Hiểu | Understand | | `#2196F3` | Giải thích, mô tả, phân loại, tóm tắt |
| 3 | `apply` | Vận dụng | Apply | | `#FF9800` | Tính toán, giải, áp dụng, thực hiện |
| 4 | `analyze` | Phân tích | Analyze | | `#9C27B0` | Phân tích, phân biệt, kiểm tra, suy luận |
| 5 | `evaluate` | Đánh giá | Evaluate | | `#F44336` | Đánh giá, phê bình, lập luận, chứng minh |
| 6 | `create` | Sáng tạo | Create | Cao nhất | `#E91E63` | Thiết kế, xây dựng, lập kế hoạch, đề xuất |

**Ứng dụng trong hệ thống:**
- Mỗi câu hỏi (`questions.cognitive_level_id`) được gán một cấp độ Bloom (nullable — chưa phân loại).
- Mỗi phần thi (`exam_template_sections.cognitive_level_id`) có thể lọc câu hỏi theo cấp độ cụ thể (NULL = không lọc).
- Hỗ trợ filter theo Bloom trên API `/api/v1/questions?cognitiveLevel=apply`.
- Báo cáo thống kê phân bổ câu hỏi theo cấp độ nhận thức trong từng đề thi.

---

## 6. Project Structure (.NET Core)

```
ExamHub.sln
│
├── ExamHub.API/                            # Entry point — ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── ConfigController.cs            # Grade, Subject, Topic, Difficulty,
│   │   │                                  # QuestionType, CognitiveLevel  ← [MỚI]
│   │   ├── QuestionsController.cs
│   │   ├── ExamTemplatesController.cs
│   │   ├── ExamsController.cs
│   │   └── AuthController.cs
│   ├── Middleware/
│   │   ├── JwtMiddleware.cs
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── ExamHub.Core/                          # Toàn bộ logic nghiệp vụ & infrastructure
│   │
│   ├── Application/                       # Business Logic — CQRS / Services
│   │   ├── Features/
│   │   │   ├── Questions/
│   │   │   │   ├── Commands/              # CreateQuestion, UpdateQuestion, Delete
│   │   │   │   └── Queries/               # GetQuestions (filter + Bloom), GetById
│   │   │   ├── Exams/
│   │   │   │   ├── Commands/              # GenerateExam, BatchGenerateExam, PublishExam
│   │   │   │   └── Queries/               # GetExam, ExportExam
│   │   │   └── Config/
│   │   │       └── CognitiveLevels/       # [MỚI] GetCognitiveLevels CRUD
│   │   ├── Services/
│   │   │   ├── ExamGeneratorService.cs    # Thuật toán sinh đề (hỗ trợ Bloom filter)
│   │   │   ├── ExportService.cs           # PDF (QuestPDF) / Word (ClosedXML)
│   │   │   └── StorageService.cs          # MinIO upload/download
│   │   ├── DTOs/
│   │   └── Validators/                    # FluentValidation
│   │
│   ├── Domain/                            # Core Entities, Enums, Interfaces
│   │   ├── Entities/
│   │   │   ├── GradeLevel.cs
│   │   │   ├── Subject.cs
│   │   │   ├── Topic.cs
│   │   │   ├── DifficultyLevel.cs
│   │   │   ├── QuestionType.cs
│   │   │   ├── CognitiveLevel.cs          # [MỚI] Bloom's Taxonomy entity
│   │   │   ├── Question.cs                # [CẬP NHẬT] thêm CognitiveLevelId
│   │   │   ├── QuestionAnswer.cs
│   │   │   ├── ExamTemplate.cs
│   │   │   ├── ExamTemplateSection.cs     # [CẬP NHẬT] thêm CognitiveLevelId
│   │   │   ├── Exam.cs
│   │   │   ├── ExamQuestion.cs
│   │   │   ├── ExamSubmission.cs
│   │   │   └── SubmissionAnswer.cs
│   │   ├── Enums/
│   │   │   ├── ExamStatus.cs              # Draft / Published / Archived
│   │   │   ├── SubmissionStatus.cs        # InProgress / Submitted / Graded
│   │   │   └── BloomLevel.cs              # [MỚI] Remember..Create (mirror DB)
│   │   └── Interfaces/
│   │       ├── IQuestionRepository.cs
│   │       ├── IExamRepository.cs
│   │       └── IStorageService.cs
│   │
│   └── Infrastructure/                    # EF Core, MinIO, Redis
│       ├── Persistence/
│       │   ├── AppDbContext.cs            # [CẬP NHẬT] thêm DbSet<CognitiveLevel>
│       │   ├── Configurations/            # EF Core Fluent API configs
│       │   │   └── CognitiveLevelConfiguration.cs  # [MỚI]
│       │   ├── Repositories/
│       │   └── Migrations/
│       ├── Storage/
│       │   └── MinioStorageService.cs
│       └── Caching/
│           └── RedisCacheService.cs       # Cache key: qpool:{topicId}:{diffId}:{typeId}:{cogId}
│
├── frontend/                              # React Application
│   ├── src/
│   │   ├── pages/
│   │   │   ├── admin/                     # GradeLevel, Subject, Topic, User,
│   │   │   │                              # CognitiveLevel  ← [MỚI]
│   │   │   ├── teacher/                   # QuestionBank, ExamTemplate, Exam
│   │   │   └── student/                   # TakeExam, Result
│   │   ├── components/
│   │   │   ├── question/                  # QuestionCard, QuestionEditor, Answers,
│   │   │   │                              # BloomLevelBadge  ← [MỚI]
│   │   │   ├── exam/                      # ExamGeneratorForm, ExamPreview
│   │   │   └── shared/                    # DataTable, FilterPanel, FileUpload,
│   │   │                                  # BloomPyramid  ← [MỚI]
│   │   ├── api/
│   │   ├── store/
│   │   └── hooks/
│   └── package.json
│
└── docker-compose.yml
```

---

## 7. API Endpoints (RESTful)

### Auth
```
POST   /api/v1/auth/login                      # Đăng nhập → JWT + Refresh Token
POST   /api/v1/auth/refresh                    # Làm mới access token
POST   /api/v1/auth/logout
GET    /api/v1/auth/me
```

### Config
```
GET    /api/v1/grade-levels
POST   /api/v1/grade-levels
PUT    /api/v1/grade-levels/{id}

GET    /api/v1/subjects?gradeId={id}
POST   /api/v1/subjects
PUT    /api/v1/subjects/{id}

GET    /api/v1/topics?subjectId={id}
POST   /api/v1/topics
PUT    /api/v1/topics/{id}

GET    /api/v1/difficulty-levels
GET    /api/v1/question-types

# [MỚI] Bloom's Taxonomy
GET    /api/v1/cognitive-levels                # Lấy danh sách 6 cấp độ
POST   /api/v1/cognitive-levels                # Admin: thêm (hiếm dùng)
PUT    /api/v1/cognitive-levels/{id}           # Admin: sửa mô tả, màu
```

### Question Bank
```
GET    /api/v1/questions                       # filter: gradeId, subjectId, topicId,
                                               #         difficultyId, typeId, keyword,
                                               #         cognitiveLevel  ← [MỚI]
GET    /api/v1/questions/{id}
POST   /api/v1/questions                       # body có cognitive_level_id
PUT    /api/v1/questions/{id}
DELETE /api/v1/questions/{id}
POST   /api/v1/questions/bulk-import           # Upload Excel → parse → save
POST   /api/v1/questions/{id}/image            # Upload ảnh → MinIO → URL
PATCH  /api/v1/questions/{id}/verify           # Admin/Teacher: duyệt câu hỏi
```

### Exam Templates
```
GET    /api/v1/exam-templates
POST   /api/v1/exam-templates                  # body sections có cognitive_level_id
PUT    /api/v1/exam-templates/{id}
DELETE /api/v1/exam-templates/{id}
```

### Exams
```
POST   /api/v1/exams/generate                  # Sinh đề từ template config
POST   /api/v1/exams/batch-generate            # Sinh N mã đề cùng lúc (batch)
GET    /api/v1/exams/{id}
PATCH  /api/v1/exams/{id}/publish
GET    /api/v1/exams/{id}/export?format=pdf    # QuestPDF → MinIO → presigned URL
GET    /api/v1/exams/{id}/export?format=docx   # ClosedXML → MinIO → presigned URL
```

### Results
```
POST   /api/v1/submissions                     # Học sinh nộp bài
GET    /api/v1/submissions/{id}                # Kết quả chi tiết
GET    /api/v1/exams/{id}/submissions          # Danh sách nộp bài theo đề
GET    /api/v1/exams/{id}/analytics            # Thống kê theo Bloom, độ khó
```

---

## 8. Exam Generation Algorithm (C#)

```csharp
// ExamGeneratorService.cs — v2 (hỗ trợ Bloom filter)
public async Task<Exam> GenerateAsync(GenerateExamRequest request)
{
    var exam = new Exam { Title = request.Title, /* ... */ };
    var usedQuestionIds = new HashSet<Guid>();

    foreach (var section in request.Sections)
    {
        // 1. Tính số câu theo từng mức độ khó (Stratified Sampling)
        var easyCount   = (int)Math.Round(section.QuestionCount * section.PctEasy   / 100.0);
        var mediumCount = (int)Math.Round(section.QuestionCount * section.PctMedium / 100.0);
        var hardCount   = (int)Math.Round(section.QuestionCount * section.PctHard   / 100.0);
        var vhCount     = section.QuestionCount - easyCount - mediumCount - hardCount;

        // 2. Query câu hỏi từ DB (Redis cache pool)
        //    Cache key: qpool:{topicId}:{diffId}:{typeId}:{cogId}  ← [CẬP NHẬT]
        var pool = await _questionRepo.GetPoolAsync(new QuestionFilter
        {
            TopicId          = section.TopicId,
            QuestionTypeId   = section.QuestionTypeId,
            CognitiveLevelId = section.CognitiveLevelId,  // ← [MỚI] Bloom filter
            ExcludeIds       = usedQuestionIds
        });

        // 3. Stratified random theo từng mức độ khó
        var selected = SelectRandom(pool, DifficultyLevel.Easy,     easyCount)
            .Concat(SelectRandom(pool, DifficultyLevel.Medium,  mediumCount))
            .Concat(SelectRandom(pool, DifficultyLevel.Hard,    hardCount))
            .Concat(SelectRandom(pool, DifficultyLevel.VeryHard, vhCount))
            .ToList();

        // 4. Snapshot nội dung tại thời điểm tạo đề
        exam.Questions.AddRange(selected.Select((q, i) => new ExamQuestion
        {
            QuestionId      = q.Id,
            SortOrder       = i,
            Score           = section.ScorePerQuestion,
            ContentSnapshot = q.Content,
            AnswersSnapshot = JsonSerializer.Serialize(q.Answers)
        }));

        usedQuestionIds.UnionWith(selected.Select(q => q.Id));
    }

    // 5. Fisher-Yates shuffle nếu cần
    if (request.ShuffleQuestions)
        exam.Questions = exam.Questions.OrderBy(_ => Random.Shared.Next()).ToList();

    return await _examRepo.SaveAsync(exam);
}

private IEnumerable<Question> SelectRandom(
    IEnumerable<Question> pool, DifficultyLevel level, int count) =>
    pool.Where(q => q.DifficultyLevel == level)
        .OrderBy(_ => Random.Shared.Next())
        .Take(count);
```

### Batch Generation (N mã đề không trùng câu)
```csharp
// Option A — Partition-based (không trùng câu, O(n))
var batchId = Guid.NewGuid();
var shuffledPool = pool.OrderBy(_ => Random.Shared.Next()).ToList(); // Fisher-Yates
var partitions = shuffledPool.Chunk(questionsPerExam);               // .NET 6+
var exams = partitions.Select((part, idx) => BuildExam(part, idx, batchId));

// Option B — Clone + shuffle thứ tự (cùng câu, đảo thứ tự)
var variants = Enumerable.Range(1, N).Select(idx => {
    var clone = DeepClone(baseExam);
    clone.VariantIndex  = idx;
    clone.ParentExamId  = baseExam.Id;
    clone.BatchId       = batchId;
    clone.Questions     = clone.Questions.OrderBy(_ => Random.Shared.Next()).ToList();
    return clone;
});
```

---

## 9. C# Entity — CognitiveLevel (Mới)

```csharp
// Domain/Entities/CognitiveLevel.cs
public class CognitiveLevel : BaseEntity<int>
{
    public string  Code        { get; set; } = null!; // "remember", "understand", ...
    public string  Name        { get; set; } = null!; // "Nhớ", "Hiểu", ...
    public string  NameEn      { get; set; } = null!; // "Remember", "Understand", ...
    public short   LevelOrder  { get; set; }          // 1 → 6
    public string? Description { get; set; }
    public string? ColorCode   { get; set; }          // "#4CAF50"
    public bool    IsActive    { get; set; } = true;

    // Navigation
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<ExamTemplateSection> Sections { get; set; } = new List<ExamTemplateSection>();
}

// Cập nhật Question.cs — thêm FK
public int?           CognitiveLevelId { get; set; }           // nullable
public CognitiveLevel? CognitiveLevel  { get; set; }

// Cập nhật ExamTemplateSection.cs — thêm FK
public int?           CognitiveLevelId { get; set; }           // nullable = không lọc
public CognitiveLevel? CognitiveLevel  { get; set; }

// EF Core Configuration
public class CognitiveLevelConfiguration : IEntityTypeConfiguration<CognitiveLevel>
{
    public void Configure(EntityTypeBuilder<CognitiveLevel> builder)
    {
        builder.ToTable("cognitive_levels");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.Property(x => x.LevelOrder).IsRequired();
        builder.HasIndex(x => x.LevelOrder).IsUnique();
        builder.HasCheckConstraint("CK_level_order", "level_order BETWEEN 1 AND 6");
    }
}
```

---

## 10. Redis Cache Strategy (Cập nhật v2)

```
Cache key cũ:  qpool:{topicId}:{difficultyId}:{typeId}
Cache key mới: qpool:{topicId}:{difficultyId}:{typeId}:{cognitiveId|"all"}

TTL: 2 phút
Nội dung cache: Chỉ cache danh sách ID (~50 bytes/câu), KHÔNG cache full content
Invalidate: Khi câu hỏi thêm/sửa/xóa/duyệt → xóa cache liên quan

Ví dụ:
  qpool:5:1:1:all    → topic 5, easy, multiple_choice, không lọc Bloom
  qpool:5:1:1:3      → topic 5, easy, multiple_choice, Bloom = Apply (id=3)
```

---

## 11. MinIO Integration (C#)

```csharp
// Program.cs
builder.Services.AddMinio(client => client
    .WithEndpoint(builder.Configuration["MinIO:Endpoint"])
    .WithCredentials(
        builder.Configuration["MinIO:AccessKey"],
        builder.Configuration["MinIO:SecretKey"])
    .WithSSL(false));

// MinioStorageService.cs
public async Task<string> UploadAsync(IFormFile file, string folder)
{
    var objectName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    await _minio.PutObjectAsync(new PutObjectArgs()
        .WithBucket("examhub")
        .WithObject(objectName)
        .WithStreamData(file.OpenReadStream())
        .WithObjectSize(file.Length)
        .WithContentType(file.ContentType));

    return await _minio.PresignedGetObjectAsync(
        new PresignedGetObjectArgs()
            .WithBucket("examhub")
            .WithObject(objectName)
            .WithExpiry(3600));  // Presigned URL hết hạn sau 1 giờ
}
```

**MinIO Bucket Structure:**
```
examhub/
├── questions/      # Ảnh/file đính kèm câu hỏi
├── exports/        # File PDF/Word đề thi đã xuất
└── imports/        # File Excel import câu hỏi (tạm)
```

---

## 12. Authentication & Authorization

```csharp
public static class Roles
{
    public const string Admin   = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
}

// Phân quyền endpoint mẫu
[Authorize(Roles = "Admin")]
[HttpPost("grade-levels")]
public async Task<IActionResult> CreateGradeLevel(...)

[Authorize(Roles = "Admin")]
[HttpGet("cognitive-levels")]           // ← [MỚI] GET: Teacher cũng đọc được
[HttpPut("cognitive-levels/{id}")]      // ← [MỚI] PUT: chỉ Admin

[Authorize(Roles = "Admin,Teacher")]
[HttpPost("questions")]
public async Task<IActionResult> CreateQuestion(...)

[Authorize]
[HttpPost("submissions")]
public async Task<IActionResult> SubmitExam(...)
```

**JWT Config (appsettings.json):**
```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret",
    "Issuer": "ExamHub",
    "Audience": "ExamHubUsers",
    "AccessTokenExpireMinutes": 15,
    "RefreshTokenExpireDays": 7
  }
}
```

---

## 13. Non-Functional Requirements

- **Performance**: Sinh đề < 2 giây với ngân hàng 10,000+ câu (Redis cache pool + covering index)
- **Scalability**: Hỗ trợ 1,000 concurrent users; scale horizontal bằng Docker
- **Security**: JWT Bearer + Refresh Token, RBAC, FluentValidation cho tất cả input
- **Data Integrity**: Snapshot câu hỏi khi tạo đề (đề không đổi khi câu gốc bị sửa)
- **File Handling**: MinIO self-hosted, presigned URL cho upload/download an toàn
- **Availability**: 99.9% uptime với Docker + Nginx
- **Education Quality**: Phân loại câu hỏi theo Bloom's Taxonomy đảm bảo đề thi đúng chuẩn nhận thức

---

## 14. Tech Stack

| Layer | Technology | Ghi chú |
|-------|-----------|---------|
| **Frontend** | React 18 + Vite + TypeScript | SPA |
| **UI** | TailwindCSS + shadcn/ui | |
| **State** | Zustand | Lightweight |
| **HTTP** | Axios + TanStack Query | Cache + refetch |
| **Rich Text** | TipTap | Soạn câu hỏi có định dạng, công thức |
| **Backend** | **ASP.NET Core Web API (.NET 8)** | C# |
| **ORM** | Entity Framework Core 8 | Code-first migrations |
| **CQRS** | MediatR | Clean Architecture |
| **Validation** | FluentValidation | |
| **Auth** | ASP.NET Core Identity + JWT Bearer | |
| **Database** | **PostgreSQL 16** | 15 bảng (v2) |
| **Cache** | **Redis 7** (StackExchange.Redis) | Cache key cập nhật hỗ trợ Bloom |
| **File Storage** | **MinIO** (Minio.AspNetCore SDK) | S3-compatible, self-hosted |
| **Export PDF** | **QuestPDF** | Không cần Headless Chrome |
| **Export Word** | **ClosedXML** | |
| **Import Excel** | **EPPlus** | Import câu hỏi hàng loạt |
| **Container** | Docker + Docker Compose | 6 services |
| **Reverse Proxy** | **Nginx** | |
| **CI/CD** | GitHub Actions | |

---

## 15. Docker Compose

```yaml
version: '3.9'

services:
  api:
    build:
      context: ./ExamHub.API
      dockerfile: Dockerfile
    ports: ["5000:8080"]
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=examhub;Username=postgres;Password=secret"
      Redis__ConnectionString: "redis:6379"
      MinIO__Endpoint: "minio:9000"
      MinIO__AccessKey: "minioadmin"
      MinIO__SecretKey: "minioadmin"
      Jwt__SecretKey: "your-super-secret-256-bit-key"
    depends_on: [postgres, redis, minio]

  frontend:
    build: ./frontend
    ports: ["3000:80"]
    depends_on: [api]

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: examhub
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: secret
    volumes: ["pgdata:/var/lib/postgresql/data"]
    ports: ["5432:5432"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    ports:
      - "9000:9000"     # S3 API Endpoint
      - "9001:9001"     # MinIO Web Console
    environment:
      MINIO_ROOT_USER: minioadmin
      MINIO_ROOT_PASSWORD: minioadmin
    volumes: ["miniodata:/data"]

  nginx:
    image: nginx:alpine
    ports: ["80:80"]
    volumes: ["./nginx.conf:/etc/nginx/nginx.conf:ro"]
    depends_on: [api, frontend]

volumes:
  pgdata:
  miniodata:
```

---
