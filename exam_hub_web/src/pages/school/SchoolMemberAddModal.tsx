import {useState} from 'react'
import {Alert, App, Button, Modal, Select, Table, Tabs, Tag, Upload} from 'antd'
import type {UploadFile} from 'antd'
import {InboxOutlined} from '@ant-design/icons'
import {
    useBulkAddSchoolMembersMutation,
    useImportSchoolMembersMutation,
    usePreviewSchoolMembersMutation,
} from '../../hooks/queries/useSchoolMembers'
import {schoolMemberService} from '../../services/schoolMemberService'
import {ROLE_LABEL} from '../../constants'
import {eligibleSchoolUsers, sectionsForCohort} from './schoolMemberAddOptions'
import {
    canImportSchoolMemberPreview,
    validateSchoolMemberImportFile,
} from './schoolMemberImportFile'

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
    const [fileList, setFileList] = useState<UploadFile[]>([])
    const [preview, setPreview] = useState<SchoolMemberImportPreview>()
    const [importResult, setImportResult] = useState<SchoolMemberBulkResult>()
    const {message} = App.useApp()
    const bulkAdd = useBulkAddSchoolMembersMutation(schoolId)
    const previewImport = usePreviewSchoolMembersMutation()
    const importMembers = useImportSchoolMembersMutation(schoolId)
    const file = fileList[0]?.originFileObj as File | undefined

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

    /** Đổi file thì preview và kết quả cũ không còn nói về file đang chọn. */
    function changeFile(next: UploadFile[]) {
        setFileList(next.slice(-1))
        setPreview(undefined)
        setImportResult(undefined)
    }

    async function downloadTemplate() {
        try {
            const blob = await schoolMemberService.downloadImportTemplate()
            const url = URL.createObjectURL(blob)
            const anchor = document.createElement('a')
            anchor.href = url
            anchor.download = 'school-member-import-template.xlsx'
            document.body.appendChild(anchor)
            anchor.click()
            anchor.remove()
            setTimeout(() => URL.revokeObjectURL(url), 1000)
        } catch {
            message.error('Không thể tải file mẫu')
        }
    }

    async function checkFile() {
        const error = validateSchoolMemberImportFile(file)
        if (error) return void message.error(error)
        const response = await previewImport.mutateAsync({schoolId, file: file!})
        if (!response.data) return void message.error(response.message || 'Không thể kiểm tra file')
        setPreview(response.data)
        setImportResult(undefined)
    }

    /** Xoá preview sau khi import để trạng thái hợp lệ cũ không kích hoạt lần import thứ hai. */
    async function importValidRows() {
        const error = validateSchoolMemberImportFile(file)
        if (error) return void message.error(error)
        const response = await importMembers.mutateAsync(file!)
        if (!response.data) return void message.error(response.message || 'Import thất bại')
        setImportResult(response.data)
        setPreview(undefined)
    }

    function reset() {
        setRole('Teacher')
        setUserIds([])
        setCohortId(undefined)
        setSection(undefined)
        setManualResult(undefined)
        setFileList([])
        setPreview(undefined)
        setImportResult(undefined)
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
                        {value: 'Admin', label: ROLE_LABEL.Admin},
                        {value: 'Teacher', label: ROLE_LABEL.Teacher},
                        {value: 'Student', label: ROLE_LABEL.Student},
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

    const excelTab = (
        <div className="flex flex-col gap-4">
            <Alert
                type="info"
                message="Cột bắt buộc theo đúng thứ tự: UserName, Role, CohortName, Section"
                description="Chỉ nhận Teacher và Student. Teacher để trống CohortName và Section; Student bắt buộc cả hai."
            />

            <div>
                <Button onClick={downloadTemplate}>Tải file mẫu</Button>
            </div>

            <Upload.Dragger
                accept=".xlsx"
                maxCount={1}
                fileList={fileList}
                beforeUpload={() => false}
                onChange={({fileList: next}) => changeFile(next)}
            >
                <p className="ant-upload-drag-icon"><InboxOutlined/></p>
                <p className="ant-upload-text">Kéo thả hoặc bấm để chọn file .xlsx</p>
                <p className="ant-upload-hint">Tối đa 10 MB, một file mỗi lần.</p>
            </Upload.Dragger>

            <div>
                <Button onClick={checkFile} loading={previewImport.isPending} disabled={!file}>
                    Kiểm tra dữ liệu
                </Button>
            </div>

            {preview && (
                <>
                    <Alert
                        type={preview.errorCount ? 'warning' : 'success'}
                        message={`${preview.validCount} dòng hợp lệ, ${preview.errorCount} dòng lỗi`}
                    />
                    <Table<SchoolMemberImportRowResult>
                        size="small"
                        rowKey="rowNumber"
                        pagination={false}
                        scroll={{x: 700, y: 320}}
                        dataSource={preview.rows}
                        columns={[
                            {title: 'Dòng', dataIndex: 'rowNumber'},
                            {title: 'Tên đăng nhập', dataIndex: 'userName'},
                            {title: 'Vai trò', dataIndex: 'role', render: v => ROLE_LABEL[v] ?? v},
                            {title: 'Khoá', dataIndex: 'cohortName'},
                            {title: 'Lớp', dataIndex: 'section'},
                            {
                                title: 'Trạng thái',
                                render: (_, row) => (
                                    <Tag color={row.isValid ? 'success' : 'error'}>
                                        {row.isValid ? 'Hợp lệ' : 'Lỗi'}
                                    </Tag>
                                ),
                            },
                            {title: 'Chi tiết', render: (_, row) => row.errors.join('; ')},
                        ]}
                    />
                    <div>
                        <Button
                            type="primary"
                            disabled={!canImportSchoolMemberPreview(preview)}
                            loading={importMembers.isPending}
                            onClick={importValidRows}
                        >
                            Import dòng hợp lệ
                        </Button>
                    </div>
                </>
            )}

            {importResult && (
                <Alert
                    type={importResult.errorCount ? 'warning' : 'success'}
                    message={`Đã import ${importResult.successCount} dòng, ${importResult.errorCount} lỗi`}
                    description={importResult.errors.map(error => (
                        <div key={error.rowNumber}>Dòng {error.rowNumber}: {error.message}</div>
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
                    {key: 'excel', label: 'Import Excel', children: excelTab},
                ]}
            />
        </Modal>
    )
}
