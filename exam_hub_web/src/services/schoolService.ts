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

    /** School là một trong hai danh mục duy nhất có route xoá bắt buộc (`DELETE {id}/force`). */
    override remove(id: number, force = false) {
        return force
            ? AuthHttp.delete<void>(`/${this.basePath}/${id}/force`)
            : super.remove(id)
    }
}

export const schoolService = new SchoolService()
