import {useEffect, useMemo, useState} from 'react'
import {Button, Input, message, Popconfirm, Select, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, SearchOutlined} from '@ant-design/icons'
import {topicService} from '../../../services/topicService'
import {subjectService} from '../../../services/subjectService'
import {gradeLevelService} from '../../../services/gradeLevelService'
import {TopicFormModal} from './TopicFormModal'
import {useCategoryTab} from '../../../hooks/useCategoryTab'

export function TopicTab() {
    const [subjects, setSubjects] = useState<Subject[]>([])
    const [gradeLevels, setGradeLevels] = useState<GradeLevel[]>([])
    const [search, setSearch] = useState('')
    const [filterSubject, setFilterSubject] = useState<number | undefined>()
    const [filterParent, setFilterParent] = useState<number | undefined>()
    const [filterGrade, setFilterGrade] = useState<number | undefined>()

    const {data, loading, modalOpen, editing, handleSave, handleDelete, openCreate, openEdit, closeModal} =
        useCategoryTab(topicService, 'chủ đề')

    useEffect(() => {
        void subjectService.getAll()
            .then(res => setSubjects(res.data ?? []))
            .catch(() => message.error('Không thể tải danh sách môn học'))
        void gradeLevelService.getAll()
            .then(res => setGradeLevels(res.data ?? []))
            .catch(() => message.error('Không thể tải danh sách cấp lớp'))
    }, [])

    const subjectById = useMemo(
        () => new Map(subjects.map(s => [s.id, s])),
        [subjects],
    )

    const gradeMap = useMemo(
        () => new Map(gradeLevels.map(g => [g.id, g.name])),
        [gradeLevels],
    )

    const topicMap = useMemo(
        () => new Map(data.map(t => [t.id, t.name])),
        [data],
    )

    // Cấp lớp của chủ đề suy ra từ môn học (topic → subject → gradeLevel).
    const gradeIdOfTopic = (t: Topic) => subjectById.get(t.subjectId)?.gradeLevelId

    // Chỉ liệt kê các chủ đề đang là cha của ít nhất một chủ đề khác.
    const parentOptions = useMemo(() => {
        const parentIds = new Set(data.filter(t => t.parentId != null).map(t => t.parentId))
        return data.filter(t => parentIds.has(t.id))
    }, [data])

    // Tên môn lặp giữa các cấp lớp -> lọc theo cấp lớp đang chọn,
    // khi chưa chọn cấp lớp thì gắn thêm tên cấp lớp để phân biệt.
    const subjectOptions = useMemo(() => {
        const list = filterGrade === undefined
            ? subjects
            : subjects.filter(s => s.gradeLevelId === filterGrade)
        return list.map(s => ({
            value: s.id,
            label: filterGrade === undefined
                ? `${s.name} · ${gradeMap.get(s.gradeLevelId) ?? ''}`.trim()
                : s.name,
        }))
    }, [subjects, filterGrade, gradeMap])

    const filtered = useMemo(
        () => data.filter(t => {
            const matchSearch = t.name.toLowerCase().includes(search.toLowerCase())
            const matchSubject = filterSubject === undefined || t.subjectId === filterSubject
            const matchParent = filterParent === undefined || t.parentId === filterParent
            const matchGrade = filterGrade === undefined || gradeIdOfTopic(t) === filterGrade
            return matchSearch && matchSubject && matchParent && matchGrade
        }),
        [data, search, filterSubject, filterParent, filterGrade, subjectById],
    )

    const columns: TableColumnsType<Topic> = [
        {
            title: 'ID', dataIndex: 'id', key: 'id', width: 60,
            render: v => <span className="text-gray-400">{v}</span>,
        },
        {
            title: 'Tên chủ đề', dataIndex: 'name', key: 'name',
            render: (name, record) => (
                <span className="inline-flex items-center gap-1.5 font-medium">
                    {record.parentId
                        ? <span className="text-gray-300 text-xs">└</span>
                        : <span className="w-2 h-2 rounded-full bg-amber-500 inline-block"/>
                    }
                    {name}
                </span>
            ),
        },
        {
            title: 'Mã (code)', dataIndex: 'code', key: 'code',
            render: v => v
                ? <span className="badge bg-amber-50 text-amber-700">{v}</span>
                : <span className="text-gray-300">—</span>,
        },
        {
            title: 'Môn học', dataIndex: 'subjectId', key: 'subjectId',
            render: v => <span className="text-gray-600">{subjectById.get(v)?.name ?? `ID: ${v}`}</span>,
        },
        {
            title: 'Cấp lớp', key: 'gradeLevel',
            render: (_, record) => {
                const gid = gradeIdOfTopic(record)
                return gid
                    ? <span className="text-gray-600">{gradeMap.get(gid) ?? `ID: ${gid}`}</span>
                    : <span className="text-gray-300">—</span>
            },
        },
        {
            title: 'Chủ đề cha', dataIndex: 'parentId', key: 'parentId',
            render: v => v
                ? <span className="text-gray-500">{topicMap.get(v) ?? `ID: ${v}`}</span>
                : <span className="text-gray-300">—</span>,
        },
        {
            title: 'Thứ tự', dataIndex: 'sortOrder', key: 'sortOrder',
            render: v => <span className="text-gray-500">{v}</span>,
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
                        title="Xóa chủ đề này?"
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
                        placeholder="Tìm chủ đề..."
                        style={{width: 200}}
                    />
                    <Select
                        placeholder="Tất cả cấp lớp"
                        allowClear
                        style={{width: 160}}
                        value={filterGrade}
                        onChange={grade => {
                            setFilterGrade(grade)
                            setFilterSubject(undefined)
                        }}
                        options={gradeLevels.map(g => ({value: g.id, label: g.name}))}
                    />
                    <Select
                        placeholder="Tất cả môn học"
                        allowClear
                        showSearch
                        optionFilterProp="label"
                        style={{width: 180}}
                        value={filterSubject}
                        onChange={setFilterSubject}
                        options={subjectOptions}
                    />
                    <Select
                        placeholder="Tất cả chủ đề cha"
                        allowClear
                        showSearch
                        optionFilterProp="label"
                        style={{width: 200}}
                        value={filterParent}
                        onChange={setFilterParent}
                        options={parentOptions.map(t => ({value: t.id, label: t.name}))}
                    />
                </div>
                <Button type="primary" icon={<PlusOutlined/>} onClick={openCreate}>
                    Thêm chủ đề
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
                            Hiển thị {filtered.length} trong tổng số {data.length} chủ đề
                        </span>
                    )}
                />
            </div>

            <TopicFormModal
                key={editing?.id ?? 'new'}
                open={modalOpen}
                record={editing}
                subjects={subjects}
                onClose={closeModal}
                onSave={handleSave}
            />
        </div>
    )
}
