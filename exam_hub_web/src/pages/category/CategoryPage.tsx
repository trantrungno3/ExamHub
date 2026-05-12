import { useState } from 'react'
import { Switch } from 'antd'
import { SearchOutlined, PlusOutlined, WarningOutlined } from '@ant-design/icons'

/* ─── Types ──────────────────────────────────────── */
type Tab = 'grade' | 'subject' | 'topic' | 'difficulty' | 'question-type' | 'cognitive'

const TABS: { key: Tab; label: string }[] = [
  { key: 'grade',         label: 'Cấp lớp' },
  { key: 'subject',       label: 'Môn học' },
  { key: 'topic',         label: 'Chủ đề' },
  { key: 'difficulty',    label: 'Độ khó' },
  { key: 'question-type', label: 'Loại câu hỏi' },
  { key: 'cognitive',     label: 'Cấp độ nhận thức' },
]

/* ─── Grade data ─────────────────────────────────── */
const GRADES = [
  { id: 1,  name: 'Lớp 1',  grade: 1,  desc: 'Cấp tiểu học' },
  { id: 2,  name: 'Lớp 2',  grade: 2,  desc: 'Cấp tiểu học' },
  { id: 3,  name: 'Lớp 3',  grade: 3,  desc: 'Cấp tiểu học' },
  { id: 4,  name: 'Lớp 4',  grade: 4,  desc: 'Cấp tiểu học' },
  { id: 5,  name: 'Lớp 5',  grade: 5,  desc: 'Cấp tiểu học' },
  { id: 6,  name: 'Lớp 6',  grade: 6,  desc: 'Cấp THCS' },
  { id: 7,  name: 'Lớp 7',  grade: 7,  desc: 'Cấp THCS' },
  { id: 8,  name: 'Lớp 8',  grade: 8,  desc: 'Cấp THCS' },
  { id: 9,  name: 'Lớp 9',  grade: 9,  desc: 'Cấp THCS' },
  { id: 10, name: 'Lớp 10', grade: 10, desc: 'Cấp THPT' },
  { id: 11, name: 'Lớp 11', grade: 11, desc: 'Cấp THPT' },
  { id: 12, name: 'Lớp 12', grade: 12, desc: 'Cấp THPT' },
]

/* ─── Difficulty data ────────────────────────────── */
type DiffRow = {
  id: number
  code: string
  name: string
  codeBadge: string
  nameBadge: string
  weight: string
  priority: string
}

const DIFFICULTIES: DiffRow[] = [
  { id: 1, code: 'easy',      name: 'Dễ',       codeBadge: 'bg-green-100 text-green-700',  nameBadge: 'bg-green-100 text-green-700',  weight: '×1.00', priority: 'Ưu tiên 1' },
  { id: 2, code: 'medium',    name: 'Trung bình',codeBadge: 'bg-yellow-100 text-yellow-700',nameBadge: 'bg-yellow-100 text-yellow-700',weight: '×1.50', priority: 'Ưu tiên 2' },
  { id: 3, code: 'hard',      name: 'Khó',       codeBadge: 'bg-red-100 text-red-600',     nameBadge: 'bg-red-100 text-red-600',     weight: '×2.00', priority: 'Ưu tiên 3' },
  { id: 4, code: 'very_hard', name: 'Rất khó',   codeBadge: 'bg-purple-100 text-purple-700',nameBadge:'bg-purple-100 text-purple-700',weight: '×2.50', priority: 'Ưu tiên 4' },
]

/* ─── Bloom's taxonomy data ──────────────────────── */
type BloomLevel = {
  id: number
  name: string
  eng: string
  code: string
  color: string
  tagColor: string
  keywords: string
  pyramidWidth: string
}

