import {useCallback, useEffect} from 'react'
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
    LogoutOutlined,
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
    school:     <BankOutlined/>,
    user:       <UserOutlined/>,
    category:   <TagsOutlined/>,
}

const FALLBACK_NAV = [
    {path: '/app/dashboard', label: 'Tổng quan',     icon: 'dashboard'},
    {path: '/app/questions', label: 'Câu hỏi',       icon: 'question'},
    {path: '/app/exams',     label: 'Mẫu đề thi',    icon: 'template'},
    {path: '/app/generate',  label: 'Sinh đề thi',   icon: 'generate'},
    {path: '/app/exam-list', label: 'Đề thi',        icon: 'exam'},
    {path: '/app/schools',   label: 'Quản lý trường', icon: 'school'},
    {path: '/app/users',     label: 'Người dùng',    icon: 'user'},
    {path: '/app/category',  label: 'Danh mục',      icon: 'category'},
]

const REFRESH_BUFFER_MS = 5 * 60 * 1000

export default function AppLayout() {
    const location = useLocation()
    const navigate = useNavigate()
    const {token, logout, refresh} = useAuth()
    const {data: menuItems} = useMenuQuery()
    const navItems = (menuItems && menuItems.length > 0 ? menuItems : FALLBACK_NAV)

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
                    {navItems.map((item) => (
                        <button
                            key={item.path}
                            onClick={() => navigate(item.path)}
                            className={`sidebar-nav-item ${
                                location.pathname.startsWith(item.path) ? 'sidebar-nav-item--active' : ''
                            }`}
                        >
                            <span className="text-base">{ICON_MAP[item.icon] ?? <AppstoreOutlined/>}</span>
                            <span>{item.label}</span>
                        </button>
                    ))}
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
