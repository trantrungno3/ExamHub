import {useQuery} from '@tanstack/react-query'
import {topicService} from '../../services/topicService'
import {subjectService} from '../../services/subjectService'
import {gradeLevelService} from '../../services/gradeLevelService'
import {difficultyLevelService} from '../../services/difficultyLevelService'
import {questionTypeService} from '../../services/questionTypeService'
import {cognitiveLevelService} from '../../services/cognitiveLevelService'

export function useTopicsQuery() {
    return useQuery({queryKey: ['topics', 'all'], queryFn: async () => (await topicService.getAll()).data ?? []})
}

export function useSubjectsQuery() {
    return useQuery({queryKey: ['subjects', 'all'], queryFn: async () => (await subjectService.getAll()).data ?? []})
}

export function useGradeLevelsListQuery() {
    return useQuery({queryKey: ['gradeLevels', 'all'], queryFn: async () => (await gradeLevelService.getAll()).data ?? []})
}

export function useDifficultyLevelsQuery() {
    return useQuery({queryKey: ['difficultyLevels', 'all'], queryFn: async () => (await difficultyLevelService.getAll()).data ?? []})
}

export function useQuestionTypesQuery() {
    return useQuery({queryKey: ['questionTypes', 'all'], queryFn: async () => (await questionTypeService.getAll()).data ?? []})
}

export function useCognitiveLevelsQuery() {
    return useQuery({queryKey: ['cognitiveLevels', 'all'], queryFn: async () => (await cognitiveLevelService.getAll()).data ?? []})
}
