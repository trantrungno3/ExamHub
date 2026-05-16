import {useCallback, useEffect, useState} from 'react'
import {Form, Input, InputNumber, Modal, Select, Switch} from 'antd'
import {topicService} from '../../../services/topicService'

type Props = {
    open: boolean
    record: Topic | null
    subjects: Subject[]
    onClose: () => void
    onSave: (body: TopicBody) => Promise<boolean>
}

export function TopicFormModal({open, record, subjects, onClose, onSave}: Props) {
    const [form] = Form.useForm()
    const [saving, setSaving] = useState(false)
    const [parentOptions, setParentOptions] = useState<Topic[]>([])
    const isEdit = !!record

    const loadParentOptions = useCallback(async (subjectId: number, excludeId?: number) => {
        const res = await topicService.getBySubject(subjectId)
        const items = res.data ?? []
        setParentOptions(excludeId ? items.filter(t => t.id !== excludeId) : items)
    }, [])

    // Only load parent topic options (async — no synchronous setState)
    useEffect(() => {
        if (open) {
            form.setFieldsValue(
                record
                    ? {
                        subjectId: record.subjectId,
                        parentId: record.parentId,
                        name: record.name,
                        code: record.code,
                        sortOrder: record.sortOrder,
                        description: record.description,
                        isActive: record.isActive
                    }
                    : {
                        subjectId: undefined,
                        parentId: undefined,
                        name: '',
                        code: '',
                        sortOrder: 1,
                        description: '',
                        isActive: true
                    }
            )
            if (record?.subjectId) void loadParentOptions(record.subjectId, record.id)
        }
    }, [open, record, loadParentOptions])

    const onSubjectChange = (subjectId: number) => {
        form.setFieldValue('parentId', undefined)
        setParentOptions([])
        void loadParentOptions(subjectId)
    }

    const handleOk = async () => {
        const values = await form.validateFields()
        setSaving(true)
        const ok = await onSave(values)
        setSaving(false)
        if (ok) {
            form.resetFields()
            setParentOptions([])
            onClose()
        }
    }

    return (
        <Modal
            title={isEdit ? 'Sửa chủ đề' : 'Thêm chủ đề'}
            open={open}
            onOk={handleOk}
            onCancel={() => {
                form.resetFields();
                setParentOptions([]);
                onClose()
            }}
            okText={isEdit ? 'Cập nhật' : 'Thêm'}
            cancelText="Hủy"
            confirmLoading={saving}
            width={520}

        >
            <Form
                form={form}
                layout="vertical"
                className="mt-4"
                initialValues={record
                    ? {
                        subjectId: record.subjectId,
                        parentId: record.parentId,
                        name: record.name,
                        code: record.code,
                        sortOrder: record.sortOrder,
                        description: record.description,
                        isActive: record.isActive
                    }
                    : {
                        subjectId: undefined,
                        parentId: undefined,
                        name: '',
                        code: '',
                        sortOrder: 1,
                        description: '',
                        isActive: true
                    }
                }
            >
                <Form.Item
                    label="Môn học"
                    name="subjectId"
                    rules={[{required: true, message: 'Vui lòng chọn môn học'}]}
                >
                    <Select
                        placeholder="Chọn môn học"
                        options={subjects.map(s => ({value: s.id, label: s.name}))}
                        onChange={onSubjectChange}
                    />
                </Form.Item>
                <Form.Item label="Chủ đề cha (tùy chọn)" name="parentId">
                    <Select
                        placeholder="Không có (chủ đề gốc)"
                        allowClear
                        options={parentOptions.map(t => ({value: t.id, label: t.name}))}
                    />
                </Form.Item>
                <Form.Item
                    label="Tên chủ đề"
                    name="name"
                    rules={[{required: true, message: 'Vui lòng nhập tên chủ đề'}]}
                >
                    <Input placeholder="VD: Đại số"/>
                </Form.Item>
                <Form.Item label="Mã (code)" name="code">
                    <Input placeholder="VD: ALG"/>
                </Form.Item>
                <Form.Item label="Thứ tự (sort_order)" name="sortOrder">
                    <InputNumber min={1} className="w-full"/>
                </Form.Item>
                <Form.Item label="Mô tả" name="description">
                    <Input.TextArea rows={2} placeholder="Mô tả ngắn"/>
                </Form.Item>
                <Form.Item label="Trạng thái" name="isActive" valuePropName="checked">
                    <Switch checkedChildren="Hoạt động" unCheckedChildren="Tắt"/>
                </Form.Item>
            </Form>
        </Modal>
    )
}
