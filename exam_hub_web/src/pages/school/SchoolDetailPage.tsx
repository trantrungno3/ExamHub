import {useMemo, useState} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Breadcrumb, Button, Form, Input, Modal, Popconfirm, Segmented, Table, Tabs, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, RightOutlined, SearchOutlined} from '@ant-design/icons'
import {StatusTag} from '../../components/StatusTag'
import {ROLE_COLOR, ROLE_LABEL} from '../../constants'
import {useSchoolsQuery} from '../../hooks/queries/useSchools'
import {useCohortsQuery, useCreateCohortMutation, useDeleteCohortMutation} from '../../hooks/queries/useCohorts'
import {useSchoolMembersQuery, useRemoveSchoolMemberMutation, useSetSchoolMemberActiveMutation} from '../../hooks/queries/useSchoolMembers'
import {useCohortMembersBySchoolQuery} from '../../hooks/queries/useCohortMembers'
import {useUsersQuery} from '../../hooks/queries/useUsers'
import {statusCode} from '../../services/requestService'
import PageHeader from '../../components/PageHeader'
import SchoolMemberAddModal from './SchoolMemberAddModal'
import {buildMemberRows, filterMemberRows, type MemberKind, type MemberRow} from './schoolMemberRows'
import {useDebounced} from '../../hooks/useDebounced'

