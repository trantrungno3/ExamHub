import {useEffect, useState} from 'react'
import {Form, Input, Modal, Select, Switch} from 'antd'

type Props = {
    open: boolean
    record: Subject | null
    gradeLevels: GradeLevel[]
    onClose: () => void
    onSave: (body: SubjectBody) => Promise<boolean>
}

export function SubjectFormModal({open, record, gradeLevels, onClose, onSave}: Props) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)
    const isEdit = !!record

    useEffect(() => {
        if (open) {
            form.setFieldsValue(
                record
                    ? {
                        gradeLevelId: record.gradeLevelId,
                        name: record.name,
                        code: record.code,
                        description: record.description,
                        isActive: record.isActive
                    }
                    : {gradeLevelId: undefined, name: '', code: '', description: '', isActive: true}
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
            title={isEdit ? 'Sửa môn học' : 'Thêm môn học'}
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
                        gradeLevelId: record.gradeLevelId,
                        name: record.name,
                        code: record.code,
                        description: record.description,
                        isActive: record.isActive
                    }
                    : {gradeLevelId: undefined, name: '', code: '', description: '', isActive: true}
                }
            >
                <Form.Item
                    label="Cấp lớp"
                    name="gradeLevelId"
                    rules={[{required: true, message: 'Vui lòng chọn cấp lớp'}]}
                >
                    <Select placeholder="Chọn cấp lớp" options={gradeLevels.map(g => ({value: g.id, label: g.name}))}/>
                </Form.Item>
                <Form.Item
                    label="Tên môn học"
                    name="name"
                    rules={[{required: true, message: 'Vui lòng nhập tên môn học'}]}
                >
                    <Input placeholder="VD: Toán học"/>
                </Form.Item>
                <Form.Item
                    label="Mã môn (code)"
                    name="code"
                    rules={[{required: true, message: 'Vui lòng nhập mã môn'}]}
                >
                    <Input placeholder="VD: MATH"/>
                </Form.Item>
                <Form.Item label="Mô tả" name="description">
                    <Input.TextArea rows={2} placeholder="Mô tả ngắn về môn học"/>
                </Form.Item>
                <Form.Item label="Trạng thái" name="isActive" valuePropName="checked">
                    <Switch checkedChildren="Hoạt động" unCheckedChildren="Tắt"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
