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

    const qCount = exam.questions?.length ?? 0
    const stats: [string, string][] = [
        ['Thời gian', `${exam.durationMinutes} phút`],
        ['Số câu hỏi', `${qCount} câu`],
        ['Tổng điểm', `${exam.totalScore} điểm`],
        ['Mã đề', exam.examCode ?? '—'],
    ]
    const info: [string, string][] = [
        ['Môn học', exam.subjectName ?? '—'],
        ['Thí sinh', user?.displayName ?? user?.userName ?? '—'],
        ['Lớp', exam.className ?? '—'],
        ['Năm học', exam.schoolYear ?? '—'],
    ]

    return (
        <div className="min-h-full" style={{background: '#f5f4f1'}}>
            {/* Hero xanh */}
            <div className="px-4 pt-10 pb-24 text-center text-white" style={{background: '#3a74f5'}}>
                <h1 className="text-[30px] font-bold leading-tight">{exam.title}</h1>
                <p className="mt-1.5 text-[14px]" style={{color: '#cdd9fb'}}>
                    {exam.schoolYear ? `Năm học ${exam.schoolYear} · ` : ''}
                    {exam.className ? `Lớp ${exam.className}` : 'ExamHub'}
                </p>
                <div className="max-w-4xl mx-auto mt-7 grid grid-cols-2 sm:grid-cols-4 gap-4">
                    {stats.map(([k, v]) => (
                        <div key={k} className="rounded-xl px-4 py-4"
                             style={{background: 'rgba(255,255,255,0.14)', border: '1px solid rgba(255,255,255,0.22)'}}>
                            <div className="text-[20px] font-bold text-white leading-tight">{v}</div>
                            <div className="text-[12px] mt-1" style={{color: '#cdd9fb'}}>{k}</div>
                        </div>
                    ))}
                </div>
            </div>

            {/* Card trắng đè lên hero */}
            <div className="max-w-2xl mx-auto px-4 -mt-14 pb-12">
                <div className="bg-white rounded-2xl border p-6 sm:p-7" style={{borderColor: '#eceef2'}}>
                    <h2 className="text-[16px] font-semibold" style={{color: '#191d27'}}>Thông tin bài thi</h2>
                    <div className="mt-3">
                        {info.map(([k, v]) => (
                            <div key={k} className="flex items-center justify-between py-2.5 text-[14px]"
                                 style={{borderBottom: '1px dotted #e7e5e4'}}>
                                <span style={{color: '#6f7788'}}>{k}</span>
                                <span className="font-semibold" style={{color: '#1d2129'}}>{v}</span>
                            </div>
                        ))}
                    </div>

                    <div className="flex gap-3 items-start rounded-lg px-4 py-3 text-[13.5px] leading-6 mt-4"
                         style={{background: '#fff4e5', border: '1px solid #ffe0b2', color: '#b26a00'}}>
                        <WarningOutlined className="text-base mt-0.5 shrink-0"/>
                        <div>
                            <p>Sau khi bắt đầu, đồng hồ đếm ngược sẽ chạy và không thể tạm dừng.</p>
                            <p>Hết giờ hệ thống sẽ tự động nộp bài. Không thoát ra khỏi trang khi đang làm bài.</p>
                        </div>
                    </div>

                    <div className="my-5">
                        <Checkbox checked={agreed} onChange={e => setAgreed(e.target.checked)}>
                            <span className="text-[13.5px]" style={{color: '#3a4051'}}>Tôi đã đọc và hiểu các quy định của bài thi</span>
                        </Checkbox>
                    </div>

                    <Button type="primary" size="large" block disabled={!agreed} onClick={startTaking}
                        icon={<ClockCircleOutlined/>} className="!h-12 !text-[15px] !font-semibold">
                        Bắt đầu làm bài
                    </Button>
                </div>
            </div>
        </div>
    )
}
