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
-- QUẢN LÝ NGƯỜI DÙNG (Users)
-- ============================================================

CREATE TABLE public.app_users
(
    id                 UUID         NOT NULL PRIMARY KEY,
    username           VARCHAR(50)  NOT NULL,
    avartar            TEXT,
    normalizedusername VARCHAR(50)  NOT NULL,
    displayname        VARCHAR(150) NOT NULL,
    description        VARCHAR(500),
    phonenumber        VARCHAR(20),
    sex                BOOLEAN,
    refreshtoken       VARCHAR(500),
    email              JSON,
    accessfailedcount  SMALLINT,
    deleted            TIMESTAMPTZ,
    lockoutenabled     BOOLEAN,
    lockoutenddateutc  TIMESTAMPTZ,
    normalizedemail    VARCHAR(100),
    passwordhash       VARCHAR(100),
    roles              VARCHAR(50)[],
    providerkey        VARCHAR(50),
    loginprovider      VARCHAR(50),
    claims             JSON[],
    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150)
);



-- ============================================================
-- PHẦN 1: CẤU HÌNH HỆ THỐNG (Configuration)
-- ============================================================

-- Lớp học (1 - 12)
CREATE TABLE public.grade_levels
(
    id           SERIAL PRIMARY KEY,
    name         VARCHAR(50) NOT NULL,        -- "Lớp 10"
    grade_number SMALLINT    NOT NULL UNIQUE, -- 1 → 12
    description  TEXT,
    is_active    BOOLEAN     NOT NULL DEFAULT TRUE,
    created_by    VARCHAR(150),
    created      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by    VARCHAR(150),
    modified     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Môn học
CREATE TABLE public.subjects
(
    id             SERIAL PRIMARY KEY,
    grade_level_id INT          NOT NULL REFERENCES grade_levels (id) ON DELETE CASCADE,
    name           VARCHAR(100) NOT NULL, -- "Toán", "Ngữ văn", "Hóa học"
    code           VARCHAR(20)  NOT NULL, -- "MATH", "LIT", "CHEM"
    description    TEXT,
    is_active      BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by      VARCHAR(150),
    created        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by      VARCHAR(150),
    modified       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    UNIQUE (grade_level_id, code)
);

-- Chủ đề / Chương / Unit
CREATE TABLE public.topics
(
    id          SERIAL PRIMARY KEY,
    subject_id  INT          NOT NULL REFERENCES subjects (id) ON DELETE CASCADE,
    parent_id   INT REFERENCES topics (id), -- Hỗ trợ chủ đề lồng nhau
    name        VARCHAR(200) NOT NULL,      -- "Chương 1: Nguyên tử"
    code        VARCHAR(50),
    sort_order  INT          NOT NULL DEFAULT 0,
    description TEXT,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by   VARCHAR(150),
    created     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by   VARCHAR(150),
    modified    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Mức độ khó
CREATE TABLE public.difficulty_levels
(
    id           SERIAL PRIMARY KEY,
    code         VARCHAR(20)   NOT NULL UNIQUE, -- 'easy', 'medium', 'hard', 'very_hard'
    name         VARCHAR(50)   NOT NULL,        -- "Dễ", "Trung bình", "Khó", "Rất khó"
    score_weight NUMERIC(3, 2) NOT NULL DEFAULT 1.0,
    sort_order   SMALLINT      NOT NULL DEFAULT 0,
    is_active    BOOLEAN       NOT NULL DEFAULT TRUE,
    created_by    VARCHAR(150),
    created      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    modified_by    VARCHAR(150),
    modified     TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

-- Loại câu hỏi
CREATE TABLE public.question_types
(
    id          SERIAL PRIMARY KEY,
    code        VARCHAR(30)  NOT NULL UNIQUE, -- 'multiple_choice', 'true_false', ...
    name        VARCHAR(100) NOT NULL,        -- "Trắc nghiệm 4 đáp án"
    description TEXT,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by   VARCHAR(150),
    created     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by   VARCHAR(150),
    modified    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- ============================================================
-- [MỚI] Cấp độ nhận thức — Bloom's Taxonomy (2001 revision)
-- Phân loại câu hỏi theo 6 cấp độ tư duy của Benjamin Bloom
-- Dùng để: lọc câu hỏi, phân tích chất lượng đề thi,
--          phân bổ cấp độ nhận thức trong exam_template_sections
-- ============================================================
CREATE TABLE public.cognitive_levels
(
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
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by   VARCHAR(150),
    created     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by   VARCHAR(150),
    modified    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- ============================================================
-- PHẦN 2: QUẢN LÝ TRƯỜNG HỌC (School Management)
-- ============================================================

-- Thông tin trường học
CREATE TABLE public.schools
(
    id        SERIAL PRIMARY KEY,
    name      VARCHAR(255) NOT NULL,
    code      VARCHAR(50)  NOT NULL UNIQUE, -- "THPT-NGUYEN-DU", "TH-CHU-VAN-AN"
    address   TEXT,
    phone     VARCHAR(20),
    email     VARCHAR(100),
    is_active BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by VARCHAR(150),
    created   TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by VARCHAR(150),
    modified     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Khoá học — đơn vị tuyển sinh theo năm (VD: Khoá 2020-2025)
-- Mỗi khoá thuộc 1 trường, bao gồm nhiều năm học liên tiếp
CREATE TABLE public.cohorts
(
    id           SERIAL PRIMARY KEY,
    school_id    INT          NOT NULL REFERENCES schools (id) ON DELETE CASCADE,
    name         VARCHAR(100) NOT NULL,             -- "Khoá 2020-2025"
    start_year   SMALLINT     NOT NULL,             -- 2020
    end_year     SMALLINT     NOT NULL,             -- 2025
    grade_start  SMALLINT     NOT NULL,             -- Lớp bắt đầu: 1, 6, 10, ...
    num_classes  SMALLINT     NOT NULL DEFAULT 1,   -- Số lớp song song → A, B, C, ...
    is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
    created_by    VARCHAR(150),
    created      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by    VARCHAR(150),
    modified     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_cohort_years CHECK (end_year > start_year),
    CONSTRAINT chk_cohort_num_classes CHECK (num_classes BETWEEN 1 AND 26),
    UNIQUE (school_id, start_year, grade_start)
);

-- Lớp học — sinh tự động từ khoá qua trigger
-- VD: Khoá 2020-2025 (grade_start=1, suffix='A') → 1A/2020-2021 ... 5A/2024-2025
-- GVCN nằm ở đây vì có thể thay đổi mỗi năm học
CREATE TABLE public.cohort_classes
(
    id                  SERIAL PRIMARY KEY,
    cohort_id           INT         NOT NULL REFERENCES cohorts (id) ON DELETE CASCADE,
    grade_level_id      INT         NOT NULL REFERENCES grade_levels (id),
    class_name          VARCHAR(20) NOT NULL, -- "1A", "2A", "10A", ...
    section             VARCHAR(10) NOT NULL DEFAULT 'A', -- Ban/lớp: A, B, C, ...
    school_year         VARCHAR(20) NOT NULL, -- "2020-2021", "2021-2022"
    year_index          SMALLINT    NOT NULL, -- 1, 2, 3, ... (năm thứ mấy của khoá)
    homeroom_teacher_id UUID        REFERENCES app_users (id) ON DELETE SET NULL,
    created_by           VARCHAR(150),
    created             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150),
    modified            TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (cohort_id, year_index, section)
);

-- Học sinh thuộc khoá học
-- Tách riêng để app_users không bị thêm cột
-- 1 học sinh chỉ thuộc 1 khoá trong 1 trường
CREATE TABLE public.cohort_members
(
    id         UUID PRIMARY KEY     DEFAULT gen_random_uuid(),
    cohort_id  INT         NOT NULL REFERENCES cohorts (id) ON DELETE CASCADE,
    student_id UUID        NOT NULL REFERENCES app_users (id) ON DELETE CASCADE,
    section    VARCHAR(10),                      -- Lớp của HS (A, B, ...); NULL = chưa xếp lớp
    joined_at  DATE        NOT NULL DEFAULT CURRENT_DATE,
    is_active  BOOLEAN     NOT NULL DEFAULT TRUE,
    created_by  VARCHAR(150),
    created    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by  VARCHAR(150),
    modified   TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (cohort_id, student_id)
);

-- Giáo viên / Admin thuộc trường
-- Tách riêng để app_users không bị thêm cột
-- role ở đây là role ngữ cảnh trong trường — khác với roles[] JWT toàn hệ thống
CREATE TABLE public.school_members
(
    id        UUID PRIMARY KEY     DEFAULT gen_random_uuid(),
    school_id INT         NOT NULL REFERENCES schools (id) ON DELETE CASCADE,
    user_id   UUID        NOT NULL REFERENCES app_users (id) ON DELETE CASCADE,
    role      VARCHAR(20) NOT NULL CHECK (role IN ('Admin', 'Teacher')),
    is_active BOOLEAN     NOT NULL DEFAULT TRUE,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by VARCHAR(150),
    created   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by VARCHAR(150),
    modified  TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    UNIQUE (school_id, user_id)
);

-- ============================================================
-- PHẦN 2b: TRIGGER — Tự động sinh cohort_classes khi tạo khoá
-- ============================================================

-- Hàm sinh các dòng lớp học từ thông tin khoá
CREATE
OR REPLACE FUNCTION public.generate_cohort_classes(p_cohort_id INT)
RETURNS VOID AS $$
DECLARE
    v_cohort   public.cohorts%ROWTYPE;
    v_duration SMALLINT;
    i          SMALLINT;
    j          SMALLINT;
    v_section  VARCHAR(10);
BEGIN
    SELECT * INTO v_cohort FROM public.cohorts WHERE id = p_cohort_id;
    v_duration := v_cohort.end_year - v_cohort.start_year;

    FOR i IN 1..v_duration LOOP
        FOR j IN 1..v_cohort.num_classes LOOP
            v_section := chr(64 + j);   -- 1→A, 2→B, 3→C, ...
            INSERT INTO public.cohort_classes (
                cohort_id, grade_level_id, class_name, school_year, year_index, section
            ) VALUES (
                p_cohort_id,
                v_cohort.grade_start + i - 1,
                (v_cohort.grade_start + i - 1)::TEXT || v_section,
                (v_cohort.start_year + i - 1)::TEXT || '-' || (v_cohort.start_year + i)::TEXT,
                i,
                v_section
            );
        END LOOP;
    END LOOP;
END;
$$
LANGUAGE plpgsql;

-- Trigger gọi hàm trên sau mỗi INSERT vào cohorts
CREATE
OR REPLACE FUNCTION public.trg_generate_cohort_classes()
RETURNS TRIGGER AS $$
BEGIN
    PERFORM
public.generate_cohort_classes(NEW.id);
RETURN NEW;
END;
$$
LANGUAGE plpgsql;

CREATE TRIGGER trg_after_cohort_insert
    AFTER INSERT
    ON public.cohorts
    FOR EACH ROW
    EXECUTE FUNCTION public.trg_generate_cohort_classes();

-- Giáo viên phụ trách môn/lớp
CREATE TABLE public.teacher_subjects
(
    id         SERIAL PRIMARY KEY,
    user_id    UUID NOT NULL REFERENCES app_users (id) ON DELETE CASCADE,
    subject_id INT  NOT NULL REFERENCES subjects (id) ON DELETE CASCADE,
    UNIQUE (user_id, subject_id)
);

-- Phân công GV giảng dạy cho lớp (1 môn/lớp = 1 GV)
CREATE TABLE public.cohort_class_teachers
(
    id              SERIAL PRIMARY KEY,
    cohort_class_id INT  NOT NULL REFERENCES cohort_classes (id) ON DELETE CASCADE,
    subject_id      INT  NOT NULL REFERENCES subjects (id)       ON DELETE CASCADE,
    teacher_id      UUID NOT NULL REFERENCES app_users (id)      ON DELETE CASCADE,
    UNIQUE (cohort_class_id, subject_id)
);

-- ============================================================
-- PHẦN 4: NGÂN HÀNG CÂU HỎI (Question Bank)
-- ============================================================

CREATE TABLE public.questions
(
    id                  UUID PRIMARY KEY   DEFAULT gen_random_uuid(),
    topic_id            INT       NOT NULL REFERENCES topics (id),
    question_type_id    INT       NOT NULL REFERENCES question_types (id),
    difficulty_level_id INT       NOT NULL REFERENCES difficulty_levels (id),
    -- [MỚI] Phân loại theo Bloom's Taxonomy
    -- NULL = chưa phân loại (không bắt buộc để tương thích ngược)
    cognitive_level_id  INT       REFERENCES cognitive_levels (id) ON DELETE SET NULL,

    -- Nội dung câu hỏi
    content             TEXT      NOT NULL, -- HTML/Markdown
    content_plain       TEXT,               -- Thuần text để tìm kiếm full-text
    explanation         TEXT,
    image_url           TEXT,
    audio_url           TEXT,

    -- Metadata
    source              VARCHAR(200),
    tags                TEXT[],
    usage_count         INT       NOT NULL DEFAULT 0,
    is_active           BOOLEAN   NOT NULL DEFAULT TRUE,
    status              VARCHAR(20) NOT NULL DEFAULT 'pending'
                             CHECK (status IN ('pending', 'approved', 'rejected')),
    verified_by         UUID REFERENCES app_users (id),
    verified_at         TIMESTAMP,
    rejection_reason    TEXT,     -- Lý do từ chối (đi kèm status = 'rejected')

    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150)
);

-- Đáp án câu hỏi
CREATE TABLE public.question_answers
(
    id            UUID PRIMARY KEY  DEFAULT gen_random_uuid(),
    question_id   UUID     NOT NULL REFERENCES questions (id) ON DELETE CASCADE,
    content       TEXT     NOT NULL,
    content_plain TEXT,
    is_correct    BOOLEAN  NOT NULL DEFAULT FALSE,
    sort_order    SMALLINT NOT NULL DEFAULT 0,
    explanation   TEXT
);

-- ============================================================
-- PHẦN 5: MẪU ĐỀ THI (Exam Templates)
-- ============================================================

CREATE TABLE public.exam_templates
(
    id                UUID PRIMARY KEY       DEFAULT gen_random_uuid(),
    grade_level_id    INT           NOT NULL REFERENCES grade_levels (id),
    subject_id        INT           NOT NULL REFERENCES subjects (id),
    

    title             VARCHAR(300)  NOT NULL,
    description       TEXT,
    duration_minutes  INT           NOT NULL DEFAULT 45,
    total_questions   INT,
    total_score       NUMERIC(5, 2) NOT NULL DEFAULT 10.0,

    shuffle_questions BOOLEAN       NOT NULL DEFAULT TRUE,
    shuffle_answers   BOOLEAN       NOT NULL DEFAULT TRUE,
    prevent_duplicate BOOLEAN       NOT NULL DEFAULT TRUE,

    instructions      TEXT,
    is_active         BOOLEAN       NOT NULL DEFAULT TRUE,
    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150)
);

-- Cấu hình từng phần của đề thi
CREATE TABLE public.exam_template_sections
(
    id                 UUID PRIMARY KEY   DEFAULT gen_random_uuid(),
    exam_template_id   UUID      NOT NULL REFERENCES exam_templates (id) ON DELETE CASCADE,
    topic_id           INT REFERENCES topics (id),         -- NULL = toàn bộ môn
    question_type_id   INT REFERENCES question_types (id), -- NULL = tất cả loại
    -- [MỚI] Lọc câu hỏi theo cấp độ Bloom trong section này
    -- NULL = không lọc theo cấp độ nhận thức
    cognitive_level_id INT       REFERENCES cognitive_levels (id) ON DELETE SET NULL,

    section_name       VARCHAR(200),
    question_count     INT       NOT NULL,
    score_per_question NUMERIC(4, 2),
    sort_order         SMALLINT  NOT NULL DEFAULT 0,

    -- Phân bổ độ khó (%)
    pct_easy           SMALLINT  NOT NULL DEFAULT 0 CHECK (pct_easy BETWEEN 0 AND 100),
    pct_medium         SMALLINT  NOT NULL DEFAULT 0 CHECK (pct_medium BETWEEN 0 AND 100),
    pct_hard           SMALLINT  NOT NULL DEFAULT 0 CHECK (pct_hard BETWEEN 0 AND 100),
    pct_very_hard      SMALLINT  NOT NULL DEFAULT 0 CHECK (pct_very_hard BETWEEN 0 AND 100),
    -- Constraint tổng % = 100 xử lý ở application layer

    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150)
);

-- ============================================================
-- PHẦN 6: ĐỀ THI (Generated Exams)
-- ============================================================

CREATE TABLE public.exams
(
    id               UUID PRIMARY KEY       DEFAULT gen_random_uuid(),
    exam_template_id UUID REFERENCES exam_templates (id),
    grade_level_id   INT           NOT NULL REFERENCES grade_levels (id),
    subject_id       INT           NOT NULL REFERENCES subjects (id),

    title            VARCHAR(300)  NOT NULL,
    exam_code        VARCHAR(50) UNIQUE, -- "DE_001"
    duration_minutes INT           NOT NULL DEFAULT 45,
    total_score      NUMERIC(5, 2) NOT NULL DEFAULT 10.0,
    instructions     TEXT,
    status           VARCHAR(20)   NOT NULL DEFAULT 'draft'
        CHECK (status IN ('draft', 'published', 'archived')),

    -- Thông tin sử dụng
    school_year      VARCHAR(20),        -- "2024-2025"
    semester         SMALLINT CHECK (semester IN (1, 2)),
    exam_date        DATE,
    class_name       VARCHAR(100),

    -- Batch generation
    parent_exam_id   UUID REFERENCES exams (id),
    variant_index    SMALLINT,
    batch_id         UUID,

    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150)
);

-- Câu hỏi trong đề thi (snapshot tại thời điểm tạo đề)
CREATE TABLE public.exam_questions
(
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    exam_id          UUID NOT NULL REFERENCES exams (id) ON DELETE CASCADE,
    question_id      UUID NOT NULL REFERENCES questions (id),
    section_name     VARCHAR(200),
    sort_order       INT  NOT NULL,
    score            NUMERIC(4, 2),

    content_snapshot TEXT NOT NULL,
    answers_snapshot JSONB, -- [{content, is_correct, sort_order}]
    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150),
    UNIQUE (exam_id, question_id)
);

