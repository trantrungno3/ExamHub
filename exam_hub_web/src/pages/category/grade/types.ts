export type Grade = {
    id: number
    name: string
    grade: number
    desc: string
    active: boolean
    createdAt: string
}

export const GRADES: Grade[] = [
    {id: 1, name: 'Lớp 1', grade: 1, desc: 'Cấp tiểu học', active: true, createdAt: '01/01/2024'},
    {id: 2, name: 'Lớp 2', grade: 2, desc: 'Cấp tiểu học', active: true, createdAt: '01/01/2024'},
    {id: 3, name: 'Lớp 3', grade: 3, desc: 'Cấp tiểu học', active: true, createdAt: '01/01/2024'},
    {id: 4, name: 'Lớp 4', grade: 4, desc: 'Cấp tiểu học', active: true, createdAt: '01/01/2024'},
    {id: 5, name: 'Lớp 5', grade: 5, desc: 'Cấp tiểu học', active: true, createdAt: '01/01/2024'},
    {id: 6, name: 'Lớp 6', grade: 6, desc: 'Cấp THCS', active: true, createdAt: '01/01/2024'},
    {id: 7, name: 'Lớp 7', grade: 7, desc: 'Cấp THCS', active: true, createdAt: '01/01/2024'},
    {id: 8, name: 'Lớp 8', grade: 8, desc: 'Cấp THCS', active: true, createdAt: '01/01/2024'},
    {id: 9, name: 'Lớp 9', grade: 9, desc: 'Cấp THCS', active: true, createdAt: '01/01/2024'},
    {id: 10, name: 'Lớp 10', grade: 10, desc: 'Cấp THPT', active: true, createdAt: '01/01/2024'},
    {id: 11, name: 'Lớp 11', grade: 11, desc: 'Cấp THPT', active: true, createdAt: '01/01/2024'},
    {id: 12, name: 'Lớp 12', grade: 12, desc: 'Cấp THPT', active: true, createdAt: '01/01/2024'},
]
