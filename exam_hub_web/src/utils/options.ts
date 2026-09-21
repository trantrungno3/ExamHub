/** Dựng options cho antd <Select> từ danh mục {id, name}. */
export const toOptions = <T extends {id: number | string; name: string}>(items?: T[]) =>
    (items ?? []).map(i => ({value: i.id, label: i.name}))

/** Bản linh hoạt cho danh sách không có sẵn field `name` (VD: cohortClass.className). */
export const toOptionsBy = <T>(items: T[] | undefined,
                               getValue: (i: T) => number | string,
                               getLabel: (i: T) => string) =>
    (items ?? []).map(i => ({value: getValue(i), label: getLabel(i)}))
