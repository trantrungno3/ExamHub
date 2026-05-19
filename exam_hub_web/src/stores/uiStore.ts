import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface UiState {
    sidebarCollapsed: boolean
    theme: 'light' | 'dark'
    locale: 'vi' | 'en'
}

interface UiActions {
    toggleSidebar: () => void
    setSidebarCollapsed: (collapsed: boolean) => void
    setTheme: (theme: 'light' | 'dark') => void
    setLocale: (locale: 'vi' | 'en') => void
}

export const useUiStore = create<UiState & UiActions>()(
    persist(
        (set) => ({
            sidebarCollapsed: false,
            theme: 'light',
            locale: 'vi',

            toggleSidebar: () => set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),
            setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
            setTheme: (theme) => set({ theme }),
            setLocale: (locale) => set({ locale }),
        }),
        {
            name: 'examhub_ui',
            partialize: (state) => ({ theme: state.theme, locale: state.locale, sidebarCollapsed: state.sidebarCollapsed }),
        }
    )
)
