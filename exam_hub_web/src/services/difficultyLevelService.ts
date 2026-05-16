import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class DifficultyLevelService extends CategoryServiceBase<DifficultyLevel, DifficultyLevelBody> {
    constructor() {
        super('difficultylevel')
    }

    getByCode(code: string) {
        return AuthHttp.get<DifficultyLevel>(`/${this.basePath}/code/${code}`)
    }
}

export const difficultyLevelService = new DifficultyLevelService()
