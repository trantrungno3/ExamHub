/* ─── Exam Session (Kỳ thi) — mirrors ExamSessionDtos.cs ─── */

type ExamSessionPickMode = 'Random' | 'StudentChoice'
type ExamSessionStatus = 'draft' | 'published' | 'closed'
/** Trạng thái khả dụng của kỳ thi theo khung giờ (phía học sinh). */
type ExamSessionAvailability = 'upcoming' | 'open' | 'closed'
/** Trạng thái của học sinh với một đề trong pool. */
type SessionPoolItemState = 'notStarted' | 'inProgress' | 'completed'

/** Tóm tắt kỳ thi cho danh sách quản lý (ExamSessionResponse). */
interface ExamSession {
    id: string
    title: string
    subjectId: number
    subjectName?: string
    gradeLevelId: number
    gradeLevelName?: string
    /** epoch ms */
    openAt: number
    /** epoch ms */
    closeAt: number
    maxAttempts: number
    pickMode: ExamSessionPickMode
    status: ExamSessionStatus
    examCount: number
    assignmentCount: number
}

/** Một đề trong pool (SessionExamResponse). */
interface SessionExam {
    examId: string
    title: string
    examCode?: string
    totalScore: number
}

/** Giao lớp/khoá (AssignmentResponse). */
interface SessionAssignment {
    id: string
    cohortId?: number
    cohortName?: string
    cohortClassId?: number
    cohortClassName?: string
    schoolName?: string
}

/** Chi tiết kỳ thi (ExamSessionDetailResponse). */
interface ExamSessionDetail {
    id: string
    title: string
    description?: string
    subjectId: number
    subjectName?: string
    gradeLevelId: number
    gradeLevelName?: string
    openAt: number
    closeAt: number
    maxAttempts: number
    pickMode: ExamSessionPickMode
    status: ExamSessionStatus
    exams: SessionExam[]
    assignments: SessionAssignment[]
}

/** Body tạo/cập nhật kỳ thi (Create/UpdateExamSessionRequest). openAt/closeAt: ISO 8601. */
interface ExamSessionBody {
    title: string
    description?: string
    subjectId: number
    gradeLevelId: number
    openAt: string
    closeAt: string
    maxAttempts: number
    pickMode: ExamSessionPickMode
}

/** Body giao lớp/khoá (CreateAssignmentRequest). Chọn đúng 1 trong 2. */
interface CreateAssignmentBody {
    cohortId?: number
    cohortClassId?: number
}

/** Kỳ thi được giao — phía học sinh (MySessionResponse). */
interface MySession {
    id: string
    title: string
    subjectName?: string
    gradeLevelName?: string
    openAt: number
    closeAt: number
    pickMode: ExamSessionPickMode
    availability: ExamSessionAvailability
    maxAttempts: number
    usedAttempts: number
    inProgressSubmissionId?: string
    inProgressExamId?: string
}

/** Một đề trong pool + trạng thái học sinh (SessionPoolItemResponse). */
interface SessionPoolItem {
    examId: string
    title: string
    examCode?: string
    totalScore: number
    studentState: SessionPoolItemState
    submissionId?: string
}

/** Kết quả vào thi (StartSessionResponse). */
interface StartSessionResult {
    submissionId: string
    examId: string
}

interface ExamSessionPagedQuery {
    page?: number
    pageSize?: number
    subjectId?: number
    gradeLevelId?: number
    status?: ExamSessionStatus
    keyword?: string
}
