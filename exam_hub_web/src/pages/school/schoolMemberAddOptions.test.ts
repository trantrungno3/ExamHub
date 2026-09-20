import {describe, expect, it} from 'vitest'
import {eligibleSchoolUsers, sectionsForCohort} from './schoolMemberAddOptions'

const user = (id: string, roles: string[], isDeleted = false) =>
    ({id, roles, isDeleted}) as UserResponse

describe('eligibleSchoolUsers', () => {
    it('returns only eligible teachers by global role and membership', () => {
        const users = [
            user('available', ['Teacher']),
            user('student', ['Student']),
            user('deleted', ['Teacher'], true),
            user('existing', ['Teacher']),
        ]
        const members = [{userId: 'existing'} as SchoolMember]
        expect(eligibleSchoolUsers('Teacher', users, members, []).map(x => x.id))
            .toEqual(['available'])
    })

    it('excludes students already in any loaded school cohort', () => {
        const users = [user('available', ['Student']), user('existing', ['Student'])]
        const students = [{studentId: 'existing'} as CohortMember]
        expect(eligibleSchoolUsers('Student', users, [], students).map(x => x.id))
            .toEqual(['available'])
    })

    it('matches the global role case-insensitively', () => {
        const users = [user('available', ['admin'])]
        expect(eligibleSchoolUsers('Admin', users, [], []).map(x => x.id)).toEqual(['available'])
    })
})

describe('sectionsForCohort', () => {
    it('builds section names from cohort class count', () => {
        expect(sectionsForCohort({numClasses: 3})).toEqual(['A', 'B', 'C'])
    })

    it('returns nothing without a cohort', () => {
        expect(sectionsForCohort()).toEqual([])
    })
})
