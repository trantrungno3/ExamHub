interface ApiResponse<T> {
    status: resStatus
    message: string
    data?: T
}

enum resStatus {
    error = 0,
    success = 1,
    created = 2,
    updated = 3,
    deleted = 4,
    notFound = 5,
}