interface UserResponse {
    id: string
    userName: string | null
    displayName: string
    email: string | null
    phoneNumber: string | null
    sex: boolean
    avartar: string | null
    address: string | null
    description: string | null
    roles: string[]
    lockoutEnabled: boolean
    isDeleted: boolean
}

interface CreateUserRequest {
    userName: string
    password: string
    displayName: string
    email?: string
    phoneNumber?: string
    sex: boolean
}

interface UpdateUserRequest {
    displayName: string
    email?: string | null
    phoneNumber?: string | null
    sex: boolean
    avartar?: string | null
    address?: string | null
    description?: string | null
}

interface ResetPasswordRequest {
    newPassword: string
}

interface SetRolesRequest {
    roles: string[]
}
