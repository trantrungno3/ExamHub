import {Navigate, Outlet} from 'react-router-dom'
import {useAuthStore} from '../stores/authStore'
import {ROUTES} from './paths'

type Props = {
    allowedRoles?: string[]
}

export function ProtectedRoute({allowedRoles}: Readonly<Props>) {
    const {isAuthenticated, user} = useAuthStore()

    if (!isAuthenticated) return <Navigate to={ROUTES.LOGIN} replace/>

    if (!user?.roles?.length) return <Navigate to={ROUTES.NO_ROLE} replace/>

    if (allowedRoles && allowedRoles.length > 0) {
        const hasRole = user?.roles.some(r => allowedRoles.includes(r)) ?? false
        if (!hasRole) return <Navigate to={ROUTES.FORBIDDEN} replace/>
    }

    return <Outlet/>
}
