import {useLocation, useNavigate, useParams} from 'react-router-dom'
import type {TableColumnsType} from 'antd'
import {Button, Table} from 'antd'
import {ArrowLeftOutlined} from '@ant-design/icons'
import {useFinalizeSubmissionMutation, useSubmissionsBySessionQuery} from '../../hooks/queries/useSubmissions'
import {StatusTag, type StatusVariant} from '../../components/StatusTag'
import {formatTimestamp} from '../../utils/datetime'
import {ROUTES} from '../../routes/paths'

const STATUS_VARIANT: Record<SubmissionStatus, StatusVariant> = {
    InProgress: 'default', Submitted: 'warning', Graded: 'success',
}
const STATUS_LABEL: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Chờ chấm', Graded: 'Đã chấm',
}

export default function SubmissionListPage() {
    const {id} = useParams<{ id: string }>()
    const navigate = useNavigate()
    const {state} = useLocation() as { state?: { title?: string; subjectName?: string; gradeLevelName?: string } }
    const {data: submissions, isLoading} = useSubmissionsBySessionQuery(id)
    const finalize = useFinalizeSubmissionMutation()

    const rows = submissions ?? []
    const gradedCount = rows.filter(s => s.status === 'Graded').length
    const pending = rows.filter(s => s.status === 'Submitted')

    const subtitle = [state?.title, state?.subjectName, state?.gradeLevelName].filter(Boolean).join(' · ')

    const columns: TableColumnsType<ExamSubmission> = [
        {
            title: 'Học sinh', key: 'student',
            render: (_, s) => (
                <div>
                    <div className="font-medium text-gray-800">
                        {s.studentName || `HS ${s.studentId.slice(0, 8)}…`}
                    </div>
                    <div className="text-xs text-gray-400">
                        {s.studentClassName ? `Lớp ${s.studentClassName}` : '—'}
                    </div>
                </div>
            ),
        },
        {
            title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 140,
            render: (v: SubmissionStatus) => <StatusTag status={STATUS_VARIANT[v]} label={STATUS_LABEL[v]}/>,
        },
        {title: 'Điểm', dataIndex: 'totalScore', key: 'totalScore', width: 90, render: v => v ?? '—'},
        {
            title: 'Nộp lúc',
            dataIndex: 'submittedAt',
            key: 'submittedAt',
            width: 160,
            render: (v: number) => formatTimestamp(v)
        },
        {
            title: 'Thao tác', key: 'actions', width: 140, fixed: 'right',
            render: (_, s) => (
                <button className="text-blue-600 text-sm hover:underline"
                        onClick={() => navigate(
                            ROUTES.SUBMISSION_REVIEW.replace(':id', s.id),
                            {state: {studentName: s.studentName, studentClassName: s.studentClassName, sessionId: id}},
                        )}>
                    Xem &amp; chấm
                </button>
            ),
        },
    ]

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Bài nộp kỳ thi</p>
                    <p className="top-bar-subtitle">{subtitle || 'Danh sách bài nộp của học sinh'}</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                <div className="flex items-center justify-between">
                    <button className="text-blue-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => navigate(ROUTES.EXAM_SESSIONS)}>
                        <ArrowLeftOutlined/> Danh sách kỳ thi
                    </button>
                    <div className="flex items-center gap-3">
                        <span className="text-sm text-gray-500">Đã chấm {gradedCount}/{rows.length} bài nộp</span>
                        <Button type="primary" disabled={pending.length === 0} loading={finalize.isPending}
                                onClick={() => pending.forEach(s => finalize.mutate(s.id))}>
                            Chốt điểm &amp; công bố
                        </Button>
                    </div>
                </div>

                <div className="section-card shrink-0">
                    <Table columns={columns} dataSource={rows} rowKey="id" loading={isLoading}
                           pagination={{showTotal: total => `Tổng số ${total} bài nộp`}}/>
                </div>
            </div>
        </>
    )
}
