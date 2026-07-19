import {useEffect, useMemo, useState} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Button, Checkbox, Input, InputNumber, Modal, Popconfirm, Select, Spin, Table, Tag, message} from 'antd'
import type {TableColumnsType} from 'antd'
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

const PICK_MODE_OPTIONS = [
    {value: 'Random', label: 'Ngẫu nhiên (hệ thống bốc đề)'},
    {value: 'StudentChoice', label: 'Học sinh tự chọn đề'},
]

/** epoch ms → giá trị cho input datetime-local (giờ địa phương). */
function msToLocalInput(ms: number): string {
    const d = new Date(ms)
    const pad = (n: number) => String(n).padStart(2, '0')
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
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

    // ── Form state ──────────────────────────────────────────────────────
    const [title, setTitle] = useState('')
    const [description, setDescription] = useState('')
    const [subjectId, setSubjectId] = useState<number>()
    const [gradeLevelId, setGradeLevelId] = useState<number>()
    const [openLocal, setOpenLocal] = useState('')
    const [closeLocal, setCloseLocal] = useState('')
    const [maxAttempts, setMaxAttempts] = useState(1)
    const [pickMode, setPickMode] = useState<ExamSessionPickMode>('Random')

    useEffect(() => {
        if (!detail) return
        setTitle(detail.title)
        setDescription(detail.description ?? '')
        setSubjectId(detail.subjectId)
        setGradeLevelId(detail.gradeLevelId)
        setOpenLocal(msToLocalInput(detail.openAt))
        setCloseLocal(msToLocalInput(detail.closeAt))
        setMaxAttempts(detail.maxAttempts)
        setPickMode(detail.pickMode)
    }, [detail])

    const canSave = title.trim() && subjectId && gradeLevelId && openLocal && closeLocal

    const handleSave = async () => {
        if (!canSave) {
            message.warning('Vui lòng nhập đủ tiêu đề, môn, cấp lớp và khung giờ.')
            return
        }
        const body: ExamSessionBody = {
            title: title.trim(),
            description: description.trim() || undefined,
            subjectId: subjectId!,
            gradeLevelId: gradeLevelId!,
            openAt: new Date(openLocal).toISOString(),
            closeAt: new Date(closeLocal).toISOString(),
            maxAttempts,
            pickMode,
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

            <div className="flex-1 overflow-auto p-6 flex flex-col gap-4 max-w-4xl">
                {isLoading && isEdit ? (
                    <Spin/>
                ) : (
                    <>
                        {/* ── Cấu hình ── */}
                        <div className="section-card flex flex-col gap-4">
                            <h3 className="font-semibold text-gray-800">Thông tin kỳ thi</h3>
                            <div>
                                <label className="block text-sm text-gray-600 mb-1">Tiêu đề *</label>
                                <Input value={title} onChange={e => setTitle(e.target.value)} placeholder="VD: Kiểm tra giữa kỳ 1"/>
                            </div>
                            <div>
                                <label className="block text-sm text-gray-600 mb-1">Mô tả</label>
                                <Input.TextArea value={description} onChange={e => setDescription(e.target.value)} rows={2}/>
                            </div>
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm text-gray-600 mb-1">Môn *</label>
                                    <Select className="w-full" showSearch optionFilterProp="label" value={subjectId}
                                            disabled={isPublished}
                                            onChange={setSubjectId}
                                            options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-600 mb-1">Cấp lớp *</label>
                                    <Select className="w-full" value={gradeLevelId} disabled={isPublished}
                                            onChange={setGradeLevelId}
                                            options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-600 mb-1">Mở lúc *</label>
                                    <input type="datetime-local" className="ant-input w-full border border-gray-300 rounded px-2 py-1"
                                           value={openLocal} onChange={e => setOpenLocal(e.target.value)}/>
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-600 mb-1">Đóng lúc *</label>
                                    <input type="datetime-local" className="ant-input w-full border border-gray-300 rounded px-2 py-1"
                                           value={closeLocal} onChange={e => setCloseLocal(e.target.value)}/>
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-600 mb-1">Số lượt tối đa</label>
                                    <InputNumber className="w-full" min={1} max={100} value={maxAttempts}
                                                 onChange={v => setMaxAttempts(v ?? 1)}/>
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-600 mb-1">Cách chọn đề</label>
                                    <Select className="w-full" value={pickMode} options={PICK_MODE_OPTIONS}
                                            onChange={v => setPickMode(v as ExamSessionPickMode)}/>
                                </div>
                            </div>
                            <div>
                                <Button type="primary" loading={create.isPending || update.isPending} onClick={handleSave}>
                                    {isEdit ? 'Lưu thay đổi' : 'Tạo & tiếp tục'}
                                </Button>
                            </div>
                        </div>

                        {isEdit && detail && (
                            <>
                                <PoolSection sessionId={detail.id} exams={detail.exams}
                                             subjectId={detail.subjectId} gradeLevelId={detail.gradeLevelId}/>
                                <AssignmentSection sessionId={detail.id} assignments={detail.assignments}/>

                                <div className="section-card flex items-center justify-between">
                                    <div>
                                        <h3 className="font-semibold text-gray-800">Phát hành</h3>
                                        <p className="text-sm text-gray-500">Cần ≥1 đề và ≥1 lớp/khoá, thời điểm đóng ở tương lai.</p>
                                    </div>
                                    <Button type="primary" disabled={isPublished} loading={publish.isPending}
                                            onClick={() => publish.mutate(detail.id)}>
                                        {isPublished ? 'Đã phát hành' : 'Phát hành kỳ thi'}
                                    </Button>
                                </div>
                            </>
                        )}
                    </>
                )}
            </div>
        </>
    )
}

// ── Pool đề ─────────────────────────────────────────────────────────────
function PoolSection({sessionId, exams, subjectId, gradeLevelId}: {
    sessionId: string; exams: SessionExam[]; subjectId: number; gradeLevelId: number
}) {
    const [modalOpen, setModalOpen] = useState(false)
    const removeExam = useRemoveSessionExamMutation()

    const columns: TableColumnsType<SessionExam> = [
        {title: 'Tiêu đề', dataIndex: 'title', key: 'title'},
        {title: 'Mã đề', dataIndex: 'examCode', key: 'examCode', width: 120, render: v => v ?? '—'},
        {title: 'Điểm', dataIndex: 'totalScore', key: 'totalScore', width: 80},
        {
            title: '', key: 'actions', width: 80,
            render: (_, e) => (
                <Popconfirm title="Gỡ đề khỏi kỳ thi?" okText="Gỡ" cancelText="Hủy"
                            onConfirm={() => removeExam.mutate({id: sessionId, examId: e.examId})}>
                    <button className="btn-delete">Gỡ</button>
                </Popconfirm>
            ),
        },
    ]

    return (
        <div className="section-card flex flex-col gap-3">
            <div className="flex items-center justify-between">
                <h3 className="font-semibold text-gray-800">Đề trong kỳ thi ({exams.length})</h3>
                <Button icon={<PlusOutlined/>} onClick={() => setModalOpen(true)}>Thêm đề</Button>
            </div>
            <Table columns={columns} dataSource={exams} rowKey="examId" size="small" pagination={false}
                   locale={{emptyText: 'Chưa có đề nào'}}/>
            <AddExamsModal open={modalOpen} onClose={() => setModalOpen(false)} sessionId={sessionId}
                           subjectId={subjectId} gradeLevelId={gradeLevelId}
                           existingIds={exams.map(e => e.examId)}/>
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

    return (
        <div className="section-card flex flex-col gap-3">
            <h3 className="font-semibold text-gray-800">Giao cho ({assignments.length})</h3>
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

            <div className="flex flex-col gap-2">
                {assignments.length === 0 && <p className="text-gray-500 text-sm">Chưa giao cho lớp/khoá nào.</p>}
                {assignments.map(a => (
                    <div key={a.id} className="flex items-center justify-between border border-gray-100 rounded px-3 py-2">
                        <span className="text-sm text-gray-700">
                            {a.cohortClassId
                                ? (a.cohortClassName ?? `Lớp #${a.cohortClassId}`)
                                : (a.cohortName ?? `Khoá #${a.cohortId}`)}
                        </span>
                        <Popconfirm title="Gỡ giao này?" okText="Gỡ" cancelText="Hủy"
                                    onConfirm={() => removeAssignment.mutate({id: sessionId, assignmentId: a.id})}>
                            <button className="btn-delete">Gỡ</button>
                        </Popconfirm>
                    </div>
                ))}
            </div>
        </div>
    )
}
