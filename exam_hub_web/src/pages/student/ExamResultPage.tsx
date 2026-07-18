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

    if (isLoading) return <div className="flex justify-center py-20"><Spin size="large"/></div>
    if (!sub) return <div className="flex justify-center py-20"><Empty description="Không tìm thấy bài nộp"/></div>

    const graded = sub.status === 'Graded'

    return (
        <div className="min-h-screen bg-gray-100 flex flex-col items-center py-12 px-4">
            <div className="bg-white rounded-2xl shadow-lg w-full max-w-[600px] p-8">
                <div className="text-center mb-6">
                    <p className="text-gray-500 text-sm">Kết quả bài thi</p>
                    <p className="text-4xl font-bold text-blue-600 mt-2">
                        {graded && sub.totalScore != null ? sub.totalScore : '—'}
                    </p>
                    <Tag className="mt-2" color={graded ? 'green' : 'gold'}>{STATUS_LABEL[sub.status]}</Tag>
                    {!graded && (
                        <p className="text-[12px] text-gray-400 mt-2">
                            Phần trắc nghiệm đã chấm tự động; điểm tổng sẽ có sau khi giáo viên chốt điểm.
                        </p>
                    )}
                </div>

                <div className="flex flex-col gap-2">
                    {(sub.answers ?? []).map((a, i) => (
                        <div key={a.id} className="flex items-center justify-between border-b border-gray-100 py-2">
                            <span className="text-sm text-gray-700">Câu {i + 1}</span>
                            <span className="flex items-center gap-2 text-sm">
                                {a.isCorrect === true && <CheckCircleOutlined className="text-green-500"/>}
                                {a.isCorrect === false && <CloseCircleOutlined className="text-red-500"/>}
                                <span className="text-gray-500">{a.scoreEarned} đ</span>
                            </span>
                        </div>
                    ))}
                </div>

                <Button type="primary" block className="!mt-6" onClick={() => navigate('/student/exams')}>
                    Hoàn tất
                </Button>
            </div>
        </div>
    )
}
