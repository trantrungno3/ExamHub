export const secondsUntil = (deadlineAt: number, now = Date.now()) =>
    Math.max(0, Math.ceil((deadlineAt - now) / 1000))
