import {Navigate, Outlet, useNavigate} from 'react-router-dom'
import {Button} from 'antd'
import {LogoutOutlined, UserOutlined} from '@ant-design/icons'
import {useAuth} from '../AuthProvider'

export default function StudentLayout() {
    const navigate = useNavigate()
    const {user, isAuthenticated, logout} = useAuth()

    if (!isAuthenticated) return <Navigate to="/login" replace/>

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    return (
        <div className="min-h-screen bg-gray-100 flex flex-col">
            <header className="bg-white border-b border-gray-200 px-6 h-14 flex items-center justify-between">
                <div className="flex items-center gap-2">
                    <div className="sidebar-logo-icon">EH</div>
                    <span className="font-semibold text-gray-800">ExamHub</span>
                </div>
                <div className="flex items-center gap-4">
                    <button
                        className="text-sm text-gray-600 hover:text-blue-600 flex items-center gap-1.5"
                        onClick={() => navigate('/student/profile')}
                    >
                        <UserOutlined/>
                        {user?.displayName ?? user?.userName}
                    </button>
                    <Button size="small" icon={<LogoutOutlined/>} onClick={handleLogout}>
                        Đăng xuất
                    </Button>
                </div>
            </header>

            <main className="flex-1 overflow-auto">
                <Outlet/>
            </main>
        </div>
    )
}
