import {useState} from 'react'
import {Button, Collapse, Drawer, Empty, InputNumber, Spin, Tag, message} from 'antd'
import {useAuth} from '../../AuthProvider'
import {useExamWithQuestionsQuery} from '../../hooks/queries/useExams'
import {
    useFinalizeSubmissionMutation,
    useGradeAnswerMutation,
    useSubmissionQuery,
    useSubmissionsByExamQuery,
} from '../../hooks/queries/useSubmissions'
import {stripHtml} from '../../utils/snapshot'

type Props = {examId?: string; onClose: () => void}

const STATUS_COLOR: Record<SubmissionStatus, string> = {InProgress: 'default', Submitted: 'gold', Graded: 'green'}

export function SubmissionsDrawer({examId, onClose}: Props) {
    const {data: submissions, isLoading} = useSubmissionsByExamQuery(examId)
    const exam = useExamWithQuestionsQuery(examId)

    const questionContent = (examQuestionId: string) =>
        stripHtml(exam.data?.questions?.find(q => q.id === examQuestionId)?.contentSnapshot)

    return (
        <Drawer title="Bài nộp & chấm điểm" open={!!examId} onClose={onClose} width={680}>
            {isLoading && <Spin/>}
            {!isLoading && (submissions?.length ?? 0) === 0 && <Empty description="Chưa có bài nộp"/>}
            <div className="flex flex-col gap-3">
                {(submissions ?? []).map(s => (
                    <SubmissionCard key={s.id} submissionId={s.id} questionContent={questionContent}/>
                ))}
            </div>
        </Drawer>
    )
}

function SubmissionCard({submissionId, questionContent}: {
    submissionId: string
    questionContent: (examQuestionId: string) => string
}) {
    const {user} = useAuth()
    const {data: sub, isLoading} = useSubmissionQuery(submissionId)
    const grade = useGradeAnswerMutation()
    const finalize = useFinalizeSubmissionMutation()
    const [scores, setScores] = useState<Record<string, number>>({})

    if (isLoading || !sub) return <div className="section-card p-4"><Spin/></div>

    const essays = (sub.answers ?? []).filter(a => !a.selectedAnswerIds || a.selectedAnswerIds.length === 0)

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
                <span className="text-sm font-medium text-gray-700">Học sinh: {sub.studentId.slice(0, 8)}…</span>
                <div className="flex items-center gap-2">
                    <Tag color={STATUS_COLOR[sub.status]}>{sub.status}</Tag>
                    {sub.totalScore != null && <span className="text-sm font-semibold">{sub.totalScore} đ</span>}
                </div>
            </div>

            {essays.length > 0 ? (
                <Collapse
                    size="small"
                    items={[{
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
                                            <Button size="small" loading={grade.isPending} onClick={() => submitGrade(a.id)}>Lưu điểm</Button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ),
                    }]}
                />
            ) : (
                <p className="text-[12px] text-gray-400">Toàn bộ trắc nghiệm — đã chấm tự động.</p>
            )}

            <Button type="primary" size="small" className="!mt-3" loading={finalize.isPending}
                    onClick={() => finalize.mutate(sub.id)}>
                Chốt điểm
            </Button>
        </div>
    )
}
