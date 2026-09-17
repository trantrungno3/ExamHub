import {useQuery, useQueryClient} from '@tanstack/react-query'
import {userService} from '../../services/userService'

export const USER_KEYS = {
    all: ['users', 'all'] as const,
}

export function useUsersQuery() {
    return useQuery({
        queryKey: USER_KEYS.all,
        queryFn: async () => (await userService.getAll()).data ?? [],
    })
}

export function useInvalidateUsers() {
    const qc = useQueryClient()
    return () => void qc.invalidateQueries({queryKey: USER_KEYS.all})
}
