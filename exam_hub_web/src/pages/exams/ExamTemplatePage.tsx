import { useNavigate } from 'react-router-dom'
import { PlusOutlined, SearchOutlined } from '@ant-design/icons'
import { EXAM_TEMPLATE_STATS as STATS, MOCK_TEMPLATES as TEMPLATES } from '../../__mocks__/templates'

const DIST_COLORS = ['bg-green-400', 'bg-yellow-400', 'bg-orange-400', 'bg-red-500']

const STATUS_BADGE: Record<string, string> = {
  'Hoàn chỉnh': 'badge badge-green',
  'Nháp':        'badge badge-yellow',
}

/* ─── Component ──────────────────────────────────── */
export default function ExamTemplatePage() {
  const navigate = useNavigate()

  return (
    <>
      <div className="top-bar">
        <div>
          <p className="top-bar-title">Mẫu đề thi</p>
          <p className="top-bar-subtitle">Quản lý mẫu cấu trúc và sinh đề tự động</p>
        </div>
        <div className="top-bar-avatar">TT</div>
      </div>

      <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
        {/* Stats */}
        <div className="flex gap-4">
          {STATS.map((s) => (
            <div key={s.label} className="stat-card">
              <div className={`stat-card-icon ${s.iconBg}`}>{s.icon}</div>
              <div>
                <p className="stat-card-value">{s.value}</p>
                <p className="stat-card-label">{s.label}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Search + Add */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 border border-gray-200 rounded-lg
                          px-3 py-2 bg-white w-64">
            <SearchOutlined className="text-gray-400 text-sm" />
            <input
              className="outline-none text-sm text-gray-700 bg-transparent flex-1"
              placeholder="Tìm mẫu đề thi..."
            />
          </div>
          <button
            onClick={() => navigate('/app/exams/create')}
            className="btn-primary ml-auto"
          >
            <PlusOutlined /> Tạo mẫu đề thi
          </button>
        </div>

        {/* Table */}
        <div className="section-card">
          <table className="data-table">
            <thead>
              <tr>
                <th className="table-th">Tên mẫu đề thi</th>
                <th className="table-th">Lớp</th>
                <th className="table-th">Môn</th>
                <th className="table-th">Câu</th>
                <th className="table-th">Điểm</th>
                <th className="table-th">Thời gian</th>
                <th className="table-th">Phân bố độ khó</th>
                <th className="table-th">Trạng thái</th>
                <th className="table-th">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {TEMPLATES.map((t, i) => (
                <tr key={i} className="table-row">
                  <td className="table-td font-medium text-gray-800">{t.name}</td>
                  <td className="table-td">{t.grade}</td>
                  <td className="table-td">{t.subject}</td>
                  <td className="table-td">{t.count}</td>
                  <td className="table-td">{t.score}</td>
                  <td className="table-td text-gray-500">{t.time}</td>
                  <td className="table-td">
                    <div className="flex flex-col gap-1">
                      <div className="flex h-2 rounded-full overflow-hidden w-24">
                        {t.dist.map((pct, di) => (
                          <div
                            key={di}
                            className={DIST_COLORS[di]}
                            style={{ width: `${pct}%` }}
                          />
                        ))}
                      </div>
                      <span className="text-[10px] text-gray-400">
                        {t.dist[0]}% / {t.dist[1]}% / {t.dist[2]}% / {t.dist[3]}%
                      </span>
                    </div>
                  </td>
                  <td className="table-td">
                    <span className={STATUS_BADGE[t.status] ?? 'badge badge-gray'}>
                      {t.status}
                    </span>
                  </td>
                  <td className="table-td">
                    <div className="flex gap-1.5">
                      <button className="btn-view">Xem</button>
                      <button className="btn-edit">Sửa</button>
                      <button className="btn-delete">Xóa</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Pagination */}
          <div className="px-4 py-3 flex items-center justify-between border-t border-gray-50">
            <p className="text-[12px] text-gray-400">
              Hiển thị 1–{TEMPLATES.length} trong tổng số 28 mẫu đề
            </p>
            <div className="flex gap-1">
              {[1, 2, 3].map((p) => (
                <button
                  key={p}
                  className={`w-8 h-8 rounded-lg text-xs font-medium transition-colors ${
                    p === 1 ? 'bg-blue-600 text-white' : 'text-gray-500 hover:bg-gray-100'
                  }`}
                >
                  {p}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>
    </>
  )
}
