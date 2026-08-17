import {useEffect, useMemo, useState} from 'react'
import {Button, Input, message, Popconfirm, Select, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, SearchOutlined} from '@ant-design/icons'
import {subjectService} from '../../../services/subjectService'
import {gradeLevelService} from '../../../services/gradeLevelService'
import {SubjectFormModal} from './SubjectFormModal'
import {useCategoryTab} from '../../../hooks/useCategoryTab'
import {formatTimestamp} from '../../../utils/datetime'

export function SubjectTab() {
    const [gradeLevels, setGradeLevels] = useState<GradeLevel[]>([])
    const [search, setSearch] = useState('')
    const [filterGrade, setFilterGrade] = useState<number | undefined>()

    const {data, loading, modalOpen, editing, handleSave, handleDelete, openCreate, openEdit, closeModal} =
        useCategoryTab(subjectService, 'môn học')

    useEffect(() => {
        void gradeLevelService.getAll()
            .then(res => setGradeLevels(res.data ?? []))
            .catch(() => message.error('Không thể tải danh sách cấp lớp'))
    }, [])

    const gradeMap = useMemo(
        () => new Map(gradeLevels.map(g => [g.id, g.name])),
        [gradeLevels],
    )

    const filtered = useMemo(
        () => data.filter(s => {
            const matchSearch = s.name.toLowerCase().includes(search.toLowerCase())
                || s.code.toLowerCase().includes(search.toLowerCase())
            const matchGrade = filterGrade === undefined || s.gradeLevelId === filterGrade
            return matchSearch && matchGrade
        }),
        [data, search, filterGrade],
    )

    const columns: TableColumnsType<Subject> = [
        {
            title: 'ID', dataIndex: 'id', key: 'id', width: 60,
            render: v => <span className="text-gray-400">{v}</span>,
        },
        {
            title: 'Tên môn học', dataIndex: 'name', key: 'name',
            render: name => (
                <span className="inline-flex items-center gap-1.5 font-medium">
                    <span className="w-2 h-2 rounded-full bg-blue-500 inline-block"/>
                    {name}
                </span>
            ),
        },
        {
            title: 'Mã (code)', dataIndex: 'code', key: 'code',
            render: v => <span className="badge bg-blue-50 text-blue-600">{v}</span>,
        },
        {
            title: 'Cấp lớp', dataIndex: 'gradeLevelId', key: 'gradeLevelId',
            render: v => <span className="text-gray-600">{gradeMap.get(v) ?? `ID: ${v}`}</span>,
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
            title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt',
            render: v => <span className="text-gray-400">{formatTimestamp(v, 'DD/MM/YYYY')}</span>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 120,
            render: (_, record) => (
                <div className="flex gap-2">
                    <button className="btn-edit" onClick={() => openEdit(record)}>Sửa</button>
                    <Popconfirm
                        title="Xóa môn học này?"
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
            <div className="flex items-center justify-between gap-3">
                <div className="flex items-center gap-2">
                    <Input
                        prefix={<SearchOutlined className="text-gray-400"/>}
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        placeholder="Tìm môn học..."
                        style={{width: 200}}
                    />
                    <Select
                        placeholder="Tất cả cấp lớp"
                        allowClear
                        style={{width: 160}}
                        value={filterGrade}
                        onChange={setFilterGrade}
                        options={gradeLevels.map(g => ({value: g.id, label: g.name}))}
                    />
                </div>
                <Button type="primary" icon={<PlusOutlined/>} onClick={openCreate}>
                    Thêm môn học
                </Button>
            </div>

            <div className="section-card shrink-0">
                <Table
                    columns={columns}
                    dataSource={filtered}
                    rowKey="id"
                    pagination={false}
                    loading={loading}
                    scroll={{x: 700}}
                    footer={() => (
                        <span className="text-[12px] text-gray-400">
                            Hiển thị {filtered.length} trong tổng số {data.length} môn học
                        </span>
                    )}
                />
            </div>

            <SubjectFormModal
                key={editing?.id ?? 'new'}
                open={modalOpen}
                record={editing}
                gradeLevels={gradeLevels}
                onClose={closeModal}
                onSave={handleSave}
            />
        </div>
    )
}
