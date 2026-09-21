# UI Review Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Sửa 22 phát hiện review UI của `exam_hub_web` — bundle, re-render, trùng lặp, a11y, token màu — mà không đổi bố cục thị giác của bất kỳ màn hình nào.

**Architecture:** Làm theo tầng, gom trùng lặp trước. `PageHeader` thay 16 bản sao `.top-bar` ở Task 1–2 để hai phase sau (a11y, token màu) chỉ sửa một file thay vì 16. Trang làm bài (`ExamTakingPage`) tách thành 3 task riêng vì là đường đi rủi ro nhất.

**Tech Stack:** React 19, Vite 7, TypeScript 5.9, antd 6, Tailwind 4, TanStack Query 5, Zustand 5, vitest 5.

**Spec:** `docs/superpowers/specs/2026-09-18-ui-review-fixes-design.md`

## Global Constraints

- **Không đổi thị giác.** Ngoại lệ duy nhất được phép: avatar top-bar hiện chữ cái đầu của người dùng thật (thay `"TT"`), và hai lớp màu chữ phụ được nâng tương phản ở Task 13. Mọi thay đổi khác phải giữ nguyên pixel.
- **Không thêm dependency.** Không cài `@testing-library/react`, không thêm thư viện UI.
- **Không đụng `exam_hub_api/`.** Working tree đang có `exam_hub_api/database_schema.sql` bị sửa — không `git add` file đó ở bất kỳ task nào.
- **Verify mặc định = `npm run build` xanh + tự chạy app kiểm luồng bị đụng.** Repo không có test render; KHÔNG dựng hạ tầng test mới. Chỉ Task 8 có chu trình TDD (vitest, logic thuần).
- **Đường chạy lệnh:** mọi lệnh `npm` chạy trong `exam_hub_web/`.
- **Nhánh:** `ui-review-fixes`.
- **Không đổi tên file/route/API** ngoài những chỗ plan nêu đích danh.
- **Tiếng Việt** cho mọi chuỗi hiển thị và comment mới, khớp văn phong file đang sửa.

---

## File Structure

**Tạo mới:**
- `src/components/PageHeader.tsx` — khối top-bar dùng chung (title, subtitle, back, avatar)
- `src/hooks/useDebounced.ts` — trễ giá trị đưa vào query key
- `src/utils/options.ts` — helper thuần dựng `{value,label}` cho antd Select
- `src/pages/student/ExamTimer.tsx` — đồng hồ đếm ngược tự giữ state
- `src/pages/student/answerState.ts` — logic thuần "câu nào đã trả lời"
- `src/pages/student/answerState.test.ts` — test cho file trên
- `src/components/RouteError.tsx` — `errorElement` của router
- `src/constants/theme.ts` — bảng màu dùng chung cho antd + Tailwind
- `src/components/StatCard.tsx` — thẻ số dùng chung (gộp 2 bản trùng)

**Sửa:**
- 16 page có `.top-bar` (Task 1–2)
- `src/AuthProvider.tsx`, `src/routes/ProtectedRoute.tsx` (Task 6)
- `src/pages/student/ExamTakingPage.tsx` (Task 7, 8, 9)
- `src/routes/index.tsx`, `src/layouts/AppLayout.tsx` (Task 10, 12)
- `index.html`, `src/index.css` (Task 11, 13)
- `src/App.tsx`, `src/components/StatusTag.tsx` (Task 15, 16)
- `package.json` (Task 19)

---

### Task 1: Component `PageHeader` + 3 call site đại diện

Ba page này phủ cả 4 biến thể của API. Làm trước để chốt API trước khi đổi hàng loạt.

**Files:**
- Create: `exam_hub_web/src/components/PageHeader.tsx`
- Modify: `exam_hub_web/src/pages/dashboard/DashboardPage.tsx:87-93`
- Modify: `exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx:130-146`
- Modify: `exam_hub_web/src/pages/school/SchoolDetailPage.tsx:166-172`

**Interfaces:**
- Consumes: `useAuthStore` từ `src/stores/authStore.ts` (đã có).
- Produces: `PageHeader` (default export) với props
  `{ title?: string; subtitle?: ReactNode; backTo?: string; left?: ReactNode; right?: ReactNode }`.
  Task 2 dùng lại đúng API này.

- [ ] **Step 1: Tạo `src/components/PageHeader.tsx`**

```tsx
import type {ReactNode} from 'react'
import {useNavigate} from 'react-router-dom'
import {ArrowLeftOutlined} from '@ant-design/icons'
import {useAuthStore} from '../stores/authStore'

type Props = {
    /** Tiêu đề trang. Bỏ qua nếu truyền `left`. */
    title?: string
    /** Dòng phụ — ReactNode để bọc được breadcrumb có link. */
    subtitle?: ReactNode
    /** Có giá trị → hiện nút ← điều hướng về path này. */
    backTo?: string
    /** Thay toàn bộ khối trái (dùng cho <Breadcrumb>). */
    left?: ReactNode
    /** Thay khối phải. Mặc định: avatar chữ cái đầu của người dùng. */
    right?: ReactNode
}

function UserAvatar() {
    const user = useAuthStore(s => s.user)
    const name = user?.displayName ?? user?.userName ?? 'A'
    return <div className="top-bar-avatar">{name.charAt(0).toUpperCase()}</div>
}

export default function PageHeader({title, subtitle, backTo, left, right}: Props) {
    const navigate = useNavigate()

    return (
        <div className="top-bar">
            {left ?? (
                <div className="flex items-center gap-3">
                    {backTo && (
                        <button className="text-gray-500 hover:text-gray-800"
                                aria-label="Quay lại"
                                onClick={() => navigate(backTo)}>
                            <ArrowLeftOutlined/>
                        </button>
                    )}
                    <div>
                        {title && <p className="top-bar-title">{title}</p>}
                        {subtitle && <p className="top-bar-subtitle">{subtitle}</p>}
                    </div>
                </div>
            )}
            {right ?? <UserAvatar/>}
        </div>
    )
}
```

Lưu ý: khối trái luôn bọc `flex items-center gap-3` kể cả khi không có `backTo`. `.top-bar`
là `justify-between` nên một div bọc thêm không đổi bố cục — đã đối chiếu với
`ExamSessionEditPage:131` vốn dùng đúng class này.

- [ ] **Step 2: `DashboardPage` — biến thể title + subtitle + avatar**

Xoá `DashboardPage.tsx:87-93`:

```tsx
            <div className="top-bar">
                <div>
                    <p className="top-bar-title">Tổng quan hệ thống</p>
                    <p className="top-bar-subtitle">{today}</p>
                </div>
                <div className="top-bar-avatar">TT</div>
            </div>
```

Thay bằng:

```tsx
            <PageHeader title="Tổng quan hệ thống" subtitle={today}/>
```

Thêm import: `import PageHeader from '../../components/PageHeader'`

- [ ] **Step 3: `ExamSessionEditPage` — biến thể back + right tuỳ biến**

Xoá `ExamSessionEditPage.tsx:130-146` (cả khối `<div className="top-bar">…</div>`), thay bằng:

```tsx
            <PageHeader
                title={isEdit ? 'Sửa kỳ thi' : 'Tạo kỳ thi'}
                subtitle="Cấu hình, chọn đề và giao cho lớp/khoá"
                backTo={ROUTES.EXAM_SESSIONS}
                right={detail && (
                    <Tag color={isPublished ? 'green' : detail.status === 'closed' ? 'default' : 'gold'}>
                        {isPublished ? 'Đã phát hành' : detail.status === 'closed' ? 'Đã đóng' : 'Nháp'}
                    </Tag>
                )}
            />
```

Thêm import `PageHeader`. Giữ nguyên import `Tag` và `ROUTES`. Nếu `ArrowLeftOutlined` không còn
chỗ dùng nào khác trong file thì bỏ khỏi import.

Lưu ý: khi `detail` là `undefined`, `right` nhận `undefined` → `PageHeader` rơi về avatar mặc
định. Trang này trước đây không có avatar, nhưng hiện avatar khi đang tải là chấp nhận được và
nhất quán với 15 page còn lại.

- [ ] **Step 4: `SchoolDetailPage` — biến thể `left` (Breadcrumb)**

Xoá `SchoolDetailPage.tsx:166-172`, thay bằng:

```tsx
            <PageHeader left={
                <Breadcrumb items={[
                    {title: <a onClick={() => navigate('/app/schools')}>Trường học</a>},
                    {title: school?.name ?? `Trường #${schoolId}`},
                ]}/>
            }/>
