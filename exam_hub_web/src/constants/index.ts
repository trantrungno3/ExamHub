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
