import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {schoolMemberService} from '../../services/schoolMemberService'

export const SCHOOL_MEMBER_KEYS = {
    bySchool: (schoolId: number) => ['schoolMembers', 'school', schoolId] as const,
}

export function useSchoolMembersQuery(schoolId: number) {
    return useQuery({
        queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId),
        queryFn: async () => {
            const res = await schoolMemberService.getBySchool(schoolId)
            return res.data ?? []
        },
        enabled: schoolId > 0,
    })
}

export function useAddSchoolMemberMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: SchoolMemberBody) => schoolMemberService.add(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Thêm thành viên thành công')
            void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useUpdateSchoolMemberMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, body}: {id: string; body: SchoolMemberBody}) =>
            schoolMemberService.update(id, body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Cập nhật thành công')
            void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useRemoveSchoolMemberMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => schoolMemberService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa thành viên')
            void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Không thể xóa'),
    })
}

export function useSetSchoolMemberActiveMutation(schoolId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: ({id, isActive}: {id: string; isActive: boolean}) =>
            schoolMemberService.setActive(id, isActive),
        onSuccess: () => {
            void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}
