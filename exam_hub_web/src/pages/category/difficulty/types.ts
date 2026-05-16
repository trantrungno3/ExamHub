export type DiffRow = {
    id: number
    code: string
    name: string
    codeBadge: string
    nameBadge: string
    weight: string
    priority: string
    active: boolean
}

export const DIFFICULTIES: DiffRow[] = [
    {
        id: 1,
        code: 'easy',
        name: 'Dễ',
        codeBadge: 'bg-green-100 text-green-700',
        nameBadge: 'bg-green-100 text-green-700',
        weight: '×1.00',
        priority: 'Ưu tiên 1',
        active: true,
    },
    {
        id: 2,
        code: 'medium',
        name: 'Trung bình',
        codeBadge: 'bg-yellow-100 text-yellow-700',
        nameBadge: 'bg-yellow-100 text-yellow-700',
        weight: '×1.50',
        priority: 'Ưu tiên 2',
        active: true,
    },
    {
        id: 3,
        code: 'hard',
        name: 'Khó',
        codeBadge: 'bg-red-100 text-red-600',
        nameBadge: 'bg-red-100 text-red-600',
        weight: '×2.00',
        priority: 'Ưu tiên 3',
        active: true,
    },
    {
        id: 4,
        code: 'very_hard',
        name: 'Rất khó',
        codeBadge: 'bg-purple-100 text-purple-700',
        nameBadge: 'bg-purple-100 text-purple-700',
        weight: '×2.50',
        priority: 'Ưu tiên 4',
        active: true,
    },
]
