import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Empty, Input, Popconfirm, Select, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, SearchOutlined, ThunderboltOutlined} from '@ant-design/icons'
import {useDeleteExamTemplateMutation, useExamTemplatesByGradeQuery} from '../../hooks/queries/useExamTemplates'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'

export default function ExamTemplatePage() {
    const navigate = useNavigate()
    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()

    const [gradeId, setGradeId] = useState<number>()
    const [subjectId, setSubjectId] = useState<number>()
    const [search, setSearch] = useState('')

    // Mặc định cấp lớp đầu tiên (derived — tránh setState trong effect).
    const effectiveGradeId = gradeId ?? grades.data?.[0]?.id

    const {data: templates, isLoading} = useExamTemplatesByGradeQuery(effectiveGradeId)
    const deleteMutation = useDeleteExamTemplateMutation()

    const filtered = useMemo(
        () => (templates ?? []).filter(t => {
            const matchSubject = subjectId === undefined || t.subjectId === subjectId
            const matchSearch = t.title.toLowerCase().includes(search.toLowerCase())
            return matchSubject && matchSearch
        }),
        [templates, subjectId, search],
    )

    const columns: TableColumnsType<ExamTemplate> = [
        {title: 'Tên mẫu đề thi', dataIndex: 'title', key: 'title', render: v => <span className="font-medium text-gray-800">{v}</span>},
        {title: 'Lớp', dataIndex: 'gradeLevelName', key: 'gradeLevelName', width: 100, render: v => v ?? '—'},
        {title: 'Môn', dataIndex: 'subjectName', key: 'subjectName', width: 130, render: v => v ?? '—'},
        {title: 'Số câu', dataIndex: 'totalQuestions', key: 'totalQuestions', width: 90, render: v => v ?? '—'},
        {title: 'Điểm', dataIndex: 'totalScore', key: 'totalScore', width: 80},
        {title: 'Thời gian', dataIndex: 'durationMinutes', key: 'durationMinutes', width: 110, render: v => `${v} phút`},
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 110,
            render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 230, fixed: 'right',
            render: (_, t) => (
                <div className="flex gap-2 items-center">
                    <button className="text-blue-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => navigate(`/app/generate?templateId=${t.id}`)}>
                        <ThunderboltOutlined/> Sinh đề
                    </button>
                    <button className="btn-edit" onClick={() => navigate(`/app/exams/${t.id}/edit`)}>Sửa</button>
                    <Popconfirm title="Xóa mẫu đề này?" okText="Xóa" cancelText="Hủy"
                                okButtonProps={{danger: true}} onConfirm={() => deleteMutation.mutate(t.id)}>
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Mẫu đề thi</p>
                    <p className="top-bar-subtitle">Quản lý mẫu cấu trúc và sinh đề tự động</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <Select
                        placeholder="Chọn cấp lớp" style={{width: 160}}
                        value={effectiveGradeId} onChange={setGradeId}
                        options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}
                    />
                    <Select
                        placeholder="Tất cả môn" allowClear showSearch optionFilterProp="label" style={{width: 180}}
                        value={subjectId} onChange={setSubjectId}
                        options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}
                    />
                    <Input
                        prefix={<SearchOutlined className="text-gray-400"/>}
                        placeholder="Tìm mẫu đề thi..." style={{width: 220}} allowClear
                        value={search} onChange={e => setSearch(e.target.value)}
                    />
                    <Button type="primary" icon={<PlusOutlined/>} className="ml-auto"
                            onClick={() => navigate('/app/exams/create')}>
                        Tạo mẫu đề thi
                    </Button>
                </div>

                <div className="section-card">
                    <Table
                        columns={columns}
                        dataSource={filtered}
                        rowKey="id"
                        loading={isLoading}
                        scroll={{x: 900}}
                        locale={{emptyText: <Empty description="Chưa có mẫu đề cho cấp lớp này"/>}}
                        pagination={{pageSize: 10, showTotal: total => `Tổng số ${total} mẫu đề`}}
                    />
                </div>
            </div>
        </>
    )
}
