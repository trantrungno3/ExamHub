import {useEffect, useMemo, useRef, useState} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Empty, Form, Input, Modal, Radio, Spin, message} from 'antd'
import {ClockCircleOutlined} from '@ant-design/icons'
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

    if (isLoading) return <div className="exam-desk flex justify-center py-24"><Spin size="large"/></div>
    if (!exam) return <div className="exam-desk flex justify-center py-24"><Empty description="Không tìm thấy đề thi"/></div>

    return <ExamRunner exam={exam} studentId={user?.id} studentName={user?.displayName ?? user?.userName}
        sessionId={sessionId} submissionId={submissionId}/>
}

function ExamRunner({exam, studentId, studentName, sessionId, submissionId}: {
    exam: Exam; studentId?: string; studentName?: string; sessionId?: string; submissionId?: string
}) {
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
    const [timeLeft, setTimeLeft] = useState(() => exam.durationMinutes * 60)
    const qRefs = useRef<(HTMLElement | null)[]>([])
    const autoSubmitted = useRef(false)

    const answeredCount = questions.filter(q => hasAnswer(values[q.id])).length
    const mm = String(Math.floor(timeLeft / 60)).padStart(2, '0')
    const ss = String(timeLeft % 60).padStart(2, '0')
    const danger = timeLeft <= 300

    // Đồng hồ đếm ngược
    useEffect(() => {
        const id = setInterval(() => setTimeLeft(t => (t > 0 ? t - 1 : 0)), 1000)
        return () => clearInterval(id)
    }, [])

    // Theo dõi câu đang xem để làm nổi trên phiếu trả lời
    useEffect(() => {
        const obs = new IntersectionObserver((entries) => {
            const visible = entries.filter(e => e.isIntersecting)
                .sort((a, b) => (a.target as HTMLElement).offsetTop - (b.target as HTMLElement).offsetTop)
            if (visible[0]) setActiveIdx(Number((visible[0].target as HTMLElement).dataset.idx))
        }, {rootMargin: '-45% 0px -50% 0px', threshold: 0})
        qRefs.current.forEach(el => el && obs.observe(el))
        return () => obs.disconnect()
    }, [questions.length])

    const buildAndSubmit = async () => {
        if (!studentId) { message.error('Không xác định được học sinh đang đăng nhập'); return }
        const vals = form.getFieldsValue()
        const body: ExamSubmissionBody = {
            examId: exam.id,
            studentId,
            answers: questions.map((eq, idx) => {
                const v = vals[eq.id]
                const isEssay = parsed[idx].length === 0
                return {
                    examQuestionId: eq.id,
                    selectedAnswerIds: !isEssay && typeof v === 'string' && v ? [v] : undefined,
                    essayContent: isEssay && typeof v === 'string' ? v : undefined,
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
        const unanswered = questions.length - answeredCount
        Modal.confirm({
            title: 'Nộp bài thi?',
            content: unanswered > 0 ? `Còn ${unanswered} câu chưa trả lời.` : 'Bạn đã trả lời tất cả các câu.',
            okText: 'Nộp bài', cancelText: 'Tiếp tục làm', okButtonProps: {danger: true},
            onOk: buildAndSubmit,
        })
    }

    const scrollTo = (idx: number) => qRefs.current[idx]?.scrollIntoView({behavior: 'smooth', block: 'start'})
    const pickBubble = (qid: string, answerId: string) => {
        form.setFieldValue(qid, answerId)
        setValues(form.getFieldsValue())
    }

    return (
        <div className="exam-desk flex flex-col">
            {/* Thanh trên cùng */}
            <div className="exam-topbar">
                <div className="exam-topbar-brand">
                    <div className="exam-topbar-icon">EH</div>
                    <div className="min-w-0">
                        <p className="text-[13px] font-semibold text-stone-800 leading-tight truncate">{exam.title}</p>
                        <p className="text-[11px] text-stone-500 leading-tight truncate">
                            {exam.examCode ? `Mã đề ${exam.examCode}` : 'ExamHub'}{exam.subjectName ? ` · ${exam.subjectName}` : ''}
                        </p>
                    </div>
                </div>
                <div className={`exam-timer ${danger ? 'exam-timer--danger' : ''}`}>
                    <ClockCircleOutlined/>{mm}:{ss}
                </div>
            </div>

            <Form form={form} component={false} onValuesChange={() => setValues(form.getFieldsValue())}>
                <div className="exam-layout">
                    {/* Tờ đề thi */}
                    <article className="exam-paper">
                        <div className="paper-eyebrow">Đề thi{exam.schoolYear ? ` · Năm học ${exam.schoolYear}` : ''}</div>
                        <h1 className="paper-title">{exam.title}</h1>
                        <p className="paper-subtitle">
                            {exam.subjectName ? `Môn: ${exam.subjectName}` : ''}
                            {` — Thời gian làm bài: ${exam.durationMinutes} phút`}
                        </p>
                        <div className="paper-rule"/>

                        <div className="paper-meta">
                            <div className="paper-meta-row"><span className="paper-meta-key">Mã đề:</span>
                                <span className="paper-fill">{exam.examCode ?? '—'}</span></div>
                            <div className="paper-meta-row"><span className="paper-meta-key">Họ và tên:</span>
                                <span className="paper-fill">{studentName ?? ''}</span></div>
                            <div className="paper-meta-row"><span className="paper-meta-key">Số câu:</span>
                                <span className="paper-fill">{questions.length} câu · {exam.totalScore} điểm</span></div>
                            <div className="paper-meta-row"><span className="paper-meta-key">Lớp:</span>
                                <span className="paper-fill">{exam.className ?? ''}</span></div>
                        </div>

                        <div className="paper-divider"/>

                        {questions.map((q, idx) => {
                            const opts = parsed[idx]
                            const isEssay = opts.length === 0
                            return (
                                <section key={q.id} className="exam-q" data-idx={idx}
                                    ref={el => { qRefs.current[idx] = el }}>
                                    <p className="exam-q-head">
                                        <span className="exam-q-num">Câu {idx + 1}.</span> {stripHtml(q.contentSnapshot)}
                                        {q.score != null && <span className="exam-q-score">{q.score}đ</span>}
                                    </p>

                                    <Form.Item name={q.id} noStyle>
                                        {isEssay ? (
                                            <Input.TextArea className="paper-essay" autoSize={{minRows: 5}}
                                                placeholder="Trình bày bài làm..."/>
                                        ) : (
                                            <Radio.Group className="paper-radio-group">
                                                {opts.map((opt, i) => (
                                                    <Radio key={opt.id || i} value={opt.id}>
                                                        <span className="paper-opt-letter">{letter(i)}.</span>
                                                        {stripHtml(opt.content)}
                                                    </Radio>
                                                ))}
                                            </Radio.Group>
                                        )}
                                    </Form.Item>
                                </section>
                            )
                        })}

                        <div className="paper-end">— Hết —</div>
                    </article>

                    {/* Phiếu trả lời */}
                    <aside className="answer-sheet">
                        <div className="sheet-head">
                            <div className="sheet-title">Phiếu trả lời</div>
                            <div className="sheet-stats">
                                <span className="text-emerald-600 font-semibold">{answeredCount}</span>
                                <span>/ {questions.length} câu đã làm</span>
                            </div>
                        </div>

                        <div className="sheet-rows">
                            {questions.map((q, idx) => {
                                const opts = parsed[idx]
                                const val = values[q.id]
                                const active = idx === activeIdx
                                return (
                                    <div key={q.id} className={`sheet-row ${active ? 'sheet-row--active' : ''}`}>
                                        <span className="sheet-num" onClick={() => scrollTo(idx)}>{idx + 1}</span>
                                        {opts.length === 0 ? (
                                            <span className={`sheet-essay-dot ${hasAnswer(val) ? 'sheet-essay-dot--filled' : ''}`}
                                                title="Câu tự luận"/>
                                        ) : (
                                            <span className="sheet-bubbles">
                                                {opts.map((opt, i) => (
                                                    <span key={opt.id || i}
                                                        className={`sheet-bubble ${val === opt.id ? 'sheet-bubble--filled' : ''}`}
                                                        onClick={() => pickBubble(q.id, opt.id)}>
                                                        {letter(i)}
                                                    </span>
                                                ))}
                                            </span>
                                        )}
                                    </div>
                                )
                            })}
                        </div>

                        <div className="sheet-foot">
                            <Button danger type="primary" block loading={submit.isPending} onClick={confirmSubmit}
                                className="!h-11 !font-semibold">
                                Nộp bài thi
                            </Button>
                        </div>
                    </aside>
                </div>
            </Form>

            {/* Thanh nộp bài cho màn nhỏ */}
            <div className="exam-mobilebar">
                <span className="text-[13px] text-stone-600 flex-1">
                    Đã làm <b className="text-emerald-600">{answeredCount}</b>/{questions.length} câu
                </span>
                <Button danger type="primary" loading={submit.isPending} onClick={confirmSubmit} className="!font-semibold">
                    Nộp bài thi
                </Button>
            </div>
        </div>
    )
}
