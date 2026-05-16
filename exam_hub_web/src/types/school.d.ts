/* ─── School entity types (matches API JSON serialization) ── */

interface School {
    id: number
    name: string
    code: string
    address?: string
    phone?: string
    email?: string
    isActive: boolean
    createdAt: string
    updatedAt: string
}

interface SchoolBody {
    name: string
    code: string
    address?: string
    phone?: string
    email?: string
    isActive?: boolean
}
