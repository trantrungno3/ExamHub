import {AuthHttp} from './requestService'

export class CategoryServiceBase<TEntity, TBody> {
    protected readonly basePath: string

    constructor(basePath: string) {
        this.basePath = basePath
    }

    getAll() {
        return AuthHttp.get<TEntity[]>(`/${this.basePath}`)
    }

    getActive() {
        return AuthHttp.get<TEntity[]>(`/${this.basePath}/active`)
    }

    getById(id: number) {
        return AuthHttp.get<TEntity>(`/${this.basePath}/${id}`)
    }

    create(body: TBody) {
        return AuthHttp.post<TEntity>(`/${this.basePath}`, body)
    }

    update(id: number, body: TBody) {
        return AuthHttp.put<TEntity>(`/${this.basePath}/${id}`, body)
    }

    remove(id: number) {
        return AuthHttp.delete<void>(`/${this.basePath}/${id}`)
    }

    toggleActive(id: number, active: boolean) {
        return AuthHttp.patch<boolean>(`/${this.basePath}/${id}/active`, active)
    }
}
