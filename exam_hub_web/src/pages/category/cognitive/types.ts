export type BloomLevel = {
    id: number
    name: string
    eng: string
    code: string
    color: string
    tagColor: string
    keywords: string
    pyramidWidth: string
    active: boolean
}

export const BLOOM_LEVELS: BloomLevel[] = [
    {
        id: 1,
        name: 'Nhớ',
        eng: 'Remember',
        code: 'remember',
        color: 'bg-sky-200',
        tagColor: 'bg-sky-100 text-sky-700',
        keywords: 'Liệt kê · Xác định · Nhận ra · Gọi tên · Định nghĩa',
        pyramidWidth: 'w-full',
        active: true,
    },
    {
        id: 2,
        name: 'Hiểu',
        eng: 'Understand',
        code: 'understand',
        color: 'bg-blue-300',
        tagColor: 'bg-blue-100 text-blue-700',
        keywords: 'Phân tả · Giải thích · Phân loại · So sánh · Tóm tắt',
        pyramidWidth: 'w-[87%]',
        active: true,
    },
    {
        id: 3,
        name: 'Vận dụng',
        eng: 'Apply',
        code: 'apply',
        color: 'bg-amber-300',
        tagColor: 'bg-amber-100 text-amber-700',
        keywords: 'Tính toán · Giải · Áp dụng · Thực hiện · Xây dựng',
        pyramidWidth: 'w-[74%]',
        active: true,
    },
    {
        id: 4,
        name: 'Phân tích',
        eng: 'Analyze',
        code: 'analyze',
        color: 'bg-blue-500',
        tagColor: 'bg-blue-100 text-blue-700',
        keywords: 'Phân tích · Phân biệt · Kiểm tra · Suy luận · So sánh',
        pyramidWidth: 'w-[60%]',
        active: true,
    },
    {
        id: 5,
        name: 'Đánh giá',
        eng: 'Evaluate',
        code: 'evaluate',
        color: 'bg-rose-400',
        tagColor: 'bg-rose-100 text-rose-700',
        keywords: 'Đánh giá · Phê bình · Lập luận · Chứng minh · Ưu tiên',
        pyramidWidth: 'w-[47%]',
        active: true,
    },
    {
        id: 6,
        name: 'Sáng tạo',
        eng: 'Create',
        code: 'create',
        color: 'bg-red-600',
        tagColor: 'bg-red-100 text-red-700',
        keywords: 'Thiết kế · Xây dựng · Lập kế hoạch · Sáng tác · Đề xuất',
        pyramidWidth: 'w-[33%]',
        active: true,
    },
]
