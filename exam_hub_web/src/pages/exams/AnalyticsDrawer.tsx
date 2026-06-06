import {Drawer, Empty, Spin} from 'antd'
import {Bar, BarChart, CartesianGrid, Cell, ResponsiveContainer, Tooltip, XAxis, YAxis} from 'recharts'
import {useExamAnalyticsQuery} from '../../hooks/queries/useExams'

type Props = {examId?: string; onClose: () => void}

const COLORS = ['#4CAF50', '#2196F3', '#FF9800', '#9C27B0', '#F44336', '#E91E63', '#00BCD4', '#795548']

export function AnalyticsDrawer({examId, onClose}: Props) {
    const {data, isLoading} = useExamAnalyticsQuery(examId)

    return (
        <Drawer title="Phân tích đề thi" open={!!examId} onClose={onClose} width={560}>
            {isLoading && <Spin/>}
            {!isLoading && !data && <Empty description="Chưa có dữ liệu phân tích"/>}
            {data && (
                <div className="flex flex-col gap-6">
                    <p className="text-sm text-gray-500">Tổng số câu hỏi: <b>{data.totalQuestions}</b></p>
                    <DistributionChart title="Theo cấp độ Bloom" items={data.bloomDistribution}/>
                    <DistributionChart title="Theo độ khó" items={data.difficultyDistribution}/>
                    <DistributionChart title="Theo chủ đề" items={data.topicDistribution}/>
                </div>
            )}
        </Drawer>
    )
}

function DistributionChart({title, items}: {title: string; items: DistributionItem[]}) {
    if (items.length === 0) return null
    return (
        <div>
            <p className="text-[13px] font-semibold text-gray-700 mb-2">{title}</p>
            <ResponsiveContainer width="100%" height={Math.max(140, items.length * 38)}>
                <BarChart data={items} layout="vertical" margin={{left: 20, right: 30}}>
                    <CartesianGrid strokeDasharray="3 3" horizontal={false}/>
                    <XAxis type="number" allowDecimals={false}/>
                    <YAxis type="category" dataKey="label" width={120} tick={{fontSize: 12}}/>
                    <Tooltip/>
                    <Bar dataKey="count" name="Số câu" radius={[0, 4, 4, 0]}>
                        {items.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]}/>)}
                    </Bar>
                </BarChart>
            </ResponsiveContainer>
        </div>
    )
}
