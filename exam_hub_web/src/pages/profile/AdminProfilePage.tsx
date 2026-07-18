import {useQuery} from '@tanstack/react-query'
import {ProfileCard} from './ProfileCard'
import {useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {useSchoolsQuery} from '../../hooks/queries/useSchools'
import {userService} from '../../services/userService'
import {examService} from '../../services/examService'

function StatBox({label, value}: {label: string; value: string | number}) {
    return (
        <div className="section-card p-4 flex-1">
            <p className="text-gray-400 text-xs">{label}</p>
            <p className="text-2xl font-bold text-blue-600 mt-1">{value}</p>
        </div>
    )
}

export default function AdminProfilePage() {
    const subjects = useSubjectsQuery()
    const schools = useSchoolsQuery()

    const {data: userCount = 0} = useQuery({
        queryKey: ['stats', 'users'],
        queryFn: async () => (await userService.getAll()).data?.length ?? 0,
    })
    const {data: examTotal = 0} = useQuery({
        queryKey: ['stats', 'exams'],
        queryFn: async () => (await examService.getPaged({page: 1, pageSize: 1})).data?.total ?? 0,
    })

    return (
        <div className="p-6 flex flex-col gap-4">
            <p className="text-xl font-semibold text-gray-800">Thông tin quản trị viên</p>

            <ProfileCard/>

            <div>
                <p className="font-medium text-gray-700 mb-2">Thống kê hệ thống</p>
                <div className="flex gap-4 flex-wrap">
                    <StatBox label="Người dùng" value={userCount}/>
                    <StatBox label="Đề thi" value={examTotal}/>
                    <StatBox label="Trường học" value={schools.data?.length ?? 0}/>
                    <StatBox label="Môn học" value={subjects.data?.length ?? 0}/>
                </div>
            </div>
        </div>
    )
}
