import {useNavigate} from 'react-router-dom'
import {Empty, List, Modal, Spin, Tag} from 'antd'
import {useMySessionSubmissionsQuery} from '../../hooks/queries/useSubmissions'
import {formatTimestamp} from '../../utils/datetime'
import {SUBMISSION_STATUS_LABEL_STUDENT, SUBMISSION_STATUS_TAG_COLOR} from '../../constants'

type Props = {sessionId?: string; studentId?: string; title?: string; onClose: () => void}

export function SessionResultsModal({sessionId, studentId, title, onClose}: Props) {
    const navigate = useNavigate()
    const {data: submissions, isLoading} = useMySessionSubmissionsQuery(sessionId, studentId)

    return (
        <Modal open={!!sessionId} onCancel={onClose} footer={null} title={title ?? 'Kết quả các lần thi'}>
            {isLoading && <div className="flex justify-center py-8"><Spin/></div>}
            {!isLoading && (submissions?.length ?? 0) === 0 && <Empty description="Chưa có lần nộp nào"/>}
            {!isLoading && (submissions?.length ?? 0) > 0 && (
                <List
                    dataSource={submissions ?? []}
                    renderItem={(s, i) => (
                        <List.Item
                            className="cursor-pointer hover:bg-stone-50 !px-2 rounded"
                            onClick={() => navigate(`/student/exam/result?submissionId=${s.id}`)}>
                            <div className="flex items-center justify-between w-full gap-3">
                                <span className="text-sm text-stone-700">
                                    Lần {(submissions?.length ?? 0) - i} · {formatTimestamp(s.submittedAt ?? s.createdAt)}
                                </span>
                                <div className="flex items-center gap-2">
                                    <Tag color={SUBMISSION_STATUS_TAG_COLOR[s.status]}>{SUBMISSION_STATUS_LABEL_STUDENT[s.status]}</Tag>
                                    <span className="text-sm font-semibold text-stone-900">
                                        {s.status === 'Graded' && s.totalScore != null ? `${s.totalScore} đ` : '—'}
                                    </span>
                                </div>
                            </div>
                        </List.Item>
                    )}
                />
            )}
        </Modal>
    )
}
