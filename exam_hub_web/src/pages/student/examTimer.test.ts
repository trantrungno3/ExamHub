import {describe, expect, it} from 'vitest'
import {remainingSeconds, secondsUntil} from './examTimer'

describe('remainingSeconds', () => {
    it('subtracts elapsed seconds from session duration', () => {
        expect(remainingSeconds(45, 125)).toBe(2575)
    })

    it('never returns a negative value', () => {
        expect(remainingSeconds(1, 90)).toBe(0)
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
