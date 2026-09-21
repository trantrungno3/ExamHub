import {useMemo} from 'react'
import {ProfileCard} from './ProfileCard'
import {useAuth} from '../../hooks/useAuth'
import {useMySubmissionsQuery} from '../../hooks/queries/useSubmissions'
import {BRAND} from '../../constants/theme'

function StatBox({label, value, tone}: {label: string; value: string | number; tone: 'blue' | 'green'}) {
    const c = tone === 'blue' ? {bg: BRAND.primaryTint, fg: BRAND.primary} : {bg: BRAND.successTint, fg: BRAND.success}
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

    const stats = useMemo(() => {
        const done = submissions.length
        const graded = submissions.filter(s => s.status === 'Graded' && s.totalScore != null)
        const avg = graded.length
            ? (graded.reduce((sum, s) => sum + (s.totalScore ?? 0), 0) / graded.length).toFixed(1)
            : '—'
        return {done, avg}
    }, [submissions])

    return (
        <div className="p-6 sm:p-8 flex flex-col gap-4 max-w-5xl mx-auto w-full">
            <div>
                <h1 className="text-[26px] font-bold" style={{color: BRAND.ink}}>Hồ sơ của tôi</h1>
                <p className="text-[13.5px] mt-1" style={{color: '#6f6a60'}}>Thông tin cá nhân và kết quả học tập</p>
            </div>

            <div className="flex gap-4">
                <StatBox tone="blue" label="Số đề đã làm" value={stats.done}/>
                <StatBox tone="green" label="Điểm trung bình (đã chấm)" value={stats.avg}/>
            </div>

            <ProfileCard/>
        </div>
    )
}