-- ============================================================
-- PHẦN 6b: KỲ THI (Exam Sessions)
-- ============================================================
CREATE TABLE public.exam_sessions
(
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title          VARCHAR(300) NOT NULL,
    description    TEXT,
    subject_id     INT NOT NULL REFERENCES subjects (id),
    grade_level_id INT NOT NULL REFERENCES grade_levels (id),
    open_at        TIMESTAMPTZ NOT NULL,
    close_at       TIMESTAMPTZ NOT NULL,
    max_attempts   SMALLINT NOT NULL DEFAULT 1 CHECK (max_attempts >= 1),
    pick_mode      VARCHAR(20) NOT NULL DEFAULT 'Random'
                   CHECK (pick_mode IN ('Random', 'StudentChoice')),
    status         VARCHAR(20) NOT NULL DEFAULT 'draft'
                   CHECK (status IN ('draft', 'published', 'closed')),
    created        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by     VARCHAR(150),
    modified       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by    VARCHAR(150),
    CHECK (close_at > open_at)
);

CREATE TABLE public.exam_session_exams
(
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id UUID NOT NULL REFERENCES exam_sessions (id) ON DELETE CASCADE,
    exam_id    UUID NOT NULL REFERENCES exams (id),
    UNIQUE (session_id, exam_id)
);

