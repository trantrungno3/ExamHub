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
}

interface ExamSubmissionBody {
    examId: string
    studentId: string
    answers: SubmissionAnswerBody[]
}

interface GradeAnswerBody {
    scoreEarned: number
    isCorrect: boolean
    feedback?: string
    gradedBy: string
}