```

Thêm import `PageHeader`, giữ `Breadcrumb`.

- [ ] **Step 5: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh, không lỗi TS, không warning mới.

- [ ] **Step 6: Kiểm thủ công**

Run: `cd exam_hub_web && npm run dev`
Mở `/app/dashboard`, `/app/exam-sessions/create`, `/app/exam-sessions/<id>/edit`, `/app/schools/<id>`.
Kiểm: top-bar cao và canh lề y hệt trước; avatar hiện chữ cái đầu của tài khoản đang đăng nhập
(không còn `TT`); nút ← ở trang sửa kỳ thi vẫn về danh sách kỳ thi; `<Tag>` trạng thái vẫn ở góc phải.

- [ ] **Step 7: Commit**

```bash
git add exam_hub_web/src/components/PageHeader.tsx exam_hub_web/src/pages/dashboard/DashboardPage.tsx exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx exam_hub_web/src/pages/school/SchoolDetailPage.tsx
git commit -m "refactor(web): extract PageHeader, show real user avatar"
```

---

### Task 2: Đổi 13 call site `.top-bar` còn lại

**Files:**
- Modify: `exam_hub_web/src/pages/category/CategoryPage.tsx:21-26`
- Modify: `exam_hub_web/src/pages/exams/CreateExamTemplatePage.tsx:162-172`
- Modify: `exam_hub_web/src/pages/exams/ExamDetailPage.tsx:32-40`
- Modify: `exam_hub_web/src/pages/exams/ExamListPage.tsx:110-116`
- Modify: `exam_hub_web/src/pages/exams/ExamSessionListPage.tsx:96-102`
- Modify: `exam_hub_web/src/pages/exams/ExamTemplatePage.tsx:129-135`
- Modify: `exam_hub_web/src/pages/exams/GeneratePage.tsx:174-180`
- Modify: `exam_hub_web/src/pages/exams/SubmissionListPage.tsx:70-76`
- Modify: `exam_hub_web/src/pages/exams/SubmissionReviewPage.tsx:51-57`
- Modify: `exam_hub_web/src/pages/questions/AddQuestionPage.tsx:186-194`
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx:263-269`
- Modify: `exam_hub_web/src/pages/school/CohortDetailPage.tsx:137-143`
- Modify: `exam_hub_web/src/pages/school/SchoolListPage.tsx:72-75`

**Interfaces:**
- Consumes: `PageHeader` từ Task 1 — `{title?, subtitle?, backTo?, left?, right?}`.
- Produces: không có API mới.

- [ ] **Step 1: 8 page biến thể đơn giản (title + subtitle)**

Mỗi file: xoá khối `<div className="top-bar">…</div>`, thêm
`import PageHeader from '../../components/PageHeader'`, thay bằng:

```tsx
// CategoryPage.tsx:21-26
<PageHeader title="Danh mục cấu hình"/>

// ExamDetailPage.tsx:32-40
<PageHeader title="Xem chi tiết đề thi"
            subtitle={exam ? `${exam.title}${exam.examCode ? ` · Mã đề ${exam.examCode}` : ''}` : 'Đang tải…'}/>

// ExamListPage.tsx:110-116
<PageHeader title="Đề thi" subtitle="Danh sách đề thi đã sinh — xem trước & xuất file"/>

// ExamSessionListPage.tsx:96-102
<PageHeader title="Kỳ thi" subtitle="Cấu hình kỳ thi theo môn + cấp lớp, giao cho lớp/khoá"/>

// ExamTemplatePage.tsx:129-135
<PageHeader title="Mẫu đề thi" subtitle="Cấu hình cấu trúc đề thi để sinh đề tự động"/>

// GeneratePage.tsx:174-180
<PageHeader title="Sinh đề thi" subtitle="Sinh đề tự động từ ngân hàng câu hỏi theo cấu hình phần thi"/>

// SubmissionListPage.tsx:70-76
<PageHeader title="Bài nộp kỳ thi" subtitle={subtitle || 'Danh sách bài nộp của học sinh'}/>

// SubmissionReviewPage.tsx:51-57
<PageHeader title="Xem bài làm học sinh" subtitle={exam?.title ?? 'Đang tải…'}/>

// QuestionBankPage.tsx:263-269
<PageHeader title="Ngân hàng câu hỏi"
            subtitle="Quản lý toàn bộ câu hỏi theo môn học · chủ đề · độ khó · cấp độ Bloom"/>

// SchoolListPage.tsx:72-75
<PageHeader title="Quản lý trường học"/>
```

(`SchoolListPage` hiện đặt `<p className="top-bar-title">` trực tiếp trong `.top-bar`, không có
div bọc — `PageHeader` thêm div bọc, không đổi bố cục vì `.top-bar` là flex `justify-between`.)

- [ ] **Step 2: 2 page có subtitle là breadcrumb có link**

```tsx
// AddQuestionPage.tsx:186-194
<PageHeader
    title={isEdit ? 'Sửa câu hỏi' : 'Thêm câu hỏi mới'}
    subtitle={
        <>
            <span className="cursor-pointer hover:underline" style={{color: '#3a74f5'}}
                  onClick={() => navigate('/app/questions')}>Câu hỏi</span>
            {' / '}{isEdit ? 'Chỉnh sửa' : 'Thêm mới'}
        </>
    }
/>

// CreateExamTemplatePage.tsx:162-172
<PageHeader
    title={isEdit ? 'Sửa mẫu đề thi' : 'Tạo mẫu đề thi mới'}
    subtitle={
        <>
            <span className="text-blue-500 cursor-pointer hover:underline"
                  onClick={() => navigate('/app/exams')}>Mẫu đề thi</span>
            {' / '}{isEdit ? 'Chỉnh sửa' : 'Tạo mới'}
        </>
    }
/>
```

Giữ nguyên hex `#3a74f5` và class `text-blue-500` ở bước này — Task 16 xử lý.

- [ ] **Step 3: `CohortDetailPage` — biến thể `left`**

```tsx
// CohortDetailPage.tsx:137-143
<PageHeader left={
    <Breadcrumb items={[
        {title: <a onClick={() => navigate('/app/schools')}>Trường học</a>},
        {title: `Khoá #${cohortId}`},
    ]}/>
}/>
```

- [ ] **Step 4: Xác nhận không còn bản sao nào**

Run: `cd exam_hub_web && grep -rn 'top-bar-avatar' src`
Expected: chỉ còn đúng 2 dòng — `src/index.css` (định nghĩa class) và
`src/components/PageHeader.tsx`.

Run: `cd exam_hub_web && grep -rn 'className="top-bar"' src`
Expected: chỉ còn `src/components/PageHeader.tsx`.

- [ ] **Step 5: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh. Nếu TS báo import không dùng (ví dụ `ArrowLeftOutlined` ở page đã bỏ nút back),
gỡ import đó.

- [ ] **Step 6: Kiểm thủ công**

Mở lần lượt 13 trang trên. Đối chiếu: chiều cao top-bar, vị trí title/subtitle, avatar bên phải.
Bấm link breadcrumb ở `AddQuestionPage`, `CreateExamTemplatePage`, `CohortDetailPage`,
`SchoolDetailPage` — vẫn điều hướng đúng.

- [ ] **Step 7: Commit**

```bash
git add exam_hub_web/src/pages
git commit -m "refactor(web): use PageHeader across remaining pages"
```

---

### Task 3: Gộp `stripHtml` về một bản

**Files:**
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx:429-431` (xoá)
- Modify: `exam_hub_web/src/pages/questions/AddQuestionPage.tsx:411-413` (xoá)

**Interfaces:**
- Consumes: `stripHtml(html?: string): string` từ `src/utils/snapshot.ts:11` (đã có).
- Produces: không có API mới.

Bản trong `snapshot.ts` nhận `html?: string` (rộng hơn hai bản cục bộ vốn nhận `html: string`)
và trả `''` cho `undefined`, nên mọi call site hiện tại vẫn hợp kiểu.

- [ ] **Step 1: Xoá bản cục bộ trong `QuestionBankPage.tsx`**

Xoá khối cuối file:

```tsx
function stripHtml(html: string): string {
    return html.replace(/<[^>]*>/g, ' ').replace(/\s+/g, ' ').trim()
}
```

Thêm vào import block: `import {stripHtml} from '../../utils/snapshot'`

- [ ] **Step 2: Xoá bản cục bộ trong `AddQuestionPage.tsx`**

Xoá khối tương tự ở `AddQuestionPage.tsx:411-413`, thêm cùng dòng import.

- [ ] **Step 3: Xác nhận chỉ còn một định nghĩa**

Run: `cd exam_hub_web && grep -rn 'function stripHtml' src`
Expected: đúng 1 dòng — `src/utils/snapshot.ts:11`.

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công**

Mở `/app/questions` — cột "Nội dung câu hỏi" vẫn hiện plaintext, không lộ thẻ HTML.
Mở `/app/questions/<id>/edit` — nội dung câu hỏi hiện đúng.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/pages/questions
git commit -m "refactor(web): drop duplicate stripHtml copies"
```

---

### Task 4: Helper `toOptions` cho antd Select

Thay thế mục #12 của spec. Spec đề xuất hook `useCascadeOptions`, nhưng hook đó chỉ có đúng một
consumer thật (`QuestionBankPage`) — `ExamSessionEditPage` chỉ cần `subjectOptions` và gọi hook sẽ
kéo theo `useTopicsQuery` thừa. Trùng lặp thật sự là `.map(x => ({value: x.id, label: x.name}))`,
lặp ở ~15 chỗ. Dùng helper thuần, không dùng hook.

**Files:**
- Create: `exam_hub_web/src/utils/options.ts`
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx` (các `Select` trong modal lọc)
- Modify: `exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx` (Select "Cấp lớp", "Trường", "Khoá")

**Interfaces:**
- Consumes: không.
- Produces: `toOptions<T extends {id: number|string; name: string}>(items?: T[]): {value, label}[]`
  và `toOptionsBy<T>(items, getValue, getLabel)`. Không task nào sau dùng lại.

