import {Http} from './requestService'

export const authService = {
    login: (values: LoginFormValues) => Http.post('/Auth/login', values),
    register: (values: RegisterFormValues) => Http.post('/Auth/register', values),
    refresh: (refreshToken: string) => Http.post<TokenModel>('/Auth/refresh', {refreshToken}),
}
