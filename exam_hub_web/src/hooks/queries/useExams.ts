import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {examService} from '../../services/examService'
import {examGeneratorService} from '../../services/examGeneratorService'

export const EXAM_KEYS = {
    all: ['exams'] as const,
    paged: (query: ExamPagedQuery) => ['exams', 'paged', query] as const,
    detail: (id: string) => ['exams', 'detail', id] as const,
    variants: (id: string) => ['exams', 'variants', id] as const,
    analytics: (id: string) => ['exams', 'analytics', id] as const,
}

export function useExamsQuery(query: ExamPagedQuery) {
    return useQuery({
        queryKey: EXAM_KEYS.paged(query),
        queryFn: async () => {
            const res = await examService.getPaged(query)
            return res.data ?? {total: 0, page: query.page ?? 1, pageSize: query.pageSize ?? 20, items: []}
        },
    })
}

export function useExamWithQuestionsQuery(id?: string) {
    return useQuery({
        queryKey: EXAM_KEYS.detail(id ?? ''),
        queryFn: async () => (await examService.getWithQuestions(id!)).data ?? null,
        enabled: !!id,
    })
}

export function useExamVariantsQuery(id?: string) {
    return useQuery({
        queryKey: EXAM_KEYS.variants(id ?? ''),
        queryFn: async () => (await examService.getVariants(id!)).data ?? [],
        enabled: !!id,
    })
}

export function useExamAnalyticsQuery(id?: string) {
    return useQuery({
        queryKey: EXAM_KEYS.analytics(id ?? ''),
        queryFn: async () => (await examService.getAnalytics(id!)).data ?? null,
        enabled: !!id,
    })
}

export function usePublishExamMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => examService.publish(id),
        onSuccess: () => {
            message.success('Đã phát hành đề thi')
            void qc.invalidateQueries({queryKey: EXAM_KEYS.all})
        },
        onError: () => message.error('Không thể phát hành đề thi'),
    })
}

export function useDeleteExamMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => examService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa đề thi')
            void qc.invalidateQueries({queryKey: EXAM_KEYS.all})
        },
        onError: () => message.error('Không thể xóa đề thi'),
    })
}

export function useGenerateExamMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: GenerateExamBody) => examGeneratorService.generate(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error || !res.data) {
                message.error(res.message || 'Sinh đề thất bại')
                return
            }
            message.success('Sinh đề thành công')
            void qc.invalidateQueries({queryKey: EXAM_KEYS.all})
        },
        onError: () => message.error('Sinh đề thất bại'),
    })
}

export function useBatchGenerateExamMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: BatchGenerateExamBody) => examGeneratorService.batchGenerate(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error || !res.data) {
                message.error(res.message || 'Sinh lô đề thất bại')
                return
            }
            message.success(`Đã sinh ${res.data.variants.length} biến thể`)
            void qc.invalidateQueries({queryKey: EXAM_KEYS.all})
        },
        onError: () => message.error('Sinh lô đề thất bại'),
    })
}
