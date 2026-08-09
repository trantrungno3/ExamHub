import {useEffect, useMemo, useState} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Button, Checkbox, DatePicker, Form, Input, InputNumber, Modal, Popconfirm, Select, Spin, Table, Tag, message} from 'antd'
import type {TableColumnsType} from 'antd'
import dayjs, {type Dayjs} from 'dayjs'
import {ArrowLeftOutlined, PlusOutlined} from '@ant-design/icons'
import {
    useAddAssignmentMutation,
    useCreateExamSessionMutation,
    useExamSessionQuery,
    usePublishSessionMutation,
    useRemoveAssignmentMutation,
    useRemoveSessionExamMutation,
    useSetSessionExamsMutation,
    useUpdateExamSessionMutation,
} from '../../hooks/queries/useExamSessions'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {useExamsQuery} from '../../hooks/queries/useExams'
import {useSchoolsQuery} from '../../hooks/queries/useSchools'
import {useCohortsQuery} from '../../hooks/queries/useCohorts'
import {useCohortClassesQuery} from '../../hooks/queries/useCohortClasses'
import {statusCode} from '../../services/requestService'
import {ROUTES} from '../../routes/paths'
import {AnalyticsDrawer} from './AnalyticsDrawer'

const PICK_MODE_OPTIONS = [
    {value: 'Random', label: 'Ngẫu nhiên (hệ thống bốc đề)'},
    {value: 'StudentChoice', label: 'Học sinh tự chọn đề'},
]

type ExamSessionFormValues = {
    title: string
    description?: string
    subjectId: number
    gradeLevelId: number
    openLocal: Dayjs
    closeLocal: Dayjs
    maxAttempts: number
    pickMode: ExamSessionPickMode
}