- [ ] **Step 1: Tạo `src/utils/options.ts`**

```ts
/** Dựng options cho antd <Select> từ danh mục {id, name}. */
export const toOptions = <T extends {id: number | string; name: string}>(items?: T[]) =>
    (items ?? []).map(i => ({value: i.id, label: i.name}))

/** Bản linh hoạt cho danh sách không có sẵn field `name` (VD: cohortClass.className). */
export const toOptionsBy = <T>(items: T[] | undefined,
                               getValue: (i: T) => number | string,
                               getLabel: (i: T) => string) =>
    (items ?? []).map(i => ({value: getValue(i), label: getLabel(i)}))
```

- [ ] **Step 2: Dùng trong `QuestionBankPage.tsx`**

Trong modal lọc, thay từng chỗ:

```tsx
// trước
options={(grades.data ?? []).map(g => ({value: g.id, label: g.name}))}
// sau
options={toOptions(grades.data)}
```

Áp dụng cho `grades`, `difficulties`, `questionTypes`, `cognitives`.
Giữ nguyên `subjectOptions` và `topicOptions` (`QuestionBankPage.tsx:118-133`) — chúng có logic lọc
theo `draft.gradeLevelId`/`draft.subjectId`, không phải map thuần; chỉ đổi phần `.map(...)` cuối
thành `toOptions(...)` nếu đọc ra gọn hơn, còn lại để nguyên.

Thêm import: `import {toOptions} from '../../utils/options'`

- [ ] **Step 3: Dùng trong `ExamSessionEditPage.tsx`**

```tsx
// ExamSessionEditPage.tsx:180-183 — Select "Cấp lớp"
options={toOptions(grades.data)}

// ExamSessionEditPage.tsx:392 — Select "Trường"
options={toOptions(schools.data)}

// ExamSessionEditPage.tsx:401 — Select "Khoá"
options={toOptions(cohorts.data)}

// ExamSessionEditPage.tsx:407 — Select "Lớp", field tên là className
options={toOptionsBy(classes.data, c => c.id, c => c.className)}
```

Giữ nguyên `subjectOptions` (`:72-77`) — có lọc theo `watchedGradeId`.

Thêm import: `import {toOptions, toOptionsBy} from '../../utils/options'`

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh. TS phải suy ra đúng kiểu `value` (number) cho cả 4 Select.

- [ ] **Step 5: Kiểm thủ công**

Mở `/app/questions` → "Bộ lọc": cả 6 dropdown đổ đúng dữ liệu, chọn được, lọc ra kết quả.
Mở `/app/exam-sessions/<id>/edit`: dropdown Cấp lớp / Trường / Khoá / Lớp đổ đúng, cascade
Trường→Khoá→Lớp vẫn hoạt động.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/utils/options.ts exam_hub_web/src/pages/questions/QuestionBankPage.tsx exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx
git commit -m "refactor(web): add toOptions helper for select options"
```

---

### Task 5: `useDebounced` + gắn vào 4 ô tìm kiếm

Hiện mỗi ký tự gõ vào ô tìm kiếm đổi query key → một request mỗi ký tự.

**Files:**
- Create: `exam_hub_web/src/hooks/useDebounced.ts`
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx:104-107` (query memo)
- Modify: `exam_hub_web/src/pages/exams/ExamListPage.tsx` (query memo)
- Modify: `exam_hub_web/src/pages/exams/ExamSessionListPage.tsx` (query memo)
- Modify: `exam_hub_web/src/pages/student/StudentExamListPage.tsx:48-50` (query memo)

**Interfaces:**
- Consumes: không.
- Produces: `useDebounced<T>(value: T, delay?: number): T` — mặc định `delay = 300`.

- [ ] **Step 1: Tạo `src/hooks/useDebounced.ts`**

```ts
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
```

- [ ] **Step 2: `QuestionBankPage`**

Ô `<Input>` giữ nguyên `value={keyword}` (gõ vẫn mượt). Chỉ đổi giá trị đi vào query:

```tsx
// ngay sau: const [keyword, setKeyword] = useState('')
const debouncedKeyword = useDebounced(keyword)

// QuestionBankPage.tsx:104-107 — đổi `keyword` thành `debouncedKeyword` ở CẢ object và deps
const query: QuestionPagedQuery = useMemo(
    () => ({page, pageSize, keyword: debouncedKeyword, topicId, questionTypeId, difficultyLevelId,
            cognitiveLevelId, reviewStatus, subjectId, gradeLevelId}),
    [page, pageSize, debouncedKeyword, topicId, questionTypeId, difficultyLevelId,
     cognitiveLevelId, reviewStatus, subjectId, gradeLevelId],
)
```

Thêm import: `import {useDebounced} from '../../hooks/useDebounced'`

- [ ] **Step 3: 3 page còn lại**

Cùng một khuôn: thêm `const debouncedKeyword = useDebounced(keyword)` ngay sau khai báo
`keyword`, rồi thay `keyword` bằng `debouncedKeyword` **bên trong object query và trong mảng deps
của `useMemo` tương ứng**. Không đổi prop `value` của `<Input>`.

- `ExamListPage.tsx` — query memo dựng `ExamPagedQuery`
- `ExamSessionListPage.tsx` — query memo dựng query kỳ thi
- `StudentExamListPage.tsx:48-50` — `examsQuery`

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công (mở tab Network)**

Mở `/app/questions`, gõ liên tục 8 ký tự vào ô tìm kiếm.
Expected: ô input hiện đủ 8 ký tự tức thì; Network chỉ có **1** request danh sách câu hỏi sau khi
ngừng gõ ~300 ms (trước đây là 8). Lặp lại ở `/app/exam-list`, `/app/exam-sessions`,
`/student/exams`.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/hooks/useDebounced.ts exam_hub_web/src/pages
git commit -m "perf(web): debounce search keyword before query key"
```

---

### Task 6: Selector cho `useAuthStore`

Hai chỗ destructure toàn store nên `isRefreshing` bật/tắt mỗi lần refresh token là mọi consumer
re-render, kể cả `AppLayout`.

**Files:**
- Modify: `exam_hub_web/src/AuthProvider.tsx:12-15`
- Modify: `exam_hub_web/src/routes/ProtectedRoute.tsx:10`

**Interfaces:**
- Consumes: `useAuthStore` (đã có).
- Produces: `useAuth()` giữ nguyên chữ ký trả về `{token, user, isAuthenticated, login, logout, refresh}`
  — mọi call site hiện tại không đổi.

- [ ] **Step 1: `AuthProvider.tsx`**

```tsx
export function useAuth() {
    const token = useAuthStore(s => s.token)
    const user = useAuthStore(s => s.user)
    const isAuthenticated = useAuthStore(s => s.isAuthenticated)
    const login = useAuthStore(s => s.login)
    const logout = useAuthStore(s => s.logout)
    const refresh = useAuthStore(s => s.refresh)
    return {token, user, isAuthenticated, login, logout, refresh}
}
```

`login`/`logout`/`refresh` là hàm ổn định do zustand giữ nguyên identity giữa các lần set, nên
`useCallback` ở call site vẫn đúng.

- [ ] **Step 2: `ProtectedRoute.tsx:10`**

```tsx
    const isAuthenticated = useAuthStore(s => s.isAuthenticated)
    const user = useAuthStore(s => s.user)
```

- [ ] **Step 3: Xác nhận không còn chỗ gọi không selector**

Run: `cd exam_hub_web && grep -rn 'useAuthStore()' src`
Expected: không có kết quả.

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công**

Đăng nhập, điều hướng qua vài trang trong `/app`, đăng xuất, đăng nhập lại.
Kiểm: chuyển hướng khi chưa đăng nhập vẫn về `/login`; tài khoản không có role vẫn về `/no-role`;
avatar trên top-bar vẫn đúng.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/AuthProvider.tsx exam_hub_web/src/routes/ProtectedRoute.tsx
git commit -m "perf(web): select individual fields from auth store"
```

---

### Task 7: Tách `ExamTimer` khỏi `ExamRunner`

`ExamTakingPage.tsx:125-130` gọi `setTimeLeft` mỗi giây ở component gốc → re-render tờ đề,
sidebar và lưới 40 nút mỗi giây, suốt 45–90 phút.

**Files:**
- Create: `exam_hub_web/src/pages/student/ExamTimer.tsx`
- Modify: `exam_hub_web/src/pages/student/ExamTakingPage.tsx:56, 116-118, 125-130, 151-159, 199-201`

**Interfaces:**
- Consumes: `secondsUntil(deadlineAt: number, now?: number): number` từ
  `src/pages/student/examTimer.ts:7` (đã có).
- Produces: `ExamTimer` (named export) — `{ deadlineAt: number; onExpire: () => void }`.
  Task 8 không đụng file này.

- [ ] **Step 1: Tạo `src/pages/student/ExamTimer.tsx`**

