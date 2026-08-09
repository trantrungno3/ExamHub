import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {questionService} from '../../services/questionService'

export const QUESTION_KEYS = {
    all: ['questions'] as const,
    stats: ['questionStats'] as const,
    paged: (query: QuestionPagedQuery) => ['questions', 'paged', query] as const,
    detail: (id: string) => ['questions', 'detail', id] as const,
}

export function useQuestionStatsQuery() {
    return useQuery({
        queryKey: QUESTION_KEYS.stats,
        queryFn: async () =>
            (await questionService.getStats()).data ?? {total: 0, verified: 0, unverified: 0, inactive: 0},
    })
}

export function useQuestionsQuery(query: QuestionPagedQuery) {
    return useQuery({
        queryKey: QUESTION_KEYS.paged(query),
        queryFn: async () => {
            const res = await questionService.getPaged(query)
            return res.data ?? {total: 0, page: query.page ?? 1, pageSize: query.pageSize ?? 20, items: []}
        },
    })
}

export function useQuestionQuery(id: string | undefined) {
    return useQuery({
        queryKey: QUESTION_KEYS.detail(id ?? ''),
        queryFn: async () => {
            const res = await questionService.getById(id!)
            return res.data ?? null
        },
        enabled: !!id,
    })
}

export function useDeleteQuestionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => questionService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa câu hỏi')
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.stats})
        },
        onError: () => message.error('Không thể xóa câu hỏi'),
    })
}

export function useVerifyQuestionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => questionService.verify(id),
        onSuccess: () => {
            message.success('Đã duyệt câu hỏi')
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.stats})
        },
        onError: () => message.error('Không thể duyệt câu hỏi'),
    })
}

export function useUnverifyQuestionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => questionService.unverify(id),
        onSuccess: () => {
            message.success('Đã bỏ duyệt câu hỏi')
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.stats})
        },
        onError: () => message.error('Không thể bỏ duyệt câu hỏi'),
    })
}

export function useBulkImportMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (args: BulkImportArgs) => questionService.bulkImport(args),
        onSuccess: (res) => {
            if (res.status === statusCode.Error || !res.data) {
                message.error(res.message || 'Import thất bại')
                return
            }
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
        },
        onError: () => message.error('Import thất bại'),
    })
}
