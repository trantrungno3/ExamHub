import {AuthHttp} from './requestService'

class SubmissionService {
    private readonly basePath = 'exam-submissions'

    getById(id: string) {
        return AuthHttp.get<ExamSubmission>(`/${this.basePath}/${id}`)
    }

    getByExam(examId: string) {
        return AuthHttp.get<ExamSubmission[]>(`/${this.basePath}/by-exam/${examId}`)
    }

    getByStudent(studentId: string) {
        return AuthHttp.get<ExamSubmission[]>(`/${this.basePath}/by-student/${studentId}`)
    }

    getByExamAndStudent(examId: string, studentId: string) {
        return AuthHttp.get<ExamSubmission>(`/${this.basePath}/by-exam/${examId}/student/${studentId}`)
    }

    /** Học sinh nộp bài kèm câu trả lời (chấm trắc nghiệm tự động ngay lúc nộp). */
    submit(body: ExamSubmissionBody) {
        return AuthHttp.post<ExamSubmission>(`/${this.basePath}`, body)
    }

    /** Giáo viên chấm điểm một câu tự luận. */
    gradeAnswer(answerId: string, body: GradeAnswerBody) {
        return AuthHttp.post<void>(`/${this.basePath}/answers/${answerId}/grade`, body)
    }

    /** Giáo viên chốt điểm: tổng hợp điểm từng câu, chuyển trạng thái → Graded. */
    finalize(id: string) {
        return AuthHttp.post<ExamSubmission>(`/${this.basePath}/${id}/finalize`)
    }
}

export const submissionService = new SubmissionService()
