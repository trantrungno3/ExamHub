import {Suspense, useCallback, useEffect, useState} from 'react'
import {Spin} from 'antd'
import {NavLink, Outlet, useLocation, useNavigate} from 'react-router-dom'
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
} from '@ant-design/icons'
import type {ReactNode} from 'react'
import {useAuth} from '../hooks/useAuth'
import {isTokenExpired} from '../utils/jwt'
import {useMenuQuery} from '../hooks/queries/useMenu'
import {ROUTES} from '../routes/paths'

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
        // Không còn kiểm tra refresh expiry ở client: cookie HttpOnly là nguồn sự thật, refresh
        // thất bại mới là tín hiệu hết phiên.
        if (isTokenExpired(token.expiresAt)) {
            void refresh().then(ok => {
                if (!ok) navigate('/login', {replace: true})
            })
        } else if (isTokenExpired(token.expiresAt, REFRESH_BUFFER_MS)) {
            void refresh()
        }
    }, [location.pathname, token, navigate, refresh, logout])

    // TEMP-PERF: đo click → paint của mỗi lần đổi page. Xoá sau khi debug xong.
    useEffect(() => {
        const t0 = performance.now()
        requestAnimationFrame(() => requestAnimationFrame(() => {
            const paint = performance.now() - t0
            const net = performance.getEntriesByType('resource')
                .filter(e => e.startTime >= t0 - 50 && e.name.includes('/api/'))
                .map(e => `${e.name.split('/api/')[1].split('?')[0]}:${Math.round(e.duration)}ms`)
            console.log(`[nav] ${location.pathname} paint=${Math.round(paint)}ms api=[${net.join(' ')}]`)
        }))
    }, [location.pathname])

    const handleLogout = useCallback(() => {
        logout()
        navigate('/login')
    }, [logout, navigate])

    return (
        <div className="app-layout">
            <aside className="sidebar">
                <button className="sidebar-logo" aria-label="Về trang chủ" onClick={() => navigate(ROUTES.APP)}>
                    <div className="sidebar-logo-icon">EH</div>
                    <span className="sidebar-logo-name">ExamHub</span>
                </button>

                <nav className="sidebar-nav">
                    {navItems.map((item) => {
                        if (item.children && item.children.length > 0) {
                            const activeChild = isChildActive(item.children)
                            const open = openGroups[item.key] ?? activeChild
                            return (
                                <div key={item.key}>
                                    <button
                                        onClick={() => toggleGroup(item.key, activeChild)}
                                        aria-expanded={open}
                                        className={`sidebar-nav-item ${open || activeChild ? 'sidebar-nav-item--open' : ''}`}
                                    >
                                        <span className="text-base">{ICON_MAP[item.icon] ?? <AppstoreOutlined/>}</span>
                                        <span className="flex-1 text-left">{item.label}</span>
                                        <DownOutlined className={`sidebar-chevron ${open ? '' : '-rotate-90'}`}/>
                                    </button>
                                    {open && (
                                        <div className="sidebar-submenu" role="group">
                                            {item.children.map((child) => (
                                                <NavLink
                                                    key={child.key}
                                                    to={child.path ?? '#'}
                                                    className={({isActive}) =>
                                                        `sidebar-subitem ${isActive ? 'sidebar-nav-item--active' : ''}`}
                                                >
                                                    <span>{child.label}</span>
                                                </NavLink>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            )
                        }
                        return (
                            <NavLink
                                key={item.key}
                                to={item.path ?? '#'}
                                className={({isActive}) =>
                                    `sidebar-nav-item ${isActive ? 'sidebar-nav-item--active' : ''}`}
                            >
                                <span className="text-base">{ICON_MAP[item.icon] ?? <AppstoreOutlined/>}</span>
                                <span>{item.label}</span>
                            </NavLink>
                        )
                    })}
                </nav>

                <div className="sidebar-footer">
                    <NavLink
                        to="/app/profile"
                        className={({isActive}) => `sidebar-nav-item ${isActive ? 'sidebar-nav-item--active' : ''}`}
                    >
                        <UserOutlined/>
                        <span>Tài khoản</span>
                    </NavLink>
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
                <Suspense fallback={<div className="flex-1 flex items-center justify-center"><Spin size="large"/></div>}>
                    <Outlet/>
                </Suspense>
            </div>
        </div>
    )
}
