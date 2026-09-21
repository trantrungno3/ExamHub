import {useEffect, useState} from 'react'

/** Trả giá trị trễ `delay` ms sau lần đổi cuối. Dùng để hoãn query key, không hoãn input. */
export function useDebounced<T>(value: T, delay = 300): T {
    const [debounced, setDebounced] = useState(value)

    useEffect(() => {
        const id = setTimeout(() => setDebounced(value), delay)
        return () => clearTimeout(id)
    }, [value, delay])

    return debounced
}
