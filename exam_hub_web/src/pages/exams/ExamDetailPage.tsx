import {useNavigate, useParams} from 'react-router-dom'
import {Empty, Spin, message} from 'antd'
import {ArrowLeftOutlined} from '@ant-design/icons'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {examService} from '../../services/examService'
import {parseAnswers, stripHtml} from '../../utils/snapshot'
import {StatusTag} from '../../components/StatusTag'
import {EXAM_STATUS_LABEL, EXAM_STATUS_VARIANT, OPTION_LETTER} from '../../constants'
import {ROUTES} from '../../routes/paths'

export default function ExamDetailPage() {
    const {id} = useParams<{id: string}>()
    const navigate = useNavigate()
    const {data: exam, isLoading} = useExamWithQuestionsQuery(id)

    const handleExport = async (format: ExportFormat) => {
        if (!id) return
        try {
            const res = await examService.export(id, format)
            if (res.data?.url) window.open(res.data.url, '_blank')
            else message.error(res.message || 'Xuất file thất bại')
        } catch {
            message.error('Xuất file thất bại')
        }
    }

    const questions = exam?.questions ?? []

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Xem chi tiết đề thi</p>
                    <p className="top-bar-subtitle">
                        {exam ? `${exam.title}${exam.examCode ? ` · Mã đề ${exam.examCode}` : ''}` : 'Đang tải…'}
                    </p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                {isLoading && <div className="flex justify-center py-16"><Spin size="large"/></div>}
                {!isLoading && !exam && <Empty description="Không tìm thấy đề thi"/>}

                {exam && (
                    <>
                        {/* Action bar */}
                        <div className="flex items-center justify-between">
                            <button className="text-blue-600 text-sm hover:underline flex items-center gap-1"
                                    onClick={() => navigate(ROUTES.EXAM_LIST)}>
                                <ArrowLeftOutlined/> Danh sách đề đã tạo
                            </button>
                            <div className="flex items-center gap-2">
                                <button className="btn-primary-sm" onClick={() => void handleExport('pdf')}>Xuất PDF</button>
                                <button className="btn-neutral-sm" onClick={() => void handleExport('docx')}>Xuất Word</button>
                            </div>
                        </div>

                        {/* Info card */}
                        <div className="section-card shrink-0 p-5 flex items-start justify-between gap-4">
                            <div>
                                <h2 className="text-lg font-semibold text-gray-800">{exam.title}</h2>
                                <p className="text-sm text-gray-500 mt-2 flex flex-wrap gap-x-4 gap-y-1">
                                    {exam.examCode && <span>Mã đề <b className="font-medium">{exam.examCode}</b></span>}
                                    {exam.gradeLevelName && <span>Lớp {exam.gradeLevelName}</span>}
                                    {exam.subjectName && <span>Môn {exam.subjectName}</span>}
                                    <span>{questions.length} câu hỏi</span>
                                    <span>{exam.durationMinutes} phút</span>
                                    <span>Tổng điểm {exam.totalScore}</span>
                                </p>
                            </div>
                            <StatusTag status={EXAM_STATUS_VARIANT[exam.status]} label={EXAM_STATUS_LABEL[exam.status]}/>
                        </div>

                        {/* Questions card */}
                        <div className="section-card shrink-0 p-5">
                            <div className="flex items-center justify-between border-b border-gray-100 pb-3 mb-4">
                                <h3 className="text-sm font-semibold text-gray-800">Nội dung đề thi</h3>
                                <span className="text-xs text-gray-400">{questions.length} câu · Tổng {exam.totalScore} điểm</span>
                            </div>
                            <div className="flex flex-col gap-5">
                                {questions.map((q, i) => {
                                    const options = parseAnswers(q.answersSnapshot)
                                    return (
                                        <div key={q.id} className={i < questions.length - 1 ? 'border-b border-gray-100 pb-5' : ''}>
                                            <div className="flex items-center gap-3 mb-1">
                                                <span className="font-bold text-gray-800">Câu {i + 1}</span>
                                                {q.sectionName && <span className="text-xs text-gray-400">{q.sectionName}</span>}
                                                {q.score != null && <span className="text-xs text-gray-400">· {q.score} điểm</span>}
                                            </div>
                                            <p className="text-[13px] text-gray-700 font-medium">{stripHtml(q.contentSnapshot)}</p>
                                            {options.length > 0 && (
                                                <div className="grid grid-cols-2 gap-x-8 gap-y-1 mt-2">
                                                    {options.map((a, ai) => (
                                                        <span key={a.id || ai}
                                                              className={`text-[13px] ${a.isCorrect ? 'text-green-600 font-semibold' : 'text-gray-700'}`}>
                                                            {OPTION_LETTER[ai]}. {stripHtml(a.content)}{a.isCorrect ? '  ✓' : ''}
                                                        </span>
                                                    ))}
                                                </div>
                                            )}
                                        </div>
                                    )
                                })}
                            </div>
                        </div>
                    </>
                )}
            </div>
        </>
    )
}
