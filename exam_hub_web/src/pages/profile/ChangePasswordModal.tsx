import {Form, Input, Modal, message} from 'antd'
import {authService} from '../../services/authService'
import {statusCode} from '../../services/requestService'

interface Props {
    open: boolean
    onClose: () => void
}

interface FormValues {
    oldPassword: string
    newPassword: string
    confirmPassword: string
}

export function ChangePasswordModal({open, onClose}: Props) {
    const [form] = Form.useForm<FormValues>()

    const handleOk = async () => {
        const values = await form.validateFields()
        const res = await authService.changePassword({
            oldPassword: values.oldPassword,
            newPassword: values.newPassword,
        })
        if (res.status === statusCode.Error || !res.data) {
            message.error(res.message || 'Đổi mật khẩu thất bại')
            return
        }
        message.success('Đổi mật khẩu thành công')
        form.resetFields()
        onClose()
    }

    return (
        <Modal title="Đổi mật khẩu" open={open} onOk={handleOk}
               onCancel={() => { form.resetFields(); onClose() }}
               okText="Đổi mật khẩu" cancelText="Hủy">
            <Form form={form} layout="vertical">
                <Form.Item name="oldPassword" label="Mật khẩu hiện tại"
                           rules={[{required: true, message: 'Nhập mật khẩu hiện tại'}]}>
                    <Input.Password/>
                </Form.Item>
                <Form.Item name="newPassword" label="Mật khẩu mới"
                           rules={[{required: true, message: 'Nhập mật khẩu mới'}, {min: 6, message: 'Tối thiểu 6 ký tự'}]}>
                    <Input.Password/>
                </Form.Item>
                <Form.Item name="confirmPassword" label="Xác nhận mật khẩu mới"
                           dependencies={['newPassword']}
                           rules={[
                               {required: true, message: 'Xác nhận mật khẩu mới'},
                               ({getFieldValue}) => ({
                                   validator(_, value) {
                                       if (!value || getFieldValue('newPassword') === value) return Promise.resolve()
                                       return Promise.reject(new Error('Mật khẩu xác nhận không khớp'))
                                   },
                               }),
                           ]}>
                    <Input.Password/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
