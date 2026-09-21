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

    /** Cohort là một trong hai danh mục duy nhất có route xoá bắt buộc (`DELETE {id}/force`). */
    override remove(id: number, force = false) {
        return force
            ? AuthHttp.delete<void>(`/${this.basePath}/${id}/force`)
            : super.remove(id)
    }
}

export const cohortService = new CohortService()
