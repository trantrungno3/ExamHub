import {useState} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Empty, Spin} from 'antd'
import {CheckCircleOutlined, CheckOutlined, CloseCircleOutlined} from '@ant-design/icons'
import {useSubmissionQuery} from '../../hooks/queries/useSubmissions'
import {SUBMISSION_STATUS_LABEL_STUDENT} from '../../constants'

function fmtDuration(sec: number): string {
    const m = Math.floor(sec / 60)
    const s = sec % 60
    return s ? `${m} phút ${s} giây` : `${m} phút`
}

function StatBox({label, value, tone}: {label: string; value: number | string; tone: 'green' | 'red' | 'gray' | 'blue'}) {
    const c = {
        green: {bg: '#e7f7ef', fg: '#1ea375'},
        red: {bg: '#fde9e9', fg: '#e74242'},
        gray: {bg: '#eef0f3', fg: '#6f7788'},
        blue: {bg: '#eef1ff', fg: '#3a74f5'},
    }[tone]
    return (
        <div className="rounded-xl px-4 py-3.5" style={{background: c.bg}}>
            <div className="text-[20px] font-bold leading-tight" style={{color: c.fg}}>{value}</div>
            <div className="text-[12px] mt-0.5" style={{color: '#6f6a60'}}>{label}</div>
        </div>
    )
}

export default function ExamResultPage() {
    const navigate = useNavigate()
    const [params] = useSearchParams()
    const submissionId = params.get('submissionId') ?? undefined
    const {data: sub, isLoading} = useSubmissionQuery(submissionId)
    const [showDetail, setShowDetail] = useState(false)

    if (isLoading) return <div className="exam-desk flex justify-center py-24"><Spin size="large"/></div>
    if (!sub) return <div className="exam-desk flex justify-center py-24"><Empty description="Không tìm thấy bài nộp"/></div>

    const graded = sub.status === 'Graded'
    const answers = sub.answers ?? []
    const correct = answers.filter(a => a.isCorrect === true).length
    const wrong = answers.filter(a => a.isCorrect === false).length
    const blank = answers.length - correct - wrong
    const pass = graded && (sub.isPassed ?? (sub.totalScore != null && sub.totalScore >= 5))

    return (
        <div className="min-h-full" style={{background: '#f5f4f1'}}>
            {/* Hero xanh */}
            <div className="px-4 pt-10 pb-24 text-center text-white" style={{background: '#3a74f5'}}>
                <div className="mx-auto w-14 h-14 rounded-full flex items-center justify-center text-[24px]"
                     style={{background: pass ? '#1ea375' : 'rgba(255,255,255,0.18)'}}>
                    {pass ? <CheckOutlined/> : '📝'}
                </div>
                <h1 className="mt-3 text-[26px] font-bold">
                    {graded ? (pass ? 'Bạn đã ĐẠT!' : 'Chưa đạt') : 'Đã nộp bài'}
                </h1>
                <p className="mt-1 text-[14px]" style={{color: '#cdd9fb'}}>{SUBMISSION_STATUS_LABEL_STUDENT[sub.status]}</p>
            </div>

            {/* Card kết quả */}
            <div className="max-w-2xl mx-auto px-4 -mt-16 pb-12">
                <div className="bg-white rounded-2xl border p-6 sm:p-7" style={{borderColor: '#eceef2'}}>
                    <div className={`result-score ${graded ? 'result-score--graded' : 'result-score--pending'}`}>
                        {graded && sub.totalScore != null ? sub.totalScore : '—'}
                    </div>
                    <p className="text-center text-[12px] font-semibold uppercase tracking-wider mt-1"
                       style={{color: graded ? (pass ? '#1ea375' : '#e74242') : '#9aa2b1'}}>
                        {graded ? (pass ? 'ĐẠT' : 'CHƯA ĐẠT') : 'CHỜ CHẤM'}
                    </p>

                    {!graded && (
                        <p className="text-center text-[12.5px] mt-2 leading-5 max-w-[380px] mx-auto" style={{color: '#9aa2b1'}}>
                            Phần trắc nghiệm đã chấm tự động; điểm tổng sẽ có sau khi giáo viên chốt điểm.
                        </p>
                    )}

                    <div className="grid grid-cols-2 gap-3 mt-5">
                        <StatBox tone="green" label="Số câu đúng" value={correct}/>
                        <StatBox tone="red" label="Số câu sai" value={wrong}/>
                        <StatBox tone="gray" label={graded ? 'Bỏ trống' : 'Chưa chấm'} value={blank}/>
                        {sub.durationSeconds != null && (
                            <StatBox tone="blue" label="Thời gian làm" value={fmtDuration(sub.durationSeconds)}/>
                        )}
                    </div>

                    {showDetail && (
                        <div className="mt-6">
                            <p className="text-[13px] font-semibold mb-1" style={{color: '#6f7788'}}>Chi tiết theo câu</p>
                            {answers.map((a, i) => (
                                <div key={a.id} className="flex items-center justify-between py-2 text-[14px]"
                                     style={{borderBottom: '1px dashed #eceef2'}}>
                                    <span className="font-semibold" style={{color: '#1d2129'}}>Câu {i + 1}</span>
                                    <span className="flex items-center gap-2.5">
                                        {a.isCorrect === true && <CheckCircleOutlined style={{color: '#1ea375'}}/>}
                                        {a.isCorrect === false && <CloseCircleOutlined style={{color: '#e74242'}}/>}
                                        <span className="tabular-nums" style={{color: '#6f7788'}}>{a.scoreEarned} đ</span>
                                    </span>
                                </div>
                            ))}
                        </div>
                    )}

                    <div className="grid grid-cols-2 gap-3 mt-7">
                        <Button block className="!h-11 !font-semibold"
                                onClick={() => setShowDetail(v => !v)}>
                            {showDetail ? 'Ẩn đáp án chi tiết' : 'Xem đáp án chi tiết'}
                        </Button>
                        <Button type="primary" block className="!h-11 !font-semibold"
                                onClick={() => navigate('/student/exams')}>
                            Về danh sách kỳ thi
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    )
}
