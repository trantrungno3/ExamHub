import {useEffect, useMemo, useRef, useState} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Empty, Form, Input, Modal, Radio, Spin, message} from 'antd'
import {
    ArrowLeftOutlined, ArrowRightOutlined, FlagFilled, FlagOutlined,
} from '@ant-design/icons'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {useSubmitExamMutation} from '../../hooks/queries/useSubmissions'
import {useAuth} from '../../AuthProvider'
import {parseAnswers, stripHtml} from '../../utils/snapshot'

const letter = (i: number) => String.fromCharCode(65 + i)
const hasAnswer = (v: unknown) => (typeof v === 'string' ? v.trim().length > 0 : v != null)

export default function ExamTakingPage() {
    const [params] = useSearchParams()
    const examId = params.get('examId') ?? undefined
    const sessionId = params.get('sessionId') ?? undefined
    const submissionId = params.get('submissionId') ?? undefined
    const {data: exam, isLoading} = useExamWithQuestionsQuery(examId)
    const {user} = useAuth()

    if (isLoading) return <div className="take-shell flex items-center justify-center"><Spin size="large"/></div>
    if (!exam) return <div className="take-shell flex items-center justify-center"><Empty description="Không tìm thấy đề thi"/></div>

    return <ExamRunner exam={exam} studentId={user?.id} studentName={user?.displayName ?? user?.userName}
        sessionId={sessionId} submissionId={submissionId}/>
}

