import {Button, Result} from 'antd'
import {useNavigate} from 'react-router-dom'
import {useAuth} from '../../AuthProvider'
import {ROUTES} from '../../routes/paths'

export default function NoRolePage() {
    const navigate = useNavigate()
    const {logout} = useAuth()

    const handleLogout = () => {
        logout()
        navigate(ROUTES.LOGIN, {replace: true})
    }

    return (
        <div className="flex h-screen items-center justify-center bg-gray-100">
            <Result
                status="warning"
                title="Tài khoản chưa được cấp vai trò"
                subTitle="Tài khoản không có quyền truy cập hệ thống, vui lòng liên hệ quản trị viên hoặc giáo viên."
                extra={<Button type="primary" onClick={handleLogout}>Đăng xuất</Button>}
            />
        </div>
    )
}
