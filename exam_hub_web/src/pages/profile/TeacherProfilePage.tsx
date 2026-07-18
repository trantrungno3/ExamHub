import {useQuery} from '@tanstack/react-query'
import {Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {ProfileCard} from './ProfileCard'
import {useAuth} from '../../AuthProvider'
import {useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {useSchoolsQuery} from '../../hooks/queries/useSchools'
import {teacherSubjectService} from '../../services/teacherSubjectService'
import {schoolMemberService} from '../../services/schoolMemberService'

export default function TeacherProfilePage() {
    const {user} = useAuth()
    const subjects = useSubjectsQuery()
    const schools = useSchoolsQuery()

    const {data: teacherSubjects = []} = useQuery({
        queryKey: ['teacherSubjects', user?.id],
        queryFn: async () => (await teacherSubjectService.getByTeacher(user!.id)).data ?? [],
        enabled: !!user?.id,
    })
    const {data: memberships = []} = useQuery({
        queryKey: ['schoolMembers', 'user', user?.id],
        queryFn: async () => (await schoolMemberService.getByUser(user!.id)).data ?? [],
        enabled: !!user?.id,
    })

    const subjectName = (id: number) =>
        teacherSubjects.find(t => t.subjectId === id)?.subject?.name
        ?? subjects.data?.find(s => s.id === id)?.name
        ?? `Môn #${id}`
    const schoolName = (id: number) => schools.data?.find(s => s.id === id)?.name ?? `Trường #${id}`

    const subjectColumns: TableColumnsType<TeacherSubject> = [
        {title: 'Môn học', dataIndex: 'subjectId', key: 'subjectId', render: v => subjectName(v)},
    ]
    const schoolColumns: TableColumnsType<SchoolMember> = [
        {title: 'Trường', dataIndex: 'schoolId', key: 'schoolId', render: v => schoolName(v)},
        {title: 'Vai trò', dataIndex: 'role', key: 'role', render: v => <Tag>{v}</Tag>},
        {title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
            render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>},
    ]

    return (
        <div className="p-6 flex flex-col gap-4">
            <p className="text-xl font-semibold text-gray-800">Thông tin giáo viên</p>

            <ProfileCard/>

            <div>
                <p className="font-medium text-gray-700 mb-2">Môn học phụ trách</p>
                <div className="section-card">
                    <Table columns={subjectColumns} dataSource={teacherSubjects} rowKey="id" pagination={false}
                           locale={{emptyText: 'Chưa được phân công môn học'}}/>
                </div>
            </div>

            <div>
                <p className="font-medium text-gray-700 mb-2">Trường giảng dạy</p>
                <div className="section-card">
                    <Table columns={schoolColumns} dataSource={memberships} rowKey="id" pagination={false}
                           locale={{emptyText: 'Chưa thuộc trường nào'}}/>
                </div>
            </div>
        </div>
    )
}
