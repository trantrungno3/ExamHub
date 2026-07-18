import {useEffect, useMemo} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Button, Checkbox, Form, Input, Select, Upload, message} from 'antd'
import {DeleteOutlined, PaperClipOutlined, PlusOutlined} from '@ant-design/icons'
import RichTextEditor from '../../components/RichTextEditor'
import {useMutation, useQueryClient} from '@tanstack/react-query'
import {questionService} from '../../services/questionService'
import {statusCode} from '../../services/requestService'
import {QUESTION_KEYS, useQuestionQuery} from '../../hooks/queries/useQuestions'
import {
    useCognitiveLevelsQuery,
    useDifficultyLevelsQuery,
    useGradeLevelsListQuery,
    useQuestionTypesQuery,
    useSubjectsQuery,
    useTopicsQuery,
} from '../../hooks/queries/useCategoryLists'

type AnswerForm = {content: string; isCorrect: boolean}
type QuestionForm = {
    gradeLevelId?: number
    subjectId?: number
    topicId?: number
    questionTypeId?: number
    difficultyLevelId?: number
    cognitiveLevelId?: number
    content: string
    explanation?: string
    source?: string
    tags?: string[]
    answers: AnswerForm[]
}

const EMPTY: QuestionForm = {
    content: '',
    answers: [{content: '', isCorrect: true}, {content: '', isCorrect: false}],
}

