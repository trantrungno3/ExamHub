import {useQuery} from '@tanstack/react-query'
import {menuService} from '../../services/menuService'

export const MENU_KEYS = {
    all: ['menu'] as const,
}

export function useMenuQuery() {
    return useQuery({
        queryKey: MENU_KEYS.all,
        queryFn: async () => {
            const res = await menuService.getMenu()
            return res.data ?? []
        },
        staleTime: 5 * 60 * 1000,
    })
}
