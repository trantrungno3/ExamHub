import {useNavigate} from 'react-router-dom'
import {Button, Empty, Spin, Tag, message} from 'antd'
import {useMySessionsQuery, useStartSessionMutation} from '../../hooks/queries/useExamSessions'
import {statusCode} from '../../services/requestService'

const AVAILABILITY: Record<ExamSessionAvailability, {label: string; color: string}> = {
    upcoming: {label: 'Sắp mở', color: 'blue'},
    open: {label: 'Đang mở', color: 'green'},
    closed: {label: 'Đã đóng', color: 'default'},
}

function fmt(ms: number): string {
    return new Date(ms).toLocaleString('vi-VN', {day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'})
}

function takeUrl(examId: string, sessionId: string, submissionId: string): string {
    const p = new URLSearchParams({examId, sessionId, submissionId})
    return `/student/exam?${p.toString()}`
}

export default function StudentSessionListPage() {
    const navigate = useNavigate()
    const {data: sessions = [], isLoading} = useMySessionsQuery()
    const start = useStartSessionMutation()

    const startAndGo = async (s: MySession) => {
        const res = await start.mutateAsync({id: s.id})
        if (res.status === statusCode.Error || !res.data) {
            message.error(res.message || 'Không thể vào thi')
            return
        }
        navigate(takeUrl(res.data.examId, s.id, res.data.submissionId))
    }

    const renderAction = (s: MySession) => {
        const remaining = s.maxAttempts - s.usedAttempts
        if (s.inProgressSubmissionId && s.inProgressExamId) {
            return (
                <Button type="primary" loading={start.isPending}
                        onClick={() => navigate(takeUrl(s.inProgressExamId!, s.id, s.inProgressSubmissionId!))}>
                    Tiếp tục
                </Button>
            )
        }
        if (s.availability !== 'open') {
            return <Button disabled>{s.availability === 'upcoming' ? 'Chưa mở' : 'Đã đóng'}</Button>
        }
        if (remaining <= 0) {
            return <Button disabled>Hết lượt</Button>
        }
        if (s.pickMode === 'StudentChoice') {
            return (
                <Button type="primary" onClick={() => navigate(`/student/session/${s.id}/pool`)}>
                    Chọn đề
                </Button>
            )
        }
        return (
            <Button type="primary" loading={start.isPending} onClick={() => startAndGo(s)}>
                Vào thi
            </Button>
        )
    }

    return (
        <div className="p-6 flex flex-col gap-4">
            <div>
                <p className="text-xl font-semibold text-gray-800">Kỳ thi của tôi</p>
                <p className="text-sm text-gray-500">Các kỳ thi được giao cho lớp/khoá của bạn</p>
            </div>

            {isLoading ? (
                <Spin/>
            ) : sessions.length === 0 ? (
                <Empty description="Chưa có kỳ thi nào được giao"/>
            ) : (
                <div className="grid gap-4 md:grid-cols-2">
                    {sessions.map(s => {
                        const av = AVAILABILITY[s.availability]
                        const remaining = s.maxAttempts - s.usedAttempts
                        return (
                            <div key={s.id} className="section-card flex flex-col gap-2">
                                <div className="flex items-start justify-between gap-2">
                                    <p className="font-semibold text-gray-800">{s.title}</p>
                                    <Tag color={av.color}>{av.label}</Tag>
                                </div>
                                <p className="text-sm text-gray-500">
                                    {s.subjectName ?? '—'} · {s.gradeLevelName ?? '—'}
                                </p>
                                <p className="text-sm text-gray-500">{fmt(s.openAt)} → {fmt(s.closeAt)}</p>
                                <div className="flex items-center justify-between mt-1">
                                    <span className="text-sm text-gray-600">
                                        Lượt còn lại: <b>{Math.max(0, remaining)}</b>/{s.maxAttempts}
                                        {s.pickMode === 'StudentChoice' && ' · Tự chọn đề'}
                                    </span>
                                    {renderAction(s)}
                                </div>
                            </div>
                        )
                    })}
                </div>
            )}
        </div>
    )
}
