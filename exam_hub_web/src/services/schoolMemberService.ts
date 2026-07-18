import {AuthHttp} from './requestService'

class SchoolMemberService {
    getById(id: string) {
        return AuthHttp.get<SchoolMember>(`/schoolmember/${id}`)
    }

    getBySchool(schoolId: number) {
        return AuthHttp.get<SchoolMember[]>(`/schoolmember/by-school/${schoolId}`)
    }

    getBySchoolAndRole(schoolId: number, role: string) {
        return AuthHttp.get<SchoolMember[]>(`/schoolmember/by-school/${schoolId}/role/${encodeURIComponent(role)}`)
    }

    getByUser(userId: string) {
        return AuthHttp.get<SchoolMember[]>(`/schoolmember/by-user/${userId}`)
    }

    add(body: SchoolMemberBody) {
        return AuthHttp.post<SchoolMember>(`/schoolmember`, body)
    }

    update(id: string, body: SchoolMemberBody) {
        return AuthHttp.put<SchoolMember>(`/schoolmember/${id}`, body)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/schoolmember/${id}`)
    }

    setActive(id: string, isActive: boolean) {
        return AuthHttp.patch<boolean>(`/schoolmember/${id}/active`, isActive)
    }
}

export const schoolMemberService = new SchoolMemberService()