```tsx
import {useEffect, useRef, useState} from 'react'
import {secondsUntil} from './examTimer'

type Props = {
    /** Mốc hết giờ tuyệt đối (ms epoch). */
    deadlineAt: number
    /** Gọi đúng một lần khi đồng hồ chạm 0. */
    onExpire: () => void
}

/**
 * Đồng hồ đếm ngược tự giữ state — tách khỏi ExamRunner để tick mỗi giây
 * không re-render tờ đề và lưới câu hỏi.
 */
export function ExamTimer({deadlineAt, onExpire}: Props) {
    const [timeLeft, setTimeLeft] = useState(() => secondsUntil(deadlineAt))
    const expired = useRef(false)
    const onExpireRef = useRef(onExpire)
    onExpireRef.current = onExpire

    useEffect(() => {
        const tick = () => setTimeLeft(secondsUntil(deadlineAt))
        tick()
        const id = setInterval(tick, 1000)
        return () => clearInterval(id)
    }, [deadlineAt])

    useEffect(() => {
        if (timeLeft === 0 && !expired.current) {
            expired.current = true
            onExpireRef.current()
        }
    }, [timeLeft])

    const mm = String(Math.floor(timeLeft / 60)).padStart(2, '0')
    const ss = String(timeLeft % 60).padStart(2, '0')
    const danger = timeLeft <= 300

    return (
        <div className={`take-timer ${danger ? 'take-timer--danger' : ''}`}
             role="timer" aria-live="off" aria-label={`Còn lại ${mm} phút ${ss} giây`}>
            <span className="take-timer-dot"/>{mm}:{ss}
        </div>
    )
}
```

`onExpireRef` giữ callback mới nhất mà không đưa `onExpire` vào deps — nếu đưa vào, hàm dựng lại
mỗi render sẽ reset interval liên tục.

- [ ] **Step 2: Gỡ state đồng hồ khỏi `ExamRunner`**

Trong `ExamTakingPage.tsx`, xoá:

- dòng `56`: `const [timeLeft, setTimeLeft] = useState(() => secondsUntil(effectiveDeadline))`
- dòng `57`: `const autoSubmitted = useRef(false)`
- dòng `116-118`: ba dòng `mm`, `ss`, `danger`
- dòng `125-130`: `useEffect` chứa `setInterval(tick, 1000)`
- dòng `151-159`: `useEffect` auto-nộp khi `timeLeft === 0`

Giữ nguyên `effectiveDeadline`, `setEffectiveDeadline` và `buildAndSubmit`.

- [ ] **Step 3: Thay JSX đồng hồ**

Xoá `ExamTakingPage.tsx:199-201`:

```tsx
                <div className={`take-timer ${danger ? 'take-timer--danger' : ''}`}>
                    <span className="take-timer-dot"/>{mm}:{ss}
                </div>
```

Thay bằng:

```tsx
                <ExamTimer deadlineAt={effectiveDeadline} onExpire={handleExpire}/>
```

Thêm ngay trên phần `return` của `ExamRunner`:

```tsx
    const handleExpire = () => {
        if (!studentId) return
        message.warning('Đã hết giờ làm bài. Hệ thống tự động nộp bài.')
        void buildAndSubmit()
    }
```

Cờ chống gọi hai lần đã nằm trong `ExamTimer` (`expired.current`), nên `autoSubmitted` ở
`ExamRunner` không còn cần.

Thêm import: `import {ExamTimer} from './ExamTimer'`.
Gỡ `secondsUntil` khỏi import ở dòng `13` nếu `ExamRunner` không còn dùng
(`deadlineFromDuration` vẫn dùng ở `:81`).

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh. Nếu TS báo `timeLeft`/`danger` không tồn tại, còn sót chỗ dùng ở JSX — sửa nốt.

- [ ] **Step 5: Kiểm thủ công**

Mở một kỳ thi đang mở, vào làm bài.
Kiểm: đồng hồ đếm lùi mỗi giây; đổi câu hỏi không làm đồng hồ nhảy hay reset; còn ≤ 5 phút thì
đồng hồ chuyển đỏ. Tạo một kỳ thi `durationMinutes = 1` và ngồi chờ hết giờ: hiện đúng một
message cảnh báo và tự nộp một lần, chuyển sang trang kết quả.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/pages/student/ExamTimer.tsx exam_hub_web/src/pages/student/ExamTakingPage.tsx
git commit -m "perf(web): isolate exam countdown into its own component"
```

---

### Task 8: Thay state `values` bằng tập câu đã trả lời (TDD)

`ExamTakingPage.tsx:204` gọi `setValues(form.getFieldsValue(true))` mỗi lần Form đổi → mỗi ký tự
trong ô tự luận dựng object mới và re-render cả `ExamRunner`. Form đã là nguồn duy nhất; `values`
chỉ phục vụ hai việc: đếm câu đã trả lời và tô màu ô trong lưới.

Đây là task rủi ro cao nhất của plan — làm theo đúng chu trình test trước.

**Files:**
- Create: `exam_hub_web/src/pages/student/answerState.ts`
- Create: `exam_hub_web/src/pages/student/answerState.test.ts`
- Modify: `exam_hub_web/src/pages/student/ExamTakingPage.tsx:16, 51, 97, 113, 176-182, 204`

**Interfaces:**
- Consumes: không.
- Produces:
  - `hasAnswer(v: unknown): boolean`
  - `answeredIds(values: Record<string, unknown>): Set<string>`
  - `sameSet(a: Set<string>, b: Set<string>): boolean`

- [ ] **Step 1: Viết test trước — `src/pages/student/answerState.test.ts`**

```ts
import {describe, expect, it} from 'vitest'
import {answeredIds, hasAnswer, sameSet} from './answerState'

describe('hasAnswer', () => {
    it('treats a non-empty string as answered', () => {
        expect(hasAnswer('a1')).toBe(true)
    })

    it('treats an empty string as unanswered', () => {
        expect(hasAnswer('')).toBe(false)
    })

    it('treats a whitespace-only essay as unanswered', () => {
        expect(hasAnswer('   \n  ')).toBe(false)
    })

    it('treats undefined and null as unanswered', () => {
        expect(hasAnswer(undefined)).toBe(false)
        expect(hasAnswer(null)).toBe(false)
    })
})

describe('answeredIds', () => {
    it('keeps only the ids that have an answer', () => {
        const ids = answeredIds({q1: 'a1', q2: '', q3: 'bài làm', q4: undefined})
        expect([...ids].sort()).toEqual(['q1', 'q3'])
    })

    it('returns an empty set for no values', () => {
        expect(answeredIds({}).size).toBe(0)
    })
})

describe('sameSet', () => {
    it('is true for equal contents regardless of insertion order', () => {
        expect(sameSet(new Set(['a', 'b']), new Set(['b', 'a']))).toBe(true)
    })

    it('is false when one has an extra member', () => {
        expect(sameSet(new Set(['a']), new Set(['a', 'b']))).toBe(false)
    })

    it('is false for equal sizes but different members', () => {
        expect(sameSet(new Set(['a']), new Set(['b']))).toBe(false)
    })
})
```

- [ ] **Step 2: Chạy test để xác nhận nó fail**

Run: `cd exam_hub_web && npx vitest run src/pages/student/answerState.test.ts`
Expected: FAIL — không resolve được module `./answerState`.

- [ ] **Step 3: Viết `src/pages/student/answerState.ts`**

```ts
/** Một câu được coi là đã trả lời khi có id đáp án, hoặc bài tự luận có nội dung. */
export const hasAnswer = (v: unknown): boolean =>
    typeof v === 'string' ? v.trim().length > 0 : v != null

/** Tập id câu hỏi đã có đáp án, lấy từ giá trị Form. */
export const answeredIds = (values: Record<string, unknown>): Set<string> => {
    const ids = new Set<string>()
    for (const [id, v] of Object.entries(values)) {
        if (hasAnswer(v)) ids.add(id)
    }
    return ids
}

/** So sánh nông hai tập — dùng để bỏ qua setState khi tập không đổi. */
export const sameSet = (a: Set<string>, b: Set<string>): boolean => {
    if (a.size !== b.size) return false
    for (const v of a) if (!b.has(v)) return false
    return true
}
```

- [ ] **Step 4: Chạy test để xác nhận nó pass**

Run: `cd exam_hub_web && npx vitest run src/pages/student/answerState.test.ts`
Expected: PASS — 9 test.

Run: `cd exam_hub_web && npm test`
Expected: toàn bộ test cũ vẫn xanh.

- [ ] **Step 5: Thay `values` trong `ExamTakingPage`**

Xoá dòng `16`:

```tsx
const hasAnswer = (v: unknown) => (typeof v === 'string' ? v.trim().length > 0 : v != null)
```

Thêm import: `import {answeredIds, hasAnswer, sameSet} from './answerState'`

Đổi dòng `51`:

```tsx
// trước
const [values, setValues] = useState<Record<string, unknown>>({})
// sau
const [answered, setAnswered] = useState<Set<string>>(new Set())
```

Đổi dòng `204` (`onValuesChange` của Form) — chỉ setState khi tập thực sự đổi:

```tsx
            <Form form={form} component={false}
                  onValuesChange={() => {
                      const next = answeredIds(form.getFieldsValue(true))
                      setAnswered(prev => (sameSet(prev, next) ? prev : next))
                  }}>
```

Đổi dòng `113`:

```tsx
// trước
const answeredCount = questions.filter(q => hasAnswer(values[q.id])).length
// sau
const answeredCount = questions.reduce((n, q) => n + (answered.has(q.id) ? 1 : 0), 0)
```

Đổi dòng `176-182` (`cellClass`):

```tsx
    const cellClass = (q: {id: string}, idx: number) => {
        const isAnswered = answered.has(q.id)
        if (idx === activeIdx) return `take-cell take-cell--current${isAnswered ? ' take-cell--current-answered' : ''}`
        if (flagged.has(q.id)) return 'take-cell take-cell--flagged'
        if (isAnswered) return 'take-cell take-cell--answered'
        return 'take-cell'
    }
