import {useEffect, useMemo} from 'react'
import {useNavigate, useParams} from 'react-router-dom'
import {Button, Form, Input, InputNumber, message, Select, Switch} from 'antd'
import {CloseOutlined, PlusOutlined} from '@ant-design/icons'
import {useMutation, useQueryClient} from '@tanstack/react-query'
import {examTemplateService} from '../../services/examTemplateService'
import {statusCode} from '../../services/requestService'
import {EXAM_TEMPLATE_KEYS, useExamTemplateQuery} from '../../hooks/queries/useExamTemplates'
import {
    useCognitiveLevelsQuery,
    useGradeLevelsListQuery,
    useQuestionTypesQuery,
    useSubjectsQuery,
    useTopicsQuery,
} from '../../hooks/queries/useCategoryLists'

const EMPTY_SECTION: ExamTemplateSectionBody = {
    sectionName: '',
    topicId: undefined,
    questionTypeId: undefined,
    cognitiveLevelId: undefined,
    questionCount: 10,
    scorePerQuestion: 0.25,
    pctEasy: 40,
    pctMedium: 30,
    pctHard: 20,
    pctVeryHard: 10,
}

const EMPTY: ExamTemplateBody = {
    gradeLevelId: 0,
    subjectId: 0,
    title: '',
    durationMinutes: 45,
    totalScore: 10,
    shuffleQuestions: true,
    shuffleAnswers: false,
    preventDuplicate: true,
    isActive: true,
    sections: [{...EMPTY_SECTION}],
}

