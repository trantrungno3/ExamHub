export const QUESTION_BANK_STATS = [
    { label: 'Tổng câu hỏi', value: '2,841', iconBg: 'bg-blue-100',   icon: '📝' },
    { label: 'Trắc nghiệm',   value: '2,104', iconBg: 'bg-green-100',  icon: '☑️' },
    { label: 'Đúng / Sai',    value: '483',   iconBg: 'bg-orange-100', icon: '⚖️' },
    { label: 'Tự luận',       value: '254',   iconBg: 'bg-purple-100', icon: '✏️' },
]

export type MockQuestion = {
    id: number
    content: string
    subject: string
    grade: number
    type: string
    difficulty: string
    status: string
}

export const MOCK_QUESTIONS: MockQuestion[] = [
    { id: 1, content: 'Trong tam giác ABC, nếu AB²+BC²=AC² thì tam giác ABC là tam giác gì?',                         subject: 'Toán', grade: 10, type: 'Trắc nghiệm', difficulty: 'Dễ',         status: 'Đã duyệt'  },
    { id: 2, content: 'Giải phương trình 2x² - 5x + 2 = 0. Tìm nghiệm x₁, x₂ khi đó x₁ + x₂ bằng bao nhiêu?',      subject: 'Toán', grade: 11, type: 'Trắc nghiệm', difficulty: 'Trung bình', status: 'Đã duyệt'  },
    { id: 3, content: 'Phát biểu nào là đúng về định luật bảo toàn năng lượng trong hệ cô lập?',                      subject: 'Lý',   grade: 10, type: 'Trắc nghiệm', difficulty: 'Khó',        status: 'Chờ duyệt' },
    { id: 4, content: 'Các đồng phân của C₄H₈O₂ có khả năng tham gia phản ứng tráng bạc là mấy chất?',              subject: 'Hóa',  grade: 11, type: 'Trắc nghiệm', difficulty: 'Trung bình', status: 'Đã duyệt'  },
    { id: 5, content: "Choose the correct form: 'She ___ to school every day.'",                                       subject: 'Anh',  grade: 12, type: 'Điền vào',    difficulty: 'Dễ',         status: 'Đã duyệt'  },
    { id: 6, content: 'Điền vào chỗ trống: Quá trình tổng hợp ATP trong ti thể còn gọi là ___',                      subject: 'Sinh', grade: 11, type: 'Điền vào',    difficulty: 'Trung bình', status: 'Nháp'      },
    { id: 7, content: 'Nhận xét nào về vật trong điện trường đều là đúng khi vật đặt ở điểm có điện thế cao hơn?',   subject: 'Lý',   grade: 12, type: 'Trắc nghiệm', difficulty: 'Khó',        status: 'Đã duyệt'  },
    { id: 8, content: 'Trong tế bào nhân thực, bào quan nào có chức năng chính là tổng hợp protein?',                subject: 'Sinh', grade: 10, type: 'Trắc nghiệm', difficulty: 'Dễ',         status: 'Đã duyệt'  },
]
