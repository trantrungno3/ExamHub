import {BRAND} from '../constants/theme'

const MAP = {
    success: {bg: BRAND.successSoft, fg: BRAND.success},
    danger: {bg: BRAND.dangerSoft, fg: BRAND.danger},
    warning: {bg: BRAND.warningSoft, fg: BRAND.warning},
    // #eef0f3 không có trong @theme — giữ nguyên.
    default: {bg: '#eef0f3', fg: BRAND.muted},
} as const

export type StatusVariant = keyof typeof MAP

export function StatusTag({status, label}: {status: StatusVariant; label: string}) {
    const c = MAP[status]
    return (
        <span
            style={{background: c.bg, color: c.fg}}
            className="inline-flex items-center rounded-full px-2.5 py-0.5 text-[12px] font-medium leading-none whitespace-nowrap"
        >
            {label}
        </span>
    )
}
