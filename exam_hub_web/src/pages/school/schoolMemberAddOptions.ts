/** Tài khoản còn có thể thêm vào trường với vai trò đã chọn.
 *  Thành viên đang hoạt động và ngừng hoạt động đều tính là đã tồn tại. */
export function eligibleSchoolUsers(
    role: SchoolMembershipRole,
    users: UserResponse[],
    members: SchoolMember[],
    students: CohortMember[],
) {
    const existing = new Set(role === 'Student'
        ? students.map(x => x.studentId)
        : members.map(x => x.userId))
    return users.filter(user =>
        !user.isDeleted &&
        user.roles.some(value => value.toLowerCase() === role.toLowerCase()) &&
        !existing.has(user.id))
}

/** Dải lớp hợp lệ của khoá: A, B, C… theo numClasses. */
export function sectionsForCohort(cohort?: Pick<Cohort, 'numClasses'>): string[] {
    return cohort
        ? Array.from({length: cohort.numClasses}, (_, index) => String.fromCharCode(65 + index))
        : []
}
