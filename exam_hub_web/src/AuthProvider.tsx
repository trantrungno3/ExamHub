/**
 * @deprecated Shim for backward compatibility — logic moved to authStore.
 */
import type { ReactNode } from 'react'

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
    return <>{children}</>
}
