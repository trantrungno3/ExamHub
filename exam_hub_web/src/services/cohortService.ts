import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class CohortService extends CategoryServiceBase<Cohort, CohortBody> {
    constructor() {
        super('cohort')
    }

    getBySchool(schoolId: number) {
        return AuthHttp.get<Cohort[]>(`/${this.basePath}/by-school/${schoolId}`)
    }

    getWithClasses(id: number) {
        return AuthHttp.get<Cohort>(`/${this.basePath}/${id}/with-classes`)
    }

    getWithMembers(id: number) {
        return AuthHttp.get<Cohort>(`/${this.basePath}/${id}/with-members`)
    }
}

export const cohortService = new CohortService()
