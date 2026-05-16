import {AuthHttp} from './requestService'
import {CategoryServiceBase} from './categoryServiceBase'

class TopicService extends CategoryServiceBase<Topic, TopicBody> {
    constructor() {
        super('topic')
    }

    getBySubject(subjectId: number) {
        return AuthHttp.get<Topic[]>(`/${this.basePath}/by-subject/${subjectId}`)
    }

    getRootBySubject(subjectId: number) {
        return AuthHttp.get<Topic[]>(`/${this.basePath}/root/by-subject/${subjectId}`)
    }

    getChildren(parentId: number) {
        return AuthHttp.get<Topic[]>(`/${this.basePath}/${parentId}/children`)
    }
}

export const topicService = new TopicService()
