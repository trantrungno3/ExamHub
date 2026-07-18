import {useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Form, Input, Modal, Popconfirm, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, RightOutlined} from '@ant-design/icons'
import {
    useSchoolsQuery,
    useCreateSchoolMutation,
    useUpdateSchoolMutation,
    useDeleteSchoolMutation,
} from '../../hooks/queries/useSchools'
import {statusCode} from '../../services/requestService'

export default function SchoolListPage() {
    const navigate = useNavigate()
    const {data: schools = [], isFetching} = useSchoolsQuery()
    const createMutation = useCreateSchoolMutation()
    const updateMutation = useUpdateSchoolMutation()
    const deleteMutation = useDeleteSchoolMutation()

    const [modalOpen, setModalOpen] = useState(false)
    const [editing, setEditing] = useState<School | null>(null)
    const [form] = Form.useForm<SchoolBody>()

    const openCreate = () => { setEditing(null); form.resetFields(); setModalOpen(true) }
    const openEdit = (record: School) => {
        setEditing(record)
        form.setFieldsValue({name: record.name, code: record.code, address: record.address, phone: record.phone, email: record.email, isActive: record.isActive})
        setModalOpen(true)
    }

    const handleOk = async () => {
        const values = await form.validateFields()
        const res = editing
            ? await updateMutation.mutateAsync({id: editing.id, body: values})
            : await createMutation.mutateAsync(values)
        if (res.status !== statusCode.Error) {
            setModalOpen(false)
            form.resetFields()
        }
    }

    const columns: TableColumnsType<School> = [
        {title: 'Tên trường', dataIndex: 'name', key: 'name', render: v => <span className="font-medium">{v}</span>},
        {title: 'Mã trường', dataIndex: 'code', key: 'code'},
        {title: 'Địa chỉ', dataIndex: 'address', key: 'address', render: v => v ?? '—'},
        {title: 'Email', dataIndex: 'email', key: 'email', render: v => v ?? '—'},
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
            render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 160,
            render: (_, record) => (
                <div className="flex gap-2">
                    <button className="btn-edit" onClick={() => openEdit(record)}>Sửa</button>
                    <Popconfirm title="Xóa trường này?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                        onConfirm={() => deleteMutation.mutate(record.id)}>
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                    <Button size="small" icon={<RightOutlined/>} onClick={() => navigate(`/app/schools/${record.id}`)}>
                        Chi tiết
                    </Button>
                </div>
            ),
        },
    ]

    return (
        <>
            <div className="top-bar">
                <p className="top-bar-title">Quản lý trường học</p>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                <div className="flex justify-end">
                    <Button type="primary" icon={<PlusOutlined/>} onClick={openCreate}>
                        Thêm trường
                    </Button>
                </div>

                <div className="section-card">
                    <Table
                        columns={columns}
                        dataSource={schools}
                        rowKey="id"
                        loading={isFetching}
                        pagination={false}
                        footer={() => (
                            <span className="text-[12px] text-gray-400">
                                Tổng: {schools.length} trường
                            </span>
                        )}
                    />
                </div>
            </div>

            <Modal
                title={editing ? 'Sửa trường học' : 'Thêm trường học'}
                open={modalOpen}
                onOk={handleOk}
                onCancel={() => setModalOpen(false)}
                okText={editing ? 'Lưu' : 'Thêm'}
                cancelText="Hủy"
                confirmLoading={createMutation.isPending || updateMutation.isPending}
            >
                <Form form={form} layout="vertical">
                    <Form.Item name="name" label="Tên trường" rules={[{required: true, message: 'Nhập tên trường'}]}>
                        <Input/>
                    </Form.Item>
                    <Form.Item name="code" label="Mã trường" rules={[{required: true, message: 'Nhập mã trường'}]}>
                        <Input/>
                    </Form.Item>
                    <Form.Item name="address" label="Địa chỉ">
                        <Input/>
                    </Form.Item>
                    <Form.Item name="phone" label="Điện thoại">
                        <Input/>
                    </Form.Item>
                    <Form.Item name="email" label="Email">
                        <Input/>
                    </Form.Item>
                </Form>
            </Modal>
        </>
    )
}
