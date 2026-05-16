import {useMemo, useState} from 'react'
import {Button, Input, Popconfirm, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, SearchOutlined} from '@ant-design/icons'
import {questionTypeService} from '../../../services/questionTypeService'
import {QuestionTypeFormModal} from './QuestionTypeFormModal'
import {useCategoryTab} from '../../../hooks/useCategoryTab'

export function QuestionTypeTab() {
    const [search, setSearch] = useState('')
    const {data, loading, modalOpen, editing, handleSave, handleDelete, openCreate, openEdit, closeModal} =
        useCategoryTab(questionTypeService, 'loại câu hỏi')

    const filtered = useMemo(
        () => data.filter(q =>
            q.name.toLowerCase().includes(search.toLowerCase())
            || q.code.toLowerCase().includes(search.toLowerCase()),
        ),
        [data, search],
    )

    const columns: TableColumnsType<QuestionType> = [
        {
            title: 'ID', dataIndex: 'id', key: 'id', width: 60,
            render: v => <span className="text-gray-400">{v}</span>,
        },
        {
            title: 'Mã (code)', dataIndex: 'code', key: 'code',
            render: v => <span className="badge bg-indigo-50 text-indigo-700">{v}</span>,
        },
        {
            title: 'Tên loại câu hỏi', dataIndex: 'name', key: 'name',
            render: name => <span className="font-medium">{name}</span>,
        },
        {
            title: 'Mô tả', dataIndex: 'description', key: 'description',
            render: v => <span className="text-gray-500">{v ?? '—'}</span>,
        },
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
            render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 120,
            render: (_, record) => (
                <div className="flex gap-2">
                    <button className="btn-edit" onClick={() => openEdit(record)}>Sửa</button>
                    <Popconfirm
                        title="Xóa loại câu hỏi này?"
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
                    placeholder="Tìm loại câu hỏi..."
                    style={{width: 224}}
                />
                <Button type="primary" icon={<PlusOutlined/>} onClick={openCreate}>
                    Thêm loại câu hỏi
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
                            Hiển thị {filtered.length} trong tổng số {data.length} loại câu hỏi
                        </span>
                    )}
                />
            </div>

            <QuestionTypeFormModal
                open={modalOpen}
                record={editing}
                onClose={closeModal}
                onSave={handleSave}
            />
        </div>
    )
}
