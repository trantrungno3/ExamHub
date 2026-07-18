import {AuthHttp} from './requestService'

class MenuService {
    getMenu() {
        return AuthHttp.get<MenuItem[]>('/menu')
    }
}

export const menuService = new MenuService()
