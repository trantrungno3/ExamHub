import {useAuth} from '../../AuthProvider'
import AdminProfilePage from './AdminProfilePage'
import TeacherProfilePage from './TeacherProfilePage'

/** /app/profile — render màn theo vai trò (Admin ưu tiên), giữ 3 component tách biệt. */
export default function AppProfilePage() {
    const {user} = useAuth()
    return user?.roles?.includes('Admin') ? <AdminProfilePage/> : <TeacherProfilePage/>
}
