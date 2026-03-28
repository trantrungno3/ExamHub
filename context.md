# Hệ Thống Tạo Sinh Đề Thi (Exam Generation System)

## 1. Tổng Quan Hệ Thống

Hệ thống tạo sinh đề thi tự động cho phép giáo viên/admin cấu hình và sinh đề thi theo trình độ lớp học (1–12), môn học, chủ đề và độ khó. Hệ thống quản lý ngân hàng câu hỏi và cho phép sinh đề thi đa dạng theo yêu cầu.

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
| **Config Module** | Quản lý lớp học, môn học, chủ đề, độ khó | Controller + Service + EF Core |
| **Question Bank Module** | CRUD câu hỏi, lọc, tìm kiếm full-text | Repository Pattern + Specification |
| **Exam Generator Module** | Thuật toán sinh đề theo điều kiện | ExamGeneratorService + Strategy Pattern |
| **Auth / User Module** | Đăng nhập, phân quyền theo role | ASP.NET Core Identity + JWT Bearer |
| **Result & Analytics** | Lưu kết quả, thống kê, báo cáo | Background Service + CQRS/MediatR |
| **Export Module** | Xuất PDF, Word, Excel | QuestPDF + ClosedXML |
| **File Storage** | Upload/download ảnh, file đề thi | MinIO SDK (S3-compatible) |

### 2.3 Luồng Hoạt Động Chính

```
[Admin] → Cấu hình lớp học, môn học, chủ đề, độ khó
    ↓
[Giáo viên] → Nhập câu hỏi vào ngân hàng
              (upload ảnh → MinIO), gán lớp/môn/chủ đề/độ khó
    ↓
[Giáo viên] → Tạo đề thi: chọn môn, lớp, chủ đề, số câu, tỉ lệ độ khó
    ↓
[ExamGeneratorService] → Query câu hỏi từ PostgreSQL
                         Random theo điều kiện + phân bổ độ khó
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
                                                             └── has many → QuestionAnswer (Đáp án)

ExamTemplate (Mẫu đề thi)
    └── belongs to → GradeLevel, Subject
    └── has many  → ExamTemplateSection (Phần thi)
                        └── config → Topic, DifficultyDistribution,
                                     QuestionCount, ScorePerQuestion

Exam (Đề thi cụ thể — sinh từ template)
    └── has many → ExamQuestion (snapshot nội dung tại thời điểm tạo)
                        └── answers_snapshot (JSON)

ExamSubmission (Bài nộp)
    └── belongs to → Exam, Student
    └── has many → SubmissionAnswer
```

---

## 4. Project Structure (.NET Core)

