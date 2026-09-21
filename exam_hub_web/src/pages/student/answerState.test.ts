import {describe, expect, it} from 'vitest'
import {answeredIds, hasAnswer, sameSet} from './answerState'

describe('hasAnswer', () => {
    it('treats a non-empty string as answered', () => {
        expect(hasAnswer('a1')).toBe(true)
    })

    it('treats an empty string as unanswered', () => {
        expect(hasAnswer('')).toBe(false)
    })

    it('treats a whitespace-only essay as unanswered', () => {
        expect(hasAnswer('   \n  ')).toBe(false)
    })

    it('treats undefined and null as unanswered', () => {
        expect(hasAnswer(undefined)).toBe(false)
        expect(hasAnswer(null)).toBe(false)
    })
})

describe('answeredIds', () => {
    it('keeps only the ids that have an answer', () => {
        const ids = answeredIds({q1: 'a1', q2: '', q3: 'bài làm', q4: undefined})
        expect([...ids].sort()).toEqual(['q1', 'q3'])
    })

    it('returns an empty set for no values', () => {
        expect(answeredIds({}).size).toBe(0)
    })
})

describe('sameSet', () => {
    it('is true for equal contents regardless of insertion order', () => {
        expect(sameSet(new Set(['a', 'b']), new Set(['b', 'a']))).toBe(true)
    })

    it('is false when one has an extra member', () => {
        expect(sameSet(new Set(['a']), new Set(['a', 'b']))).toBe(false)
    })

    it('is false for equal sizes but different members', () => {
        expect(sameSet(new Set(['a']), new Set(['b']))).toBe(false)
    })
})
