import {useEffect, useState} from 'react'
import {Form, Input, Modal, Radio} from 'antd'

type Props = {
    open: boolean
    record: UserResponse | null
    onClose: () => void
    onSave: (body: CreateUserRequest | UpdateUserRequest) => Promise<boolean>
}

export function UserFormModal({open, record, onClose, onSave}: Readonly<Props>) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)
    const isEdit = !!record

    useEffect(() => {
        if (!open) return
        if (record) {
            form.setFieldsValue({
                displayName: record.displayName,
                email:       record.email ?? '',
                phoneNumber: record.phoneNumber ?? '',
                sex:         record.sex,
                address:     record.address ?? '',
                description: record.description ?? '',
            })
        } else {
            form.resetFields()
            form.setFieldsValue({sex: false})
        }
    }, [open, record, form])

    const handleOk = async () => {
        const values = await form.validateFields()
        setSaving(true)
        const ok = await onSave(values)
        setSaving(false)
        if (ok) onClose()
    }

    return (
        <Modal
            title={isEdit ? 'Sửa người dùng' : 'Thêm người dùng'}
            open={open}
            onOk={handleOk}
            onCancel={() => { form.resetFields(); onClose() }}
            okText={isEdit ? 'Cập nhật' : 'Thêm'}
            cancelText="Hủy"
            confirmLoading={saving}
            width={520}
            destroyOnHide
        >
            <Form form={form} layout="vertical" className="mt-4">
                {!isEdit && (
                    <>
                        <Form.Item
                            label="Tên đăng nhập"
                            name="userName"
                            rules={[{required: true, message: 'Vui lòng nhập tên đăng nhập'}]}
                        >
                            <Input placeholder="VD: nguyenvana"/>
                        </Form.Item>
                        <Form.Item
                            label="Mật khẩu"
                            name="password"
                            rules={[{required: true, message: 'Vui lòng nhập mật khẩu'}, {min: 6, message: 'Tối thiểu 6 ký tự'}]}
                        >
                            <Input.Password placeholder="Mật khẩu"/>
                        </Form.Item>
                    </>
                )}
                <Form.Item
                    label="Tên hiển thị"
                    name="displayName"
                    rules={[{required: true, message: 'Vui lòng nhập tên hiển thị'}]}
                >
                    <Input placeholder="VD: Nguyễn Văn A"/>
                </Form.Item>
                <div className="grid grid-cols-2 gap-x-4">
                    <Form.Item label="Email" name="email">
                        <Input placeholder="email@example.com"/>
                    </Form.Item>
                    <Form.Item label="Số điện thoại" name="phoneNumber">
                        <Input placeholder="0909..."/>
                    </Form.Item>
                </div>
                <Form.Item label="Giới tính" name="sex">
                    <Radio.Group>
                        <Radio value={false}>Nam</Radio>
                        <Radio value={true}>Nữ</Radio>
                    </Radio.Group>
                </Form.Item>
                {isEdit && (
                    <>
                        <Form.Item label="Địa chỉ" name="address">
                            <Input placeholder="VD: 123 Nguyễn Trãi, TP.HCM"/>
                        </Form.Item>
                        <Form.Item label="Mô tả" name="description">
                            <Input.TextArea rows={2} placeholder="Ghi chú về người dùng..."/>
                        </Form.Item>
                    </>
                )}
            </Form>
        </Modal>
    )
}
