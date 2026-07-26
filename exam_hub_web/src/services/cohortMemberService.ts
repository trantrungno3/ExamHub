import {AuthHttp} from './requestService'

class CohortMemberService {
    getById(id: string) {
        return AuthHttp.get<CohortMember>(`/cohortmember/${id}`)
    }

    getByCohort(cohortId: number) {
        return AuthHttp.get<CohortMember[]>(`/cohortmember/by-cohort/${cohortId}`)
    }

    getByStudent(studentId: string) {
        return AuthHttp.get<CohortMember[]>(`/cohortmember/by-student/${studentId}`)
    }

    add(body: CohortMemberBody) {
        return AuthHttp.post<CohortMember>(`/cohortmember`, body)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/cohortmember/${id}`)
    }

    setActive(id: string, isActive: boolean) {
        return AuthHttp.patch<boolean>(`/cohortmember/${id}/active`, isActive)
    }

    setSection(id: string, section: string | null) {
        return AuthHttp.patch<boolean>(`/cohortmember/${id}/section`, section)
    }
}

export const cohortMemberService = new CohortMemberService()
