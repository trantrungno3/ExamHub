import {useState} from 'react'
import {Checkbox, Modal, Space} from 'antd'
import {AVAILABLE_ROLES} from '../../services/userService'
import {ROLE_LABEL} from '../../constants'

type Props = {
    userName: string | null
    currentRoles: string[]
    onClose: () => void
    onSave: (body: SetRolesRequest) => Promise<boolean>
}

export function RolesModal({userName, currentRoles, onClose, onSave}: Readonly<Props>) {
    const [selected, setSelected] = useState<string[]>(() => [...currentRoles])
    const [saving, setSaving] = useState(false)

    const handleOk = async () => {
        setSaving(true)
        const ok = await onSave({roles: selected})
        setSaving(false)
        if (ok) onClose()
    }

    return (
        <Modal
            title={`Phân quyền — ${userName ?? ''}`}
            open
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
                            {ROLE_LABEL[role] ?? role}
                        </Checkbox>
                    ))}
                </Space>
            </div>
        </Modal>
    )
}
