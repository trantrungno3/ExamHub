import {describe, expect, it} from 'vitest'
import {
    MAX_SCHOOL_MEMBER_IMPORT_BYTES,
    validateSchoolMemberImportFile,
} from './schoolMemberImportFile'

describe('validateSchoolMemberImportFile', () => {
    it.each([
        [undefined, 'Vui lòng chọn file Excel.'],
        [{name: 'members.csv', size: 10}, 'Chỉ chấp nhận file Excel (.xlsx).'],
        [{name: 'members.xlsx', size: 0}, 'File import không được để trống.'],
        [
            {name: 'members.xlsx', size: MAX_SCHOOL_MEMBER_IMPORT_BYTES + 1},
            'File import không được vượt quá 10 MB.',
        ],
    ])('rejects invalid file %#', (file, expected) => {
        expect(validateSchoolMemberImportFile(file)).toBe(expected)
    })

    it('accepts uppercase extension at the exact limit', () => {
        expect(
            validateSchoolMemberImportFile({
                name: 'MEMBERS.XLSX',
                size: MAX_SCHOOL_MEMBER_IMPORT_BYTES,
            }),
        ).toBeNull()
    })
})