CREATE TABLE public.exam_session_assignments
(
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id      UUID NOT NULL REFERENCES exam_sessions (id) ON DELETE CASCADE,
    cohort_id       INT REFERENCES cohorts (id) ON DELETE CASCADE,
    cohort_class_id INT REFERENCES cohort_classes (id) ON DELETE CASCADE,
    CHECK ((cohort_id IS NOT NULL)::int + (cohort_class_id IS NOT NULL)::int = 1),
    UNIQUE (session_id, cohort_id, cohort_class_id)
);

-- ============================================================
-- PHẦN 7: KẾT QUẢ THI (Exam Results)
-- ============================================================

CREATE TABLE public.exam_submissions
(
    id               UUID PRIMARY KEY     DEFAULT gen_random_uuid(),
    exam_id          UUID        NOT NULL REFERENCES exams (id),
    student_id       UUID        NOT NULL REFERENCES app_users (id),
    session_id       UUID REFERENCES exam_sessions (id),
    attempt_no       SMALLINT    NOT NULL DEFAULT 1,

    started_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    submitted_at     TIMESTAMPTZ,
    duration_seconds INT,

    total_score      NUMERIC(5, 2),
    is_passed        BOOLEAN,
    status           VARCHAR(20) NOT NULL DEFAULT 'in_progress'
        CHECK (status IN ('in_progress', 'submitted', 'graded')),

    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150)
);