```

Đổi dòng `97` (khôi phục đáp án đã lưu) — sau khi `form.setFieldsValue(merged)`:

```tsx
            form.setFieldsValue(merged)
            setAnswered(answeredIds(merged))
```

(thay cho `setValues(prev => ({ ...restored, ...prev }))`)

`hasAnswer` vẫn được import vì dùng trong `answeredIds`; nếu `ExamRunner` không còn gọi trực tiếp,
bỏ khỏi danh sách import.

- [ ] **Step 6: Build + test**

Run: `cd exam_hub_web && npm run build && npm test`
Expected: cả hai xanh.

- [ ] **Step 7: Kiểm thủ công — cả luồng làm bài**

Vào làm một bài có ít nhất 1 câu tự luận và vài câu trắc nghiệm:

1. Chọn đáp án câu 1 → ô 1 trong lưới chuyển xanh, "Đã trả lời" tăng 1, thanh tiến độ tăng.
2. Bỏ chọn / đổi đáp án → số đếm không nhảy sai.
3. Gõ vào ô tự luận: ký tự đầu → ô chuyển xanh, đếm +1; gõ tiếp 20 ký tự → số đếm **đứng yên**,
   con trỏ không nhảy, không mất ký tự.
4. Xoá sạch ô tự luận → ô về xám, đếm −1.
5. Đánh dấu vài câu → số "Đã đánh dấu" đúng.
6. Chờ qua mốc autosave 20 giây, F5 tải lại trang → đáp án đã nhập được khôi phục **và** lưới
   hiện đúng các ô xanh (đây là chỗ Step 5 sửa dòng `97`).
7. Nộp bài → điểm/kết quả đúng với các đáp án đã chọn.

- [ ] **Step 8: Commit**

```bash
git add exam_hub_web/src/pages/student/answerState.ts exam_hub_web/src/pages/student/answerState.test.ts exam_hub_web/src/pages/student/ExamTakingPage.tsx
git commit -m "perf(web): track answered ids instead of mirroring form values"
```

---

### Task 9: Khởi tạo lười + `toSorted`

Hai sửa nhỏ độc lập, gộp một commit.

**Files:**
- Modify: `exam_hub_web/src/pages/student/ExamTakingPage.tsx:54-55`
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx:300-302`

**Interfaces:**
- Consumes: không. Produces: không.

- [ ] **Step 1: `ExamTakingPage` — bỏ tính toán thừa mỗi render**

`useRef(Date.now() + …)` tính biểu thức ở **mọi** render dù chỉ dùng giá trị lần đầu.

```tsx
// trước (dòng 54-55)
    const fallbackDeadline = useRef(Date.now() + exam.durationMinutes * 60_000)
    const [effectiveDeadline, setEffectiveDeadline] = useState(deadlineAt ?? fallbackDeadline.current)

// sau
    const [effectiveDeadline, setEffectiveDeadline] = useState(
        () => deadlineAt ?? Date.now() + exam.durationMinutes * 60_000,
    )
```

Gỡ `useRef` khỏi import ở dòng `1` nếu file không còn chỗ dùng nào khác (Task 7 đã chuyển
`autoSubmitted` sang `ExamTimer`; `timerSyncSubmission` ở dòng `58` vẫn dùng `useRef`, nên nhiều
khả năng vẫn phải giữ — kiểm lại trước khi gỡ).

- [ ] **Step 2: `QuestionBankPage` — sắp xếp bất biến, có memo**

```tsx
// trước (dòng 300-302), nằm trong JSX phần "Bloom legend"
{[...(cognitives.data ?? [])].sort((a, b) => a.levelOrder - b.levelOrder).map(c => (
    <Chip key={c.id} label={`${c.levelOrder}.${c.name}`} color={BLOOM_CHIP[c.code] ?? NEUTRAL_CHIP}/>
))}
```

Thêm cạnh các `useMemo` khác (gần dòng `149-157`):

```tsx
    const bloomLegend = useMemo(
        () => (cognitives.data ?? []).toSorted((a, b) => a.levelOrder - b.levelOrder),
        [cognitives.data])
```

Rồi trong JSX:

```tsx
{bloomLegend.map(c => (
    <Chip key={c.id} label={`${c.levelOrder}.${c.name}`} color={BLOOM_CHIP[c.code] ?? NEUTRAL_CHIP}/>
))}
```

- [ ] **Step 3: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh. `toSorted` cần `lib` ES2023 — nếu TS báo không có trên `Array`, đổi
`compilerOptions.lib` trong `exam_hub_web/tsconfig.app.json` lên `["ES2023", "DOM", "DOM.Iterable"]`
và build lại. (`target` đã là ES2022 trở lên trong template Vite.)

- [ ] **Step 4: Kiểm thủ công**

`/app/questions`: dải chip Bloom vẫn hiện đủ, đúng thứ tự 1→6.
`/student/exam/take?...`: đồng hồ khởi tạo đúng thời lượng khi vào bài lần đầu (không có
`deadlineAt` trên URL), và đúng thời gian còn lại khi vào lại bài dở.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/src/pages/student/ExamTakingPage.tsx exam_hub_web/src/pages/questions/QuestionBankPage.tsx
git commit -m "perf(web): lazy deadline init, memoize bloom legend sort"
```

---

### Task 10: Lazy route + Suspense + errorElement

`routes/index.tsx:52-82` import tĩnh cả 30 page, kéo `recharts`, `@tiptap/*`, `katex` vào chunk
khởi động cho mọi người dùng.

**Files:**
- Create: `exam_hub_web/src/components/RouteError.tsx`
- Modify: `exam_hub_web/src/routes/index.tsx`
- Modify: `exam_hub_web/src/layouts/AppLayout.tsx:313-315`
- Modify: `exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx` (lazy `AnalyticsDrawer`)

**Interfaces:**
- Consumes: không.
- Produces: `RouteError` (default export), không nhận props.

- [ ] **Step 1: Đo bundle trước khi sửa**

Run: `cd exam_hub_web && npm run build && ls -l dist/assets/*.js`
Ghi lại tên + kích thước từng file vào chỗ nháp — Step 7 so lại.

- [ ] **Step 2: Tạo `src/components/RouteError.tsx`**

```tsx
import {Button, Result} from 'antd'
import {useRouteError} from 'react-router-dom'

/** Màn hiển thị khi một route ném lỗi — thay cho màn trắng. */
export default function RouteError() {
    const error = useRouteError()
    console.error(error)

    return (
        <Result
            status="error"
            title="Đã xảy ra lỗi"
            subTitle="Không tải được nội dung trang. Thử tải lại giúp mình nhé."
            extra={<Button type="primary" onClick={() => window.location.reload()}>Tải lại trang</Button>}
        />
    )
}
```

- [ ] **Step 3: Lazy 4 page nặng trong `routes/index.tsx`**

Đổi 4 dòng import tĩnh thành lazy:

```tsx
import {createBrowserRouter, Navigate} from 'react-router-dom'
import {lazy} from 'react'
// … các import tĩnh khác giữ nguyên …
import RouteError from '../components/RouteError'

const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage'))
const AddQuestionPage = lazy(() => import('../pages/questions/AddQuestionPage'))
const GeneratePage = lazy(() => import('../pages/exams/GeneratePage'))
const CreateExamTemplatePage = lazy(() => import('../pages/exams/CreateExamTemplatePage'))
```

Gỡ 4 dòng `import DashboardPage from …` / `AddQuestionPage` / `GeneratePage` /
`CreateExamTemplatePage` cũ. Phần `element: <DashboardPage/>` … trong mảng route giữ nguyên.

Thêm `errorElement` cho route gốc — bọc mảng hiện tại trong một route cha:

```tsx
export const router = createBrowserRouter([
    {
        errorElement: <RouteError/>,
        children: [
            { path: ROUTES.HOME, element: <Navigate to={ROUTES.LOGIN} replace/> },
            // … toàn bộ các route hiện có, giữ nguyên thứ tự và nội dung …
        ],
    },
])
```

- [ ] **Step 4: Suspense trong `AppLayout`**

Cả 4 page lazy đều nằm dưới `AppLayout`, nên một `<Suspense>` ở đây là đủ.

```tsx
// AppLayout.tsx:313-315 — trước
            <div className="page-canvas">
                <Outlet/>
            </div>

// sau
            <div className="page-canvas">
                <Suspense fallback={<div className="flex-1 flex items-center justify-center"><Spin size="large"/></div>}>
                    <Outlet/>
                </Suspense>
            </div>
```

Thêm import: `import {Suspense} from 'react'` (gộp vào dòng import react sẵn có) và
`import {Spin} from 'antd'`.

- [ ] **Step 5: Lazy `AnalyticsDrawer`**

`AnalyticsDrawer` kéo `recharts` vào `ExamSessionEditPage` dù chỉ mở khi bấm "Phân tích".

```tsx
// ExamSessionEditPage.tsx — thay import tĩnh dòng 38
const AnalyticsDrawer = lazy(() => import('./AnalyticsDrawer').then(m => ({default: m.AnalyticsDrawer})))
```

`.then(...)` là bắt buộc: `AnalyticsDrawer.tsx:9` là named export, còn `lazy()` chỉ nhận module
có default export.

Bọc chỗ dùng trong `PoolSection` (dòng `283`):

```tsx
            <Suspense fallback={null}>
                <AnalyticsDrawer examId={analyticsExamId} onClose={() => setAnalyticsExamId(undefined)}/>
            </Suspense>