export default function CreateExamTemplatePage() {
    const navigate = useNavigate()
    const {id} = useParams<{ id: string }>()
    const isEdit = !!id
    const [form] = Form.useForm<ExamTemplateBody>()
    const qc = useQueryClient()

    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()
    const topics = useTopicsQuery()
    const questionTypes = useQuestionTypesQuery()
    const cognitives = useCognitiveLevelsQuery()
    const {data: existing} = useExamTemplateQuery(id)

    // Theo dõi lớp / môn / các phần để lọc chủ đề và kiểm tra tổng số câu.
    const watchedGradeId = Form.useWatch('gradeLevelId', form)
    const watchedSubjectId = Form.useWatch('subjectId', form)
    const watchedSections = Form.useWatch('sections', form)

    // Chủ đề lọc theo lớp + môn đã chọn ở phần thông tin mẫu đề.
    const topicOptions = useMemo(() => {
        const subjectById = new Map((subjects.data ?? []).map(s => [s.id, s]))
        return (topics.data ?? [])
            .filter(t => {
                if (!watchedSubjectId && !watchedGradeId) return false
                if (watchedSubjectId && t.subjectId !== watchedSubjectId) return false
                if (watchedGradeId && subjectById.get(t.subjectId)?.gradeLevelId !== watchedGradeId) return false
                return true
            })
            .map(t => ({value: t.id, label: t.name}))
    }, [topics.data, subjects.data, watchedSubjectId, watchedGradeId])

    // Tổng số câu cộng dồn từ các phần thi.
    const sectionsTotal = (watchedSections ?? [] as ExamTemplateSectionBody[])
        .reduce((acc: number, s) => acc + (Number(s?.questionCount) || 0), 0)

    // "Tổng số câu" luôn tự động bằng tổng số câu các phần.
    useEffect(() => {
        if (form.getFieldValue('totalQuestions') !== sectionsTotal)
            form.setFieldValue('totalQuestions', sectionsTotal)
    }, [sectionsTotal, form])

    useEffect(() => {
        if (isEdit && existing) {
            form.setFieldsValue({
                gradeLevelId: existing.gradeLevelId,
                subjectId: existing.subjectId,
                title: existing.title,
                description: existing.description,
                durationMinutes: existing.durationMinutes,
                totalQuestions: existing.totalQuestions,
                totalScore: existing.totalScore,
                shuffleQuestions: existing.shuffleQuestions,
                shuffleAnswers: existing.shuffleAnswers,
                preventDuplicate: existing.preventDuplicate,
                instructions: existing.instructions,
                isActive: existing.isActive,
                sections: existing.sections?.map(s => ({
                    sectionName: s.sectionName,
                    topicId: s.topicId,
                    questionTypeId: s.questionTypeId,
                    cognitiveLevelId: s.cognitiveLevelId,
                    questionCount: s.questionCount,
                    scorePerQuestion: s.scorePerQuestion,
                    pctEasy: s.pctEasy,
                    pctMedium: s.pctMedium,
                    pctHard: s.pctHard,
                    pctVeryHard: s.pctVeryHard,
                })) ?? [{...EMPTY_SECTION}],
            })
        }
    }, [isEdit, existing, form])

    const saveMutation = useMutation({
        mutationFn: (body: ExamTemplateBody) =>
            isEdit ? examTemplateService.update(id!, body) : examTemplateService.create(body),
        onSuccess: (res) => {
            if (res.status === statusCode.Error || !res.data) {
                message.error(res.message || 'Lưu mẫu đề thất bại')
                return
            }
            message.success(isEdit ? 'Cập nhật mẫu đề thành công' : 'Tạo mẫu đề thành công')
            void qc.invalidateQueries({queryKey: EXAM_TEMPLATE_KEYS.all})
            navigate('/app/exams')
        },
        onError: () => message.error('Lưu mẫu đề thất bại'),
    })

    const handleSubmit = async () => {
        const v = await form.validateFields()
        saveMutation.mutate(v)
    }

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">{isEdit ? 'Sửa mẫu đề thi' : 'Tạo mẫu đề thi mới'}</p>
                    <p className="top-bar-subtitle">
                        <span className="text-blue-500 cursor-pointer hover:underline"
                              onClick={() => navigate('/app/exams')}>Mẫu đề thi</span>
                        {' / '}{isEdit ? 'Chỉnh sửa' : 'Tạo mới'}
                    </p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto p-6">
                <Form
                    form={form}
                    layout="vertical"
                    initialValues={EMPTY}
                    onValuesChange={changed => {
                        // Đổi lớp / môn -> bỏ chủ đề đã chọn ở các phần vì không còn hợp lệ.
                        if ('gradeLevelId' in changed || 'subjectId' in changed) {
                            const sections = (form.getFieldValue('sections') ?? []) as ExamTemplateSectionBody[]
                            if (sections.some(s => s?.topicId != null))
                                form.setFieldValue('sections', sections.map(s => ({...s, topicId: undefined})))
                        }
                    }}
                >

                    <div className="flex gap-5 items-start">
                        {/* Left: template info */}
                        <div className="flex-[2] flex flex-col gap-4 min-w-0">
                            <div className="form-section">
                                <p className="form-section-title">Thông tin mẫu đề</p>
                                <Form.Item label="Tên đề" name="title"
                                           rules={[{required: true, message: 'Nhập tên đề'}]}>
                                    <Input placeholder="VD: Kiểm tra Toán HK1 Lớp 10"/>
                                </Form.Item>
                                <div className="grid grid-cols-2 gap-4">
                                    <Form.Item label="Lớp" name="gradeLevelId"
                                               rules={[{required: true, message: 'Chọn lớp'}]}>
                                        <Select placeholder="Chọn lớp"
                                                options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                                    </Form.Item>
                                    <Form.Item label="Môn học" name="subjectId"
                                               rules={[{required: true, message: 'Chọn môn'}]}>
                                        <Select placeholder="Chọn môn" showSearch optionFilterProp="label"
                                                options={(subjects.data ?? []).map(s => ({
                                                    value: s.id,
                                                    label: s.name
                                                }))}/>
                                    </Form.Item>
                                </div>
                                <div className="grid grid-cols-3 gap-4">
                                    <Form.Item label="Thời gian (phút)" name="durationMinutes"
                                               rules={[{required: true}]}>
                                        <InputNumber min={1} className="w-full"/>
                                    </Form.Item>
                                    <Form.Item
                                        label="Tổng số câu"
                                        name="totalQuestions"
                                        help="Tự động theo tổng số câu các phần"
                                    >
                                        <InputNumber min={0} className="w-full" readOnly/>
                                    </Form.Item>
                                    <Form.Item label="Tổng điểm" name="totalScore" rules={[{required: true}]}>
                                        <InputNumber min={0} step={0.5} className="w-full"/>
                                    </Form.Item>
                                </div>
                                <Form.Item label="Hướng dẫn làm bài" name="instructions">
                                    <Input.TextArea rows={2} placeholder="Học sinh đọc kỹ đề trước khi làm bài..."/>
                                </Form.Item>
                            </div>

                            <div className="form-section">
                                <p className="form-section-title">Cấu hình sinh đề</p>
                                <div className="flex flex-col gap-2">
                                    <Form.Item label="Trộn câu hỏi" name="shuffleQuestions" valuePropName="checked"
                                               className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                    <Form.Item label="Trộn đáp án" name="shuffleAnswers" valuePropName="checked"
                                               className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                    <Form.Item label="Chống trùng câu hỏi" name="preventDuplicate"
                                               valuePropName="checked" className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                    <Form.Item label="Kích hoạt" name="isActive" valuePropName="checked"
                                               className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                </div>
                            </div>
                        </div>

                        {/* Right: sections */}
                        <div className="flex-[1.5] flex flex-col gap-3 min-w-0">
                            <Form.List
                                name="sections"
                                rules={[{
                                    validator: async (_, sections: ExamTemplateSectionBody[]) => {
                                        if (!sections || sections.length < 1)
                                            return Promise.reject(new Error('Cần ít nhất 1 phần thi'))
                                    },
                                }]}
                            >
                                {(fields, {add, remove}, {errors}) => (
                                    <>
                                        <div className="flex items-center justify-between">
                                            <p className="text-[13px] font-semibold text-gray-700">Cấu hình phần thi</p>
                                            <Button type="primary" size="small" icon={<PlusOutlined/>}
                                                    onClick={() => add({...EMPTY_SECTION})}>
                                                Thêm phần
                                            </Button>
                                        </div>
                                        {fields.map((field, idx) => (
                                            <div key={field.key} className="form-section">
                                                <div className="flex items-center justify-between mb-2">
                                                    <span
                                                        className="text-[13px] font-semibold text-gray-600">Phần {idx + 1}</span>
                                                    {fields.length > 1 && (
                                                        <button type="button"
                                                                className="text-gray-400 hover:text-red-500"
                                                                onClick={() => remove(field.name)}>
                                                            <CloseOutlined/>
                                                        </button>
                                                    )}
                                                </div>
                                                <Form.Item label="Tên phần" name={[field.name, 'sectionName']}
                                                           className="!mb-2">
                                                    <Input placeholder="VD: Phần trắc nghiệm"/>
                                                </Form.Item>
                                                <div className="grid grid-cols-2 gap-2">
                                                    <Form.Item label="Chủ đề" name={[field.name, 'topicId']}
                                                               className="!mb-2">
                                                        <Select
                                                            placeholder={watchedSubjectId || watchedGradeId ? 'Chủ đề' : 'Chọn lớp / môn trước'}
                                                            allowClear showSearch optionFilterProp="label"
                                                            notFoundContent={watchedSubjectId || watchedGradeId ? 'Không có chủ đề' : 'Chọn lớp / môn trước'}
                                                            options={topicOptions}/>
                                                    </Form.Item>
                                                    <Form.Item label="Loại câu" name={[field.name, 'questionTypeId']}
                                                               className="!mb-2">
                                                        <Select placeholder="Loại" allowClear
                                                                options={(questionTypes.data ?? []).map(t => ({
                                                                    value: t.id,
                                                                    label: t.name
                                                                }))}/>
                                                    </Form.Item>
                                                </div>
                                                <Form.Item label="Bloom (tuỳ chọn)"
                                                           name={[field.name, 'cognitiveLevelId']} className="!mb-2">
                                                    <Select placeholder="Không lọc Bloom" allowClear
                                                            options={(cognitives.data ?? []).map(c => ({
                                                                value: c.id,
                                                                label: c.name
                                                            }))}/>
                                                </Form.Item>
                                                <div className="grid grid-cols-2 gap-2">
                                                    <Form.Item label="Số câu" name={[field.name, 'questionCount']}
                                                               className="!mb-2"
                                                               rules={[{required: true, message: 'Nhập số câu'}]}
                                                    >
                                                        <InputNumber min={1} className="w-full"/>
                                                    </Form.Item>
                                                    <Form.Item label="Điểm / câu"
                                                               name={[field.name, 'scorePerQuestion']}
                                                               className="!mb-2">
                                                        <InputNumber min={0} step={0.25} className="w-full"/>
                                                    </Form.Item>
                                                </div>
                                                <p className="form-label">Phân bố độ khó (%) — tổng phải = 100</p>
                                                <div className="grid grid-cols-4 gap-2">
                                                    <Form.Item name={[field.name, 'pctEasy']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full"
                                                                     suffix="Dễ"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctMedium']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full"
                                                                     suffix="TB"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctHard']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full"
                                                                     suffix="Khó"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctVeryHard']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full"
                                                                     suffix="RK"/>
                                                    </Form.Item>
                                                </div>
                                            </div>
                                        ))}
                                        <Form.ErrorList errors={errors}/>
                                    </>
                                )}
                            </Form.List>
                        </div>
                    </div>
                </Form>
            </div>

            <div className="action-bar">
                <Button onClick={() => navigate('/app/exams')}>Hủy bỏ</Button>
                <Button type="primary" loading={saveMutation.isPending} onClick={handleSubmit}>
                    {isEdit ? 'Cập nhật mẫu đề' : 'Lưu mẫu đề thi'}
                </Button>
            </div>
        </>
    )
}
