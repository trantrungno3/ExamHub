import {AuthHttp} from './requestService'

const BASE = '/users'

export const AVAILABLE_ROLES = ['Admin', 'Teacher', 'Student']

export const userService = {
    getAll:        ()                                      => AuthHttp.get<UserResponse[]>(BASE),
    create:        (body: CreateUserRequest)               => AuthHttp.post<UserResponse>(BASE, body),
    update:        (id: string, body: UpdateUserRequest)   => AuthHttp.put<UserResponse>(`${BASE}/${id}`, body),
    remove:        (id: string)                            => AuthHttp.delete<void>(`${BASE}/${id}`),
    setLock:       (id: string, isLocked: boolean)         => AuthHttp.patch<boolean>(`${BASE}/${id}/lock`, isLocked),
    resetPassword: (id: string, body: ResetPasswordRequest) => AuthHttp.patch<boolean>(`${BASE}/${id}/reset-password`, body),
    setRoles:      (id: string, body: SetRolesRequest)     => AuthHttp.put<string[]>(`${BASE}/${id}/roles`, body),
    bulkImport:       (file: File, defaultPassword: string) => {
        const form = new FormData()
        form.append('file', file)
        form.append('defaultPassword', defaultPassword)
        return AuthHttp.postForm<BulkImportResult>(`${BASE}/bulk-import`, form)
    },
    downloadTemplate: ()                                    => AuthHttp.getBlob(`${BASE}/bulk-import/template`),
}