function ExamRunner({exam, studentId, studentName, sessionId, submissionId}: {
    exam: Exam; studentId?: string; studentName?: string; sessionId?: string; submissionId?: string
}) {
    const className = exam.className
    const navigate = useNavigate()
    const submit = useSubmitExamMutation()
    const [form] = Form.useForm()

    const questions = useMemo(
        () => [...(exam.questions ?? [])].sort((a, b) => a.sortOrder - b.sortOrder),
        [exam],
    )
    const parsed = useMemo(() => questions.map(q => parseAnswers(q.answersSnapshot)), [questions])

    const [values, setValues] = useState<Record<string, unknown>>({})
    const [activeIdx, setActiveIdx] = useState(0)
    const [flagged, setFlagged] = useState<Set<string>>(new Set())
    const [timeLeft, setTimeLeft] = useState(() => exam.durationMinutes * 60)
    const autoSubmitted = useRef(false)

    const total = questions.length
    const answeredCount = questions.filter(q => hasAnswer(values[q.id])).length
    const unanswered = total - answeredCount
    const progress = total ? Math.round((answeredCount / total) * 100) : 0
    const mm = String(Math.floor(timeLeft / 60)).padStart(2, '0')
    const ss = String(timeLeft % 60).padStart(2, '0')
    const danger = timeLeft <= 300

    const activeQ = questions[activeIdx]
    const activeOpts = parsed[activeIdx] ?? []
    const isEssay = activeOpts.length === 0

    // Đồng hồ đếm ngược
    useEffect(() => {
        const id = setInterval(() => setTimeLeft(t => (t > 0 ? t - 1 : 0)), 1000)
        return () => clearInterval(id)
    }, [])

    const buildAndSubmit = async () => {
        if (!studentId) { message.error('Không xác định được học sinh đang đăng nhập'); return }
        const vals = form.getFieldsValue()
        const body: ExamSubmissionBody = {
            examId: exam.id,
            studentId,
            answers: questions.map((eq, idx) => {
                const v = vals[eq.id]
                const essay = parsed[idx].length === 0
                return {
                    examQuestionId: eq.id,
                    selectedAnswerIds: !essay && typeof v === 'string' && v ? [v] : undefined,
                    essayContent: essay && typeof v === 'string' ? v : undefined,
                }
            }),
            sessionId,
            submissionId,
        }
        const res = await submit.mutateAsync(body)
        if (res.data) {
            message.success('Nộp bài thành công')
            navigate(`/student/exam/result?submissionId=${res.data.id}`)
        } else {
            message.error(res.message || 'Nộp bài thất bại')
        }
    }

    // Hết giờ → tự động nộp bài
    useEffect(() => {
        if (timeLeft === 0 && !autoSubmitted.current && studentId) {
            autoSubmitted.current = true
            message.warning('Đã hết giờ làm bài. Hệ thống tự động nộp bài.')
            void buildAndSubmit()
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [timeLeft])

    const confirmSubmit = () => {
        Modal.confirm({
            title: 'Nộp bài thi?',
            content: unanswered > 0 ? `Còn ${unanswered} câu chưa trả lời.` : 'Bạn đã trả lời tất cả các câu.',
            okText: 'Nộp bài', cancelText: 'Tiếp tục làm', okButtonProps: {danger: true},
            onOk: buildAndSubmit,
        })
    }

    const go = (i: number) => setActiveIdx(Math.max(0, Math.min(total - 1, i)))
    const toggleFlag = () => setFlagged(prev => {
        const n = new Set(prev)
        if (n.has(activeQ.id)) n.delete(activeQ.id); else n.add(activeQ.id)
        return n
    })
    const cellClass = (q: {id: string}, idx: number) => {
        if (idx === activeIdx) return 'take-cell take-cell--current'
        if (flagged.has(q.id)) return 'take-cell take-cell--flagged'
        if (hasAnswer(values[q.id])) return 'take-cell take-cell--answered'
        return 'take-cell'
    }
    const avatarChar = (studentName ?? 'A').charAt(0).toUpperCase()

    return (
        <div className="take-shell">
            {/* Thanh trên cùng */}
            <div className="take-top">
                <div className="flex items-center gap-2.5 min-w-0">
                    <div className="take-top-icon">EH</div>
                    <div className="min-w-0 hidden sm:block">
                        <p className="text-[13px] font-semibold text-stone-800 leading-tight truncate">{exam.title}</p>
                        <p className="text-[11px] text-stone-500 leading-tight truncate">
                            {exam.examCode ? `Mã đề ${exam.examCode}` : 'ExamHub'}
                            {studentName ? ` · ${studentName}` : ''}{className ? ` · Lớp ${className}` : ''}
                        </p>
                    </div>
                </div>
                <div className={`take-timer ${danger ? 'take-timer--danger' : ''}`}>
                    <span className="take-timer-dot"/>{mm}:{ss}
                </div>
            </div>

            <Form form={form} component={false} onValuesChange={() => setValues(form.getFieldsValue())}>
                <div className="take-body">
                    {/* Khu câu hỏi (tối) */}
                    <div className="take-main">
                        <div className="flex items-center gap-3">
                            <span className="take-qnum">{activeIdx + 1}</span>
                            <span className="text-[15px] font-semibold text-slate-200">
                                Câu hỏi {activeIdx + 1} <span className="text-slate-500">/ {total}</span>
                            </span>
                            {activeQ?.score != null && (
                                <span className="ml-auto text-[13px] font-semibold text-blue-300">{activeQ.score}đ</span>
                            )}
                        </div>

                        {activeQ && (
                            <>
                                <div className="take-qbox">{stripHtml(activeQ.contentSnapshot)}</div>

                                <Form.Item name={activeQ.id} noStyle>
                                    {isEssay ? (
                                        <Input.TextArea className="take-essay" autoSize={{minRows: 6}}
                                            placeholder="Trình bày bài làm..."/>
                                    ) : (
                                        <Radio.Group className="take-radio">
                                            {activeOpts.map((opt, i) => (
                                                <Radio key={opt.id || i} value={opt.id}>
                                                    <span className="take-opt-letter">{letter(i)}</span>
                                                    <span className="flex-1">{stripHtml(opt.content)}</span>
                                                </Radio>
                                            ))}
                                        </Radio.Group>
                                    )}
                                </Form.Item>
                            </>
                        )}

                        {/* Điều hướng câu */}
                        <div className="take-navbar">
                            <Button icon={<ArrowLeftOutlined/>} disabled={activeIdx === 0}
                                    onClick={() => go(activeIdx - 1)}>
                                Câu trước
                            </Button>
                            <div className="flex items-center gap-3">
                                <button type="button" title="Đánh dấu để xem lại"
                                        className={`take-flag ${flagged.has(activeQ?.id) ? 'take-flag--on' : ''}`}
                                        onClick={toggleFlag}>
                                    {flagged.has(activeQ?.id) ? <FlagFilled/> : <FlagOutlined/>}
                                </button>
                                <span className="text-[13px] text-slate-400">Câu {activeIdx + 1} / {total}</span>
                            </div>
                            <Button type="primary" icon={<ArrowRightOutlined/>} iconPosition="end"
                                    disabled={activeIdx >= total - 1} onClick={() => go(activeIdx + 1)}>
                                Câu tiếp
                            </Button>
                        </div>
                    </div>

                    {/* Phiếu trả lời (sáng) */}
                    <aside className="take-side">
                        <div className="take-side-head">
                            <div className="take-side-avatar">{avatarChar}</div>
                            <div className="min-w-0">
                                <p className="text-[14px] font-semibold text-stone-800 truncate">{studentName ?? '—'}</p>
                                {className && <p className="text-[12px] text-stone-500">Lớp {className}</p>}
                            </div>
                        </div>

                        <div className="px-4 py-3 flex border-b border-[#eceef2]">
                            <div className="take-stat">
                                <p className="take-stat-num text-emerald-600">{answeredCount}</p>
                                <p className="take-stat-label">Đã trả lời</p>
                            </div>
                            <div className="take-stat">
                                <p className="take-stat-num text-stone-700">{unanswered}</p>
                                <p className="take-stat-label">Chưa trả lời</p>
                            </div>
                            <div className="take-stat">
                                <p className="take-stat-num text-amber-500">{flagged.size}</p>
                                <p className="take-stat-label">Đã đánh dấu</p>
                            </div>
                        </div>

                        <div className="px-4 py-3 border-b border-[#eceef2]">
                            <div className="flex items-center justify-between text-[12px] text-stone-500 mb-1.5">
                                <span>Tiến độ</span><span className="font-semibold text-stone-700">{progress}%</span>
                            </div>
                            <div className="take-progress"><div className="take-progress-fill" style={{width: `${progress}%`}}/></div>
                        </div>

                        <div className="px-4 py-3 flex-1 overflow-auto">
                            <p className="text-[12px] font-semibold text-stone-600 mb-2">Bảng câu hỏi</p>
                            <div className="take-grid">
                                {questions.map((q, idx) => (
                                    <button key={q.id} type="button" className={cellClass(q, idx)}
                                            onClick={() => go(idx)}>
                                        {idx + 1}
                                    </button>
                                ))}
                            </div>
                        </div>

                        <div className="p-3 border-t border-[#eceef2]">
                            {unanswered > 0 && (
                                <p className="text-center text-[12px] text-stone-500 mb-2">Còn {unanswered} câu chưa trả lời</p>
                            )}
                            <Button type="primary" block loading={submit.isPending} onClick={confirmSubmit}
                                    className="!h-11 !font-semibold"
                                    style={{background: '#22c55e', borderColor: '#22c55e'}}>
                                Nộp bài thi
                            </Button>
                        </div>
                    </aside>
                </div>
            </Form>

            {/* Thanh nộp bài cho màn nhỏ (không có panel bên) */}
            <div className="lg:hidden flex items-center gap-3 px-4 py-3 bg-white border-t border-[#eceef2]">
                <span className="text-[13px] text-stone-600 flex-1">
                    Đã làm <b className="text-emerald-600">{answeredCount}</b>/{total} câu
                </span>
                <Button danger type="primary" loading={submit.isPending} onClick={confirmSubmit} className="!font-semibold">
                    Nộp bài thi
                </Button>
            </div>
        </div>
    )
}
