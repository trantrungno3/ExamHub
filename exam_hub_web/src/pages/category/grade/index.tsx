import {useMemo, useState} from 'react'
import type {TableColumnsType} from 'antd'
import {Button, Input, Popconfirm, Table, Tag} from 'antd'
import {PlusOutlined, SearchOutlined} from '@ant-design/icons'
import {gradeLevelService} from '../../../services/gradeLevelService'
import {GradeFormModal} from './GradeFormModal'
import {useCategoryTab} from '../../../hooks/useCategoryTab'

export function GradeTab() {
    const [search, setSearch] = useState('')
    const {data, loading, modalOpen, editing, handleSave, handleDelete, openCreate, openEdit, closeModal} =
        useCategoryTab(gradeLevelService, 'cấp lớp')
    const filtered = useMemo(
        () => data.filter(g => g.name.toLowerCase().includes(search.toLowerCase())),
        [data, search],
    )

    const columns: TableColumnsType<GradeLevel> = [
        {
            title: 'ID', dataIndex: 'id', key: 'id', width: 60,
            render: v => <span className="text-gray-400">{v}</span>,
        },
        {
            title: 'Tên cấp lớp', dataIndex: 'name', key: 'name',
            render: name => (
                <span className="inline-flex items-center gap-1.5 font-medium">
                    <span className="w-2 h-2 rounded-full bg-green-500 inline-block"/>
                    {name}
                </span>
            ),
        },
        {title: 'grade_number', dataIndex: 'gradeNumber', key: 'gradeNumber'},
        {
            title: 'Mô tả', dataIndex: 'description', key: 'description',
            render: v => <span className="text-gray-500">{v ?? '—'}</span>,
        },
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
            render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>,
        },
        {
            title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt',
            render: v => <span className="text-gray-400">{v ? new Date(v).toLocaleDateString('vi-VN') : '—'}</span>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 120,
            render: (_, record) => (
                <div className="flex gap-2">
                    <button className="btn-edit" onClick={() => openEdit(record)}>Sửa</button>
                    <Popconfirm
                        title="Xóa cấp lớp này?"
                        okText="Xóa" cancelText="Hủy"
                        okButtonProps={{danger: true}}
                        onConfirm={() => handleDelete(record.id)}
                    >
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    return (
        <div className="flex flex-col gap-4 p-6">
            <div className="flex items-center justify-between">
                <Input
                    prefix={<SearchOutlined className="text-gray-400"/>}
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                    placeholder="Tìm cấp lớp..."
                    style={{width: 224}}
                />
                <Button type="primary" icon={<PlusOutlined/>} onClick={openCreate}>
                    Thêm lớp
                </Button>
            </div>

            <div className="section-card">
                <Table
                    columns={columns}
                    dataSource={filtered}
                    rowKey="id"
                    pagination={false}
                    loading={loading}
                    footer={() => (
                        <span className="text-[12px] text-gray-400">
                            Hiển thị {filtered.length} trong tổng số {data.length} cấp lớp
                        </span>
                    )}
                />
            </div>

            <GradeFormModal
                key={editing?.id ?? 'new'}
                open={modalOpen}
                record={editing}
                onClose={closeModal}
                onSave={handleSave}
            />
        </div>
    )
}
