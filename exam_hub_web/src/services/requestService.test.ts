import {describe, expect, it} from 'vitest'
import {parseDotnetError} from './requestService'

describe('parseDotnetError', () => {
    it('lấy message từ stack trace .NET', () => {
        expect(parseDotnetError(
            'System.ArgumentException: Tiêu đề Excel phải là UserName, Role, CohortName, Section.\n' +
            '   at ExamHub.Core.Infrastructure.Persistence.Services.Implementations.SchoolMemberBulkService.ValidateHeaders(IXLWorksheet sheet) in D:\\a\\b.cs:line 331',
        )).toBe('Tiêu đề Excel phải là UserName, Role, CohortName, Section.')
    })

    it('bỏ phần inner exception', () => {
        expect(parseDotnetError('System.InvalidOperationException: Ngoài ---> System.Exception: Trong'))
            .toBe('Ngoài')
    })

    it('giữ nguyên text không phải exception', () => {
        expect(parseDotnetError('Bad Gateway')).toBe('Bad Gateway')
    })

    it('rỗng thì trả thông báo mặc định', () => {
        expect(parseDotnetError('   \n at Foo()')).toBe('Đã xảy ra lỗi không xác định.')
    })
})
