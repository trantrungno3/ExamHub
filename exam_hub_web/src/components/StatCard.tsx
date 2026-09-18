import type {ReactNode} from 'react'
import {BRAND} from '../constants/theme'

type Props = {
    label: string
    value?: number
    icon: ReactNode
    /** Màu chữ của ô icon. */
    color: string
    /** Màu nền của ô icon. */
    bg: string
}

export function StatCard({label, value, icon, color, bg}: Props) {
    return (
        <div className="flex-1 bg-white rounded-xl border p-4 flex items-center gap-3"
             style={{borderColor: BRAND.border}}>
            <div className="w-10 h-10 rounded-lg flex items-center justify-center text-[18px]"
                 style={{background: bg, color}}>
                {icon}
            </div>
            <div>
                <div className="text-[22px] font-bold leading-tight" style={{color: BRAND.ink}}>
                    {value != null ? value.toLocaleString('vi-VN') : '—'}
                </div>
                <div className="text-[12px]" style={{color: BRAND.muted}}>{label}</div>
            </div>
        </div>
    )
}
