import {useState} from 'react'
import {Alert, Button, Input, Modal, Upload, message} from 'antd'
import type {UploadFile} from 'antd'
import {DownloadOutlined, InboxOutlined} from '@ant-design/icons'
import {userService} from '../../services/userService'

type Props = {
    open: boolean
    onClose: () => void
    onImported: () => void
}

export function UserBulkImportModal({open, onClose, onImported}: Props) {
    const [fileList, setFileList] = useState<UploadFile[]>([])
    const [password, setPassword] = useState('')
    const [result, setResult] = useState<BulkImportResult | null>(null)
    const [submitting, setSubmitting] = useState(false)

    const file = fileList[0]?.originFileObj as File | undefined
    const canSubmit = !!file && password.trim().length > 0

    const reset = () => {
        setFileList([])
        setPassword('')
        setResult(null)
    }

    const handleClose = () => {
        reset()
        onClose()
    }

    const handleDownloadTemplate = async () => {
        try {
            const blob = await userService.downloadTemplate()
            const url = URL.createObjectURL(blob)
            const a = document.createElement('a')
            a.href = url
            a.download = 'user-import-template.xlsx'
            a.click()
            URL.revokeObjectURL(url)
        } catch {
            message.error('Không thể tải file mẫu')
        }
    }

    const handleSubmit = async () => {
        if (!file || !password.trim()) return
        setSubmitting(true)
        try {
            const res = await userService.bulkImport(file, password.trim())
            if (res.data) {
                setResult(res.data)
                if (res.data.successCount > 0) onImported()
            } else {
                message.error(res.message || 'Import thất bại')
            }
        } catch {
            message.error('Có lỗi xảy ra khi import')
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <Modal
            title="Nhập người dùng từ Excel (.xlsx)"
            open={open}
            onCancel={handleClose}
            width={560}
            footer={[
                <Button key="cancel" onClick={handleClose}>Đóng</Button>,
                <Button
                    key="submit"
                    type="primary"
                    disabled={!canSubmit}
                    loading={submitting}
                    onClick={handleSubmit}
                >
                    Bắt đầu import
                </Button>,
            ]}
        >
            <div className="flex flex-col gap-3 mt-4">
                <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-500">
                        Cột: UserName, DisplayName, Email, PhoneNumber, Sex, Role
                    </span>
                    <Button size="small" icon={<DownloadOutlined/>} onClick={handleDownloadTemplate}>
                        Tải file mẫu
                    </Button>
                </div>

                <div>
                    <label className="form-label">Mật khẩu mặc định (áp cho mọi tài khoản)</label>
                    <Input.Password
                        placeholder="Nhập mật khẩu mặc định"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                    />
                </div>

                <Upload.Dragger
                    accept=".xlsx"
                    maxCount={1}
                    fileList={fileList}
                    beforeUpload={() => false}
                    onChange={({fileList: fl}) => setFileList(fl.slice(-1))}
                >
                    <p className="ant-upload-drag-icon"><InboxOutlined/></p>
                    <p className="ant-upload-text">Kéo thả hoặc bấm để chọn file .xlsx</p>
                </Upload.Dragger>

                {result && (
                    <Alert
                        type={result.errorCount > 0 ? 'warning' : 'success'}
                        showIcon
                        message={`Đã tạo ${result.successCount} tài khoản, ${result.errorCount} lỗi`}
                        description={result.errors.length > 0 && (
                            <ul className="list-disc pl-4 max-h-40 overflow-auto">
                                {result.errors.map((e, i) => (
                                    <li key={i}>Dòng {e.rowNumber}: {e.message}</li>
                                ))}
                            </ul>
                        )}
                    />
                )}
            </div>
        </Modal>
    )
}
