import {useMemo, useState} from 'react'
import {useNavigate} from 'react-router-dom'
import {Button, Input, Select, Table, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {SearchOutlined} from '@ant-design/icons'
import {useExamsQuery} from '../../hooks/queries/useExams'
import {useMySubmissionsQuery} from '../../hooks/queries/useSubmissions'
import {useGradeLevelsListQuery, useSubjectsQuery} from '../../hooks/queries/useCategoryLists'
import {useAuth} from '../../AuthProvider'

/** Trạng thái làm bài nhìn từ phía học sinh. */
type StudentStatus = 'NotStarted' | 'InProgress' | 'Submitted' | 'Graded'

const STUDENT_STATUS_LABEL: Record<StudentStatus, string> = {
    NotStarted: 'Chưa làm',
    InProgress: 'Đang làm',
    Submitted: 'Đã nộp (chờ chấm)',
    Graded: 'Đã chấm',
}
const STUDENT_STATUS_COLOR: Record<StudentStatus, string> = {
    NotStarted: 'default',
    InProgress: 'processing',
    Submitted: 'gold',
    Graded: 'green',
}

interface ExamRow extends Exam {
    studentStatus: StudentStatus
    myScore?: number
    submissionId?: string
}

export default function StudentExamListPage() {
    const navigate = useNavigate()
    const {user} = useAuth()

    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()

    const [gradeLevelId, setGradeLevelId] = useState<number>()
    const [subjectId, setSubjectId] = useState<number>()
    const [studentStatus, setStudentStatus] = useState<StudentStatus>()
    const [keyword, setKeyword] = useState('')

    // Load đề đã phát hành (pageSize lớn — lọc/phân trang client-side để gộp trạng thái làm bài)
    const examsQuery: ExamPagedQuery = useMemo(
        () => ({page: 1, pageSize: 100, status: 'Published', gradeLevelId, subjectId, keyword}),
        [gradeLevelId, subjectId, keyword],
    )
    const {data: examPage, isLoading: examsLoading} = useExamsQuery(examsQuery)
    const {data: submissions = [], isLoading: subsLoading} = useMySubmissionsQuery(user?.id)

    // Map examId → submission mới nhất của học sinh
    const subByExam = useMemo(() => {
        const map = new Map<string, ExamSubmission>()
        for (const s of submissions) {
            const prev = map.get(s.examId)
            if (!prev || s.createdAt > prev.createdAt) map.set(s.examId, s)
        }
        return map
    }, [submissions])

    const rows: ExamRow[] = useMemo(() => {
        const exams = examPage?.items ?? []
        return exams.map(e => {
            const sub = subByExam.get(e.id)
            let status: StudentStatus = 'NotStarted'
            if (sub) status = sub.status as StudentStatus
            return {
                ...e,
                studentStatus: status,
                myScore: status === 'Graded' ? sub?.totalScore : undefined,
                submissionId: sub?.id,
            }
        })
    }, [examPage, subByExam])

    const filteredRows = useMemo(
        () => (studentStatus ? rows.filter(r => r.studentStatus === studentStatus) : rows),
        [rows, studentStatus],
    )

    const columns: TableColumnsType<ExamRow> = [
        {title: 'Tiêu đề', dataIndex: 'title', key: 'title', render: v => <span className="font-medium text-gray-800">{v}</span>},
        {title: 'Mã đề', dataIndex: 'examCode', key: 'examCode', width: 100, render: v => v ?? '—'},
        {title: 'Môn', dataIndex: 'subjectName', key: 'subjectName', width: 120, render: v => v ?? '—'},
        {title: 'Lớp', dataIndex: 'gradeLevelName', key: 'gradeLevelName', width: 80, render: v => v ?? '—'},
        {title: 'Thời gian', dataIndex: 'durationMinutes', key: 'durationMinutes', width: 90, render: v => `${v} phút`},
        {title: 'Tổng điểm', dataIndex: 'totalScore', key: 'totalScore', width: 90},
        {
            title: 'Trạng thái', dataIndex: 'studentStatus', key: 'studentStatus', width: 150,
            render: (v: StudentStatus) => <Tag color={STUDENT_STATUS_COLOR[v]}>{STUDENT_STATUS_LABEL[v]}</Tag>,
        },
        {
            title: 'Điểm của tôi', key: 'myScore', width: 110,
            render: (_, r) => r.studentStatus === 'Graded'
                ? <span className="font-semibold text-blue-600">{r.myScore ?? 0}/{r.totalScore}</span>
                : <span className="text-gray-400">—</span>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 130, fixed: 'right',
            render: (_, r) => {
                if (r.studentStatus === 'NotStarted' || r.studentStatus === 'InProgress') {
                    return (
                        <Button type="primary" size="small" onClick={() => navigate(`/student/exam?examId=${r.id}`)}>
                            {r.studentStatus === 'InProgress' ? 'Tiếp tục' : 'Vào thi'}
                        </Button>
                    )
                }
                return (
                    <Button size="small" onClick={() => navigate(`/student/exam/result?submissionId=${r.submissionId}`)}>
                        Xem kết quả
                    </Button>
                )
            },
        },
    ]

    return (
        <div className="p-6 flex flex-col gap-4">
            <div>
                <p className="text-xl font-semibold text-gray-800">Đề thi của tôi</p>
                <p className="text-sm text-gray-500">Danh sách đề thi, trạng thái làm bài và điểm số</p>
            </div>

            <div className="flex items-center gap-2 flex-wrap">
                <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm đề thi..."
                       style={{width: 220}} allowClear value={keyword}
                       onChange={e => setKeyword(e.target.value)}/>
                <Select placeholder="Môn" allowClear showSearch optionFilterProp="label" style={{width: 160}}
                        value={subjectId} onChange={setSubjectId}
                        options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                <Select placeholder="Lớp" allowClear style={{width: 130}} value={gradeLevelId}
                        onChange={setGradeLevelId}
                        options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                <Select placeholder="Trạng thái" allowClear style={{width: 180}} value={studentStatus}
                        onChange={setStudentStatus}
                        options={(Object.keys(STUDENT_STATUS_LABEL) as StudentStatus[]).map(s => ({value: s, label: STUDENT_STATUS_LABEL[s]}))}/>
            </div>

            <div className="section-card shrink-0">
                <Table columns={columns} dataSource={filteredRows} rowKey="id"
                       loading={examsLoading || subsLoading}
                       scroll={{x: 1000}}
                       pagination={{
                           pageSize: 10, showSizeChanger: true,
                           showTotal: total => `Tổng số ${total} đề thi`,
                       }}/>
            </div>
        </div>
    )
}
