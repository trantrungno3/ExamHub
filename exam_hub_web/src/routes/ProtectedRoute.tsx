import {Navigate, Outlet} from 'react-router-dom'
import {useAuthStore} from '../stores/authStore'
import {routeDecision} from './routeDecision'

type Props = {
    allowedRoles?: string[]
}

export function ProtectedRoute({allowedRoles}: Readonly<Props>) {
    const isAuthenticated = useAuthStore(s => s.isAuthenticated)
    const user = useAuthStore(s => s.user)

    const redirectTo = routeDecision(isAuthenticated, user?.roles, allowedRoles)
    return redirectTo ? <Navigate to={redirectTo} replace/> : <Outlet/>
}
