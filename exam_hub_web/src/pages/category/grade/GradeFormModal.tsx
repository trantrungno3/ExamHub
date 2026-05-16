import {useEffect, useState} from 'react'
import {Form, Input, InputNumber, Modal, Switch} from 'antd'

type Props = {
    open: boolean
    record: GradeLevel | null
    onClose: () => void
    onSave: (body: GradeLevelBody) => Promise<boolean>
}

export function GradeFormModal({open, record, onClose, onSave}: Props) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)
    const isEdit = !!record

    useEffect(() => {
        if (open) {
            form.setFieldsValue(
                record
                    ? {
                        name: record.name,
                        gradeNumber: record.gradeNumber,
                        description: record.description,
                        isActive: record.isActive
                    }
                    : {name: '', gradeNumber: undefined, description: '', isActive: true}
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
            title={isEdit ? 'Sửa cấp lớp' : 'Thêm cấp lớp'}
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
                    ? {
                        name: record.name,
                        gradeNumber: record.gradeNumber,
                        description: record.description,
                        isActive: record.isActive
                    }
                    : {name: '', gradeNumber: undefined, description: '', isActive: true}
                }
            >
                <Form.Item
                    label="Tên cấp lớp"
                    name="name"
                    rules={[{required: true, message: 'Vui lòng nhập tên'}]}
                >
                    <Input placeholder="VD: Lớp 10"/>
                </Form.Item>
                <Form.Item
                    label="Số lớp (grade_number)"
                    name="gradeNumber"
                    rules={[{required: true, message: 'Vui lòng nhập số lớp'}]}
                >
                    <InputNumber min={1} max={12} className="w-full" placeholder="1 – 12"/>
                </Form.Item>
                <Form.Item label="Mô tả" name="description">
                    <Input.TextArea rows={2} placeholder="VD: Cấp THPT"/>
                </Form.Item>
                <Form.Item label="Trạng thái" name="isActive" valuePropName="checked">
                    <Switch checkedChildren="Hoạt động" unCheckedChildren="Tắt"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
