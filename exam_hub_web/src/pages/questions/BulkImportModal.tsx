import {useState} from 'react'
import {Alert, Button, Modal, Select, Upload} from 'antd'
import type {UploadFile} from 'antd'
import {InboxOutlined} from '@ant-design/icons'
import {useBulkImportMutation} from '../../hooks/queries/useQuestions'

type Props = {
    open: boolean
    onClose: () => void
    topics: Topic[]
    difficulties: DifficultyLevel[]
    cognitives: CognitiveLevel[]
}

export function BulkImportModal({open, onClose, topics, difficulties, cognitives}: Props) {
    const [fileList, setFileList] = useState<UploadFile[]>([])
    const [topicId, setTopicId] = useState<number>()
    const [difficultyId, setDifficultyId] = useState<number>()
    const [cognitiveId, setCognitiveId] = useState<number>()
    const [result, setResult] = useState<BulkImportResult | null>(null)
    const mutation = useBulkImportMutation()

    const file = fileList[0]?.originFileObj as File | undefined
    const canSubmit = !!file && !!topicId && !!difficultyId

    const reset = () => {
        setFileList([])
        setTopicId(undefined)
        setDifficultyId(undefined)
        setCognitiveId(undefined)
        setResult(null)
    }

    const handleClose = () => {
        reset()
        onClose()
    }

    const handleSubmit = async () => {
        if (!file || !topicId || !difficultyId) return
        const res = await mutation.mutateAsync({
            file,
            defaultTopicId: topicId,
            defaultDifficultyLevelId: difficultyId,
            defaultCognitiveLevelId: cognitiveId,
        })
        if (res.data) setResult(res.data)
    }

    return (
        <Modal
            title="Nhập câu hỏi từ Excel (.xlsx)"
            open={open}
            onCancel={handleClose}
            width={560}
            footer={[
                <Button key="cancel" onClick={handleClose}>Đóng</Button>,
                <Button
                    key="submit"
                    type="primary"
                    disabled={!canSubmit}
                    loading={mutation.isPending}
                    onClick={handleSubmit}
                >
                    Bắt đầu import
                </Button>,
            ]}
        >
            <div className="flex flex-col gap-3 mt-4">
                <div>
                    <label className="form-label">Chủ đề mặc định</label>
                    <Select
                        placeholder="Chọn chủ đề áp dụng cho các câu thiếu thông tin"
                        className="w-full"
                        value={topicId}
                        onChange={setTopicId}
                        showSearch
                        optionFilterProp="label"
                        options={topics.map(t => ({value: t.id, label: t.name}))}
                    />
                </div>
                <div>
                    <label className="form-label">Độ khó mặc định</label>
                    <Select
                        placeholder="Chọn độ khó mặc định"
                        className="w-full"
                        value={difficultyId}
                        onChange={setDifficultyId}
                        options={difficulties.map(d => ({value: d.id, label: d.name}))}
                    />
                </div>
                <div>
                    <label className="form-label">Cấp độ nhận thức mặc định (tuỳ chọn)</label>
                    <Select
                        placeholder="Không phân loại Bloom"
                        className="w-full"
                        allowClear
                        value={cognitiveId}
                        onChange={setCognitiveId}
                        options={cognitives.map(c => ({value: c.id, label: c.name}))}
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
                        message={`Đã import ${result.successCount} câu, ${result.errorCount} lỗi`}
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