-- Chi tiết câu trả lời
CREATE TABLE public.submission_answers
(
    id                  UUID PRIMARY KEY       DEFAULT gen_random_uuid(),
    submission_id       UUID          NOT NULL REFERENCES exam_submissions (id) ON DELETE CASCADE,
    exam_question_id    UUID          NOT NULL REFERENCES exam_questions (id),

    selected_answer_ids UUID[],
    essay_content       TEXT,
    is_correct          BOOLEAN,
    score_earned        NUMERIC(4, 2) NOT NULL DEFAULT 0,
    feedback            TEXT,
    graded_by           UUID REFERENCES app_users (id),
    created            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_by           VARCHAR(150),
    modified           TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(150),
    UNIQUE (submission_id, exam_question_id)
);

-- ============================================================
-- PHẦN 8: INDEXES
-- ============================================================

-- School Management indexes
CREATE INDEX idx_cohorts_school ON public.cohorts (school_id, is_active);
CREATE INDEX idx_cohort_classes_year ON public.cohort_classes (school_year);
CREATE INDEX idx_cohort_classes_cohort ON public.cohort_classes (cohort_id, year_index);
CREATE INDEX idx_cohort_members_student ON public.cohort_members (student_id, is_active);
CREATE INDEX idx_cohort_members_cohort ON public.cohort_members (cohort_id, is_active);
CREATE INDEX idx_school_members_user ON public.school_members (user_id, is_active);
CREATE INDEX idx_school_members_school ON public.school_members (school_id, role);

CREATE INDEX idx_questions_topic ON public.questions (topic_id);
CREATE INDEX idx_questions_difficulty ON public.questions (difficulty_level_id);
CREATE INDEX idx_questions_type ON public.questions (question_type_id);
CREATE INDEX idx_questions_active ON public.questions (is_active, status);
CREATE INDEX idx_questions_tags ON public.questions USING GIN(tags);
CREATE INDEX idx_questions_fulltext ON public.questions USING GIN(to_tsvector('simple', content_plain));

-- [MỚI] Index cho cognitive_level_id — phục vụ lọc câu hỏi theo Bloom
CREATE INDEX idx_questions_cognitive ON public.questions (cognitive_level_id);

-- Covering partial index cho sinh đề thi (quan trọng nhất)
-- [CẬP NHẬT] Thêm cognitive_level_id vào INCLUDE để index-only scan
-- khi sinh đề có lọc theo cấp độ nhận thức
CREATE INDEX idx_q_pool ON public.questions (topic_id, difficulty_level_id, question_type_id) INCLUDE (id, cognitive_level_id)
    WHERE is_active = true AND status = 'approved';

-- [MỚI] Index hỗ trợ lọc pool theo cả cognitive_level
CREATE INDEX idx_q_pool_cognitive ON public.questions (topic_id, cognitive_level_id, difficulty_level_id) INCLUDE (id)
    WHERE is_active = true AND status = 'approved';

CREATE INDEX idx_exams_template ON public.exams (exam_template_id);
CREATE INDEX idx_exams_subject ON public.exams (subject_id);
CREATE INDEX idx_exams_grade ON public.exams (grade_level_id);
CREATE INDEX idx_exams_status ON public.exams (status);
CREATE INDEX idx_exams_batch ON public.exams (batch_id);
CREATE INDEX idx_exams_parent ON public.exams (parent_exam_id);

CREATE INDEX idx_submissions_exam ON public.exam_submissions (exam_id);
CREATE INDEX idx_submissions_student ON public.exam_submissions (student_id);
CREATE INDEX idx_submissions_session ON public.exam_submissions (session_id, student_id);

CREATE INDEX idx_exam_sessions_filter ON public.exam_sessions (subject_id, grade_level_id, status);
CREATE INDEX idx_session_assignments_session ON public.exam_session_assignments (session_id);
CREATE INDEX idx_session_assignments_cohort ON public.exam_session_assignments (cohort_id);
CREATE INDEX idx_session_assignments_class ON public.exam_session_assignments (cohort_class_id);

-- ============================================================
-- PHẦN 9: DỮ LIỆU MẪU (Seed Data)
-- ============================================================

