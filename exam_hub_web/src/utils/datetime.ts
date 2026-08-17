import dayjs, {type Dayjs} from 'dayjs'

/**
 * Backend trả về một số DateTime dạng Unix timestamp GIÂY (qua `ToTimestamp()`),
 * ví dụ `ExamSubmission.submittedAt/startedAt/createdAt`. Dùng `dayjs.unix()` để
 * chuyển sang datetime — KHÔNG dùng `new Date(ts)` vì nó hiểu là mili-giây → ra 1970.
 *
 * Lưu ý: các timestamp mili-giây (ví dụ `ExamSession.openAt/closeAt` serialize bằng
 * `ToUnixTimeMilliseconds()`) thì dùng `dayjs(ms)` bình thường, không qua hàm này.
 */
export function fromTimestamp(ts?: number | null): Dayjs | null {
    if (ts == null) return null
    return dayjs.unix(ts)
}

/** Format timestamp (giây) → chuỗi ngày giờ. Mặc định `HH:mm DD/MM/YYYY`. */
export function formatTimestamp(ts?: number | null, pattern = 'HH:mm DD/MM/YYYY'): string {
    const d = fromTimestamp(ts)
    return d ? d.format(pattern) : '—'
}
