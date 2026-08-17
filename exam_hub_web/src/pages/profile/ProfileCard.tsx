import {useState} from 'react'
import {Button, Spin, Tag} from 'antd'
import {EditOutlined, KeyOutlined, UserOutlined} from '@ant-design/icons'
import {useProfileQuery} from '../../hooks/queries/useProfile'
import {useAuth} from '../../AuthProvider'
import {EditProfileModal} from './EditProfileModal'
import {ChangePasswordModal} from './ChangePasswordModal'
import {ROLE_COLOR, ROLE_LABEL} from '../../constants'

/** Thẻ thông tin tài khoản dùng chung cho cả 3 màn profile. */
export function ProfileCard() {
    const {data: info, isLoading} = useProfileQuery()
    const {user} = useAuth()
    const [editOpen, setEditOpen] = useState(false)
    const [pwOpen, setPwOpen] = useState(false)

    const roles = info?.roles?.length ? info.roles : (user?.roles ?? [])

    return (
        <div className="section-card p-6">
            <div className="flex items-start justify-between">
                <div className="flex items-center gap-4">
                    <div className="w-16 h-16 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-2xl">
                        <UserOutlined/>
                    </div>
                    <div>
                        {isLoading ? <Spin/> : (
                            <>
                                <p className="text-lg font-semibold text-gray-800">
                                    {info?.displayName ?? user?.displayName ?? user?.userName}
                                </p>
                                <p className="text-sm text-gray-500">@{info?.userName ?? user?.userName}</p>
                                <div className="flex gap-1 mt-1">
                                    {roles.map(r => <Tag key={r} color={ROLE_COLOR[r] ?? 'default'}>{ROLE_LABEL[r] ?? r}</Tag>)}
                                </div>
                            </>
                        )}
                    </div>
                </div>
                <div className="flex gap-2">
                    <Button icon={<EditOutlined/>} onClick={() => setEditOpen(true)}>Sửa</Button>
                    <Button icon={<KeyOutlined/>} onClick={() => setPwOpen(true)}>Đổi mật khẩu</Button>
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4 mt-6 text-sm">
                <Field label="Tên hiển thị" value={info?.displayName ?? user?.displayName}/>
                <Field label="Tên đăng nhập" value={info?.userName ?? user?.userName}/>
                <Field label="Email" value={info?.email}/>
                <Field label="Số điện thoại" value={info?.phoneNumber}/>
                <Field label="Vai trò" value={roles.join(', ')}/>
            </div>

            <EditProfileModal open={editOpen} current={info} onClose={() => setEditOpen(false)}/>
            <ChangePasswordModal open={pwOpen} onClose={() => setPwOpen(false)}/>
        </div>
    )
}

function Field({label, value}: {label: string; value?: string | null}) {
    return (
        <div>
            <p className="text-gray-400 text-xs">{label}</p>
            <p className="text-gray-800">{value || '—'}</p>
        </div>
    )
}
