export const EXAM_TEMPLATE_STATS = [
    { label: 'Tổng mẫu đề',  value: '28',  iconBg: 'bg-blue-100',   icon: '📋' },
    { label: 'Đã công bố',    value: '19',  iconBg: 'bg-green-100',  icon: '✅' },
    { label: 'Tổng câu hỏi', value: '186', iconBg: 'bg-orange-100', icon: '❓' },
    { label: 'Lớp tham gia', value: '35',  iconBg: 'bg-gray-100',   icon: '🏫' },
]

export type MockTemplate = {
    name: string
    grade: string
    subject: string
    count: number
    score: number
    time: string
    dist: [number, number, number, number]
    status: string
}

export const MOCK_TEMPLATES: MockTemplate[] = [
    { name: 'Kiểm tra Vật lý - Tuần 10',     grade: 'Lớp 10', subject: 'Vật lý',   count: 10, score: 12, time: '45 phút', dist: [40,30,20,10], status: 'Hoàn chỉnh' },
    { name: 'Thi HK1 - Vật lý Lớp 12',       grade: 'Lớp 12', subject: 'Vật lý',   count: 40, score: 10, time: '90 phút', dist: [20,40,30,10], status: 'Hoàn chỉnh' },
    { name: 'Ôn tập Toán 12 - Giới hạn',     grade: 'Lớp 12', subject: 'Toán',     count: 60, score: 10, time: '90 phút', dist: [30,30,25,15], status: 'Nháp'       },
    { name: 'Kiểm tra 15 phút - Toán',        grade: 'Lớp 10', subject: 'Toán',     count: 5,  score: 1,  time: '15 phút', dist: [50,30,15, 5], status: 'Hoàn chỉnh' },
    { name: 'Kiểm tra Sinh học - Chương 2',   grade: 'Lớp 11', subject: 'Sinh học', count: 6,  score: 1,  time: '15 phút', dist: [40,35,15,10], status: 'Hoàn chỉnh' },
    { name: 'Kiểm tra 1 tiết - Lịch sử',     grade: 'Lớp 12', subject: 'Lịch sử',  count: 40, score: 10, time: '45 phút', dist: [25,35,30,10], status: 'Nháp'       },
    { name: 'Ôn nguyện vọng - Hóa Chương 1', grade: 'Lớp 10', subject: 'Hóa học',  count: 20, score: 10, time: '30 phút', dist: [35,30,20,15], status: 'Hoàn chỉnh' },
]
