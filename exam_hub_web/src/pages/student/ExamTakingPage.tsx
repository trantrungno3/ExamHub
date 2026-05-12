import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { FlagOutlined, CheckOutlined } from '@ant-design/icons'

/* ─── Types ──────────────────────────────────────── */
type AnswerKey = 'A' | 'B' | 'C' | 'D'

type Question = {
  id: number
  text: string
  formula?: string
  options: Record<AnswerKey, string>
}

/* ─── Questions data ─────────────────────────────── */
const QUESTIONS: Question[] = Array.from({ length: 40 }, (_, i) => ({
  id: i + 1,
  text: `Câu hỏi số ${i + 1}: Giải phương trình và tìm nghiệm thực của: (${i + 1})x² - ${i + 2}x + ${i} = 0`,
  options: {
    A: `x = ${((i + 2) / 2).toFixed(1)}`,
    B: `x = ${(i + 1)}`,
    C: `x = -${(i + 1)}`,
    D: 'Phương trình vô nghiệm',
  },
}))

// Override Q23 with the exact content from design
QUESTIONS[22] = {
  id: 23,
  text: 'Trong mặt phẳng tọa độ Oxy, cho tam giác ABC có A(1; 2), B(4; 6), C(7; 2). Tìm tọa độ trọng tâm G của tam giác:',
  formula: 'G = ( (xₐ + x_B + x_C) / 3  ,  (yₐ + y_B + y_C) / 3 )',
  options: {
    A: 'G(3; 10/3) — Trọng tâm nằm trong tam giác',
    B: 'G(4; 10/3) — Trọng tâm là điểm cân bằng',
    C: 'G(3; 3) — Điểm của đường thẳng y = 1/2',
    D: 'G(4; 3) — Trung điểm của đường trung tuyến',
  },
}

/* ─── Initial state helpers ──────────────────────── */
const makeInitialAnswers = (): Record<number, AnswerKey> => {
  const result: Record<number, AnswerKey> = {}
  const keys: AnswerKey[] = ['A', 'B', 'C', 'D']
  for (let i = 0; i < 22; i++) result[i] = keys[i % 4]
  return result
}

const INITIAL_FLAGGED = new Set([5, 15, 20])
const START_SECONDS = 32 * 60 + 47

/* ─── Sub-component: answer option ──────────────── */
function AnswerOption({
  letter,
  text,
  selected,
  onClick,
}: {
  letter: AnswerKey
  text: string
  selected: boolean
  onClick: () => void
}) {
  return (
    <div
      onClick={onClick}
      className={`exam-answer-opt ${selected ? 'exam-answer-opt--selected' : ''}`}
    >
      <div className={`answer-circle ${selected ? 'answer-circle--selected' : ''}`}>
        {letter}
      </div>
      <span
        className={`flex-1 text-[14px] leading-relaxed pt-0.5 ${
          selected ? 'text-white font-medium' : 'text-gray-700'
        }`}
      >
        {text}
      </span>
      {selected && (
        <CheckOutlined className="text-white text-base shrink-0 mt-1" />
      )}
    </div>
  )
}

/* ─── Question grid button ───────────────────────── */
function GridBtn({
  num,
  answered,
  current,
  flagged,
  onClick,
}: {
  num: number
  answered: boolean
  current: boolean
  flagged: boolean
  onClick: () => void
}) {
  let cls = 'q-grid-btn'
  if (current)  cls += ' q-grid-btn--current'
  else if (answered) cls += ' q-grid-btn--answered'
  else if (flagged)  cls += ' q-grid-btn--flagged'

  return (
    <button onClick={onClick} className={cls} title={`Câu ${num}`}>
      {num}
    </button>
  )
}

