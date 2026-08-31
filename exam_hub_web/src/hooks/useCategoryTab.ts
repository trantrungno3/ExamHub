import {useCallback, useEffect, useState} from 'react'
import {message} from 'antd'
import type {CategoryServiceBase} from '../services/categoryServiceBase'
import {statusCode} from '../services/requestService'

/**
 * State + hành vi CRUD dùng chung cho các trang quản lý danh mục dạng bảng (Môn học, Chủ đề, Khối
 * lớp, Dạng câu hỏi...): tự fetch danh sách khi mount, mở/đóng modal thêm-sửa, lưu (create hoặc
 * update tuỳ có `editing` hay không), xoá kèm cập nhật lạc quan (bỏ item khỏi `data` ngay khi xoá
 * thành công thay vì fetch lại). Mỗi trang chỉ cần truyền `service` (implement CategoryServiceBase)
 * và nhãn tiếng Việt của entity để hiện trong toast lỗi/thành công.
 */
export function useCategoryTab<TEntity extends { id: number }, TBody>(
    service: CategoryServiceBase<TEntity, TBody>,
    entityLabel: string,
) {
    const [data, setData] = useState<TEntity[]>([])
    const [loading, setLoading] = useState(true)
    const [modalOpen, setModalOpen] = useState(false)
    const [editing, setEditing] = useState<TEntity | null>(null)

    const fetchData = useCallback(() => {
        void service.getAll()
            .then(res => setData(res.data ?? []))
            .catch(() => message.error(`Không thể tải danh sách ${entityLabel}`))
            .finally(() => setLoading(false))
    }, [service, entityLabel])

    // loading starts as true, so no setState needed here
    useEffect(() => {
        fetchData()
    }, [fetchData])

    const handleSave = useCallback(async (body: TBody): Promise<boolean> => {
        try {
            const res = editing
                ? await service.update(editing.id, body)
                : await service.create(body)
            if (!res.status || !res.data) {
                message.error(res.message || 'Có lỗi xảy ra')
                return false
            }
            message.success(editing ? 'Cập nhật thành công' : 'Thêm thành công')
            setLoading(true)
            fetchData()
            return true
        } catch {
            message.error('Có lỗi xảy ra')
            return false
        }
    }, [editing, service, fetchData])

    const handleDelete = useCallback(async (id: number) => {
        try {
            const res = await service.remove(id)
            if (res.status !== statusCode.Deleted) {
                message.error(res.message || 'Không thể xóa')
                return
            }
            message.success('Đã xóa')
            setData(prev => prev.filter(item => item.id !== id))
        } catch {
            message.error('Không thể xóa')
        }
    }, [service])

    const openCreate = useCallback(() => {
        setEditing(null);
        setModalOpen(true)
    }, [])
    const openEdit = useCallback((record: TEntity) => {
        setEditing(record);
        setModalOpen(true)
    }, [])
    const closeModal = useCallback(() => setModalOpen(false), [])

    return {data, loading, modalOpen, editing, fetchData, handleSave, handleDelete, openCreate, openEdit, closeModal}
}
