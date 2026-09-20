/**
 * Trộn hai nguồn thành viên của một trường thành một danh sách: `school_members` (Admin/Teacher) và
 * `cohort_members` (học sinh). Hai nguồn rời nhau — BE chỉ ghi học sinh vào `cohort_members` — nên
 * không có dòng trùng, nhưng id CÓ THỂ trùng giữa hai bảng nên `key` phải kèm tiền tố.
 */

export type MemberKind = 'staff' | 'student'

export type MemberRow = {
    key: string
    kind: MemberKind
    /** Id bản ghi gốc — dùng cho mutation bật/tắt, xoá thành viên. */
    memberId: string
    userId: string
    displayName: string
    email: string
    /** Giá trị thô như BE trả ('Admin' | 'Teacher' | 'Student') — nhãn tiếng Việt do ROLE_LABEL lo. */
    role: string
    /** Chỉ có với học sinh: 'K24 / A' | 'K24 / Chưa xếp' | '#<cohortId>' khi không tra được khoá. */
    cohortLabel?: string
    isActive: boolean
}

export function buildMemberRows(
    members: SchoolMember[],
    students: CohortMember[],
    users: UserResponse[],
    cohorts: Cohort[],
): MemberRow[] {
    // Map thay cho Array.find trong render: bảng có thể vài trăm dòng × vài nghìn user.
    const userById = new Map(users.map(u => [u.id, u]))
    const cohortById = new Map(cohorts.map(c => [c.id, c]))

    const identity = (userId: string) => {
        const u = userById.get(userId)
        return {
            userId,
            displayName: u?.displayName || u?.userName || userId,
            email: u?.email || '—',
        }
    }

    return [
        ...members.map((m): MemberRow => ({
            key: `staff:${m.id}`,
            kind: 'staff',
            memberId: m.id,
            role: m.role,
            isActive: m.isActive,
            ...identity(m.userId),
        })),
        ...students.map((s): MemberRow => {
            const cohortName = cohortById.get(s.cohortId)?.name
            return {
                key: `student:${s.id}`,
                kind: 'student',
                memberId: s.id,
                role: 'Student',
                cohortLabel: cohortName
                    ? `${cohortName} / ${s.section ?? 'Chưa xếp'}`
                    : `#${s.cohortId}`,
                isActive: s.isActive,
                ...identity(s.studentId),
            }
        }),
    ]
}

export function filterMemberRows(
    rows: MemberRow[],
    kind: 'all' | MemberKind,
    keyword: string,
): MemberRow[] {
    const q = keyword.trim().toLowerCase()
    return rows.filter(r =>
        (kind === 'all' || r.kind === kind) &&
        (q === '' || r.displayName.toLowerCase().includes(q) || r.email.toLowerCase().includes(q)))
}
