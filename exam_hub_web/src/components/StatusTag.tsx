const MAP = {
    success: {bg: '#dff5ed', fg: '#1ea375'},
    danger: {bg: '#fee5e5', fg: '#e74242'},
    warning: {bg: '#fff4e5', fg: '#d98a00'},
    default: {bg: '#eef0f3', fg: '#6f7788'},
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
