import {useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Empty, Spin, message} from 'antd'
import {CalendarOutlined, ReadOutlined} from '@ant-design/icons'
import {useMySessionsQuery, useStartSessionMutation} from '../../hooks/queries/useExamSessions'
import {statusCode} from '../../services/requestService'
import {useAuth} from '../../AuthProvider'
import {SessionResultsModal} from './SessionResultsModal'

const AVAILABILITY: Record<ExamSessionAvailability, string> = {
    upcoming: 'Sắp mở',
    open: 'Đang mở',
    closed: 'Đã đóng',
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
    const {user} = useAuth()
    const {data: sessions = [], isLoading} = useMySessionsQuery()
    const start = useStartSessionMutation()
    const [results, setResults] = useState<{id: string; title: string}>()

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
        <div className="exam-desk min-h-full p-6 sm:p-8">
            <div className="max-w-5xl mx-auto flex flex-col gap-6">
                <div>
                    <div className="exam-list-eyebrow">Phòng thi</div>
                    <h1 className="exam-list-title">Kỳ thi của tôi</h1>
                    <p className="exam-list-sub">Các kỳ thi được giao cho lớp/khoá của bạn</p>
                </div>

                {isLoading ? (
                    <div className="flex justify-center py-16"><Spin size="large"/></div>
                ) : sessions.length === 0 ? (
                    <div className="bg-white/60 rounded-xl border border-stone-200 py-16">
                        <Empty description="Chưa có kỳ thi nào được giao"/>
                    </div>
                ) : (
                    <div className="grid gap-4 md:grid-cols-2">
                        {sessions.map(s => {
                            const remaining = Math.max(0, s.maxAttempts - s.usedAttempts)
                            return (
                                <div key={s.id} className={`exam-ticket exam-ticket--${s.availability} flex flex-col gap-2.5`}>
                                    <div className="flex items-start justify-between gap-3">
                                        <h3 className="exam-ticket-title">{s.title}</h3>
                                        <span className={`exam-stamp exam-stamp--${s.availability}`}>
                                            {AVAILABILITY[s.availability]}
                                        </span>
                                    </div>
                                    <div className="exam-ticket-meta">
                                        <ReadOutlined className="text-stone-400"/>
                                        <span>{s.subjectName ?? '—'} · {s.gradeLevelName ?? '—'}</span>
                                    </div>
                                    <div className="exam-ticket-meta">
                                        <CalendarOutlined className="text-stone-400"/>
                                        <span>{fmt(s.openAt)} → {fmt(s.closeAt)}</span>
                                    </div>
                                    <div className="exam-ticket-foot">
                                        <span className="text-[13px] text-stone-600">
                                            Lượt còn lại: <b className="text-stone-900">{remaining}</b>/{s.maxAttempts}
                                            {s.pickMode === 'StudentChoice' && ' · Tự chọn đề'}
                                        </span>
                                        <div className="flex items-center gap-2">
                                            {s.usedAttempts > 0 && (
                                                <Button onClick={() => setResults({id: s.id, title: s.title})}>
                                                    Xem kết quả
                                                </Button>
                                            )}
                                            {renderAction(s)}
                                        </div>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                )}
            </div>

            <SessionResultsModal sessionId={results?.id} studentId={user?.id} title={results?.title}
                                 onClose={() => setResults(undefined)}/>
        </div>
    )
}
