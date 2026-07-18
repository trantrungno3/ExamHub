/* ─── Profile (self-service) types ── */

interface UpdateProfileBody {
    displayName: string
    email?: string
    phoneNumber?: string
}

interface ChangePasswordBody {
    oldPassword: string
    newPassword: string
}

interface TeacherSubject {
    id: number
    userId: string
    subjectId: number
    subject?: {
        id: number
        name: string
    }
}
