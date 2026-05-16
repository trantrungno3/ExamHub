/* ─── Category entity types (matches API JSON serialization) ── */

interface GradeLevel {
    id: number
    name: string
    gradeNumber: number
    description?: string
    isActive: boolean
    createdAt: string
    updatedAt: string
    subjects?: Subject[]
}

interface GradeLevelBody {
    name: string
    gradeNumber: number
    description?: string
    isActive?: boolean
}

interface Subject {
    id: number
    gradeLevelId: number
    name: string
    code: string
    description?: string
    isActive: boolean
    createdAt: string
    updatedAt: string
    gradeLevel?: GradeLevel
    topics?: Topic[]
}

interface SubjectBody {
    gradeLevelId: number
    name: string
    code: string
    description?: string
    isActive?: boolean
}

interface Topic {
    id: number
    subjectId: number
    parentId?: number
    name: string
    code?: string
    sortOrder: number
    description?: string
    isActive: boolean
    createdAt: string
    updatedAt: string
    subject?: Subject
    parent?: Topic
    children?: Topic[]
}

interface TopicBody {
    subjectId: number
    parentId?: number
    name: string
    code?: string
    sortOrder?: number
    description?: string
    isActive?: boolean
}

interface DifficultyLevel {
    id: number
    code: string
    name: string
    scoreWeight: number
    sortOrder: number
    isActive: boolean
}

interface DifficultyLevelBody {
    code: string
    name: string
    scoreWeight: number
    sortOrder?: number
    isActive?: boolean
}

interface QuestionType {
    id: number
    code: string
    name: string
    description?: string
    isActive: boolean
}

interface QuestionTypeBody {
    code: string
    name: string
    description?: string
    isActive?: boolean
}

interface CognitiveLevel {
    id: number
    code: string
    name: string
    nameEn: string
    levelOrder: number
    description?: string
    colorCode?: string
    isActive: boolean
}

interface CognitiveLevelBody {
    code: string
    name: string
    nameEn: string
    levelOrder: number
    description?: string
    colorCode?: string
    isActive?: boolean
}
