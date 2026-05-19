import {useEffect, useState} from 'react'
import {Checkbox, Modal, Space} from 'antd'
import {AVAILABLE_ROLES} from '../../services/userService'

type Props = {
    open: boolean
    userName: string | null
    currentRoles: string[]
    onClose: () => void
    onSave: (body: SetRolesRequest) => Promise<boolean>
}

export function RolesModal({open, userName, currentRoles, onClose, onSave}: Readonly<Props>) {
    const [selected, setSelected] = useState<string[]>([])
    const [saving, setSaving] = useState(false)

    useEffect(() => {
        if (open) setSelected([...currentRoles])
    }, [open, currentRoles])

    const handleOk = async () => {
        setSaving(true)
        const ok = await onSave({roles: selected})
        setSaving(false)
        if (ok) onClose()
    }

    return (
        <Modal
            title={`Phân quyền — ${userName ?? ''}`}
            open={open}
            onOk={handleOk}
            onCancel={onClose}
            okText="Lưu"
            cancelText="Hủy"
            confirmLoading={saving}
            width={360}
            destroyOnHidden
        >
            <div className="mt-4">
                <Space direction="vertical">
                    {AVAILABLE_ROLES.map(role => (
                        <Checkbox
                            key={role}
                            checked={selected.includes(role)}
                            onChange={e => {
                                setSelected(prev =>
                                    e.target.checked ? [...prev, role] : prev.filter(r => r !== role)
                                )
                            }}
                        >
                            {role}
                        </Checkbox>
                    ))}
                </Space>
            </div>
        </Modal>
    )
}