export default function ExamSessionEditPage() {
    const {id} = useParams<{id: string}>()
    const navigate = useNavigate()
    const isEdit = !!id

    const subjects = useSubjectsQuery()
    const grades = useGradeLevelsListQuery()
    const {data: detail, isLoading} = useExamSessionQuery(id)

    const create = useCreateExamSessionMutation()
    const update = useUpdateExamSessionMutation()
    const publish = usePublishSessionMutation()

    const [form] = Form.useForm<ExamSessionFormValues>()

    useEffect(() => {
        if (!detail) return
        form.setFieldsValue({
            title: detail.title,
            description: detail.description ?? '',
            subjectId: detail.subjectId,
            gradeLevelId: detail.gradeLevelId,
            openLocal: dayjs(detail.openAt),
            closeLocal: dayjs(detail.closeAt),
            maxAttempts: detail.maxAttempts,
            pickMode: detail.pickMode,
        })
    }, [detail, form])

    const handleSave = async () => {
        let v: ExamSessionFormValues
        try {
            v = await form.validateFields()
        } catch {
            return // AntD tự hiển thị lỗi trên từng field
        }
        const body: ExamSessionBody = {
            title: v.title.trim(),
            description: v.description?.trim() || undefined,
            subjectId: v.subjectId,
            gradeLevelId: v.gradeLevelId,
            openAt: v.openLocal.toISOString(),
            closeAt: v.closeLocal.toISOString(),
            maxAttempts: v.maxAttempts ?? 1,
            pickMode: v.pickMode,
        }
        if (new Date(body.closeAt) <= new Date(body.openAt)) {
            message.warning('Thời điểm đóng phải sau thời điểm mở.')
            return
        }
        if (isEdit) {
            await update.mutateAsync({id: id!, body})
        } else {
            const res = await create.mutateAsync(body)
            if (res.status !== statusCode.Error && res.data) {
                navigate(`${ROUTES.EXAM_SESSIONS}/${res.data}/edit`, {replace: true})
            }
        }
    }

    const isPublished = detail?.status === 'published'

    return (
        <>
            <div className="top-bar">
                <div className="flex items-center gap-3">
                    <button className="text-gray-500 hover:text-gray-800" onClick={() => navigate(ROUTES.EXAM_SESSIONS)}>
                        <ArrowLeftOutlined/>
                    </button>
                    <div>
                        <p className="top-bar-title">{isEdit ? 'Sửa kỳ thi' : 'Tạo kỳ thi'}</p>
                        <p className="top-bar-subtitle">Cấu hình, chọn đề và giao cho lớp/khoá</p>
                    </div>
                </div>
                {detail && (
                    <Tag color={isPublished ? 'green' : detail.status === 'closed' ? 'default' : 'gold'}>
                        {isPublished ? 'Đã phát hành' : detail.status === 'closed' ? 'Đã đóng' : 'Nháp'}
                    </Tag>
                )}
            </div>

            <div className="flex-1 overflow-auto p-6">
                <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 items-start">
                {isLoading && isEdit ? (
                    <Spin/>
                ) : (
                    <>
                        {/* ── Cấu hình ── */}
                        <div className="bg-white rounded-xl border border-[#eceef2] p-5">
                        <Form form={form} layout="vertical"
                            initialValues={{maxAttempts: 1, pickMode: 'Random'}}
                            className="session-info-form">
                            <h3 className="text-[15px] font-semibold text-[#191d27] mb-4">Thông tin kỳ thi</h3>
                            <Form.Item label="Tiêu đề" name="title" rules={[{required: true, message: 'Nhập tiêu đề kỳ thi'}]}>
                                <Input placeholder="VD: Kiểm tra giữa kỳ 1"/>
                            </Form.Item>
                            <Form.Item label="Mô tả" name="description">
                                <Input.TextArea rows={2}/>
                            </Form.Item>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
                                <Form.Item label="Môn" name="subjectId" rules={[{required: true, message: 'Chọn môn'}]}>
                                    <Select showSearch optionFilterProp="label" disabled={isPublished}
                                        options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                                </Form.Item>
                                <Form.Item label="Cấp lớp" name="gradeLevelId" rules={[{required: true, message: 'Chọn cấp lớp'}]}>
                                    <Select disabled={isPublished}
                                        options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                                </Form.Item>
                                <Form.Item label="Mở lúc" name="openLocal" rules={[{required: true, message: 'Chọn thời điểm mở'}]}>
                                    <DatePicker className="w-full" showTime format="DD/MM/YYYY HH:mm"
                                        placeholder="Chọn ngày giờ mở"/>
                                </Form.Item>
                                <Form.Item label="Đóng lúc" name="closeLocal" rules={[{required: true, message: 'Chọn thời điểm đóng'}]}>
                                    <DatePicker className="w-full" showTime format="DD/MM/YYYY HH:mm"
                                        placeholder="Chọn ngày giờ đóng"/>
                                </Form.Item>
                                <Form.Item label="Số lượt tối đa" name="maxAttempts">
                                    <InputNumber className="w-full" min={1} max={100}/>
                                </Form.Item>
                                <Form.Item label="Cách chọn đề" name="pickMode">
                                    <Select options={PICK_MODE_OPTIONS}/>
                                </Form.Item>
                            </div>
                            <div className="flex items-center gap-3 mt-2">
                                <Button type="primary" loading={create.isPending || update.isPending} onClick={handleSave}>
                                    {isEdit ? 'Lưu thay đổi' : 'Tạo & tiếp tục'}
                                </Button>
                                {isEdit && detail && (
                                    <Button className="border-[#1ea375] text-[#1ea375]"
                                            disabled={isPublished} loading={publish.isPending}
                                            onClick={() => publish.mutate(detail.id)}>
                                        {isPublished ? 'Đã xuất bản' : 'Xuất bản'}
                                    </Button>
                                )}
                            </div>
                        </Form>
                        </div>

                        {isEdit && detail && (
                            <div className="flex flex-col gap-4">
                                <PoolSection sessionId={detail.id} exams={detail.exams}
                                             subjectId={detail.subjectId} gradeLevelId={detail.gradeLevelId}/>
                                <AssignmentSection sessionId={detail.id} assignments={detail.assignments}/>
                            </div>
                        )}
                    </>
                )}
                </div>
            </div>
        </>
    )
}

