import {useMemo} from 'react'
import {useQuery} from '@tanstack/react-query'
import {Table} from 'antd'
import type {TableColumnsType} from 'antd'
import {ProfileCard} from './ProfileCard'
import {StatusTag} from '../../components/StatusTag'
import {useAuth} from '../../AuthProvider'
import {useMySubmissionsQuery} from '../../hooks/queries/useSubmissions'
import {cohortMemberService} from '../../services/cohortMemberService'

function StatBox({label, value, tone}: {label: string; value: string | number; tone: 'blue' | 'green'}) {
    const c = tone === 'blue' ? {bg: '#eef1ff', fg: '#3a74f5'} : {bg: '#e7f7ef', fg: '#1ea375'}
    return (
        <div className="rounded-xl p-5 flex-1" style={{background: c.bg}}>
            <p className="text-[28px] font-bold" style={{color: c.fg}}>{value}</p>
            <p className="text-[13px] mt-1" style={{color: '#6f6a60'}}>{label}</p>
        </div>
    )
}

export default function StudentProfilePage() {
    const {user} = useAuth()
    const {data: submissions = []} = useMySubmissionsQuery(user?.id)
    const {data: cohortMembers = []} = useQuery({
        queryKey: ['cohortMembers', 'student', user?.id],
        queryFn: async () => (await cohortMemberService.getByStudent(user!.id)).data ?? [],
        enabled: !!user?.id,
    })

    const stats = useMemo(() => {
        const done = submissions.length
        const graded = submissions.filter(s => s.status === 'Graded' && s.totalScore != null)
        const avg = graded.length
            ? (graded.reduce((sum, s) => sum + (s.totalScore ?? 0), 0) / graded.length).toFixed(1)
            : '—'
        return {done, avg}
    }, [submissions])

    const cohortColumns: TableColumnsType<CohortMember> = [
        {title: 'Khoá', dataIndex: 'cohortId', key: 'cohortId', render: v => `Khoá #${v}`},
        {title: 'Ngày tham gia', dataIndex: 'joinedAt', key: 'joinedAt',
            render: v => v ? new Date(v).toLocaleDateString('vi-VN') : '—'},
        {title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive',
            render: v => <StatusTag status={v ? 'success' : 'default'} label={v ? 'Đang học' : 'Ngừng'}/>},
    ]

    return (
        <div className="p-6 flex flex-col gap-4">
            <p className="text-xl font-semibold text-gray-800">Hồ sơ của tôi</p>

            <div className="flex gap-4">
                <StatBox tone="blue" label="Số đề đã làm" value={stats.done}/>
                <StatBox tone="green" label="Điểm trung bình (đã chấm)" value={stats.avg}/>
            </div>

            <ProfileCard/>

            <div>
                <p className="font-medium text-gray-700 mb-2">Khoá học đang tham gia</p>
                <div className="section-card shrink-0">
                    <Table columns={cohortColumns} dataSource={cohortMembers} rowKey="id" pagination={false}
                           scroll={{x: 600}}
                           locale={{emptyText: 'Chưa tham gia khoá học nào'}}/>
                </div>
            </div>
        </div>
    )
}
