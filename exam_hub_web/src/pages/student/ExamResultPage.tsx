import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Empty, Spin, Tag} from 'antd'
import {CheckCircleOutlined, CloseCircleOutlined} from '@ant-design/icons'
import {useSubmissionQuery} from '../../hooks/queries/useSubmissions'

const STATUS_LABEL: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Đã nộp (chờ chấm)', Graded: 'Đã chấm',
}

export default function ExamResultPage() {
    const navigate = useNavigate()
    const [params] = useSearchParams()
    const submissionId = params.get('submissionId') ?? undefined
    const {data: sub, isLoading} = useSubmissionQuery(submissionId)

    if (isLoading) return <div className="exam-desk flex justify-center py-24"><Spin size="large"/></div>
    if (!sub) return <div className="exam-desk flex justify-center py-24"><Empty description="Không tìm thấy bài nộp"/></div>

    const graded = sub.status === 'Graded'
    const answers = sub.answers ?? []

    return (
        <div className="exam-desk flex items-center justify-center px-4 py-10">
            <article className="exam-paper exam-paper--sheet">
                <div className="paper-eyebrow">Kết quả bài thi</div>
                <div className="paper-rule"/>

                <div className={`result-score ${graded ? 'result-score--graded' : 'result-score--pending'}`}>
                    {graded && sub.totalScore != null ? sub.totalScore : '—'}
                </div>
                <div className="text-center mt-3">
                    <Tag color={graded ? 'green' : 'gold'}>{STATUS_LABEL[sub.status]}</Tag>
                    {!graded && (
                        <p className="text-[12.5px] text-stone-400 mt-2 leading-5 max-w-[380px] mx-auto"
                            style={{fontFamily: "'Lora','Times New Roman',Georgia,serif"}}>
                            Phần trắc nghiệm đã chấm tự động; điểm tổng sẽ có sau khi giáo viên chốt điểm.
                        </p>
                    )}
                </div>

                <div className="paper-divider"/>

                <div>
                    {answers.map((a, i) => (
                        <div key={a.id} className="result-line">
                            <span className="font-semibold text-stone-800">Câu {i + 1}</span>
                            <span className="flex items-center gap-2.5">
                                {a.isCorrect === true && <CheckCircleOutlined className="text-emerald-500"/>}
                                {a.isCorrect === false && <CloseCircleOutlined className="text-red-500"/>}
                                <span className="text-stone-500 tabular-nums">{a.scoreEarned} đ</span>
                            </span>
                        </div>
                    ))}
                </div>

                <Button type="primary" block className="!mt-7 !h-11 !font-semibold" onClick={() => navigate('/student/exams')}>
                    Hoàn tất
                </Button>
            </article>
        </div>
    )
}
