import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Dropdown, Input, Popconfirm, Select, Table, message} from 'antd'
import type {TableColumnsType} from 'antd'
import {BarChartOutlined, DownloadOutlined, EyeOutlined, SearchOutlined} from '@ant-design/icons'
import {AnalyticsDrawer} from './AnalyticsDrawer'
import {
    useDeleteExamMutation,
    useExamsQuery,
    usePublishExamMutation,
} from '../../hooks/queries/useExams'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {examService} from '../../services/examService'
import {StatusTag} from '../../components/StatusTag'
import {EXAM_STATUS_LABEL, EXAM_STATUS_VARIANT} from '../../constants'
import {ROUTES} from '../../routes/paths'

export default function ExamListPage() {
    const navigate = useNavigate()
    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()

    const [page, setPage] = useState(1)
    const [pageSize, setPageSize] = useState(20)
    const [gradeLevelId, setGradeLevelId] = useState<number>()
    const [subjectId, setSubjectId] = useState<number>()
    const [status, setStatus] = useState<ExamStatus>()
    const [keyword, setKeyword] = useState('')
    const [analyticsExamId, setAnalyticsExamId] = useState<string>()
    const [exporting, setExporting] = useState<string>()

    const query: ExamPagedQuery = useMemo(
        () => ({page, pageSize, gradeLevelId, subjectId, status, keyword}),
        [page, pageSize, gradeLevelId, subjectId, status, keyword],
    )
    const {data, isLoading} = useExamsQuery(query)
    const publish = usePublishExamMutation()
    const remove = useDeleteExamMutation()

    const handleExport = async (id: string, format: ExportFormat) => {
        setExporting(`${id}:${format}`)
        try {
            const res = await examService.export(id, format)
            if (res.data?.url) window.open(res.data.url, '_blank')
            else message.error(res.message || 'Xuất file thất bại')
        } catch {
            message.error('Xuất file thất bại')
        } finally {
            setExporting(undefined)
        }
    }

    const columns: TableColumnsType<Exam> = [
        {title: 'Tiêu đề', dataIndex: 'title', key: 'title', render: v => <span className="font-medium text-gray-800">{v}</span>},
        {title: 'Mã đề', dataIndex: 'examCode', key: 'examCode', width: 100, render: v => v ?? '—'},
        {title: 'Lớp', dataIndex: 'gradeLevelName', key: 'gradeLevelName', width: 90, render: v => v ?? '—'},
        {title: 'Môn', dataIndex: 'subjectName', key: 'subjectName', width: 120, render: v => v ?? '—'},
        {title: 'Điểm', dataIndex: 'totalScore', key: 'totalScore', width: 70},
        {
            title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 130,
            render: (v: ExamStatus) => <StatusTag status={EXAM_STATUS_VARIANT[v]} label={EXAM_STATUS_LABEL[v]}/>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 440, fixed: 'right',
            render: (_, e) => (
                <div className="flex gap-2 items-center">
                    <button className="text-blue-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => navigate(ROUTES.EXAM_DETAIL.replace(':id', e.id))}>
                        <EyeOutlined/> Xem
                    </button>
                    <button className="text-gray-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => setAnalyticsExamId(e.id)}>
                        <BarChartOutlined/> Phân tích
                    </button>
                    <Dropdown
                        menu={{
                            items: [
                                {key: 'pdf', label: 'Xuất PDF'},
                                {key: 'docx', label: 'Xuất Word'},
                            ],
                            onClick: ({key}) => void handleExport(e.id, key as ExportFormat),
                        }}
                    >
                        <button className="text-gray-600 text-sm hover:underline flex items-center gap-1">
                            <DownloadOutlined/> {exporting?.startsWith(e.id) ? 'Đang xuất...' : 'Xuất'}
                        </button>
                    </Dropdown>
                    {e.status === 'Draft' && (
                        <button className="text-green-600 text-sm hover:underline"
                                onClick={() => publish.mutate(e.id)}>Phát hành</button>
                    )}
                    <Popconfirm title="Xóa đề thi này?" okText="Xóa" cancelText="Hủy"
                                okButtonProps={{danger: true}} onConfirm={() => remove.mutate(e.id)}>
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
                    <p className="top-bar-title">Đề thi</p>
                    <p className="top-bar-subtitle">Danh sách đề thi đã sinh — xem trước & xuất file</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm đề thi..."
                           style={{width: 220}} allowClear value={keyword}
                           onChange={e => {
                               setKeyword(e.target.value)
                               setPage(1)
                           }}/>
                    <Select placeholder="Lớp" allowClear style={{width: 130}} value={gradeLevelId}
                            onChange={v => {
                                setGradeLevelId(v)
                                setPage(1)
                            }}
                            options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                    <Select placeholder="Môn" allowClear showSearch optionFilterProp="label" style={{width: 160}}
                            value={subjectId} onChange={v => {
                                setSubjectId(v)
                                setPage(1)
                            }}
                            options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                    <Select placeholder="Trạng thái" allowClear style={{width: 150}} value={status}
                            onChange={v => {
                                setStatus(v)
                                setPage(1)
                            }}
                            options={(['Draft', 'Published', 'Archived'] as ExamStatus[]).map(s => ({value: s, label: EXAM_STATUS_LABEL[s]}))}/>
                </div>

                <div className="section-card shrink-0">
                    <Table columns={columns} dataSource={data?.items ?? []} rowKey="id" loading={isLoading}
                           scroll={{x: 1200}}
                           pagination={{
                               current: page, pageSize, total: data?.total ?? 0, showSizeChanger: true,
                               showTotal: total => `Tổng số ${total} đề thi`,
                               onChange: (p, ps) => {
                                   setPage(p)
                                   setPageSize(ps)
                               },
                           }}/>
                </div>
            </div>

            <AnalyticsDrawer examId={analyticsExamId} onClose={() => setAnalyticsExamId(undefined)}/>
        </>
    )
}
