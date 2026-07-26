import {useState} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Breadcrumb, Button, Form, Input, Modal, Popconfirm, Select, Table, Tabs, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined, RightOutlined} from '@ant-design/icons'
import {useSchoolsQuery} from '../../hooks/queries/useSchools'
import {useCohortsQuery, useCreateCohortMutation, useDeleteCohortMutation} from '../../hooks/queries/useCohorts'
import {useSchoolMembersQuery, useAddSchoolMemberMutation, useRemoveSchoolMemberMutation, useSetSchoolMemberActiveMutation} from '../../hooks/queries/useSchoolMembers'
import {statusCode} from '../../services/requestService'
import {userService} from '../../services/userService'
import {useQuery} from '@tanstack/react-query'

export default function SchoolDetailPage() {
    const {id} = useParams<{id: string}>()
    const schoolId = Number(id)
    const navigate = useNavigate()

    const {data: schools = []} = useSchoolsQuery()
    const school = schools.find(s => s.id === schoolId)

    const {data: cohorts = [], isFetching: fetchingCohorts} = useCohortsQuery(schoolId)
    const {data: members = [], isFetching: fetchingMembers} = useSchoolMembersQuery(schoolId)

    const createCohortMutation = useCreateCohortMutation(schoolId)
    const deleteCohortMutation = useDeleteCohortMutation(schoolId)
    const addMemberMutation = useAddSchoolMemberMutation(schoolId)
    const removeMemberMutation = useRemoveSchoolMemberMutation(schoolId)
    const setActiveMutation = useSetSchoolMemberActiveMutation(schoolId)

    const {data: allUsers = []} = useQuery({
        queryKey: ['users'],
        queryFn: async () => (await userService.getAll()).data ?? [],
    })

    const [cohortModal, setCohortModal] = useState(false)
    const [memberModal, setMemberModal] = useState(false)
    const [cohortForm] = Form.useForm<CohortBody>()
    const [memberForm] = Form.useForm<SchoolMemberBody>()

    const handleAddCohort = async () => {
        const values = await cohortForm.validateFields()
        const res = await createCohortMutation.mutateAsync({...values, schoolId})
        if (res.status !== statusCode.Error) { setCohortModal(false); cohortForm.resetFields() }
    }

    const handleAddMember = async () => {
        const values = await memberForm.validateFields()
        const res = await addMemberMutation.mutateAsync({...values, schoolId})
        if (res.status !== statusCode.Error) { setMemberModal(false); memberForm.resetFields() }
    }

    const cohortColumns: TableColumnsType<Cohort> = [
        {title: 'Tên khoá', dataIndex: 'name', key: 'name', render: v => <span className="font-medium">{v}</span>},
        {title: 'Năm bắt đầu', dataIndex: 'startYear', key: 'startYear'},
        {title: 'Năm kết thúc', dataIndex: 'endYear', key: 'endYear'},
        {title: 'Lớp bắt đầu', dataIndex: 'gradeStart', key: 'gradeStart'},
        {title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>},
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, record) => (
                <div className="flex gap-2">
                    <Popconfirm title="Xóa khoá học này?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                        onConfirm={() => deleteCohortMutation.mutate(record.id)}>
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                    <Button size="small" icon={<RightOutlined/>} onClick={() => navigate(`/app/cohorts/${record.id}`)}>
                        Chi tiết
                    </Button>
                </div>
            ),
        },
    ]

    const memberColumns: TableColumnsType<SchoolMember> = [
        {title: 'User ID', dataIndex: 'userId', key: 'userId', render: v => <span className="font-mono text-xs">{v}</span>},
        {title: 'Vai trò', dataIndex: 'role', key: 'role', render: v => <Tag>{v}</Tag>},
        {title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>},
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, record) => (
                <div className="flex gap-2">
                    <Button size="small" onClick={() => setActiveMutation.mutate({id: record.id, isActive: !record.isActive})}>
                        {record.isActive ? 'Tắt' : 'Bật'}
                    </Button>
                    <Popconfirm title="Xóa thành viên?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                        onConfirm={() => removeMemberMutation.mutate(record.id)}>
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
                    <Table columns={cohortColumns} dataSource={cohorts} rowKey="id" loading={fetchingCohorts} pagination={false}/>
                </div>
            ),
        },
        {
            key: 'members', label: 'Thành viên trường',
            children: (
                <div className="flex flex-col gap-4 p-4">
                    <div className="flex justify-end">
                        <Button type="primary" icon={<PlusOutlined/>} onClick={() => setMemberModal(true)}>
                            Thêm thành viên
                        </Button>
                    </div>
                    <Table columns={memberColumns} dataSource={members} rowKey="id" loading={fetchingMembers} pagination={false}/>
                </div>
            ),
        },
    ]

    return (
        <>
            <div className="top-bar">
                <Breadcrumb items={[
                    {title: <a onClick={() => navigate('/app/schools')}>Trường học</a>},
                    {title: school?.name ?? `Trường #${schoolId}`},
                ]}/>
                <div className="top-bar-avatar">TT</div>
            </div>

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

            {/* Modal thêm thành viên */}
            <Modal title="Thêm thành viên trường" open={memberModal} onOk={handleAddMember}
                onCancel={() => setMemberModal(false)} okText="Thêm" cancelText="Hủy"
                confirmLoading={addMemberMutation.isPending}>
                <Form form={memberForm} layout="vertical">
                    <Form.Item name="userId" label="Người dùng" rules={[{required: true}]}>
                        <Select showSearch optionFilterProp="label"
                            options={allUsers.map(u => ({value: u.id, label: `${u.displayName ?? u.userName} (${u.roles.join(', ')})`}))}/>
                    </Form.Item>
                    <Form.Item name="role" label="Vai trò" rules={[{required: true}]}>
                        <Select options={[{value: 'Admin', label: 'Admin'}, {value: 'Teacher', label: 'Teacher'}]}/>
                    </Form.Item>
                </Form>
            </Modal>
        </>
    )
}
