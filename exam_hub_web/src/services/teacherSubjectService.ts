import {AuthHttp} from './requestService'

class TeacherSubjectService {
    getByTeacher(userId: string) {
        return AuthHttp.get<TeacherSubject[]>(`/teacher-subjects/teacher/${userId}`)
    }
}

export const teacherSubjectService = new TeacherSubjectService()
