export const DASHBOARD_STATS = [
    { label: 'Tổng câu hỏi', value: '2,841', trend: '↑ +124 tuần này', iconBg: 'bg-blue-100',   iconColor: 'text-blue-600',   icon: '❓' },
    { label: 'Đề thi đã tạo', value: '186',   trend: '↑ +12 tuần này',  iconBg: 'bg-green-100',  iconColor: 'text-green-600',  icon: '📄' },
    { label: 'Giáo viên',     value: '48',    trend: '↑ +6 môn học',    iconBg: 'bg-purple-100', iconColor: 'text-purple-600', icon: '👨‍🏫' },
    { label: 'Học sinh',      value: '1,204', trend: '↑ +12 lớp học',   iconBg: 'bg-orange-100', iconColor: 'text-orange-600', icon: '🎓' },
]

export const RECENT_EXAMS = [
    { name: 'Kiểm tra Toán HK1 Lớp 10', subject: 'Toán',    count: 40, status: 'Đã công bố', date: '20/12/2024' },
    { name: 'Ôn thi Vật lý Chương 3',    subject: 'Vật lý',  count: 30, status: 'Nháp',       date: '19/12/2024' },
    { name: 'Kiểm tra Tiếng Anh 15p',    subject: 'Anh văn', count: 20, status: 'Đã công bố', date: '18/12/2024' },
    { name: 'Đề cương Hóa học Lớp 11',   subject: 'Hóa học', count: 50, status: 'Lưu trữ',   date: '17/12/2024' },
    { name: 'Kiểm tra Sinh học Tuần 8',   subject: 'Sinh học',count: 25, status: 'Nháp',       date: '16/12/2024' },
    { name: 'Ôn tập Lịch sử Lớp 12',    subject: 'Lịch sử', count: 35, status: 'Đã công bố', date: '15/12/2024' },
    { name: 'Kiểm tra Địa lý HK2',       subject: 'Địa lý',  count: 30, status: 'Nháp',       date: '14/12/2024' },
]

export const CHART_BARS = [
    { label: 'Toán', heights: [72, 52] as [number, number], colors: ['bg-blue-500', 'bg-purple-400'] },
    { label: 'Lý',   heights: [48, 36] as [number, number], colors: ['bg-blue-500', 'bg-purple-400'] },
    { label: 'Hóa',  heights: [56, 28] as [number, number], colors: ['bg-blue-500', 'bg-purple-400'] },
    { label: 'Sinh', heights: [36, 48] as [number, number], colors: ['bg-blue-500', 'bg-purple-400'] },
    { label: 'Sử',   heights: [28, 20] as [number, number], colors: ['bg-blue-500', 'bg-purple-400'] },
    { label: 'Văn',  heights: [44, 36] as [number, number], colors: ['bg-blue-500', 'bg-purple-400'] },
]