export default function SchoolDetailPage() {
    const {id} = useParams<{id: string}>()
    const schoolId = Number(id)
    const navigate = useNavigate()

    const {data: schools = []} = useSchoolsQuery()
    const school = schools.find(s => s.id === schoolId)

    const {data: cohorts = [], isFetching: fetchingCohorts} = useCohortsQuery(schoolId)
    const {data: members = [], isFetching: fetchingMembers} = useSchoolMembersQuery(schoolId)
    const {data: students = [], isFetching: fetchingStudents} = useCohortMembersBySchoolQuery(schoolId)

    const createCohortMutation = useCreateCohortMutation(schoolId)
    const deleteCohortMutation = useDeleteCohortMutation(schoolId)
    const removeMemberMutation = useRemoveSchoolMemberMutation(schoolId)
    const setActiveMutation = useSetSchoolMemberActiveMutation(schoolId)

    const {data: allUsers = []} = useUsersQuery()

    const [cohortModal, setCohortModal] = useState(false)
    const [memberModal, setMemberModal] = useState(false)
    const [cohortForm] = Form.useForm<CohortBody>()

    // Tab "Thành viên" gộp nhân sự trường + học sinh các khoá thành một bảng, lọc client-side.
    const [memberKind, setMemberKind] = useState<'all' | MemberKind>('all')
    const [memberKeyword, setMemberKeyword] = useState('')
    const debouncedMemberKeyword = useDebounced(memberKeyword)
    const memberRows = useMemo(
        () => buildMemberRows(members, students, allUsers, cohorts),
        [members, students, allUsers, cohorts],
    )
    const visibleMemberRows = useMemo(
        () => filterMemberRows(memberRows, memberKind, debouncedMemberKeyword),
        [memberRows, memberKind, debouncedMemberKeyword],
    )

    const handleAddCohort = async () => {
        const values = await cohortForm.validateFields()
        const res = await createCohortMutation.mutateAsync({...values, schoolId})
        if (res.status !== statusCode.Error) { setCohortModal(false); cohortForm.resetFields() }
    }

    const cohortColumns: TableColumnsType<Cohort> = [
        {title: 'Tên khoá', dataIndex: 'name', key: 'name', render: v => <span className="font-medium">{v}</span>},
        {title: 'Năm bắt đầu', dataIndex: 'startYear', key: 'startYear'},
        {title: 'Năm kết thúc', dataIndex: 'endYear', key: 'endYear'},
        {title: 'Lớp bắt đầu', dataIndex: 'gradeStart', key: 'gradeStart'},
        {title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: v => <StatusTag status={v ? 'success' : 'default'} label={v ? 'Hoạt động' : 'Tắt'}/>},
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, record) => (
                <div className="flex gap-2">
                    <Popconfirm title="Xóa khoá học này?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                        onConfirm={() => deleteCohortMutation.mutate({id: record.id})}>
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                    <Button size="small" icon={<RightOutlined/>} onClick={() => navigate(`/app/cohorts/${record.id}`)}>
                        Chi tiết
                    </Button>
                </div>
            ),
        },
    ]

    const memberColumns: TableColumnsType<MemberRow> = [
        {title: 'Họ tên', dataIndex: 'displayName', key: 'displayName', render: v => <span className="font-medium">{v}</span>},
        {title: 'Email', dataIndex: 'email', key: 'email'},
        {title: 'Vai trò', dataIndex: 'role', key: 'role', width: 130,
            render: v => <Tag color={ROLE_COLOR[v] ?? 'default'}>{ROLE_LABEL[v] ?? v}</Tag>},
        {
            title: 'Khoá/Lớp', dataIndex: 'cohortLabel', key: 'cohortLabel', width: 140,
            render: v => v ?? <span className="text-gray-400">—</span>,
        },
        {
            title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 120,
            render: (v, record) => <StatusTag status={v ? 'success' : 'default'}
                label={v ? (record.kind === 'student' ? 'Đang học' : 'Hoạt động') : 'Tắt'}/>,
        },
        {
            title: 'Thao tác', key: 'actions', width: 140,
            // Học sinh không có mutation ở màn này — quản lý trong trang chi tiết khoá.
            render: (_, record) => record.kind !== 'staff' ? null : (
                <div className="flex gap-2">
                    <Button size="small" onClick={() => setActiveMutation.mutate({id: record.memberId, isActive: !record.isActive})}>
                        {record.isActive ? 'Tắt' : 'Bật'}
                    </Button>
                    <Popconfirm title="Xóa thành viên?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                        onConfirm={() => removeMemberMutation.mutate(record.memberId)}>
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    const tabItems = [
        {
            key: 'cohorts', label: 'Khoá học',
            children: (
                <div className="flex flex-col gap-4 p-4">
                    <div className="flex justify-end">
                        <Button type="primary" icon={<PlusOutlined/>} onClick={() => setCohortModal(true)}>
                            Thêm khoá học
                        </Button>
                    </div>
                    <Table columns={cohortColumns} dataSource={cohorts} rowKey="id" loading={fetchingCohorts} pagination={false} scroll={{x: 700}}/>
                </div>
            ),
        },
        {
            key: 'members', label: `Thành viên (${memberRows.length})`,
            children: (
                <div className="flex flex-col gap-4 p-4">
                    <div className="flex items-center gap-2 flex-wrap">
                        <Segmented value={memberKind} onChange={v => setMemberKind(v as 'all' | MemberKind)}
                            options={[
                                {value: 'all', label: 'Tất cả'},
                                {value: 'staff', label: 'Nhân sự'},
                                {value: 'student', label: 'Học sinh'},
                            ]}/>
                        <Input prefix={<SearchOutlined className="text-gray-400"/>} placeholder="Tìm theo tên/email..."
                            style={{width: 240}} allowClear value={memberKeyword}
                            onChange={e => setMemberKeyword(e.target.value)}/>
                        <Button type="primary" icon={<PlusOutlined/>} className="ml-auto"
                            onClick={() => setMemberModal(true)}>
                            Thêm thành viên
                        </Button>
                    </div>
                    <Table columns={memberColumns} dataSource={visibleMemberRows} rowKey="key"
                        loading={fetchingMembers || fetchingStudents}
                        pagination={{pageSize: 20, showSizeChanger: true}} scroll={{x: 700}}/>
                </div>
            ),
        },
    ]

    return (
        <>
            <PageHeader left={
                <Breadcrumb items={[
                    {title: <a onClick={() => navigate('/app/schools')}>Trường học</a>},
                    {title: school?.name ?? `Trường #${schoolId}`},
                ]}/>
            }/>

            <div className="flex-1 overflow-auto">
                <Tabs items={tabItems} className="category-tabs"
                    tabBarStyle={{paddingInline: 24, marginBottom: 0, background: '#fff'}}/>
            </div>

            {/* Modal thêm khoá học */}
            <Modal title="Thêm khoá học" open={cohortModal} onOk={handleAddCohort}
                onCancel={() => setCohortModal(false)} okText="Thêm" cancelText="Hủy"
                confirmLoading={createCohortMutation.isPending}>
                <Form form={cohortForm} layout="vertical">
                    <Form.Item name="name" label="Tên khoá" rules={[{required: true}]}><Input/></Form.Item>
                    <div className="flex gap-4">
                        <Form.Item name="startYear" label="Năm bắt đầu" rules={[{required: true}]} className="flex-1">
                            <Input type="number"/>
                        </Form.Item>
                        <Form.Item name="endYear" label="Năm kết thúc" rules={[{required: true}]} className="flex-1">
                            <Input type="number"/>
                        </Form.Item>
                    </div>
                    <div className="flex gap-4">
                        <Form.Item name="gradeStart" label="Lớp bắt đầu" rules={[{required: true}]} className="flex-1">
                            <Input type="number" placeholder="10"/>
                        </Form.Item>
                        <Form.Item name="numClasses" label="Số lớp" className="flex-1" initialValue={1}
                            rules={[{required: true}]}>
                            <Input type="number" min={1} max={26} placeholder="1"/>
                        </Form.Item>
                    </div>
                </Form>
            </Modal>

            {/* Modal thêm thành viên: thủ công nhiều người hoặc import Excel */}
            <SchoolMemberAddModal
                open={memberModal}
                schoolId={schoolId}
                users={allUsers}
                cohorts={cohorts}
                members={members}
                students={students}
                onClose={() => setMemberModal(false)}
            />
        </>
    )
}
