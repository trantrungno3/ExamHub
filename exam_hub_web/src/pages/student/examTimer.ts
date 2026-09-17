export const remainingSeconds = (durationMinutes: number, durationSeconds: number) =>
    Math.max(0, durationMinutes * 60 - Math.max(0, durationSeconds))

export const secondsUntil = (deadlineAt: number, now = Date.now()) =>
    Math.max(0, Math.ceil((deadlineAt - now) / 1000))
