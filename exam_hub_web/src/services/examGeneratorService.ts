import {AuthHttp} from './requestService'

class ExamGeneratorService {
    private readonly basePath = 'exam-generator'

    /** Sinh một đề thi → trả về { examId }. */
    generate(body: GenerateExamBody) {
        return AuthHttp.post<GenerateExamResult>(`/${this.basePath}`, body)
    }

    /** Sinh lô nhiều biến thể → trả về { batchId, variants[] }. */
    batchGenerate(body: BatchGenerateExamBody) {
        return AuthHttp.post<BatchGenerateResult>(`/${this.basePath}/batch`, body)
    }
}

export const examGeneratorService = new ExamGeneratorService()
