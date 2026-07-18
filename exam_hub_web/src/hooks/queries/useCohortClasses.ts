import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {cohortClassService} from '../../services/cohortClassService'

export const COHORT_CLASS_KEYS = {
    byCohort: (cohortId: number) => ['cohortClasses', 'cohort', cohortId] as const,
}

export function useCohortClassesQuery(cohortId: number) {
    return useQuery({
        queryKey: COHORT_CLASS_KEYS.byCohort(cohortId),
        queryFn: async () => {
            const res = await cohortClassService.getByCohort(cohortId)
            return res.data ?? []
        },
        enabled: cohortId > 0,
    })
}

export function useSetHomeroomTeacherMutation(cohortId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, body}: {id: number; body: SetHomeroomTeacherBody}) =>
            cohortClassService.setHomeroomTeacher(id, body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Cập nhật giáo viên chủ nhiệm thành công')
            void qc.invalidateQueries({queryKey: COHORT_CLASS_KEYS.byCohort(cohortId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}
