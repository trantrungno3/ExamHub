import {AuthHttp} from './requestService'

class ExamTemplateService {
    private readonly basePath = 'exam-templates'

    getById(id: string) {
        return AuthHttp.get<ExamTemplate>(`/${this.basePath}/${id}`)
    }

    getWithSections(id: string) {
        return AuthHttp.get<ExamTemplate>(`/${this.basePath}/${id}/with-sections`)
    }

    getBySubject(subjectId: number) {
        return AuthHttp.get<ExamTemplate[]>(`/${this.basePath}/by-subject/${subjectId}`)
    }

    getByGrade(gradeLevelId: number) {
        return AuthHttp.get<ExamTemplate[]>(`/${this.basePath}/by-grade/${gradeLevelId}`)
    }

    create(body: ExamTemplateBody) {
        return AuthHttp.post<ExamTemplate>(`/${this.basePath}`, body)
    }

    update(id: string, body: ExamTemplateBody) {
        return AuthHttp.put<ExamTemplate>(`/${this.basePath}/${id}`, body)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/${this.basePath}/${id}`)
    }
}

export const examTemplateService = new ExamTemplateService()
