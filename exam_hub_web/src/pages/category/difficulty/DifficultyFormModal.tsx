import {useEffect, useState} from 'react'
import {Form, Input, InputNumber, Modal, Switch} from 'antd'

type Props = {
    open: boolean
    record: DifficultyLevel | null
    onClose: () => void
    onSave: (body: DifficultyLevelBody) => Promise<boolean>
}

export function DifficultyFormModal({open, record, onClose, onSave}: Props) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)
    const isEdit = !!record

    useEffect(() => {
        if (open) {
            form.setFieldsValue(
                record
                    ? {
                        code: record.code,
                        name: record.name,
                        scoreWeight: record.scoreWeight,
                        sortOrder: record.sortOrder,
                        isActive: record.isActive
                    }
                    : {code: '', name: '', scoreWeight: 1.0, sortOrder: undefined, isActive: true}
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
            title={isEdit ? 'Sửa độ khó' : 'Thêm độ khó'}
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
                        code: record.code,
                        name: record.name,
                        scoreWeight: record.scoreWeight,
                        sortOrder: record.sortOrder,
                        isActive: record.isActive
                    }
                    : {code: '', name: '', scoreWeight: 1.0, sortOrder: undefined, isActive: true}
                }
            >
                <Form.Item
                    label="Mã (code)"
                    name="code"
                    rules={[{required: true, message: 'Vui lòng nhập mã'}]}
                >
                    <Input placeholder="VD: easy"/>
                </Form.Item>
                <Form.Item
                    label="Tên (name)"
                    name="name"
                    rules={[{required: true, message: 'Vui lòng nhập tên'}]}
                >
                    <Input placeholder="VD: Dễ"/>
                </Form.Item>
                <Form.Item
                    label="Hệ số (score_weight)"
                    name="scoreWeight"
                    rules={[{required: true, message: 'Vui lòng nhập hệ số'}]}
                >
                    <InputNumber min={0.1} step={0.1} precision={2} className="w-full" placeholder="VD: 1.00"/>
                </Form.Item>
                <Form.Item label="Thứ tự (sort_order)" name="sortOrder">
                    <InputNumber min={1} className="w-full" placeholder="VD: 1"/>
                </Form.Item>
                <Form.Item label="Trạng thái" name="isActive" valuePropName="checked">
                    <Switch checkedChildren="Hoạt động" unCheckedChildren="Tắt"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
