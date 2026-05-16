import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class SchoolService extends CategoryServiceBase<School, SchoolBody> {
    constructor() {
        super('school')
    }

    getByCode(code: string) {
        return AuthHttp.get<School>(`/${this.basePath}/code/${code}`)
    }

    getWithCohorts(id: number) {
        return AuthHttp.get<School>(`/${this.basePath}/${id}/with-cohorts`)
    }

    getWithMembers(id: number) {
        return AuthHttp.get<School>(`/${this.basePath}/${id}/with-members`)
    }
}

export const schoolService = new SchoolService()
