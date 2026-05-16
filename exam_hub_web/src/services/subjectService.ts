import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class SubjectService extends CategoryServiceBase<Subject, SubjectBody> {
    constructor() {
        super('subject')
    }

    getByGradeLevel(gradeLevelId: number) {
        return AuthHttp.get<Subject[]>(`/${this.basePath}/by-grade/${gradeLevelId}`)
    }

    getWithTopics(id: number) {
        return AuthHttp.get<Subject>(`/${this.basePath}/${id}/with-topics`)
    }
}

export const subjectService = new SubjectService()
