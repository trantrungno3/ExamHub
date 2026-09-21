import type {ReactNode} from 'react'
import {useNavigate} from 'react-router-dom'
import {ArrowLeftOutlined} from '@ant-design/icons'
import {useAuthStore} from '../stores/authStore'

type Props = {
    /** Tiêu đề trang. Bỏ qua nếu truyền `left`. */
    title?: string
    /** Dòng phụ — ReactNode để bọc được breadcrumb có link. */
    subtitle?: ReactNode
    /** Có giá trị → hiện nút ← điều hướng về path này. */
    backTo?: string
    /** Thay toàn bộ khối trái (dùng cho <Breadcrumb>). */
    left?: ReactNode
    /** Thay khối phải. Mặc định: avatar chữ cái đầu của người dùng. */
    right?: ReactNode
}

function UserAvatar() {
    const user = useAuthStore(s => s.user)
    const name = user?.displayName ?? user?.userName ?? 'A'
    return <div className="top-bar-avatar">{name.charAt(0).toUpperCase()}</div>
}

export default function PageHeader({title, subtitle, backTo, left, right}: Props) {
    const navigate = useNavigate()

    return (
        <div className="top-bar">
            {left ?? (
                <div className="flex items-center gap-3">
                    {backTo && (
                        <button className="text-gray-500 hover:text-gray-800"
                                aria-label="Quay lại"
                                onClick={() => navigate(backTo)}>
                            <ArrowLeftOutlined/>
                        </button>
                    )}
                    <div>
                        {title && <p className="top-bar-title">{title}</p>}
                        {subtitle && <p className="top-bar-subtitle">{subtitle}</p>}
                    </div>
                </div>
            )}
            {right ?? <UserAvatar/>}
        </div>
    )
}
