interface ApiResponse<T> {
    status: resStatus
    message: string
    data?: T
}

/** Kết quả phân trang trả về trong `data` của ApiResponse. */
interface PagedResult<T> {
    total: number
    page: number
    pageSize: number
    items: T[]
}

enum resStatus {
    error = 0,
    success = 1,
    created = 2,
    updated = 3,
    deleted = 4,
    notFound = 5,
}