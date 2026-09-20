import {useMutation, useQuery, useQueryClient, type QueryClient} from '@tanstack/react-query'
import {App} from 'antd'
import {statusCode} from '../../services/requestService'
import {schoolMemberService} from '../../services/schoolMemberService'
import {COHORT_MEMBER_KEYS} from './useCohortMembers'

export const SCHOOL_MEMBER_KEYS = {
    bySchool: (schoolId: number) => ['schoolMembers', 'school', schoolId] as const,
}

/** Thêm hàng loạt và import đều có thể chạm cả thành viên trường lẫn học sinh theo khoá. */
function invalidateMemberships(qc: QueryClient, schoolId: number) {
    void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
    void qc.invalidateQueries({queryKey: COHORT_MEMBER_KEYS.bySchool(schoolId)})
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
    const {message} = App.useApp()
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
    const {message} = App.useApp()
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
    const {message} = App.useApp()
    return useMutation({
        mutationFn: (id: string) => schoolMemberService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa thành viên')
            void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Không thể xóa'),
    })
}

export function useBulkAddSchoolMembersMutation(schoolId: number) {
    const qc = useQueryClient()
    const {message} = App.useApp()
    return useMutation({
        mutationFn: (body: SchoolMemberBulkAddRequest) => schoolMemberService.bulkAdd(body),
        onSuccess: res => {
            if (!res.data) return void message.error(res.message || 'Không thể thêm thành viên')
            if (res.data.successCount > 0) invalidateMemberships(qc, schoolId)
        },
        onError: () => void message.error('Không thể thêm thành viên'),
    })
}

export function usePreviewSchoolMembersMutation() {
    const {message} = App.useApp()
    return useMutation({
        mutationFn: ({schoolId, file}: {schoolId: number; file: File}) =>
            schoolMemberService.previewImport(schoolId, file),
        onError: () => void message.error('Không thể kiểm tra file import'),
    })
}

export function useImportSchoolMembersMutation(schoolId: number) {
    const qc = useQueryClient()
    const {message} = App.useApp()
    return useMutation({
        mutationFn: (file: File) => schoolMemberService.bulkImport(schoolId, file),
        onSuccess: res => {
            if (!res.data) return void message.error(res.message || 'Import thất bại')
            if (res.data.successCount > 0) invalidateMemberships(qc, schoolId)
        },
        onError: () => void message.error('Import thất bại'),
    })
}

export function useSetSchoolMemberActiveMutation(schoolId: number) {
    const qc = useQueryClient()
    const {message} = App.useApp()
    return useMutation({
        mutationFn: ({id, isActive}: {id: string; isActive: boolean}) =>
            schoolMemberService.setActive(id, isActive),
        onSuccess: () => {
            void qc.invalidateQueries({queryKey: SCHOOL_MEMBER_KEYS.bySchool(schoolId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}
