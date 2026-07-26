import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {cohortMemberService} from '../../services/cohortMemberService'

export const COHORT_MEMBER_KEYS = {
    byCohort: (cohortId: number) => ['cohortMembers', 'cohort', cohortId] as const,
}

export function useCohortMembersQuery(cohortId: number) {
    return useQuery({
        queryKey: COHORT_MEMBER_KEYS.byCohort(cohortId),
        queryFn: async () => {
            const res = await cohortMemberService.getByCohort(cohortId)
            return res.data ?? []
        },
        enabled: cohortId > 0,
    })
}

export function useAddCohortMemberMutation(cohortId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: CohortMemberBody) => cohortMemberService.add(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Thêm học sinh thành công')
            void qc.invalidateQueries({queryKey: COHORT_MEMBER_KEYS.byCohort(cohortId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useRemoveCohortMemberMutation(cohortId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => cohortMemberService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa học sinh')
            void qc.invalidateQueries({queryKey: COHORT_MEMBER_KEYS.byCohort(cohortId)})
        },
        onError: () => message.error('Không thể xóa'),
    })
}

export function useSetCohortMemberActiveMutation(cohortId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, isActive}: {id: string; isActive: boolean}) =>
            cohortMemberService.setActive(id, isActive),
        onSuccess: () => {
            void qc.invalidateQueries({queryKey: COHORT_MEMBER_KEYS.byCohort(cohortId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useSetCohortMemberSectionMutation(cohortId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, section}: {id: string; section: string | null}) =>
            cohortMemberService.setSection(id, section),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Cập nhật lớp thành công')
            void qc.invalidateQueries({queryKey: COHORT_MEMBER_KEYS.byCohort(cohortId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}
