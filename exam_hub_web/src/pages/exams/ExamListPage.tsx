import {useMemo, useState} from 'react'
import {Drawer, Dropdown, Input, Popconfirm, Select, Spin, Table, Tag, message} from 'antd'
import type {TableColumnsType} from 'antd'
import {BarChartOutlined, DownloadOutlined, EyeOutlined, SearchOutlined, SolutionOutlined} from '@ant-design/icons'
import {SubmissionsDrawer} from './SubmissionsDrawer'
import {AnalyticsDrawer} from './AnalyticsDrawer'
import {
    useDeleteExamMutation,
    useExamsQuery,
    useExamWithQuestionsQuery,
    usePublishExamMutation,
} from '../../hooks/queries/useExams'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {examService} from '../../services/examService'
import {parseAnswers, stripHtml} from '../../utils/snapshot'

const STATUS_COLOR: Record<ExamStatus, string> = {Draft: 'gold', Published: 'green', Archived: 'default'}
const STATUS_LABEL: Record<ExamStatus, string> = {Draft: 'Nháp', Published: 'Đã phát hành', Archived: 'Lưu trữ'}

export default function ExamListPage() {
    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()

    const [page, setPage] = useState(1)
    const [pageSize, setPageSize] = useState(20)
    const [gradeLevelId, setGradeLevelId] = useState<number>()
    const [subjectId, setSubjectId] = useState<number>()
    const [status, setStatus] = useState<ExamStatus>()
    const [keyword, setKeyword] = useState('')
    const [previewId, setPreviewId] = useState<string>()
    const [submissionsExamId, setSubmissionsExamId] = useState<string>()
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
            render: (v: ExamStatus) => <Tag color={STATUS_COLOR[v]}>{STATUS_LABEL[v]}</Tag>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 440, fixed: 'right',
            render: (_, e) => (
                <div className="flex gap-2 items-center">
                    <button className="text-blue-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => setPreviewId(e.id)}>
                        <EyeOutlined/> Xem
                    </button>
                    <button className="text-gray-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => setSubmissionsExamId(e.id)}>
                        <SolutionOutlined/> Bài nộp
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
                            options={(['Draft', 'Published', 'Archived'] as ExamStatus[]).map(s => ({value: s, label: STATUS_LABEL[s]}))}/>
                </div>

                <div className="section-card">
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

            <ExamPreviewDrawer examId={previewId} onClose={() => setPreviewId(undefined)}/>
            <SubmissionsDrawer examId={submissionsExamId} onClose={() => setSubmissionsExamId(undefined)}/>
            <AnalyticsDrawer examId={analyticsExamId} onClose={() => setAnalyticsExamId(undefined)}/>
        </>
    )
}

function ExamPreviewDrawer({examId, onClose}: {examId?: string; onClose: () => void}) {
    const {data: exam, isLoading} = useExamWithQuestionsQuery(examId)

    return (
        <Drawer title={exam?.title ?? 'Xem trước đề thi'} open={!!examId} onClose={onClose} width={640}>
            {isLoading && <Spin/>}
            {exam && (
                <div className="flex flex-col gap-4">
                    <div className="text-sm text-gray-500">
                        {exam.examCode && <span>Mã đề: {exam.examCode} · </span>}
                        Thời gian: {exam.durationMinutes} phút · Tổng điểm: {exam.totalScore}
                    </div>
                    {(exam.questions ?? []).map((q, i) => (
                        <div key={q.id} className="border-b border-gray-100 pb-3">
                            <p className="font-medium text-gray-800">
                                Câu {i + 1}{q.score != null ? ` (${q.score}đ)` : ''}: {stripHtml(q.contentSnapshot)}
                            </p>
                            <ol className="list-[upper-alpha] pl-6 mt-1 text-gray-600 text-sm">
                                {parseAnswers(q.answersSnapshot).map((a, ai) => (
                                    <li key={ai} className={a.isCorrect ? 'text-green-600 font-medium' : ''}>
                                        {stripHtml(a.content)}
                                    </li>
                                ))}
                            </ol>
                        </div>
                    ))}
                </div>
            )}
        </Drawer>
    )
}