```

Thêm `lazy, Suspense` vào import react của file.

- [ ] **Step 6: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 7: Đo lại bundle**

Run: `cd exam_hub_web && ls -l dist/assets/*.js`
Expected: xuất hiện thêm các chunk tách riêng; chunk chính nhỏ hơn số ghi ở Step 1.
Kiểm `recharts`, `tiptap`, `katex` không còn trong chunk chính:

Run: `cd exam_hub_web && grep -l "recharts\|tiptap\|katex" dist/assets/*.js`
Expected: chỉ khớp các chunk phụ, không khớp chunk `index-*.js`.

Ghi mức giảm vào commit message ở Step 9.

- [ ] **Step 8: Kiểm thủ công**

Mở lần lượt `/app/dashboard`, `/app/questions/add`, `/app/generate`, `/app/exams/create` — mỗi
trang hiện `<Spin>` chớp nhoáng rồi render đầy đủ, không lỗi console.
Mở `/app/exam-sessions/<id>/edit` → bấm "Phân tích" ở một đề → drawer mở, biểu đồ cột hiện đúng.
Sửa tạm một page để ném lỗi (`throw new Error('test')` ở đầu component), mở trang đó: hiện màn
`RouteError` thay vì màn trắng. **Hoàn tác sửa tạm này trước khi commit.**

- [ ] **Step 9: Commit**

```bash
git add exam_hub_web/src/components/RouteError.tsx exam_hub_web/src/routes/index.tsx exam_hub_web/src/layouts/AppLayout.tsx exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx
git commit -m "perf(web): lazy-load heavy routes and analytics drawer

Main chunk <trước> -> <sau> KB."
```

---

### Task 11: Font không chặn render

`index.css:1-2` dùng hai `@import` Google Fonts — trình duyệt chỉ biết đến chúng sau khi tải và
parse xong CSS, tạo chuỗi tải nối tiếp.

**Files:**
- Modify: `exam_hub_web/index.html:3-7`
- Modify: `exam_hub_web/src/index.css:1-2` (xoá)

**Interfaces:**
- Consumes: không. Produces: không.

- [ ] **Step 1: Thêm link vào `index.html`**

Chèn vào `<head>`, ngay trước `<title>`:

```html
    <link rel="preconnect" href="https://fonts.googleapis.com"/>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin/>
    <link rel="stylesheet"
          href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Lora:ital,wght@0,400;0,500;0,600;0,700;1,400;1,500&display=swap"/>
```

Hai họ font gộp vào một request (`&family=`), giữ đúng các weight/style mà hai `@import` cũ xin.

- [ ] **Step 2: Xoá 2 dòng `@import` trong `index.css`**

Xoá dòng `1` và `2`. Giữ nguyên dòng `@import "tailwindcss";`.

- [ ] **Step 3: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 4: Kiểm thủ công**

Run: `cd exam_hub_web && npm run dev`
Mở DevTools → Network → lọc `Font`. Tải lại trang.
Expected: request font bắt đầu ngay đầu waterfall (song song với CSS), không phải sau khi CSS tải xong.
Mở `/student/exam/take?...` — chữ trên tờ đề vẫn là Inter; các chỗ dùng Lora (nếu có) vẫn đúng font,
không bị rơi về serif mặc định.

- [ ] **Step 5: Commit**

```bash
git add exam_hub_web/index.html exam_hub_web/src/index.css
git commit -m "perf(web): preconnect fonts instead of css @import"
```

---

### Task 12: Sidebar dùng `NavLink`

`AppLayout.tsx:245-291` dùng `<button onClick={navigate}>` — không Ctrl/middle-click mở tab mới
được, không có `aria-current`, screen reader không biết mục nào đang mở.

**Files:**
- Modify: `exam_hub_web/src/layouts/AppLayout.tsx:146-147, 245-310`

**Interfaces:**
- Consumes: không. Produces: không.

- [ ] **Step 1: Đổi import**

```tsx
import {NavLink, Outlet, useLocation, useNavigate} from 'react-router-dom'
```

(`useNavigate` vẫn cần cho `handleLogout` và effect kiểm token; `useLocation` vẫn cần cho
`isChildActive`.)

- [ ] **Step 2: Mục cha có children — thêm `aria-expanded`**

```tsx
                                    <button
                                        onClick={() => toggleGroup(item.key, activeChild)}
                                        aria-expanded={open}
                                        className={`sidebar-nav-item ${activeChild ? 'sidebar-nav-item--active' : ''}`}
                                    >
```

- [ ] **Step 3: Mục con → `NavLink`**

```tsx
                                            {item.children.map((child) => (
                                                <NavLink
                                                    key={child.key}
                                                    to={child.path ?? '#'}
                                                    className={({isActive}) =>
                                                        `sidebar-nav-item ${isActive ? 'sidebar-nav-item--active' : ''}`}
                                                >
                                                    <span className="text-base">{ICON_MAP[child.icon] ?? <AppstoreOutlined/>}</span>
                                                    <span>{child.label}</span>
                                                </NavLink>
                                            ))}
```

`NavLink` tự gắn `aria-current="page"` khi active — không cần thêm tay.

- [ ] **Step 4: Mục lá ở cấp một → `NavLink`**

```tsx
                        return (
                            <NavLink
                                key={item.key}
                                to={item.path ?? '#'}
                                className={({isActive}) =>
                                    `sidebar-nav-item ${isActive ? 'sidebar-nav-item--active' : ''}`}
                            >
                                <span className="text-base">{ICON_MAP[item.icon] ?? <AppstoreOutlined/>}</span>
                                <span>{item.label}</span>
                            </NavLink>
                        )
```

- [ ] **Step 5: Mục "Tài khoản" ở footer → `NavLink`**

```tsx
                    <NavLink
                        to="/app/profile"
                        className={({isActive}) => `sidebar-nav-item ${isActive ? 'sidebar-nav-item--active' : ''}`}
                    >
                        <UserOutlined/>
                        <span>Tài khoản</span>
                    </NavLink>
```

Nút "Đăng xuất" giữ nguyên `<button>` — nó là hành động, không phải điều hướng.

- [ ] **Step 6: Kiểm khác biệt khớp active**

Code cũ dùng `location.pathname.startsWith(path)`; `NavLink` mặc định khớp cả nhánh con
(không cần `end`), nên hành vi tương đương. Riêng mục `/app/questions`: khi đang ở
`/app/questions/add`, cả hai cách đều tô sáng "Câu hỏi" — đúng như hiện tại.
`isChildActive` (dòng `206-207`) vẫn dùng `location`, giữ nguyên.

- [ ] **Step 7: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 8: Kiểm thủ công**

- Bấm từng mục sidebar: điều hướng đúng, mục đang mở vẫn được tô sáng y như trước.
- Mở/đóng nhóm "Quản lý đề thi": vẫn hoạt động.
- Ctrl+click (Cmd+click trên macOS) một mục → mở tab mới đúng URL.
- Chuột giữa một mục → mở tab mới.
- Chỉ dùng Tab + Enter đi hết sidebar → tới được mọi mục.
- DevTools → chọn mục đang active → có `aria-current="page"`; nút nhóm có `aria-expanded`.

- [ ] **Step 9: Commit**

```bash
git add exam_hub_web/src/layouts/AppLayout.tsx
git commit -m "a11y(web): sidebar navigation as real links"
```

---

### Task 13: CSS a11y — focus, reduced-motion, tương phản

**Files:**
- Modify: `exam_hub_web/src/index.css` (thêm khối mới sau phần Reset; sửa `:591`, `:564`)

**Interfaces:**
- Consumes: không. Produces: không.

- [ ] **Step 1: Thêm `:focus-visible` dùng chung**

Chèn ngay sau khối `/* ─── Reset ─── */` (sau dòng `#root { … }`):

```css
/* ─── Focus (bàn phím) ──────────────────────────── */
:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
  border-radius: 4px;
}
```

Dùng `:focus-visible` (không phải `:focus`) nên click chuột không hiện viền — giữ nguyên diện mạo
hiện tại cho người dùng chuột.

- [ ] **Step 2: Tôn trọng `prefers-reduced-motion`**

Chèn ngay dưới khối trên:

```css
@media (prefers-reduced-motion: reduce) {
  .exam-timer--danger,
  .take-timer--danger { animation: none; }
  .exam-paper { animation: none; }
  *, *::before, *::after {
    transition-duration: 0.01ms !important;
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
  }
}
```

Hai đồng hồ đang `animate-pulse` vô hạn (`index.css:827`, `:1040`) và `.exam-paper` chạy
`paper-rise` (`:841`).

- [ ] **Step 3: Nâng tương phản hai lớp chữ phụ**

```css
/* index.css:590-593 — .table-th */
.table-th {
  @apply px-4 py-3 text-left text-[11px] font-semibold text-gray-600
         uppercase tracking-wide bg-gray-50;
}

/* index.css:564-566 — .stat-card-label */
.stat-card-label {
  @apply text-[11px] text-gray-600;
}
```

