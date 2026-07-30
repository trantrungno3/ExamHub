import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import type {TableColumnsType} from 'antd'
import {Button, Input, Popconfirm, Select, Table, Tag} from 'antd'
import {CheckOutlined, PlusOutlined, SearchOutlined, UploadOutlined} from '@ant-design/icons'
import {useDeleteQuestionMutation, useQuestionsQuery, useVerifyQuestionMutation} from '../../hooks/queries/useQuestions'
import {
    useCognitiveLevelsQuery,
    useDifficultyLevelsQuery,
    useQuestionTypesQuery,
    useTopicsQuery,
} from '../../hooks/queries/useCategoryLists'
import {BulkImportModal} from './BulkImportModal'

const DIFF_COLOR: Record<string, string> = {easy: 'green', medium: 'gold', hard: 'orange', very_hard: 'red'}

export default function QuestionBankPage() {
    const navigate = useNavigate()

    const [page, setPage] = useState(1)
    const [pageSize, setPageSize] = useState(20)
    const [keyword, setKeyword] = useState('')
    const [topicId, setTopicId] = useState<number>()
    const [questionTypeId, setQuestionTypeId] = useState<number>()
    const [difficultyLevelId, setDifficultyLevelId] = useState<number>()
    const [cognitiveLevelId, setCognitiveLevelId] = useState<number>()
    const [isVerified, setIsVerified] = useState<boolean>()
    const [importOpen, setImportOpen] = useState(false)

    const query: QuestionPagedQuery = useMemo(
        () => ({page, pageSize, keyword, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, isVerified}),
        [page, pageSize, keyword, topicId, questionTypeId, difficultyLevelId, cognitiveLevelId, isVerified],
    )

    const {data, isLoading} = useQuestionsQuery(query)
    const topics = useTopicsQuery()
    const questionTypes = useQuestionTypesQuery()
    const difficulties = useDifficultyLevelsQuery()
    const cognitives = useCognitiveLevelsQuery()

    const deleteMutation = useDeleteQuestionMutation()
    const verifyMutation = useVerifyQuestionMutation()

    const columns: TableColumnsType<Question> = [
        {
            title: 'Nội dung câu hỏi', dataIndex: 'content', key: 'content', ellipsis: true,
            render: (_, q) => <span
                className="font-medium text-gray-800">{q.contentPlain || stripHtml(q.content)}</span>,
        },
        {
            title: 'Chủ đề', dataIndex: 'topicName', key: 'topicName', width: 140,
            render: v => <span className="text-gray-600">{v ?? '—'}</span>,
        },
        {
            title: 'Loại', dataIndex: 'questionTypeName', key: 'questionTypeName', width: 130,
            render: v => v ? <Tag color="blue">{v}</Tag> : '—',
        },
        {
            title: 'Độ khó', dataIndex: 'difficultyLevelName', key: 'difficultyLevelName', width: 110,
            render: (_, q) => q.difficultyLevelName
                ? <Tag
                    color={DIFF_COLOR[difficultyCode(difficulties.data, q.difficultyLevelId)] ?? 'default'}>{q.difficultyLevelName}</Tag>
                : '—',
        },
        {
            title: 'Bloom', dataIndex: 'cognitiveLevelName', key: 'cognitiveLevelName', width: 120,
            render: v => v ? <Tag color="purple">{v}</Tag> : <span className="text-gray-300">—</span>,
        },
        {
            title: 'Trạng thái', dataIndex: 'isVerified', key: 'isVerified', width: 110,
            render: v => <Tag color={v ? 'green' : 'gold'}>{v ? 'Đã duyệt' : 'Chờ duyệt'}</Tag>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 180, fixed: 'right',
            render: (_, q) => (
                <div className="flex gap-2 items-center">
                    <button className="btn-edit" onClick={() => navigate(`/app/questions/${q.id}/edit`)}>Sửa</button>
                    {!q.isVerified && (
                        <button
                            className="text-green-600 text-sm hover:underline flex items-center gap-1"
                            onClick={() => verifyMutation.mutate(q.id)}
                        >
                            <CheckOutlined/> Duyệt
                        </button>
                    )}
                    <Popconfirm
                        title="Xóa câu hỏi này?"
                        okText="Xóa" cancelText="Hủy"
                        okButtonProps={{danger: true}}
                        onConfirm={() => deleteMutation.mutate(q.id)}
                    >
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Ngân hàng câu hỏi</p>
                    <p className="top-bar-subtitle">Quản lý, tìm kiếm và phân loại toàn bộ câu hỏi</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                {/* Filters */}
                <div className="flex items-center gap-2 flex-wrap">
                    <Input
                        prefix={<SearchOutlined className="text-gray-400"/>}
                        placeholder="Tìm câu hỏi..."
                        style={{width: 220}}
                        allowClear
                        value={keyword}
                        onChange={e => {
                            setKeyword(e.target.value)
                            setPage(1)
                        }}
                    />
                    <Select
                        placeholder="Chủ đề" allowClear showSearch optionFilterProp="label" style={{width: 160}}
                        value={topicId} onChange={v => {
                        setTopicId(v)
                        setPage(1)
                    }}
                        options={(topics.data ?? []).map(t => ({value: t.id, label: t.name}))}
                    />
                    <Select
                        placeholder="Loại" allowClear style={{width: 150}}
                        value={questionTypeId} onChange={v => {
                        setQuestionTypeId(v)
                        setPage(1)
                    }}
                        options={(questionTypes.data ?? []).map(t => ({value: t.id, label: t.name}))}
                    />
                    <Select
                        placeholder="Độ khó" allowClear style={{width: 130}}
                        value={difficultyLevelId} onChange={v => {
                        setDifficultyLevelId(v)
                        setPage(1)
                    }}
                        options={(difficulties.data ?? []).map(d => ({value: d.id, label: d.name}))}
                    />
                    <Select
                        placeholder="Bloom" allowClear style={{width: 140}}
                        value={cognitiveLevelId} onChange={v => {
                        setCognitiveLevelId(v)
                        setPage(1)
                    }}
                        options={(cognitives.data ?? []).map(c => ({value: c.id, label: c.name}))}
                    />
                    <Select
                        placeholder="Trạng thái" allowClear style={{width: 130}}
                        value={isVerified} onChange={v => {
                        setIsVerified(v)
                        setPage(1)
                    }}
                        options={[{value: true, label: 'Đã duyệt'}, {value: false, label: 'Chờ duyệt'}]}
                    />
                    <div className="flex gap-2 ml-auto">
                        <Button icon={<UploadOutlined/>} onClick={() => setImportOpen(true)}>Nhập Excel</Button>
                        <Button type="primary" icon={<PlusOutlined/>} onClick={() => navigate('/app/questions/add')}>
                            Thêm câu hỏi
                        </Button>
                    </div>
                </div>

                <div className="section-card shrink-0">
                    <Table
                        columns={columns}
                        dataSource={data?.items ?? []}
                        rowKey="id"
                        loading={isLoading}
                        scroll={{x: 700}}
                        pagination={{
                            current: page,
                            pageSize,
                            total: data?.total ?? 0,
                            showSizeChanger: true,
                            showTotal: total => `Tổng số ${total} câu hỏi`,
                            onChange: (p, ps) => {
                                setPage(p)
                                setPageSize(ps)
                            },
                        }}
                    />
                </div>
            </div>

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

function difficultyCode(list: DifficultyLevel[] | undefined, id: number): string {
    return list?.find(d => d.id === id)?.code ?? ''
}
