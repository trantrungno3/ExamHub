import {Button, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, WarningOutlined} from '@ant-design/icons'
import {difficultyLevelService} from '../../../services/difficultyLevelService'
import {DifficultyFormModal} from './DifficultyFormModal'
import {useCategoryTab} from '../../../hooks/useCategoryTab'

const BADGE_BY_CODE: Record<string, string> = {
    easy:      'bg-green-100 text-green-700',
    medium:    'bg-yellow-100 text-yellow-700',
    hard:      'bg-red-100 text-red-600',
    very_hard: 'bg-purple-100 text-purple-700',
}

function badge(code: string) {
    return BADGE_BY_CODE[code] ?? 'bg-gray-100 text-gray-700'
}

export function DifficultyTab() {
    const {data, loading, modalOpen, editing, handleSave, openCreate, openEdit, closeModal} =
        useCategoryTab(difficultyLevelService, 'độ khó')

    const columns: TableColumnsType<DifficultyLevel> = [
        {
            title: 'ID', dataIndex: 'id', key: 'id', width: 60,
            render: v => <span className="text-gray-400">{v}</span>,
        },
        {
            title: 'Mã (code)', dataIndex: 'code', key: 'code',
            render: code => <span className={`badge ${badge(code)}`}>{code}</span>,
        },
        {
            title: 'Tên (name)', dataIndex: 'name', key: 'name',
            render: (name, record) => <span className={`badge ${badge(record.code)}`}>{name}</span>,
        },
        {
            title: 'Hệ số (score_weight)', dataIndex: 'scoreWeight', key: 'scoreWeight',
            render: v => <span className="font-bold text-gray-800">×{Number(v).toFixed(2)}</span>,
        },
        {
            title: 'Thứ tự (sort_order)', dataIndex: 'sortOrder', key: 'sortOrder',
            render: v => <span className="text-gray-500">Ưu tiên {v}</span>,
        },
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
            render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 80,
            render: (_, record) => (
                <button className="btn-edit" onClick={() => openEdit(record)}>Sửa</button>
            ),
        },
    ]

    return (
        <div className="flex flex-col gap-4 p-6">
            <div className="flex items-start justify-between gap-4 bg-amber-50 border border-amber-200 rounded-xl px-5 py-3.5">
                <p className="text-[13px] text-amber-800 font-medium flex items-center gap-2">
                    <WarningOutlined className="text-amber-500"/>
                    Dữ liệu này ảnh hưởng đến toàn bộ thuật toán sinh đề. Chỉnh sửa cẩn thận.
                </p>
                <Button type="primary" icon={<PlusOutlined/>} className="shrink-0" onClick={openCreate}>
                    Thêm độ khó
                </Button>
            </div>

            <div className="section-card">
                <Table
                    columns={columns}
                    dataSource={data}
                    rowKey="id"
                    pagination={false}
                    loading={loading}
                    footer={() => (
                        <span className="text-[12px] text-gray-400">
                            {data.length} mức độ khó — Seed data mặc định hệ thống
                        </span>
                    )}
                />
            </div>

            <DifficultyFormModal
                open={modalOpen}
                record={editing}
                onClose={closeModal}
                onSave={handleSave}
            />
        </div>
    )
}
