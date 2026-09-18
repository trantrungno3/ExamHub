import {lazy} from 'react'

/**
 * Bản lazy của AnalyticsDrawer — drawer chỉ mở khi bấm "Phân tích", nhưng nó kéo
 * theo recharts. Hai trang dùng chung (ExamListPage, ExamSessionEditPage) nên gói
 * lazy() ở đây một lần thay vì lặp ở cả hai.
 *
 * `.then(...)` vì AnalyticsDrawer là named export, còn lazy() chỉ nhận default export.
 * Nơi dùng phải bọc trong <Suspense>.
 */
export const AnalyticsDrawer = lazy(
    () => import('./AnalyticsDrawer').then(m => ({default: m.AnalyticsDrawer})),
)
