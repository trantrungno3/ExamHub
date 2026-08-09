/* ─── Question Bank types (mirrors QuestionDto.cs) ─── */

interface QuestionAnswer {
    id: string
    content: string
    contentPlain?: string
    isCorrect: boolean
    sortOrder: number
    explanation?: string
}

interface QuestionAnswerBody {
    content: string
    contentPlain?: string
    isCorrect: boolean
    explanation?: string
}

interface Question {
    id: string
    topicId: number
    topicName?: string
    questionTypeId: number
    questionTypeName?: string
    difficultyLevelId: number
    difficultyLevelName?: string
    cognitiveLevelId?: number
    cognitiveLevelName?: string
    content: string
    contentPlain?: string
    explanation?: string
    imageUrl?: string
    audioUrl?: string
    source?: string
    tags: string[]
    usageCount: number
    isActive: boolean
    isVerified: boolean
    rejectionReason?: string | null
    createdAt: number
    updatedAt: number
    answers?: QuestionAnswer[]
}

interface QuestionBody {
    topicId: number
    questionTypeId: number
    difficultyLevelId: number
    cognitiveLevelId?: number
    content: string
    contentPlain?: string
    explanation?: string
    imageUrl?: string
    audioUrl?: string
    source?: string
    tags?: string[]
    isActive?: boolean
    isVerified?: boolean
    answers: QuestionAnswerBody[]
}

interface QuestionPagedQuery {
    page?: number
    pageSize?: number
    topicId?: number
    questionTypeId?: number
    difficultyLevelId?: number
    cognitiveLevelId?: number
    keyword?: string
    isVerified?: boolean
    reviewStatus?: string
}

/* Thống kê ngân hàng câu hỏi (mirrors QuestionStatsResponse.cs) */
interface QuestionStats {
    total: number
    verified: number
    pending: number
    rejected: number
    inactive: number
}

/* Bulk import (mirrors BulkImportQuestionRequest.cs) */
interface BulkImportRowError {
    rowNumber: number
    message: string
}

interface BulkImportResult {
    successCount: number
    errorCount: number
    errors: BulkImportRowError[]
}

interface BulkImportArgs {
    file: File
    defaultTopicId: number
    defaultDifficultyLevelId: number
    defaultCognitiveLevelId?: number
}
