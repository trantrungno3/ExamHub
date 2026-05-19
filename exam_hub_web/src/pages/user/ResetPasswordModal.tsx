import {useState} from 'react'
import {Form, Input, Modal} from 'antd'

type Props = {
    open: boolean
    userName: string | null
    onClose: () => void
    onSave: (body: ResetPasswordRequest) => Promise<boolean>
}

export function ResetPasswordModal({open, userName, onClose, onSave}: Readonly<Props>) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)

    const handleOk = async () => {
        const values = await form.validateFields()
        setSaving(true)
        const ok = await onSave({newPassword: values.newPassword})
        setSaving(false)
        if (ok) { form.resetFields(); onClose() }
    }

    return (
        <Modal
            title={`Đặt lại mật khẩu — ${userName ?? ''}`}
            open={open}
            onOk={handleOk}
            onCancel={() => { form.resetFields(); onClose() }}
            okText="Đặt lại"
            cancelText="Hủy"
            confirmLoading={saving}
            width={420}
            destroyOnHidden
        >
            <Form form={form} layout="vertical" className="mt-4">
                <Form.Item
                    label="Mật khẩu mới"
                    name="newPassword"
                    rules={[
                        {required: true, message: 'Vui lòng nhập mật khẩu mới'},
                        {min: 6, message: 'Tối thiểu 6 ký tự'},
                    ]}
                >
                    <Input.Password placeholder="Mật khẩu mới"/>
                </Form.Item>
                <Form.Item
                    label="Xác nhận mật khẩu"
                    name="confirmPassword"
                    dependencies={['newPassword']}
                    rules={[
                        {required: true, message: 'Vui lòng xác nhận mật khẩu'},
                        ({getFieldValue}) => ({
                            validator(_, value) {
                                if (!value || getFieldValue('newPassword') === value) return Promise.resolve()
                                return Promise.reject(new Error('Mật khẩu không khớp'))
                            },
                        }),
                    ]}
                >
                    <Input.Password placeholder="Nhập lại mật khẩu"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
