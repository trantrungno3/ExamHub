export const MAX_SCHOOL_MEMBER_IMPORT_BYTES = 10 * 1024 * 1024

/** Kiểm tra metadata file trước khi upload; trả về thông báo lỗi hoặc null khi hợp lệ. */
export function validateSchoolMemberImportFile(
    file?: Pick<File, 'name' | 'size'>,
): string | null {
    if (!file) return 'Vui lòng chọn file Excel.'
    if (!file.name.toLowerCase().endsWith('.xlsx')) return 'Chỉ chấp nhận file Excel (.xlsx).'
    if (file.size === 0) return 'File import không được để trống.'
    return file.size > MAX_SCHOOL_MEMBER_IMPORT_BYTES
        ? 'File import không được vượt quá 10 MB.'
        : null
}

/** Chỉ cho phép import khi preview còn ít nhất một dòng hợp lệ. */
export function canImportSchoolMemberPreview(
    preview?: Pick<SchoolMemberImportPreview, 'validCount'>,
): boolean {
    return (preview?.validCount ?? 0) > 0
}
