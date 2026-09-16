export type StudentSessionAction =
    | {kind: 'unavailable'; label: 'Chưa mở' | 'Đã đóng'}
    | {kind: 'results'}
    | {kind: 'resume'}
    | {kind: 'pick'}
    | {kind: 'start'}

export function getStudentSessionAction(s: MySession): StudentSessionAction {
    if (s.availability !== 'open') {
        if (s.usedAttempts > 0) return {kind: 'results'}
        return {
            kind: 'unavailable',
            label: s.availability === 'upcoming' ? 'Chưa mở' : 'Đã đóng',
        }
    }

    if (s.inProgressSubmissionId && s.inProgressExamId) return {kind: 'resume'}
    if (s.maxAttempts - s.usedAttempts <= 0) return {kind: 'results'}
    return s.pickMode === 'StudentChoice' ? {kind: 'pick'} : {kind: 'start'}
}
