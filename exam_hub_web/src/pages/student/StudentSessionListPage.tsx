import {useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Empty, Spin, message} from 'antd'
import {ArrowRightOutlined, CalendarOutlined, ReadOutlined} from '@ant-design/icons'
import {useMySessionsQuery, useStartSessionMutation} from '../../hooks/queries/useExamSessions'
import {statusCode} from '../../services/requestService'
import {useAuth} from '../../AuthProvider'
import {SessionResultsModal} from './SessionResultsModal'

const AVAILABILITY: Record<ExamSessionAvailability, string> = {
    upcoming: 'Sắp mở',
    open: 'Đang mở',
    closed: 'Đã đóng',
}

/** Cùng ngày → "dd/MM/yyyy · HH:mm–HH:mm"; khác ngày → "dd/MM HH:mm → dd/MM HH:mm". */
function fmtRange(openAt: number, closeAt: number): string {
    const o = new Date(openAt)
    const c = new Date(closeAt)
    const d = (x: Date) => x.toLocaleDateString('vi-VN', {day: '2-digit', month: '2-digit', year: 'numeric'})
    const dShort = (x: Date) => x.toLocaleDateString('vi-VN', {day: '2-digit', month: '2-digit'})
    const t = (x: Date) => x.toLocaleTimeString('vi-VN', {hour: '2-digit', minute: '2-digit'})
    return o.toDateString() === c.toDateString()
        ? `${d(o)} · ${t(o)}–${t(c)}`
        : `${dShort(o)} ${t(o)} → ${dShort(c)} ${t(c)}`
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

    const openResults = (s: MySession) => setResults({id: s.id, title: s.title})

    // Nút hành động full-width dưới card. Khi kỳ thi đóng/hết lượt nhưng đã có
    // bài nộp → tái dụng ô nút để "Xem kết quả" (giữ layout 1 nút như Figma).
    const renderAction = (s: MySession) => {
        const remaining = s.maxAttempts - s.usedAttempts
        if (s.inProgressSubmissionId && s.inProgressExamId) {
            return (
                <Button type="primary" block loading={start.isPending}
                        icon={<ArrowRightOutlined/>} iconPosition="end"
                        onClick={() => navigate(takeUrl(s.inProgressExamId!, s.id, s.inProgressSubmissionId!))}>
                    Tiếp tục
                </Button>
            )
        }
        if (s.availability !== 'open') {
            if (s.usedAttempts > 0) return <Button block onClick={() => openResults(s)}>Xem kết quả</Button>
            return <Button block disabled>{s.availability === 'upcoming' ? 'Chưa mở' : 'Đã đóng'}</Button>
        }
        if (remaining <= 0) {
            if (s.usedAttempts > 0) return <Button block onClick={() => openResults(s)}>Xem kết quả</Button>
            return <Button block disabled>Hết lượt</Button>
        }
        if (s.pickMode === 'StudentChoice') {
            return (
                <Button type="primary" block icon={<ArrowRightOutlined/>} iconPosition="end"
                        onClick={() => navigate(`/student/session/${s.id}/pool`, {
                            state: {title: s.title, subjectName: s.subjectName, gradeLevelName: s.gradeLevelName},
                        })}>
                    Chọn đề
                </Button>
            )
        }
        return (
            <Button type="primary" block loading={start.isPending}
                    icon={<ArrowRightOutlined/>} iconPosition="end" onClick={() => startAndGo(s)}>
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
                        {sessions.map(s => (
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
                                    <span>{fmtRange(s.openAt, s.closeAt)}</span>
                                </div>
                                <div className="mt-1.5">{renderAction(s)}</div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <SessionResultsModal sessionId={results?.id} studentId={user?.id} title={results?.title}
                                 onClose={() => setResults(undefined)}/>
        </div>
    )
}
