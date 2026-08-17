import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import type {TableColumnsType} from 'antd'
import {Button, Input, Modal, Popconfirm, Select, Table, Tooltip, message} from 'antd'
import {
    CheckCircleFilled,
    CheckOutlined,
    ClockCircleFilled,
    CloseCircleFilled,
    DatabaseOutlined,
    PlusOutlined,
    SearchOutlined,
    StopOutlined,
    UploadOutlined,
} from '@ant-design/icons'
import {useQueryClient} from '@tanstack/react-query'
import {
    QUESTION_KEYS,
    useDeleteQuestionMutation,
    useQuestionsQuery,
    useQuestionStatsQuery,
    useRejectQuestionMutation,
    useUnverifyQuestionMutation,
    useVerifyQuestionMutation,
} from '../../hooks/queries/useQuestions'
import {
    useCognitiveLevelsQuery,
    useDifficultyLevelsQuery,
    useQuestionTypesQuery,
    useTopicsQuery,
} from '../../hooks/queries/useCategoryLists'
import {questionService} from '../../services/questionService'
import {StatusTag} from '../../components/StatusTag'
import {BulkImportModal} from './BulkImportModal'
import {BLOOM_CHIP, BLOOM_NUM, DEFAULT_PAGE, DEFAULT_PAGE_SIZE, DIFF_CHIP, NEUTRAL_CHIP, TYPE_CHIP, type ChipColor} from '../../constants'

type ReviewState = 'approved' | 'rejected' | 'pending'
const reviewState = (q: Question): ReviewState => (q.status as ReviewState) ?? 'pending'

function Chip({label, color}: {label: string; color: ChipColor}) {
    return (
        <span style={{background: color.bg, color: color.fg}}
              className="inline-flex items-center rounded-full px-2.5 py-0.5 text-[12px] font-medium whitespace-nowrap">
            {label}
        </span>
    )
}

function StatCard({label, value, icon, color, bg}: {
    label: string; value?: number; icon: React.ReactNode; color: string; bg: string
}) {
    return (
        <div className="flex-1 bg-white rounded-xl border p-4 flex items-center gap-3" style={{borderColor: '#eceef2'}}>
            <div className="w-10 h-10 rounded-lg flex items-center justify-center text-[18px]"
                 style={{background: bg, color}}>
                {icon}
            </div>
            <div>
                <div className="text-[22px] font-bold leading-tight" style={{color: '#191d27'}}>
                    {value != null ? value.toLocaleString('vi-VN') : '—'}
                </div>
                <div className="text-[12px]" style={{color: '#6f7788'}}>{label}</div>
            </div>
        </div>
    )
}

