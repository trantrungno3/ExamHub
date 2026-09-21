import {describe, expect, it} from 'vitest'
import {deadlineFromDuration, remainingSeconds, secondsUntil} from './examTimer'

describe('remainingSeconds', () => {
    it('subtracts elapsed seconds from session duration', () => {
        expect(remainingSeconds(45, 125)).toBe(2575)
    })

    it('never returns a negative value', () => {
        expect(remainingSeconds(1, 90)).toBe(0)
    })
})

describe('deadlineFromDuration', () => {
    it('adds remaining seconds to now', () => {
        expect(deadlineFromDuration(45, 125, 10_000)).toBe(10_000 + 2575 * 1000)
    })

    it('clamps elapsed beyond duration to now', () => {
        expect(deadlineFromDuration(1, 90, 10_000)).toBe(10_000)
    })

    it('is deterministic for identical inputs', () => {
        const a = deadlineFromDuration(45, 125, 10_000)
        const b = deadlineFromDuration(45, 125, 10_000)
        expect(a).toBe(b)
    })
})

describe('secondsUntil', () => {
    it('derives remaining whole seconds from an absolute deadline', () => {
        expect(secondsUntil(70_001, 10_000)).toBe(61)
    })

    it('never returns a negative value', () => {
        expect(secondsUntil(9_999, 10_000)).toBe(0)
    })
})
