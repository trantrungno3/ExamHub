import {AuthHttp, cleanParams} from './requestService'

class ExamTemplateService {
    private readonly basePath = 'exam-templates'

    getStats() {
        return AuthHttp.get<ExamTemplateStats>(`/${this.basePath}/stats`)
    }

    getById(id: string) {
        return AuthHttp.get<ExamTemplate>(`/${this.basePath}/${id}`)
    }

    getWithSections(id: string) {
        return AuthHttp.get<ExamTemplate>(`/${this.basePath}/${id}/with-sections`)
    }

    getList(filter: {subjectId?: number; gradeLevelId?: number} = {}) {
        return AuthHttp.get<ExamTemplate[]>(`/${this.basePath}`, cleanParams({...filter}))
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
