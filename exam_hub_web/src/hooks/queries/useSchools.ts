import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message, Modal} from 'antd'
import {statusCode} from '../../services/requestService'
import {schoolService} from '../../services/schoolService'

export const SCHOOL_KEYS = {
    all: ['schools'] as const,
    detail: (id: number) => ['schools', id] as const,
}

export function useSchoolsQuery() {
    return useQuery({
        queryKey: SCHOOL_KEYS.all,
        queryFn: async () => {
            const res = await schoolService.getAll()
            return res.data ?? []
        },
    })
}

export function useCreateSchoolMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: SchoolBody) => schoolService.create(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Thêm trường thành công')
            void qc.invalidateQueries({queryKey: SCHOOL_KEYS.all})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useUpdateSchoolMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, body}: {id: number; body: SchoolBody}) => schoolService.update(id, body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Cập nhật thành công')
            void qc.invalidateQueries({queryKey: SCHOOL_KEYS.all})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useDeleteSchoolMutation() {
    const qc = useQueryClient()
    const mutation = useMutation({
        mutationFn: ({id, force = false}: {id: number; force?: boolean}) => schoolService.remove(id, force),
        onSuccess: (res, variables) => {
            if (res.status === statusCode.Conflict) {
                Modal.confirm({
                    title: 'Đang được sử dụng',
                    content: res.message,
                    okText: 'Xoá bắt buộc',
                    okType: 'danger',
                    cancelText: 'Hủy',
                    onOk: () => mutation.mutate({id: variables.id, force: true}),
                })
                return
            }
            if (res.status === statusCode.Error) { message.error(res.message || 'Không thể xóa'); return }
            message.success('Đã xóa')
            void qc.invalidateQueries({queryKey: SCHOOL_KEYS.all})
        },
        onError: () => message.error('Không thể xóa'),
    })
    return mutation
}
