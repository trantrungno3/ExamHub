import {AuthHttp} from './requestService'

class SchoolMemberService {
    getById(id: string) {
        return AuthHttp.get<SchoolMember>(`/schoolmember/${id}`)
    }

    getBySchool(schoolId: number) {
        return AuthHttp.get<SchoolMember[]>(`/schoolmember/by-school/${schoolId}`)
    }

    getBySchoolAndRole(schoolId: number, role: string) {
        return AuthHttp.get<SchoolMember[]>(`/schoolmember/by-school/${schoolId}/role/${encodeURIComponent(role)}`)
    }

    getByUser(userId: string) {
        return AuthHttp.get<SchoolMember[]>(`/schoolmember/by-user/${userId}`)
    }

    add(body: SchoolMemberBody) {
        return AuthHttp.post<SchoolMember>(`/schoolmember`, body)
    }

    update(id: string, body: SchoolMemberBody) {
        return AuthHttp.put<SchoolMember>(`/schoolmember/${id}`, body)
    }

    remove(id: string) {
        return AuthHttp.delete<void>(`/schoolmember/${id}`)
    }

    setActive(id: string, isActive: boolean) {
        return AuthHttp.patch<boolean>(`/schoolmember/${id}/active`, isActive)
    }

    bulkAdd(body: SchoolMemberBulkAddRequest) {
        return AuthHttp.post<SchoolMemberBulkResult>('/schoolmember/bulk', body)
    }

    previewImport(schoolId: number, file: File) {
        return AuthHttp.postForm<SchoolMemberImportPreview>(
            '/schoolmember/bulk-import/preview', importForm(schoolId, file),
        )
    }

    bulkImport(schoolId: number, file: File) {
        return AuthHttp.postForm<SchoolMemberBulkResult>(
            '/schoolmember/bulk-import', importForm(schoolId, file),
        )
    }

    downloadImportTemplate() {
        return AuthHttp.getBlob('/schoolmember/bulk-import/template')
    }
}

function importForm(schoolId: number, file: File) {
    const form = new FormData()
    form.append('schoolId', String(schoolId))
    form.append('file', file)
    return form
}

export const schoolMemberService = new SchoolMemberService()
