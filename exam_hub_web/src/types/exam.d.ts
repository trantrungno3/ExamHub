/* ─── Exam + generation + analytics types (mirrors ExamDto.cs, GenerateExamApiRequest.cs,
   BatchGenerateExamApiRequest.cs, ExamAnalyticsResponse.cs) ─── */

type ExamStatus = 'Draft' | 'Published' | 'Archived'

interface ExamQuestionSnapshot {
    id: string
    questionId: string
    sectionName?: string
    sortOrder: number
    score?: number
    contentSnapshot: string
    /** JSON string: [{id, content, is_correct, sort_order, explanation}] */
    answersSnapshot?: string
}

interface Exam {
    id: string
    examTemplateId?: string
    gradeLevelId: number
    gradeLevelName?: string
    subjectId: number
    subjectName?: string
    title: string
    examCode?: string
    durationMinutes: number
    totalScore: number
    instructions?: string
    status: ExamStatus
    schoolYear?: string
    semester?: number
    examDate?: string
    className?: string
    parentExamId?: string
    variantIndex?: number
    batchId?: string
    createdAt: number
    updatedAt: number
    questions?: ExamQuestionSnapshot[]
}

interface ExamPagedQuery {
    page?: number
    pageSize?: number
    gradeLevelId?: number
    subjectId?: number
    status?: ExamStatus
    keyword?: string
}

type ExportFormat = 'pdf' | 'docx'

interface ExportResult {
    url: string
    format: string
}

/* ── Analytics ── */
interface DistributionItem {
    label: string
    count: number
    percentage: number
}

interface ExamAnalytics {
    examId: string
    totalQuestions: number
    bloomDistribution: DistributionItem[]
    difficultyDistribution: DistributionItem[]
    topicDistribution: DistributionItem[]
}

/* ── Generation ── */
interface SectionConfig {
    sectionName?: string
    topicId: number
    questionTypeId?: number
    cognitiveLevelId?: number
    questionCount: number
    pctEasy: number
    pctMedium: number
    pctHard: number
    pctVeryHard: number
    scorePerQuestion: number
}

interface GenerateExamBody {
    title: string
    examTemplateId?: string
    gradeLevelId: number
    subjectId: number
    durationMinutes: number
    shuffleQuestions: boolean
    sections: SectionConfig[]
}

interface GenerateExamResult {
    examId: string
}

type VariantNaming = 'ALPHA' | 'NUMBER'

interface BatchGenerateExamBody {
    title: string
    examTemplateId?: string
    gradeLevelId: number
    subjectId: number
    durationMinutes: number
    shuffleQuestions: boolean
    shuffleAnswers: boolean
    variantCount: number
    variantNaming: VariantNaming
    sections: SectionConfig[]
}

interface VariantSummary {
    examId: string
    examCode?: string
    variantIndex: number
    variantCode: string
}

interface BatchGenerateResult {
    batchId: string
    variants: VariantSummary[]
}
