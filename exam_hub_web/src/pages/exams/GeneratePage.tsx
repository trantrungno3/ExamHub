import {useEffect, useMemo} from 'react'
import {useNavigate, useSearchParams} from 'react-router-dom'
import {Button, Form, Input, InputNumber, Select, Switch, message} from 'antd'
import {CloseOutlined, PlusOutlined, ThunderboltOutlined} from '@ant-design/icons'
import {useBatchGenerateExamMutation, useGenerateExamMutation} from '../../hooks/queries/useExams'
import {useExamTemplateQuery} from '../../hooks/queries/useExamTemplates'
import {
    useCognitiveLevelsQuery,
    useGradeLevelsListQuery,
    useQuestionTypesQuery,
    useSubjectsQuery,
    useTopicsQuery,
} from '../../hooks/queries/useCategoryLists'

type GenerateForm = {
    title: string
    gradeLevelId?: number
    subjectId?: number
    durationMinutes: number
    totalQuestions: number
    totalScore: number
    shuffleQuestions: boolean
    shuffleAnswers: boolean
    preventDuplicate: boolean
    batchMode: boolean
    variantCount: number
    variantNaming: VariantNaming
    sections: SectionConfig[]
}

const EMPTY_SECTION: SectionConfig = {
    sectionName: '',
    topicId: undefined,
    questionTypeId: undefined,
    cognitiveLevelId: undefined,
    questionCount: 10,
    pctEasy: 40,
    pctMedium: 30,
    pctHard: 20,
    pctVeryHard: 10,
    scorePerQuestion: 0.25,
}

const EMPTY: GenerateForm = {
    title: '',
    durationMinutes: 45,
    totalQuestions: 10,
    totalScore: 10,
    shuffleQuestions: true,
    shuffleAnswers: false,
    preventDuplicate: true,
    batchMode: false,
    variantCount: 4,
    variantNaming: 'ALPHA',
    sections: [{...EMPTY_SECTION}],
}

