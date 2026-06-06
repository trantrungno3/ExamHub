/* ─── Exam Template types (mirrors ExamTemplateDto.cs) ─── */

interface ExamTemplateSection {
    id: string
    examTemplateId: string
    topicId?: number
    topicName?: string
    questionTypeId?: number
    questionTypeName?: string
    cognitiveLevelId?: number
    cognitiveLevelName?: string
    sectionName?: string
    questionCount: number
    scorePerQuestion?: number
    sortOrder: number
    pctEasy: number
    pctMedium: number
    pctHard: number
    pctVeryHard: number
}

interface ExamTemplateSectionBody {
    topicId?: number
    questionTypeId?: number
    cognitiveLevelId?: number
    sectionName?: string
    questionCount: number
    scorePerQuestion?: number
    pctEasy: number
    pctMedium: number
    pctHard: number
    pctVeryHard: number
}

interface ExamTemplate {
    id: string
    gradeLevelId: number
    gradeLevelName?: string
    subjectId: number
    subjectName?: string
    title: string
    description?: string
    durationMinutes: number
    totalQuestions?: number
    totalScore: number
    shuffleQuestions: boolean
    shuffleAnswers: boolean
    preventDuplicate: boolean
    instructions?: string
    isActive: boolean
    createdAt: number
    updatedAt: number
    sections?: ExamTemplateSection[]
}

interface ExamTemplateBody {
    gradeLevelId: number
    subjectId: number
    title: string
    description?: string
    durationMinutes: number
    totalQuestions?: number
    totalScore: number
    shuffleQuestions: boolean
    shuffleAnswers: boolean
    preventDuplicate: boolean
    instructions?: string
    isActive: boolean
    sections: ExamTemplateSectionBody[]
}
