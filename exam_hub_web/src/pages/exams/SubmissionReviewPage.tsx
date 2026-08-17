import {useLocation, useNavigate, useParams} from 'react-router-dom'
import {Empty, Spin} from 'antd'
import {ArrowLeftOutlined, PrinterOutlined} from '@ant-design/icons'
import {useSubmissionQuery} from '../../hooks/queries/useSubmissions'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {parseAnswers, stripHtml} from '../../utils/snapshot'

const STATUS_LABEL: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Chờ chấm', Graded: 'Đã chấm',
}
const OPTION_LETTER = ['A', 'B', 'C', 'D', 'E', 'F']

export default function SubmissionReviewPage() {
    const {id} = useParams<{id: string}>()
    const navigate = useNavigate()
    const {state} = useLocation() as {state?: {studentName?: string; studentClassName?: string}}
    const {data: sub, isLoading} = useSubmissionQuery(id)
    const {data: exam} = useExamWithQuestionsQuery(sub?.examId)

    if (isLoading) return <div className="flex justify-center py-24"><Spin size="large"/></div>
    if (!sub) return <div className="p-6"><Empty description="Không tìm thấy bài nộp"/></div>

    const questionOf = (eqId: string) => exam?.questions?.find(q => q.id === eqId)
    const answers = sub.answers ?? []
    // Câu trắc nghiệm = snapshot có option; tự luận = không có option.
    const objectives = answers.filter(a => parseAnswers(questionOf(a.examQuestionId)?.answersSnapshot).length > 0)
    const correctCount = objectives.filter(a => a.isCorrect === true).length
    // Ưu tiên tên từ BE (nếu Task 6 đã enrich getById) → router state (Task 5) → fallback id.
    const studentName = sub.studentName || state?.studentName || `HS ${sub.studentId.slice(0, 8)}…`
    const studentClassName = sub.studentClassName || state?.studentClassName

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Xem bài làm học sinh</p>
                    <p className="top-bar-subtitle">{exam?.title ?? 'Đang tải…'}</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                {/* Action bar */}
                <div className="flex items-center justify-between">
                    <button className="text-blue-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => navigate(-1)}>
                        <ArrowLeftOutlined/> Danh sách bài nộp
                    </button>
                    <button className="btn-neutral-sm inline-flex items-center gap-1" onClick={() => window.print()}>
                        <PrinterOutlined/> Xuất bảng điểm
                    </button>
                </div>

                {/* Student + score card */}
                <div className="section-card p-5 flex items-start justify-between gap-4">
                    <div className="flex items-start gap-4">
                        <div className="w-14 h-14 rounded-full flex items-center justify-center text-lg font-semibold"
                             style={{background: '#e9ecfe', color: '#3a74f5'}}>
                            {studentName.slice(0, 2).toUpperCase()}
                        </div>
                        <div>
                            <h2 className="text-lg font-semibold text-gray-800">{studentName}</h2>
                            <p className="text-sm text-gray-500 mt-1">
                                {studentClassName && <span>Lớp {studentClassName} · </span>}
                                {sub.submittedAt && <span>Nộp lúc {new Date(sub.submittedAt).toLocaleString('vi-VN')}</span>}
                            </p>
                        </div>
                    </div>
                    <div className="rounded-xl px-5 py-3 text-right" style={{background: '#f0f4ff'}}>
                        <span className="text-3xl font-bold" style={{color: '#3a74f5'}}>
                            {sub.totalScore != null ? sub.totalScore : '—'}
                        </span>
                        <span className="text-sm text-gray-500"> / 10</span>
                        <p className="text-xs text-gray-500 mt-1">
                            {STATUS_LABEL[sub.status]} · Đúng {correctCount}/{objectives.length} câu
                        </p>
                    </div>
                </div>

                {/* Answer sheet */}
                <div className="section-card p-5">
                    <div className="flex items-center justify-between border-b border-gray-100 pb-3 mb-4">
                        <h3 className="text-sm font-semibold text-gray-800">Chi tiết bài làm</h3>
                        <span className="text-xs text-gray-400">
                            Đúng {correctCount}/{objectives.length}{sub.totalScore != null ? ` · ${sub.totalScore} điểm` : ''}
                        </span>
                    </div>
                    <div className="flex flex-col gap-5">
                        {answers.map((a, i) => {
                            const q = questionOf(a.examQuestionId)
                            const options = parseAnswers(q?.answersSnapshot)
                            const isEssay = options.length === 0
                            const chosen = options.filter(o => (a.selectedAnswerIds ?? []).includes(o.id))
                            const correct = options.filter(o => o.isCorrect)
                            const ok = a.isCorrect === true
                            const fmt = (arr: typeof options) =>
                                arr.map(o => `${OPTION_LETTER[options.indexOf(o)]}. ${stripHtml(o.content)}`).join('; ') || '(bỏ trống)'
                            return (
                                <div key={a.id} className={i < answers.length - 1 ? 'border-b border-gray-100 pb-5' : ''}>
                                    <div className="flex items-center justify-between mb-1">
                                        <div className="flex items-center gap-3">
                                            <span className="font-bold text-gray-800">Câu {i + 1}</span>
                                            {q?.sectionName && <span className="text-xs text-gray-400">{q.sectionName}</span>}
                                        </div>
                                        <div className="flex items-center gap-3">
                                            {!isEssay && (
                                                <span className="text-xs font-semibold px-2 py-0.5 rounded-full"
                                                      style={ok
                                                          ? {background: '#dff5ed', color: '#1ea375'}
                                                          : {background: '#fee2e2', color: '#dc3c3c'}}>
                                                    {ok ? 'Đúng' : 'Sai'}
                                                </span>
                                            )}
                                            <span className="text-[13px] font-semibold" style={{color: ok ? '#1ea375' : '#6f7788'}}>
                                                {a.scoreEarned ?? 0} đ
                                            </span>
                                        </div>
                                    </div>
                                    <p className="text-[13px] text-gray-700 font-medium">{stripHtml(q?.contentSnapshot)}</p>
                                    {isEssay ? (
                                        <p className="text-[13px] text-gray-500 mt-1 whitespace-pre-wrap">
                                            Bài làm: {a.essayContent || '(trống)'}
                                        </p>
                                    ) : (
                                        <div className="grid grid-cols-2 gap-x-8 mt-1">
                                            <span className="text-[13px] font-semibold" style={{color: ok ? '#1ea375' : '#dc3c3c'}}>
                                                HS chọn: {fmt(chosen)} {ok ? '✓' : '✗'}
                                            </span>
                                            <span className="text-[13px] font-medium" style={{color: '#1ea375'}}>
                                                Đáp án đúng: {fmt(correct)}
                                            </span>
                                        </div>
                                    )}
                                </div>
                            )
                        })}
                    </div>
                </div>
            </div>
        </>
    )
}
