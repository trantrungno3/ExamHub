import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class QuestionTypeService extends CategoryServiceBase<QuestionType, QuestionTypeBody> {
    constructor() {
        super('questiontype')
    }

    getByCode(code: string) {
        return AuthHttp.get<QuestionType>(`/${this.basePath}/code/${code}`)
    }
}

export const questionTypeService = new QuestionTypeService()
