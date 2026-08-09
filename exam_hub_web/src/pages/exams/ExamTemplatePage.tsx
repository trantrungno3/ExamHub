import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import type {TableColumnsType} from 'antd'
import {Button, Empty, Input, Popconfirm, Select, Table} from 'antd'
import {
    BarsOutlined,
    CheckCircleFilled,
    CheckOutlined,
    CloseOutlined,
    DatabaseOutlined,
    PlusOutlined,
    SearchOutlined,
    ThunderboltFilled,
    ThunderboltOutlined,
} from '@ant-design/icons'
import {
    useDeleteExamTemplateMutation,
    useExamTemplatesByGradeQuery,
    useExamTemplateStatsQuery,
} from '../../hooks/queries/useExamTemplates'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {StatusTag} from '../../components/StatusTag'

function StatCard({label, value, icon, color, bg}: {
    label: string; value?: number; icon: React.ReactNode; color: string; bg: string
}) {
    return (
        <div className="flex-1 bg-white rounded-xl border p-4 flex items-center gap-3" style={{borderColor: '#eceef2'}}>
            <div className="w-10 h-10 rounded-lg flex items-center justify-center text-[18px]" style={{background: bg, color}}>
                {icon}
            </div>
            <div>
                <div className="text-[22px] font-bold leading-tight" style={{color: '#191d27'}}>
                    {value != null ? value.toLocaleString('vi-VN') : '—'}
                </div>
                <div className="text-[12px]" style={{color: '#6f7788'}}>{label}</div>
            </div>
        </div>
    )
}

function BoolIcon({on}: {on?: boolean}) {
    return on
        ? <CheckOutlined style={{color: '#1ea375'}}/>
        : <CloseOutlined style={{color: '#c4cad3'}}/>
}

export default function ExamTemplatePage() {
    const navigate = useNavigate()
    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()
    const stats = useExamTemplateStatsQuery()

    const [gradeId, setGradeId] = useState<number>()
    const [subjectId, setSubjectId] = useState<number>()
    const [search, setSearch] = useState('')

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
        {
            title: 'Tên mẫu đề', dataIndex: 'title', key: 'title',
            render: (v, t) => (
                <span className="flex items-center gap-2">
                    <span className="w-2 h-2 rounded-full inline-block shrink-0"
                          style={{background: t.isActive ? '#1ea375' : '#c4cad3'}}/>
                    <span className="font-medium" style={{color: '#1d2129'}}>{v}</span>
                </span>
            ),
        },
        {
            title: 'Lớp', dataIndex: 'gradeLevelName', key: 'gradeLevelName', width: 90,
            render: v => v
                ? <span className="inline-flex items-center rounded-md px-2 py-0.5 text-[12px] font-medium"
                        style={{background: '#eef0f3', color: '#6f7788'}}>{v}</span>
                : '—',
        },
        {title: 'Môn', dataIndex: 'subjectName', key: 'subjectName', width: 120, render: v => v ?? '—'},
        {title: 'TG', dataIndex: 'durationMinutes', key: 'durationMinutes', width: 70, render: v => `${v}'`},
        {title: 'Câu', dataIndex: 'totalQuestions', key: 'totalQuestions', width: 70, align: 'center', render: v => v ?? '—'},
        {title: 'Điểm', dataIndex: 'totalScore', key: 'totalScore', width: 70, align: 'center'},
        {
            title: 'Đảo', dataIndex: 'shuffleQuestions', key: 'shuffleQuestions', width: 70, align: 'center',
            render: v => <BoolIcon on={v}/>,
        },
        {
            title: 'Chống trùng', dataIndex: 'preventDuplicate', key: 'preventDuplicate', width: 110, align: 'center',
            render: v => <BoolIcon on={v}/>,
        },
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 110,
            render: v => <StatusTag status={v ? 'success' : 'default'} label={v ? 'Hoạt động' : 'Ẩn'}/>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 210, fixed: 'right',
            render: (_, t) => (
                <div className="flex gap-2 items-center">
                    <button className="btn-edit" onClick={() => navigate(`/app/exams/${t.id}/edit`)}>Sửa</button>
                    <button className="text-[13px] hover:underline flex items-center gap-1" style={{color: '#1ea375'}}
                            onClick={() => navigate(`/app/generate?templateId=${t.id}`)}>
                        <ThunderboltOutlined/> Sinh đề
                    </button>
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
                    <p className="top-bar-subtitle">Cấu hình cấu trúc đề thi để sinh đề tự động</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                {/* Stat cards */}
                <div className="flex gap-4 flex-wrap">
                    <StatCard label="Tổng mẫu" value={stats.data?.totalTemplates} icon={<DatabaseOutlined/>} color="#3a74f5" bg="#eef1ff"/>
                    <StatCard label="Đang dùng" value={stats.data?.activeTemplates} icon={<CheckCircleFilled/>} color="#1ea375" bg="#e7f7ef"/>
                    <StatCard label="Tổng đề sinh" value={stats.data?.totalExamsGenerated} icon={<ThunderboltFilled/>} color="#8b5cf6" bg="#f3ecfe"/>
                    <StatCard label="Trung bình câu" value={stats.data?.avgQuestions} icon={<BarsOutlined/>} color="#d98a00" bg="#fff4e5"/>
                </div>

                <div className="flex items-center gap-2 flex-wrap">
                    <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm mẫu đề..."
                           style={{width: 220}} allowClear value={search} onChange={e => setSearch(e.target.value)}/>
                    <Select placeholder="Chọn cấp lớp" style={{width: 150}}
                            value={effectiveGradeId} onChange={setGradeId}
                            options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                    <Select placeholder="Môn học" allowClear showSearch optionFilterProp="label" style={{width: 170}}
                            value={subjectId} onChange={setSubjectId}
                            options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                    <Button type="primary" icon={<PlusOutlined/>} className="ml-auto"
                            onClick={() => navigate('/app/exams/create')}>
                        Tạo mẫu đề thi
                    </Button>
                </div>

                <div className="section-card shrink-0">
                    <Table
                        columns={columns}
                        dataSource={filtered}
                        rowKey="id"
                        loading={isLoading}
                        scroll={{x: 900}}
                        locale={{emptyText: <Empty description="Chưa có mẫu đề cho cấp lớp này"/>}}
                        pagination={{pageSize: 10, showTotal: total => `Hiển thị ${filtered.length} trong tổng số ${total} mẫu đề`}}
                    />
                </div>
            </div>
        </>
    )
}
