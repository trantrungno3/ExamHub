import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {submissionService} from '../../services/submissionService'

export const SUBMISSION_KEYS = {
    all: ['submissions'] as const,
    detail: (id: string) => ['submissions', 'detail', id] as const,
    byExam: (examId: string) => ['submissions', 'byExam', examId] as const,
    byStudent: (studentId: string) => ['submissions', 'byStudent', studentId] as const,
    bySession: (sessionId: string, page: number, pageSize: number) =>
        ['submissions', 'bySession', sessionId, page, pageSize] as const,
    bySessionStudent: (sessionId: string, studentId: string) =>
        ['submissions', 'bySession', sessionId, 'student', studentId] as const,
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

const EMPTY_PAGE: Paged<ExamSubmission> = {total: 0, page: 1, pageSize: 20, items: []}

export function useSubmissionsBySessionQuery(sessionId?: string, page = 1, pageSize = 20) {
    return useQuery({
        queryKey: SUBMISSION_KEYS.bySession(sessionId ?? '', page, pageSize),
        queryFn: async () =>
            (await submissionService.getBySession(sessionId!, page, pageSize)).data ?? EMPTY_PAGE,
        enabled: !!sessionId,
        // Giữ trang cũ trong lúc tải trang mới để bảng không nhảy về rỗng.
        placeholderData: (previous) => previous,
    })
}

export function useMySessionSubmissionsQuery(sessionId?: string, studentId?: string) {
    return useQuery({
        queryKey: SUBMISSION_KEYS.bySessionStudent(sessionId ?? '', studentId ?? ''),
        queryFn: async () =>
            (await submissionService.getBySessionAndStudent(sessionId!, studentId!)).data ?? [],
        enabled: !!sessionId && !!studentId,
    })
}

export function useMySubmissionsQuery(studentId?: string) {
    return useQuery({
        queryKey: SUBMISSION_KEYS.byStudent(studentId ?? ''),
        queryFn: async () => (await submissionService.getByStudent(studentId!)).data ?? [],
        enabled: !!studentId,
    })
}

export function useSubmitExamMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: ExamSubmissionBody) => submissionService.submit(body),
        onSuccess: () => {
            void qc.invalidateQueries({queryKey: ['exam-sessions']})
            void qc.invalidateQueries({queryKey: SUBMISSION_KEYS.all})
        },
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
