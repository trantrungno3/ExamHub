import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Switch } from 'antd'
import { CloseOutlined, PlusOutlined } from '@ant-design/icons'

/* ─── Types ──────────────────────────────────────── */
type DiffDist = { easy: number; medium: number; hard: number; vhard: number }

type ExamSection = {
  id: number
  name: string
  type: string
  count: number
  pointsPerQ: number
  dist: DiffDist
}

/* ─── Distribution bar ───────────────────────────── */
const DIST_SEGMENTS = [
  { key: 'easy',   label: 'Dễ',      color: 'bg-green-400',  text: 'text-green-600'  },
  { key: 'medium', label: 'TB',       color: 'bg-yellow-400', text: 'text-yellow-600' },
  { key: 'hard',   label: 'Khó',     color: 'bg-orange-400', text: 'text-orange-600' },
  { key: 'vhard',  label: 'Rất khó', color: 'bg-red-500',    text: 'text-red-600'    },
]

function DistributionBar({ dist }: { dist: DiffDist }) {
  return (
    <div>
      <div className="flex h-3 rounded-full overflow-hidden mb-2">
        {DIST_SEGMENTS.map((s) => (
          <div
            key={s.key}
            className={s.color}
            style={{ width: `${dist[s.key as keyof DiffDist]}%` }}
          />
        ))}
      </div>
      <div className="flex gap-3 flex-wrap">
        {DIST_SEGMENTS.map((s) => (
          <span key={s.key} className={`text-[11px] font-medium flex items-center gap-1 ${s.text}`}>
            <span className={`w-2 h-2 rounded-full inline-block ${s.color}`} />
            {s.label} {dist[s.key as keyof DiffDist]}%
          </span>
        ))}
      </div>
    </div>
  )
}

/* ─── Section panel ──────────────────────────────── */
function SectionPanel({
  section,
  onRemove,
}: {
  section: ExamSection
  onRemove: () => void
}) {
  const total = (section.count * section.pointsPerQ).toFixed(2)

  return (
    <div className="exam-section-panel">
      <div className="exam-section-header">
        <span>{section.name}</span>
        <button
          onClick={onRemove}
          className="text-white/50 hover:text-white transition-colors"
        >
          <CloseOutlined />
        </button>
      </div>

      <div className="exam-section-body">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="form-label">Loại câu</label>
            <select
              className="w-full border border-gray-200 rounded-lg px-2.5 py-2 text-sm
                         text-gray-600 bg-white outline-none focus:border-blue-400 cursor-pointer"
            >
              <option>{section.type}</option>
              <option>Trắc nghiệm</option>
              <option>Đúng / Sai</option>
              <option>Tự luận</option>
              <option>Điền vào chỗ trống</option>
            </select>
          </div>
          <div>
            <label className="form-label">Số câu</label>
            <input
              type="number"
              defaultValue={section.count}
              min={1}
              className="w-full border border-gray-200 rounded-lg px-2.5 py-2 text-sm
                         text-gray-700 outline-none focus:border-blue-400"
            />
          </div>
        </div>

        <div>
          <label className="form-label">Phân bố độ khó</label>
          <DistributionBar dist={section.dist} />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="form-label">Điểm / Câu</label>
            <input
              type="number"
              step="0.25"
              defaultValue={section.pointsPerQ}
              className="w-full border border-gray-200 rounded-lg px-2.5 py-2 text-sm
                         text-gray-700 outline-none focus:border-blue-400"
            />
          </div>
          <div>
            <label className="form-label">Tổng điểm</label>
            <input
              readOnly
              value={total}
              className="w-full border border-gray-100 rounded-lg px-2.5 py-2 text-sm
                         text-gray-500 bg-gray-50 outline-none"
            />
          </div>
        </div>
      </div>
    </div>
  )
}

