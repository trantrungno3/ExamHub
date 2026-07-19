import {useCallback, useEffect, useState} from 'react'
import {Outlet, useLocation, useNavigate} from 'react-router-dom'
import {
    AppstoreOutlined,
    UnorderedListOutlined,
    FileTextOutlined,
    ThunderboltOutlined,
    UserOutlined,
    TagsOutlined,
    BankOutlined,
    QuestionCircleOutlined,
    ScheduleOutlined,
    LogoutOutlined,
    DownOutlined,
    RightOutlined,
} from '@ant-design/icons'
import type {ReactNode} from 'react'
import {useAuth} from '../AuthProvider'
import {isTokenExpired} from '../utils/jwt'
import {useMenuQuery} from '../hooks/queries/useMenu'

const ICON_MAP: Record<string, ReactNode> = {
    dashboard:  <AppstoreOutlined/>,
    question:   <QuestionCircleOutlined/>,
    template:   <FileTextOutlined/>,
    generate:   <ThunderboltOutlined/>,
    exam:       <UnorderedListOutlined/>,
    session:    <ScheduleOutlined/>,
    school:     <BankOutlined/>,
    user:       <UserOutlined/>,
    category:   <TagsOutlined/>,
}

const FALLBACK_NAV: MenuItem[] = [
    {key: 'dashboard', path: '/app/dashboard', label: 'Tổng quan',      icon: 'dashboard', order: 1},
    {key: 'questions', path: '/app/questions', label: 'Câu hỏi',        icon: 'question',  order: 2},
    {
        key: 'exam-mgmt', label: 'Quản lý đề thi', icon: 'template', order: 3,
        children: [
            {key: 'exams',         path: '/app/exams',         label: 'Mẫu đề thi',  icon: 'template',  order: 1},
            {key: 'generate',      path: '/app/generate',      label: 'Sinh đề thi', icon: 'generate',  order: 2},
            {key: 'exam-list',     path: '/app/exam-list',     label: 'Đề thi',      icon: 'exam',      order: 3},
            {key: 'exam-sessions', path: '/app/exam-sessions', label: 'Kỳ thi',      icon: 'session',   order: 4},
        ],
    },
    {key: 'schools',  path: '/app/schools',  label: 'Quản lý trường', icon: 'school',   order: 6},
    {key: 'users',    path: '/app/users',    label: 'Người dùng',     icon: 'user',     order: 7},
    {key: 'category', path: '/app/category', label: 'Danh mục',       icon: 'category', order: 8},
]

const REFRESH_BUFFER_MS = 5 * 60 * 1000

export default function AppLayout() {
    const location = useLocation()
    const navigate = useNavigate()
    const {token, logout, refresh} = useAuth()
    const {data: menuItems} = useMenuQuery()
    const navItems: MenuItem[] = (menuItems && menuItems.length > 0 ? menuItems : FALLBACK_NAV)
    const [openGroups, setOpenGroups] = useState<Record<string, boolean>>({})

    const isChildActive = (children?: MenuItem[]) =>
        children?.some(c => c.path && location.pathname.startsWith(c.path)) ?? false

    const toggleGroup = (key: string, defaultOpen: boolean) =>
        setOpenGroups(prev => ({...prev, [key]: !(prev[key] ?? defaultOpen)}))

    useEffect(() => {
        if (!token) {
            navigate('/login', {replace: true})
            return
        }
        if (isTokenExpired(token.refreshExpiresAt)) {
            logout()
            navigate('/login', {replace: true})
            return
        }
        if (isTokenExpired(token.expiresAt)) {
            void refresh().then(ok => {
                if (!ok) navigate('/login', {replace: true})
            })
        } else if (isTokenExpired(token.expiresAt, REFRESH_BUFFER_MS)) {
            void refresh()
        }
    }, [location.pathname, token, navigate, refresh, logout])

    const handleLogout = useCallback(() => {
        logout()
        navigate('/login')
    }, [logout, navigate])

    return (
        <div className="app-layout">
            <aside className="sidebar">
                <div className="sidebar-logo">
                    <div className="sidebar-logo-icon">EH</div>
                    <span className="sidebar-logo-name">ExamHub</span>
                </div>

                <nav className="sidebar-nav">
                    {navItems.map((item) => {
                        if (item.children && item.children.length > 0) {
                            const activeChild = isChildActive(item.children)
                            const open = openGroups[item.key] ?? activeChild
                            return (
                                <div key={item.key}>
                                    <button
                                        onClick={() => toggleGroup(item.key, activeChild)}
                                        className={`sidebar-nav-item ${activeChild ? 'sidebar-nav-item--active' : ''}`}
                                    >
                                        <span className="text-base">{ICON_MAP[item.icon] ?? <AppstoreOutlined/>}</span>
                                        <span className="flex-1 text-left">{item.label}</span>
                                        <span className="text-xs">{open ? <DownOutlined/> : <RightOutlined/>}</span>
                                    </button>
                                    {open && (
                                        <div className="ml-4">
                                            {item.children.map((child) => (
                                                <button
                                                    key={child.key}
                                                    onClick={() => child.path && navigate(child.path)}
                                                    className={`sidebar-nav-item ${
                                                        child.path && location.pathname.startsWith(child.path) ? 'sidebar-nav-item--active' : ''
                                                    }`}
                                                >
                                                    <span className="text-base">{ICON_MAP[child.icon] ?? <AppstoreOutlined/>}</span>
                                                    <span>{child.label}</span>
                                                </button>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            )
                        }
                        return (
                            <button
                                key={item.key}
                                onClick={() => item.path && navigate(item.path)}
                                className={`sidebar-nav-item ${
                                    item.path && location.pathname.startsWith(item.path) ? 'sidebar-nav-item--active' : ''
                                }`}
                            >
                                <span className="text-base">{ICON_MAP[item.icon] ?? <AppstoreOutlined/>}</span>
                                <span>{item.label}</span>
                            </button>
                        )
                    })}
                </nav>

                <div className="sidebar-footer">
                    <button
                        onClick={() => navigate('/app/profile')}
                        className={`sidebar-nav-item ${
                            location.pathname.startsWith('/app/profile') ? 'sidebar-nav-item--active' : ''
                        }`}
                    >
                        <UserOutlined/>
                        <span>Tài khoản</span>
                    </button>
                    <button
                        onClick={handleLogout}
                        className="sidebar-nav-item text-red-400 hover:!text-red-300 hover:!bg-red-500/10"
                    >
                        <LogoutOutlined/>
                        <span>Đăng xuất</span>
                    </button>
                </div>
            </aside>

            <div className="page-canvas">
                <Outlet/>
            </div>
        </div>
    )
}
