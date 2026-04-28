-- ============================================================
-- EXAMHUB — DATABASE SCHEMA v3
-- PostgreSQL 16
-- v2: Thêm bảng cognitive_levels (Bloom's Taxonomy)
--     FK cognitive_level_id vào questions, exam_template_sections
-- v3: Thêm School Management Module
--     schools, cohorts, cohort_classes, cohort_members, school_members
--     Trigger tự động sinh cohort_classes khi INSERT cohort
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
                                 grade_level_id INT          NOT NULL REFERENCES grade_levels(id) ON DELETE CASCADE,
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
                                          code         VARCHAR(20)  NOT NULL UNIQUE,   -- 'easy', 'medium', 'hard', 'very_hard'
                                          name         VARCHAR(50)  NOT NULL,          -- "Dễ", "Trung bình", "Khó", "Rất khó"
                                          score_weight NUMERIC(3,2) NOT NULL DEFAULT 1.0,
                                          sort_order   SMALLINT     NOT NULL DEFAULT 0,
                                          is_active    BOOLEAN      NOT NULL DEFAULT TRUE
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
-- [MỚI] Cấp độ nhận thức — Bloom's Taxonomy (2001 revision)
-- Phân loại câu hỏi theo 6 cấp độ tư duy của Benjamin Bloom
-- Dùng để: lọc câu hỏi, phân tích chất lượng đề thi,
--          phân bổ cấp độ nhận thức trong exam_template_sections
-- ============================================================
CREATE TABLE public.cognitive_levels (
                                         id          SERIAL PRIMARY KEY,
                                         code        VARCHAR(30)  NOT NULL UNIQUE,
    -- 'remember' | 'understand' | 'apply'
    -- 'analyze'  | 'evaluate'   | 'create'
                                         name        VARCHAR(100) NOT NULL,
    -- "Nhớ" | "Hiểu" | "Vận dụng"
    -- "Phân tích" | "Đánh giá" | "Sáng tạo"
                                         name_en     VARCHAR(100) NOT NULL,
    -- "Remember" | "Understand" | "Apply"
    -- "Analyze"  | "Evaluate"   | "Create"
                                         level_order SMALLINT     NOT NULL UNIQUE CHECK (level_order BETWEEN 1 AND 6),
    -- Thứ tự từ thấp → cao: 1 (Nhớ) → 6 (Sáng tạo)
                                         description TEXT,
    -- Mô tả chi tiết cấp độ, các động từ hành động tiêu biểu
                                         color_code  VARCHAR(10),
    -- Hex color để hiển thị badge UI (#4CAF50, #2196F3, ...)
                                         is_active   BOOLEAN      NOT NULL DEFAULT TRUE
);

-- ============================================================
-- PHẦN 2: QUẢN LÝ TRƯỜNG HỌC (School Management)
-- ============================================================

-- Thông tin trường học
CREATE TABLE public.schools (
                                id         SERIAL       PRIMARY KEY,
                                name       VARCHAR(255) NOT NULL,
                                code       VARCHAR(50)  NOT NULL UNIQUE,   -- "THPT-NGUYEN-DU", "TH-CHU-VAN-AN"
                                address    TEXT,
                                phone      VARCHAR(20),
                                email      VARCHAR(100),
                                is_active  BOOLEAN      NOT NULL DEFAULT TRUE,
                                created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
                                updated_at TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Khoá học — đơn vị tuyển sinh theo năm (VD: Khoá 2020-2025)
-- Mỗi khoá thuộc 1 trường, bao gồm nhiều năm học liên tiếp
CREATE TABLE public.cohorts (
                                id           SERIAL       PRIMARY KEY,
                                school_id    INT          NOT NULL REFERENCES schools(id) ON DELETE CASCADE,
                                name         VARCHAR(100) NOT NULL,            -- "Khoá 2020-2025"
                                start_year   SMALLINT     NOT NULL,            -- 2020
                                end_year     SMALLINT     NOT NULL,            -- 2025
                                grade_start  SMALLINT     NOT NULL,            -- Lớp bắt đầu: 1, 6, 10, ...
                                class_suffix VARCHAR(10)  NOT NULL DEFAULT 'A', -- Hậu tố: "A" → 1A, 2A, 3A, ...
                                is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
                                created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

                                CONSTRAINT chk_cohort_years CHECK (end_year > start_year),
                                UNIQUE (school_id, start_year, grade_start)
);

-- Lớp học — sinh tự động từ khoá qua trigger
-- VD: Khoá 2020-2025 (grade_start=1, suffix='A') → 1A/2020-2021 ... 5A/2024-2025
-- GVCN nằm ở đây vì có thể thay đổi mỗi năm học
CREATE TABLE public.cohort_classes (
                                       id                  SERIAL       PRIMARY KEY,
                                       cohort_id           INT          NOT NULL REFERENCES cohorts(id) ON DELETE CASCADE,
                                       grade_level_id      INT          NOT NULL REFERENCES grade_levels(id),
                                       class_name          VARCHAR(20)  NOT NULL,   -- "1A", "2A", "10A", ...
                                       school_year         VARCHAR(20)  NOT NULL,   -- "2020-2021", "2021-2022"
                                       year_index          SMALLINT     NOT NULL,   -- 1, 2, 3, ... (năm thứ mấy của khoá)
                                       homeroom_teacher_id UUID         REFERENCES app_users(id) ON DELETE SET NULL,
                                       created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

                                       UNIQUE (cohort_id, year_index)
);

-- Học sinh thuộc khoá học
-- Tách riêng để app_users không bị thêm cột
-- 1 học sinh chỉ thuộc 1 khoá trong 1 trường
CREATE TABLE public.cohort_members (
                                       id         UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
                                       cohort_id  INT         NOT NULL REFERENCES cohorts(id) ON DELETE CASCADE,
                                       student_id UUID        NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
                                       joined_at  DATE        NOT NULL DEFAULT CURRENT_DATE,
                                       is_active  BOOLEAN     NOT NULL DEFAULT TRUE,

                                       UNIQUE (cohort_id, student_id)
);

-- Giáo viên / Admin thuộc trường
-- Tách riêng để app_users không bị thêm cột
-- role ở đây là role ngữ cảnh trong trường — khác với roles[] JWT toàn hệ thống
CREATE TABLE public.school_members (
                                       id        UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
                                       school_id INT         NOT NULL REFERENCES schools(id) ON DELETE CASCADE,
                                       user_id   UUID        NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
                                       role      VARCHAR(20) NOT NULL CHECK (role IN ('Admin', 'Teacher')),
                                       is_active BOOLEAN     NOT NULL DEFAULT TRUE,
                                       joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

                                       UNIQUE (school_id, user_id)
);

-- ============================================================
-- PHẦN 2b: TRIGGER — Tự động sinh cohort_classes khi tạo khoá
-- ============================================================

-- Hàm sinh các dòng lớp học từ thông tin khoá
CREATE OR REPLACE FUNCTION public.generate_cohort_classes(p_cohort_id INT)
RETURNS VOID AS $$
DECLARE
v_cohort   public.cohorts%ROWTYPE;
    v_duration SMALLINT;
    i          SMALLINT;
BEGIN
SELECT * INTO v_cohort FROM public.cohorts WHERE id = p_cohort_id;
v_duration := v_cohort.end_year - v_cohort.start_year;

FOR i IN 1..v_duration LOOP
        INSERT INTO public.cohort_classes (
            cohort_id,
            grade_level_id,
            class_name,
            school_year,
            year_index
        ) VALUES (
            p_cohort_id,
            v_cohort.grade_start + i - 1,
            (v_cohort.grade_start + i - 1)::TEXT || v_cohort.class_suffix,
            (v_cohort.start_year + i - 1)::TEXT || '-' || (v_cohort.start_year + i)::TEXT,
            i
        );
END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Trigger gọi hàm trên sau mỗi INSERT vào cohorts
CREATE OR REPLACE FUNCTION public.trg_generate_cohort_classes()
RETURNS TRIGGER AS $$
BEGIN
    PERFORM public.generate_cohort_classes(NEW.id);
RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_after_cohort_insert
    AFTER INSERT ON public.cohorts
    FOR EACH ROW
    EXECUTE FUNCTION public.trg_generate_cohort_classes();

-- ============================================================
-- PHẦN 3: QUẢN LÝ NGƯỜI DÙNG (Users)
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
-- PHẦN 4: NGÂN HÀNG CÂU HỎI (Question Bank)
-- ============================================================

CREATE TABLE public.questions (
                                  id                   UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                  topic_id             INT          NOT NULL REFERENCES topics(id),
                                  question_type_id     INT          NOT NULL REFERENCES question_types(id),
                                  difficulty_level_id  INT          NOT NULL REFERENCES difficulty_levels(id),
    -- [MỚI] Phân loại theo Bloom's Taxonomy
    -- NULL = chưa phân loại (không bắt buộc để tương thích ngược)
                                  cognitive_level_id   INT          REFERENCES cognitive_levels(id) ON DELETE SET NULL,
                                  created_by           UUID         NOT NULL REFERENCES app_users(id),

    -- Nội dung câu hỏi
                                  content              TEXT         NOT NULL,    -- HTML/Markdown
                                  content_plain        TEXT,                     -- Thuần text để tìm kiếm full-text
                                  explanation          TEXT,
                                  image_url            TEXT,
                                  audio_url            TEXT,

    -- Metadata
                                  source               VARCHAR(200),
                                  tags                 TEXT[],
                                  usage_count          INT          NOT NULL DEFAULT 0,
                                  is_active            BOOLEAN      NOT NULL DEFAULT TRUE,
                                  is_verified          BOOLEAN      NOT NULL DEFAULT FALSE,
                                  verified_by          UUID         REFERENCES app_users(id),
                                  verified_at          TIMESTAMP,

                                  created_at           TIMESTAMP    NOT NULL DEFAULT NOW(),
                                  updated_at           TIMESTAMP    NOT NULL DEFAULT NOW()
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
-- PHẦN 5: MẪU ĐỀ THI (Exam Templates)
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
                                               topic_id         INT          REFERENCES topics(id),         -- NULL = toàn bộ môn
                                               question_type_id INT          REFERENCES question_types(id), -- NULL = tất cả loại
    -- [MỚI] Lọc câu hỏi theo cấp độ Bloom trong section này
    -- NULL = không lọc theo cấp độ nhận thức
                                               cognitive_level_id INT        REFERENCES cognitive_levels(id) ON DELETE SET NULL,

                                               section_name       VARCHAR(200),
                                               question_count     INT          NOT NULL,
                                               score_per_question NUMERIC(4,2),
                                               sort_order         SMALLINT     NOT NULL DEFAULT 0,

    -- Phân bổ độ khó (%)
                                               pct_easy      SMALLINT NOT NULL DEFAULT 0 CHECK (pct_easy      BETWEEN 0 AND 100),
                                               pct_medium    SMALLINT NOT NULL DEFAULT 0 CHECK (pct_medium    BETWEEN 0 AND 100),
                                               pct_hard      SMALLINT NOT NULL DEFAULT 0 CHECK (pct_hard      BETWEEN 0 AND 100),
                                               pct_very_hard SMALLINT NOT NULL DEFAULT 0 CHECK (pct_very_hard BETWEEN 0 AND 100),
    -- Constraint tổng % = 100 xử lý ở application layer

                                               created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- PHẦN 6: ĐỀ THI (Generated Exams)
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

-- Câu hỏi trong đề thi (snapshot tại thời điểm tạo đề)
CREATE TABLE public.exam_questions (
                                       id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
                                       exam_id          UUID         NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
                                       question_id      UUID         NOT NULL REFERENCES questions(id),
                                       section_name     VARCHAR(200),
                                       sort_order       INT          NOT NULL,
                                       score            NUMERIC(4,2),

                                       content_snapshot TEXT         NOT NULL,
                                       answers_snapshot JSONB,        -- [{content, is_correct, sort_order}]

                                       UNIQUE (exam_id, question_id)
);

-- ============================================================
-- PHẦN 7: KẾT QUẢ THI (Exam Results)
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
-- PHẦN 8: INDEXES
-- ============================================================

-- School Management indexes
CREATE INDEX idx_cohorts_school         ON public.cohorts(school_id, is_active);
CREATE INDEX idx_cohort_classes_year    ON public.cohort_classes(school_year);
CREATE INDEX idx_cohort_classes_cohort  ON public.cohort_classes(cohort_id, year_index);
CREATE INDEX idx_cohort_members_student ON public.cohort_members(student_id, is_active);
CREATE INDEX idx_cohort_members_cohort  ON public.cohort_members(cohort_id, is_active);
CREATE INDEX idx_school_members_user    ON public.school_members(user_id, is_active);
CREATE INDEX idx_school_members_school  ON public.school_members(school_id, role);

CREATE INDEX idx_questions_topic      ON public.questions(topic_id);
CREATE INDEX idx_questions_difficulty ON public.questions(difficulty_level_id);
CREATE INDEX idx_questions_type       ON public.questions(question_type_id);
CREATE INDEX idx_questions_active     ON public.questions(is_active, is_verified);
CREATE INDEX idx_questions_tags       ON public.questions USING GIN(tags);
CREATE INDEX idx_questions_fulltext   ON public.questions USING GIN(to_tsvector('simple', content_plain));

-- [MỚI] Index cho cognitive_level_id — phục vụ lọc câu hỏi theo Bloom
CREATE INDEX idx_questions_cognitive  ON public.questions(cognitive_level_id);

-- Covering partial index cho sinh đề thi (quan trọng nhất)
-- [CẬP NHẬT] Thêm cognitive_level_id vào INCLUDE để index-only scan
-- khi sinh đề có lọc theo cấp độ nhận thức
CREATE INDEX idx_q_pool ON public.questions(topic_id, difficulty_level_id, question_type_id)
    INCLUDE (id, cognitive_level_id)
    WHERE is_active = true AND is_verified = true;

-- [MỚI] Index hỗ trợ lọc pool theo cả cognitive_level
CREATE INDEX idx_q_pool_cognitive ON public.questions(topic_id, cognitive_level_id, difficulty_level_id)
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
-- PHẦN 9: DỮ LIỆU MẪU (Seed Data)
-- ============================================================

INSERT INTO public.difficulty_levels (code, name, score_weight, sort_order) VALUES
                                                                                ('easy',      'Dễ',         1.0, 1),
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
                                                         ('Lớp 1',  1),  ('Lớp 2',  2),  ('Lớp 3',  3),  ('Lớp 4',  4),
                                                         ('Lớp 5',  5),  ('Lớp 6',  6),  ('Lớp 7',  7),  ('Lớp 8',  8),
                                                         ('Lớp 9',  9),  ('Lớp 10', 10), ('Lớp 11', 11), ('Lớp 12', 12);

-- [MỚI] Seed data cho Bloom's Taxonomy (Anderson & Krathwohl, 2001)
-- 6 cấp độ nhận thức từ thấp → cao
INSERT INTO public.cognitive_levels (code, name, name_en, level_order, description, color_code) VALUES
                                                                                                    (
                                                                                                        'remember',
                                                                                                        'Nhớ',
                                                                                                        'Remember',
                                                                                                        1,
                                                                                                        'Ghi nhớ và nhận biết thông tin, sự kiện, khái niệm đã học. '
                                                                                                            'Động từ tiêu biểu: liệt kê, xác định, nhận ra, gọi tên, ghi lại, định nghĩa.',
                                                                                                        '#4CAF50'   -- Xanh lá — cấp thấp nhất, nền tảng
                                                                                                    ),
                                                                                                    (
                                                                                                        'understand',
                                                                                                        'Hiểu',
                                                                                                        'Understand',
                                                                                                        2,
                                                                                                        'Giải thích, diễn giải, tóm tắt ý nghĩa của thông tin theo cách của mình. '
                                                                                                            'Động từ tiêu biểu: giải thích, mô tả, phân loại, so sánh, tóm tắt, minh họa.',
                                                                                                        '#2196F3'   -- Xanh dương
                                                                                                    ),
                                                                                                    (
                                                                                                        'apply',
                                                                                                        'Vận dụng',
                                                                                                        'Apply',
                                                                                                        3,
                                                                                                        'Sử dụng kiến thức đã học vào tình huống mới hoặc cụ thể. '
                                                                                                            'Động từ tiêu biểu: tính toán, giải, áp dụng, thực hiện, xây dựng, sử dụng.',
                                                                                                        '#FF9800'   -- Cam
                                                                                                    ),
                                                                                                    (
                                                                                                        'analyze',
                                                                                                        'Phân tích',
                                                                                                        'Analyze',
                                                                                                        4,
                                                                                                        'Chia nhỏ thông tin thành các thành phần, xác định mối quan hệ và cấu trúc. '
                                                                                                            'Động từ tiêu biểu: phân tích, so sánh, phân biệt, kiểm tra, suy luận, phân loại.',
                                                                                                        '#9C27B0'   -- Tím
                                                                                                    ),
                                                                                                    (
                                                                                                        'evaluate',
                                                                                                        'Đánh giá',
                                                                                                        'Evaluate',
                                                                                                        5,
                                                                                                        'Đưa ra phán xét, lập luận, bảo vệ hoặc phê bình dựa trên tiêu chí nhất định. '
                                                                                                            'Động từ tiêu biểu: đánh giá, phê bình, lập luận, bào chữa, ưu tiên, chứng minh.',
                                                                                                        '#F44336'   -- Đỏ
                                                                                                    ),
                                                                                                    (
                                                                                                        'create',
                                                                                                        'Sáng tạo',
                                                                                                        'Create',
                                                                                                        6,
                                                                                                        'Tổng hợp kiến thức để tạo ra sản phẩm, ý tưởng hoặc giải pháp hoàn toàn mới. '
                                                                                                            'Động từ tiêu biểu: thiết kế, xây dựng, lập kế hoạch, sáng tác, đề xuất, tổng hợp.',
                                                                                                        '#E91E63'   -- Hồng đậm — cấp cao nhất
                                                                                                    );
-- [v3] Seed data cho School Management Module
INSERT INTO public.schools (name, code, address) VALUES
                                                     ('Trường Tiểu học Nguyễn Du',  'TH-NGUYEN-DU',    'Hà Nội'),
                                                     ('Trường THCS Chu Văn An',     'THCS-CHU-VAN-AN',  'Hà Nội'),
                                                     ('Trường THPT Lê Quý Đôn',     'THPT-LE-QUY-DON',  'Hà Nội');

-- Khi INSERT cohort → trigger tự sinh cohort_classes
-- Trường tiểu học: Khoá 2020-2025 (lớp 1→5)
INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, class_suffix)
VALUES (1, 'Khoá 2020-2025', 2020, 2025, 1, 'A');
-- → Tự sinh: 1A/2020-2021, 2A/2021-2022, 3A/2022-2023, 4A/2023-2024, 5A/2024-2025

INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, class_suffix)
VALUES (1, 'Khoá 2021-2026', 2021, 2026, 1, 'A');
-- → Tự sinh: 1A/2021-2022, 2A/2022-2023, 3A/2023-2024, 4A/2024-2025, 5A/2025-2026

-- Trường THPT: Khoá 2021-2024 (lớp 10→12)
INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, class_suffix)
VALUES (3, 'Khoá 2021-2024', 2021, 2024, 10, 'A');
-- → Tự sinh: 10A/2021-2022, 11A/2022-2023, 12A/2023-2024
