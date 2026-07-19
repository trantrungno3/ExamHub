import {useEffect, useMemo, useState} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Empty, Input, Modal, Spin, message} from 'antd'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {useSubmitExamMutation} from '../../hooks/queries/useSubmissions'
import {useAuth} from '../../AuthProvider'
import {parseAnswers, stripHtml} from '../../utils/snapshot'

type AnswerState = {selectedAnswerIds?: string[]; essayContent?: string}

export default function ExamTakingPage() {
    const [params] = useSearchParams()
    const examId = params.get('examId') ?? undefined
    const sessionId = params.get('sessionId') ?? undefined
    const submissionId = params.get('submissionId') ?? undefined
    const {data: exam, isLoading} = useExamWithQuestionsQuery(examId)
    const {user} = useAuth()

    if (isLoading) return <div className="flex justify-center py-20"><Spin size="large"/></div>
    if (!exam) return <div className="flex justify-center py-20"><Empty description="Không tìm thấy đề thi"/></div>

    return <ExamRunner exam={exam} studentId={user?.id} sessionId={sessionId} submissionId={submissionId}/>
}

function ExamRunner({exam, studentId, sessionId, submissionId}: {
    exam: Exam; studentId?: string; sessionId?: string; submissionId?: string
}) {
    const navigate = useNavigate()
    const submit = useSubmitExamMutation()

    const questions = useMemo(
        () => [...(exam.questions ?? [])].sort((a, b) => a.sortOrder - b.sortOrder),
        [exam],
    )

    const [current, setCurrent] = useState(0)
    const [answers, setAnswers] = useState<Record<string, AnswerState>>({})
    const [timeLeft, setTimeLeft] = useState(() => exam.durationMinutes * 60)

    useEffect(() => {
        const id = setInterval(() => setTimeLeft(t => (t > 0 ? t - 1 : 0)), 1000)
        return () => clearInterval(id)
    }, [])

    const q = questions[current]
    const options = parseAnswers(q?.answersSnapshot)
    const isEssay = options.length === 0
    const answeredCount = Object.values(answers).filter(a => (a.selectedAnswerIds?.length || a.essayContent)).length

    const mm = String(Math.floor(timeLeft / 60)).padStart(2, '0')
    const ss = String(timeLeft % 60).padStart(2, '0')

    const selectOption = (answerId: string) =>
        setAnswers(prev => ({...prev, [q.id]: {selectedAnswerIds: [answerId]}}))

    const setEssay = (text: string) =>
        setAnswers(prev => ({...prev, [q.id]: {essayContent: text}}))

    const doSubmit = async () => {
        if (!studentId) {
            message.error('Không xác định được học sinh đang đăng nhập')
            return
        }
        const body: ExamSubmissionBody = {
            examId: exam.id,
            studentId,
            answers: questions.map(eq => ({
                examQuestionId: eq.id,
                selectedAnswerIds: answers[eq.id]?.selectedAnswerIds,
                essayContent: answers[eq.id]?.essayContent,
            })),
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

    const confirmSubmit = () => {
        const unanswered = questions.length - answeredCount
        Modal.confirm({
            title: 'Nộp bài thi?',
            content: unanswered > 0 ? `Còn ${unanswered} câu chưa trả lời.` : 'Bạn đã trả lời tất cả các câu.',
            okText: 'Nộp bài',
            cancelText: 'Tiếp tục làm',
            onOk: doSubmit,
        })
    }

    return (
        <div className="h-screen flex flex-col overflow-hidden bg-gray-100">
            <div className="taking-topbar">
                <div className="flex items-center gap-2 shrink-0">
                    <div className="student-logo-icon">EH</div>
                    <span className="font-bold text-white text-[14px]">ExamHub</span>
                </div>
                <div className="flex-1 text-center">
                    <p className="text-white font-semibold text-[15px] leading-tight">{exam.title}</p>
                    <p className="text-gray-400 text-[11px] mt-0.5">
                        {exam.examCode ? `Mã đề: ${exam.examCode} · ` : ''}{exam.subjectName ?? ''}
                    </p>
                </div>
                <div className={`taking-timer ${timeLeft < 300 ? 'animate-pulse' : ''}`}>{mm}:{ss}</div>
            </div>

            <div className="flex-1 flex overflow-hidden">
                <div className="flex-[3] flex flex-col overflow-hidden bg-white border-r border-gray-100">
                    <div className="flex items-center justify-between px-6 py-3 border-b border-gray-100 bg-gray-50 shrink-0">
                        <span className="text-sm font-semibold text-gray-700">Câu hỏi {current + 1} / {questions.length}</span>
                        <span className={`badge ${answers[q.id] ? 'badge-orange' : 'badge-gray'}`}>
                            {answers[q.id] ? 'Đã trả lời' : 'Chưa trả lời'}
                        </span>
                    </div>

                    <div className="flex-1 overflow-auto px-8 py-6">
                        <p className="text-[15px] text-gray-800 leading-relaxed mb-4">
                            Câu {current + 1}{q.score != null ? ` (${q.score}đ)` : ''}: {stripHtml(q.contentSnapshot)}
                        </p>

                        {isEssay ? (
                            <Input.TextArea
                                rows={8}
                                placeholder="Nhập câu trả lời tự luận..."
                                value={answers[q.id]?.essayContent ?? ''}
                                onChange={e => setEssay(e.target.value)}
                            />
                        ) : (
                            <div className="flex flex-col gap-2 mt-2">
                                {options.map((opt, i) => {
                                    const selected = answers[q.id]?.selectedAnswerIds?.[0] === opt.id
                                    return (
                                        <div key={opt.id || i} onClick={() => selectOption(opt.id)}
                                             className={`exam-answer-opt ${selected ? 'exam-answer-opt--selected' : ''}`}>
                                            <div className={`answer-circle ${selected ? 'answer-circle--selected' : ''}`}>
                                                {String.fromCharCode(65 + i)}
                                            </div>
                                            <span className={`flex-1 text-[14px] leading-relaxed pt-0.5 ${selected ? 'text-white font-medium' : 'text-gray-700'}`}>
                                                {stripHtml(opt.content)}
                                            </span>
                                        </div>
                                    )
                                })}
                            </div>
                        )}
                    </div>

                    <div className="flex items-center justify-between px-6 py-3.5 border-t border-gray-100 bg-white shrink-0">
                        <Button disabled={current === 0} onClick={() => setCurrent(c => c - 1)}>← Câu trước</Button>
                        <span className="text-sm text-gray-500">Câu {current + 1} / {questions.length}</span>
                        <Button type="primary" disabled={current === questions.length - 1} onClick={() => setCurrent(c => c + 1)}>Câu tiếp →</Button>
                    </div>
                </div>

                <div className="w-72 shrink-0 flex flex-col bg-white overflow-hidden">
                    <div className="flex-1 overflow-auto p-4 flex flex-col gap-4">
                        <div className="grid grid-cols-2 gap-2">
                            <div className="bg-gray-50 rounded-xl p-2.5 text-center border border-gray-100">
                                <p className="text-xl font-bold text-orange-500">{answeredCount}</p>
                                <p className="text-[10px] text-gray-400 mt-0.5">Đã trả lời</p>
                            </div>
                            <div className="bg-gray-50 rounded-xl p-2.5 text-center border border-gray-100">
                                <p className="text-xl font-bold text-gray-500">{questions.length - answeredCount}</p>
                                <p className="text-[10px] text-gray-400 mt-0.5">Chưa trả lời</p>
                            </div>
                        </div>
                        <div className="grid grid-cols-5 gap-1.5">
                            {questions.map((eq, idx) => {
                                let cls = 'q-grid-btn'
                                if (idx === current) cls += ' q-grid-btn--current'
                                else if (answers[eq.id]) cls += ' q-grid-btn--answered'
                                return <button key={eq.id} className={cls} onClick={() => setCurrent(idx)}>{idx + 1}</button>
                            })}
                        </div>
                    </div>
                    <div className="p-4 border-t border-gray-100 shrink-0">
                        <Button type="primary" block loading={submit.isPending} onClick={confirmSubmit}
                                className="!bg-green-500 hover:!bg-green-600 !h-11">
                            Nộp bài thi
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    )
}