export default function GeneratePage() {
    const navigate = useNavigate()
    const [params] = useSearchParams()
    const templateId = params.get('templateId') ?? undefined
    const [form] = Form.useForm<GenerateForm>()

    const grades = useGradeLevelsListQuery()
    const subjects = useSubjectsQuery()
    const topics = useTopicsQuery()
    const questionTypes = useQuestionTypesQuery()
    const cognitives = useCognitiveLevelsQuery()
    const {data: template} = useExamTemplateQuery(templateId)

    const generate = useGenerateExamMutation()
    const batchGenerate = useBatchGenerateExamMutation()
    const batchMode = Form.useWatch('batchMode', form)

    // Theo dõi lớp / môn để lọc chủ đề ở các phần thi.
    const watchedGradeId = Form.useWatch('gradeLevelId', form)
    const watchedSubjectId = Form.useWatch('subjectId', form)
    const watchedSections = Form.useWatch('sections', form)

    // Tổng số câu cộng dồn từ các phần thi (dùng để hiển thị "Tổng số câu" tự động).
    const sectionsTotal = (watchedSections ?? [] as SectionConfig[])
        .reduce((acc: number, s) => acc + (Number(s?.questionCount) || 0), 0)

    // "Tổng số câu" luôn tự động bằng tổng số câu các phần.
    useEffect(() => {
        if (form.getFieldValue('totalQuestions') !== sectionsTotal)
            form.setFieldValue('totalQuestions', sectionsTotal)
    }, [sectionsTotal, form])

    // Chủ đề lọc theo lớp + môn đã chọn ở phần thông tin đề thi.
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

    // Prefill từ mẫu đề (nếu truy cập qua "Sinh đề" từ trang mẫu đề).
    useEffect(() => {
        if (template) {
            form.setFieldsValue({
                title: template.title,
                gradeLevelId: template.gradeLevelId,
                subjectId: template.subjectId,
                durationMinutes: template.durationMinutes,
                totalScore: template.totalScore,
                shuffleQuestions: template.shuffleQuestions,
                shuffleAnswers: template.shuffleAnswers,
                preventDuplicate: template.preventDuplicate,
                sections: template.sections?.map(s => ({
                    sectionName: s.sectionName,
                    topicId: s.topicId,
                    questionTypeId: s.questionTypeId,
                    cognitiveLevelId: s.cognitiveLevelId,
                    questionCount: s.questionCount,
                    pctEasy: s.pctEasy,
                    pctMedium: s.pctMedium,
                    pctHard: s.pctHard,
                    pctVeryHard: s.pctVeryHard,
                    scorePerQuestion: s.scorePerQuestion ?? 0.25,
                })) ?? [{...EMPTY_SECTION}],
            })
        }
    }, [template, form])

    const handleSubmit = async () => {
        const v = await form.validateFields()
        for (const s of v.sections) {
            if (s.pctEasy + s.pctMedium + s.pctHard + s.pctVeryHard !== 100) {
                message.error('Mỗi phần thi: tổng tỉ lệ độ khó phải bằng 100%')
                return
            }
        }
        const base = {
            title: v.title,
            examTemplateId: templateId,
            gradeLevelId: v.gradeLevelId!,
            subjectId: v.subjectId!,
            durationMinutes: v.durationMinutes,
            totalScore: v.totalScore,
            shuffleQuestions: v.shuffleQuestions,
            shuffleAnswers: v.shuffleAnswers,
            preventDuplicate: v.preventDuplicate,
            sections: v.sections,
        }
        if (v.batchMode) {
            const res = await batchGenerate.mutateAsync({
                ...base,
                variantCount: v.variantCount,
                variantNaming: v.variantNaming,
            })
            if (res.data) navigate('/app/exams')
        } else {
            const res = await generate.mutateAsync(base)
            if (res.data) navigate('/app/exams')
        }
    }

    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Sinh đề thi</p>
                    <p className="top-bar-subtitle">Sinh đề tự động từ ngân hàng câu hỏi theo cấu hình phần thi</p>
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
                            const sections = (form.getFieldValue('sections') ?? []) as SectionConfig[]
                            if (sections.some(s => s?.topicId))
                                form.setFieldValue('sections', sections.map(s => ({...s, topicId: undefined})))
                        }
                    }}
                >
                    <div className="flex gap-5 items-start">
                        <div className="flex-[2] flex flex-col gap-4 min-w-0">
                            <div className="form-section">
                                <p className="form-section-title">Thông tin đề thi</p>
                                <Form.Item label="Tiêu đề" name="title" rules={[{required: true, message: 'Nhập tiêu đề'}]}>
                                    <Input placeholder="VD: Đề kiểm tra Toán 10 — Tuần 10"/>
                                </Form.Item>
                                <div className="grid grid-cols-2 gap-4">
                                    <Form.Item label="Lớp" name="gradeLevelId" rules={[{required: true, message: 'Chọn lớp'}]}>
                                        <Select placeholder="Chọn lớp"
                                                options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                                    </Form.Item>
                                    <Form.Item label="Môn học" name="subjectId" rules={[{required: true, message: 'Chọn môn'}]}>
                                        <Select placeholder="Chọn môn" showSearch optionFilterProp="label"
                                                options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                                    </Form.Item>
                                </div>
                                <div className="grid grid-cols-3 gap-4">
                                    <Form.Item label="Thời gian (phút)" name="durationMinutes" rules={[{required: true}]}>
                                        <InputNumber min={1} className="w-full"/>
                                    </Form.Item>
                                    <Form.Item label="Tổng số câu" name="totalQuestions" help="Tự động theo tổng số câu các phần">
                                        <InputNumber min={0} className="w-full" readOnly/>
                                    </Form.Item>
                                    <Form.Item label="Tổng điểm" name="totalScore" rules={[{required: true}]}>
                                        <InputNumber min={0} step={0.5} className="w-full"/>
                                    </Form.Item>
                                </div>
                            </div>

                            <div className="form-section">
                                <p className="form-section-title">Cấu hình sinh đề</p>
                                <div className="flex flex-col gap-2">
                                    <Form.Item label="Trộn câu hỏi" name="shuffleQuestions" valuePropName="checked" className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                    <Form.Item label="Trộn đáp án" name="shuffleAnswers" valuePropName="checked" className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                    <Form.Item label="Chống trùng câu hỏi" name="preventDuplicate" valuePropName="checked" className="!mb-0">
                                        <Switch/>
                                    </Form.Item>
                                </div>
                            </div>

                            <div className="form-section">
                                <p className="form-section-title">Sinh theo lô (nhiều biến thể)</p>
                                <Form.Item label="Bật chế độ lô" name="batchMode" valuePropName="checked" className="!mb-2">
                                    <Switch/>
                                </Form.Item>
                                {batchMode && (
                                    <div className="grid grid-cols-2 gap-4">
                                        <Form.Item label="Số biến thể" name="variantCount" rules={[{required: true}]}>
                                            <InputNumber min={1} max={20} className="w-full"/>
                                        </Form.Item>
                                        <Form.Item label="Cách đặt mã" name="variantNaming">
                                            <Select options={[{value: 'ALPHA', label: 'A, B, C...'}, {value: 'NUMBER', label: '1, 2, 3...'}]}/>
                                        </Form.Item>
                                    </div>
                                )}
                            </div>
                        </div>

                        <div className="flex-[1.5] flex flex-col gap-3 min-w-0">
                            <Form.List name="sections">
                                {(fields, {add, remove}) => (
                                    <>
                                        <div className="flex items-center justify-between">
                                            <p className="text-[13px] font-semibold text-gray-700">Phần thi</p>
                                            <Button type="primary" size="small" icon={<PlusOutlined/>}
                                                    onClick={() => add({...EMPTY_SECTION})}>Thêm phần</Button>
                                        </div>
                                        {fields.map((field, idx) => (
                                            <div key={field.key} className="form-section">
                                                <div className="flex items-center justify-between mb-2">
                                                    <span className="text-[13px] font-semibold text-gray-600">Phần {idx + 1}</span>
                                                    {fields.length > 1 && (
                                                        <button type="button" className="text-gray-400 hover:text-red-500"
                                                                onClick={() => remove(field.name)}><CloseOutlined/></button>
                                                    )}
                                                </div>
                                                <div className="grid grid-cols-2 gap-2">
                                                    <Form.Item label="Chủ đề (tuỳ chọn)" name={[field.name, 'topicId']} className="!mb-2">
                                                        <Select allowClear
                                                            placeholder={watchedSubjectId || watchedGradeId ? 'Mọi chủ đề của môn' : 'Chọn lớp / môn trước'}
                                                            showSearch optionFilterProp="label"
                                                            notFoundContent={watchedSubjectId || watchedGradeId ? 'Không có chủ đề' : 'Chọn lớp / môn trước'}
                                                            options={topicOptions}/>
                                                    </Form.Item>
                                                    <Form.Item label="Loại câu" name={[field.name, 'questionTypeId']} className="!mb-2">
                                                        <Select placeholder="Mọi loại" allowClear
                                                                options={(questionTypes.data ?? []).map(t => ({value: t.id, label: t.name}))}/>
                                                    </Form.Item>
                                                </div>
                                                <Form.Item label="Bloom (tuỳ chọn)" name={[field.name, 'cognitiveLevelId']} className="!mb-2">
                                                    <Select placeholder="Không lọc Bloom" allowClear
                                                            options={(cognitives.data ?? []).map(c => ({value: c.id, label: c.name}))}/>
                                                </Form.Item>
                                                <div className="grid grid-cols-2 gap-2">
                                                    <Form.Item label="Số câu" name={[field.name, 'questionCount']} className="!mb-2"
                                                               rules={[{required: true}]}>
                                                        <InputNumber min={1} className="w-full"/>
                                                    </Form.Item>
                                                    <Form.Item label="Điểm / câu" name={[field.name, 'scorePerQuestion']} className="!mb-2"
                                                               rules={[{required: true}]}>
                                                        <InputNumber min={0.01} step={0.25} className="w-full"/>
                                                    </Form.Item>
                                                </div>
                                                <p className="form-label">Phân bố độ khó (%) — tổng = 100</p>
                                                <div className="grid grid-cols-4 gap-2">
                                                    <Form.Item name={[field.name, 'pctEasy']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" suffix="Dễ"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctMedium']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" suffix="TB"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctHard']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" suffix="Khó"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctVeryHard']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" suffix="RK"/>
                                                    </Form.Item>
                                                </div>
                                            </div>
                                        ))}
                                    </>
                                )}
                            </Form.List>
                        </div>
                    </div>
                </Form>
            </div>

            <div className="action-bar">
                <Button onClick={() => navigate('/app/exams')}>Hủy bỏ</Button>
                <Button type="primary" icon={<ThunderboltOutlined/>}
                        loading={generate.isPending || batchGenerate.isPending} onClick={handleSubmit}>
                    Sinh đề thi
                </Button>
            </div>
        </>
    )
}
