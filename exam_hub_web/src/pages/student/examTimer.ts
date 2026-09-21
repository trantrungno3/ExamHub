export const remainingSeconds = (durationMinutes: number, durationSeconds: number) =>
    Math.max(0, durationMinutes * 60 - Math.max(0, durationSeconds))

export const deadlineFromDuration = (durationMinutes: number, durationSeconds: number, now = Date.now()) =>
    now + remainingSeconds(durationMinutes, durationSeconds) * 1000

export const secondsUntil = (deadlineAt: number, now = Date.now()) =>
    Math.max(0, Math.ceil((deadlineAt - now) / 1000))
