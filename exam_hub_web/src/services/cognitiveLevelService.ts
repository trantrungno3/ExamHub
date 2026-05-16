import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class CognitiveLevelService extends CategoryServiceBase<CognitiveLevel, CognitiveLevelBody> {
    constructor() {
        super('cognitivelevel')
    }

    getByCode(code: string) {
        return AuthHttp.get<CognitiveLevel>(`/${this.basePath}/code/${code}`)
    }
}

export const cognitiveLevelService = new CognitiveLevelService()
