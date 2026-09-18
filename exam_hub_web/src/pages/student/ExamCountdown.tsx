import {useEffect, useRef, useState} from 'react'
import {secondsUntil} from './examTimer'

type Props = {
    /** Mốc hết giờ tuyệt đối (ms epoch). */
    deadlineAt: number
    /** Gọi đúng một lần khi đồng hồ chạm 0. */
    onExpire: () => void
}

/**
 * Đồng hồ đếm ngược tự giữ state — tách khỏi ExamRunner để tick mỗi giây
 * không re-render tờ đề và lưới câu hỏi.
 */
export function ExamCountdown({deadlineAt, onExpire}: Props) {
    const [timeLeft, setTimeLeft] = useState(() => secondsUntil(deadlineAt))
    const expired = useRef(false)
    const onExpireRef = useRef(onExpire)

    // Giữ callback mới nhất mà không đưa `onExpire` vào deps của effect đếm ngược —
    // hàm đó dựng lại mỗi render nên sẽ reset interval liên tục.
    // Effect này khai báo trước nên chạy trước effect kiểm hết giờ bên dưới.
    useEffect(() => {
        onExpireRef.current = onExpire
    }, [onExpire])

    useEffect(() => {
        const tick = () => setTimeLeft(secondsUntil(deadlineAt))
        tick()
        const id = setInterval(tick, 1000)
        return () => clearInterval(id)
    }, [deadlineAt])

    useEffect(() => {
        if (timeLeft === 0 && !expired.current) {
            expired.current = true
            onExpireRef.current()
        }
    }, [timeLeft])

    const mm = String(Math.floor(timeLeft / 60)).padStart(2, '0')
    const ss = String(timeLeft % 60).padStart(2, '0')
    const danger = timeLeft <= 300

    return (
        <div className={`take-timer ${danger ? 'take-timer--danger' : ''}`}
             role="timer" aria-live="off" aria-label={`Còn lại ${mm} phút ${ss} giây`}>
            <span className="take-timer-dot"/>{mm}:{ss}
        </div>
    )
}
