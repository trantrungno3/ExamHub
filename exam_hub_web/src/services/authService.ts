import {AuthHttp, Http} from './requestService'

export const authService = {
    login: (values: LoginFormValues) => Http.post('/Auth/login', values),
    register: (values: RegisterFormValues) => Http.post('/Auth/register', values),
    refresh: (tokens: TokenPair) => Http.post<TokenPair>('/Auth/refresh-token', tokens),

    getInfo: () => AuthHttp.get<UserInfo>('/Auth/info'),
    updateProfile: (body: UpdateProfileBody) => AuthHttp.put<UserInfo>('/Auth/profile', body),
    changePassword: (body: ChangePasswordBody) => AuthHttp.post<boolean>('/Auth/change-password', body),
}
