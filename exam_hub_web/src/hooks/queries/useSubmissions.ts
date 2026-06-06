import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {submissionService} from '../../services/submissionService'

export const SUBMISSION_KEYS = {
    all: ['submissions'] as const,
    detail: (id: string) => ['submissions', 'detail', id] as const,
    byExam: (examId: string) => ['submissions', 'byExam', examId] as const,
}

export function useSubmissionQuery(id?: string) {
    return useQuery({
        queryKey: SUBMISSION_KEYS.detail(id ?? ''),
        queryFn: async () => (await submissionService.getById(id!)).data ?? null,
        enabled: !!id,
    })
}

export function useSubmissionsByExamQuery(examId?: string) {
    return useQuery({
        queryKey: SUBMISSION_KEYS.byExam(examId ?? ''),
        queryFn: async () => (await submissionService.getByExam(examId!)).data ?? [],
        enabled: !!examId,
    })
}

export function useSubmitExamMutation() {
    return useMutation({
        mutationFn: (body: ExamSubmissionBody) => submissionService.submit(body),
    })
}

export function useGradeAnswerMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({answerId, body}: {answerId: string; body: GradeAnswerBody}) =>
            submissionService.gradeAnswer(answerId, body),
        onSuccess: () => {
            message.success('Đã chấm điểm câu trả lời')
            void qc.invalidateQueries({queryKey: SUBMISSION_KEYS.all})
        },
        onError: () => message.error('Chấm điểm thất bại'),
    })
}

export function useFinalizeSubmissionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => submissionService.finalize(id),
        onSuccess: (res) => {
            if (res.status === statusCode.Error || !res.data) {
                message.error(res.message || 'Chốt điểm thất bại')
                return
            }
            message.success('Đã chốt điểm bài nộp')
            void qc.invalidateQueries({queryKey: SUBMISSION_KEYS.all})
        },
        onError: () => message.error('Chốt điểm thất bại'),
    })
}
