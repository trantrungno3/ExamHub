import {AuthHttp, cleanParams} from './requestService'

class QuestionService {
    private readonly basePath = 'questions'

    getPaged(query: QuestionPagedQuery = {}) {
        return AuthHttp.get<PagedResult<Question>>(`/${this.basePath}`, cleanParams({...query}))
    }

    getById(id: string) {
        return AuthHttp.get<Question>(`/${this.basePath}/${id}`)
    }

    getByTopic(topicId: number) {
        return AuthHttp.get<Question[]>(`/${this.basePath}/by-topic/${topicId}`)
    }

    create(body: QuestionBody) {
        return AuthHttp.post<Question>(`/${this.basePath}`, body)
    }

    update(id: string, body: QuestionBody) {
        return AuthHttp.put<Question>(`/${this.basePath}/${id}`, body)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/${this.basePath}/${id}`)
    }

    verify(id: string) {
        return AuthHttp.post<void>(`/${this.basePath}/${id}/verify`)
    }

    unverify(id: string) {
        return AuthHttp.post<void>(`/${this.basePath}/${id}/unverify`)
    }

    /** Thống kê số câu hỏi theo trạng thái (stat card). */
    getStats() {
        return AuthHttp.get<QuestionStats>(`/${this.basePath}/stats`)
    }

    /** Upload ảnh/PDF (≤ 10 MB) cho câu hỏi → trả về URL MinIO. */
    uploadAttachment(id: string, file: File) {
        const form = new FormData()
        form.append('file', file)
        return AuthHttp.postForm<{url: string}>(`/${this.basePath}/${id}/attachment`, form)
    }

    /** Import hàng loạt từ file .xlsx. */
    bulkImport(args: BulkImportArgs) {
        const form = new FormData()
        form.append('file', args.file)
        form.append('defaultTopicId', String(args.defaultTopicId))
        form.append('defaultDifficultyLevelId', String(args.defaultDifficultyLevelId))
        if (args.defaultCognitiveLevelId != null)
            form.append('defaultCognitiveLevelId', String(args.defaultCognitiveLevelId))
        return AuthHttp.postForm<BulkImportResult>(`/${this.basePath}/bulk-import`, form)
    }
}

export const questionService = new QuestionService()
