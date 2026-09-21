import {Outlet, useNavigate} from 'react-router-dom'
import {Button} from 'antd'
import {LogoutOutlined} from '@ant-design/icons'
import {useAuth} from '../hooks/useAuth'
import {ROUTES} from '../routes/paths'
import {BRAND} from '../constants/theme'

export default function StudentLayout() {
    const navigate = useNavigate()
    // Không check auth ở đây nữa: ProtectedRoute allowedRoles={['Student']} bọc ngoài layout này
    // đã chặn cả chưa đăng nhập và sai role, hai chỗ cùng redirect dễ lệch nhau.
    const {user, logout} = useAuth()

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    const displayName = user?.displayName ?? user?.userName ?? 'A'

    return (
        <div className="min-h-screen flex flex-col" style={{background: BRAND.desk}}>
            <header className="h-16 px-6 flex items-center justify-between shrink-0" style={{background: BRAND.primary}}>
                <button className="flex items-center gap-2.5 cursor-pointer" aria-label="Về trang chủ"
                        onClick={() => navigate(ROUTES.STUDENT_EXAMS)}>
                    <div className="w-[30px] h-[30px] rounded-md bg-white flex items-center justify-center text-[12px] font-bold"
                         style={{color: BRAND.primary}}>
                        EH
                    </div>
                    <span className="font-semibold text-white">ExamHub</span>
                </button>
                <div className="flex items-center gap-4 text-white">
                    <button className="text-right leading-tight" onClick={() => navigate('/student/profile')}>
                        <div className="text-[13px] font-medium">{displayName}</div>
                        <div className="text-[12px]" style={{color: BRAND.primaryOn}}>Học sinh</div>
                    </button>
                    <div className="w-8 h-8 rounded-full flex items-center justify-center text-[13px] font-semibold"
                         style={{background: '#eaf0ff', color: BRAND.primary}}>
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
