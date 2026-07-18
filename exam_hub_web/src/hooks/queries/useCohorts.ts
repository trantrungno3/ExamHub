import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {cohortService} from '../../services/cohortService'

export const COHORT_KEYS = {
    bySchool: (schoolId: number) => ['cohorts', 'school', schoolId] as const,
}

export function useCohortsQuery(schoolId: number) {
    return useQuery({
        queryKey: COHORT_KEYS.bySchool(schoolId),
        queryFn: async () => {
            const res = await cohortService.getBySchool(schoolId)
            return res.data ?? []
        },
        enabled: schoolId > 0,
    })
}

export function useCreateCohortMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: CohortBody) => cohortService.create(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Thêm khoá học thành công')
            void qc.invalidateQueries({queryKey: COHORT_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useUpdateCohortMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, body}: {id: number; body: CohortBody}) => cohortService.update(id, body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Cập nhật thành công')
            void qc.invalidateQueries({queryKey: COHORT_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useDeleteCohortMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: number) => cohortService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa')
            void qc.invalidateQueries({queryKey: COHORT_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Không thể xóa'),
    })
}
