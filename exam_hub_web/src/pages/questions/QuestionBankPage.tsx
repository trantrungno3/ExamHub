import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { PlusOutlined, UploadOutlined, SearchOutlined } from '@ant-design/icons'
import { QUESTION_BANK_STATS as STATS, MOCK_QUESTIONS as QUESTIONS } from '../../__mocks__/questions'

type FilterKey = 'all' | 'mc' | 'tf' | 'essay' | 'fill' | 'thptqg' | 'thpt'

const TYPE_FILTERS: {
  key: FilterKey
  label: string
  off: string
  on: string
}[] = [
  { key: 'all',    label: 'Tất cả',             off: 'bg-gray-100 text-gray-600 border-gray-200',       on: 'bg-gray-700  text-white border-gray-700'  },
  { key: 'mc',     label: 'Trắc nghiệm',        off: 'bg-blue-50  text-blue-600  border-blue-200',      on: 'bg-blue-600  text-white border-blue-600'  },
  { key: 'tf',     label: 'Đúng / Sai',         off: 'bg-green-50 text-green-600 border-green-200',     on: 'bg-green-600 text-white border-green-600' },
  { key: 'essay',  label: 'Tự luận',             off: 'bg-purple-50 text-purple-600 border-purple-200', on: 'bg-purple-600 text-white border-purple-600'},
  { key: 'fill',   label: 'Điền vào chỗ trống', off: 'bg-orange-50 text-orange-600 border-orange-200', on: 'bg-orange-500 text-white border-orange-500'},
  { key: 'thptqg', label: 'THPTQG',             off: 'bg-teal-50  text-teal-600  border-teal-200',      on: 'bg-teal-600  text-white border-teal-600'  },
  { key: 'thpt',   label: 'THPT',               off: 'bg-pink-50  text-pink-600  border-pink-200',      on: 'bg-pink-600  text-white border-pink-600'  },
]


const TYPE_BADGE: Record<string, string> = {
  'Trắc nghiệm': 'badge badge-blue',
  'Đúng/Sai':   'badge badge-green',
  'Điền vào':   'badge badge-orange',
  'Tự luận':    'badge badge-purple',
}

const DIFF_BADGE: Record<string, string> = {
  'Dễ':         'badge badge-green',
  'Trung bình': 'badge badge-yellow',
  'Khó':        'badge badge-orange',
  'Rất khó':    'badge badge-red',
}

const STATUS_BADGE: Record<string, string> = {
  'Đã duyệt':  'badge badge-green',
  'Chờ duyệt': 'badge badge-yellow',
  'Nháp':       'badge badge-gray',
}

/* ─── Component ──────────────────────────────────── */
export default function QuestionBankPage() {
  const [activeType, setActiveType] = useState<FilterKey>('all')
  const navigate = useNavigate()

  return (
    <>
      <div className="top-bar">
        <div>
          <p className="top-bar-title">Ngân hàng câu hỏi</p>
          <p className="top-bar-subtitle">Quản lý, tìm kiếm và phân loại toàn bộ câu hỏi</p>
        </div>
        <div className="top-bar-avatar">TT</div>
      </div>

      <div className="flex-1 overflow-auto p-6 flex flex-col gap-4">
        {/* Stats row */}
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

        {/* Filters */}
        <div className="flex flex-col gap-2.5">
          {/* Type chips */}
          <div className="flex items-center gap-2 flex-wrap">
            {TYPE_FILTERS.map((f) => (
              <button
                key={f.key}
                onClick={() => setActiveType(f.key)}
                className={`filter-chip ${activeType === f.key ? f.on : f.off}`}
              >
                {f.label}
              </button>
            ))}
          </div>

          {/* Dropdown filters + action buttons */}
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-2 border border-gray-200 rounded-lg
                            px-3 py-2 bg-white w-52">
              <SearchOutlined className="text-gray-400 text-sm" />
              <input
                className="outline-none text-sm text-gray-700 bg-transparent w-full"
                placeholder="Tìm câu hỏi..."
              />
            </div>

            {['Môn học', 'Lớp', 'Độ khó', 'Chủ đề', 'Trạng thái'].map((f) => (
              <select
                key={f}
                className="border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-500
                           bg-white outline-none hover:border-blue-400 cursor-pointer transition-colors"
              >
                <option>{f}</option>
              </select>
            ))}

            <button className="text-sm text-blue-600 px-2 hover:underline whitespace-nowrap">
              + Bộ lọc khác
            </button>

            <div className="flex gap-2 ml-auto shrink-0">
              <button className="flex items-center gap-1.5 px-4 py-2 border border-gray-300 text-sm
                                  font-medium text-gray-700 rounded-lg hover:bg-gray-50 transition-colors">
                <UploadOutlined /> Nhập Excel
              </button>
              <button
                onClick={() => navigate('/app/questions/add')}
                className="btn-primary"
              >
                <PlusOutlined /> Thêm câu hỏi
              </button>
            </div>
          </div>
        </div>

        {/* Table */}
        <div className="section-card">
          <table className="data-table">
            <thead>
              <tr>
                <th className="table-th">Nội dung câu hỏi</th>
                <th className="table-th">Môn</th>
                <th className="table-th">Lớp</th>
                <th className="table-th">Loại</th>
                <th className="table-th">Độ khó</th>
                <th className="table-th">Trạng thái</th>
                <th className="table-th">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              {QUESTIONS.map((q) => (
                <tr key={q.id} className="table-row">
                  <td className="table-td max-w-xs">
                    <p className="truncate text-gray-800 font-medium">{q.content}</p>
                  </td>
                  <td className="table-td">{q.subject}</td>
                  <td className="table-td">{q.grade}</td>
                  <td className="table-td">
                    <span className={TYPE_BADGE[q.type] ?? 'badge badge-gray'}>{q.type}</span>
                  </td>
                  <td className="table-td">
                    <span className={DIFF_BADGE[q.difficulty] ?? 'badge badge-gray'}>{q.difficulty}</span>
                  </td>
                  <td className="table-td">
                    <span className={STATUS_BADGE[q.status] ?? 'badge badge-gray'}>{q.status}</span>
                  </td>
                  <td className="table-td">
                    <div className="flex gap-2">
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
              Hiển thị 1–{QUESTIONS.length} trong tổng số 2,841 câu hỏi
            </p>
            <div className="flex gap-1">
              {[1, 2, 3, '…'].map((p, i) => (
                <button
                  key={i}
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
