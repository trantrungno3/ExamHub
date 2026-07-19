import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {examSessionService} from '../../services/examSessionService'

export const EXAM_SESSION_KEYS = {
    all: ['exam-sessions'] as const,
    paged: (query: ExamSessionPagedQuery) => ['exam-sessions', 'paged', query] as const,
    detail: (id: string) => ['exam-sessions', 'detail', id] as const,
    my: ['exam-sessions', 'my'] as const,
    pool: (id: string) => ['exam-sessions', 'pool', id] as const,
}

// ── Queries ─────────────────────────────────────────────────────────────
export function useExamSessionsQuery(query: ExamSessionPagedQuery) {
    return useQuery({
        queryKey: EXAM_SESSION_KEYS.paged(query),
        queryFn: async () => {
            const res = await examSessionService.list(query)
            return res.data ?? {total: 0, page: query.page ?? 1, pageSize: query.pageSize ?? 20, items: []}
        },
    })
}

export function useExamSessionQuery(id?: string) {
    return useQuery({
        queryKey: EXAM_SESSION_KEYS.detail(id ?? ''),
        queryFn: async () => (await examSessionService.getDetail(id!)).data ?? null,
        enabled: !!id,
    })
}

export function useMySessionsQuery() {
    return useQuery({
        queryKey: EXAM_SESSION_KEYS.my,
        queryFn: async () => (await examSessionService.getMy()).data ?? [],
    })
}

export function useSessionPoolQuery(id?: string) {
    return useQuery({
        queryKey: EXAM_SESSION_KEYS.pool(id ?? ''),
        queryFn: async () => (await examSessionService.getPool(id!)).data ?? [],
        enabled: !!id,
    })
}

// ── Mutations ───────────────────────────────────────────────────────────
export function useCreateExamSessionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: ExamSessionBody) => examSessionService.create(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Tạo kỳ thi thất bại')
                return
            }
            message.success('Đã tạo kỳ thi')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.all})
        },
        onError: () => message.error('Tạo kỳ thi thất bại'),
    })
}

export function useUpdateExamSessionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, body}: {id: string; body: ExamSessionBody}) => examSessionService.update(id, body),
        onSuccess: (res, {id}) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Cập nhật kỳ thi thất bại')
                return
            }
            message.success('Đã cập nhật kỳ thi')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.all})
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Cập nhật kỳ thi thất bại'),
    })
}

export function useDeleteExamSessionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => examSessionService.remove(id),
        onSuccess: () => {
            message.success('Đã xoá kỳ thi')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.all})
        },
        onError: () => message.error('Xoá kỳ thi thất bại'),
    })
}

export function useSetSessionExamsMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, examIds}: {id: string; examIds: string[]}) => examSessionService.setExams(id, examIds),
        onSuccess: (res, {id}) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Cập nhật đề thất bại')
                return
            }
            message.success('Đã cập nhật đề')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Cập nhật đề thất bại'),
    })
}

export function useRemoveSessionExamMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, examId}: {id: string; examId: string}) => examSessionService.removeExam(id, examId),
        onSuccess: (_res, {id}) => {
            message.success('Đã gỡ đề')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Gỡ đề thất bại'),
    })
}

export function useAddAssignmentMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, body}: {id: string; body: CreateAssignmentBody}) => examSessionService.addAssignment(id, body),
        onSuccess: (res, {id}) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Giao kỳ thi thất bại')
                return
            }
            message.success('Đã giao kỳ thi')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Giao kỳ thi thất bại'),
    })
}

export function useRemoveAssignmentMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, assignmentId}: {id: string; assignmentId: string}) => examSessionService.removeAssignment(id, assignmentId),
        onSuccess: (_res, {id}) => {
            message.success('Đã gỡ giao')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Gỡ giao thất bại'),
    })
}

export function usePublishSessionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => examSessionService.publish(id),
        onSuccess: (res, id) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Phát hành kỳ thi thất bại')
                return
            }
            message.success('Đã phát hành kỳ thi')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.all})
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Phát hành kỳ thi thất bại'),
    })
}

export function useCloseSessionMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => examSessionService.close(id),
        onSuccess: (res, id) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Đóng kỳ thi thất bại')
                return
            }
            message.success('Đã đóng kỳ thi')
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.all})
            void qc.invalidateQueries({queryKey: EXAM_SESSION_KEYS.detail(id)})
        },
        onError: () => message.error('Đóng kỳ thi thất bại'),
    })
}

export function useStartSessionMutation() {
    return useMutation({
        mutationFn: ({id, examId}: {id: string; examId?: string}) => examSessionService.start(id, examId),
    })
}
