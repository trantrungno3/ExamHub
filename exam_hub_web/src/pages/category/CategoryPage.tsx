import {Tabs} from 'antd'
import {GradeTab} from './grade'
import {DifficultyTab} from './difficulty'
import {CognitiveTab} from './cognitive'
import {SubjectTab} from './subject'
import {TopicTab} from './topic'
import {QuestionTypeTab} from './question-type'

const TAB_ITEMS = [
    {key: 'grade', label: 'Cấp lớp', children: <GradeTab/>},
    {key: 'subject', label: 'Môn học', children: <SubjectTab/>},
    {key: 'topic', label: 'Chủ đề', children: <TopicTab/>},
    {key: 'difficulty', label: 'Độ khó', children: <DifficultyTab/>},
    {key: 'question-type', label: 'Loại câu hỏi', children: <QuestionTypeTab/>},
    {key: 'cognitive', label: 'Cấp độ nhận thức', children: <CognitiveTab/>},
]

export default function CategoryPage() {
    return (
        <>
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Danh mục cấu hình</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>

            <div className="flex-1 overflow-auto">
                <Tabs
                    defaultActiveKey="grade"
                    items={TAB_ITEMS}
                    className="category-tabs"
                    tabBarStyle={{paddingInline: 24, marginBottom: 0, background: '#fff'}}
                />
            </div>
        </>
    )
}
