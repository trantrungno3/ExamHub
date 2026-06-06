import {useState} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Checkbox, Empty, Spin} from 'antd'
import {BarChartOutlined, ClockCircleOutlined, QuestionCircleOutlined, WarningOutlined} from '@ant-design/icons'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {useAuth} from '../../AuthProvider'

export default function ExamCoverPage() {
    const [agreed, setAgreed] = useState(false)
    const navigate = useNavigate()
    const [params] = useSearchParams()
    const {user} = useAuth()
    const examId = params.get('examId') ?? undefined
    const {data: exam, isLoading} = useExamWithQuestionsQuery(examId)

    const infoCards = exam ? [
        {icon: <ClockCircleOutlined/>, value: `${exam.durationMinutes} phút`, label: 'Thời gian'},
        {icon: <QuestionCircleOutlined/>, value: `${exam.questions?.length ?? 0} câu`, label: 'Số câu hỏi'},
        {icon: <BarChartOutlined/>, value: `${exam.totalScore} điểm`, label: 'Tổng điểm'},
    ] : []

    return (
        <div className="min-h-screen bg-gray-100 flex flex-col">
            <nav className="student-navbar">
                <div className="flex items-center gap-2.5">
                    <div className="student-logo-icon">EH</div>
                    <span className="font-bold text-gray-800 text-[15px]">ExamHub</span>
                </div>
                <div className="flex items-center gap-3 text-sm">
                    <span className="text-gray-700 font-medium">{user?.displayName ?? user?.userName ?? 'Học sinh'}</span>
                </div>
            </nav>

            {isLoading && <div className="flex justify-center py-20"><Spin size="large"/></div>}
            {!isLoading && !exam && (
                <div className="flex justify-center py-20">
                    <Empty description="Không tìm thấy đề thi. Thiếu tham số ?examId="/>
                </div>
            )}

            {exam && (
                <>
                    <div className="exam-hero">
                        <h1 className="exam-hero-title">{exam.title}</h1>
                        <p className="exam-hero-sub">
                            {exam.schoolYear ? `Năm học ${exam.schoolYear}` : ''}
                            {exam.className ? ` · ${exam.className}` : ''}
                        </p>
                        <div className="flex gap-4 max-w-2xl mx-auto">
                            {infoCards.map(c => (
                                <div key={c.label} className="exam-info-card">
                                    <div className="exam-info-card-icon">{c.icon}</div>
                                    <p className="exam-info-card-value">{c.value}</p>
                                    <p className="exam-info-card-label">{c.label}</p>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="flex justify-center px-4 py-10 -mt-8">
                        <div className="bg-white rounded-2xl shadow-lg w-full max-w-[560px] p-8">
                            <h2 className="text-[17px] font-semibold text-gray-800 mb-5">Thông tin bài thi</h2>
                            <div className="border border-gray-100 rounded-xl overflow-hidden mb-5">
                                <div className="exam-detail-row">
                                    <span className="exam-detail-label">Mã đề thi:</span>
                                    <span className="exam-detail-value">{exam.examCode ?? '—'}</span>
                                </div>
                                <div className="exam-detail-row">
                                    <span className="exam-detail-label">Môn học:</span>
                                    <span className="exam-detail-value">{exam.subjectName ?? '—'}</span>
                                </div>
                                <div className="exam-detail-row">
                                    <span className="exam-detail-label">Số câu:</span>
                                    <span className="exam-detail-value">{exam.questions?.length ?? 0} câu</span>
                                </div>
                            </div>

                            <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3.5 mb-5">
                                <div className="flex gap-3 items-start text-amber-800">
                                    <WarningOutlined className="text-amber-500 text-base mt-0.5 shrink-0"/>
                                    <div className="text-[13px] leading-5">
                                        <p>Sau khi bắt đầu, đồng hồ đếm ngược sẽ chạy.</p>
                                        <p>Không thể tạm dừng hoặc thoát ra khỏi bài thi.</p>
                                    </div>
                                </div>
                            </div>

                            <div className="mb-6">
                                <Checkbox checked={agreed} onChange={e => setAgreed(e.target.checked)}>
                                    <span className="text-[13px] text-gray-600">Tôi đã đọc và hiểu các quy định của bài thi</span>
                                </Checkbox>
                            </div>

                            <button
                                onClick={() => agreed && navigate(`/student/exam/take?examId=${exam.id}`)}
                                disabled={!agreed}
                                className={`w-full py-3.5 rounded-xl text-white font-semibold text-[15px] transition-all ${
                                    agreed ? 'bg-blue-600 hover:bg-blue-700 cursor-pointer shadow-md shadow-blue-200'
                                        : 'bg-blue-300 cursor-not-allowed'
                                }`}
                            >
                                Bắt đầu làm bài →
                            </button>
                        </div>
                    </div>
                </>
            )}
        </div>
    )
}
