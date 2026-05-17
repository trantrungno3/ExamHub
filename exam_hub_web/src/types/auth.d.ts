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

interface TokenModel {
    accessToken: string
    refreshToken: string
    expiresAt: number
    refreshExpiresAt: number
}

interface UserInfo {
    userName: string
    displayName?: string
    phoneNumber?: string
    roles: string[]
}

