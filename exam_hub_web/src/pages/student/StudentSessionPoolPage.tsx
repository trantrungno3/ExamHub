import {useLocation, useNavigate, useParams} from 'react-router-dom'
import {Button, Empty, Spin, Tag, message} from 'antd'
import {ArrowRightOutlined, FileTextOutlined, LeftOutlined} from '@ant-design/icons'
import {useSessionPoolQuery, useStartSessionMutation} from '../../hooks/queries/useExamSessions'
import {statusCode} from '../../services/requestService'
import {ROUTES} from '../../routes/paths'

const STATE_TAG: Record<SessionPoolItemState, {label: string; color: string}> = {
    notStarted: {label: 'Chưa làm', color: 'default'},
    inProgress: {label: 'Đang làm', color: 'processing'},
    completed: {label: 'Đã hoàn thành', color: 'green'},
}

type PoolNavState = {title?: string; subjectName?: string; gradeLevelName?: string}

function takeUrl(examId: string, sessionId: string, submissionId: string): string {
    const p = new URLSearchParams({examId, sessionId, submissionId})
    return `/student/exam?${p.toString()}`
}

export default function StudentSessionPoolPage() {
    const {id} = useParams<{id: string}>()
    const navigate = useNavigate()
    const nav = (useLocation().state ?? {}) as PoolNavState
    const {data: pool = [], isLoading} = useSessionPoolQuery(id)
    const start = useStartSessionMutation()

    const choose = async (item: SessionPoolItem) => {
        if (!id) return
        if (item.studentState === 'inProgress' && item.submissionId) {
            navigate(takeUrl(item.examId, id, item.submissionId))
            return
        }
        const res = await start.mutateAsync({id, examId: item.examId})
        if (res.status === statusCode.Error || !res.data) {
            message.error(res.message || 'Không thể vào thi')
            return
        }
        navigate(takeUrl(res.data.examId, id, res.data.submissionId))
    }

    const subtitle = nav.title
        ? `Kỳ thi: ${nav.title}${nav.subjectName ? ` · ${nav.subjectName}` : ''}${nav.gradeLevelName ? ` · ${nav.gradeLevelName}` : ''} — chọn một đề bên dưới để bắt đầu`
        : 'Chọn một đề bên dưới để bắt đầu'

    return (
        <div className="exam-desk min-h-full p-6 sm:p-8">
            <div className="max-w-4xl mx-auto flex flex-col gap-4">
                <div>
                    <button className="inline-flex items-center gap-1 text-[13px] font-medium"
                            style={{color: '#3a74f5'}} onClick={() => navigate(ROUTES.STUDENT_EXAMS)}>
                        <LeftOutlined className="text-[11px]"/> Quay lại kỳ thi của tôi
                    </button>
                    <h1 className="exam-list-title mt-1">Chọn đề để làm</h1>
                    <p className="exam-list-sub">{subtitle}</p>
                </div>

                {isLoading ? (
                    <div className="flex justify-center py-16"><Spin size="large"/></div>
                ) : pool.length === 0 ? (
                    <div className="bg-white/60 rounded-xl border border-stone-200 py-16">
                        <Empty description="Kỳ thi chưa có đề"/>
                    </div>
                ) : (
                    <div className="flex flex-col gap-3">
                        {pool.map((item, idx) => {
                            const tag = STATE_TAG[item.studentState]
                            const isCompleted = item.studentState === 'completed'
                            return (
                                <div key={item.examId}
                                     className="bg-white rounded-xl border border-[#eceef2] px-4 py-3.5 flex items-center gap-4">
                                    <div className="w-11 h-11 rounded-lg flex items-center justify-center shrink-0"
                                         style={{background: '#eef1ff', color: '#3a74f5'}}>
                                        <FileTextOutlined className="text-[18px]"/>
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="font-semibold text-[15px] truncate" style={{color: '#191d27'}}>
                                            Đề số {idx + 1}{item.examCode ? ` (${item.examCode})` : ''}
                                        </p>
                                        <Tag color={tag.color} className="mt-1">{tag.label}</Tag>
                                    </div>
                                    {isCompleted ? (
                                        <Button disabled>Đã làm</Button>
                                    ) : (
                                        <Button type="primary" loading={start.isPending}
                                                icon={<ArrowRightOutlined/>} iconPosition="end"
                                                onClick={() => choose(item)}>
                                            {item.studentState === 'inProgress' ? 'Tiếp tục' : 'Bắt đầu'}
                                        </Button>
                                    )}
                                </div>
                            )
                        })}
                    </div>
                )}
            </div>
        </div>
    )
}