/* ─── Main component ─────────────────────────────── */
export default function ExamTakingPage() {
  const navigate = useNavigate()
  const [currentIdx, setCurrentIdx] = useState(22)       // 0-indexed → Q23
  const [answers, setAnswers] = useState(makeInitialAnswers)
  const [flagged, setFlagged] = useState(INITIAL_FLAGGED)
  const [timeLeft, setTimeLeft] = useState(START_SECONDS)

  /* countdown timer */
  useEffect(() => {
    const id = setInterval(
      () => setTimeLeft((t) => (t > 0 ? t - 1 : 0)),
      1000,
    )
    return () => clearInterval(id)
  }, [])

  const mm = String(Math.floor(timeLeft / 60)).padStart(2, '0')
  const ss = String(timeLeft % 60).padStart(2, '0')
  const timerDanger = timeLeft < 5 * 60

  const question = QUESTIONS[currentIdx]
  const answeredCount = Object.keys(answers).length
  const unansweredCount = 40 - answeredCount
  const flaggedCount = flagged.size

  const selectAnswer = (key: AnswerKey) =>
    setAnswers((prev) => ({ ...prev, [currentIdx]: key }))

  const toggleFlag = () =>
    setFlagged((prev) => {
      const next = new Set(prev)
      next.has(currentIdx) ? next.delete(currentIdx) : next.add(currentIdx)
      return next
    })

  const goTo = (idx: number) => {
    if (idx >= 0 && idx < 40) setCurrentIdx(idx)
  }

  const handleSubmit = () => {
    if (confirm('Bạn có chắc muốn nộp bài thi? Còn ' + unansweredCount + ' câu chưa trả lời.')) {
      navigate('/student/exam')
    }
  }

  return (
    <div className="h-screen flex flex-col overflow-hidden bg-gray-100">

      {/* ── Top bar ── */}
      <div className="taking-topbar">
        {/* Logo */}
        <div className="flex items-center gap-2 shrink-0">
          <div className="student-logo-icon">EH</div>
          <span className="font-bold text-white text-[14px]">ExamHub</span>
        </div>

        {/* Exam info (center) */}
        <div className="flex-1 text-center">
          <p className="text-white font-semibold text-[15px] leading-tight">
            Kiểm tra Toán học — Học kỳ 1
          </p>
          <p className="text-gray-400 text-[11px] mt-0.5">
            Mã đề: DE_2026_MATH_001&nbsp;·&nbsp;23/04/2026&nbsp;·&nbsp;Lớp 10A1
          </p>
        </div>

        {/* Timer */}
        <div className={`taking-timer ${timerDanger ? 'animate-pulse' : ''}`}>
          {mm}:{ss}
        </div>
      </div>

      {/* ── Main area ── */}
      <div className="flex-1 flex overflow-hidden">

        {/* ── Left: question panel ── */}
        <div className="flex-[3] flex flex-col overflow-hidden bg-white border-r border-gray-100">

          {/* Question header */}
          <div className="flex items-center justify-between px-6 py-3 border-b border-gray-100 bg-gray-50 shrink-0">
            <div className="flex items-center gap-3">
              <span className="text-sm font-semibold text-gray-700">
                Câu hỏi {currentIdx + 1} / 40
              </span>
              <span className={`badge ${answers[currentIdx] ? 'badge-orange' : 'badge-gray'}`}>
                {answers[currentIdx] ? 'Đã trả lời' : 'Chưa trả lời'}
              </span>
              {flagged.has(currentIdx) && (
                <span className="badge badge-yellow">Đã đánh dấu</span>
              )}
            </div>
            <button
              onClick={toggleFlag}
              title="Đánh dấu câu hỏi"
              className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium
                          transition-colors ${
                            flagged.has(currentIdx)
                              ? 'bg-yellow-100 text-yellow-700 hover:bg-yellow-200'
                              : 'bg-gray-100 text-gray-500 hover:bg-gray-200'
                          }`}
            >
              <FlagOutlined />
              {flagged.has(currentIdx) ? 'Bỏ đánh dấu' : 'Đánh dấu'}
            </button>
          </div>

          {/* Question content (scrollable) */}
          <div className="flex-1 overflow-auto px-8 py-6">
            <p className="text-[15px] text-gray-800 leading-relaxed mb-4">
              {question.text}
            </p>

            {question.formula && (
              <div className="bg-blue-50 border border-blue-100 rounded-xl px-5 py-3 mb-6
                              text-blue-800 text-[14px] font-mono text-center">
                {question.formula}
              </div>
            )}

            {/* Answer options */}
            <div className="flex flex-col gap-0 mt-2">
              {(['A', 'B', 'C', 'D'] as AnswerKey[]).map((key) => (
                <AnswerOption
                  key={key}
                  letter={key}
                  text={question.options[key]}
                  selected={answers[currentIdx] === key}
                  onClick={() => selectAnswer(key)}
                />
              ))}
            </div>
          </div>

          {/* ── Bottom navigation ── */}
          <div className="flex items-center justify-between px-6 py-3.5 border-t border-gray-100
                          bg-white shrink-0">
            <button
              onClick={() => goTo(currentIdx - 1)}
              disabled={currentIdx === 0}
              className={`flex items-center gap-2 px-5 py-2 rounded-lg text-sm font-medium
                          border transition-colors ${
                            currentIdx === 0
                              ? 'border-gray-200 text-gray-300 cursor-not-allowed'
                              : 'border-gray-300 text-gray-700 hover:bg-gray-50'
                          }`}
            >
              ← Câu trước
            </button>

            <span className="text-sm text-gray-500">
              Câu <span className="font-semibold text-gray-800">{currentIdx + 1}</span> / 40
            </span>

            <button
              onClick={() => goTo(currentIdx + 1)}
              disabled={currentIdx === 39}
              className={`flex items-center gap-2 px-5 py-2 rounded-lg text-sm font-semibold
                          transition-colors ${
                            currentIdx === 39
                              ? 'bg-blue-200 text-white cursor-not-allowed'
                              : 'bg-blue-600 text-white hover:bg-blue-700'
                          }`}
            >
              Câu tiếp →
            </button>
          </div>
        </div>

        {/* ── Right: progress panel ── */}
        <div className="w-72 shrink-0 flex flex-col bg-white overflow-hidden">
          <div className="flex-1 overflow-auto p-4 flex flex-col gap-4">

            {/* Student info */}
            <div className="flex items-center gap-3 pb-3 border-b border-gray-100">
              <div className="w-10 h-10 rounded-full bg-blue-600 flex items-center
                              justify-center text-white font-bold text-sm shrink-0">
                A
              </div>
              <div>
                <p className="text-[13px] font-semibold text-gray-800">Nguyễn Văn An</p>
                <p className="text-[11px] text-gray-400">Mã đề · DE_2026_MATH_001</p>
              </div>
            </div>

            {/* Stats */}
            <div className="grid grid-cols-3 gap-2">
              {[
                { value: answeredCount,  label: 'Đã trả lời',    color: 'text-orange-500' },
                { value: unansweredCount,label: 'Chưa trả lời',  color: 'text-gray-500'   },
                { value: flaggedCount,   label: 'Đã đánh dấu',   color: 'text-yellow-500' },
              ].map((s) => (
                <div key={s.label}
                  className="bg-gray-50 rounded-xl p-2.5 text-center border border-gray-100">
                  <p className={`text-xl font-bold ${s.color}`}>{s.value}</p>
                  <p className="text-[10px] text-gray-400 mt-0.5 leading-tight">{s.label}</p>
                </div>
              ))}
            </div>

            {/* Grid header + legend */}
            <div>
              <div className="flex items-center justify-between mb-2">
                <p className="text-[12px] font-semibold text-gray-700">Bảng câu hỏi</p>
                <div className="flex gap-2">
                  {[
                    { cls: 'bg-orange-400', label: 'Đã làm' },
                    { cls: 'bg-yellow-100 border border-yellow-400', label: 'Đánh dấu' },
                    { cls: 'bg-gray-100 border border-gray-200', label: 'Chưa làm' },
                  ].map((l) => (
                    <span key={l.label} className="flex items-center gap-1 text-[10px] text-gray-400">
                      <span className={`w-2.5 h-2.5 rounded-sm inline-block ${l.cls}`} />
                      {l.label}
                    </span>
                  ))}
                </div>
              </div>

              {/* Question grid (5 columns) */}
              <div className="grid grid-cols-5 gap-1.5">
                {QUESTIONS.map((q, idx) => (
                  <GridBtn
                    key={q.id}
                    num={q.id}
                    answered={idx in answers}
                    current={idx === currentIdx}
                    flagged={flagged.has(idx)}
                    onClick={() => setCurrentIdx(idx)}
                  />
                ))}
              </div>
            </div>
          </div>

          {/* Submit area */}
          <div className="p-4 border-t border-gray-100 shrink-0">
            <button
              onClick={handleSubmit}
              className="w-full py-3 bg-green-500 hover:bg-green-600 text-white font-semibold
                         text-[14px] rounded-xl transition-colors shadow-sm"
            >
              Nộp bài thi
            </button>
            {unansweredCount > 0 && (
              <p className="text-center text-[11px] text-gray-400 mt-2">
                Còn {unansweredCount} câu chưa trả lời
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
