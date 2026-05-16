import {useCallback, useEffect, useState} from 'react'
import {message} from 'antd'
import type {CategoryServiceBase} from '../services/categoryServiceBase'

export function useCategoryTab<TEntity extends {id: number}, TBody>(
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
    useEffect(() => { fetchData() }, [fetchData])

    const handleSave = useCallback(async (body: TBody): Promise<boolean> => {
        try {
            const res = editing
                ? await service.update(editing.id, body)
                : await service.create(body)
            if (!res.isSuccess) {
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
            await service.remove(id)
            message.success('Đã xóa')
            setData(prev => prev.filter(item => item.id !== id))
        } catch {
            message.error('Không thể xóa')
        }
    }, [service])

    const openCreate = useCallback(() => { setEditing(null); setModalOpen(true) }, [])
    const openEdit = useCallback((record: TEntity) => { setEditing(record); setModalOpen(true) }, [])
    const closeModal = useCallback(() => setModalOpen(false), [])

    return {data, loading, modalOpen, editing, fetchData, handleSave, handleDelete, openCreate, openEdit, closeModal}
}