export default function AddQuestionPage() {
    const navigate = useNavigate()
    const {id} = useParams<{id: string}>()
    const isEdit = !!id
    const [form] = Form.useForm<QuestionForm>()
    const qc = useQueryClient()

    const topics = useTopicsQuery()
    const subjects = useSubjectsQuery()
    const grades = useGradeLevelsListQuery()
    const questionTypes = useQuestionTypesQuery()
    const difficulties = useDifficultyLevelsQuery()
    const cognitives = useCognitiveLevelsQuery()
    const {data: existing} = useQuestionQuery(id)

    /** Cascading: Cấp lớp → Môn học → Chủ đề. Mỗi select chỉ render danh sách đã lọc. */
    const gradeLevelId = Form.useWatch('gradeLevelId', form)
    const subjectId = Form.useWatch('subjectId', form)

    const gradeOptions = useMemo(
        () => [...(grades.data ?? [])]
            .sort((a, b) => a.gradeNumber - b.gradeNumber)
            .map(g => ({value: g.id, label: g.name})),
        [grades.data],
    )
    const subjectOptions = useMemo(
        () => (subjects.data ?? [])
            .filter(s => s.gradeLevelId === gradeLevelId)
            .sort((a, b) => a.name.localeCompare(b.name))
            .map(s => ({value: s.id, label: s.name})),
        [subjects.data, gradeLevelId],
    )
    const topicOptions = useMemo(
        () => (topics.data ?? [])
            .filter(t => t.subjectId === subjectId)
            .sort((a, b) => a.name.localeCompare(b.name))
            .map(t => ({value: t.id, label: t.name})),
        [topics.data, subjectId],
    )

    useEffect(() => {
        if (isEdit && existing) {
            form.setFieldsValue({
                topicId: existing.topicId,
                questionTypeId: existing.questionTypeId,
                difficultyLevelId: existing.difficultyLevelId,
                cognitiveLevelId: existing.cognitiveLevelId,
                content: existing.content,
                explanation: existing.explanation,
                source: existing.source,
                tags: existing.tags,
                answers: existing.answers?.map(a => ({content: a.content, isCorrect: a.isCorrect}))
                    ?? EMPTY.answers,
            })
        }
    }, [isEdit, existing, form])

    /** Khi sửa: suy ra Cấp lớp + Môn học từ topicId (cần topics/subjects đã tải). */
    useEffect(() => {
        if (!isEdit || !existing || !topics.data || !subjects.data) return
        const topic = topics.data.find(t => t.id === existing.topicId)
        const subject = topic ? subjects.data.find(s => s.id === topic.subjectId) : undefined
        form.setFieldsValue({subjectId: topic?.subjectId, gradeLevelId: subject?.gradeLevelId})
    }, [isEdit, existing, topics.data, subjects.data, form])

    const saveMutation = useMutation({
        mutationFn: (body: QuestionBody) =>
            isEdit ? questionService.update(id!, body) : questionService.create(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error || !res.data) {
                message.error(res.message || 'Lưu câu hỏi thất bại')
                return
            }
            message.success(isEdit ? 'Cập nhật câu hỏi thành công' : 'Tạo câu hỏi thành công')
            void qc.invalidateQueries({queryKey: QUESTION_KEYS.all})
            navigate('/app/questions')
        },
        onError: () => message.error('Lưu câu hỏi thất bại'),
    })

    const handleSubmit = async () => {
        const v = await form.validateFields()
        const body: QuestionBody = {
            topicId: v.topicId!,
            questionTypeId: v.questionTypeId!,
            difficultyLevelId: v.difficultyLevelId!,
            cognitiveLevelId: v.cognitiveLevelId,
            content: v.content,
            contentPlain: stripHtml(v.content),
            explanation: v.explanation,
            source: v.source,
            tags: v.tags,
            answers: v.answers.map(a => ({content: a.content, isCorrect: a.isCorrect})),
        }
        saveMutation.mutate(body)
    }

    const uploadAttachment = async (file: File) => {
        if (!id) return
        const res = await questionService.uploadAttachment(id, file)
        if (res.data?.url) message.success('Đã tải tệp đính kèm lên')
        else message.error(res.message || 'Tải tệp thất bại')
    }

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">{isEdit ? 'Sửa câu hỏi' : 'Thêm câu hỏi mới'}</p>
                    <p className="top-bar-subtitle">
                        <span className="text-blue-500 cursor-pointer hover:underline"
                              onClick={() => navigate('/app/questions')}>Câu hỏi</span>
                        {' / '}{isEdit ? 'Chỉnh sửa' : 'Thêm mới'}
                    </p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6">
                <Form form={form} layout="vertical" initialValues={EMPTY}>
                    <div className="flex gap-5 items-start">
                        {/* Left: content + answers */}
                        <div className="flex-1 flex flex-col gap-4 min-w-0">
                            <div className="form-section">
                                <p className="form-section-title">Nội dung câu hỏi</p>
                                <Form.Item name="content" rules={[{required: true, message: 'Nhập nội dung câu hỏi'}]}>
                                    <RichTextEditor
                                        placeholder="Nhập nội dung câu hỏi..."
                                        minHeight={120}
                                    />
                                </Form.Item>
                                <p className="form-section-title">Giải thích đáp án</p>
                                <Form.Item name="explanation">
                                    <RichTextEditor
                                        placeholder="Giải thích đáp án (tuỳ chọn)..."
                                        minHeight={80}
                                    />
                                </Form.Item>
                            </div>

                            <div className="form-section">
                                <p className="form-section-title">Đáp án</p>
                                <Form.List
                                    name="answers"
                                    rules={[{
                                        validator: async (_, answers: AnswerForm[]) => {
                                            if (!answers || answers.length < 2)
                                                return Promise.reject(new Error('Cần ít nhất 2 đáp án'))
                                            if (!answers.some(a => a?.isCorrect))
                                                return Promise.reject(new Error('Cần ít nhất 1 đáp án đúng'))
                                        },
                                    }]}
                                >
                                    {(fields, {add, remove}, {errors}) => (
                                        <div className="flex flex-col gap-2">
                                            {fields.map(field => (
                                                <div key={field.key} className="flex items-center gap-2">
                                                    <Form.Item
                                                        name={[field.name, 'isCorrect']}
                                                        valuePropName="checked"
                                                        noStyle
                                                    >
                                                        <Checkbox/>
                                                    </Form.Item>
                                                    <Form.Item
                                                        name={[field.name, 'content']}
                                                        className="flex-1 !mb-0"
                                                        rules={[{required: true, message: 'Nhập nội dung đáp án'}]}
                                                    >
                                                        <Input placeholder="Nội dung đáp án"/>
                                                    </Form.Item>
                                                    {fields.length > 2 && (
                                                        <Button type="text" danger icon={<DeleteOutlined/>}
                                                                onClick={() => remove(field.name)}/>
                                                    )}
                                                </div>
                                            ))}
                                            <Button type="dashed" icon={<PlusOutlined/>}
                                                    onClick={() => add({content: '', isCorrect: false})}>
                                                Thêm đáp án
                                            </Button>
                                            <Form.ErrorList errors={errors}/>
                                        </div>
                                    )}
                                </Form.List>
                            </div>

                            {isEdit && (
                                <div className="form-section">
                                    <p className="form-section-title">Tệp đính kèm (ảnh / PDF, ≤ 10 MB)</p>
                                    <Upload
                                        accept="image/*,application/pdf"
                                        maxCount={1}
                                        showUploadList={false}
                                        beforeUpload={(file) => {
                                            void uploadAttachment(file as unknown as File)
                                            return false
                                        }}
                                    >
                                        <Button icon={<PaperClipOutlined/>}>Đính kèm tệp</Button>
                                    </Upload>
                                </div>
                            )}
                        </div>

                        {/* Right: classification */}
                        <div className="w-72 flex flex-col gap-4 shrink-0">
                            <div className="form-section">
                                <p className="form-section-title">Phân loại câu hỏi</p>
                                <Form.Item label="Cấp lớp" name="gradeLevelId" rules={[{required: true, message: 'Chọn cấp lớp'}]}>
                                    <Select
                                        placeholder="Chọn cấp lớp"
                                        showSearch optionFilterProp="label"
                                        options={gradeOptions}
                                        onChange={() => form.setFieldsValue({subjectId: undefined, topicId: undefined})}
                                    />
                                </Form.Item>
                                <Form.Item label="Môn học" name="subjectId" rules={[{required: true, message: 'Chọn môn học'}]}>
                                    <Select
                                        placeholder={gradeLevelId ? 'Chọn môn học' : 'Chọn cấp lớp trước'}
                                        disabled={!gradeLevelId}
                                        showSearch optionFilterProp="label"
                                        options={subjectOptions}
                                        onChange={() => form.setFieldsValue({topicId: undefined})}
                                    />
                                </Form.Item>
                                <Form.Item label="Chủ đề" name="topicId" rules={[{required: true, message: 'Chọn chủ đề'}]}>
                                    <Select
                                        placeholder={subjectId ? 'Chọn chủ đề' : 'Chọn môn học trước'}
                                        disabled={!subjectId}
                                        showSearch optionFilterProp="label"
                                        options={topicOptions}
                                    />
                                </Form.Item>
                                <Form.Item label="Loại câu hỏi" name="questionTypeId" rules={[{required: true, message: 'Chọn loại'}]}>
                                    <Select placeholder="Chọn loại"
                                            options={(questionTypes.data ?? []).map(t => ({value: t.id, label: t.name}))}/>
                                </Form.Item>
                                <Form.Item label="Độ khó" name="difficultyLevelId" rules={[{required: true, message: 'Chọn độ khó'}]}>
                                    <Select placeholder="Chọn độ khó"
                                            options={(difficulties.data ?? []).map(d => ({value: d.id, label: d.name}))}/>
                                </Form.Item>
                                <Form.Item label="Cấp độ nhận thức (Bloom)" name="cognitiveLevelId">
                                    <Select placeholder="Chưa phân loại" allowClear
                                            options={(cognitives.data ?? []).map(c => ({value: c.id, label: c.name}))}/>
                                </Form.Item>
                            </div>

                            <div className="form-section">
                                <p className="form-section-title">Khác</p>
                                <Form.Item label="Nguồn" name="source">
                                    <Input placeholder="VD: SGK Toán 10, trang 45"/>
                                </Form.Item>
                                <Form.Item label="Tags" name="tags">
                                    <Select mode="tags" placeholder="Thêm từ khoá..." tokenSeparators={[',']}/>
                                </Form.Item>
                            </div>
                        </div>
                    </div>
                </Form>
            </div>

            <div className="action-bar">
                <Button onClick={() => navigate('/app/questions')}>Hủy bỏ</Button>
                <Button type="primary" loading={saveMutation.isPending} onClick={handleSubmit}>
                    {isEdit ? 'Cập nhật' : 'Lưu câu hỏi'}
                </Button>
            </div>
        </>
    )
}

function stripHtml(html: string): string {
    return html.replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim()
}