/* ─── Main component ─────────────────────────────── */
export default function CreateExamTemplatePage() {
  const navigate = useNavigate()
  const [shuffleQ,    setShuffleQ]    = useState(true)
  const [shuffleA,    setShuffleA]    = useState(false)
  const [preventDup,  setPreventDup]  = useState(true)

  const [sections, setSections] = useState<ExamSection[]>([
    {
      id: 1, name: 'Phần 1: Trắc nghiệm', type: 'Trắc nghiệm',
      count: 32, pointsPerQ: 0.25,
      dist: { easy: 40, medium: 30, hard: 20, vhard: 10 },
    },
    {
      id: 2, name: 'Phần 2: Tự luận', type: 'Tự luận',
      count: 8, pointsPerQ: 1,
      dist: { easy: 0, medium: 50, hard: 30, vhard: 20 },
    },
  ])

  const addSection = () => {
    const nextId = Math.max(...sections.map((s) => s.id)) + 1
    setSections([
      ...sections,
      {
        id: nextId,
        name: `Phần ${nextId}: Trắc nghiệm`,
        type: 'Trắc nghiệm',
        count: 10,
        pointsPerQ: 0.25,
        dist: { easy: 40, medium: 30, hard: 20, vhard: 10 },
      },
    ])
  }

  const removeSection = (id: number) =>
    setSections(sections.filter((s) => s.id !== id))

  return (
    <>
      {/* Top bar */}
      <div className="top-bar">
        <div>
          <p className="top-bar-title">Tạo mẫu đề thi mới</p>
          <p className="top-bar-subtitle">
            <span
              className="text-blue-500 cursor-pointer hover:underline"
              onClick={() => navigate('/app/exams')}
            >
              Mẫu đề thi
            </span>
            {' / '}Tạo mới
          </p>
        </div>
        <div className="top-bar-avatar">TT</div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto p-6">
        <div className="flex gap-5 items-start">

          {/* ── Left: form ── */}
          <div className="flex-[2] flex flex-col gap-4 min-w-0">

            {/* Exam info */}
            <div className="form-section">
              <p className="form-section-title">Thông tin mẫu đề (exam_templates)</p>
              <div className="flex flex-col gap-4">
                <div>
                  <label className="form-label">Tên đề *</label>
                  <input
                    className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                               text-gray-700 outline-none focus:border-blue-400 transition-colors
                               placeholder:text-gray-300"
                    placeholder="VD: Kiểm tra Toán HK1 Lớp 10"
                  />
                </div>

                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <label className="form-label">Lớp *</label>
                    <select
                      className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                                 text-gray-600 bg-white outline-none focus:border-blue-400 cursor-pointer"
                    >
                      <option value="">Chọn lớp</option>
                      {Array.from({ length: 12 }, (_, i) => i + 1).map((g) => (
                        <option key={g}>Lớp {g}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="form-label">Môn học</label>
                    <select
                      className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                                 text-gray-600 bg-white outline-none focus:border-blue-400 cursor-pointer"
                    >
                      <option value="">Chọn môn</option>
                      {['Toán', 'Vật lý', 'Hóa học', 'Sinh học', 'Ngữ văn', 'Tiếng Anh', 'Lịch sử', 'Địa lý'].map(
                        (s) => <option key={s}>{s}</option>,
                      )}
                    </select>
                  </div>
                  <div>
                    <label className="form-label">Thời gian (phút)</label>
                    <input
                      type="number"
                      defaultValue={45}
                      min={1}
                      className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                                 text-gray-700 outline-none focus:border-blue-400"
                    />
                  </div>
                </div>

                <div>
                  <label className="form-label">Hướng dẫn làm bài</label>
                  <textarea
                    className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                               text-gray-600 outline-none resize-none h-20
                               placeholder:text-gray-300 focus:border-blue-400 transition-colors"
                    placeholder="Học sinh đọc kỹ đề trước khi làm bài. Không sử dụng tài liệu..."
                  />
                </div>
              </div>
            </div>

            {/* Generation settings */}
            <div className="form-section">
              <p className="form-section-title">Cấu hình sinh đề</p>
              {[
                { label: 'Đầu trộn câu hỏi',   sub: 'shuffle_questions',  value: shuffleQ,   set: setShuffleQ   },
                { label: 'Đầu trộn đáp án',     sub: 'shuffle_answers',    value: shuffleA,   set: setShuffleA   },
                { label: 'Chống trùng câu hỏi', sub: 'prevent_duplicate',  value: preventDup, set: setPreventDup },
              ].map((t, i, arr) => (
                <div
                  key={t.sub}
                  className={`toggle-row ${i === arr.length - 1 ? '!border-b-0' : ''}`}
                >
                  <div>
                    <p className="toggle-label">{t.label}</p>
                    <p className="toggle-sublabel">{t.sub}</p>
                  </div>
                  <Switch checked={t.value} onChange={t.set} />
                </div>
              ))}
            </div>
          </div>

          {/* ── Right: sections panel ── */}
          <div className="flex-[1.3] flex flex-col gap-3 min-w-0">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-[13px] font-semibold text-gray-700">Câu hình phần thi</p>
                <p className="text-[11px] text-gray-400 mt-0.5">exam_template_sections</p>
              </div>
              <button onClick={addSection} className="btn-primary text-xs">
                <PlusOutlined /> Thêm phần
              </button>
            </div>

            <div className="flex flex-col gap-3">
              {sections.map((sec) => (
                <SectionPanel
                  key={sec.id}
                  section={sec}
                  onRemove={() => removeSection(sec.id)}
                />
              ))}
            </div>

            {sections.length === 0 && (
              <div className="text-center py-10 text-gray-400 text-sm border-2 border-dashed
                              border-gray-200 rounded-xl">
                Chưa có phần thi nào. Nhấn "Thêm phần" để bắt đầu.
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Action bar */}
      <div className="action-bar">
        <button
          onClick={() => navigate('/app/exams')}
          className="px-5 py-2 border border-gray-300 text-sm font-medium text-gray-700
                     rounded-lg hover:bg-gray-50 transition-colors"
        >
          Hủy bỏ
        </button>
        <button className="btn-outline-blue">Lưu mẫu đề thi</button>
        <button className="btn-primary">Lọc &amp; Đề thi ngay</button>
      </div>
    </>
  )
}