export default function QuestionBankPage() {
    const navigate = useNavigate()
    const qc = useQueryClient()

    const [page, setPage] = useState(DEFAULT_PAGE)
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
    const [keyword, setKeyword] = useState('')
    const [topicId, setTopicId] = useState<number>()
    const [questionTypeId, setQuestionTypeId] = useState<number>()
    const [difficultyLevelId, setDifficultyLevelId] = useState<number>()
    const [cognitiveLevelId, setCognitiveLevelId] = useState<number>()
    const [reviewStatus, setReviewStatus] = useState<string>()
    const [importOpen, setImportOpen] = useState(false)
    const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([])

    const query: QuestionPagedQuery = useMemo(
        () => ({page, pageSize, keyword, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, reviewStatus}),
        [page, pageSize, keyword, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, reviewStatus],
    )

    const {data, isLoading} = useQuestionsQuery(query)
    const stats = useQuestionStatsQuery()
    const topics = useTopicsQuery()
    const questionTypes = useQuestionTypesQuery()
    const difficulties = useDifficultyLevelsQuery()
    const cognitives = useCognitiveLevelsQuery()

    const deleteMutation = useDeleteQuestionMutation()
    const verifyMutation = useVerifyQuestionMutation()
    const unverifyMutation = useUnverifyQuestionMutation()
    const rejectMutation = useRejectQuestionMutation()
    const [rejectTarget, setRejectTarget] = useState<Question>()
    const [rejectReason, setRejectReason] = useState('')

    const handleReject = () => {
        if (!rejectTarget || !rejectReason.trim()) return
        rejectMutation.mutate({id: rejectTarget.id, reason: rejectReason.trim()}, {
            onSuccess: () => { setRejectTarget(undefined); setRejectReason('') },
        })
    }

    const diffCodeById = useMemo(
        () => Object.fromEntries((difficulties.data ?? []).map(d => [d.id, d.code])),
        [difficulties.data])
    const typeCodeById = useMemo(
        () => Object.fromEntries((questionTypes.data ?? []).map(t => [t.id, t.code])),
        [questionTypes.data])
    const cogCodeById = useMemo(
        () => Object.fromEntries((cognitives.data ?? []).map(c => [c.id, c.code])),
        [cognitives.data])

    const invalidate = () => {
        void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
        void qc.invalidateQueries({queryKey: QUESTION_KEYS.stats})
    }
    const bulkVerify = async () => {
        await Promise.all(selectedRowKeys.map(id => questionService.verify(id)))
        message.success(`Đã duyệt ${selectedRowKeys.length} câu hỏi`)
        setSelectedRowKeys([]); invalidate()
    }
    const bulkDelete = async () => {
        await Promise.all(selectedRowKeys.map(id => questionService.remove(id)))
        message.success(`Đã xoá ${selectedRowKeys.length} câu hỏi`)
        setSelectedRowKeys([]); invalidate()
    }

    const columns: TableColumnsType<Question> = [
        {
            title: 'Nội dung câu hỏi', dataIndex: 'content', key: 'content',
            render: (_, q) => (
                <div className="min-w-0">
                    <div className="font-medium line-clamp-1" style={{color: '#1d2129'}}>
                        {q.contentPlain || stripHtml(q.content)}
                    </div>
                    {q.topicName && <div className="text-[12px]" style={{color: '#9aa2b1'}}>{q.topicName}</div>}
                </div>
            ),
        },
        {
            title: 'Chủ đề', dataIndex: 'topicName', key: 'topicName', width: 140,
            render: v => <span style={{color: '#6f7788'}}>{v ?? '—'}</span>,
        },
        {
            title: 'Loại', dataIndex: 'questionTypeName', key: 'questionTypeName', width: 150,
            render: (_, q) => q.questionTypeName
                ? <Chip label={q.questionTypeName} color={TYPE_CHIP[typeCodeById[q.questionTypeId]] ?? NEUTRAL_CHIP}/>
                : '—',
        },
        {
            title: 'Độ khó', dataIndex: 'difficultyLevelName', key: 'difficultyLevelName', width: 110,
            render: (_, q) => q.difficultyLevelName
                ? <Chip label={q.difficultyLevelName} color={DIFF_CHIP[diffCodeById[q.difficultyLevelId]] ?? NEUTRAL_CHIP}/>
                : '—',
        },
        {
            title: 'Bloom', dataIndex: 'cognitiveLevelName', key: 'cognitiveLevelName', width: 130,
            render: (_, q) => {
                if (!q.cognitiveLevelId || !q.cognitiveLevelName) return <span style={{color: '#c4cad3'}}>—</span>
                const code = cogCodeById[q.cognitiveLevelId]
                const b = BLOOM_CHIP[code]
                const num = BLOOM_NUM[code]
                return <Chip label={`${num ?? ''}${num ? '.' : ''}${q.cognitiveLevelName}`} color={b ?? NEUTRAL_CHIP}/>
            },
        },
        {
            title: 'Duyệt', dataIndex: 'status', key: 'status', width: 120,
            render: (_, q) => {
                const st = reviewState(q)
                if (st === 'approved') return <StatusTag status="success" label="Đã duyệt"/>
                if (st === 'pending') return <StatusTag status="warning" label="Chờ duyệt"/>
                return (
                    <Tooltip title={q.rejectionReason}>
                        <span><StatusTag status="danger" label="Bị từ chối"/></span>
                    </Tooltip>
                )
            },
        },
        {
            title: 'Thao tác', key: 'actions', width: 230, fixed: 'right',
            render: (_, q) => {
                const st = reviewState(q)
                return (
                    <div className="flex gap-2 items-center">
                        <button className="btn-edit" onClick={() => navigate(`/app/questions/${q.id}/edit`)}>Sửa</button>
                        {st === 'approved' ? (
                            <button className="text-[13px] hover:underline" style={{color: '#d98a00'}}
                                    onClick={() => unverifyMutation.mutate(q.id)}>Bỏ duyệt</button>
                        ) : (
                            <button className="text-[13px] hover:underline flex items-center gap-1" style={{color: '#1ea375'}}
                                    onClick={() => verifyMutation.mutate(q.id)}><CheckOutlined/> Duyệt</button>
                        )}
                        {st === 'pending' && (
                            <button className="text-[13px] hover:underline" style={{color: '#e74242'}}
                                    onClick={() => { setRejectTarget(q); setRejectReason('') }}>Từ chối</button>
                        )}
                        <Popconfirm title="Xóa câu hỏi này?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                                    onConfirm={() => deleteMutation.mutate(q.id)}>
                            <button className="btn-delete">Xóa</button>
                        </Popconfirm>
                    </div>
                )
            },
        },
    ]

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Ngân hàng câu hỏi</p>
                    <p className="top-bar-subtitle">Quản lý toàn bộ câu hỏi theo môn học · chủ đề · độ khó · cấp độ Bloom</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                {/* Stat cards */}
                <div className="flex gap-4 flex-wrap">
                    <StatCard label="Tổng câu hỏi" value={stats.data?.total} icon={<DatabaseOutlined/>} color="#3a74f5" bg="#eef1ff"/>
                    <StatCard label="Đã duyệt" value={stats.data?.verified} icon={<CheckCircleFilled/>} color="#1ea375" bg="#e7f7ef"/>
                    <StatCard label="Chờ duyệt" value={stats.data?.pending} icon={<ClockCircleFilled/>} color="#d98a00" bg="#fff4e5"/>
                    <StatCard label="Bị từ chối" value={stats.data?.rejected} icon={<CloseCircleFilled/>} color="#e74242" bg="#fee5e5"/>
                    <StatCard label="Không HĐ" value={stats.data?.inactive} icon={<StopOutlined/>} color="#6f7788" bg="#eef0f3"/>
                </div>

                {/* Filters */}
                <div className="flex items-center gap-2 flex-wrap">
                    <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm nội dung câu hỏi..."
                           style={{width: 220}} allowClear value={keyword}
                           onChange={e => { setKeyword(e.target.value); setPage(1) }}/>
                    <Select placeholder="Chủ đề" allowClear showSearch optionFilterProp="label" style={{width: 160}}
                            value={topicId} onChange={v => { setTopicId(v); setPage(1) }}
                            options={(topics.data ?? []).map(t => ({value: t.id, label: t.name}))}/>
                    <Select placeholder="Độ khó" allowClear style={{width: 130}}
                            value={difficultyLevelId} onChange={v => { setDifficultyLevelId(v); setPage(1) }}
                            options={(difficulties.data ?? []).map(d => ({value: d.id, label: d.name}))}/>
                    <Select placeholder="Loại câu hỏi" allowClear style={{width: 160}}
                            value={questionTypeId} onChange={v => { setQuestionTypeId(v); setPage(1) }}
                            options={(questionTypes.data ?? []).map(t => ({value: t.id, label: t.name}))}/>
                    <Select placeholder="Bloom" allowClear style={{width: 140}}
                            value={cognitiveLevelId} onChange={v => { setCognitiveLevelId(v); setPage(1) }}
                            options={(cognitives.data ?? []).map(c => ({value: c.id, label: c.name}))}/>
                    <Select placeholder="Trạng thái" allowClear style={{width: 140}}
                            value={reviewStatus} onChange={v => { setReviewStatus(v); setPage(1) }}
                            options={[
                                {value: 'approved', label: 'Đã duyệt'},
                                {value: 'pending', label: 'Chờ duyệt'},
                                {value: 'rejected', label: 'Bị từ chối'},
                            ]}/>
                    <div className="flex gap-2 ml-auto">
                        <Button icon={<UploadOutlined/>} onClick={() => setImportOpen(true)}>Nhập Excel</Button>
                        <Button type="primary" icon={<PlusOutlined/>} onClick={() => navigate('/app/questions/add')}>
                            Thêm câu hỏi
                        </Button>
                    </div>
                </div>

                {/* Bloom legend */}
                <div className="flex items-center gap-2 flex-wrap text-[12px]">
                    <span style={{color: '#6f7788'}}>Bloom:</span>
                    {[...(cognitives.data ?? [])].sort((a, b) => a.levelOrder - b.levelOrder).map(c => (
                        <Chip key={c.id} label={`${c.levelOrder}.${c.name}`} color={BLOOM_CHIP[c.code] ?? NEUTRAL_CHIP}/>
                    ))}
                </div>

                {/* Bulk action bar */}
                {selectedRowKeys.length > 0 && (
                    <div className="flex items-center gap-3 px-3 py-2 rounded-lg"
                         style={{background: '#eef1ff', border: '1px solid #d6e0fb'}}>
                        <span className="text-[13px] font-medium" style={{color: '#3a74f5'}}>
                            Đã chọn {selectedRowKeys.length}
                        </span>
                        <Button size="small" type="primary" onClick={bulkVerify}>Duyệt hàng loạt</Button>
                        <Popconfirm title={`Xoá ${selectedRowKeys.length} câu hỏi?`} okText="Xoá" cancelText="Huỷ"
                                    okButtonProps={{danger: true}} onConfirm={bulkDelete}>
                            <Button size="small" danger>Xoá hàng loạt</Button>
                        </Popconfirm>
                        <Button size="small" type="text" onClick={() => setSelectedRowKeys([])}>Bỏ chọn</Button>
                    </div>
                )}

                <div className="section-card shrink-0">
                    <Table
                        columns={columns}
                        dataSource={data?.items ?? []}
                        rowKey="id"
                        loading={isLoading}
                        scroll={{x: 900}}
                        rowSelection={{selectedRowKeys, onChange: keys => setSelectedRowKeys(keys as string[])}}
                        pagination={{
                            current: page,
                            pageSize,
                            total: data?.total ?? 0,
                            showSizeChanger: true,
                            showTotal: total => `Hiển thị ${data?.items?.length ?? 0} trong tổng số ${total} câu hỏi`,
                            onChange: (p, ps) => { setPage(p); setPageSize(ps) },
                        }}
                    />
                </div>
            </div>

            <Modal
                title="Từ chối câu hỏi"
                open={!!rejectTarget}
                onCancel={() => { setRejectTarget(undefined); setRejectReason('') }}
                onOk={handleReject}
                okText="Từ chối"
                cancelText="Huỷ"
                okButtonProps={{danger: true, disabled: !rejectReason.trim(), loading: rejectMutation.isPending}}
            >
                <p className="text-[13px] mb-2" style={{color: '#6f7788'}}>
                    Nhập lý do từ chối câu hỏi. Câu hỏi sẽ chuyển sang trạng thái <b>Bị từ chối</b> và không dùng để sinh đề.
                </p>
                <Input.TextArea
                    rows={3}
                    autoFocus
                    placeholder="VD: Nội dung chưa rõ ràng, thiếu đáp án đúng..."
                    value={rejectReason}
                    onChange={e => setRejectReason(e.target.value)}
                />
            </Modal>

            <BulkImportModal
                open={importOpen}
                onClose={() => setImportOpen(false)}
                topics={topics.data ?? []}
                difficulties={difficulties.data ?? []}
                cognitives={cognitives.data ?? []}
            />
        </>
    )
}

function stripHtml(html: string): string {
    return html.replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim()
}
