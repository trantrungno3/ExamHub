import {AuthHttp, cleanParams} from './requestService'

class ExamService {
    private readonly basePath = 'exams'

    getPaged(query: ExamPagedQuery = {}) {
        return AuthHttp.get<PagedResult<Exam>>(`/${this.basePath}`, cleanParams({...query}))
    }

    getById(id: string) {
        return AuthHttp.get<Exam>(`/${this.basePath}/${id}`)
    }

    getWithQuestions(id: string) {
        return AuthHttp.get<Exam>(`/${this.basePath}/${id}/with-questions`)
    }

    getVariants(parentId: string) {
        return AuthHttp.get<Exam[]>(`/${this.basePath}/${parentId}/variants`)
    }

    publish(id: string) {
        return AuthHttp.post<boolean>(`/${this.basePath}/${id}/publish`)
    }

    archive(id: string) {
        return AuthHttp.post<boolean>(`/${this.basePath}/${id}/archive`)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/${this.basePath}/${id}`)
    }

    getAnalytics(id: string) {
        return AuthHttp.get<ExamAnalytics>(`/${this.basePath}/${id}/analytics`)
    }

    /** Xuất đề thi → trả về URL MinIO của file PDF/DOCX. */
    export(id: string, format: ExportFormat) {
        return AuthHttp.get<ExportResult>(`/${this.basePath}/${id}/export`, {format})
    }
}

export const examService = new ExamService()
