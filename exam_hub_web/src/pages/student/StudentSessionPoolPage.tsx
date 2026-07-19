import {useNavigate, useParams} from 'react-router-dom'
import {Button, Empty, Spin, Tag, message} from 'antd'
import {ArrowLeftOutlined} from '@ant-design/icons'
import {useSessionPoolQuery, useStartSessionMutation} from '../../hooks/queries/useExamSessions'
import {statusCode} from '../../services/requestService'
import {ROUTES} from '../../routes/paths'

const STATE_TAG: Record<SessionPoolItemState, {label: string; color: string}> = {
    notStarted: {label: 'Chưa làm', color: 'default'},
    inProgress: {label: 'Đang làm', color: 'processing'},
    completed: {label: 'Đã làm', color: 'green'},
}

function takeUrl(examId: string, sessionId: string, submissionId: string): string {
    const p = new URLSearchParams({examId, sessionId, submissionId})
    return `/student/exam?${p.toString()}`
}

export default function StudentSessionPoolPage() {
    const {id} = useParams<{id: string}>()
    const navigate = useNavigate()
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

    return (
        <div className="p-6 flex flex-col gap-4">
            <div className="flex items-center gap-3">
                <button className="text-gray-500 hover:text-gray-800" onClick={() => navigate(ROUTES.STUDENT_EXAMS)}>
                    <ArrowLeftOutlined/>
                </button>
                <div>
                    <p className="text-xl font-semibold text-gray-800">Chọn đề</p>
                    <p className="text-sm text-gray-500">Chọn một đề để làm bài</p>
                </div>
            </div>

            {isLoading ? (
                <Spin/>
            ) : pool.length === 0 ? (
                <Empty description="Kỳ thi chưa có đề"/>
            ) : (
                <div className="grid gap-3 md:grid-cols-2">
                    {pool.map(item => {
                        const tag = STATE_TAG[item.studentState]
                        const isCompleted = item.studentState === 'completed'
                        return (
                            <div key={item.examId} className="section-card flex items-center justify-between gap-3">
                                <div>
                                    <p className="font-medium text-gray-800">
                                        {item.title}{item.examCode ? ` (${item.examCode})` : ''}
                                    </p>
                                    <p className="text-sm text-gray-500">Tổng điểm: {item.totalScore}</p>
                                    <Tag color={tag.color} className="mt-1">{tag.label}</Tag>
                                </div>
                                <Button type="primary" disabled={isCompleted} loading={start.isPending}
                                        onClick={() => choose(item)}>
                                    {item.studentState === 'inProgress' ? 'Tiếp tục' : 'Làm đề này'}
                                </Button>
                            </div>
                        )
                    })}
                </div>
            )}
        </div>
    )
}
