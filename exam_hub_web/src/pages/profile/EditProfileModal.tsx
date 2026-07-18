import {useEffect} from 'react'
import {Form, Input, Modal, message} from 'antd'
import {useQueryClient} from '@tanstack/react-query'
import {authService} from '../../services/authService'
import {statusCode} from '../../services/requestService'
import {PROFILE_KEYS} from '../../hooks/queries/useProfile'

interface Props {
    open: boolean
    current?: UserInfo | null
    onClose: () => void
}

export function EditProfileModal({open, current, onClose}: Props) {
    const qc = useQueryClient()
    const [form] = Form.useForm<UpdateProfileBody>()

    useEffect(() => {
        if (open) {
            form.setFieldsValue({
                displayName: current?.displayName ?? '',
                phoneNumber: current?.phoneNumber ?? '',
                email: current?.email ?? '',
            })
        }
    }, [open, current, form])

    const handleOk = async () => {
        const values = await form.validateFields()
        const res = await authService.updateProfile(values)
        if (res.status === statusCode.Error || !res.data) {
            message.error(res.message || 'Cập nhật thất bại')
            return
        }
        message.success('Cập nhật thông tin thành công')
        void qc.invalidateQueries({queryKey: PROFILE_KEYS.me})
        onClose()
    }

    return (
        <Modal title="Sửa thông tin cá nhân" open={open} onOk={handleOk} onCancel={onClose}
               okText="Lưu" cancelText="Hủy">
            <Form form={form} layout="vertical">
                <Form.Item name="displayName" label="Tên hiển thị" rules={[{required: true, message: 'Nhập tên hiển thị'}]}>
                    <Input/>
                </Form.Item>
                <Form.Item name="phoneNumber" label="Số điện thoại">
                    <Input/>
                </Form.Item>
                <Form.Item name="email" label="Email">
                    <Input type="email"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
