import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {statusCode} from '../../services/requestService'
import {cohortClassTeacherService} from '../../services/cohortClassTeacherService'

export const CLASS_TEACHER_KEYS = {
    byClass: (classId: number) => ['classTeachers', classId] as const,
    eligible: (classId: number, subjectId: number) => ['eligibleTeachers', classId, subjectId] as const,
}

export function useClassTeachersQuery(classId: number) {
    return useQuery({
        queryKey: CLASS_TEACHER_KEYS.byClass(classId),
        queryFn: async () => (await cohortClassTeacherService.getByClass(classId)).data ?? [],
        enabled: classId > 0,
    })
}

export function useEligibleTeachersQuery(classId: number, subjectId?: number) {
    return useQuery({
        queryKey: CLASS_TEACHER_KEYS.eligible(classId, subjectId ?? 0),
        queryFn: async () => (await cohortClassTeacherService.getEligibleTeachers(classId, subjectId!)).data ?? [],
        enabled: classId > 0 && !!subjectId,
    })
}

export function useAssignTeacherMutation(classId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (body: AssignTeacherBody) => cohortClassTeacherService.assign(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Phân công giáo viên thành công')
            void qc.invalidateQueries({queryKey: CLASS_TEACHER_KEYS.byClass(classId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}

export function useRemoveTeacherMutation(classId: number) {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: number) => cohortClassTeacherService.remove(id),
        onSuccess: (res) => {
            if (res.status === statusCode.Error) { message.error(res.message || 'Có lỗi xảy ra'); return }
            message.success('Đã xoá phân công')
            void qc.invalidateQueries({queryKey: CLASS_TEACHER_KEYS.byClass(classId)})
        },
        onError: () => message.error('Có lỗi xảy ra'),
    })
}