`text-gray-400` trên nền trắng ≈ 3:1, dưới ngưỡng AA 4.5:1 cho chữ thường. `gray-600` đạt ngưỡng.

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công**

- Tab qua sidebar, nút, ô input: mỗi phần tử có viền xanh rõ ràng. Click chuột: **không** có viền.
- Bật reduced-motion của hệ điều hành (Windows: Settings → Accessibility → Visual effects →
  tắt Animation effects), mở trang làm bài lúc còn < 5 phút: đồng hồ đỏ nhưng **không** nhấp nháy;
  tờ đề hiện ra không có hiệu ứng trượt.
- Tắt lại reduced-motion: hiệu ứng trở lại như cũ.
- Mở `/app/dashboard` và `/app/questions`: chữ tiêu đề cột bảng và nhãn thẻ số đậm hơn trước,
  bố cục không xê dịch.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/index.css
git commit -m "a11y(web): focus-visible ring, reduced-motion, AA contrast"
```

---

### Task 14: Nhãn cho lưới câu hỏi

**Files:**
- Modify: `exam_hub_web/src/pages/student/ExamTakingPage.tsx:297-304`

**Interfaces:**
- Consumes: `answered: Set<string>`, `flagged: Set<string>`, `activeIdx` từ Task 8 (đã có trong
  `ExamRunner`).
- Produces: không.

- [ ] **Step 1: Thêm `aria-label` và `aria-current`**

```tsx
                            <div className="take-grid" role="group" aria-label="Bảng câu hỏi">
                                {questions.map((q, idx) => (
                                    <button key={q.id} type="button" className={cellClass(q, idx)}
                                            aria-current={idx === activeIdx ? 'true' : undefined}
                                            aria-label={`Câu ${idx + 1}${answered.has(q.id) ? ', đã trả lời' : ', chưa trả lời'}${flagged.has(q.id) ? ', đã đánh dấu' : ''}`}
                                            onClick={() => go(idx)}>
                                        {idx + 1}
                                    </button>
                                ))}
                            </div>
```

Trạng thái ô hiện chỉ truyền tải bằng màu — screen reader và người mù màu không đọc được.

- [ ] **Step 2: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 3: Kiểm thủ công**

Vào trang làm bài, DevTools → chọn một ô trong lưới → kiểm `aria-label` đổi đúng sau khi trả lời
câu đó và sau khi đánh dấu. Ô đang mở có `aria-current="true"`.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_web/src/pages/student/ExamTakingPage.tsx
git commit -m "a11y(web): label question grid cells with their state"
```

---

### Task 15: Một nguồn khai báo màu

`index.css:376-389` (`@theme`) và `App.tsx:24-39` (antd `ConfigProvider`) hiện khai báo lại cùng
bảng màu ở hai nơi.

**Files:**
- Create: `exam_hub_web/src/constants/theme.ts`
- Modify: `exam_hub_web/src/App.tsx:24-39`

**Interfaces:**
- Consumes: không.
- Produces: `BRAND` — object hằng, các khoá:
  `primary, primarySoft, success, successSoft, danger, dangerSoft, warning, warningSoft, ink, muted, border, surface, sidebar` — mọi giá trị là chuỗi hex.
  Task 16 dùng lại object này.

- [ ] **Step 1: Tạo `src/constants/theme.ts`**

Giá trị lấy đúng từ `@theme` trong `index.css:376-389` — không đổi một hex nào.

```ts
/**
 * Bảng màu thương hiệu. Giữ đồng bộ với khối @theme trong src/index.css —
 * antd đọc từ đây, Tailwind đọc từ @theme.
 */
export const BRAND = {
    primary: '#3a74f5',
    primarySoft: '#e9ecfe',
    success: '#1ea375',
    successSoft: '#dff5ed',
    danger: '#e74242',
    dangerSoft: '#fee5e5',
    warning: '#d98a00',
    warningSoft: '#fff4e5',
    ink: '#191d27',
    muted: '#6f7788',
    border: '#eceef2',
    surface: '#f5f5f6',
    sidebar: '#191d27',
} as const
```

- [ ] **Step 2: `App.tsx` đọc từ `BRAND`**

```tsx
import {ConfigProvider} from 'antd'
import {RouterProvider} from 'react-router-dom'
import {router} from './routes'
import {AuthProvider} from './AuthProvider'
import {BRAND} from './constants/theme'

const theme = {
  token: {
    colorPrimary: BRAND.primary,
    colorSuccess: BRAND.success,
    colorError: BRAND.danger,
    colorWarning: BRAND.warning,
    colorLink: BRAND.primary,
    colorTextHeading: BRAND.ink,
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    borderRadius: 8,
  },
  components: {
    Table: {headerBg: BRAND.surface, headerColor: BRAND.muted, borderColor: '#f0f1f4'},
    Button: {controlHeight: 38},
  },
}
```

`#f0f1f4` không có trong `@theme` — giữ nguyên hex tại chỗ, không tự đặt tên token mới.

- [ ] **Step 3: Thêm comment trỏ ngược trong `index.css`**

Ngay trên khối `@theme` (dòng `374`):

```css
/* ─── Design tokens (Figma) ──────────────────────
   Giữ đồng bộ với src/constants/theme.ts (antd đọc từ đó). */
```

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công**

Mở `/app/questions`: màu nút primary, header bảng, màu link — không đổi so với trước.
Mở `/app/exam-sessions`: `<Tag>` các trạng thái giữ nguyên màu.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/constants/theme.ts exam_hub_web/src/App.tsx exam_hub_web/src/index.css
git commit -m "refactor(web): single source for brand colors"
```

---

### Task 16: Gộp `StatCard` trùng lặp + bỏ hex hardcode

`StatCard` hiện có **hai bản giống hệt nhau** (khác đúng một chỗ xuống dòng):
`QuestionBankPage.tsx:63-80` và `ExamTemplatePage.tsx:24-41`. Gộp thành một component dùng chung,
đồng thời thay hex bằng `BRAND` — sửa một lần cho cả hai.

**Files:**
- Create: `exam_hub_web/src/components/StatCard.tsx`
- Modify: `exam_hub_web/src/components/StatusTag.tsx:1-6`
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx:63-80` (xoá bản cục bộ), `:274-279` (call site)
- Modify: `exam_hub_web/src/pages/exams/ExamTemplatePage.tsx:24-41` (xoá bản cục bộ), `:140-143` (call site)

**Interfaces:**
- Consumes: `BRAND` từ `src/constants/theme.ts` (Task 15).
- Produces: `StatCard` (named export) —
  `{ label: string; value?: number; icon: ReactNode; color: string; bg: string }`.

- [ ] **Step 1: `StatusTag.tsx`**

```tsx
import {BRAND} from '../constants/theme'

const MAP = {
    success: {bg: BRAND.successSoft, fg: BRAND.success},
    danger: {bg: BRAND.dangerSoft, fg: BRAND.danger},
    warning: {bg: BRAND.warningSoft, fg: BRAND.warning},
    default: {bg: '#eef0f3', fg: BRAND.muted},
} as const
```

`#eef0f3` không có trong `@theme` — giữ nguyên.

- [ ] **Step 2: Tạo `src/components/StatCard.tsx`**

```tsx
import type {ReactNode} from 'react'
import {BRAND} from '../constants/theme'

type Props = {
    label: string
    value?: number
    icon: ReactNode
    /** Màu chữ của ô icon. */
    color: string
    /** Màu nền của ô icon. */
    bg: string
}

export function StatCard({label, value, icon, color, bg}: Props) {
    return (
        <div className="flex-1 bg-white rounded-xl border p-4 flex items-center gap-3"
             style={{borderColor: BRAND.border}}>
            <div className="w-10 h-10 rounded-lg flex items-center justify-center text-[18px]"
                 style={{background: bg, color}}>
                {icon}
            </div>
            <div>
                <div className="text-[22px] font-bold leading-tight" style={{color: BRAND.ink}}>
                    {value != null ? value.toLocaleString('vi-VN') : '—'}
                </div>
                <div className="text-[12px]" style={{color: BRAND.muted}}>{label}</div>
            </div>
        </div>
    )
}
```

Markup sao nguyên văn từ `QuestionBankPage.tsx:63-80` (bản có xuống dòng), chỉ thay 3 hex
`#eceef2` → `BRAND.border`, `#191d27` → `BRAND.ink`, `#6f7788` → `BRAND.muted`.

- [ ] **Step 3: Xoá hai bản cục bộ, đổi call site**

Xoá `function StatCard` ở `QuestionBankPage.tsx:63-80` và `ExamTemplatePage.tsx:24-41`.
Thêm vào cả hai file: `import {StatCard} from '../../components/StatCard'`.

`ExamTemplatePage` còn có `function BoolIcon` ngay dưới `StatCard` — **giữ nguyên**, chỉ dùng
trong file đó.

Ở call site, đổi các hex trùng `BRAND`:

