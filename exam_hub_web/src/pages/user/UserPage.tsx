import {useCallback, useEffect, useMemo, useState} from 'react'
import type {TableColumnsType} from 'antd'
import {Button, Input, Popconfirm, Switch, Table, Tag, Tooltip} from 'antd'
import {BookOutlined, DeleteOutlined, EditOutlined, KeyOutlined, PlusOutlined, SearchOutlined, TeamOutlined, UploadOutlined} from '@ant-design/icons'
import {message} from 'antd'
import {userService} from '../../services/userService'
import {UserFormModal} from './UserFormModal'
import {ResetPasswordModal} from './ResetPasswordModal'
import {RolesModal} from './RolesModal'
import {TeacherSubjectsModal} from './TeacherSubjectsModal'
import {UserBulkImportModal} from './UserBulkImportModal'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'

type ModalState =
    | {type: 'none'}
    | {type: 'form'; record: UserResponse | null}
    | {type: 'password'; record: UserResponse}
    | {type: 'roles'; record: UserResponse}
    | {type: 'subjects'; record: UserResponse}

export default function UserPage() {
    const [data, setData] = useState<UserResponse[]>([])
    const [loading, setLoading] = useState(true)
    const [search, setSearch] = useState('')
    const [modal, setModal] = useState<ModalState>({type: 'none'})
    const [lockingId, setLockingId] = useState<string | null>(null)
    const [importOpen, setImportOpen] = useState(false)

    // Làm ấm cache môn học / cấp lớp để modal Phân công môn học mở là hiện bảng ngay.
    useSubjectsQuery()
    useGradeLevelsListQuery()

    const fetchData = useCallback(() => {
        setLoading(true)
        void userService.getAll()
            .then(res => setData(res.data ?? []))
            .catch(() => message.error('Không thể tải danh sách người dùng'))
            .finally(() => setLoading(false))
    }, [])

    useEffect(() => { fetchData() }, [fetchData])

    const filtered = useMemo(
        () => data.filter(u =>
            u.displayName.toLowerCase().includes(search.toLowerCase()) ||
            (u.userName ?? '').toLowerCase().includes(search.toLowerCase()) ||
            (u.email ?? '').toLowerCase().includes(search.toLowerCase())
        ),
        [data, search],
    )

    const handleSave = useCallback(async (body: CreateUserRequest | UpdateUserRequest): Promise<boolean> => {
        try {
            const editing = modal.type === 'form' ? modal.record : null
            const res = editing
                ? await userService.update(editing.id, body as UpdateUserRequest)
                : await userService.create(body as CreateUserRequest)
            if (!res.data) { message.error(res.message || 'Có lỗi xảy ra'); return false }
            message.success(editing ? 'Cập nhật thành công' : 'Thêm người dùng thành công')
            fetchData()
            return true
        } catch { message.error('Có lỗi xảy ra'); return false }
    }, [modal, fetchData])

    const handleDelete = useCallback(async (id: string) => {
        try {
            await userService.remove(id)
            message.success('Đã xóa người dùng')
            setData(prev => prev.filter(u => u.id !== id))
        } catch { message.error('Không thể xóa người dùng') }
    }, [])

    const handleLockToggle = useCallback(async (record: UserResponse) => {
        setLockingId(record.id)
        try {
            const next = !record.lockoutEnabled
            await userService.setLock(record.id, next)
            message.success(next ? 'Đã khóa tài khoản' : 'Đã mở khóa tài khoản')
            setData(prev => prev.map(u => u.id === record.id ? {...u, lockoutEnabled: next} : u))
        } catch { message.error('Có lỗi xảy ra') }
        finally { setLockingId(null) }
    }, [])

    const handleResetPassword = useCallback(async (body: ResetPasswordRequest): Promise<boolean> => {
        if (modal.type !== 'password') return false
        try {
            await userService.resetPassword(modal.record.id, body)
            message.success('Đặt lại mật khẩu thành công')
            return true
        } catch { message.error('Có lỗi xảy ra'); return false }
    }, [modal])

    const handleSetRoles = useCallback(async (body: SetRolesRequest): Promise<boolean> => {
        if (modal.type !== 'roles') return false
        try {
            await userService.setRoles(modal.record.id, body)
            message.success('Cập nhật phân quyền thành công')
            setData(prev => prev.map(u => u.id === modal.record.id ? {...u, roles: body.roles} : u))
            return true
        } catch { message.error('Có lỗi xảy ra'); return false }
    }, [modal])

    const columns: TableColumnsType<UserResponse> = [
        {
            title: 'Tên đăng nhập', dataIndex: 'userName', key: 'userName', width: 160,
            render: v => <span className="font-mono text-sm text-gray-700">{v ?? '—'}</span>,
        },
        {
            title: 'Tên hiển thị', dataIndex: 'displayName', key: 'displayName',
            render: (name, r) => (
                <div className="flex items-center gap-2">
                    <div className="w-7 h-7 rounded-full bg-blue-500 flex items-center justify-center text-white text-xs font-bold shrink-0">
                        {name.charAt(0).toUpperCase()}
                    </div>
                    <div>
                        <div className="font-medium text-sm">{name}</div>
                        {r.email && <div className="text-xs text-gray-400">{r.email}</div>}
                    </div>
                </div>
            ),
        },
        {
            title: 'Số điện thoại', dataIndex: 'phoneNumber', key: 'phoneNumber', width: 130,
            render: v => <span className="text-gray-500 text-sm">{v ?? '—'}</span>,
        },
        {
            title: 'Giới tính', dataIndex: 'sex', key: 'sex', width: 90,
            render: v => <Tag color={v ? 'pink' : 'blue'}>{v ? 'Nữ' : 'Nam'}</Tag>,
        },
        {
            title: 'Vai trò', dataIndex: 'roles', key: 'roles', width: 200,
            render: (roles: string[]) => roles.length
                ? roles.map(r => <Tag key={r} color={r === 'Admin' ? 'red' : r === 'Teacher' ? 'orange' : 'green'}>{r}</Tag>)
                : <span className="text-gray-300 text-xs">Chưa có</span>,
        },
        {
            title: 'Trạng thái', key: 'status', width: 110,
            render: (_, r) => (
                <div className="flex flex-col gap-1">
                    {r.isDeleted && <Tag color="default">Đã xóa</Tag>}
                    <Tooltip title={r.lockoutEnabled ? 'Đang khóa — nhấn để mở' : 'Đang hoạt động — nhấn để khóa'}>
                        <Switch
                            size="small"
                            checked={!r.lockoutEnabled}
                            loading={lockingId === r.id}
                            checkedChildren="Mở"
                            unCheckedChildren="Khóa"
                            onChange={() => handleLockToggle(r)}
                        />
                    </Tooltip>
                </div>
            ),
        },
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, r) => (
                <div className="flex items-center gap-1">
                    <Tooltip title="Sửa thông tin">
                        <button className="btn-icon" onClick={() => setModal({type: 'form', record: r})}>
                            <EditOutlined/>
                        </button>
                    </Tooltip>
                    <Tooltip title="Phân quyền">
                        <button className="btn-icon" onClick={() => setModal({type: 'roles', record: r})}>
                            <TeamOutlined/>
                        </button>
                    </Tooltip>
                    {r.roles.includes('Teacher') && (
                        <Tooltip title="Phân công môn học">
                            <button className="btn-icon" onClick={() => setModal({type: 'subjects', record: r})}>
                                <BookOutlined/>
                            </button>
                        </Tooltip>
                    )}
                    <Tooltip title="Đặt lại mật khẩu">
                        <button className="btn-icon" onClick={() => setModal({type: 'password', record: r})}>
                            <KeyOutlined/>
                        </button>
                    </Tooltip>
                    <Popconfirm
                        title="Xóa người dùng này?"
                        description={`Tài khoản "${r.displayName}" sẽ bị xóa vĩnh viễn.`}
                        okText="Xóa" cancelText="Hủy"
                        okButtonProps={{danger: true}}
                        onConfirm={() => handleDelete(r.id)}
                    >
                        <Tooltip title="Xóa">
                            <button className="btn-icon btn-icon-danger"><DeleteOutlined/></button>
                        </Tooltip>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    const editingRecord = modal.type === 'form' ? modal.record : null

    return (
        <div className="flex flex-col gap-4 p-6">
            <div className="flex items-center justify-between">
                <Input
                    prefix={<SearchOutlined className="text-gray-400"/>}
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    placeholder="Tìm theo tên, tài khoản, email..."
                    style={{width: 280}}
                />
                <div className="flex items-center gap-2">
                    <Button icon={<UploadOutlined/>} onClick={() => setImportOpen(true)}>
                        Nhập từ Excel
                    </Button>
                    <Button type="primary" icon={<PlusOutlined/>} onClick={() => setModal({type: 'form', record: null})}>
                        Thêm người dùng
                    </Button>
                </div>
            </div>

            <div className="section-card shrink-0">
                <Table
                    columns={columns}
                    dataSource={filtered}
                    rowKey="id"
                    loading={loading}
                    scroll={{x: 900}}
                    pagination={{pageSize: 15, showSizeChanger: false}}
                    footer={() => (
                        <span className="text-[12px] text-gray-400">
                            Hiển thị {filtered.length} trong tổng số {data.length} người dùng
                        </span>
                    )}
                />
            </div>

            <UserFormModal
                key={editingRecord?.id ?? 'new'}
                open={modal.type === 'form'}
                record={editingRecord}
                onClose={() => setModal({type: 'none'})}
                onSave={handleSave}
            />
            <ResetPasswordModal
                open={modal.type === 'password'}
                userName={modal.type === 'password' ? modal.record.userName : null}
                onClose={() => setModal({type: 'none'})}
                onSave={handleResetPassword}
            />
            <RolesModal
                open={modal.type === 'roles'}
                userName={modal.type === 'roles' ? modal.record.userName : null}
                currentRoles={modal.type === 'roles' ? modal.record.roles : []}
                onClose={() => setModal({type: 'none'})}
                onSave={handleSetRoles}
            />
            <TeacherSubjectsModal
                open={modal.type === 'subjects'}
                userId={modal.type === 'subjects' ? modal.record.id : null}
                userName={modal.type === 'subjects' ? modal.record.userName : null}
                onClose={() => setModal({type: 'none'})}
            />
            <UserBulkImportModal
                open={importOpen}
                onClose={() => setImportOpen(false)}
                onImported={fetchData}
            />
        </div>
    )
}
