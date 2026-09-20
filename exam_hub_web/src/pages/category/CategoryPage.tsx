import {Tabs} from 'antd'
import {useSearchParams} from 'react-router-dom'
import {GradeTab} from './grade'
import {DifficultyTab} from './difficulty'
import {CognitiveTab} from './cognitive'
import {SubjectTab} from './subject'
import {TopicTab} from './topic'
import {QuestionTypeTab} from './question-type'
import PageHeader from '../../components/PageHeader'

const TAB_ITEMS = [
    {key: 'grade', label: 'Cấp lớp', children: <GradeTab/>},
    {key: 'subject', label: 'Môn học', children: <SubjectTab/>},
    {key: 'topic', label: 'Chủ đề', children: <TopicTab/>},
    {key: 'difficulty', label: 'Độ khó', children: <DifficultyTab/>},
    {key: 'question-type', label: 'Loại câu hỏi', children: <QuestionTypeTab/>},
    {key: 'cognitive', label: 'Cấp độ nhận thức', children: <CognitiveTab/>},
]

export default function CategoryPage() {
    const [params, setParams] = useSearchParams()
    // Validate key từ URL: ?tab=rác sẽ rơi về tab đầu thay vì render Tabs rỗng.
    const tab = params.get('tab')
    const activeKey = TAB_ITEMS.some(t => t.key === tab) ? tab! : TAB_ITEMS[0].key

    return (
        <>
            <PageHeader title="Danh mục cấu hình"/>

            <div className="flex-1 overflow-auto">
                <Tabs
                    activeKey={activeKey}
                    // replace: đổi tab không đẩy thêm entry, nút Back vẫn rời khỏi trang như mong đợi.
                    onChange={key => setParams({tab: key}, {replace: true})}
                    items={TAB_ITEMS}
                    className="category-tabs"
                    tabBarStyle={{paddingInline: 24, marginBottom: 0, background: '#fff'}}
                />
            </div>
        </>
    )
}