const BLOOM_LEVELS: BloomLevel[] = [
  { id: 1, name: 'Nhớ',     eng: 'Remember', code: 'remember', color: 'bg-sky-200',    tagColor: 'bg-sky-100 text-sky-700',    keywords: 'Liệt kê · Xác định · Nhận ra · Gọi tên · Định nghĩa', pyramidWidth: 'w-full'    },
  { id: 2, name: 'Hiểu',    eng: 'Understand',code: 'understand',color: 'bg-blue-300', tagColor: 'bg-blue-100 text-blue-700', keywords: 'Phân tả · Giải thích · Phân loại · So sánh · Tóm tắt', pyramidWidth: 'w-[87%]'   },
  { id: 3, name: 'Vận dụng',eng: 'Apply',    code: 'apply',    color: 'bg-amber-300',  tagColor: 'bg-amber-100 text-amber-700',keywords: 'Tính toán · Giải · Áp dụng · Thực hiện · Xây dựng',    pyramidWidth: 'w-[74%]'   },
  { id: 4, name: 'Phân tích',eng: 'Analyze', code: 'analyze',  color: 'bg-blue-500',   tagColor: 'bg-blue-100 text-blue-700', keywords: 'Phân tích · Phân biệt · Kiểm tra · Suy luận · So sánh', pyramidWidth: 'w-[60%]'   },
  { id: 5, name: 'Đánh giá',eng: 'Evaluate', code: 'evaluate', color: 'bg-rose-400',   tagColor: 'bg-rose-100 text-rose-700', keywords: 'Đánh giá · Phê bình · Lập luận · Chứng minh · Ưu tiên',pyramidWidth: 'w-[47%]'   },
  { id: 6, name: 'Sáng tạo',eng: 'Create',   code: 'create',   color: 'bg-red-600',    tagColor: 'bg-red-100 text-red-700',   keywords: 'Thiết kế · Xây dựng · Lập kế hoạch · Sáng tác · Đề xuất',pyramidWidth: 'w-[33%]'},
]