```
ExamGen.sln
│
├── src/
│   ├── ExamGen.API/                        # Entry point — ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   ├── ConfigController.cs         # Grade, Subject, Topic, Difficulty
│   │   │   ├── QuestionsController.cs
│   │   │   ├── ExamTemplatesController.cs
│   │   │   ├── ExamsController.cs
│   │   │   └── AuthController.cs
│   │   ├── Middleware/
│   │   │   ├── JwtMiddleware.cs
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   ├── ExamGen.Application/                # Business Logic — CQRS / Services
│   │   ├── Features/
│   │   │   ├── Questions/
│   │   │   │   ├── Commands/               # CreateQuestion, UpdateQuestion, Delete
│   │   │   │   └── Queries/                # GetQuestions, GetQuestionById
│   │   │   ├── Exams/
│   │   │   │   ├── Commands/               # GenerateExam, PublishExam
│   │   │   │   └── Queries/                # GetExam, ExportExam
│   │   │   └── Config/
│   │   ├── Services/
│   │   │   ├── ExamGeneratorService.cs     # Thuật toán sinh đề thi
│   │   │   ├── ExportService.cs            # PDF (QuestPDF) / Word (ClosedXML)
│   │   │   └── StorageService.cs           # MinIO upload/download
│   │   ├── DTOs/
│   │   └── Validators/                     # FluentValidation
│   │
│   ├── ExamGen.Domain/                     # Core Entities, Enums, Interfaces
│   │   ├── Entities/
│   │   │   ├── GradeLevel.cs
│   │   │   ├── Subject.cs
│   │   │   ├── Topic.cs
│   │   │   ├── Question.cs
│   │   │   ├── QuestionAnswer.cs
│   │   │   ├── ExamTemplate.cs
│   │   │   ├── ExamTemplateSection.cs
│   │   │   ├── Exam.cs
│   │   │   ├── ExamQuestion.cs
│   │   │   ├── ExamSubmission.cs
│   │   │   └── SubmissionAnswer.cs
│   │   ├── Enums/
│   │   │   ├── DifficultyLevel.cs          # Easy, Medium, Hard, VeryHard
│   │   │   ├── QuestionType.cs             # MultipleChoice, Essay, TrueFalse...
│   │   │   └── UserRole.cs                 # Admin, Teacher, Student
│   │   └── Interfaces/
│   │       ├── IQuestionRepository.cs
│   │       ├── IExamRepository.cs
│   │       └── IStorageService.cs
│   │
│   └── ExamGen.Infrastructure/             # EF Core, MinIO, Redis
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/             # EF Core Fluent API configs
│       │   ├── Repositories/
│       │   └── Migrations/
│       ├── Storage/
│       │   └── MinioStorageService.cs      # Minio.AspNetCore SDK
│       └── Caching/
│           └── RedisCacheService.cs        # StackExchange.Redis
│
├── frontend/                               # React Application
│   ├── src/
│   │   ├── pages/
│   │   │   ├── admin/                      # GradeLevel, Subject, Topic, User
│   │   │   ├── teacher/                    # QuestionBank, ExamTemplate, Exam
│   │   │   └── student/                    # TakeExam, Result
│   │   ├── components/
│   │   │   ├── question/                   # QuestionCard, QuestionEditor, Answers
│   │   │   ├── exam/                       # ExamGeneratorForm, ExamPreview
│   │   │   └── shared/                     # DataTable, FilterPanel, FileUpload
│   │   ├── api/                            # Axios API clients (per module)
│   │   ├── store/                          # Zustand stores
│   │   └── hooks/
│   └── package.json
│
└── docker-compose.yml
```

---

