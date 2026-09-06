import {AuthHttp} from './requestService'

class TeacherSubjectService {
    getByTeacher(userId: string) {
        return AuthHttp.get<TeacherSubject[]>(`/teacher-subjects/teacher/${userId}`)
    }

    assign(userId: string, subjectId: number) {
        return AuthHttp.post<void>('/teacher-subjects/assign', {userId, subjectId})
    }

    remove(userId: string, subjectId: number) {
        return AuthHttp.delete<void>('/teacher-subjects/remove', {userId, subjectId})
    }

    setSubjects(userId: string, subjectIds: number[]) {
        return AuthHttp.put<void>(`/teacher-subjects/teacher/${userId}/subjects`, {subjectIds})
    }
}

export const teacherSubjectService = new TeacherSubjectService()