INSERT INTO public.difficulty_levels (code, name, score_weight, sort_order)
VALUES ('easy', 'Dễ', 1.0, 1),
       ('medium', 'Trung bình', 1.5, 2),
       ('hard', 'Khó', 2.0, 3),
       ('very_hard', 'Rất khó', 2.5, 4);

INSERT INTO public.question_types (code, name)
VALUES ('multiple_choice', 'Trắc nghiệm 1 đáp án'),
       ('multiple_select', 'Trắc nghiệm nhiều đáp án'),
       ('true_false', 'Đúng/Sai'),
       ('fill_blank', 'Điền vào chỗ trống'),
       ('essay', 'Tự luận'),
       ('matching', 'Nối cột');

INSERT INTO public.grade_levels (name, grade_number)
VALUES ('Lớp 1', 1),
       ('Lớp 2', 2),
       ('Lớp 3', 3),
       ('Lớp 4', 4),
       ('Lớp 5', 5),
       ('Lớp 6', 6),
       ('Lớp 7', 7),
       ('Lớp 8', 8),
       ('Lớp 9', 9),
       ('Lớp 10', 10),
       ('Lớp 11', 11),
       ('Lớp 12', 12);

-- [MỚI] Seed data cho Bloom's Taxonomy (Anderson & Krathwohl, 2001)
-- 6 cấp độ nhận thức từ thấp → cao
INSERT INTO public.cognitive_levels (code, name, name_en, level_order, description, color_code)
VALUES ('remember',
        'Nhớ',
        'Remember',
        1,
        'Ghi nhớ và nhận biết thông tin, sự kiện, khái niệm đã học. '
            'Động từ tiêu biểu: liệt kê, xác định, nhận ra, gọi tên, ghi lại, định nghĩa.',
        '#4CAF50' -- Xanh lá — cấp thấp nhất, nền tảng
       ),
       ('understand',
        'Hiểu',
        'Understand',
        2,
        'Giải thích, diễn giải, tóm tắt ý nghĩa của thông tin theo cách của mình. '
            'Động từ tiêu biểu: giải thích, mô tả, phân loại, so sánh, tóm tắt, minh họa.',
        '#2196F3' -- Xanh dương
       ),
       ('apply',
        'Vận dụng',
        'Apply',
        3,
        'Sử dụng kiến thức đã học vào tình huống mới hoặc cụ thể. '
            'Động từ tiêu biểu: tính toán, giải, áp dụng, thực hiện, xây dựng, sử dụng.',
        '#FF9800' -- Cam
       ),
       ('analyze',
        'Phân tích',
        'Analyze',
        4,
        'Chia nhỏ thông tin thành các thành phần, xác định mối quan hệ và cấu trúc. '
            'Động từ tiêu biểu: phân tích, so sánh, phân biệt, kiểm tra, suy luận, phân loại.',
        '#9C27B0' -- Tím
       ),
       ('evaluate',
        'Đánh giá',
        'Evaluate',
        5,
        'Đưa ra phán xét, lập luận, bảo vệ hoặc phê bình dựa trên tiêu chí nhất định. '
            'Động từ tiêu biểu: đánh giá, phê bình, lập luận, bào chữa, ưu tiên, chứng minh.',
        '#F44336' -- Đỏ
       ),
       ('create',
        'Sáng tạo',
        'Create',
        6,
        'Tổng hợp kiến thức để tạo ra sản phẩm, ý tưởng hoặc giải pháp hoàn toàn mới. '
            'Động từ tiêu biểu: thiết kế, xây dựng, lập kế hoạch, sáng tác, đề xuất, tổng hợp.',
        '#E91E63' -- Hồng đậm — cấp cao nhất
       );
-- [v3] Seed data cho School Management Module
INSERT INTO public.schools (name, code, address)
VALUES ('Trường Tiểu học Nguyễn Du', 'TH-NGUYEN-DU', 'Hà Nội'),
       ('Trường THCS Chu Văn An', 'THCS-CHU-VAN-AN', 'Hà Nội'),
       ('Trường THPT Lê Quý Đôn', 'THPT-LE-QUY-DON', 'Hà Nội');

-- Khi INSERT cohort → trigger tự sinh cohort_classes
-- Trường tiểu học: Khoá 2020-2025 (lớp 1→5)
INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, num_classes)
VALUES (1, 'Khoá 2020-2025', 2020, 2025, 1, 1);
-- → Tự sinh: 1A/2020-2021, 2A/2021-2022, 3A/2022-2023, 4A/2023-2024, 5A/2024-2025

INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, num_classes)
VALUES (1, 'Khoá 2021-2026', 2021, 2026, 1, 1);
-- → Tự sinh: 1A/2021-2022, 2A/2022-2023, 3A/2023-2024, 4A/2024-2025, 5A/2025-2026

-- Trường THPT: Khoá 2021-2024 (lớp 10→12), 3 lớp A/B/C
INSERT INTO public.cohorts (school_id, name, start_year, end_year, grade_start, num_classes)
VALUES (3, 'Khoá 2021-2024', 2021, 2024, 10, 3);
-- → Tự sinh: 10A/10B/10C, 11A/11B/11C, 12A/12B/12C

insert into public.app_users (id, username, avartar, normalizedusername, displayname, description, phonenumber, sex,
                              refreshtoken, email, accessfailedcount, deleted, lockoutenabled, lockoutenddateutc,
                              normalizedemail, passwordhash, roles, providerkey, loginprovider, claims, created,
                              created_by, modified, modified_by)
values ('a2eb9cd9-2a7b-44da-b94b-f5507afe122f', 'admin@admin.vn', null, 'ADMIN@ADMIN.VN', 'Admin', null, 'undefined',
        false, null, 'null', 0, null, false, null, null, '+a/ufQN3DHM9mSsGn1h67ygKwTpqiokgRIPhjgGXBrg=', '{Admin}', null,
        null, '{}', '2026-06-07 10:52:08.226916 +00:00', null, '2026-06-07 10:52:08.226916 +00:00', null);

-- ============================================================
-- PHẦN 10: DỮ LIỆU MẪU — MÔN HỌC (subjects)
-- grade_level_id 1-12 tương ứng Lớp 1-12 (từ seed grade_levels)
-- ============================================================

