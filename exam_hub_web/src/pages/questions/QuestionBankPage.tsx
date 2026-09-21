import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import type {TableColumnsType} from 'antd'
import {Badge, Button, Form, Input, Modal, Popconfirm, Select, Table, Tooltip, message} from 'antd'
import {
    CheckCircleFilled,
    CheckOutlined,
    ClockCircleFilled,
    CloseCircleFilled,
    DatabaseOutlined,
    FilterOutlined,
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
    useGradeLevelsListQuery,
    useQuestionTypesQuery,
    useSubjectsQuery,
    useTopicsQuery,
} from '../../hooks/queries/useCategoryLists'
import {questionService} from '../../services/questionService'
import {statusCode} from '../../services/requestService'
import {StatusTag} from '../../components/StatusTag'
import {BulkImportModal} from './BulkImportModal'
import {BLOOM_CHIP, BLOOM_NUM, DEFAULT_PAGE, DEFAULT_PAGE_SIZE, DIFF_CHIP, NEUTRAL_CHIP, TYPE_CHIP, type ChipColor} from '../../constants'
import PageHeader from '../../components/PageHeader'
import {stripHtml} from '../../utils/snapshot'
import {toOptions} from '../../utils/options'
import {useDebounced} from '../../hooks/useDebounced'
import {StatCard} from '../../components/StatCard'
import {BRAND} from '../../constants/theme'

interface FilterFormValues {
    gradeLevelId?: number
    subjectId?: number
    topicId?: number
    difficultyLevelId?: number
    questionTypeId?: number
    cognitiveLevelId?: number
    reviewStatus?: string
}

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

