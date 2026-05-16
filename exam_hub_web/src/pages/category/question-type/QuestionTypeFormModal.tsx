import {useEffect, useState} from 'react'
import {Form, Input, Modal, Switch} from 'antd'

type Props = {
    open: boolean
    record: QuestionType | null
    onClose: () => void
    onSave: (body: QuestionTypeBody) => Promise<boolean>
}

export function QuestionTypeFormModal({open, record, onClose, onSave}: Props) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)
    const isEdit = !!record

    useEffect(() => {
        if (open) {
            form.setFieldsValue(
                record
                    ? {code: record.code, name: record.name, description: record.description, isActive: record.isActive}
                    : {code: '', name: '', description: '', isActive: true}
            )
        }
    }, [open, record, form])

    const handleOk = async () => {
        const values = await form.validateFields()
        setSaving(true)
        const ok = await onSave(values)
        setSaving(false)
        if (ok) {
            form.resetFields()
            onClose()
        }
    }

    return (
        <Modal
            title={isEdit ? 'Sửa loại câu hỏi' : 'Thêm loại câu hỏi'}
            open={open}
            onOk={handleOk}
            onCancel={() => {
                form.resetFields();
                onClose()
            }}
            okText={isEdit ? 'Cập nhật' : 'Thêm'}
            cancelText="Hủy"
            confirmLoading={saving}
            width={480}

        >
            <Form
                form={form}
                layout="vertical"
                className="mt-4"
                initialValues={record
                    ? {code: record.code, name: record.name, description: record.description, isActive: record.isActive}
                    : {code: '', name: '', description: '', isActive: true}
                }
            >
                <Form.Item
                    label="Mã (code)"
                    name="code"
                    rules={[{required: true, message: 'Vui lòng nhập mã'}]}
                >
                    <Input placeholder="VD: multiple_choice"/>
                </Form.Item>
                <Form.Item
                    label="Tên loại câu hỏi"
                    name="name"
                    rules={[{required: true, message: 'Vui lòng nhập tên'}]}
                >
                    <Input placeholder="VD: Trắc nghiệm"/>
                </Form.Item>
                <Form.Item label="Mô tả" name="description">
                    <Input.TextArea rows={2} placeholder="Mô tả ngắn về loại câu hỏi"/>
                </Form.Item>
                <Form.Item label="Trạng thái" name="isActive" valuePropName="checked">
                    <Switch checkedChildren="Hoạt động" unCheckedChildren="Tắt"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
