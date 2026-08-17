import {useMemo} from 'react'
import {useNavigate} from 'react-router-dom'
import {Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {FileTextOutlined, PlusOutlined, ThunderboltOutlined, UploadOutlined} from '@ant-design/icons'
import {Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip} from 'recharts'
import {useExamsQuery} from '../../hooks/queries/useExams'
import {useQuestionsQuery} from '../../hooks/queries/useQuestions'
import {formatTimestamp} from '../../utils/datetime'

const STATUS_LABEL: Record<ExamStatus, string> = {Draft: 'Nháp', Published: 'Đã công bố', Archived: 'Lưu trữ'}
const STATUS_COLOR: Record<ExamStatus, string> = {Draft: 'gold', Published: 'green', Archived: 'default'}
const PIE_COLOR: Record<ExamStatus, string> = {Draft: '#FAAD14', Published: '#52C41A', Archived: '#BFBFBF'}

export default function DashboardPage() {
    const navigate = useNavigate()
    const today = new Date().toLocaleDateString('vi-VN', {weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric'})

    // Tổng số lấy từ trường `total` của kết quả phân trang (pageSize nhỏ để nhẹ).
    const questions = useQuestionsQuery({page: 1, pageSize: 1})
    const examsHead = useExamsQuery({page: 1, pageSize: 1})
    const recentExams = useExamsQuery({page: 1, pageSize: 8})

    const stats = [
        {label: 'Tổng câu hỏi', value: questions.data?.total ?? 0, iconBg: 'bg-blue-100', icon: '❓'},
        {label: 'Đề thi đã tạo', value: examsHead.data?.total ?? 0, iconBg: 'bg-green-100', icon: '📄'},
    ]

    const statusPie = useMemo(() => {
        const counts: Record<string, number> = {}
        for (const e of recentExams.data?.items ?? []) counts[e.status] = (counts[e.status] ?? 0) + 1
        return (Object.keys(counts) as ExamStatus[]).map(s => ({name: STATUS_LABEL[s], status: s, value: counts[s]}))
    }, [recentExams.data])

    const quickActions = [
        {label: 'Thêm câu hỏi', icon: <PlusOutlined/>, bg: 'bg-blue-50', text: 'text-blue-700', to: '/app/questions/add'},
        {label: 'Tạo mẫu đề', icon: <FileTextOutlined/>, bg: 'bg-purple-50', text: 'text-purple-700', to: '/app/exams/create'},
        {label: 'Sinh đề ngay', icon: <ThunderboltOutlined/>, bg: 'bg-green-50', text: 'text-green-700', to: '/app/generate'},
        {label: 'Đề thi', icon: <UploadOutlined/>, bg: 'bg-orange-50', text: 'text-orange-700', to: '/app/exam-list'},
    ]

    const columns: TableColumnsType<Exam> = [
        {title: 'Tên đề thi', dataIndex: 'title', key: 'title', render: v => <span className="font-medium text-gray-800">{v}</span>},
        {title: 'Môn', dataIndex: 'subjectName', key: 'subjectName', render: v => v ?? '—'},
        {title: 'Lớp', dataIndex: 'gradeLevelName', key: 'gradeLevelName', render: v => v ?? '—'},
        {
            title: 'Trạng thái', dataIndex: 'status', key: 'status',
            render: (v: ExamStatus) => <Tag color={STATUS_COLOR[v]}>{STATUS_LABEL[v]}</Tag>,
        },
        {
            title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt',
            render: (v: number) => <span className="text-gray-400">{formatTimestamp(v, 'DD/MM/YYYY')}</span>,
        },
    ]

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Tổng quan hệ thống</p>
                    <p className="top-bar-subtitle">{today}</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-5">
                <div className="flex gap-4">
                    {stats.map(s => (
                        <div key={s.label} className="stat-card">
                            <div className={`stat-card-icon ${s.iconBg}`}><span>{s.icon}</span></div>
                            <div>
                                <p className="stat-card-value">{s.value.toLocaleString('vi-VN')}</p>
                                <p className="stat-card-label">{s.label}</p>
                            </div>
                        </div>
                    ))}
                </div>

                <div className="flex gap-4">
                    <div className="section-card flex-[3]">
                        <div className="section-card-header border-b border-gray-50">
                            <span className="section-card-title">Đề thi gần đây</span>
                            <button className="text-xs text-blue-600 font-medium hover:underline"
                                    onClick={() => navigate('/app/exam-list')}>Xem tất cả →</button>
                        </div>
                        <Table columns={columns} dataSource={recentExams.data?.items ?? []} rowKey="id"
                               loading={recentExams.isLoading} pagination={false} size="small" scroll={{x: 600}}/>
                    </div>

                    <div className="flex flex-col gap-4 flex-[1.4] min-w-0">
                        <div className="section-card p-4">
                            <p className="section-card-title mb-3">Thao tác nhanh</p>
                            <div className="grid grid-cols-2 gap-2">
                                {quickActions.map(a => (
                                    <button key={a.label} className={`quick-action ${a.bg} ${a.text}`}
                                            onClick={() => navigate(a.to)}>
                                        <span className="text-base">{a.icon}</span>
                                        <span>{a.label}</span>
                                    </button>
                                ))}
                            </div>
                        </div>

                        <div className="section-card p-4 flex-1">
                            <p className="section-card-title mb-2">Đề thi gần đây theo trạng thái</p>
                            {statusPie.length === 0 ? (
                                <p className="text-[12px] text-gray-400">Chưa có đề thi.</p>
                            ) : (
                                <ResponsiveContainer width="100%" height={180}>
                                    <PieChart>
                                        <Pie data={statusPie} dataKey="value" nameKey="name" outerRadius={70} label>
                                            {statusPie.map(s => <Cell key={s.status} fill={PIE_COLOR[s.status]}/>)}
                                        </Pie>
                                        <Tooltip/>
                                        <Legend/>
                                    </PieChart>
                                </ResponsiveContainer>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        </>
    )
}
