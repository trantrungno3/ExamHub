import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Switch } from 'antd'
import { PaperClipOutlined, PictureOutlined } from '@ant-design/icons'

type AnswerKey = 'A' | 'B' | 'C' | 'D'

const TOOLBAR_BTNS = ['B', 'I', 'U', '|', 'x²', 'x₂', '|', '√', 'π', '≤', '≥', '∞']

const CLASSIFICATION_FIELDS = [
  { label: 'Hình thức',  placeholder: 'Chọn hình thức' },
  { label: 'Môn học',   placeholder: 'Toán' },
  { label: 'Lớp',       placeholder: 'Chọn lớp' },
  { label: 'Chủ đề',    placeholder: 'Trắc nghiệm 1 đáp án' },
  { label: 'Độ khó',    placeholder: 'Chọn độ khó' },
  { label: 'Từ khoá',   placeholder: 'Chọn từ khoá' },
]

export default function AddQuestionPage() {
  const navigate = useNavigate()
  const [correctAnswer, setCorrectAnswer] = useState<AnswerKey>('A')
  const [answers, setAnswers] = useState<Record<AnswerKey, string>>({
    A: '5 cm',
    B: '3 cm',
    C: '√34 cm',
    D: '7 cm',
  })
  const [isAiPrinted, setIsAiPrinted] = useState(true)
  const [isVerified, setIsVerified] = useState(false)

  return (
    <>
      {/* Top bar */}
      <div className="top-bar">
        <div>
          <p className="top-bar-title">Thêm câu hỏi mới</p>
          <p className="top-bar-subtitle">
            <span
              className="text-blue-500 cursor-pointer hover:underline"
              onClick={() => navigate('/app/questions')}
            >
              Câu hỏi
            </span>
            {' / '}Thêm mới
          </p>
        </div>
        <div className="top-bar-avatar">TT</div>
      </div>

      {/* Scrollable content */}
      <div className="flex-1 overflow-auto p-6">
        <div className="flex gap-5 items-start">

          {/* ── Left: main content ── */}
          <div className="flex-1 flex flex-col gap-4 min-w-0">

            {/* Question content */}
            <div className="form-section">
              <p className="form-section-title">Nội dung câu hỏi</p>
              <div className="border border-gray-200 rounded-lg overflow-hidden focus-within:border-blue-400 transition-colors">
                {/* Toolbar */}
                <div className="flex items-center gap-0.5 px-2 py-1.5 border-b border-gray-100 bg-gray-50">
                  {TOOLBAR_BTNS.map((t, i) =>
                    t === '|' ? (
                      <span key={i} className="text-gray-300 mx-1 select-none">│</span>
                    ) : (
                      <button
                        key={i}
                        className="w-7 h-7 rounded flex items-center justify-center
                                   text-xs font-bold text-gray-500 hover:bg-gray-200 transition-colors"
                      >
                        {t}
                      </button>
                    )
                  )}
                </div>
                {/* Editor area */}
                <div className="flex">
                  <div className="w-8 shrink-0 pt-3 pb-3 text-right pr-2 text-[11px]
                                  text-gray-300 border-r border-gray-100 bg-gray-50/50
                                  select-none leading-6">
                    {[1, 2, 3].map((n) => <div key={n}>{n}</div>)}
                  </div>
                  <textarea
                    className="flex-1 px-3 py-3 text-sm text-gray-800 outline-none
                               resize-none h-28 leading-6"
                    defaultValue="Trong tam giác ABC vuông tại B, có AB = 4 cm và BC = 3 cm. Tính độ dài AC?"
                  />
                </div>
              </div>
            </div>

            {/* Explanation */}
            <div className="form-section">
              <p className="form-section-title">Giải thích đáp án (explanation)</p>
              <textarea
                className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                           text-gray-600 outline-none resize-none h-20
                           placeholder:text-gray-300 focus:border-blue-400 transition-colors"
                placeholder="Áp dụng định lý Pythagoras: AC² = AB² + BC² = 16 + 9 = 25 → AC = 5 cm"
              />
            </div>

            {/* Attachments */}
            <div className="form-section">
              <p className="form-section-title">Tệp đính kèm (image, gif, audio, pdf)</p>
              <div className="flex gap-3">
                <button className="flex items-center gap-2 px-4 py-2 border border-dashed
                                    border-gray-300 rounded-lg text-sm text-gray-500
                                    hover:border-blue-400 hover:text-blue-600 hover:bg-blue-50
                                    transition-colors">
                  <PictureOutlined /> Thêm ảnh
                </button>
                <button className="flex items-center gap-2 px-4 py-2 border border-dashed
                                    border-gray-300 rounded-lg text-sm text-gray-500
                                    hover:border-blue-400 hover:text-blue-600 hover:bg-blue-50
                                    transition-colors">
                  <PaperClipOutlined /> Đính kèm file
                </button>
              </div>
            </div>

            {/* Tags & Source */}
            <div className="form-section">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="form-label">Tags (tag[])</label>
                  <input
                    className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                               text-gray-700 outline-none focus:border-blue-400 transition-colors
                               placeholder:text-gray-300"
                    placeholder="tam giác, pythagoras, hình học..."
                  />
                </div>
                <div>
                  <label className="form-label">Nguồn (source)</label>
                  <input
                    className="w-full border border-gray-200 rounded-lg px-3 py-2.5 text-sm
                               text-gray-700 outline-none focus:border-blue-400 transition-colors
                               placeholder:text-gray-300"
                    placeholder="SGK Toán 10, trang 45..."
                  />
                </div>
              </div>
            </div>

            {/* Answers */}
            <div className="form-section">
              <p className="form-section-title">
                Đáp án (question_answers → lk_content · content · sort_order)
              </p>

              {(['A', 'B', 'C', 'D'] as AnswerKey[]).map((key) => (
                <div
                  key={key}
                  onClick={() => setCorrectAnswer(key)}
                  className={`answer-option ${correctAnswer === key ? 'answer-option--correct' : ''}`}
                >
                  {/* Radio circle */}
                  <div
                    className={`w-5 h-5 rounded-full border-2 flex items-center justify-center
                                 shrink-0 transition-colors ${
                                   correctAnswer === key
                                     ? 'border-green-500 bg-green-500'
                                     : 'border-gray-300'
                                 }`}
                  >
                    {correctAnswer === key && (
                      <div className="w-2 h-2 rounded-full bg-white" />
                    )}
                  </div>
                  <span className="text-xs font-bold text-gray-400 w-5 shrink-0">{key}.</span>
                  <input
                    value={answers[key]}
                    onChange={(e) =>
                      setAnswers((prev) => ({ ...prev, [key]: e.target.value }))
                    }
                    onClick={(e) => e.stopPropagation()}
                    className={`flex-1 text-sm outline-none bg-transparent ${
                      correctAnswer === key ? 'text-green-700 font-medium' : 'text-gray-700'
                    }`}
                    placeholder={`Nhập đáp án ${key}...`}
                  />
                  {correctAnswer === key && (
                    <span className="text-[10px] font-semibold text-green-600 bg-green-100
                                     px-2 py-0.5 rounded-full shrink-0">
                      Đáp án đúng
                    </span>
                  )}
                </div>
              ))}

              {/* No answer option */}
              <div className="flex items-center gap-3 px-3 py-2.5 rounded-lg cursor-default">
                <div className="w-5 h-5 rounded-full border-2 border-gray-200 shrink-0" />
                <span className="text-sm text-gray-400 italic">Không xác định</span>
              </div>
            </div>
          </div>

          {/* ── Right: classification sidebar ── */}
          <div className="w-72 flex flex-col gap-4 shrink-0">

            {/* Classification */}
            <div className="form-section">
              <p className="form-section-title">Phân loại câu hỏi</p>
              <div className="flex flex-col gap-3">
                {CLASSIFICATION_FIELDS.map((f) => (
                  <div key={f.label}>
                    <label className="form-label">{f.label}</label>
                    <select
                      className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm
                                 text-gray-600 bg-white outline-none focus:border-blue-400
                                 cursor-pointer transition-colors"
                    >
                      <option>{f.placeholder}</option>
                    </select>
                  </div>
                ))}
              </div>
            </div>

            {/* Settings */}
            <div className="form-section">
              <p className="form-section-title">Cài đặt</p>
              <div className="toggle-row">
                <div>
                  <p className="toggle-label">is_ai_printed</p>
                  <p className="toggle-sublabel">Cho phép AI sử dụng câu hỏi</p>
                </div>
                <Switch checked={isAiPrinted} onChange={setIsAiPrinted} size="small" />
              </div>
              <div className="toggle-row !border-b-0">
                <div>
                  <p className="toggle-label">Độ xác nhận</p>
                  <p className="toggle-sublabel">is_verified</p>
                </div>
                <Switch checked={isVerified} onChange={setIsVerified} size="small" />
              </div>
            </div>

          </div>
        </div>
      </div>

      {/* Action bar */}
      <div className="action-bar">
        <button
          onClick={() => navigate('/app/questions')}
          className="px-5 py-2 border border-gray-300 text-sm font-medium text-gray-700
                     rounded-lg hover:bg-gray-50 transition-colors"
        >
          Hủy bỏ
        </button>
        <button className="btn-primary">Lưu câu hỏi</button>
      </div>
    </>
  )
}
