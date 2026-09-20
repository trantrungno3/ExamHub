import {useNavigate} from 'react-router-dom'
import {Spin, Tag} from 'antd'
import {RightOutlined} from '@ant-design/icons'
import {useMySessionSubmissionsQuery} from '../../hooks/queries/useSubmissions'
import {formatTimestamp} from '../../utils/datetime'
import {SUBMISSION_STATUS_LABEL_STUDENT, SUBMISSION_STATUS_TAG_COLOR} from '../../constants'
import {ROUTES} from '../../routes/paths'
import {BRAND} from '../../constants/theme'

/**
 * Danh sách các lần thi của học sinh trong một kỳ thi, mở ngay trong card (thay modal cũ).
 * BE trả về đã sắp mới → cũ, nên "Lần n" đếm ngược theo vị trí.
 */
export function SessionAttempts({sessionId, studentId}: {sessionId: string; studentId?: string}) {
    const navigate = useNavigate()
    const {data: subs = [], isLoading} = useMySessionSubmissionsQuery(sessionId, studentId)

    if (isLoading) return <div className="flex justify-center py-4"><Spin size="small"/></div>
    if (subs.length === 0)
        return <p className="text-[13px] text-center py-3 m-0" style={{color: BRAND.mutedSoft}}>Chưa có lần nộp nào</p>

    const scores = subs.filter(s => s.status === 'Graded' && s.totalScore != null).map(s => s.totalScore!)
    const best = scores.length > 0 ? Math.max(...scores) : undefined

    return (
        <div className="mt-3 rounded-lg overflow-hidden" style={{background: '#fafafa'}}>
            {subs.map((s, i) => {
                const isBest = best != null && s.status === 'Graded' && s.totalScore === best
                return (
                    <button key={s.id} type="button"
                            className="w-full flex items-center gap-2.5 px-3 py-2.5 text-left bg-transparent border-0 border-b last:border-b-0 cursor-pointer hover:bg-white transition-colors"
                            style={{borderColor: BRAND.border}}
                            onClick={() => navigate(`${ROUTES.STUDENT_EXAM_RESULT}?submissionId=${s.id}`)}>
                        <span className="w-1.5 h-1.5 rounded-full shrink-0"
                              style={{background: isBest ? BRAND.success : '#d4d8e0'}}/>
                        <span className="text-[13px] font-medium shrink-0" style={{color: BRAND.ink}}>
                            Lần {subs.length - i}
                        </span>
                        <span className="text-[12.5px] truncate" style={{color: BRAND.mutedSoft}}>
                            {formatTimestamp(s.submittedAt ?? s.createdAt)}
                        </span>
                        <span className="ml-auto flex items-center gap-2 shrink-0">
                            <Tag color={SUBMISSION_STATUS_TAG_COLOR[s.status]} className="!mr-0">
                                {SUBMISSION_STATUS_LABEL_STUDENT[s.status]}
                            </Tag>
                            <span className="text-[13px] font-semibold tabular-nums" style={{color: BRAND.ink}}>
                                {s.status === 'Graded' && s.totalScore != null ? `${s.totalScore} đ` : '—'}
                            </span>
                            <RightOutlined style={{color: '#c3c8d2', fontSize: 11}}/>
                        </span>
                    </button>
                )
            })}
            {subs.length > 1 && best != null && (
                <div className="px-3 py-2 text-right text-[12.5px]" style={{color: BRAND.muted}}>
                    Điểm cao nhất: <span className="font-semibold" style={{color: BRAND.success}}>{best} đ</span>
                </div>
            )}
        </div>
    )
}