INSERT INTO public.subjects (grade_level_id, name, code, description)
VALUES
-- Tiểu học
(1, 'Toán', 'MATH', 'Môn Toán lớp 1'),
(1, 'Tiếng Việt', 'VIE', 'Môn Tiếng Việt lớp 1'),
(2, 'Toán', 'MATH', 'Môn Toán lớp 2'),
(2, 'Tiếng Việt', 'VIE', 'Môn Tiếng Việt lớp 2'),
(3, 'Toán', 'MATH', 'Môn Toán lớp 3'),
(3, 'Tiếng Việt', 'VIE', 'Môn Tiếng Việt lớp 3'),
(3, 'Tự nhiên & Xã hội', 'TNXH', 'Môn Tự nhiên & Xã hội lớp 3'),
(4, 'Toán', 'MATH', 'Môn Toán lớp 4'),
(4, 'Tiếng Việt', 'VIE', 'Môn Tiếng Việt lớp 4'),
(4, 'Khoa học', 'SCI', 'Môn Khoa học lớp 4'),
(5, 'Toán', 'MATH', 'Môn Toán lớp 5'),
(5, 'Tiếng Việt', 'VIE', 'Môn Tiếng Việt lớp 5'),
(5, 'Khoa học', 'SCI', 'Môn Khoa học lớp 5'),
-- THCS
(6, 'Toán', 'MATH', 'Môn Toán lớp 6'),
(6, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 6'),
(6, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 6'),
(6, 'Khoa học tự nhiên', 'KHTN', 'Vật lý, Hóa học, Sinh học tích hợp'),
(6, 'Lịch sử & Địa lý', 'LSDL', 'Lịch sử và Địa lý tích hợp lớp 6'),
(6, 'Tin học', 'IT', 'Môn Tin học lớp 6'),
(7, 'Toán', 'MATH', 'Môn Toán lớp 7'),
(7, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 7'),
(7, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 7'),
(7, 'Khoa học tự nhiên', 'KHTN', 'Vật lý, Hóa học, Sinh học tích hợp'),
(7, 'Lịch sử & Địa lý', 'LSDL', 'Lịch sử và Địa lý tích hợp lớp 7'),
(7, 'Tin học', 'IT', 'Môn Tin học lớp 7'),
(8, 'Toán', 'MATH', 'Môn Toán lớp 8'),
(8, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 8'),
(8, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 8'),
(8, 'Vật lý', 'PHY', 'Môn Vật lý lớp 8'),
(8, 'Hóa học', 'CHEM', 'Môn Hóa học lớp 8'),
(8, 'Sinh học', 'BIO', 'Môn Sinh học lớp 8'),
(8, 'Lịch sử', 'HIST', 'Môn Lịch sử lớp 8'),
(8, 'Địa lý', 'GEO', 'Môn Địa lý lớp 8'),
(8, 'Tin học', 'IT', 'Môn Tin học lớp 8'),
(9, 'Toán', 'MATH', 'Môn Toán lớp 9'),
(9, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 9'),
(9, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 9'),
(9, 'Vật lý', 'PHY', 'Môn Vật lý lớp 9'),
(9, 'Hóa học', 'CHEM', 'Môn Hóa học lớp 9'),
(9, 'Sinh học', 'BIO', 'Môn Sinh học lớp 9'),
(9, 'Lịch sử', 'HIST', 'Môn Lịch sử lớp 9'),
(9, 'Địa lý', 'GEO', 'Môn Địa lý lớp 9'),
(9, 'Tin học', 'IT', 'Môn Tin học lớp 9'),
-- THPT
(10, 'Toán', 'MATH', 'Môn Toán lớp 10'),
(10, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 10'),
(10, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 10'),
(10, 'Vật lý', 'PHY', 'Môn Vật lý lớp 10'),
(10, 'Hóa học', 'CHEM', 'Môn Hóa học lớp 10'),
(10, 'Sinh học', 'BIO', 'Môn Sinh học lớp 10'),
(10, 'Lịch sử', 'HIST', 'Môn Lịch sử lớp 10'),
(10, 'Địa lý', 'GEO', 'Môn Địa lý lớp 10'),
(10, 'GDCD', 'GDCD', 'Giáo dục công dân lớp 10'),
(10, 'Tin học', 'IT', 'Môn Tin học lớp 10'),
(11, 'Toán', 'MATH', 'Môn Toán lớp 11'),
(11, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 11'),
(11, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 11'),
(11, 'Vật lý', 'PHY', 'Môn Vật lý lớp 11'),
(11, 'Hóa học', 'CHEM', 'Môn Hóa học lớp 11'),
(11, 'Sinh học', 'BIO', 'Môn Sinh học lớp 11'),
(11, 'Lịch sử', 'HIST', 'Môn Lịch sử lớp 11'),
(11, 'Địa lý', 'GEO', 'Môn Địa lý lớp 11'),
(11, 'GDCD', 'GDCD', 'Giáo dục công dân lớp 11'),
(11, 'Tin học', 'IT', 'Môn Tin học lớp 11'),
(12, 'Toán', 'MATH', 'Môn Toán lớp 12'),
(12, 'Ngữ văn', 'LIT', 'Môn Ngữ văn lớp 12'),
(12, 'Tiếng Anh', 'ENG', 'Môn Tiếng Anh lớp 12'),
(12, 'Vật lý', 'PHY', 'Môn Vật lý lớp 12'),
(12, 'Hóa học', 'CHEM', 'Môn Hóa học lớp 12'),
(12, 'Sinh học', 'BIO', 'Môn Sinh học lớp 12'),
(12, 'Lịch sử', 'HIST', 'Môn Lịch sử lớp 12'),
(12, 'Địa lý', 'GEO', 'Môn Địa lý lớp 12'),
(12, 'GDCD', 'GDCD', 'Giáo dục công dân lớp 12'),
(12, 'Tin học', 'IT', 'Môn Tin học lớp 12');

-- ============================================================
-- PHẦN 11: DỮ LIỆU MẪU — CHỦ ĐỀ / CHƯƠNG (topics)
-- Dùng DO block để tra cứu subject_id động, tránh hardcode
-- ============================================================

-- ── Toán lớp 10 ─────────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 10
  AND code = 'MATH';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Mệnh đề và tập hợp', 'C1', 1),
       (sid, 'Chương 2: Bất đẳng thức và bất phương trình', 'C2', 2),
       (sid, 'Chương 3: Hệ thức lượng trong tam giác. Vectơ', 'C3', 3),
       (sid, 'Chương 4: Tổ hợp — Xác suất', 'C4', 4),
       (sid, 'Chương 5: Dãy số — Cấp số cộng — Cấp số nhân', 'C5', 5),
       (sid, 'Chương 6: Giới hạn', 'C6', 6),
       (sid, 'Chương 7: Hàm số lượng giác — Phương trình lượng giác', 'C7', 7),
       (sid, 'Chương 8: Thống kê', 'C8', 8);
END $$;

-- ── Vật lý lớp 10 ───────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 10
  AND code = 'PHY';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Mở đầu — Đo lường', 'C1', 1),
       (sid, 'Chương 2: Động học', 'C2', 2),
       (sid, 'Chương 3: Động lực học', 'C3', 3),
       (sid, 'Chương 4: Năng lượng — Công — Công suất', 'C4', 4),
       (sid, 'Chương 5: Động lượng', 'C5', 5),
       (sid, 'Chương 6: Chuyển động tròn', 'C6', 6),
       (sid, 'Chương 7: Biến dạng của vật rắn', 'C7', 7),
       (sid, 'Chương 8: Áp suất chất lỏng — Áp suất khí quyển', 'C8', 8);
END $$;

-- ── Hóa học lớp 10 ──────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 10
  AND code = 'CHEM';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Nguyên tử', 'C1', 1),
       (sid, 'Chương 2: Bảng tuần hoàn các nguyên tố hóa học', 'C2', 2),
       (sid, 'Chương 3: Liên kết hóa học', 'C3', 3),
       (sid, 'Chương 4: Phản ứng oxi hóa – khử', 'C4', 4),
       (sid, 'Chương 5: Năng lượng hóa học', 'C5', 5),
       (sid, 'Chương 6: Tốc độ phản ứng hóa học', 'C6', 6),
       (sid, 'Chương 7: Nguyên tố nhóm VIIA — Halogen', 'C7', 7);
END $$;

-- ── Sinh học lớp 10 ─────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 10
  AND code = 'BIO';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Giới thiệu chung về thế giới sống', 'C1', 1),
       (sid, 'Chương 2: Thành phần hóa học của tế bào', 'C2', 2),
       (sid, 'Chương 3: Cấu trúc tế bào', 'C3', 3),
       (sid, 'Chương 4: Chuyển hóa vật chất và năng lượng trong tế bào', 'C4', 4),
       (sid, 'Chương 5: Chu kỳ tế bào và phân bào', 'C5', 5),
       (sid, 'Chương 6: Vi sinh vật', 'C6', 6);
END $$;

-- ── Ngữ văn lớp 10 ──────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 10
  AND code = 'LIT';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Phần 1: Thần thoại và sử thi', 'P1', 1),
       (sid, 'Phần 2: Thơ trữ tình — Thơ Đường', 'P2', 2),
       (sid, 'Phần 3: Truyện Nôm — Truyện Kiều', 'P3', 3),
       (sid, 'Phần 4: Văn học trung đại Việt Nam', 'P4', 4),
       (sid, 'Phần 5: Nghị luận xã hội', 'P5', 5),
       (sid, 'Phần 6: Tiếng Việt thực hành', 'P6', 6);
END $$;

-- ── Tiếng Anh lớp 10 ────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 10
  AND code = 'ENG';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Unit 1: Family life', 'U1', 1),
       (sid, 'Unit 2: Your body and you', 'U2', 2),
       (sid, 'Unit 3: Music', 'U3', 3),
       (sid, 'Unit 4: For a better community', 'U4', 4),
       (sid, 'Unit 5: Inventions', 'U5', 5),
       (sid, 'Unit 6: Gender equality', 'U6', 6),
       (sid, 'Unit 7: Viet Nam and international organisations', 'U7', 7),
       (sid, 'Unit 8: New ways to learn', 'U8', 8),
       (sid, 'Unit 9: Protecting the environment', 'U9', 9),
       (sid, 'Unit 10: Ecotourism', 'U10', 10);
END $$;

-- ── Toán lớp 11 ─────────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 11
  AND code = 'MATH';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Hàm số lượng giác — Phương trình lượng giác', 'C1', 1),
       (sid, 'Chương 2: Dãy số — Cấp số cộng — Cấp số nhân', 'C2', 2),
       (sid, 'Chương 3: Giới hạn', 'C3', 3),
       (sid, 'Chương 4: Đạo hàm và ứng dụng', 'C4', 4),
       (sid, 'Chương 5: Quan hệ song song trong không gian', 'C5', 5),
       (sid, 'Chương 6: Quan hệ vuông góc trong không gian', 'C6', 6),
       (sid, 'Chương 7: Hàm số mũ — Hàm số logarit', 'C7', 7),
       (sid, 'Chương 8: Xác suất', 'C8', 8);
END $$;

-- ── Vật lý lớp 11 ───────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 11
  AND code = 'PHY';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Dao động', 'C1', 1),
       (sid, 'Chương 2: Sóng', 'C2', 2),
       (sid, 'Chương 3: Điện trường', 'C3', 3),
       (sid, 'Chương 4: Dòng điện không đổi', 'C4', 4),
       (sid, 'Chương 5: Điện trường trong các môi trường', 'C5', 5),
       (sid, 'Chương 6: Từ trường', 'C6', 6),
       (sid, 'Chương 7: Cảm ứng điện từ', 'C7', 7);
END $$;

-- ── Hóa học lớp 11 ──────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 11
  AND code = 'CHEM';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Cân bằng hóa học', 'C1', 1),
       (sid, 'Chương 2: Nitrogen và Sulfur', 'C2', 2),
       (sid, 'Chương 3: Đại cương về hóa học hữu cơ', 'C3', 3),
       (sid, 'Chương 4: Hydrocarbon', 'C4', 4),
       (sid, 'Chương 5: Dẫn xuất Halogen — Alcohol — Phenol', 'C5', 5),
       (sid, 'Chương 6: Hợp chất Carbonyl — Carboxylic acid', 'C6', 6);
END $$;

-- ── Sinh học lớp 11 ─────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 11
  AND code = 'BIO';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Trao đổi chất và chuyển hóa năng lượng ở thực vật', 'C1', 1),
       (sid, 'Chương 2: Trao đổi chất và chuyển hóa năng lượng ở động vật', 'C2', 2),
       (sid, 'Chương 3: Cảm ứng ở thực vật', 'C3', 3),
       (sid, 'Chương 4: Cảm ứng ở động vật', 'C4', 4),
       (sid, 'Chương 5: Sinh trưởng và phát triển ở thực vật', 'C5', 5),
       (sid, 'Chương 6: Sinh trưởng và phát triển ở động vật', 'C6', 6),
       (sid, 'Chương 7: Sinh sản', 'C7', 7);
END $$;

-- ── Toán lớp 12 ─────────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'MATH';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Ứng dụng đạo hàm để khảo sát và vẽ đồ thị hàm số', 'C1', 1),
       (sid, 'Chương 2: Hàm số mũ — Hàm số logarit', 'C2', 2),
       (sid, 'Chương 3: Nguyên hàm — Tích phân', 'C3', 3),
       (sid, 'Chương 4: Số phức', 'C4', 4),
       (sid, 'Chương 5: Thể tích khối đa diện', 'C5', 5),
       (sid, 'Chương 6: Mặt nón — Mặt trụ — Mặt cầu', 'C6', 6),
       (sid, 'Chương 7: Phương pháp tọa độ trong không gian', 'C7', 7),
       (sid, 'Chương 8: Xác suất nâng cao', 'C8', 8);
END $$;

-- ── Vật lý lớp 12 ───────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'PHY';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Dao động cơ', 'C1', 1),
       (sid, 'Chương 2: Sóng cơ và sóng âm', 'C2', 2),
       (sid, 'Chương 3: Điện xoay chiều', 'C3', 3),
       (sid, 'Chương 4: Dao động và sóng điện từ', 'C4', 4),
       (sid, 'Chương 5: Sóng ánh sáng', 'C5', 5),
       (sid, 'Chương 6: Lượng tử ánh sáng', 'C6', 6),
       (sid, 'Chương 7: Hạt nhân nguyên tử', 'C7', 7);
END $$;

-- ── Hóa học lớp 12 ──────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'CHEM';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Este — Lipit', 'C1', 1),
       (sid, 'Chương 2: Carbohydrate', 'C2', 2),
       (sid, 'Chương 3: Amin — Amino acid — Protein', 'C3', 3),
       (sid, 'Chương 4: Polimer và vật liệu polimer', 'C4', 4),
       (sid, 'Chương 5: Đại cương về kim loại', 'C5', 5),
       (sid, 'Chương 6: Kim loại kiềm — Kiềm thổ — Nhôm', 'C6', 6),
       (sid, 'Chương 7: Sắt và một số kim loại quan trọng', 'C7', 7),
       (sid, 'Chương 8: Hóa học môi trường', 'C8', 8);
END $$;

-- ── Sinh học lớp 12 ─────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'BIO';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Chương 1: Cơ chế di truyền và biến dị', 'C1', 1),
       (sid, 'Chương 2: Tính quy luật của hiện tượng di truyền', 'C2', 2),
       (sid, 'Chương 3: Di truyền học quần thể', 'C3', 3),
       (sid, 'Chương 4: Ứng dụng di truyền học', 'C4', 4),
       (sid, 'Chương 5: Di truyền học người', 'C5', 5),
       (sid, 'Chương 6: Bằng chứng và cơ chế tiến hóa', 'C6', 6),
       (sid, 'Chương 7: Sự phát sinh và phát triển sự sống', 'C7', 7),
       (sid, 'Chương 8: Sinh thái học', 'C8', 8);
END $$;

-- ── Ngữ văn lớp 12 ──────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'LIT';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Phần 1: Văn học Việt Nam 1945 – 1975', 'P1', 1),
       (sid, 'Phần 2: Văn học Việt Nam sau 1975', 'P2', 2),
       (sid, 'Phần 3: Văn học nước ngoài', 'P3', 3),
       (sid, 'Phần 4: Nghị luận văn học', 'P4', 4),
       (sid, 'Phần 5: Nghị luận xã hội', 'P5', 5),
       (sid, 'Phần 6: Tiếng Việt tổng hợp', 'P6', 6);
END $$;

-- ── Lịch sử lớp 12 ──────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'HIST';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Phần 1: Lịch sử thế giới hiện đại (1945 – 2000)', 'P1', 1),
       (sid, 'Phần 2: Lịch sử Việt Nam (1919 – 1945)', 'P2', 2),
       (sid, 'Phần 3: Lịch sử Việt Nam (1945 – 1975)', 'P3', 3),
       (sid, 'Phần 4: Lịch sử Việt Nam (1975 – 2000)', 'P4', 4);
END $$;

-- ── Địa lý lớp 12 ───────────────────────────────────────────
DO
$$ DECLARE
sid INT;
BEGIN
SELECT id
INTO sid
FROM public.subjects
WHERE grade_level_id = 12
  AND code = 'GEO';
INSERT INTO public.topics (subject_id, name, code, sort_order)
VALUES (sid, 'Phần 1: Địa lý tự nhiên Việt Nam', 'P1', 1),
       (sid, 'Phần 2: Địa lý dân cư và lao động', 'P2', 2),
       (sid, 'Phần 3: Địa lý kinh tế', 'P3', 3),
       (sid, 'Phần 4: Địa lý các vùng kinh tế', 'P4', 4),
       (sid, 'Phần 5: Địa lý biển đảo Việt Nam', 'P5', 5);
END $$;

-- ============================================================
-- MIGRATION [2026-07-19]: multi-class per cohort + student section
-- Chạy TAY trên DB dev đã tồn tại (schema gốc phía trên đã có sẵn cột mới).
-- ============================================================
-- ALTER TABLE public.cohorts ADD COLUMN num_classes SMALLINT NOT NULL DEFAULT 1;
-- ALTER TABLE public.cohorts ADD CONSTRAINT chk_cohort_num_classes CHECK (num_classes BETWEEN 1 AND 26);
-- ALTER TABLE public.cohorts DROP COLUMN class_suffix;
-- ALTER TABLE public.cohort_classes ADD COLUMN section VARCHAR(10) NOT NULL DEFAULT 'A';
-- ALTER TABLE public.cohort_classes DROP CONSTRAINT cohort_classes_cohort_id_year_index_key;
-- ALTER TABLE public.cohort_classes ADD UNIQUE (cohort_id, year_index, section);
-- ALTER TABLE public.cohort_members ADD COLUMN section VARCHAR(10);
-- (sau đó CREATE OR REPLACE FUNCTION generate_cohort_classes bản mới ở trên)