```tsx
// QuestionBankPage.tsx:274-279
<StatCard label="Tổng câu hỏi"  value={stats.data?.total}    icon={<DatabaseOutlined/>}    color={BRAND.primary} bg="#eef1ff"/>
<StatCard label="Đã duyệt"      value={stats.data?.verified} icon={<CheckCircleFilled/>}   color={BRAND.success} bg="#e7f7ef"/>
<StatCard label="Chờ duyệt"     value={stats.data?.pending}  icon={<ClockCircleFilled/>}   color={BRAND.warning} bg={BRAND.warningSoft}/>
<StatCard label="Bị từ chối"    value={stats.data?.rejected} icon={<CloseCircleFilled/>}   color={BRAND.danger}  bg={BRAND.dangerSoft}/>
<StatCard label="Không HĐ"      value={stats.data?.inactive} icon={<StopOutlined/>}        color={BRAND.muted}   bg="#eef0f3"/>
```

```tsx
// ExamTemplatePage.tsx:140-143
<StatCard label="Tổng mẫu"        value={stats.data?.totalTemplates}      icon={<DatabaseOutlined/>}   color={BRAND.primary} bg="#eef1ff"/>
<StatCard label="Đang dùng"       value={stats.data?.activeTemplates}     icon={<CheckCircleFilled/>}  color={BRAND.success} bg="#e7f7ef"/>
<StatCard label="Tổng đề sinh"    value={stats.data?.totalExamsGenerated} icon={<ThunderboltFilled/>}  color="#8b5cf6"      bg="#f3ecfe"/>
<StatCard label="Trung bình câu"  value={stats.data?.avgQuestions}        icon={<BarsOutlined/>}       color={BRAND.warning} bg={BRAND.warningSoft}/>
```

`#eef1ff`, `#e7f7ef`, `#eef0f3`, `#8b5cf6`, `#f3ecfe` không có trong `@theme` — giữ nguyên hex,
không tự đặt token mới.

Thêm `import {BRAND} from '../../constants/theme'` vào cả hai page.

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công — đối chiếu ảnh**

Chụp màn `/app/questions` (hàng 5 thẻ số + cột "Duyệt") và `/app/exams` (hàng 4 thẻ số) **trước**
khi sửa (dùng `git stash` nếu cần) và **sau** khi sửa. So hai ảnh: màu, viền, khoảng cách phải
giống hệt từng pixel.
Kiểm cả 4 biến thể `StatusTag` (Đã duyệt / Chờ duyệt / Bị từ chối / trạng thái mặc định).
Kiểm `BoolIcon` ở `/app/exams` vẫn render đúng.

- [ ] **Step 6: Xác nhận không còn bản sao**

Run: `cd exam_hub_web && grep -rn 'function StatCard' src`
Expected: không có kết quả (component dùng `export function StatCard` trong
`src/components/StatCard.tsx`, khớp bằng `grep -rn 'export function StatCard' src` → đúng 1 dòng).

- [ ] **Step 7: Commit**

```bash
git add exam_hub_web/src/components exam_hub_web/src/pages
git commit -m "refactor(web): share StatCard, use brand tokens"
```

---

### Task 17: `bulkVerify` không nuốt lỗi

`QuestionBankPage.tsx:163-167` dùng `Promise.all` — một câu lỗi là reject cả chùm: không message,
không clear selection, không invalidate. `bulkDelete` ngay dưới đã làm đúng bằng cách đếm kết quả.

**Files:**
- Modify: `exam_hub_web/src/pages/questions/QuestionBankPage.tsx:163-167`

**Interfaces:**
- Consumes: `questionService.verify(id)`, `statusCode` (đã có).
- Produces: không.

- [ ] **Step 1: Sửa `bulkVerify`**

`questionService.verify` (`src/services/questionService.ts:30`) trả `AuthHttp.post<void>` — cùng
kiểu response có field `status` như `remove` mà `bulkDelete` đang đọc. Nên đếm cả lỗi cấp nghiệp
vụ, không chỉ lỗi mạng:

```tsx
    const bulkVerify = async () => {
        const results = await Promise.allSettled(selectedRowKeys.map(id => questionService.verify(id)))
        const succeeded = results.filter(
            r => r.status === 'fulfilled' && r.value.status !== statusCode.Error).length
        const failed = results.length - succeeded
        if (failed === 0) {
            message.success(`Đã duyệt ${succeeded} câu hỏi`)
        } else if (succeeded === 0) {
            message.error(`Không thể duyệt ${failed} câu hỏi`)
        } else {
            message.warning(`Đã duyệt ${succeeded} câu hỏi, ${failed} câu hỏi thất bại`)
        }
        setSelectedRowKeys([]); invalidate()
    }
```

`statusCode` đã được import sẵn trong file (dòng `36`).

- [ ] **Step 2: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 3: Kiểm thủ công**

`/app/questions`: chọn 3 câu chờ duyệt → "Duyệt hàng loạt" → message "Đã duyệt 3 câu hỏi", danh
sách refresh, bỏ chọn hết.
Kiểm đường lỗi: DevTools → Network → bật offline, chọn 2 câu, bấm duyệt → hiện message lỗi (không
im lặng), selection vẫn được clear.

- [ ] **Step 4: Commit**

```bash
git add exam_hub_web/src/pages/questions/QuestionBankPage.tsx
git commit -m "fix(web): report partial failures in bulk verify"
```

---

### Task 18: Route kết quả thi vào `ROUTES`

**Files:**
- Modify: `exam_hub_web/src/routes/paths.ts`
- Modify: `exam_hub_web/src/routes/index.tsx` (dòng route `/student/exam/result`)
- Modify: `exam_hub_web/src/pages/student/ExamTakingPage.tsx:145`

**Interfaces:**
- Consumes: `ROUTES` từ `src/routes/paths.ts`.
- Produces: `ROUTES.STUDENT_EXAM_RESULT = '/student/exam/result'`.

- [ ] **Step 1: Thêm hằng vào `paths.ts`**

Cạnh `STUDENT_EXAM_TAKE` đã có:

```ts
    STUDENT_EXAM_RESULT: '/student/exam/result',
```

- [ ] **Step 2: Dùng trong `routes/index.tsx`**

```tsx
    { path: ROUTES.STUDENT_EXAM_RESULT, element: <ExamResultPage/> },
```

- [ ] **Step 3: Tìm hết chỗ hardcode còn lại**

Run: `cd exam_hub_web && grep -rn "'/student/exam/result" src`
Expected sau khi sửa: chỉ còn `src/routes/paths.ts`.

Chỗ đã biết: `ExamTakingPage.tsx:145`
`navigate(`/student/exam/result?submissionId=${res.data.id}`)` →
`navigate(`${ROUTES.STUDENT_EXAM_RESULT}?submissionId=${res.data.id}`)`, thêm import `ROUTES`.
Kiểm cả `ExamResultPage`, `StudentSessionListPage`, `SessionResultsModal` — grep ở trên sẽ chỉ ra.

- [ ] **Step 4: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công**

Làm và nộp một bài → chuyển đúng sang trang kết quả với `submissionId` trên URL, điểm hiện đúng.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/routes exam_hub_web/src/pages/student
git commit -m "refactor(web): move exam result path into ROUTES"
```

---

### Task 19: Gỡ 5 dependency không dùng

**Files:**
- Modify: `exam_hub_web/package.json`
- Modify: `exam_hub_web/package-lock.json` (npm tự sinh)

**Interfaces:**
- Consumes: không. Produces: không.

- [ ] **Step 1: Xác nhận lần cuối là không có chỗ dùng**

Run:
```bash
cd exam_hub_web && grep -rn "i18next\|react-i18next\|react-hook-form\|@hookform\|from 'zod'\|from \"zod\"" src
```
Expected: không có kết quả. **Nếu có bất kỳ kết quả nào, dừng task và báo lại** — package đó đang
được dùng.

- [ ] **Step 2: Gỡ**

```bash
cd exam_hub_web && npm uninstall i18next react-i18next react-hook-form @hookform/resolvers zod
```

- [ ] **Step 3: Build**

Run: `cd exam_hub_web && npm run build`
Expected: xanh. Build đổ vỡ ở đây nghĩa là còn import sót — khôi phục package đó và báo lại.

- [ ] **Step 4: Test**

Run: `cd exam_hub_web && npm test`
Expected: xanh.

- [ ] **Step 5: Kiểm thủ công**

Chạy `npm run dev`, mở `/login`, `/app/dashboard`, `/app/questions/add`, `/student/exams`.
Không có lỗi module trong console.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/package.json exam_hub_web/package-lock.json
git commit -m "chore(web): drop unused i18n, form and schema deps"
```

---

## Kiểm cuối sau toàn bộ plan

- [ ] `cd exam_hub_web && npm run build` — xanh
- [ ] `cd exam_hub_web && npm test` — xanh
- [ ] `cd exam_hub_web && npx eslint .` — không lỗi mới so với trước khi bắt đầu
- [ ] `ls -l exam_hub_web/dist/assets/*.js` — so với số ghi ở Task 10 Step 1
- [ ] `git status` — `exam_hub_api/database_schema.sql` vẫn ở trạng thái sửa nhưng chưa commit,
      đúng như lúc bắt đầu
- [ ] Đi hết một vòng nghiệp vụ: đăng nhập → tạo câu hỏi → duyệt → tạo mẫu đề → sinh đề → tạo kỳ
      thi → giao lớp → xuất bản → đăng nhập bằng tài khoản học sinh → làm bài → nộp → xem kết quả
      → đăng nhập lại bằng giáo viên → chấm bài