// ── Pool đề ─────────────────────────────────────────────────────────────
function PoolSection({sessionId, exams, subjectId, gradeLevelId}: {
    sessionId: string; exams: SessionExam[]; subjectId: number; gradeLevelId: number
}) {
    const [modalOpen, setModalOpen] = useState(false)
    const [analyticsExamId, setAnalyticsExamId] = useState<string>()
    const removeExam = useRemoveSessionExamMutation()

    const columns: TableColumnsType<SessionExam> = [
        {title: 'Tiêu đề', dataIndex: 'title', key: 'title'},
        {title: 'Mã đề', dataIndex: 'examCode', key: 'examCode', width: 120, render: v => v ?? '—'},
        {title: 'Điểm', dataIndex: 'totalScore', key: 'totalScore', width: 80},
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, e) => (
                <div className="flex items-center gap-3">
                    <button className="text-[13px] hover:underline" style={{color: '#3a74f5'}}
                            onClick={() => setAnalyticsExamId(e.examId)}>Phân tích</button>
                    <Popconfirm title="Gỡ đề khỏi kỳ thi?" okText="Gỡ" cancelText="Hủy"
                                onConfirm={() => removeExam.mutate({id: sessionId, examId: e.examId})}>
                        <button className="btn-delete">Gỡ</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    return (
        <div className="bg-white rounded-xl border border-[#eceef2] p-5 flex flex-col gap-3">
            <div className="flex items-center justify-between">
                <h3 className="text-[15px] font-semibold text-[#191d27]">Đề trong kỳ thi ({exams.length})</h3>
                <Button type="primary" icon={<PlusOutlined/>} onClick={() => setModalOpen(true)}>Thêm đề</Button>
            </div>
            <Table columns={columns} dataSource={exams} rowKey="examId" size="small" pagination={false}
                   scroll={{x: 700}}
                   locale={{emptyText: 'Chưa có đề nào'}}/>
            <AddExamsModal open={modalOpen} onClose={() => setModalOpen(false)} sessionId={sessionId}
                           subjectId={subjectId} gradeLevelId={gradeLevelId}
                           existingIds={exams.map(e => e.examId)}/>
            <AnalyticsDrawer examId={analyticsExamId} onClose={() => setAnalyticsExamId(undefined)}/>
        </div>
    )
}

function AddExamsModal({open, onClose, sessionId, subjectId, gradeLevelId, existingIds}: {
    open: boolean; onClose: () => void; sessionId: string
    subjectId: number; gradeLevelId: number; existingIds: string[]
}) {
    const query: ExamPagedQuery = useMemo(
        () => ({page: 1, pageSize: 100, status: 'Published', subjectId, gradeLevelId}),
        [subjectId, gradeLevelId],
    )
    const {data, isLoading} = useExamsQuery(query)
    const setExams = useSetSessionExamsMutation()
    const [selected, setSelected] = useState<string[]>([])

    const available = (data?.items ?? []).filter(e => !existingIds.includes(e.id))

    const handleOk = async () => {
        if (selected.length === 0) {
            onClose()
            return
        }
        await setExams.mutateAsync({id: sessionId, examIds: selected})
        setSelected([])
        onClose()
    }

    return (
        <Modal title="Thêm đề vào kỳ thi" open={open} onCancel={onClose} onOk={handleOk}
               okText="Thêm" cancelText="Hủy" confirmLoading={setExams.isPending}>
            {isLoading ? <Spin/> : available.length === 0 ? (
                <p className="text-gray-500">Không có đề đã phát hành cùng môn/cấp lớp.</p>
            ) : (
                <Checkbox.Group className="flex flex-col gap-2" value={selected}
                                onChange={v => setSelected(v as string[])}>
                    {available.map(e => (
                        <Checkbox key={e.id} value={e.id}>
                            {e.title}{e.examCode ? ` (${e.examCode})` : ''} — {e.totalScore}đ
                        </Checkbox>
                    ))}
                </Checkbox.Group>
            )}
        </Modal>
    )
}

// ── Giao lớp/khoá ───────────────────────────────────────────────────────
function AssignmentSection({sessionId, assignments}: {sessionId: string; assignments: SessionAssignment[]}) {
    const schools = useSchoolsQuery()
    const [schoolId, setSchoolId] = useState<number>()
    const [cohortId, setCohortId] = useState<number>()
    const [cohortClassId, setCohortClassId] = useState<number>()

    const cohorts = useCohortsQuery(schoolId ?? 0)
    const classes = useCohortClassesQuery(cohortId ?? 0)

    const addAssignment = useAddAssignmentMutation()
    const removeAssignment = useRemoveAssignmentMutation()

    const handleAdd = async () => {
        if (!cohortId) {
            message.warning('Chọn khoá (hoặc lớp) để giao.')
            return
        }
        const body: CreateAssignmentBody = cohortClassId ? {cohortClassId} : {cohortId}
        await addAssignment.mutateAsync({id: sessionId, body})
        setCohortClassId(undefined)
    }

    const columns: TableColumnsType<SessionAssignment> = [
        {title: 'Trường', key: 'school', render: (_, a) => a.schoolName ?? '—'},
        {
            title: 'Khoá', key: 'cohort',
            render: (_, a) => a.cohortName ?? (a.cohortId ? `Khoá #${a.cohortId}` : '—'),
        },
        {
            title: 'Lớp', key: 'class',
            render: (_, a) => a.cohortClassName
                ?? (a.cohortClassId ? `Lớp #${a.cohortClassId}` : <span className="text-gray-400">Cả khoá</span>),
        },
        {
            title: 'Sĩ số', key: 'studentCount', width: 90, align: 'center',
            render: (_, a) => a.studentCount,
        },
        {
            title: '', key: 'actions', width: 70,
            render: (_, a) => (
                <Popconfirm title="Gỡ giao này?" okText="Gỡ" cancelText="Hủy"
                    onConfirm={() => removeAssignment.mutate({id: sessionId, assignmentId: a.id})}>
                    <button className="btn-delete">Gỡ</button>
                </Popconfirm>
            ),
        },
    ]

    return (
        <div className="bg-white rounded-xl border border-[#eceef2] p-5 flex flex-col gap-3">
            <h3 className="text-[15px] font-semibold text-[#191d27]">Giao cho lớp/khoá ({assignments.length})</h3>
            <div className="flex items-end gap-2 flex-wrap">
                <div>
                    <label className="block text-xs text-gray-500 mb-1">Trường</label>
                    <Select className="w-48" placeholder="Chọn trường" value={schoolId}
                            onChange={v => {
                                setSchoolId(v)
                                setCohortId(undefined)
                                setCohortClassId(undefined)
                            }}
                            options={(schools.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                </div>
                <div>
                    <label className="block text-xs text-gray-500 mb-1">Khoá</label>
                    <Select className="w-48" placeholder="Chọn khoá" value={cohortId} disabled={!schoolId}
                            onChange={v => {
                                setCohortId(v)
                                setCohortClassId(undefined)
                            }}
                            options={(cohorts.data ?? []).map(c => ({value: c.id, label: c.name}))}/>
                </div>
                <div>
                    <label className="block text-xs text-gray-500 mb-1">Lớp (tuỳ chọn)</label>
                    <Select className="w-48" placeholder="Cả khoá" allowClear value={cohortClassId} disabled={!cohortId}
                            onChange={v => setCohortClassId(v)}
                            options={(classes.data ?? []).map(c => ({value: c.id, label: c.className}))}/>
                </div>
                <Button type="primary" loading={addAssignment.isPending} onClick={handleAdd}>Giao</Button>
            </div>

            <Table columns={columns} dataSource={assignments} rowKey="id" size="small" pagination={false}
                   scroll={{x: 700}}
                   locale={{emptyText: 'Chưa giao cho lớp/khoá nào.'}}/>
        </div>
    )
}
