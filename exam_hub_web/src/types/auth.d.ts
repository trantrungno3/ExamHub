interface LoginFormValues {
    userName: string
    password: string
    isRemember: boolean
}

interface RegisterFormValues {
    username: string
    password: string
    email: string
    phoneNumber: string
    displayName: string
}

/**
 * Phần token duy nhất frontend được thấy. Refresh token nằm trong cookie HttpOnly
 * `examhub_refresh` nên JS không đọc/ghi được — đó là mục đích.
 */
interface AccessTokenResponse {
    accessToken: string
}

interface TokenModel extends AccessTokenResponse {
    expiresAt: number
}

interface UserInfo {
    id: string
    userName: string
    displayName?: string
    phoneNumber?: string
    email?: string
    roles: string[]
    schoolIds: number[]
    cohortClassIds: number[]
    subjectIds: number[]
}

