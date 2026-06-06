/** Tiện ích đọc answers_snapshot (JSONB) của ExamQuestion. */

export type SnapshotAnswer = {
    id: string
    content: string
    isCorrect: boolean
    sortOrder: number
}

/** Bỏ thẻ HTML, gộp khoảng trắng — render plaintext. */
export function stripHtml(html?: string): string {
    if (!html) return ''
    return html.replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim()
}

/** Parse answers_snapshot JSON [{id, content, is_correct, sort_order}] → mảng đã sắp xếp. */
export function parseAnswers(json?: string): SnapshotAnswer[] {
    if (!json) return []
    try {
        const arr = JSON.parse(json) as Array<{
            id?: string; content?: string; is_correct?: boolean; sort_order?: number
        }>
        return arr
            .map(a => ({
                id: a.id ?? '',
                content: a.content ?? '',
                isCorrect: !!a.is_correct,
                sortOrder: a.sort_order ?? 0,
            }))
            .sort((x, y) => x.sortOrder - y.sortOrder)
    } catch {
        return []
    }
}
