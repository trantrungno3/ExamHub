import {useState} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Checkbox, Empty, Spin} from 'antd'
import {ClockCircleOutlined, WarningOutlined} from '@ant-design/icons'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {useAuth} from '../../AuthProvider'

export default function ExamCoverPage() {
    const [agreed, setAgreed] = useState(false)
    const navigate = useNavigate()
    const [params] = useSearchParams()
    const {user} = useAuth()
    const examId = params.get('examId') ?? undefined
    const sessionId = params.get('sessionId') ?? undefined
    const submissionId = params.get('submissionId') ?? undefined
    const {data: exam, isLoading} = useExamWithQuestionsQuery(examId)

    const startTaking = () => {
        if (!agreed || !exam) return
        const q = new URLSearchParams({examId: exam.id})
        if (sessionId) q.set('sessionId', sessionId)
        if (submissionId) q.set('submissionId', submissionId)
        navigate(`/student/exam/take?${q.toString()}`)
    }

    if (isLoading) return <div className="exam-desk flex justify-center py-24"><Spin size="large"/></div>
    if (!exam) return (
        <div className="exam-desk flex justify-center py-24">
            <Empty description="Không tìm thấy đề thi. Thiếu tham số ?examId="/>
        </div>
    )

    const info: [string, string][] = [
        ['Môn học', exam.subjectName ?? '—'],
        ['Mã đề', exam.examCode ?? '—'],
        ['Thời gian làm bài', `${exam.durationMinutes} phút`],
        ['Số câu hỏi', `${exam.questions?.length ?? 0} câu`],
        ['Tổng điểm', `${exam.totalScore} điểm`],
    ]

    return (
        <div className="exam-desk flex items-center justify-center px-4 py-10">
            <article className="exam-paper exam-paper--sheet">
                <div className="paper-eyebrow">Phiếu dự thi{exam.schoolYear ? ` · Năm học ${exam.schoolYear}` : ''}</div>
                <h1 className="paper-title">{exam.title}</h1>
                <p className="paper-subtitle">
                    {exam.className ? `Lớp ${exam.className}` : 'ExamHub'} · Thí sinh: {user?.displayName ?? user?.userName ?? '—'}
                </p>
                <div className="paper-rule"/>

                <div className="mb-1">
                    {info.map(([k, v]) => (
                        <div key={k} className="paper-info-row">
                            <span className="paper-info-key">{k}</span>
                            <span className="paper-info-val">{v}</span>
                        </div>
                    ))}
                </div>

                <div className="paper-note">
                    <WarningOutlined className="text-amber-500 text-base mt-0.5 shrink-0"/>
                    <div>
                        <p>Sau khi bắt đầu, đồng hồ đếm ngược sẽ chạy và không thể tạm dừng.</p>
                        <p>Hết giờ hệ thống sẽ tự động nộp bài. Không thoát ra khỏi trang khi đang làm bài.</p>
                    </div>
                </div>

                <div className="my-6">
                    <Checkbox checked={agreed} onChange={e => setAgreed(e.target.checked)}>
                        <span className="text-[13.5px] text-stone-600">Tôi đã đọc và hiểu các quy định của bài thi</span>
                    </Checkbox>
                </div>

                <Button type="primary" size="large" block disabled={!agreed} onClick={startTaking}
                    icon={<ClockCircleOutlined/>} className="!h-12 !text-[15px] !font-semibold">
                    Bắt đầu làm bài
                </Button>
            </article>
        </div>
    )
}
