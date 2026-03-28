-- ============================================================
-- HỆ THỐNG TẠO SINH ĐỀ THI - DATABASE SCHEMA
-- PostgreSQL
-- ============================================================

-- ============================================================
-- PHẦN 1: CẤU HÌNH HỆ THỐNG (Configuration)
-- ============================================================

-- Lớp học (1 - 12)
CREATE TABLE public.grade_levels (
                                     id           SERIAL PRIMARY KEY,
                                     name         VARCHAR(50)  NOT NULL,           -- "Lớp 10"
                                     grade_number SMALLINT     NOT NULL UNIQUE,     -- 1 → 12
                                     description  TEXT,
                                     is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
                                     created_at   TIMESTAMP    NOT NULL DEFAULT NOW(),
                                     updated_at   TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- Môn học
CREATE TABLE public.subjects (
                                 id             SERIAL PRIMARY KEY,
                                 grade_level_id INT         NOT NULL REFERENCES grade_levels(id) ON DELETE CASCADE,
                                 name           VARCHAR(100) NOT NULL,          -- "Toán", "Ngữ văn", "Hóa học"
                                 code           VARCHAR(20)  NOT NULL,          -- "MATH", "LIT", "CHEM"
                                 description    TEXT,
                                 is_active      BOOLEAN      NOT NULL DEFAULT TRUE,
                                 created_at     TIMESTAMP    NOT NULL DEFAULT NOW(),
                                 updated_at     TIMESTAMP    NOT NULL DEFAULT NOW(),
                                 UNIQUE (grade_level_id, code)
);

-- Chủ đề / Chương / Unit
CREATE TABLE public.topics (
                               id          SERIAL PRIMARY KEY,
                               subject_id  INT          NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
                               parent_id   INT          REFERENCES topics(id),  -- Hỗ trợ chủ đề lồng nhau
                               name        VARCHAR(200) NOT NULL,               -- "Chương 1: Nguyên tử"
                               code        VARCHAR(50),
                               sort_order  INT          NOT NULL DEFAULT 0,
                               description TEXT,
                               is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
                               created_at  TIMESTAMP    NOT NULL DEFAULT NOW(),
                               updated_at  TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- Mức độ khó
CREATE TABLE public.difficulty_levels (
                                          id           SERIAL PRIMARY KEY,
                                          code         VARCHAR(20)    NOT NULL UNIQUE,   -- 'easy', 'medium', 'hard', 'very_hard'
                                          name         VARCHAR(50)    NOT NULL,          -- "Dễ", "Trung bình", "Khó", "Rất khó"
                                          score_weight NUMERIC(3,2)   NOT NULL DEFAULT 1.0,
                                          sort_order   SMALLINT       NOT NULL DEFAULT 0,
                                          is_active    BOOLEAN        NOT NULL DEFAULT TRUE
);

-- Loại câu hỏi
CREATE TABLE public.question_types (
                                       id          SERIAL PRIMARY KEY,
                                       code        VARCHAR(30)  NOT NULL UNIQUE,      -- 'multiple_choice', 'true_false', ...
                                       name        VARCHAR(100) NOT NULL,             -- "Trắc nghiệm 4 đáp án"
                                       description TEXT,
                                       is_active   BOOLEAN      NOT NULL DEFAULT TRUE
);

-- ============================================================
-- PHẦN 2: QUẢN LÝ NGƯỜI DÙNG (Users)
-- ============================================================

CREATE TABLE public.app_users (
                                  id                  UUID                     NOT NULL PRIMARY KEY,
                                  username            VARCHAR(50)              NOT NULL,
                                  avartar             TEXT,
                                  normalizedusername  VARCHAR(50)              NOT NULL,
                                  displayname         VARCHAR(150)             NOT NULL,
                                  description         VARCHAR(500),
                                  phonenumber         VARCHAR(20),
                                  sex                 BOOLEAN,
                                  refreshtoken        VARCHAR(500),
                                  email               JSON,
                                  accessfailedcount   SMALLINT,
                                  deleted             TIMESTAMP WITH TIME ZONE,
                                  lockoutenabled      BOOLEAN,
                                  lockoutenddateutc   TIMESTAMP WITH TIME ZONE,
                                  normalizedemail     VARCHAR(100),
                                  passwordhash        VARCHAR(100),
                                  roles               VARCHAR(50)[],
                                  providerkey         VARCHAR(50),
                                  loginprovider       VARCHAR(50),
                                  claims              JSON[],
                                  created             TIMESTAMP WITH TIME ZONE NOT NULL,
                                  createby            VARCHAR(150),
                                  modified            TIMESTAMP WITH TIME ZONE,
                                  modifyby            VARCHAR(150)
);

-- Giáo viên phụ trách môn/lớp
CREATE TABLE public.teacher_subjects (
                                         id         SERIAL PRIMARY KEY,
                                         user_id    UUID NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
                                         subject_id INT  NOT NULL REFERENCES subjects(id) ON DELETE CASCADE,
                                         UNIQUE (user_id, subject_id)
);

-- ============================================================
-- PHẦN 3: NGÂN HÀNG CÂU HỎI (Question Bank)
-- ============================================================

CREATE TABLE public.questions (
                                  id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                  topic_id            INT          NOT NULL REFERENCES topics(id),
                                  question_type_id    INT          NOT NULL REFERENCES question_types(id),
                                  difficulty_level_id INT          NOT NULL REFERENCES difficulty_levels(id),
                                  created_by          UUID         NOT NULL REFERENCES app_users(id),

    -- Nội dung câu hỏi
                                  content             TEXT         NOT NULL,    -- HTML/Markdown
                                  content_plain       TEXT,                     -- Thuần text để tìm kiếm
                                  explanation         TEXT,
                                  image_url           TEXT,
                                  audio_url           TEXT,

    -- Metadata
                                  source              VARCHAR(200),
                                  tags                TEXT[],
                                  usage_count         INT          NOT NULL DEFAULT 0,
                                  is_active           BOOLEAN      NOT NULL DEFAULT TRUE,
                                  is_verified         BOOLEAN      NOT NULL DEFAULT FALSE,
                                  verified_by         UUID         REFERENCES app_users(id),
                                  verified_at         TIMESTAMP,

                                  created_at          TIMESTAMP    NOT NULL DEFAULT NOW(),
                                  updated_at          TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- Đáp án câu hỏi
CREATE TABLE public.question_answers (
                                         id            UUID     PRIMARY KEY DEFAULT gen_random_uuid(),
                                         question_id   UUID     NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
                                         content       TEXT     NOT NULL,
                                         content_plain TEXT,
                                         is_correct    BOOLEAN  NOT NULL DEFAULT FALSE,
                                         sort_order    SMALLINT NOT NULL DEFAULT 0,
                                         explanation   TEXT
);

-- ============================================================
-- PHẦN 4: MẪU ĐỀ THI (Exam Templates)
-- ============================================================

CREATE TABLE public.exam_templates (
                                       id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
                                       grade_level_id   INT           NOT NULL REFERENCES grade_levels(id),
                                       subject_id       INT           NOT NULL REFERENCES subjects(id),
                                       created_by       UUID          NOT NULL REFERENCES app_users(id),

                                       title            VARCHAR(300)  NOT NULL,
                                       description      TEXT,
                                       duration_minutes INT           NOT NULL DEFAULT 45,
                                       total_questions  INT,
                                       total_score      NUMERIC(5,2)  NOT NULL DEFAULT 10.0,

                                       shuffle_questions  BOOLEAN     NOT NULL DEFAULT TRUE,
                                       shuffle_answers    BOOLEAN     NOT NULL DEFAULT TRUE,
                                       prevent_duplicate  BOOLEAN     NOT NULL DEFAULT TRUE,

                                       instructions     TEXT,
                                       is_active        BOOLEAN       NOT NULL DEFAULT TRUE,
                                       created_at       TIMESTAMP     NOT NULL DEFAULT NOW(),
                                       updated_at       TIMESTAMP     NOT NULL DEFAULT NOW()
);

-- Cấu hình từng phần của đề thi
CREATE TABLE public.exam_template_sections (
                                               id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                               exam_template_id UUID         NOT NULL REFERENCES exam_templates(id) ON DELETE CASCADE,
                                               topic_id         INT          REFERENCES topics(id),        -- NULL = toàn bộ môn
                                               question_type_id INT          REFERENCES question_types(id),-- NULL = tất cả loại
                                               section_name     VARCHAR(200),
                                               question_count   INT          NOT NULL,
                                               score_per_question NUMERIC(4,2),
                                               sort_order       SMALLINT     NOT NULL DEFAULT 0,

    -- Phân bổ độ khó (%)
                                               pct_easy      SMALLINT NOT NULL DEFAULT 0 CHECK (pct_easy      BETWEEN 0 AND 100),
                                               pct_medium    SMALLINT NOT NULL DEFAULT 0 CHECK (pct_medium    BETWEEN 0 AND 100),
                                               pct_hard      SMALLINT NOT NULL DEFAULT 0 CHECK (pct_hard      BETWEEN 0 AND 100),
                                               pct_very_hard SMALLINT NOT NULL DEFAULT 0 CHECK (pct_very_hard BETWEEN 0 AND 100),

                                               created_at TIMESTAMP NOT NULL DEFAULT NOW()
    -- Constraint tổng % = 100 xử lý ở application layer
);

-- ============================================================
-- PHẦN 5: ĐỀ THI (Generated Exams)
-- ============================================================

CREATE TABLE public.exams (
                              id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                              exam_template_id UUID         REFERENCES exam_templates(id),
                              grade_level_id   INT          NOT NULL REFERENCES grade_levels(id),
                              subject_id       INT          NOT NULL REFERENCES subjects(id),
                              created_by       UUID         NOT NULL REFERENCES app_users(id),

                              title            VARCHAR(300) NOT NULL,
                              exam_code        VARCHAR(50)  UNIQUE,           -- "DE_001"
                              duration_minutes INT          NOT NULL DEFAULT 45,
                              total_score      NUMERIC(5,2) NOT NULL DEFAULT 10.0,
                              instructions     TEXT,
                              status           VARCHAR(20)  NOT NULL DEFAULT 'draft'
                                  CHECK (status IN ('draft', 'published', 'archived')),

    -- Thông tin sử dụng
                              school_year  VARCHAR(20),                       -- "2024-2025"
                              semester     SMALLINT CHECK (semester IN (1, 2)),
                              exam_date    DATE,
                              class_name   VARCHAR(100),

    -- Batch generation
                              parent_exam_id UUID     REFERENCES exams(id),
                              variant_index  SMALLINT,
                              batch_id       UUID,

                              created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                              updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Câu hỏi trong đề thi (snapshot)
CREATE TABLE public.exam_questions (
                                       id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                       exam_id          UUID         NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
                                       question_id      UUID         NOT NULL REFERENCES questions(id),
                                       section_name     VARCHAR(200),
                                       sort_order       INT          NOT NULL,
                                       score            NUMERIC(4,2),

                                       content_snapshot TEXT         NOT NULL,
                                       answers_snapshot JSONB,                         -- [{content, is_correct, sort_order}]

                                       UNIQUE (exam_id, question_id)
);

-- ============================================================
-- PHẦN 6: KẾT QUẢ THI (Exam Results)
-- ============================================================

CREATE TABLE public.exam_submissions (
                                         id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                         exam_id          UUID         NOT NULL REFERENCES exams(id),
                                         student_id       UUID         NOT NULL REFERENCES app_users(id),

                                         started_at       TIMESTAMP    NOT NULL DEFAULT NOW(),
                                         submitted_at     TIMESTAMP,
                                         duration_seconds INT,

                                         total_score      NUMERIC(5,2),
                                         is_passed        BOOLEAN,
                                         status           VARCHAR(20)  NOT NULL DEFAULT 'in_progress'
                                             CHECK (status IN ('in_progress', 'submitted', 'graded')),

                                         created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Chi tiết câu trả lời
CREATE TABLE public.submission_answers (
                                           id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                           submission_id    UUID         NOT NULL REFERENCES exam_submissions(id) ON DELETE CASCADE,
                                           exam_question_id UUID         NOT NULL REFERENCES exam_questions(id),

                                           selected_answer_ids UUID[],
                                           essay_content       TEXT,
                                           is_correct          BOOLEAN,
                                           score_earned        NUMERIC(4,2) NOT NULL DEFAULT 0,
                                           feedback            TEXT,
                                           graded_by           UUID         REFERENCES app_users(id),

                                           UNIQUE (submission_id, exam_question_id)
);

-- ============================================================
-- PHẦN 7: INDEXES
-- ============================================================

CREATE INDEX idx_questions_topic      ON public.questions(topic_id);
CREATE INDEX idx_questions_difficulty ON public.questions(difficulty_level_id);
CREATE INDEX idx_questions_type       ON public.questions(question_type_id);
CREATE INDEX idx_questions_active     ON public.questions(is_active, is_verified);
CREATE INDEX idx_questions_tags       ON public.questions USING GIN(tags);
CREATE INDEX idx_questions_fulltext   ON public.questions USING GIN(to_tsvector('simple', content_plain));

-- Covering partial index cho sinh đề thi (quan trọng nhất)
CREATE INDEX idx_q_pool ON public.questions(topic_id, difficulty_level_id, question_type_id)
    INCLUDE (id)
    WHERE is_active = true AND is_verified = true;

CREATE INDEX idx_exams_template ON public.exams(exam_template_id);
CREATE INDEX idx_exams_subject   ON public.exams(subject_id);
CREATE INDEX idx_exams_grade     ON public.exams(grade_level_id);
CREATE INDEX idx_exams_status    ON public.exams(status);
CREATE INDEX idx_exams_batch     ON public.exams(batch_id);
CREATE INDEX idx_exams_parent    ON public.exams(parent_exam_id);

CREATE INDEX idx_submissions_exam     ON public.exam_submissions(exam_id);
CREATE INDEX idx_submissions_student  ON public.exam_submissions(student_id);

-- ============================================================
-- PHẦN 8: DỮ LIỆU MẪU (Seed Data)
-- ============================================================

INSERT INTO public.difficulty_levels (code, name, score_weight, sort_order) VALUES
                                                                                ('easy',      'Dễ',        1.0, 1),
                                                                                ('medium',    'Trung bình', 1.5, 2),
                                                                                ('hard',      'Khó',        2.0, 3),
                                                                                ('very_hard', 'Rất khó',    2.5, 4);

INSERT INTO public.question_types (code, name) VALUES
                                                   ('multiple_choice', 'Trắc nghiệm 1 đáp án'),
                                                   ('multiple_select', 'Trắc nghiệm nhiều đáp án'),
                                                   ('true_false',      'Đúng/Sai'),
                                                   ('fill_blank',      'Điền vào chỗ trống'),
                                                   ('essay',           'Tự luận'),
                                                   ('matching',        'Nối cột');

INSERT INTO public.grade_levels (name, grade_number) VALUES
                                                         ('Lớp 1', 1),  ('Lớp 2', 2),  ('Lớp 3', 3),  ('Lớp 4', 4),
                                                         ('Lớp 5', 5),  ('Lớp 6', 6),  ('Lớp 7', 7),  ('Lớp 8', 8),
                                                         ('Lớp 9', 9),  ('Lớp 10', 10), ('Lớp 11', 11), ('Lớp 12', 12);