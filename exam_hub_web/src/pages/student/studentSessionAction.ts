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

/**
 * URL vào bài thi. `resuming` = học sinh đã bắt đầu lượt này từ trước → vào thẳng phần làm bài,
 * bỏ qua trang thông tin chung (họ đã đọc và tích đồng ý rồi).
 */
export function takeUrl(r: StartSessionResult, sessionId: string, resuming: boolean): string {
    const p = new URLSearchParams({
        examId: r.examId,
        sessionId,
        submissionId: r.submissionId,
        deadlineAt: String(r.deadlineAt),
        durationMinutes: String(r.durationMinutes),
    })
    return `/student/exam${resuming ? '/take' : ''}?${p.toString()}`
}
