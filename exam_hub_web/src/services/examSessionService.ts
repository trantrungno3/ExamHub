import {AuthHttp, cleanParams} from './requestService'

class ExamSessionService {
    private readonly basePath = 'exam-sessions'

    // ── Quản lý ─────────────────────────────────────────────────────────
    list(query: ExamSessionPagedQuery = {}) {
        return AuthHttp.get<PagedResult<ExamSession>>(`/${this.basePath}`, cleanParams({...query}))
    }

    getDetail(id: string) {
        return AuthHttp.get<ExamSessionDetail>(`/${this.basePath}/${id}`)
    }

    create(body: ExamSessionBody) {
        return AuthHttp.post<string>(`/${this.basePath}`, body)
    }

    update(id: string, body: ExamSessionBody) {
        return AuthHttp.put<boolean>(`/${this.basePath}/${id}`, body)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/${this.basePath}/${id}`)
    }

    setExams(id: string, examIds: string[]) {
        return AuthHttp.post<boolean>(`/${this.basePath}/${id}/exams`, {examIds})
    }

    removeExam(id: string, examId: string) {
        return AuthHttp.delete<boolean>(`/${this.basePath}/${id}/exams/${examId}`)
    }

    addAssignment(id: string, body: CreateAssignmentBody) {
        return AuthHttp.post<string>(`/${this.basePath}/${id}/assignments`, body)
    }

    removeAssignment(id: string, assignmentId: string) {
        return AuthHttp.delete<boolean>(`/${this.basePath}/${id}/assignments/${assignmentId}`)
    }

    publish(id: string) {
        return AuthHttp.post<boolean>(`/${this.basePath}/${id}/publish`)
    }

    close(id: string) {
        return AuthHttp.post<boolean>(`/${this.basePath}/${id}/close`)
    }

    // ── Phía học sinh ───────────────────────────────────────────────────
    getMy() {
        return AuthHttp.get<MySession[]>(`/${this.basePath}/my`)
    }

    getPool(id: string) {
        return AuthHttp.get<SessionPoolItem[]>(`/${this.basePath}/${id}/pool`)
    }

    start(id: string, examId?: string) {
        return AuthHttp.post<StartSessionResult>(`/${this.basePath}/${id}/start`, {examId: examId ?? null})
    }
}

export const examSessionService = new ExamSessionService()
