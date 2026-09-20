import {useState} from 'react'
import {Alert, Button, Modal, Select, Tabs} from 'antd'
import {useBulkAddSchoolMembersMutation} from '../../hooks/queries/useSchoolMembers'
import {eligibleSchoolUsers, sectionsForCohort} from './schoolMemberAddOptions'

type Props = Readonly<{
    open: boolean
    schoolId: number
    users: UserResponse[]
    cohorts: Cohort[]
    members: SchoolMember[]
    students: CohortMember[]
    onClose: () => void
}>

export default function SchoolMemberAddModal({
    open, schoolId, users, cohorts, members, students, onClose,
}: Props) {
    const [role, setRole] = useState<SchoolMembershipRole>('Teacher')
    const [userIds, setUserIds] = useState<string[]>([])
    const [cohortId, setCohortId] = useState<number>()
    const [section, setSection] = useState<string>()
    const [manualResult, setManualResult] = useState<SchoolMemberBulkResult>()
    const bulkAdd = useBulkAddSchoolMembersMutation(schoolId)

    const candidates = eligibleSchoolUsers(role, users, members, students)
    const selectedCohort = cohorts.find(x => x.id === cohortId)
    const userOptions = candidates.map(user => ({
        value: user.id,
        label: `${user.displayName} (${user.userName ?? 'không có tên đăng nhập'})`,
    }))
    const cohortOptions = cohorts
        .filter(cohort => cohort.isActive)
        .map(cohort => ({value: cohort.id, label: cohort.name}))
    const sectionOptions = sectionsForCohort(selectedCohort)
        .map(value => ({value, label: value}))
    const canSubmitManual = userIds.length > 0 &&
        (role !== 'Student' || (cohortId !== undefined && section !== undefined))

    async function submitManual() {
        const response = await bulkAdd.mutateAsync({
            schoolId,
            role,
            userIds,
            cohortId: role === 'Student' ? cohortId : undefined,
            section: role === 'Student' ? section : undefined,
        })
        if (response.data) setManualResult(response.data)
    }

    function changeRole(value: SchoolMembershipRole) {
        setRole(value)
        setUserIds([])
        setCohortId(undefined)
        setSection(undefined)
        setManualResult(undefined)
    }

    function changeCohort(value?: number) {
        setCohortId(value)
        setSection(undefined)
    }

    function reset() {
        setRole('Teacher')
        setUserIds([])
        setCohortId(undefined)
        setSection(undefined)
        setManualResult(undefined)
    }

    function close() {
        reset()
        onClose()
    }

    const manualTab = (
        <div className="flex flex-col gap-4">
            <label className="flex flex-col gap-1">
                <span className="text-sm font-medium">Vai trò</span>
                <Select<SchoolMembershipRole>
                    value={role}
                    onChange={changeRole}
                    options={[
                        {value: 'Admin', label: 'Admin'},
                        {value: 'Teacher', label: 'Teacher'},
                        {value: 'Student', label: 'Student'},
                    ]}
                />
            </label>

            <label className="flex flex-col gap-1">
                <span className="text-sm font-medium">Người dùng</span>
                <Select
                    mode="multiple"
                    showSearch
                    optionFilterProp="label"
                    placeholder="Chọn một hoặc nhiều tài khoản"
                    value={userIds}
                    onChange={setUserIds}
                    options={userOptions}
                />
            </label>

            {role === 'Student' && (
                <>
                    <label className="flex flex-col gap-1">
                        <span className="text-sm font-medium">Khoá học</span>
                        <Select
                            placeholder="Chọn khoá học đang hoạt động"
                            value={cohortId}
                            onChange={changeCohort}
                            options={cohortOptions}
                        />
                    </label>
                    <label className="flex flex-col gap-1">
                        <span className="text-sm font-medium">Lớp</span>
                        <Select
                            placeholder="Chọn lớp"
                            value={section}
                            onChange={setSection}
                            disabled={!selectedCohort}
                            options={sectionOptions}
                        />
                    </label>
                </>
            )}

            <Button
                type="primary"
                disabled={!canSubmitManual}
                loading={bulkAdd.isPending}
                onClick={submitManual}
            >
                Thêm {userIds.length > 0 ? `${userIds.length} tài khoản` : ''}
            </Button>

            {manualResult && (
                <Alert
                    type={manualResult.errorCount ? 'warning' : 'success'}
                    message={`Đã thêm ${manualResult.successCount} tài khoản, ${manualResult.errorCount} lỗi`}
                    description={manualResult.errors.map(error => (
                        <div key={error.rowNumber}>Lựa chọn {error.rowNumber}: {error.message}</div>
                    ))}
                />
            )}
        </div>
    )

    return (
        <Modal
            title="Thêm thành viên trường"
            open={open}
            onCancel={close}
            footer={null}
            width={760}
            destroyOnHidden
        >
            <Tabs
                items={[
                    {key: 'manual', label: 'Thêm thủ công', children: manualTab},
                    {key: 'excel', label: 'Import Excel', children: null},
                ]}
            />
        </Modal>
    )
}
