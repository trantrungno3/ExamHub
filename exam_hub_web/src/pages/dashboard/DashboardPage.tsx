import {
  FileTextOutlined,
  PlusOutlined,
  ThunderboltOutlined,
  UploadOutlined,
} from '@ant-design/icons'

/* ─── Data ───────────────────────────────────────── */
const STATS = [
  {
    label: 'Tổng câu hỏi',
    value: '2,841',
    trend: '↑ +124 tuần này',
    iconBg: 'bg-blue-100',
    iconColor: 'text-blue-600',
    icon: '❓',
  },
  {
    label: 'Đề thi đã tạo',
    value: '186',
    trend: '↑ +12 tuần này',
    iconBg: 'bg-green-100',
    iconColor: 'text-green-600',
    icon: '📄',
  },
  {
    label: 'Giáo viên',
    value: '48',
    trend: '↑ +6 môn học',
    iconBg: 'bg-purple-100',
    iconColor: 'text-purple-600',
    icon: '👨‍🏫',
  },
  {
    label: 'Học sinh',
    value: '1,204',
    trend: '↑ +12 lớp học',
    iconBg: 'bg-orange-100',
    iconColor: 'text-orange-600',
    icon: '🎓',
  },
]

const EXAMS = [
  { name: 'Kiểm tra Toán HK1 Lớp 10', subject: 'Toán',    count: 40, status: 'Đã công bố', date: '20/12/2024' },
  { name: 'Ôn thi Vật lý Chương 3',    subject: 'Vật lý',  count: 30, status: 'Nháp',       date: '19/12/2024' },
  { name: 'Kiểm tra Tiếng Anh 15p',    subject: 'Anh văn', count: 20, status: 'Đã công bố', date: '18/12/2024' },
  { name: 'Đề cương Hóa học Lớp 11',   subject: 'Hóa học', count: 50, status: 'Lưu trữ',   date: '17/12/2024' },
  { name: 'Kiểm tra Sinh học Tuần 8',   subject: 'Sinh học',count: 25, status: 'Nháp',       date: '16/12/2024' },
  { name: 'Ôn tập Lịch sử Lớp 12',    subject: 'Lịch sử', count: 35, status: 'Đã công bố', date: '15/12/2024' },
  { name: 'Kiểm tra Địa lý HK2',       subject: 'Địa lý',  count: 30, status: 'Nháp',       date: '14/12/2024' },
]

const QUICK_ACTIONS = [
  { label: 'Thêm câu hỏi', icon: <PlusOutlined />,         bg: 'bg-blue-50',   text: 'text-blue-700'   },
  { label: 'Tạo mẫu đề',   icon: <FileTextOutlined />,     bg: 'bg-purple-50', text: 'text-purple-700' },
  { label: 'Sinh đề ngay', icon: <ThunderboltOutlined />,  bg: 'bg-green-50',  text: 'text-green-700'  },
  { label: 'Nhập Excel',   icon: <UploadOutlined />,        bg: 'bg-orange-50', text: 'text-orange-700' },
]

const CHART_BARS = [
  { label: 'Toán', heights: [72, 52], colors: ['bg-blue-500', 'bg-purple-400'] },
  { label: 'Lý',   heights: [48, 36], colors: ['bg-blue-500', 'bg-purple-400'] },
  { label: 'Hóa',  heights: [56, 28], colors: ['bg-blue-500', 'bg-purple-400'] },
  { label: 'Sinh', heights: [36, 48], colors: ['bg-blue-500', 'bg-purple-400'] },
  { label: 'Sử',   heights: [28, 20], colors: ['bg-blue-500', 'bg-purple-400'] },
  { label: 'Văn',  heights: [44, 36], colors: ['bg-blue-500', 'bg-purple-400'] },
]

function statusBadge(status: string) {
  const map: Record<string, string> = {
    'Đã công bố': 'badge badge-green',
    'Nháp':       'badge badge-yellow',
    'Lưu trữ':    'badge badge-gray',
  }
  return map[status] ?? 'badge badge-gray'
}

/* ─── Component ──────────────────────────────────── */
export default function DashboardPage() {
  const today = new Date().toLocaleDateString('vi-VN', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric',
  })

  return (
    <>
      {/* Top bar */}
      <div className="top-bar">
        <div>
          <p className="top-bar-title">Tổng quan hệ thống</p>
          <p className="top-bar-subtitle">{today}</p>
        </div>
        <div className="top-bar-avatar">TT</div>
      </div>

      {/* Scrollable content */}
      <div className="flex-1 overflow-auto p-6 flex flex-col gap-5">

        {/* Stats row */}
        <div className="flex gap-4">
          {STATS.map((s) => (
            <div key={s.label} className="stat-card">
              <div className={`stat-card-icon ${s.iconBg}`}>
                <span>{s.icon}</span>
              </div>
              <div>
                <p className="stat-card-value">{s.value}</p>
                <p className="stat-card-label">{s.label}</p>
                <p className="stat-card-trend">{s.trend}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Main row */}
        <div className="flex gap-4">

          {/* Exam table */}
          <div className="section-card flex-[3]">
            <div className="section-card-header border-b border-gray-50">
              <span className="section-card-title">Đề thi gần đây</span>
              <a href="#" className="text-xs text-blue-600 font-medium hover:underline">
                Xem tất cả →
              </a>
            </div>
            <table className="data-table">
              <thead>
                <tr>
                  <th className="table-th">Tên đề thi</th>
                  <th className="table-th">Môn</th>
                  <th className="table-th">Câu</th>
                  <th className="table-th">Trạng thái</th>
                  <th className="table-th">Ngày tạo</th>
                </tr>
              </thead>
              <tbody>
                {EXAMS.map((e) => (
                  <tr key={e.name} className="table-row">
                    <td className="table-td font-medium text-gray-800">{e.name}</td>
                    <td className="table-td">{e.subject}</td>
                    <td className="table-td">{e.count}</td>
                    <td className="table-td">
                      <span className={statusBadge(e.status)}>{e.status}</span>
                    </td>
                    <td className="table-td text-gray-400">{e.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-4 flex-[1.4] min-w-0">

            {/* Quick actions */}
            <div className="section-card p-4">
              <p className="section-card-title mb-3">Thao tác nhanh</p>
              <div className="grid grid-cols-2 gap-2">
                {QUICK_ACTIONS.map((a) => (
                  <button
                    key={a.label}
                    className={`quick-action ${a.bg} ${a.text}`}
                  >
                    <span className="text-base">{a.icon}</span>
                    <span>{a.label}</span>
                  </button>
                ))}
              </div>
            </div>

            {/* Chart */}
            <div className="section-card p-4 flex-1">
              <p className="section-card-title mb-4">Câu hỏi theo môn</p>
              <div className="flex items-end gap-2 h-24">
                {CHART_BARS.map((bar) => (
                  <div key={bar.label} className="flex flex-col items-center gap-1 flex-1">
                    <div className="flex items-end gap-0.5 w-full">
                      {bar.heights.map((h, i) => (
                        <div
                          key={i}
                          className={`flex-1 rounded-t-sm ${bar.colors[i]}`}
                          style={{ height: `${h}px` }}
                        />
                      ))}
                    </div>
                    <span className="text-[10px] text-gray-400">{bar.label}</span>
                  </div>
                ))}
              </div>
            </div>

          </div>
        </div>

      </div>
    </>
  )
}