## 5. API Endpoints (RESTful)

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
GET    /api/v1/topics?subjectId={id}
POST   /api/v1/topics
GET    /api/v1/difficulty-levels
GET    /api/v1/question-types
```

### Question Bank
```
GET    /api/v1/questions                       # filter: gradeId, subjectId, topicId, difficultyId, type, keyword
GET    /api/v1/questions/{id}
POST   /api/v1/questions
PUT    /api/v1/questions/{id}
DELETE /api/v1/questions/{id}
POST   /api/v1/questions/bulk-import           # Upload Excel → parse → save
POST   /api/v1/questions/{id}/image            # Upload ảnh → MinIO → trả về URL
```

### Exam Templates
```
GET    /api/v1/exam-templates
POST   /api/v1/exam-templates
PUT    /api/v1/exam-templates/{id}
DELETE /api/v1/exam-templates/{id}
```

### Exams
```
POST   /api/v1/exams/generate                  # Sinh đề từ template config
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
```

---

## 6. Exam Generation Algorithm (C#)

```csharp
// ExamGeneratorService.cs
public async Task<Exam> GenerateAsync(GenerateExamRequest request)
{
    var exam = new Exam { Title = request.Title, ... };
    var usedQuestionIds = new HashSet<Guid>();

    foreach (var section in request.Sections)
    {
        // 1. Tính số câu theo từng mức độ
        var easyCount   = (int)Math.Round(section.QuestionCount * section.PctEasy / 100.0);
        var mediumCount = (int)Math.Round(section.QuestionCount * section.PctMedium / 100.0);
        var hardCount   = (int)Math.Round(section.QuestionCount * section.PctHard / 100.0);
        var vhCount     = section.QuestionCount - easyCount - mediumCount - hardCount;

        // 2. Query câu hỏi từ DB (Redis cache)
        var pool = await _questionRepo.GetPoolAsync(new QuestionFilter
        {
            TopicId        = section.TopicId,
            QuestionTypeId = section.QuestionTypeId,
            ExcludeIds     = usedQuestionIds
        });

        // 3. Random theo từng mức độ
        var selected = SelectRandom(pool, DifficultyLevel.Easy, easyCount)
            .Concat(SelectRandom(pool, DifficultyLevel.Medium, mediumCount))
            .Concat(SelectRandom(pool, DifficultyLevel.Hard, hardCount))
            .Concat(SelectRandom(pool, DifficultyLevel.VeryHard, vhCount))
            .ToList();

        // 4. Snapshot nội dung
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

---

## 7. MinIO Integration (C#)

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
        .WithBucket("exam-gen")
        .WithObject(objectName)
        .WithStreamData(file.OpenReadStream())
        .WithObjectSize(file.Length)
        .WithContentType(file.ContentType));

    // Trả về presigned URL (có hạn 1 giờ)
    return await _minio.PresignedGetObjectAsync(
        new PresignedGetObjectArgs()
            .WithBucket("exam-gen")
            .WithObject(objectName)
            .WithExpiry(3600));
}
```

**MinIO Bucket Structure:**
```
exam-gen/
├── questions/      # Ảnh/file đính kèm câu hỏi
├── exports/        # File PDF/Word đề thi đã xuất
└── imports/        # File Excel import câu hỏi (tạm)
```

---

## 8. Authentication & Authorization

```csharp
// Roles
public static class Roles
{
    public const string Admin   = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
}

// Ví dụ bảo vệ endpoint
[Authorize(Roles = "Admin")]
[HttpPost("grade-levels")]
public async Task<IActionResult> CreateGradeLevel(...)

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
    "Issuer": "ExamGen",
    "Audience": "ExamGenUsers",
    "AccessTokenExpireMinutes": 15,
    "RefreshTokenExpireDays": 7
  }
}
```

---

## 9. Non-Functional Requirements

- **Performance**: Sinh đề < 2 giây với ngân hàng 10,000+ câu (Redis cache pool câu hỏi theo filter)
- **Scalability**: Hỗ trợ 1,000 concurrent users; scale horizontal bằng Docker
- **Security**: JWT Bearer + Refresh Token, RBAC, FluentValidation cho tất cả input
- **Data Integrity**: Snapshot câu hỏi khi tạo đề (đề không đổi khi câu gốc bị sửa)
- **File Handling**: MinIO self-hosted, presigned URL cho upload/download an toàn
- **Availability**: 99.9% uptime với Docker + Nginx

---

## 10. Tech Stack

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
| **Database** | **PostgreSQL 16** | |
| **Cache** | **Redis 7** (StackExchange.Redis) | |
| **File Storage** | **MinIO** (Minio.AspNetCore SDK) | S3-compatible, self-hosted |
| **Export PDF** | **QuestPDF** | Không cần Headless Chrome |
| **Export Word** | **ClosedXML** | |
| **Import Excel** | **EPPlus** | Import câu hỏi hàng loạt |
| **Container** | Docker + Docker Compose | |
| **Reverse Proxy** | **Nginx** | |
| **CI/CD** | GitHub Actions | |

---

## 11. Docker Compose

```yaml
version: '3.9'

services:
  api:
    build:
      context: ./src/ExamGen.API
      dockerfile: Dockerfile
    ports: ["5000:8080"]
    environment:
      ConnectionStrings__Default: "Host=postgres;Database=examgen;Username=postgres;Password=secret"
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
      POSTGRES_DB: examgen
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
