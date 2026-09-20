import {useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Empty, message, Spin} from 'antd'
import {ArrowRightOutlined, CalendarOutlined, DownOutlined, ReadOutlined, UpOutlined} from '@ant-design/icons'
import {useMySessionsQuery, useStartSessionMutation} from '../../hooks/queries/useExamSessions'
import {statusCode} from '../../services/requestService'
import {useAuth} from '../../hooks/useAuth'
import {SessionAttempts} from './SessionAttempts'
import {getStudentSessionAction, takeUrl} from './studentSessionAction'
import {BRAND} from '../../constants/theme'

const AVAILABILITY: Record<ExamSessionAvailability, string> = {
    upcoming: 'Sắp mở',
    open: 'Đang mở',
    closed: 'Đã đóng',
}

/** Badge pill phẳng theo trạng thái khả dụng (khớp Figma 07A). */
const BADGE: Record<ExamSessionAvailability, { bg: string; color: string }> = {
    open: {bg: '#e3f4ec', color: BRAND.success},
    upcoming: {bg: '#e8ebfb', color: '#5b6ee0'},
    closed: {bg: BRAND.neutralSoft, color: '#8a93a5'},
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

export default function StudentSessionListPage() {
    const navigate = useNavigate()
    const {user} = useAuth()
    const {data: sessions = [], isLoading} = useMySessionsQuery()
    const start = useStartSessionMutation()
    // Kỳ thi đang mở rộng danh sách các lần thi (thay modal cũ) — mỗi lúc chỉ mở một card.
    const [openedResults, setOpenedResults] = useState<string>()

    const startAndGo = async (s: MySession) => {
        const res = await start.mutateAsync({id: s.id})
        if (res.status === statusCode.Error || !res.data) {
            message.error(res.message || 'Không thể vào thi')
            return
        }
        navigate(takeUrl(res.data, s.id, getStudentSessionAction(s).kind === 'resume'))
    }

    // Nút hành động full-width dưới card. Khi kỳ thi đóng/hết lượt nhưng đã có
    // bài nộp → tái dụng ô nút để "Xem kết quả" (giữ layout 1 nút như Figma).
    const renderAction = (s: MySession) => {
        const action = getStudentSessionAction(s)

        if (action.kind === 'unavailable')
            return <Button block disabled>{action.label}</Button>
        if (action.kind === 'results') {
            const opened = openedResults === s.id
            return (
                <Button block aria-expanded={opened}
                        icon={opened ? <UpOutlined/> : <DownOutlined/>} iconPosition="end"
                        onClick={() => setOpenedResults(opened ? undefined : s.id)}>
                    Xem kết quả
                </Button>
            )
        }
        if (action.kind === 'resume') {
            return (
                <Button type="primary" block loading={start.isPending}
                        icon={<ArrowRightOutlined/>} iconPosition="end"
                        onClick={() => startAndGo(s)}>
                    Tiếp tục
                </Button>
            )
        }

        if (action.kind === 'pick') {
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
                    <div className="exam-list-eyebrow" style={{color: '#c98a2b'}}>Phòng thi</div>
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
                            <div key={s.id}
                                 className="bg-white rounded-xl border border-border p-5 flex flex-col gap-2.5">
                                <div className="flex items-start justify-between gap-3">
                                    <h3 className="text-[17px] font-semibold leading-snug"
                                        style={{color: BRAND.ink}}>{s.title}</h3>
                                    <span className="shrink-0 text-[12px] font-medium px-2.5 py-0.5 rounded-full"
                                          style={{
                                              background: BADGE[s.availability].bg,
                                              color: BADGE[s.availability].color
                                          }}>
                                        {AVAILABILITY[s.availability]}
                                    </span>
                                </div>
                                <div className="flex items-center gap-2 text-[13px]" style={{color: BRAND.muted}}>
                                    <ReadOutlined style={{color: BRAND.mutedSoft}}/>
                                    <span>{s.subjectName ?? '—'} · {s.gradeLevelName ?? '—'}</span>
                                </div>
                                <div className="flex items-center gap-2 text-[13px]" style={{color: BRAND.muted}}>
                                    <CalendarOutlined style={{color: BRAND.mutedSoft}}/>
                                    <span>{fmtRange(s.openAt, s.closeAt)}</span>
                                </div>
                                <div className="text-[13px]" style={{color: BRAND.muted}}>Thời gian làm bài: {s.durationMinutes} phút</div>
                                <div className="pt-3 mt-1 border-t border-border">
                                    {renderAction(s)}
                                    {openedResults === s.id &&
                                        <SessionAttempts sessionId={s.id} studentId={user?.id}/>}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

        </div>
    )
}
