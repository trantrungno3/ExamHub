import {useCallback, useMemo, useState} from 'react'
import type {TableColumnsType} from 'antd'
import {Button, Input, Modal, Popconfirm, Switch, Table, Tag, Tooltip} from 'antd'
import {BookOutlined, DeleteOutlined, EditOutlined, KeyOutlined, PlusOutlined, SearchOutlined, TeamOutlined, UploadOutlined} from '@ant-design/icons'
import {message} from 'antd'
import {userService} from '../../services/userService'
import {ROLE_COLOR, ROLE_LABEL} from '../../constants'
import {statusCode} from '../../services/requestService'
import {UserFormModal} from './UserFormModal'
import {ResetPasswordModal} from './ResetPasswordModal'
import {RolesModal} from './RolesModal'
import {TeacherSubjectsModal} from './TeacherSubjectsModal'
import {UserBulkImportModal} from './UserBulkImportModal'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {useInvalidateUsers, useUsersQuery} from '../../hooks/queries/useUsers'

type ModalState =
    | {type: 'none'}
    | {type: 'form'; record: UserResponse | null}
    | {type: 'password'; record: UserResponse}
    | {type: 'roles'; record: UserResponse}
    | {type: 'subjects'; record: UserResponse}

export default function UserPage() {
    const {data, isLoading: loading} = useUsersQuery()
    const invalidateUsers = useInvalidateUsers()
    const [search, setSearch] = useState('')
    const [modal, setModal] = useState<ModalState>({type: 'none'})
    const [lockingId, setLockingId] = useState<string | null>(null)
    const [importOpen, setImportOpen] = useState(false)

    // Làm ấm cache môn học / cấp lớp để modal Phân công môn học mở là hiện bảng ngay.
    useSubjectsQuery()
    useGradeLevelsListQuery()

    const filtered = useMemo(
        () => (data ?? []).filter(u =>
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
            invalidateUsers()
            return true
        } catch { message.error('Có lỗi xảy ra'); return false }
    }, [modal, invalidateUsers])

    const removeUser = useCallback(async (id: string, force: boolean) => {
        try {
            return await userService.remove(id, force)
        } catch {
            message.error('Không thể xóa người dùng')
            return null
        }
    }, [])

    const requestDelete = useCallback(async (id: string) => {
        const res = await removeUser(id, false)
        if (!res) return
        if (res.status === statusCode.Conflict) {
            Modal.confirm({
                title: 'Đang được sử dụng',
                content: res.message,
                okText: 'Xoá bắt buộc',
                okType: 'danger',
                cancelText: 'Hủy',
                onOk: async () => {
                    const forced = await removeUser(id, true)
                    if (forced?.status === statusCode.Deleted) {
                        message.success('Đã xóa người dùng')
                        invalidateUsers()
                    } else if (forced) {
                        message.error(forced.message || 'Không thể xóa người dùng')
                    }
                },
            })
            return
        }
        if (res.status !== statusCode.Deleted) {
            message.error(res.message || 'Không thể xóa người dùng')
            return
        }
        message.success('Đã xóa người dùng')
        invalidateUsers()
    }, [removeUser, invalidateUsers])

    const handleLockToggle = useCallback(async (record: UserResponse) => {
        setLockingId(record.id)
        try {
            const next = !record.lockoutEnabled
            await userService.setLock(record.id, next)
            message.success(next ? 'Đã khóa tài khoản' : 'Đã mở khóa tài khoản')
            invalidateUsers()
        } catch { message.error('Có lỗi xảy ra') }
        finally { setLockingId(null) }
    }, [invalidateUsers])

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
            invalidateUsers()
            return true
        } catch { message.error('Có lỗi xảy ra'); return false }
    }, [modal, invalidateUsers])

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
                ? roles.map(r => <Tag key={r} color={ROLE_COLOR[r] ?? 'default'}>{ROLE_LABEL[r] ?? r}</Tag>)
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
                        onConfirm={() => requestDelete(r.id)}
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
                            Hiển thị {filtered.length} trong tổng số {(data ?? []).length} người dùng
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
            {modal.type === 'roles' && (
                <RolesModal
                    userName={modal.record.userName}
                    currentRoles={modal.record.roles}
                    onClose={() => setModal({type: 'none'})}
                    onSave={handleSetRoles}
                />
            )}
            {modal.type === 'subjects' && (
                <TeacherSubjectsModal
                    userId={modal.record.id}
                    userName={modal.record.userName}
                    onClose={() => setModal({type: 'none'})}
                />
            )}
            <UserBulkImportModal
                open={importOpen}
                onClose={() => setImportOpen(false)}
                onImported={invalidateUsers}
            />
        </div>
    )
}
