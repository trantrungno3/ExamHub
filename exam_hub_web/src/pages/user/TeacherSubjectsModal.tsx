import {useEffect, useMemo, useState} from 'react'
import type {TableColumnsType} from 'antd'
import {Input, message, Modal, Table, Tag} from 'antd'
import {SearchOutlined} from '@ant-design/icons'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {teacherSubjectService} from '../../services/teacherSubjectService'

type Props = {
    open: boolean
    userId: string | null
    userName: string | null
    onClose: () => void
}

export function TeacherSubjectsModal({open, userId, userName, onClose}: Readonly<Props>) {
    const subjects = useSubjectsQuery()
    const grades = useGradeLevelsListQuery()
    const [original, setOriginal] = useState<number[]>([])
    const [selected, setSelected] = useState<number[]>([])
    const [search, setSearch] = useState('')
    const [loading, setLoading] = useState(false)
    const [saving, setSaving] = useState(false)

    useEffect(() => {
        if (!open || !userId) return
        setSearch('')
        setOriginal([])
        setSelected([])
        setLoading(true)
        void teacherSubjectService.getByTeacher(userId)
            .then(res => {
                const ids = (res.data ?? []).map(t => t.subjectId)
                setOriginal(ids)
                setSelected(ids)
            })
            .catch(() => message.error('Không thể tải môn học của giáo viên'))
            .finally(() => setLoading(false))
    }, [open, userId])

    const gradeName = (s: Subject) =>
        s.gradeLevel?.name ?? grades.data?.find(g => g.id === s.gradeLevelId)?.name ?? `Cấp #${s.gradeLevelId}`

    const rows = useMemo(() => {
        const list = subjects.data ?? []
        const q = search.trim().toLowerCase()
        const filtered = q
            ? list.filter(s =>
                s.name.toLowerCase().includes(q) ||
                s.code.toLowerCase().includes(q) ||
                gradeName(s).toLowerCase().includes(q))
            : list
        return [...filtered].sort((a, b) =>
            a.gradeLevelId - b.gradeLevelId || a.name.localeCompare(b.name))
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [subjects.data, grades.data, search])

    const codeFilters = useMemo(() =>
            [...new Set((subjects.data ?? []).map(s => s.code))]
                .sort((a, b) => a.localeCompare(b))
                .map(c => ({text: c, value: c})),
        [subjects.data])
    const gradeFilters = useMemo(() => {
        const ids = [...new Set((subjects.data ?? []).map(s => s.gradeLevelId))]
        return ids
            .map(id => {
                const g = grades.data?.find(x => x.id === id)
                return {text: g?.name ?? `Cấp #${id}`, value: id, order: g?.gradeNumber ?? id}
            })
            .sort((a, b) => a.order - b.order)
            .map(({text, value}) => ({text, value}))
    }, [subjects.data, grades.data])

    const columns: TableColumnsType<Subject> = [
        {
            title: 'Tên môn', dataIndex: 'name', key: 'name', width: 280,
            render: v => <span className="font-medium">{v}</span>
        },
        {
            title: 'Mã', dataIndex: 'code', key: 'code', width: 120,
            filters: codeFilters, filterSearch: true, onFilter: (v, r) => r.code === v,
            render: v => <span className="font-mono text-xs text-gray-500">{v}</span>
        },
        {
            title: 'Cấp lớp', key: 'grade', width: 150,
            filters: gradeFilters, onFilter: (v, r) => r.gradeLevelId === v,
            render: (_, s) => <Tag color="blue">{gradeName(s)}</Tag>
        },
    ]

    const handleOk = async () => {
        if (!userId) return
        const toAdd = selected.filter(id => !original.includes(id))
        const toRemove = original.filter(id => !selected.includes(id))
        if (toAdd.length === 0 && toRemove.length === 0) {
            onClose();
            return
        }

        setSaving(true)
        try {
            await Promise.all([
                ...toAdd.map(id => teacherSubjectService.assign(userId, id)),
                ...toRemove.map(id => teacherSubjectService.remove(userId, id)),
            ])
            message.success('Cập nhật môn học phụ trách thành công')
            onClose()
        } catch {
            message.error('Có lỗi xảy ra khi cập nhật môn học')
        } finally {
            setSaving(false)
        }
    }

    return (
        <Modal
            title={`Phân công môn học — ${userName ?? ''}`}
            open={open}
            onOk={handleOk}
            onCancel={onClose}
            okText="Lưu"
            cancelText="Hủy"
            confirmLoading={saving}
            width={620}
        >
            <div className="mt-4 flex flex-col gap-3">
                <div className="flex items-center justify-between">
                    <Input
                        prefix={<SearchOutlined className="text-gray-400"/>}
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        placeholder="Tìm theo tên môn, mã, cấp lớp..."
                        style={{width: 280}}
                        allowClear
                    />
                    <span className="text-xs text-gray-400">Đã chọn {selected.length} môn</span>
                </div>
                <Table
                    virtual
                    columns={columns}
                    dataSource={rows}
                    rowKey="id"
                    size="small"
                    loading={loading || subjects.isLoading}
                    pagination={false}
                    scroll={{x: 550, y: 360}}
                    rowSelection={{
                        selectedRowKeys: selected,
                        onChange: keys => setSelected(keys as number[]),
                    }}
                />
            </div>
        </Modal>
    )
}
