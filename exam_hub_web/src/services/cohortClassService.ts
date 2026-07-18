import {AuthHttp} from './requestService'

class CohortClassService {
    getById(id: number) {
        return AuthHttp.get<CohortClass>(`/cohortclass/${id}`)
    }

    getByCohort(cohortId: number) {
        return AuthHttp.get<CohortClass[]>(`/cohortclass/by-cohort/${cohortId}`)
    }

    getBySchoolYear(schoolYear: string) {
        return AuthHttp.get<CohortClass[]>(`/cohortclass/by-school-year/${encodeURIComponent(schoolYear)}`)
    }

    setHomeroomTeacher(id: number, body: SetHomeroomTeacherBody) {
        return AuthHttp.patch<boolean>(`/cohortclass/${id}/homeroom-teacher`, body)
    }
}

export const cohortClassService = new CohortClassService()