/* ─── Sub-views ──────────────────────────────────── */
function GradeTab() {
  const [search, setSearch] = useState('')
  const filtered = GRADES.filter((g) =>
    g.name.toLowerCase().includes(search.toLowerCase()),
  )

  return (
    <div className="flex flex-col gap-4 p-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 border border-gray-200 rounded-lg px-3 py-2 bg-white w-56">
          <SearchOutlined className="text-gray-400" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Tìm cấp lớp..."
            className="outline-none text-sm text-gray-700 bg-transparent w-full"
          />
        </div>
        <button className="btn-primary">
          <PlusOutlined />
          Thêm lớp
        </button>
      </div>

      <div className="section-card">
        <table className="data-table">
          <thead>
            <tr>
              <th className="table-th">ID</th>
              <th className="table-th">Tên cấp lớp</th>
              <th className="table-th">grade_number</th>
              <th className="table-th">Mô tả</th>
              <th className="table-th">Trạng thái</th>
              <th className="table-th">Ngày tạo</th>
              <th className="table-th">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map((g) => (
              <tr key={g.id} className="table-row">
                <td className="table-td text-gray-400">{g.id}</td>
                <td className="table-td font-medium">
                  <span className="inline-flex items-center gap-1.5">
                    <span className="w-2 h-2 rounded-full bg-green-500 inline-block" />
                    {g.name}
                  </span>
                </td>
                <td className="table-td">{g.grade}</td>
                <td className="table-td text-gray-500">{g.desc}</td>
                <td className="table-td">
                  <span className="badge badge-green">Hoạt động</span>
                </td>
                <td className="table-td text-gray-400">01/01/2024</td>
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
        <p className="px-4 py-3 text-[12px] text-gray-400">
          Hiển thị 1–{filtered.length} trong tổng số {GRADES.length} cấp lớp
        </p>
      </div>
    </div>
  )
}

function DifficultyTab() {
  return (
    <div className="flex flex-col gap-4 p-6">
      {/* Warning banner */}
      <div className="flex items-start justify-between gap-4 bg-amber-50 border border-amber-200 rounded-xl px-5 py-3.5">
        <p className="text-[13px] text-amber-800 font-medium flex items-center gap-2">
          <WarningOutlined className="text-amber-500" />
          Dữ liệu này ảnh hưởng đến toàn bộ thuật toán sinh đề. Chỉnh sửa cẩn thận.
        </p>
        <button className="btn-primary shrink-0">
          <PlusOutlined />
          Thêm độ khó
        </button>
      </div>

      <div className="section-card">
        <table className="data-table">
          <thead>
            <tr>
              <th className="table-th">ID</th>
              <th className="table-th">Mã (code)</th>
              <th className="table-th">Tên (name)</th>
              <th className="table-th">Hệ số (score_weight)</th>
              <th className="table-th">Thứ tự (sort_order)</th>
              <th className="table-th">Trạng thái</th>
              <th className="table-th">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            {DIFFICULTIES.map((d) => (
              <tr key={d.id} className="table-row">
                <td className="table-td text-gray-400">{d.id}</td>
                <td className="table-td">
                  <span className={`badge ${d.codeBadge}`}>{d.code}</span>
                </td>
                <td className="table-td">
                  <span className={`badge ${d.nameBadge}`}>{d.name}</span>
                </td>
                <td className="table-td font-bold text-gray-800">{d.weight}</td>
                <td className="table-td text-gray-500">{d.priority}</td>
                <td className="table-td">
                  <span className="badge badge-green">Hoạt động</span>
                </td>
                <td className="table-td">
                  <button className="btn-edit">Sửa</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        <p className="px-4 py-3 text-[12px] text-gray-400">
          4 mức độ khó — Seed data mặc định hệ thống
        </p>
      </div>
    </div>
  )
}

function CognitiveTab() {
  return (
    <div className="flex flex-col gap-4 p-6">
      {/* Info banner */}
      <div className="bg-blue-50 border border-blue-100 rounded-xl px-5 py-3">
        <p className="text-[13px] text-blue-700">
          📖 Anderson &amp; Krathwohl (2001) — 6 cấp độ tư duy từ thấp → cao.
          Seed data mặc định, chỉ Admin chỉnh sửa.
        </p>
      </div>

      <div className="flex gap-4">
        {/* Pyramid */}
        <div className="section-card flex-1 p-5">
          <p className="text-sm font-semibold text-gray-700 mb-5">Tháp nhận thức Bloom</p>
          <div className="flex flex-col items-center gap-1">
            {[...BLOOM_LEVELS].reverse().map((level) => (
              <div
                key={level.id}
                className={`${level.pyramidWidth} ${level.color} rounded-md px-3 py-2
                            flex items-center justify-between`}
              >
                <span className="text-white text-[12px] font-semibold">{level.name}</span>
                <span className="text-white/75 text-[11px]">{level.eng}</span>
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

        {/* Detail cards */}
        <div className="flex flex-col gap-3 flex-1">
          <p className="text-sm font-semibold text-gray-700">Chi tiết từng cấp độ</p>
          {BLOOM_LEVELS.map((level) => (
            <div key={level.id} className="section-card p-4 flex items-start gap-3">
              <div
                className={`w-6 h-6 rounded-full flex items-center justify-center
                             text-white text-[11px] font-bold shrink-0 ${level.color}`}
              >
                {level.id}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <span className="text-[13px] font-semibold text-gray-800">{level.name}</span>
                  <span className="text-[11px] text-gray-400">{level.eng}</span>
                  <span className={`badge text-[10px] ${level.tagColor}`}>{level.code}</span>
                </div>
                <p className="text-[11px] text-gray-500 truncate">{level.keywords}</p>
              </div>
              <Switch defaultChecked size="small" className="shrink-0 mt-0.5" />
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

function PlaceholderTab({ label }: { label: string }) {
  return (
    <div className="flex items-center justify-center py-20 text-gray-400 text-sm">
      {label} — chưa có dữ liệu
    </div>
  )
}

/* ─── Main component ─────────────────────────────── */
export default function CategoryPage() {
  const [activeTab, setActiveTab] = useState<Tab>('grade')

  const tabBreadcrumb: Record<Tab, string> = {
    grade:          'Cấp lớp',
    subject:        'Môn học',
    topic:          'Chủ đề',
    difficulty:     'Độ khó',
    'question-type':'Loại câu hỏi',
    cognitive:      'Cấp độ nhận thức',
  }

  return (
    <>
      {/* Top bar */}
      <div className="top-bar">
        <div>
          <p className="top-bar-title">Danh mục cấu hình</p>
          <p className="top-bar-subtitle">
            Quản lý cấp lớp · môn học · chủ đề · độ khó · loại câu hỏi · cấp độ nhận thức
          </p>
        </div>
        <div className="top-bar-avatar">TT</div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto flex flex-col">
        {/* Tabs */}
        <div className="bg-white px-6 pt-4">
          <nav className="cat-tabs">
            {TABS.map((t) => (
              <button
                key={t.key}
                onClick={() => setActiveTab(t.key)}
                className={`cat-tab ${activeTab === t.key ? 'cat-tab--active' : ''}`}
              >
                {t.label}
              </button>
            ))}
          </nav>
        </div>

        {/* Breadcrumb */}
        <div className="px-6 py-2.5 text-[12px] text-gray-400 bg-white border-b border-gray-100">
          Danh mục{' '}
          <span className="mx-1">/</span>
          <span className="text-gray-600">{tabBreadcrumb[activeTab]}</span>
        </div>

        {/* Tab content */}
        {activeTab === 'grade'         && <GradeTab />}
        {activeTab === 'difficulty'    && <DifficultyTab />}
        {activeTab === 'cognitive'     && <CognitiveTab />}
        {activeTab === 'subject'       && <PlaceholderTab label="Môn học" />}
        {activeTab === 'topic'         && <PlaceholderTab label="Chủ đề" />}
        {activeTab === 'question-type' && <PlaceholderTab label="Loại câu hỏi" />}
      </div>
    </>
  )
}
