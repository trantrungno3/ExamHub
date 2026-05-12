import {Http} from "./requestService.ts";

async function login(values: LoginFormValues) {
    return await Http.post('/Auth/login', values);
}

async function register(values: RegisterFormValues) {
    return await Http.post('/Auth/register', values);
}


export const authService = {
    login,
    register,
}