import {useEffect} from 'react'
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
    shuffleQuestions: boolean
    batchMode: boolean
    shuffleAnswers: boolean
    variantCount: number
    variantNaming: VariantNaming
    sections: SectionConfig[]
}

const EMPTY_SECTION: SectionConfig = {
    sectionName: '',
    topicId: 0,
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
    shuffleQuestions: true,
    batchMode: false,
    shuffleAnswers: false,
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

    // Prefill từ mẫu đề (nếu truy cập qua "Sinh đề" từ trang mẫu đề).
    useEffect(() => {
        if (template) {
            form.setFieldsValue({
                title: template.title,
                gradeLevelId: template.gradeLevelId,
                subjectId: template.subjectId,
                durationMinutes: template.durationMinutes,
                shuffleQuestions: template.shuffleQuestions,
                shuffleAnswers: template.shuffleAnswers,
                sections: template.sections?.map(s => ({
                    sectionName: s.sectionName,
                    topicId: s.topicId ?? 0,
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
            shuffleQuestions: v.shuffleQuestions,
            sections: v.sections,
        }
        if (v.batchMode) {
            const res = await batchGenerate.mutateAsync({
                ...base,
                shuffleAnswers: v.shuffleAnswers,
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
                <Form form={form} layout="vertical" initialValues={EMPTY}>
                    <div className="flex gap-5 items-start">
                        <div className="flex-[2] flex flex-col gap-4 min-w-0">
                            <div className="form-section">
                                <p className="form-section-title">Thông tin đề thi</p>
                                <Form.Item label="Tiêu đề" name="title" rules={[{required: true, message: 'Nhập tiêu đề'}]}>
                                    <Input placeholder="VD: Đề kiểm tra Toán 10 — Tuần 10"/>
                                </Form.Item>
                                <div className="grid grid-cols-3 gap-4">
                                    <Form.Item label="Lớp" name="gradeLevelId" rules={[{required: true, message: 'Chọn lớp'}]}>
                                        <Select placeholder="Chọn lớp"
                                                options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}/>
                                    </Form.Item>
                                    <Form.Item label="Môn học" name="subjectId" rules={[{required: true, message: 'Chọn môn'}]}>
                                        <Select placeholder="Chọn môn" showSearch optionFilterProp="label"
                                                options={(subjects.data ?? []).map(s => ({value: s.id, label: s.name}))}/>
                                    </Form.Item>
                                    <Form.Item label="Thời gian (phút)" name="durationMinutes" rules={[{required: true}]}>
                                        <InputNumber min={1} className="w-full"/>
                                    </Form.Item>
                                </div>
                                <Form.Item label="Trộn câu hỏi" name="shuffleQuestions" valuePropName="checked" className="!mb-0">
                                    <Switch/>
                                </Form.Item>
                            </div>

                            <div className="form-section">
                                <p className="form-section-title">Sinh theo lô (nhiều biến thể)</p>
                                <Form.Item label="Bật chế độ lô" name="batchMode" valuePropName="checked" className="!mb-2">
                                    <Switch/>
                                </Form.Item>
                                {batchMode && (
                                    <div className="grid grid-cols-3 gap-4">
                                        <Form.Item label="Số biến thể" name="variantCount" rules={[{required: true}]}>
                                            <InputNumber min={1} max={20} className="w-full"/>
                                        </Form.Item>
                                        <Form.Item label="Cách đặt mã" name="variantNaming">
                                            <Select options={[{value: 'ALPHA', label: 'A, B, C...'}, {value: 'NUMBER', label: '1, 2, 3...'}]}/>
                                        </Form.Item>
                                        <Form.Item label="Trộn đáp án" name="shuffleAnswers" valuePropName="checked">
                                            <Switch/>
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
                                                    <Form.Item label="Chủ đề" name={[field.name, 'topicId']} className="!mb-2"
                                                               rules={[{required: true, message: 'Chọn chủ đề'}]}>
                                                        <Select placeholder="Chủ đề" showSearch optionFilterProp="label"
                                                                options={(topics.data ?? []).map(t => ({value: t.id, label: t.name}))}/>
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
                                                        <InputNumber min={0} max={100} className="w-full" addonAfter="Dễ"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctMedium']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" addonAfter="TB"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctHard']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" addonAfter="Khó"/>
                                                    </Form.Item>
                                                    <Form.Item name={[field.name, 'pctVeryHard']} className="!mb-0">
                                                        <InputNumber min={0} max={100} className="w-full" addonAfter="RK"/>
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
