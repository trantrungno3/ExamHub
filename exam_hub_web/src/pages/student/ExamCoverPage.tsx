import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Checkbox } from 'antd'
import {
  BarChartOutlined,
  CalendarOutlined,
  ClockCircleOutlined,
  QuestionCircleOutlined,
  WarningOutlined,
} from '@ant-design/icons'

/* ─── Data ───────────────────────────────────────── */
const INFO_CARDS = [
  { icon: <ClockCircleOutlined />, value: '45 phút',     label: 'Thời gian'    },
  { icon: <QuestionCircleOutlined />, value: '40 câu',   label: 'Số câu hỏi'  },
  { icon: <BarChartOutlined />,    value: '10.0 điểm',   label: 'Tổng điểm'   },
  { icon: <CalendarOutlined />,    value: '23/04/2026',  label: 'Ngày thi'    },
]

const EXAM_DETAILS = [
  { label: 'Mã đề thi:',        value: 'DE_2026_MATH_001'          },
  { label: 'Môn học:',          value: 'Toán học'                  },
  { label: 'Giáo viên ra đề:',  value: 'Thầy Trần Văn Bình'        },
  { label: 'Hình thức:',        value: '40 trắc nghiệm + 0 tự luận'},
  { label: 'Điểm đạt:',         value: 'Từ 5.0 điểm trở lên'       },
]

/* ─── Component ──────────────────────────────────── */
export default function ExamCoverPage() {
  const [agreed, setAgreed] = useState(false)
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">

      {/* ── Navbar ── */}
      <nav className="student-navbar">
        <div className="flex items-center gap-2.5">
          <div className="student-logo-icon">EH</div>
          <span className="font-bold text-gray-800 text-[15px]">ExamHub</span>
        </div>
        <div className="flex items-center gap-3 text-sm">
          <span className="text-gray-700 font-medium">Nguyễn Văn An</span>
          <span className="text-gray-300">|</span>
          <span className="text-gray-500">Lớp 10A1</span>
          <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center
                          justify-center text-white text-sm font-bold ml-1">
            A
          </div>
        </div>
      </nav>

      {/* ── Blue hero ── */}
      <div className="exam-hero">
        <h1 className="exam-hero-title">Kiểm tra Toán học 45 phút</h1>
        <p className="exam-hero-sub">
          Học kỳ 1 — Năm học 2024-2025&nbsp;·&nbsp;Lớp 10A1
        </p>
        <div className="flex gap-4 max-w-2xl mx-auto">
          {INFO_CARDS.map((c) => (
            <div key={c.label} className="exam-info-card">
              <div className="exam-info-card-icon">{c.icon}</div>
              <p className="exam-info-card-value">{c.value}</p>
              <p className="exam-info-card-label">{c.label}</p>
            </div>
          ))}
        </div>
      </div>

      {/* ── Info card ── */}
      <div className="flex justify-center px-4 py-10 -mt-8">
        <div className="bg-white rounded-2xl shadow-lg w-full max-w-[560px] p-8">
          <h2 className="text-[17px] font-semibold text-gray-800 mb-5">
            Thông tin bài thi
          </h2>

          {/* Detail rows */}
          <div className="border border-gray-100 rounded-xl overflow-hidden mb-5">
            {EXAM_DETAILS.map((d) => (
              <div key={d.label} className="exam-detail-row">
                <span className="exam-detail-label">{d.label}</span>
                <span className="exam-detail-value">{d.value}</span>
              </div>
            ))}
          </div>

          {/* Warning */}
          <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3.5 mb-5">
            <div className="flex gap-3 items-start text-amber-800">
              <WarningOutlined className="text-amber-500 text-base mt-0.5 shrink-0" />
              <div className="text-[13px] leading-5">
                <p>Sau khi bắt đầu, đồng hồ đếm ngược sẽ chạy.</p>
                <p>Không thể tạm dừng hoặc thoát ra khỏi bài thi.</p>
              </div>
            </div>
          </div>

          {/* Agree checkbox */}
          <div className="mb-6">
            <Checkbox
              checked={agreed}
              onChange={(e) => setAgreed(e.target.checked)}
            >
              <span className="text-[13px] text-gray-600">
                Tôi đã đọc và hiểu các quy định của bài thi
              </span>
            </Checkbox>
          </div>

          {/* Start button */}
          <button
            onClick={() => agreed && navigate('/student/exam/take')}
            disabled={!agreed}
            className={`w-full py-3.5 rounded-xl text-white font-semibold text-[15px]
                        transition-all ${
                          agreed
                            ? 'bg-blue-600 hover:bg-blue-700 cursor-pointer shadow-md shadow-blue-200'
                            : 'bg-blue-300 cursor-not-allowed'
                        }`}
          >
            Bắt đầu làm bài →
          </button>
        </div>
      </div>
    </div>
  )
}
