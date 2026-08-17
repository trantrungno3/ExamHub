import type {ReactNode} from 'react'
import {useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Collapse, Drawer, Empty, InputNumber, message, Spin, Tag} from 'antd'
import {CheckCircleFilled, CloseCircleFilled, MinusCircleOutlined} from '@ant-design/icons'
import {useAuth} from '../../AuthProvider'
import {ROUTES} from '../../routes/paths'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {
    useFinalizeSubmissionMutation,
    useGradeAnswerMutation,
    useSubmissionQuery,
    useSubmissionsBySessionQuery,
} from '../../hooks/queries/useSubmissions'
import {parseAnswers, stripHtml} from '../../utils/snapshot'

type Props = { sessionId?: string; onClose: () => void }

const STATUS_COLOR: Record<SubmissionStatus, string> = {InProgress: 'default', Submitted: 'gold', Graded: 'green'}
const STATUS_LABEL: Record<SubmissionStatus, string> = {
    InProgress: 'Đang làm', Submitted: 'Đã nộp (chờ chấm)', Graded: 'Đã chấm',
}

export function SubmissionsDrawer({sessionId, onClose}: Props) {
    const {data: submissions, isLoading} = useSubmissionsBySessionQuery(sessionId)

    return (
        <Drawer title="Bài nộp kỳ thi" open={!!sessionId} onClose={onClose} width={680}>
            {isLoading && <Spin/>}
            {!isLoading && (submissions?.length ?? 0) === 0 && <Empty description="Chưa có bài nộp"/>}
            <div className="flex flex-col gap-3">
                {(submissions ?? []).map(s => (
                    <SubmissionCard key={s.id} submissionId={s.id}
                                    studentName={s.studentName} studentClassName={s.studentClassName}/>
                ))}
            </div>
        </Drawer>
    )
}

function SubmissionCard({submissionId, studentName, studentClassName}: {
    submissionId: string; studentName?: string; studentClassName?: string
}) {
    const {user} = useAuth()
    const navigate = useNavigate()
    const {data: sub, isLoading} = useSubmissionQuery(submissionId)
    const exam = useExamWithQuestionsQuery(sub?.examId)
    const grade = useGradeAnswerMutation()
    const finalize = useFinalizeSubmissionMutation()
    const [scores, setScores] = useState<Record<string, number>>({})

    if (isLoading || !sub) return <div className="section-card p-4"><Spin/></div>

    const questionOf = (examQuestionId: string) =>
        exam.data?.questions?.find(q => q.id === examQuestionId)
    const questionContent = (examQuestionId: string) => stripHtml(questionOf(examQuestionId)?.contentSnapshot)

    // Phân loại theo LOẠI câu hỏi (có option trong snapshot = trắc nghiệm), không dựa vào việc
    // học sinh có chọn đáp án hay không — nếu không, câu trắc nghiệm bỏ trống sẽ bị coi là tự luận.
    const isEssayQuestion = (examQuestionId: string) => parseAnswers(questionOf(examQuestionId)?.answersSnapshot).length === 0

    const answers = sub.answers ?? []
    const objectives = answers.filter(a => !isEssayQuestion(a.examQuestionId))
    const essays = answers.filter(a => isEssayQuestion(a.examQuestionId))
    const objectiveScore = objectives.reduce((s, a) => s + (a.scoreEarned ?? 0), 0)
    const correctCount = objectives.filter(a => a.isCorrect === true).length

    const submitGrade = (answerId: string) => {
        if (!user?.id) {
            message.error('Không xác định được giáo viên đang đăng nhập')
            return
        }
        const score = scores[answerId] ?? 0
        grade.mutate({answerId, body: {scoreEarned: score, isCorrect: score > 0, gradedBy: user.id}})
    }

    return (
        <div className="section-card p-4">
            <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-gray-700">
                    {studentName || `HS ${sub.studentId.slice(0, 8)}…`}
                    {studentClassName && <span className="text-gray-400 font-normal"> · Lớp {studentClassName}</span>}
                </span>
                <div className="flex items-center gap-2">
                    <button className="text-blue-600 text-[13px] hover:underline"
                            onClick={() => navigate(
                                ROUTES.SUBMISSION_REVIEW.replace(':id', submissionId),
                                {state: {studentName, studentClassName}},
                            )}>
                        Xem bài làm
                    </button>
                    <Tag color={STATUS_COLOR[sub.status]}>{STATUS_LABEL[sub.status]}</Tag>
                    {sub.totalScore != null && <span className="text-sm font-semibold">{sub.totalScore} đ</span>}
                </div>
            </div>

            {objectives.length === 0 && essays.length === 0 && (
                <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Chưa có câu trả lời"/>
            )}

            {(objectives.length > 0 || essays.length > 0) && (
                <Collapse
                    size="small"
                    defaultActiveKey={essays.length > 0 ? ['essays'] : ['objectives']}
                    items={[
                        objectives.length > 0 && {
                            key: 'objectives',
                            label: `Trắc nghiệm — đã tự chấm (${correctCount}/${objectives.length} đúng · ${objectiveScore}đ)`,
                            children: (
                                <div className="flex flex-col gap-2">
                                    {objectives.map((a, i) => (
                                        <div key={a.id}
                                             className="flex items-start gap-2 border-b border-gray-100 pb-2">
                                            <span className="mt-0.5">
                                                {a.isCorrect === true &&
                                                    <CheckCircleFilled style={{color: '#1ea375'}}/>}
                                                {a.isCorrect === false &&
                                                    <CloseCircleFilled style={{color: '#e74242'}}/>}
                                                {a.isCorrect == null &&
                                                    <MinusCircleOutlined style={{color: '#c0c4cc'}}/>}
                                            </span>
                                            <p className="flex-1 text-[13px] text-gray-700">
                                                <span className="text-gray-400 mr-1">Câu {i + 1}.</span>
                                                {questionContent(a.examQuestionId)}
                                                {a.isCorrect == null &&
                                                    <span className="text-gray-400"> (bỏ trống)</span>}
                                            </p>
                                            <span
                                                className="text-[13px] font-semibold tabular-nums text-gray-600">{a.scoreEarned ?? 0}đ</span>
                                        </div>
                                    ))}
                                </div>
                            ),
                        },
                        essays.length > 0 && {
                            key: 'essays',
                            label: `Chấm ${essays.length} câu tự luận`,
                            children: (
                                <div className="flex flex-col gap-3">
                                    {essays.map(a => (
                                        <div key={a.id} className="border-b border-gray-100 pb-2">
                                            <p className="text-[13px] text-gray-700 font-medium">{questionContent(a.examQuestionId)}</p>
                                            <p className="text-[13px] text-gray-500 mt-1 whitespace-pre-wrap">{a.essayContent || '(trống)'}</p>
                                            <div className="flex items-center gap-2 mt-2">
                                                <InputNumber min={0} max={10} step={0.5} placeholder="Điểm"
                                                             value={scores[a.id] ?? a.scoreEarned}
                                                             onChange={v => setScores(p => ({...p, [a.id]: v ?? 0}))}/>
                                                <Button size="small" loading={grade.isPending}
                                                        onClick={() => submitGrade(a.id)}>Lưu điểm</Button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ),
                        },
                    ].filter(Boolean) as { key: string; label: string; children: ReactNode }[]}
                />
            )}

            <Button type="primary" size="small" className="!mt-3" loading={finalize.isPending}
                    onClick={() => finalize.mutate(sub.id)}>
                Chốt điểm
            </Button>
        </div>
    )
}
