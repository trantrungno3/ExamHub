import {Navigate, Outlet, useNavigate} from 'react-router-dom'
import {Button} from 'antd'
import {LogoutOutlined} from '@ant-design/icons'
import {useAuth} from '../AuthProvider'

export default function StudentLayout() {
    const navigate = useNavigate()
    const {user, isAuthenticated, logout} = useAuth()

    if (!isAuthenticated) return <Navigate to="/login" replace/>

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    const displayName = user?.displayName ?? user?.userName ?? 'A'

    return (
        <div className="min-h-screen flex flex-col" style={{background: '#f5f4f1'}}>
            <header className="h-16 px-6 flex items-center justify-between shrink-0" style={{background: '#3a74f5'}}>
                <div className="flex items-center gap-2.5">
                    <div className="w-[30px] h-[30px] rounded-md bg-white flex items-center justify-center text-[12px] font-bold"
                         style={{color: '#3a74f5'}}>
                        EH
                    </div>
                    <span className="font-semibold text-white">ExamHub</span>
                </div>
                <div className="flex items-center gap-4 text-white">
                    <button className="text-right leading-tight" onClick={() => navigate('/student/profile')}>
                        <div className="text-[13px] font-medium">{displayName}</div>
                        <div className="text-[12px]" style={{color: '#cdd9fb'}}>Học sinh</div>
                    </button>
                    <div className="w-8 h-8 rounded-full flex items-center justify-center text-[13px] font-semibold"
                         style={{background: '#eaf0ff', color: '#3a74f5'}}>
                        {displayName.charAt(0).toUpperCase()}
                    </div>
                    <Button size="small" ghost icon={<LogoutOutlined/>} onClick={handleLogout}>
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
