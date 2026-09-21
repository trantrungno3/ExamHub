import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query'
import {message} from 'antd'
import {examTemplateService} from '../../services/examTemplateService'

export const EXAM_TEMPLATE_KEYS = {
    all: ['examTemplates'] as const,
    stats: ['examTemplateStats'] as const,
    list: (filter: {subjectId?: number; gradeLevelId?: number}) => ['examTemplates', 'list', filter] as const,
    detail: (id: string) => ['examTemplates', 'detail', id] as const,
}

export function useExamTemplateStatsQuery() {
    return useQuery({
        queryKey: EXAM_TEMPLATE_KEYS.stats,
        queryFn: async () =>
            (await examTemplateService.getStats()).data ??
            {totalTemplates: 0, activeTemplates: 0, totalExamsGenerated: 0, avgQuestions: 0},
    })
}

export function useExamTemplatesQuery(filter: {subjectId?: number; gradeLevelId?: number} = {}) {
    return useQuery({
        queryKey: EXAM_TEMPLATE_KEYS.list(filter),
        queryFn: async () => (await examTemplateService.getList(filter)).data ?? [],
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
