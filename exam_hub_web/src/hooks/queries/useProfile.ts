import {useQuery} from '@tanstack/react-query'
import {authService} from '../../services/authService'

export const PROFILE_KEYS = {
    me: ['profile', 'me'] as const,
}

export function useProfileQuery() {
    return useQuery({
        queryKey: PROFILE_KEYS.me,
        queryFn: async () => (await authService.getInfo()).data ?? null,
    })
}
