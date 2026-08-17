import type {StatusVariant} from '../components/StatusTag'

/* ─── Exam (ExamStatus: Draft | Published | Archived) ─── */
export const EXAM_STATUS_LABEL: Record<ExamStatus, string> = {
    Draft: 'Nháp', Published: 'Đã phát hành', Archived: 'Lưu trữ',
}
export const EXAM_STATUS_VARIANT: Record<ExamStatus, StatusVariant> = {
    Draft: 'warning', Published: 'success', Archived: 'default',
}
/** Màu cho AntD <Tag> (DashboardPage). */
export const EXAM_STATUS_TAG_COLOR: Record<ExamStatus, string> = {
    Draft: 'gold', Published: 'green', Archived: 'default',
}
/** Màu hex cho biểu đồ tròn (DashboardPage). */
export const EXAM_STATUS_PIE_COLOR: Record<ExamStatus, string> = {
    Draft: '#FAAD14', Published: '#52C41A', Archived: '#BFBFBF',
}

/* ─── ExamSession (draft | published | closed) ─── */
export const SESSION_STATUS_LABEL: Record<ExamSessionStatus, string> = {
    draft: 'Nháp', published: 'Đã phát hành', closed: 'Đã đóng',
}
export const SESSION_STATUS_VARIANT: Record<ExamSessionStatus, StatusVariant> = {
    draft: 'warning', published: 'success', closed: 'default',
}
export const PICK_MODE_LABEL: Record<ExamSessionPickMode, string> = {
    Random: 'Ngẫu nhiên', StudentChoice: 'HS tự chọn',
}

/* ─── Submission (InProgress | Submitted | Graded) ─── */
/** Nhãn cho màn admin/GV. */
export const SUBMISSION_STATUS_LABEL: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Chờ chấm', Graded: 'Đã chấm',
}
/** Nhãn cho màn học sinh (diễn đạt theo góc nhìn HS). */
export const SUBMISSION_STATUS_LABEL_STUDENT: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Đã nộp (chờ chấm)', Graded: 'Đã chấm',
}
export const SUBMISSION_STATUS_VARIANT: Record<SubmissionStatus, StatusVariant> = {
    InProgress: 'default', Submitted: 'warning', Graded: 'success',
}
/** Màu cho AntD <Tag> (SessionResultsModal). */
export const SUBMISSION_STATUS_TAG_COLOR: Record<SubmissionStatus, string> = {
    InProgress: 'default', Submitted: 'gold', Graded: 'green',
}

/* ─── Dùng chung ─── */
export const OPTION_LETTER = ['A', 'B', 'C', 'D', 'E', 'F']

/* ─── Chip câu hỏi ─── */
export type ChipColor = {bg: string; fg: string}

export const NEUTRAL_CHIP: ChipColor = {bg: '#eef0f3', fg: '#6f7788'}

/** Màu chip theo mức Bloom (khoá = cognitive code). */
export const BLOOM_CHIP: Record<string, ChipColor> = {
    remember:   {bg: '#e7f7ef', fg: '#1ea375'},
    understand: {bg: '#eef1ff', fg: '#3a74f5'},
    apply:      {bg: '#fff4e5', fg: '#d98a00'},
    analyze:    {bg: '#f3ecfe', fg: '#8b5cf6'},
    evaluate:   {bg: '#fee5e5', fg: '#e74242'},
    create:     {bg: '#e6f6f6', fg: '#0ea5a5'},
}
/** Số thứ tự mức Bloom. */
export const BLOOM_NUM: Record<string, number> = {
    remember: 1, understand: 2, apply: 3, analyze: 4, evaluate: 5, create: 6,
}
export const DIFF_CHIP: Record<string, ChipColor> = {
    easy:      {bg: '#dff5ed', fg: '#1ea375'},
    medium:    {bg: '#fff4e5', fg: '#d98a00'},
    hard:      {bg: '#fee5e5', fg: '#e74242'},
    very_hard: {bg: '#fdd9d9', fg: '#c62828'},
}
export const TYPE_CHIP: Record<string, ChipColor> = {
    multiple_choice: {bg: '#eef1ff', fg: '#3a74f5'},
    multiple_select: {bg: '#eef1ff', fg: '#3a74f5'},
    true_false:      {bg: '#e6f6f6', fg: '#0ea5a5'},
    fill_blank:      {bg: '#fdeef4', fg: '#db2777'},
    essay:           {bg: '#eceafe', fg: '#6d5bd0'},
    matching:        {bg: '#fff4e5', fg: '#d98a00'},
}

/* ─── Role ─── */
export const ROLE_LABEL: Record<string, string> = {
    Admin: 'Quản trị viên', Teacher: 'Giáo viên', Student: 'Học sinh',
}
export const ROLE_COLOR: Record<string, string> = {
    Admin: 'red', Teacher: 'blue', Student: 'green',
}

/* ─── Pagination mặc định ─── */
export const DEFAULT_PAGE = 1
export const DEFAULT_PAGE_SIZE = 20
