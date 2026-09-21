/** Một câu được coi là đã trả lời khi có id đáp án, hoặc bài tự luận có nội dung. */
export const hasAnswer = (v: unknown): boolean =>
    typeof v === 'string' ? v.trim().length > 0 : v != null

/** Tập id câu hỏi đã có đáp án, lấy từ giá trị Form. */
export const answeredIds = (values: Record<string, unknown>): Set<string> => {
    const ids = new Set<string>()
    for (const [id, v] of Object.entries(values)) {
        if (hasAnswer(v)) ids.add(id)
    }
    return ids
}

/** So sánh nông hai tập — dùng để bỏ qua setState khi tập không đổi. */
export const sameSet = (a: Set<string>, b: Set<string>): boolean => {
    if (a.size !== b.size) return false
    for (const v of a) if (!b.has(v)) return false
    return true
}
