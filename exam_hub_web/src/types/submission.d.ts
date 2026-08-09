/* ─── Exam Submission types (mirrors ExamSubmissionDto.cs) ─── */

type SubmissionStatus = 'InProgress' | 'Submitted' | 'Graded'

interface SubmissionAnswer {
    id: string
    examQuestionId: string
    selectedAnswerIds?: string[]
    essayContent?: string
    isCorrect?: boolean
    scoreEarned: number
    feedback?: string
    gradedBy?: string
}

interface SubmissionAnswerBody {
    examQuestionId: string
    selectedAnswerIds?: string[]
    essayContent?: string
}

interface ExamSubmission {
    id: string
    examId: string
    studentId: string
    startedAt: number
    submittedAt?: number
    durationSeconds?: number
    totalScore?: number
    isPassed?: boolean
    status: SubmissionStatus
    createdAt: number
    answers?: SubmissionAnswer[]
    /** Tên hiển thị của học sinh — chỉ enrich ở danh sách theo kỳ thi (by-session). */
    studentName?: string
    /** Tên lớp của học sinh — chỉ enrich ở danh sách theo kỳ thi (by-session). */
    studentClassName?: string
}

interface ExamSubmissionBody {
    examId: string
    studentId: string
    answers: SubmissionAnswerBody[]
    /** Kỳ thi (nếu nộp bài trong luồng kỳ thi). */
    sessionId?: string
    /** Bản in_progress đã bốc/khoá đề — để BE cập nhật thay vì tạo mới. */
    submissionId?: string
}

interface GradeAnswerBody {
    scoreEarned: number
    isCorrect: boolean
    feedback?: string
    gradedBy: string
}
