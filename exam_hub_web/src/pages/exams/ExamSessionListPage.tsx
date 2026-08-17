import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Input, Popconfirm, Select, Table} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, SearchOutlined} from '@ant-design/icons'
import {
    useCloseSessionMutation,
    useDeleteExamSessionMutation,
    useExamSessionsQuery,
    usePublishSessionMutation,
} from '../../hooks/queries/useExamSessions'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {ROUTES} from '../../routes/paths'
import {StatusTag} from '../../components/StatusTag'
import {DEFAULT_PAGE, DEFAULT_PAGE_SIZE, PICK_MODE_LABEL, SESSION_STATUS_LABEL, SESSION_STATUS_VARIANT} from '../../constants'

function fmt(ms: number): string {
    return new Date(ms).toLocaleString('vi-VN', {day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'})
}

export default function ExamSessionListPage() {
    const navigate = useNavigate()
    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()

    const [page, setPage] = useState(DEFAULT_PAGE)
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE)
    const [gradeLevelId, setGradeLevelId] = useState<number>()
    const [subjectId, setSubjectId] = useState<number>()
    const [status, setStatus] = useState<ExamSessionStatus>()
    const [keyword, setKeyword] = useState('')

    const query: ExamSessionPagedQuery = useMemo(
        () => ({page, pageSize, gradeLevelId, subjectId, status, keyword}),
        [page, pageSize, gradeLevelId, subjectId, status, keyword],
    )
    const {data, isLoading} = useExamSessionsQuery(query)
    const publish = usePublishSessionMutation()
    const close = useCloseSessionMutation()
    const remove = useDeleteExamSessionMutation()

    const columns: TableColumnsType<ExamSession> = [
        {title: 'Tiêu đề', dataIndex: 'title', key: 'title', render: v => <span className="font-medium text-gray-800">{v}</span>},
        {title: 'Môn', dataIndex: 'subjectName', key: 'subjectName', width: 130, render: v => v ?? '—'},
        {title: 'Cấp lớp', dataIndex: 'gradeLevelName', key: 'gradeLevelName', width: 100, render: v => v ?? '—'},
        {
            title: 'Khung giờ', key: 'time', width: 260,
            render: (_, s) => <span className="text-sm text-gray-600">{fmt(s.openAt)} → {fmt(s.closeAt)}</span>,
        },
        {title: 'Cách chọn', dataIndex: 'pickMode', key: 'pickMode', width: 110, render: (v: ExamSessionPickMode) => PICK_MODE_LABEL[v]},
        {title: 'Số đề', dataIndex: 'examCount', key: 'examCount', width: 70, align: 'center'},
        {title: 'Lớp/khoá', dataIndex: 'assignmentCount', key: 'assignmentCount', width: 90, align: 'center'},
        {
            title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 130,
            render: (v: ExamSessionStatus) => <StatusTag status={SESSION_STATUS_VARIANT[v]} label={SESSION_STATUS_LABEL[v]}/>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 280, fixed: 'right',
            render: (_, s) => (
                <div className="flex gap-2 items-center flex-wrap">
                    <button className="text-blue-600 text-sm hover:underline"
                            onClick={() => navigate(`${ROUTES.EXAM_SESSIONS}/${s.id}/edit`)}>Sửa</button>
                    <button className="text-gray-600 text-sm hover:underline"
                            onClick={() => navigate(
                                ROUTES.EXAM_SESSION_SUBMISSIONS.replace(':id', s.id),
                                {state: {title: s.title, subjectName: s.subjectName, gradeLevelName: s.gradeLevelName}},
                            )}>Bài nộp</button>
                    {s.status === 'draft' && (
                        <button className="text-green-600 text-sm hover:underline"
                                onClick={() => publish.mutate(s.id)}>Phát hành</button>
                    )}
                    {s.status === 'published' && (
                        <Popconfirm title="Đóng kỳ thi này?" okText="Đóng" cancelText="Hủy"
                                    onConfirm={() => close.mutate(s.id)}>
                            <button className="text-orange-600 text-sm hover:underline">Đóng</button>
                        </Popconfirm>
                    )}
                    <Popconfirm title="Xoá kỳ thi này?" okText="Xoá" cancelText="Hủy"
                                okButtonProps={{danger: true}} onConfirm={() => remove.mutate(s.id)}>
                        <button className="btn-delete">Xoá</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Kỳ thi</p>
                    <p className="top-bar-subtitle">Cấu hình kỳ thi theo môn + cấp lớp, giao cho lớp/khoá</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
                <div className="flex items-center gap-2 flex-wrap">
                    <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm kỳ thi..."
                           style={{width: 220}} allowClear value={keyword}
                           onChange={e => {
                               setKeyword(e.target.value)
                               setPage(1)
                           }}/>
                    <Select placeholder="Cấp lớp" allowClear style={{width: 130}} value={gradeLevelId}
                            onChange={v => {
                                setGradeLevelId(v)
                                setPage(1)
                            }}
                            options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                    <Select placeholder="Môn" allowClear showSearch optionFilterProp="label" style={{width: 160}}
                            value={subjectId} onChange={v => {
                                setSubjectId(v)
                                setPage(1)
                            }}
                            options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                    <Select placeholder="Trạng thái" allowClear style={{width: 150}} value={status}
                            onChange={v => {
                                setStatus(v)
                                setPage(1)
                            }}
                            options={(['draft', 'published', 'closed'] as ExamSessionStatus[]).map(s => ({value: s, label: SESSION_STATUS_LABEL[s]}))}/>
                    <Button type="primary" icon={<PlusOutlined/>} className="ml-auto"
                            onClick={() => navigate(ROUTES.EXAM_SESSIONS_CREATE)}>Tạo kỳ thi</Button>
                </div>

                <div className="section-card shrink-0">
                    <Table columns={columns} dataSource={data?.items ?? []} rowKey="id" loading={isLoading}
                           scroll={{x: 1300}}
                           pagination={{
                               current: page, pageSize, total: data?.total ?? 0, showSizeChanger: true,
                               showTotal: total => `Tổng số ${total} kỳ thi`,
                               onChange: (p, ps) => {
                                   setPage(p)
                                   setPageSize(ps)
                               },
                           }}/>
                </div>
            </div>

        </>
    )
}
