import {useState} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Breadcrumb, Button, Form, Modal, Popconfirm, Select, Table, Tabs, Tag} from 'antd'
import type {TableColumnsType} from 'antd'
import {PlusOutlined} from '@ant-design/icons'
import {useCohortClassesQuery, useSetHomeroomTeacherMutation} from '../../hooks/queries/useCohortClasses'
import {useCohortMembersQuery, useAddCohortMemberMutation, useRemoveCohortMemberMutation, useSetCohortMemberActiveMutation, useSetCohortMemberSectionMutation} from '../../hooks/queries/useCohortMembers'
import {statusCode} from '../../services/requestService'
import {userService} from '../../services/userService'
import {useQuery} from '@tanstack/react-query'

export default function CohortDetailPage() {
    const {id} = useParams<{id: string}>()
    const cohortId = Number(id)
    const navigate = useNavigate()

    const {data: classes = [], isFetching: fetchingClasses} = useCohortClassesQuery(cohortId)
    const {data: members = [], isFetching: fetchingMembers} = useCohortMembersQuery(cohortId)
    const setHomeroomMutation = useSetHomeroomTeacherMutation(cohortId)
    const addMemberMutation = useAddCohortMemberMutation(cohortId)
    const removeMemberMutation = useRemoveCohortMemberMutation(cohortId)
    const setActiveMutation = useSetCohortMemberActiveMutation(cohortId)
    const setSectionMutation = useSetCohortMemberSectionMutation(cohortId)

    const {data: allUsers = []} = useQuery({
        queryKey: ['users'],
        queryFn: async () => (await userService.getAll()).data ?? [],
    })

    const sections = [...new Set(classes.map(c => c.section))].sort()

    const [memberModal, setMemberModal] = useState(false)
    const [memberForm] = Form.useForm<CohortMemberBody>()

    const handleAddMember = async () => {
        const values = await memberForm.validateFields()
        const res = await addMemberMutation.mutateAsync({...values, cohortId})
        if (res.status !== statusCode.Error) { setMemberModal(false); memberForm.resetFields() }
    }

    const classColumns: TableColumnsType<CohortClass> = [
        {title: 'Lớp', dataIndex: 'className', key: 'className', render: v => <span className="font-medium">{v}</span>},
        {title: 'Lớp', dataIndex: 'section', key: 'section', width: 80},
        {title: 'Năm học', dataIndex: 'schoolYear', key: 'schoolYear'},
        {title: 'Năm học (index)', dataIndex: 'yearIndex', key: 'yearIndex'},
        {
            title: 'GVCN', dataIndex: 'homeroomTeacherId', key: 'homeroomTeacherId',
            render: (v) => {
                const teacher = allUsers.find(u => u.id === v)
                return teacher ? teacher.displayName ?? teacher.userName : <span className="text-gray-400">Chưa phân công</span>
            },
        },
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, record) => (
                <Select
                    style={{width: 180}}
                    allowClear
                    placeholder="Chọn GVCN"
                    value={record.homeroomTeacherId ?? undefined}
                    showSearch optionFilterProp="label"
                    options={allUsers.filter(u => u.roles.includes('Teacher')).map(u => ({value: u.id, label: u.displayName ?? u.userName}))}
                    onChange={(val) => setHomeroomMutation.mutate({id: record.id, body: {teacherId: val ?? null}})}
                />
            ),
        },
    ]

    const memberColumns: TableColumnsType<CohortMember> = [
        {
            title: 'Học sinh', dataIndex: 'studentId', key: 'studentId',
            render: (v) => {
                const user = allUsers.find(u => u.id === v)
                return user ? user.displayName ?? user.userName : <span className="font-mono text-xs">{v}</span>
            },
        },
        {
            title: 'Lớp', dataIndex: 'section', key: 'section', width: 130,
            render: (v, record) => (
                <Select
                    style={{width: 110}} allowClear placeholder="Chưa xếp"
                    value={v ?? undefined}
                    options={sections.map(s => ({value: s, label: s}))}
                    onChange={(val) => setSectionMutation.mutate({id: record.id, section: val ?? null})}
                />
            ),
        },
        {title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', render: v => <Tag color={v ? 'green' : 'default'}>{v ? 'Hoạt động' : 'Tắt'}</Tag>},
        {
            title: 'Thao tác', key: 'actions', width: 140,
            render: (_, record) => (
                <div className="flex gap-2">
                    <Button size="small" onClick={() => setActiveMutation.mutate({id: record.id, isActive: !record.isActive})}>
                        {record.isActive ? 'Tắt' : 'Bật'}
                    </Button>
                    <Popconfirm title="Xóa học sinh khỏi khoá?" okText="Xóa" cancelText="Hủy" okButtonProps={{danger: true}}
                        onConfirm={() => removeMemberMutation.mutate(record.id)}>
                        <button className="btn-delete">Xóa</button>
                    </Popconfirm>
                </div>
            ),
        },
    ]

    const tabItems = [
        {
            key: 'classes', label: 'Lớp học',
            children: (
                <div className="p-4">
                    <Table columns={classColumns} dataSource={classes} rowKey="id" loading={fetchingClasses} pagination={false}/>
                </div>
            ),
        },
        {
            key: 'students', label: 'Học sinh',
            children: (
                <div className="flex flex-col gap-4 p-4">
                    <div className="flex justify-end">
                        <Button type="primary" icon={<PlusOutlined/>} onClick={() => setMemberModal(true)}>
                            Thêm học sinh
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
                    {title: `Khoá #${cohortId}`},
                ]}/>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto">
                <Tabs items={tabItems} className="category-tabs"
                    tabBarStyle={{paddingInline: 24, marginBottom: 0, background: '#fff'}}/>
            </div>

            <Modal title="Thêm học sinh vào khoá" open={memberModal} onOk={handleAddMember}
                onCancel={() => setMemberModal(false)} okText="Thêm" cancelText="Hủy"
                confirmLoading={addMemberMutation.isPending}>
                <Form form={memberForm} layout="vertical">
                    <Form.Item name="studentId" label="Học sinh" rules={[{required: true}]}>
                        <Select showSearch optionFilterProp="label"
                            options={allUsers.filter(u => u.roles.includes('Student')).map(u => ({value: u.id, label: u.displayName ?? u.userName}))}/>
                    </Form.Item>
                    <Form.Item name="section" label="Lớp">
                        <Select allowClear placeholder="Chưa xếp lớp"
                            options={sections.map(s => ({value: s, label: s}))}/>
                    </Form.Item>
                </Form>
            </Modal>
        </>
    )
}
