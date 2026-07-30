import {useCallback, useEffect, useState} from 'react'
import {Switch, Table, message} from 'antd'
import type {TableColumnsType} from 'antd'
import {cognitiveLevelService} from '../../../services/cognitiveLevelService'

const LEVEL_VISUAL: Record<number, {bg: string; tagColor: string; width: string}> = {
    1: {bg: 'bg-sky-200',   tagColor: 'bg-sky-100 text-sky-700',     width: 'w-full'},
    2: {bg: 'bg-blue-300',  tagColor: 'bg-blue-100 text-blue-700',   width: 'w-[87%]'},
    3: {bg: 'bg-amber-300', tagColor: 'bg-amber-100 text-amber-700', width: 'w-[74%]'},
    4: {bg: 'bg-blue-500',  tagColor: 'bg-blue-100 text-blue-700',   width: 'w-[60%]'},
    5: {bg: 'bg-rose-400',  tagColor: 'bg-rose-100 text-rose-700',   width: 'w-[47%]'},
    6: {bg: 'bg-red-600',   tagColor: 'bg-red-100 text-red-700',     width: 'w-[33%]'},
}

function vis(levelOrder: number) {
    return LEVEL_VISUAL[levelOrder] ?? {bg: 'bg-gray-300', tagColor: 'bg-gray-100 text-gray-700', width: 'w-full'}
}

export function CognitiveTab() {
    const [data, setData] = useState<CognitiveLevel[]>([])
    const [loading, setLoading] = useState(true)

    const fetchData = useCallback(() => {
        void cognitiveLevelService.getAll()
            .then(res => setData((res.data ?? []).sort((a, b) => a.levelOrder - b.levelOrder)))
            .catch(() => message.error('Không thể tải dữ liệu cấp độ nhận thức'))
            .finally(() => setLoading(false))
    }, [])

    useEffect(() => { fetchData() }, [fetchData])

    const toggleActive = useCallback(async (id: number, checked: boolean) => {
        try {
            await cognitiveLevelService.toggleActive(id, checked)
            setData(prev => prev.map(l => l.id === id ? {...l, isActive: checked} : l))
        } catch {
            message.error('Không thể cập nhật trạng thái')
        }
    }, [])

    const columns: TableColumnsType<CognitiveLevel> = [
        {
            title: 'Cấp độ', key: 'level',
            render: (_, record) => (
                <div className="flex items-center gap-2">
                    <div className={`w-6 h-6 rounded-full flex items-center justify-center text-white text-[11px] font-bold shrink-0 ${vis(record.levelOrder).bg}`}>
                        {record.levelOrder}
                    </div>
                    <span className="text-[13px] font-semibold text-gray-800">{record.name}</span>
                    <span className="text-[11px] text-gray-400">{record.nameEn}</span>
                </div>
            ),
        },
        {
            title: 'Mã (code)', dataIndex: 'code', key: 'code',
            render: (code, record) => (
                <span className={`badge text-[10px] ${vis(record.levelOrder).tagColor}`}>{code}</span>
            ),
        },
        {
            title: 'Từ khóa', dataIndex: 'description', key: 'description',
            render: v => <span className="text-[11px] text-gray-500">{v ?? '—'}</span>,
        },
        {
            title: 'Trạng thái', key: 'isActive', width: 120,
            render: (_, record) => (
                <Switch
                    checked={record.isActive}
                    size="small"
                    onChange={checked => toggleActive(record.id, checked)}
                    checkedChildren="Bật"
                    unCheckedChildren="Tắt"
                />
            ),
        },
    ]

    return (
        <div className="flex flex-col gap-4 p-6">
            <div className="bg-blue-50 border border-blue-100 rounded-xl px-5 py-3">
                <p className="text-[13px] text-blue-700">
                    Anderson &amp; Krathwohl (2001) — 6 cấp độ tư duy từ thấp → cao.
                    Seed data mặc định, chỉ Admin chỉnh sửa.
                </p>
            </div>

            <div className="flex gap-4">
                {/* Bloom's pyramid */}
                <div className="section-card flex-1 p-5">
                    <p className="text-sm font-semibold text-gray-700 mb-5">Tháp nhận thức Bloom</p>
                    <div className="flex flex-col items-center gap-1">
                        {[...data].reverse().map(level => (
                            <div
                                key={level.id}
                                className={`${vis(level.levelOrder).width} ${vis(level.levelOrder).bg} rounded-md px-3 py-2 flex items-center justify-between`}
                            >
                                <span className="text-white text-[12px] font-semibold">{level.name}</span>
                                <span className="text-white/75 text-[11px]">{level.nameEn}</span>
                            </div>
                        ))}
                    </div>
                    <div className="mt-5 text-[12px] text-gray-500 space-y-1.5">
                        <p className="font-medium text-gray-600">Ứng dụng trong hệ thống</p>
                        <p>• Mỗi câu hỏi gắn cognitive_level_id (nullable)</p>
                        <p>• Section đề thi lọc loại câu hỏi theo cấp độ Bloom cụ thể</p>
                        <p>• Filter API /api/v1/questions?cognitiveLevel=apply</p>
                    </div>
                </div>

                {/* Detail table */}
                <div className="section-card flex-1 overflow-hidden">
                    <p className="text-sm font-semibold text-gray-700 px-5 py-4 border-b border-gray-100">
                        Chi tiết từng cấp độ
                    </p>
                    <Table
                        columns={columns}
                        dataSource={data}
                        rowKey="id"
                        pagination={false}
                        size="small"
                        loading={loading}
                        scroll={{x: 600}}
                    />
                </div>
            </div>
        </div>
    )
}
