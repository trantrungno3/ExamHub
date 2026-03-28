-- ============================================================
-- HỆ THỐNG TẠO SINH ĐỀ THI - DATABASE SCHEMA
-- PostgreSQL
-- ============================================================

-- ============================================================
-- PHẦN 1: CẤU HÌNH HỆ THỐNG (Configuration)
-- ============================================================

-- Lớp học (1 - 12)
CREATE TABLE grade_levels (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(50) NOT NULL,           -- "Lớp 10"
    grade_number SMALLINT NOT NULL UNIQUE,       -- 1 → 12
    description TEXT,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Môn học
CREATE TABLE subjects (
    id              SERIAL PRIMARY KEY,
    grade_level_id  INT NOT NULL REFERENCES grade_levels(id) ON DELETE CASCADE,
    name            VARCHAR(100) NOT NULL,       -- "Toán", "Ngữ văn", "Hóa học"
    code            VARCHAR(20) NOT NULL,        -- "MATH", "LIT", "CHEM"
    description     TEXT,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE (grade_level_id, code)
);

-- Chủ đề / Chương / Unit
CREATE TABLE topics (
    id          SERIAL PRIMARY KEY,
    subject_id  INT NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
    parent_id   INT REFERENCES topics(id),       -- Hỗ trợ chủ đề lồng nhau
    name        VARCHAR(200) NOT NULL,           -- "Chương 1: Nguyên tử"
    code        VARCHAR(50),
    sort_order  INT NOT NULL DEFAULT 0,
    description TEXT,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Mức độ khó
CREATE TABLE difficulty_levels (
    id          SERIAL PRIMARY KEY,
    code        VARCHAR(20) NOT NULL UNIQUE,     -- 'easy', 'medium', 'hard', 'very_hard'
    name        VARCHAR(50) NOT NULL,            -- "Dễ", "Trung bình", "Khó", "Rất khó"
    score_weight NUMERIC(3,2) NOT NULL DEFAULT 1.0, -- Hệ số điểm
    sort_order  SMALLINT NOT NULL DEFAULT 0,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE
);

-- Loại câu hỏi
CREATE TABLE question_types (
    id          SERIAL PRIMARY KEY,
    code        VARCHAR(30) NOT NULL UNIQUE,     -- 'multiple_choice', 'true_false', 'fill_blank', 'essay', 'matching'
    name        VARCHAR(100) NOT NULL,           -- "Trắc nghiệm 4 đáp án"
    description TEXT,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE
);

-- ============================================================
-- PHẦN 2: QUẢN LÝ NGƯỜI DÙNG (Users)
-- ============================================================

CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           VARCHAR(255) NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    full_name       VARCHAR(200) NOT NULL,
    role            VARCHAR(20) NOT NULL CHECK (role IN ('admin', 'teacher', 'student')),
    avatar_url      TEXT,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    last_login_at   TIMESTAMP,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Giáo viên phụ trách môn/lớp
CREATE TABLE teacher_subjects (
    id          SERIAL PRIMARY KEY,
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    subject_id  INT NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
    UNIQUE (user_id, subject_id)
);

-- ============================================================
-- PHẦN 3: NGÂN HÀNG CÂU HỎI (Question Bank)
-- ============================================================

CREATE TABLE questions (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    topic_id            INT NOT NULL REFERENCES topics(id),
    question_type_id    INT NOT NULL REFERENCES question_types(id),
    difficulty_level_id INT NOT NULL REFERENCES difficulty_levels(id),
    created_by          UUID NOT NULL REFERENCES users(id),

    -- Nội dung câu hỏi
    content             TEXT NOT NULL,           -- Nội dung câu hỏi (hỗ trợ HTML/Markdown)
    content_plain       TEXT,                    -- Nội dung thuần text (để tìm kiếm)
    explanation         TEXT,                    -- Giải thích đáp án
    image_url           TEXT,                    -- Hình ảnh đính kèm
    audio_url           TEXT,                    -- File âm thanh (nếu có)

    -- Metadata
    source              VARCHAR(200),            -- Nguồn câu hỏi (SGK, thi thử, ...)
    tags                TEXT[],                  -- Tags tìm kiếm nhanh
    usage_count         INT NOT NULL DEFAULT 0,  -- Số lần dùng trong đề
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    is_verified         BOOLEAN NOT NULL DEFAULT FALSE, -- Đã duyệt chưa
    verified_by         UUID REFERENCES users(id),
    verified_at         TIMESTAMP,

    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Đáp án câu hỏi (cho trắc nghiệm, đúng/sai, ghép cặp)
CREATE TABLE question_answers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    question_id     UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
    content         TEXT NOT NULL,              -- Nội dung đáp án
    content_plain   TEXT,
    is_correct      BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order      SMALLINT NOT NULL DEFAULT 0,
    explanation     TEXT                        -- Giải thích tại sao đúng/sai
);

-- ============================================================
-- PHẦN 4: MẪU ĐỀ THI (Exam Templates)
-- ============================================================

CREATE TABLE exam_templates (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    grade_level_id  INT NOT NULL REFERENCES grade_levels(id),
    subject_id      INT NOT NULL REFERENCES subjects(id),
    created_by      UUID NOT NULL REFERENCES users(id),

    title           VARCHAR(300) NOT NULL,
    description     TEXT,
    duration_minutes INT NOT NULL DEFAULT 45,
    total_questions INT,
    total_score     NUMERIC(5,2) NOT NULL DEFAULT 10.0,

    -- Tùy chọn sinh đề
    shuffle_questions   BOOLEAN NOT NULL DEFAULT TRUE,
    shuffle_answers     BOOLEAN NOT NULL DEFAULT TRUE,
    prevent_duplicate   BOOLEAN NOT NULL DEFAULT TRUE,  -- Không trùng đề khác

    instructions    TEXT,                       -- Hướng dẫn làm bài
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Cấu hình từng phần của đề thi (section/part)
CREATE TABLE exam_template_sections (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    exam_template_id    UUID NOT NULL REFERENCES exam_templates(id) ON DELETE CASCADE,
    topic_id            INT REFERENCES topics(id),  -- NULL = lấy từ toàn bộ môn
    question_type_id    INT REFERENCES question_types(id), -- NULL = tất cả loại
    section_name        VARCHAR(200),               -- "Phần I: Trắc nghiệm"
    question_count      INT NOT NULL,               -- Số câu trong phần này
    score_per_question  NUMERIC(4,2),               -- Điểm mỗi câu (NULL = tính đều)
    sort_order          SMALLINT NOT NULL DEFAULT 0,

    -- Phân bổ độ khó (%)
    pct_easy            SMALLINT NOT NULL DEFAULT 0 CHECK (pct_easy BETWEEN 0 AND 100),
    pct_medium          SMALLINT NOT NULL DEFAULT 0 CHECK (pct_medium BETWEEN 0 AND 100),
    pct_hard            SMALLINT NOT NULL DEFAULT 0 CHECK (pct_hard BETWEEN 0 AND 100),
    pct_very_hard       SMALLINT NOT NULL DEFAULT 0 CHECK (pct_very_hard BETWEEN 0 AND 100),

    created_at          TIMESTAMP NOT NULL DEFAULT NOW()

    -- Constraint: tổng % = 100
    -- Thực hiện ở application layer hoặc trigger
);

-- ============================================================
-- PHẦN 5: ĐỀ THI (Generated Exams)
-- ============================================================

CREATE TABLE exams (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    exam_template_id    UUID REFERENCES exam_templates(id),
    grade_level_id      INT NOT NULL REFERENCES grade_levels(id),
    subject_id          INT NOT NULL REFERENCES subjects(id),
    created_by          UUID NOT NULL REFERENCES users(id),

    title               VARCHAR(300) NOT NULL,
    exam_code           VARCHAR(50) UNIQUE,         -- Mã đề: "DE_001"
    duration_minutes    INT NOT NULL DEFAULT 45,
    total_score         NUMERIC(5,2) NOT NULL DEFAULT 10.0,
    instructions        TEXT,
    status              VARCHAR(20) NOT NULL DEFAULT 'draft'
                            CHECK (status IN ('draft', 'published', 'archived')),

    -- Thông tin sử dụng
    school_year         VARCHAR(20),                -- "2024-2025"
    semester            SMALLINT CHECK (semester IN (1, 2)),
    exam_date           DATE,
    class_name          VARCHAR(100),

    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Câu hỏi trong đề thi (snapshot tại thời điểm tạo đề)
CREATE TABLE exam_questions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    exam_id         UUID NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    question_id     UUID NOT NULL REFERENCES questions(id),
    section_name    VARCHAR(200),
    sort_order      INT NOT NULL,
    score           NUMERIC(4,2),

    -- Snapshot nội dung (để đề không thay đổi khi câu hỏi gốc bị sửa)
    content_snapshot        TEXT NOT NULL,
    answers_snapshot        JSONB,              -- [{content, is_correct, sort_order}]

    UNIQUE (exam_id, question_id)
);

-- ============================================================
-- PHẦN 6: KẾT QUẢ THI (Exam Results)
-- ============================================================

CREATE TABLE exam_submissions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    exam_id         UUID NOT NULL REFERENCES exams(id),
    student_id      UUID NOT NULL REFERENCES users(id),

    started_at      TIMESTAMP NOT NULL DEFAULT NOW(),
    submitted_at    TIMESTAMP,
    duration_seconds INT,                       -- Thời gian làm bài thực tế

    total_score     NUMERIC(5,2),
    is_passed       BOOLEAN,
    status          VARCHAR(20) NOT NULL DEFAULT 'in_progress'
                        CHECK (status IN ('in_progress', 'submitted', 'graded')),

    created_at      TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Chi tiết câu trả lời từng câu
CREATE TABLE submission_answers (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    submission_id       UUID NOT NULL REFERENCES exam_submissions(id) ON DELETE CASCADE,
    exam_question_id    UUID NOT NULL REFERENCES exam_questions(id),

    selected_answer_ids UUID[],                 -- Với trắc nghiệm
    essay_content       TEXT,                   -- Với tự luận
    is_correct          BOOLEAN,
    score_earned        NUMERIC(4,2) NOT NULL DEFAULT 0,
    feedback            TEXT,                   -- Nhận xét của giáo viên
    graded_by           UUID REFERENCES users(id),

    UNIQUE (submission_id, exam_question_id)
);

-- ============================================================
-- PHẦN 7: INDEXES
-- ============================================================

-- Tìm kiếm câu hỏi theo môn, lớp, chủ đề, độ khó
CREATE INDEX idx_questions_topic ON questions(topic_id);
CREATE INDEX idx_questions_difficulty ON questions(difficulty_level_id);
CREATE INDEX idx_questions_type ON questions(question_type_id);
CREATE INDEX idx_questions_active ON questions(is_active, is_verified);
CREATE INDEX idx_questions_tags ON questions USING GIN(tags);
CREATE INDEX idx_questions_fulltext ON questions USING GIN(to_tsvector('simple', content_plain));

-- Đề thi
CREATE INDEX idx_exams_template ON exams(exam_template_id);
CREATE INDEX idx_exams_subject ON exams(subject_id);
CREATE INDEX idx_exams_grade ON exams(grade_level_id);
CREATE INDEX idx_exams_status ON exams(status);

-- Kết quả
CREATE INDEX idx_submissions_exam ON exam_submissions(exam_id);
CREATE INDEX idx_submissions_student ON exam_submissions(student_id);

-- ============================================================
-- PHẦN 8: DỮ LIỆU MẪU (Seed Data)
-- ============================================================

-- Độ khó
INSERT INTO difficulty_levels (code, name, score_weight, sort_order) VALUES
    ('easy',      'Dễ',        1.0, 1),
    ('medium',    'Trung bình', 1.5, 2),
    ('hard',      'Khó',        2.0, 3),
    ('very_hard', 'Rất khó',    2.5, 4);

-- Loại câu hỏi
INSERT INTO question_types (code, name) VALUES
    ('multiple_choice', 'Trắc nghiệm 1 đáp án'),
    ('multiple_select', 'Trắc nghiệm nhiều đáp án'),
    ('true_false',      'Đúng/Sai'),
    ('fill_blank',      'Điền vào chỗ trống'),
    ('essay',           'Tự luận'),
    ('matching',        'Nối cột');

-- Lớp học
INSERT INTO grade_levels (name, grade_number) VALUES
    ('Lớp 1', 1), ('Lớp 2', 2), ('Lớp 3', 3), ('Lớp 4', 4),
    ('Lớp 5', 5), ('Lớp 6', 6), ('Lớp 7', 7), ('Lớp 8', 8),
    ('Lớp 9', 9), ('Lớp 10', 10), ('Lớp 11', 11), ('Lớp 12', 12);
