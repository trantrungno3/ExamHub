import {useState} from 'react'
import {Button, Drawer, Empty, Popconfirm, Select, Table} from 'antd'
import type {TableColumnsType} from 'antd'
import {useQuery} from '@tanstack/react-query'
import {userService} from '../../services/userService'
import {useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {statusCode} from '../../services/requestService'
import {
    useAssignTeacherMutation,
    useClassTeachersQuery,
    useEligibleTeachersQuery,
    useRemoveTeacherMutation,
} from '../../hooks/queries/useCohortClassTeachers'

interface Props {
    cohortClass?: CohortClass
    open: boolean
    onClose: () => void
}

export function TeachingAssignmentDrawer({cohortClass, open, onClose}: Props) {
    const classId = cohortClass?.id ?? 0
    const [subjectId, setSubjectId] = useState<number>()
    const [teacherId, setTeacherId] = useState<string>()

    const {data: subjects = []} = useSubjectsQuery()
    const {data: allUsers = []} = useQuery({
        queryKey: ['users'],
        queryFn: async () => (await userService.getAll()).data ?? [],
    })
    const {data: assignments = []} = useClassTeachersQuery(classId)
    const {data: eligibleIds = []} = useEligibleTeachersQuery(classId, subjectId)
    const assignMut = useAssignTeacherMutation(classId)
    const removeMut = useRemoveTeacherMutation(classId)

    const subjectName = (id: number) => subjects.find(s => s.id === id)?.name ?? `#${id}`
    const teacherName = (id: string) => {
        const u = allUsers.find(x => x.id === id)
        return u ? (u.displayName ?? u.userName) : id
    }
    const eligibleTeachers = allUsers.filter(u => eligibleIds.includes(u.id))

    const handleAssign = async () => {
        if (!subjectId || !teacherId) return
        const res = await assignMut.mutateAsync({cohortClassId: classId, subjectId, teacherId})
        if (res.status !== statusCode.Error) {
            setSubjectId(undefined)
            setTeacherId(undefined)
        }
    }

    const columns: TableColumnsType<CohortClassTeacher> = [
        {title: 'Môn học', dataIndex: 'subjectId', key: 'subjectId', render: (v: number) => subjectName(v)},
        {title: 'Giáo viên', dataIndex: 'teacherId', key: 'teacherId', render: (v: string) => teacherName(v)},
        {
            title: 'Thao tác', key: 'actions', width: 90,
            render: (_, r) => (
                <Popconfirm title="Xoá phân công?" okText="Xoá" cancelText="Huỷ" okButtonProps={{danger: true}}
                    onConfirm={() => removeMut.mutate(r.id)}>
                    <button className="btn-delete">Xoá</button>
                </Popconfirm>
            ),
        },
    ]

    return (
        <Drawer title={`Phân công giảng dạy — Lớp ${cohortClass?.className ?? ''}`} open={open} onClose={onClose} width={560}>
            <div className="flex flex-col gap-4">
                <div className="flex gap-2 items-end">
                    <div className="flex-1">
                        <div className="text-[12px] text-gray-500 mb-1">Môn học</div>
                        <Select className="w-full" placeholder="Chọn môn" value={subjectId} showSearch optionFilterProp="label"
                            onChange={(v) => { setSubjectId(v); setTeacherId(undefined) }}
                            options={subjects.map(s => ({value: s.id, label: s.name}))}/>
                    </div>
                    <div className="flex-1">
                        <div className="text-[12px] text-gray-500 mb-1">Giáo viên</div>
                        <Select className="w-full" placeholder={subjectId ? 'Chọn GV' : 'Chọn môn trước'} value={teacherId}
                            disabled={!subjectId} showSearch optionFilterProp="label"
                            notFoundContent="Không có GV hợp lệ"
                            onChange={setTeacherId}
                            options={eligibleTeachers.map(u => ({value: u.id, label: u.displayName ?? u.userName}))}/>
                    </div>
                    <Button type="primary" disabled={!subjectId || !teacherId} loading={assignMut.isPending} onClick={handleAssign}>
                        Thêm
                    </Button>
                </div>

                <Table columns={columns} dataSource={assignments} rowKey="id" size="small" pagination={false}
                    locale={{emptyText: <Empty description="Chưa có phân công nào"/>}}/>
            </div>
        </Drawer>
    )
}
