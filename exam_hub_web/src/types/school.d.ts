/* ─── School entity types (matches API JSON serialization) ── */

interface School {
    id: number
    name: string
    code: string
    address?: string
    phone?: string
    email?: string
    isActive: boolean
    created: number
    modified: number
}

interface SchoolBody {
    name: string
    code: string
    address?: string
    phone?: string
    email?: string
    isActive?: boolean
}

/* ─── Cohort ── */

interface Cohort {
    id: number
    schoolId: number
    name: string
    startYear: number
    endYear: number
    gradeStart: number
    classSuffix: string
    isActive: boolean
    created: number
}

interface CohortBody {
    schoolId: number
    name: string
    startYear: number
    endYear: number
    gradeStart: number
    classSuffix?: string
    isActive?: boolean
}

/* ─── CohortClass (tự động tạo qua DB trigger, không có Create request) ── */

interface CohortClass {
    id: number
    cohortId: number
    gradeLevelId: number
    className: string
    schoolYear: string
    yearIndex: number
    homeroomTeacherId?: string
    created: number
}

interface SetHomeroomTeacherBody {
    teacherId: string | null
}

/* ─── SchoolMember ── */

interface SchoolMember {
    id: string
    schoolId: number
    userId: string
    role: string
    isActive: boolean
    joinedAt: number
}

interface SchoolMemberBody {
    schoolId: number
    userId: string
    role: string
    isActive?: boolean
}

/* ─── CohortMember ── */

interface CohortMember {
    id: string
    cohortId: number
    studentId: string
    joinedAt: number
    isActive: boolean
}

interface CohortMemberBody {
    cohortId: number
    studentId: string
    joinedAt?: number
    isActive?: boolean
}

/* ─── Menu ── */

interface MenuItem {
    key: string
    label: string
    /** Nhóm cha không có path (chỉ để thu/mở). */
    path?: string
    icon: string
    order: number
    children?: MenuItem[]
}
