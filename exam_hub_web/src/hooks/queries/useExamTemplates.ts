import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {examTemplateService} from '../../services/examTemplateService'

export const EXAM_TEMPLATE_KEYS = {
    all: ['examTemplates'] as const,
    byGrade: (gradeLevelId?: number) => ['examTemplates', 'byGrade', gradeLevelId] as const,
    detail: (id: string) => ['examTemplates', 'detail', id] as const,
}

export function useExamTemplatesByGradeQuery(gradeLevelId?: number) {
    return useQuery({
        queryKey: EXAM_TEMPLATE_KEYS.byGrade(gradeLevelId),
        queryFn: async () => (await examTemplateService.getByGrade(gradeLevelId!)).data ?? [],
        enabled: !!gradeLevelId,
    })
}

export function useExamTemplateQuery(id?: string) {
    return useQuery({
        queryKey: EXAM_TEMPLATE_KEYS.detail(id ?? ''),
        queryFn: async () => (await examTemplateService.getWithSections(id!)).data ?? null,
        enabled: !!id,
    })
}

export function useDeleteExamTemplateMutation() {
    const qc = useQueryClient()
    return useMutation({
        mutationFn: (id: string) => examTemplateService.remove(id),
        onSuccess: () => {
            message.success('Đã xóa mẫu đề thi')
            void qc.invalidateQueries({queryKey: EXAM_TEMPLATE_KEYS.all})
        },
        onError: () => message.error('Không thể xóa mẫu đề thi'),
    })
}