export default function QuestionBankPage() {
    const navigate = useNavigate()
    const qc = useQueryClient()

    const [page, setPage] = useState(DEFAULT_PAGE)
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
    const [keyword, setKeyword] = useState('')
    const [importOpen, setImportOpen] = useState(false)
    const [filterOpen, setFilterOpen] = useState(false)
    const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([])

    const [filterForm] = Form.useForm<FilterFormValues>()
    // Giá trị đang chỉnh trong modal — chỉ dùng để tính option lồng nhau (lớp/môn/chủ đề), chưa áp dụng vào query.
    const draft = Form.useWatch([], filterForm) ?? {}
    const [appliedFilters, setAppliedFilters] = useState<FilterFormValues>({})
    const {topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, reviewStatus, subjectId, gradeLevelId} = appliedFilters

    const activeFilterCount = Object.values(appliedFilters).filter(v => v !== undefined).length
    const openFilters = () => { filterForm.setFieldsValue(appliedFilters); setFilterOpen(true) }
    const applyFilters = () => { setAppliedFilters(filterForm.getFieldsValue()); setPage(1); setFilterOpen(false) }
    const resetFilters = () => filterForm.resetFields()

    const debouncedKeyword = useDebounced(keyword)
    const query: QuestionPagedQuery = useMemo(
        () => ({page, pageSize, keyword: debouncedKeyword, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, reviewStatus, subjectId, gradeLevelId}),
        [page, pageSize, debouncedKeyword, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, reviewStatus, subjectId, gradeLevelId],
    )

    const {data, isLoading} = useQuestionsQuery(query)
    const stats = useQuestionStatsQuery()
    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()
    const topics = useTopicsQuery()
    const questionTypes = useQuestionTypesQuery()
    const difficulties = useDifficultyLevelsQuery()
    const cognitives = useCognitiveLevelsQuery()

    const subjectOptions = useMemo(
        () => (subjects.data ?? [])
            .filter(s => s.gradeLevelId === draft.gradeLevelId)
            .map(s => ({value: s.id, label: s.name})),
        [subjects.data, draft.gradeLevelId],
    )
    const subjectIdsInGrade = useMemo(
        () => new Set((subjects.data ?? []).filter(s => s.gradeLevelId === draft.gradeLevelId).map(s => s.id)),
        [subjects.data, draft.gradeLevelId],
    )
    const topicOptions = useMemo(
        () => (topics.data ?? [])
            .filter(t => draft.subjectId ? t.subjectId === draft.subjectId : !draft.gradeLevelId || subjectIdsInGrade.has(t.subjectId))
            .map(t => ({value: t.id, label: t.name})),
        [topics.data, draft.subjectId, draft.gradeLevelId, subjectIdsInGrade],
    )

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
    const bloomLegend = useMemo(
        () => (cognitives.data ?? []).toSorted((a, b) => a.levelOrder - b.levelOrder),
        [cognitives.data])

    const invalidate = () => {
        void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
        void qc.invalidateQueries({queryKey: QUESTION_KEYS.stats})
    }
    const bulkVerify = async () => {
        const results = await Promise.allSettled(selectedRowKeys.map(id => questionService.verify(id)))
        const succeeded = results.filter(
            r => r.status === 'fulfilled' && r.value.status !== statusCode.Error).length
        const failed = results.length - succeeded
        if (failed === 0) {
            message.success(`Đã duyệt ${succeeded} câu hỏi`)
        } else if (succeeded === 0) {
            message.error(`Không thể duyệt ${failed} câu hỏi`)
        } else {
            message.warning(`Đã duyệt ${succeeded} câu hỏi, ${failed} câu hỏi thất bại`)
        }
        setSelectedRowKeys([]); invalidate()
    }
    const bulkDelete = async () => {
        const results = await Promise.all(selectedRowKeys.map(id => questionService.remove(id)))
        const succeeded = results.filter(r => r.status === statusCode.Deleted).length
        const failed = results.length - succeeded
        if (failed === 0) {
            message.success(`Đã xoá ${succeeded} câu hỏi`)
        } else if (succeeded === 0) {
            message.error(`Không thể xoá ${failed} câu hỏi (đang được sử dụng hoặc lỗi khác)`)
        } else {
            message.warning(`Đã xoá ${succeeded} câu hỏi, ${failed} câu hỏi không thể xoá (đang được sử dụng hoặc lỗi khác)`)
        }
        setSelectedRowKeys([]); invalidate()
    }

    const columns: TableColumnsType<Question> = [
        {
            title: 'Nội dung câu hỏi', dataIndex: 'content', key: 'content',
            render: (_, q) => (
                <div className="min-w-0">
                    <div className="font-medium line-clamp-1" style={{color: BRAND.inkStrong}}>
                        {q.contentPlain || stripHtml(q.content)}
                    </div>
                    {q.topicName && <div className="text-[12px]" style={{color: BRAND.mutedSoft}}>{q.topicName}</div>}
                </div>
            ),
        },
        {
            title: 'Chủ đề', dataIndex: 'topicName', key: 'topicName', width: 140,
            render: v => <span style={{color: BRAND.muted}}>{v ?? '—'}</span>,
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
                if (!q.cognitiveLevelId || !q.cognitiveLevelName) return <span style={{color: BRAND.borderStrong}}>—</span>
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
                            <button className="text-[13px] hover:underline" style={{color: BRAND.warning}}
                                    onClick={() => unverifyMutation.mutate(q.id)}>Bỏ duyệt</button>
                        ) : (
                            <button className="text-[13px] hover:underline flex items-center gap-1" style={{color: BRAND.success}}
                                    onClick={() => verifyMutation.mutate(q.id)}><CheckOutlined/> Duyệt</button>
                        )}
                        {st === 'pending' && (
                            <button className="text-[13px] hover:underline" style={{color: BRAND.danger}}
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
            <PageHeader title="Ngân hàng câu hỏi"
                        subtitle="Quản lý toàn bộ câu hỏi theo môn học · chủ đề · độ khó · cấp độ Bloom"/>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                {/* Stat cards */}
                <div className="flex gap-4 flex-wrap">
                    <StatCard label="Tổng câu hỏi" value={stats.data?.total} icon={<DatabaseOutlined/>} color={BRAND.primary} bg="#eef1ff"/>
                    <StatCard label="Đã duyệt" value={stats.data?.verified} icon={<CheckCircleFilled/>} color={BRAND.success} bg="#e7f7ef"/>
                    <StatCard label="Chờ duyệt" value={stats.data?.pending} icon={<ClockCircleFilled/>} color={BRAND.warning} bg={BRAND.warningSoft}/>
                    <StatCard label="Bị từ chối" value={stats.data?.rejected} icon={<CloseCircleFilled/>} color={BRAND.danger} bg={BRAND.dangerSoft}/>
                    <StatCard label="Không HĐ" value={stats.data?.inactive} icon={<StopOutlined/>} color={BRAND.muted} bg="#eef0f3"/>
                </div>

                {/* Filters */}
                <div className="flex items-center gap-2 flex-wrap">
                    <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm nội dung câu hỏi..."
                           style={{width: 220}} allowClear value={keyword}
                           onChange={e => { setKeyword(e.target.value); setPage(1) }}/>
                    <Badge count={activeFilterCount} size="small">
                        <Button icon={<FilterOutlined/>} onClick={openFilters}>Bộ lọc</Button>
                    </Badge>
                    <div className="flex gap-2 ml-auto">
                        <Button icon={<UploadOutlined/>} onClick={() => setImportOpen(true)}>Nhập Excel</Button>
                        <Button type="primary" icon={<PlusOutlined/>} onClick={() => navigate('/app/questions/add')}>
                            Thêm câu hỏi
                        </Button>
                    </div>
                </div>

                {/* Bloom legend */}
                <div className="flex items-center gap-2 flex-wrap text-[12px]">
                    <span style={{color: BRAND.muted}}>Bloom:</span>
                    {bloomLegend.map(c => (
                        <Chip key={c.id} label={`${c.levelOrder}.${c.name}`} color={BLOOM_CHIP[c.code] ?? NEUTRAL_CHIP}/>
                    ))}
                </div>

                {/* Bulk action bar */}
                {selectedRowKeys.length > 0 && (
                    <div className="flex items-center gap-3 px-3 py-2 rounded-lg"
                         style={{background: BRAND.primaryTint, border: '1px solid #d6e0fb'}}>
                        <span className="text-[13px] font-medium" style={{color: BRAND.primary}}>
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
                title="Bộ lọc câu hỏi"
                open={filterOpen}
                onCancel={() => setFilterOpen(false)}
                footer={[
                    <Button key="reset" onClick={resetFilters}>Xóa lọc</Button>,
                    <Button key="apply" type="primary" onClick={applyFilters}>Xong</Button>,
                ]}
            >
                <Form
                    form={filterForm}
                    layout="vertical"
                    onValuesChange={changed => {
                        // Đổi lớp -> bỏ môn/chủ đề đã chọn nếu không còn thuộc lớp mới.
                        if ('gradeLevelId' in changed)
                            filterForm.setFieldsValue({subjectId: undefined, topicId: undefined})
                        else if ('subjectId' in changed)
                            filterForm.setFieldValue('topicId', undefined)
                    }}
                >
                    <div className="grid grid-cols-2 gap-3">
                        <Form.Item name="gradeLevelId" label="Lớp">
                            <Select placeholder="Lớp" allowClear
                                    options={toOptions(grades.data)}/>
                        </Form.Item>
                        <Form.Item name="subjectId" label="Môn học">
                            <Select placeholder="Môn học" allowClear showSearch optionFilterProp="label"
                                    disabled={!draft.gradeLevelId} options={subjectOptions}/>
                        </Form.Item>
                        <Form.Item name="topicId" label="Chủ đề">
                            <Select placeholder="Chủ đề" allowClear showSearch optionFilterProp="label" options={topicOptions}/>
                        </Form.Item>
                        <Form.Item name="difficultyLevelId" label="Độ khó">
                            <Select placeholder="Độ khó" allowClear
                                    options={toOptions(difficulties.data)}/>
                        </Form.Item>
                        <Form.Item name="questionTypeId" label="Loại câu hỏi">
                            <Select placeholder="Loại câu hỏi" allowClear
                                    options={toOptions(questionTypes.data)}/>
                        </Form.Item>
                        <Form.Item name="cognitiveLevelId" label="Bloom">
                            <Select placeholder="Bloom" allowClear
                                    options={toOptions(cognitives.data)}/>
                        </Form.Item>
                        <Form.Item name="reviewStatus" label="Trạng thái">
                            <Select placeholder="Trạng thái" allowClear
                                    options={[
                                        {value: 'approved', label: 'Đã duyệt'},
                                        {value: 'pending', label: 'Chờ duyệt'},
                                        {value: 'rejected', label: 'Bị từ chối'},
                                    ]}/>
                        </Form.Item>
                    </div>
                </Form>
            </Modal>

            <Modal
                title="Từ chối câu hỏi"
                open={!!rejectTarget}
                onCancel={() => { setRejectTarget(undefined); setRejectReason('') }}
                onOk={handleReject}
                okText="Từ chối"
                cancelText="Huỷ"
                okButtonProps={{danger: true, disabled: !rejectReason.trim(), loading: rejectMutation.isPending}}
            >
                <p className="text-[13px] mb-2" style={{color: BRAND.muted}}>
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
