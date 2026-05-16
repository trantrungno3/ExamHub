import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class GradeLevelService extends CategoryServiceBase<GradeLevel, GradeLevelBody> {
    constructor() {
        super('gradelevel')
    }

    getWithSubjects(id: number) {
        return AuthHttp.get<GradeLevel>(`/${this.basePath}/${id}/with-subjects`)
    }
}

export const gradeLevelService = new GradeLevelService()
