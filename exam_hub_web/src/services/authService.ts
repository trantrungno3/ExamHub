import {AuthHttp, Http} from './requestService'

export const authService = {
    login: (values: LoginFormValues) => Http.post<AccessTokenResponse>('/Auth/login', values),
    register: (values: RegisterFormValues) => Http.post('/Auth/register', values),
    // Refresh token đi bằng cookie HttpOnly; body chỉ mang access token để server lấy danh tính.
    refresh: (accessToken: string) =>
        Http.post<AccessTokenResponse>('/Auth/refresh-token', {accessToken}),
    logout: () => AuthHttp.post<boolean>('/Auth/logout'),

    getInfo: () => AuthHttp.get<UserInfo>('/Auth/info'),
    updateProfile: (body: UpdateProfileBody) => AuthHttp.put<UserInfo>('/Auth/profile', body),
    changePassword: (body: ChangePasswordBody) => AuthHttp.post<boolean>('/Auth/change-password', body),
}
