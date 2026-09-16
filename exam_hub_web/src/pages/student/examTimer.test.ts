import {describe, expect, it} from 'vitest'
import {secondsUntil} from './examTimer'

describe('secondsUntil', () => {
    it('derives remaining whole seconds from an absolute deadline', () => {
        expect(secondsUntil(70_001, 10_000)).toBe(61)
    })

    it('never returns a negative value', () => {
        expect(secondsUntil(9_999, 10_000)).toBe(0)
    })
})
