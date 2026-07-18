import {AuthHttp, Http} from './requestService'

export const authService = {
    login: (values: LoginFormValues) => Http.post('/Auth/login', values),
    register: (values: RegisterFormValues) => Http.post('/Auth/register', values),
    refresh: (refreshToken: string) => Http.post<TokenModel>('/Auth/refresh', {refreshToken}),

    getInfo: () => AuthHttp.get<UserInfo>('/Auth/info'),
    updateProfile: (body: UpdateProfileBody) => AuthHttp.put<UserInfo>('/Auth/profile', body),
    changePassword: (body: ChangePasswordBody) => AuthHttp.post<boolean>('/Auth/change-password', body),
}
