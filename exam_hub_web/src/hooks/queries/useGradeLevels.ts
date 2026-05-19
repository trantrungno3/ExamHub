import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { message } from 'antd'
import { statusCode } from '../../services/requestService'
import { gradeLevelService } from '../../services/gradeLevelService'

export const GRADE_LEVEL_KEYS = {
    all: ['gradeLevels'] as const,
}

export function useGradeLevelsQuery() {
    return useQuery({
        queryKey: GRADE_LEVEL_KEYS.all,
        queryFn: async () => {
            const res = await gradeLevelService.getAll()
            return res.data ?? []
        },
    })
}

export function useCreateGradeLevelMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: GradeLevelBody) => gradeLevelService.create(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Có lỗi xảy ra')
                return
            }
            message.success('Thêm thành công')
            void qc.invalidateQueries({ queryKey: GRADE_LEVEL_KEYS.all })
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useUpdateGradeLevelMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({ id, body }: { id: number; body: GradeLevelBody }) =>
            gradeLevelService.update(id, body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) {
                message.error(res.message || 'Có lỗi xảy ra')
                return
            }
            message.success('Cập nhật thành công')
            void qc.invalidateQueries({ queryKey: GRADE_LEVEL_KEYS.all })
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useDeleteGradeLevelMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: number) => gradeLevelService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa')
            void qc.invalidateQueries({ queryKey: GRADE_LEVEL_KEYS.all })
        },
        onError: () => message.error('Không thể xóa'),
    })
}
