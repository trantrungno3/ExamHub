import {AuthHttp} from './requestService'

class CohortClassTeacherService {
    /** Danh sách phân công của một lớp */
    getByClass(cohortClassId: number) {
        return AuthHttp.get<CohortClassTeacher[]>(`/cohort-class-teachers/by-class/${cohortClassId}`)
    }

    /** Danh sách Id GV hợp lệ (thành viên trường + dạy đúng môn) */
    getEligibleTeachers(cohortClassId: number, subjectId: number) {
        return AuthHttp.get<string[]>(`/cohort-class-teachers/eligible-teachers`, {cohortClassId, subjectId})
    }

    /** Phân công GV dạy môn cho lớp */
    assign(body: AssignTeacherBody) {
        return AuthHttp.post<CohortClassTeacher>(`/cohort-class-teachers/assign`, body)
    }

    /** Xoá một phân công theo id */
    remove(id: number) {
        return AuthHttp.delete<boolean>(`/cohort-class-teachers/remove/${id}`)
    }
}

export const cohortClassTeacherService = new CohortClassTeacherService()
