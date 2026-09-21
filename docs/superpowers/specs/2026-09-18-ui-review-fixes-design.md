# Design — Khắc phục phát hiện review UI (exam_hub_web)

**Ngày:** 2026-09-18
**Phạm vi:** `exam_hub_web/` (React 19 + Vite 7 + antd 6 + Tailwind 4 + TanStack Query 5 + Zustand 5)
**Nguồn:** review UI ngày 2026-09-18, đối chiếu bộ quy tắc `vercel-react-best-practices`

---

## 1. Bối cảnh

Review toàn bộ lớp UI phát hiện 22 vấn đề thuộc 6 nhóm. Ba nhóm gốc rễ:

- **Không có code-splitting** — `routes/index.tsx` import tĩnh cả 30 page, kéo `recharts`,
  `@tiptap/*`, `katex` vào bundle đầu tiên cho mọi người dùng, kể cả học sinh chỉ vào trang làm bài.
- **Trùng lặp cắt ngang** — khối `.top-bar` copy-paste ở 16 page, `stripHtml` định nghĩa 3 lần,
  logic cascade Lớp→Môn→Chủ đề viết lại ở 2 page. Mọi thay đổi diện rộng vì thế đều nhân 16.
- **State trùng nguồn** — `ExamTakingPage` giữ song song `values` (useState) và antd Form;
  `useAuthStore()` gọi không selector nên subscribe cả store.

Kèm theo là các vấn đề a11y (không có `:focus-visible`, sidebar dùng `<button>` thay `<a>`,
tương phản dưới AA) và design token khai báo rồi không dùng.

## 2. Mục tiêu

1. Giảm bundle tải lần đầu bằng lazy-load các route nặng.
2. Loại các nguồn re-render thừa, ưu tiên trang làm bài (chạy liên tục 45–90 phút).
3. Xoá trùng lặp cắt ngang trước khi làm các thay đổi diện rộng khác.
4. Đạt mức a11y cơ bản: điều hướng bàn phím, tương phản AA, tôn trọng `prefers-reduced-motion`.
5. Đưa màu về một nguồn khai báo duy nhất.

## 3. Ngoài phạm vi

- Không đổi API, không đụng `exam_hub_api/`.
- Không đổi bố cục/thị giác của bất kỳ màn hình nào (trừ avatar top-bar hiển thị đúng người dùng
  và các chỉnh tương phản màu chữ phụ).
- Không thêm thư viện UI, không thêm framework test render.
- Không refactor ngoài danh sách phát hiện.

## 4. Quyết định đã chốt

| Quyết định | Chọn | Lý do |
|---|---|---|
| Cách tổ chức | Theo tầng, gom trùng lặp trước | Làm `PageHeader` trước kéo hex + markup từ 16 file về 1, nên phase a11y và phase token sau đó chỉ sửa một chỗ |
| Dead deps | Gỡ hết 5 package | `grep` toàn `src/` không có import nào |
| Verify | `npm run build` + đo bundle + chạy app kiểm thủ công | Repo không có test render; thêm hạ tầng test là scope riêng |
| Ngoại lệ verify | `ExamTakingPage` có thêm test thuần | Sai ở đây = học sinh mất bài; repo đã có tiền lệ `examTimer.test.ts` |

## 5. Bảng phát hiện → phase

| # | Phát hiện | Vị trí | Phase |
|---|---|---|---|
| 1 | Không code-splitting | `routes/index.tsx:52-82` | 3 |
| 2 | Font `@import` chặn render | `index.css:1-2` | 3 |
| 3 | Không có `errorElement` | `routes/index.tsx` | 3 |
| 4 | State `values` trùng Form | `ExamTakingPage.tsx:204` | 2 |
| 5 | Timer re-render cả cây mỗi giây | `ExamTakingPage.tsx:125-130` | 2 |
| 6 | `useAuthStore()` không selector | `AuthProvider.tsx:13`, `ProtectedRoute.tsx:10` | 2 |
| 7 | Search không debounce (4 chỗ) | `QuestionBankPage.tsx:285` + 3 | 2 |
| 8 | `useRef` khởi tạo không lười | `ExamTakingPage.tsx:54` | 2 |
| 9 | `[...].sort()` trong render | `QuestionBankPage.tsx:300` | 2 |
| 10 | `.top-bar` lặp 16 lần, avatar hardcode `"TT"` | 16 page | 1 |
| 11 | `stripHtml` 3 bản | `snapshot.ts:11`, `QuestionBankPage.tsx:429`, `AddQuestionPage.tsx:411` | 1 |
| 12 | Cascade Lớp→Môn→Chủ đề lặp | `QuestionBankPage.tsx:118-133`, `ExamSessionEditPage.tsx:72-77` | 1 |
| 13 | Sidebar `<button>` thay `<a>` | `AppLayout.tsx:245-291` | 4 |
| 14 | Không có `:focus-visible` | `index.css` | 4 |
| 15 | Tương phản dưới AA | `index.css:591`, `:564` | 4 |
| 16 | Animation không tôn trọng reduced-motion | `index.css:827`, `:1040`, `:841` | 4 |
| 17 | Lưới câu hỏi thiếu nhãn | `ExamTakingPage.tsx:299` | 4 |
| 18 | Hex hardcode, token không dùng | `StatusTag.tsx`, `Chip`, `StatCard`, … | 5 |
| 19 | Hai bảng màu song song | `App.tsx:24-39` + `index.css:376-389` | 5 |
| 20 | `bulkVerify` dùng `Promise.all` | `QuestionBankPage.tsx:163-167` | 6 |
| 21 | Route hardcode | `routes/index.tsx:105` | 6 |
| 22 | 5 dead deps | `package.json` | 6 |

## 6. Thiết kế theo phase

### Phase 1 — Gom về một chỗ

Không thay đổi hành vi, trừ avatar (hiện đúng người dùng thay vì `"TT"`).

**`src/components/PageHeader.tsx`** (mới)

```tsx
type Props = {
    title: string
    subtitle?: ReactNode   // ReactNode để bọc breadcrumb có link
    backTo?: string        // có → hiện nút ←
    right?: ReactNode      // mặc định: avatar chữ cái đầu từ authStore
}
```

API này phủ đủ 16 biến thể hiện có:

| Biến thể | Page |
|---|---|
| title + subtitle + avatar | 11 page (dashboard, exam-list, …) |
| title + avatar, không subtitle | `CategoryPage` |
| subtitle là breadcrumb có link | `AddQuestionPage`, `CreateExamTemplatePage` |
| back + title + subtitle + `<Tag>` thay avatar | `ExamSessionEditPage` |

Avatar đọc `useAuthStore(s => s.user)` — selector ngay từ đầu, không chờ phase 2.
Class CSS `.top-bar*` giữ nguyên, chỉ markup chuyển vào component.

**Xoá trùng:** bỏ `stripHtml` cục bộ ở `QuestionBankPage.tsx:429` và `AddQuestionPage.tsx:411`,
import từ `utils/snapshot.ts`. Bản trong `snapshot.ts` nhận `html?: string` (rộng hơn), kiểm mọi
call site vẫn hợp kiểu.

**Hook mới (tạo ở phase này, gắn vào ở phase 2):**

- `src/hooks/useDebounced.ts` — `useDebounced<T>(value: T, delay = 300): T`
- `src/hooks/useCascadeOptions.ts` — nhận `gradeLevelId`/`subjectId`, trả `subjectOptions` +
  `topicOptions`; gói `useSubjectsQuery` + `useTopicsQuery` và phần lọc đang lặp ở 2 page.

### Phase 2 — Re-render

**2.1 Selector cho authStore.** `AuthProvider.tsx:13` và `ProtectedRoute.tsx:10` đang destructure
toàn store nên `isRefreshing` bật/tắt mỗi lần refresh token là mọi consumer re-render.
Đổi sang selector từng field.

**2.2 Debounce search.** Gắn `useDebounced` vào `QuestionBankPage.tsx:285`,
`ExamListPage.tsx:123`, `ExamSessionListPage.tsx:109`, `StudentExamListPage.tsx:131`.
Ô input vẫn cập nhật tức thì; chỉ giá trị đưa vào query key bị trễ 300 ms.

**2.3 `ExamTakingPage`** — chia 3 commit tách biệt để revert được từng phần:

1. **Tách timer.** `<ExamTimer deadline onExpire>` tự giữ state đếm ngược. Hiện
   `setTimeLeft` mỗi giây ở `ExamRunner` làm re-render tờ đề + sidebar + lưới 40 nút.
   Auto-nộp chuyển thành callback `onExpire` (giữ nguyên cờ `autoSubmitted` chống gọi hai lần).
2. **Bỏ state `values`.** Form đã là nguồn duy nhất; `values` chỉ phục vụ đếm câu đã trả lời và
   tô màu ô trong lưới. Thay bằng `Set<string>` id đã trả lời, chỉ `setState` khi trạng thái
   có/không-có-đáp-án của một câu thực sự đổi — gõ ký tự thứ hai trở đi trong ô tự luận không
   còn re-render.
   Logic đặt trong `src/pages/student/answerState.ts` (hàm thuần), kèm
   `answerState.test.ts` (~15 dòng, vitest, cùng kiểu `examTimer.test.ts` đã có).
   Ca kiểm: chuỗi rỗng và chuỗi toàn khoảng trắng = chưa trả lời; chọn rồi bỏ chọn; câu tự luận
   và câu trắc nghiệm.
3. **`fallbackDeadline`** `useRef(Date.now() + …)` → `useState(() => …)`.

**2.4** `QuestionBankPage.tsx:300` `[...arr].sort()` → `toSorted()` trong `useMemo`.

### Phase 3 — Bundle

**Lazy route:** `AddQuestionPage`, `DashboardPage`, `GeneratePage`, `CreateExamTemplatePage` bọc
`React.lazy`; `AnalyticsDrawer` lazy ngay tại `ExamSessionEditPage`. Một `<Suspense>` với fallback
`<Spin/>` bọc `<Outlet/>` trong `AppLayout` là đủ cho cả nhóm.
Mục tiêu: `recharts`, `@tiptap/*`, `katex` rời khỏi chunk khởi động.

**`errorElement`** ở route gốc: component hiện thông báo + nút tải lại, thay cho màn trắng.

**Font:** bỏ 2 dòng `@import` ở `index.css:1-2`; thêm vào `index.html`:
`<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>` + `<link rel="stylesheet" …>`.
Hết chuỗi tải nối tiếp sau khi parse CSS.

### Phase 4 — A11y

- `AppLayout.tsx:245-291`: `<button onClick={navigate}>` → `<NavLink>`, giữ nguyên class,
  dùng hàm `className` của NavLink cho trạng thái active thay `location.pathname.startsWith`.
  Thêm `aria-current="page"`; nút mở/đóng nhóm thêm `aria-expanded`.
  Mở tab mới / middle-click hoạt động trở lại.
- `index.css`: thêm một khối `:focus-visible` dùng chung (outline theo `--color-primary`),
  và `@media (prefers-reduced-motion: reduce)` tắt `animate-pulse` ở `.exam-timer--danger`,
  `.take-timer--danger` và animation `paper-rise`.
- Tương phản: `.table-th` `text-gray-400` → `text-gray-600`; `.stat-card-label` `text-gray-500` →
  `text-gray-600`. (gray-400 trên nền trắng ≈ 3:1, dưới ngưỡng AA 4.5:1 cho chữ thường.)
- `ExamTakingPage.tsx:299`: thêm `aria-label` dạng `"Câu 3, đã trả lời"` và `aria-pressed` cho ô
  đang mở.

### Phase 5 — Token màu

`@theme` trong `index.css:376-389` là nguồn khai báo duy nhất. `App.tsx:24-39` (antd
`ConfigProvider`) hiện khai báo lại cùng bảng màu → tách hằng dùng chung ở
`src/constants/theme.ts`, cả hai cùng đọc.

Thay hex inline còn lại bằng token: `StatusTag.tsx:1-6`, `Chip`/`StatCard`
(`QuestionBankPage.tsx:54-80`, `ExamTemplatePage`), panel `ExamSessionEditPage:155,218`.

Đây là phase dễ lệch thị giác nhất → commit riêng, mỗi nhóm component một commit.

### Phase 6 — Dọn

- `QuestionBankPage.tsx:163-167` `bulkVerify`: `Promise.all` → `allSettled` + đếm thành công/thất
  bại, theo đúng cách `bulkDelete` ngay bên dưới đã làm. Hiện một câu lỗi là reject cả chùm:
  không có message, không clear selection, không invalidate.
- `routes/index.tsx:105` `'/student/exam/result'` → hằng trong `ROUTES`.
- Gỡ `i18next`, `react-i18next`, `react-hook-form`, `@hookform/resolvers`, `zod`.

## 7. Ràng buộc thứ tự

```
Phase 1 ──┬── Phase 4
          └── Phase 5
Phase 2, 3, 6: độc lập
```

Chỉ phase 1 là điều kiện của phase 4 và 5 (gom hex + markup trước, để hai phase đó sửa một chỗ
thay vì 16). Còn lại chạy theo thứ tự nào cũng được; dừng ở ranh giới phase bất kỳ đều để lại
cây code chạy được.

## 8. Verify

Sau **mỗi** phase:

1. `npm run build` (`tsc -b && vite build`) — xanh, không warning mới.
2. Chạy `npm run dev`, kiểm thủ công các luồng bị đụng của phase đó.
3. Commit.

Thêm theo phase:

| Phase | Kiểm thêm |
|---|---|
| 1 | Mở đủ 16 page, đối chiếu top-bar với ảnh trước khi sửa; avatar hiện đúng chữ cái đầu |
| 2 | `npm test` xanh; làm thử một bài thi: đếm câu, đánh dấu, hết giờ auto-nộp, F5 khôi phục đáp án |
| 3 | So `dist/assets/*.js` trước/sau, ghi số vào commit message; kiểm mọi route lazy mở được |
| 4 | Điều hướng sidebar chỉ bằng Tab/Enter; Ctrl+click mở tab mới; bật reduced-motion của OS |
| 5 | Đối chiếu ảnh chụp trước/sau từng nhóm component |
| 6 | `npm run build` sau khi gỡ deps (bắt import sót) |

## 9. Rủi ro

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Bỏ state `values` làm hỏng lưu/nộp bài | Cao | Tách 3 commit; logic ra hàm thuần + test; kiểm thủ công cả luồng làm bài |
| Phase 5 làm lệch màu | Trung bình | Commit nhỏ theo nhóm component; đối chiếu ảnh |
| `PageHeader` bỏ sót biến thể | Trung bình | Bảng 4 biến thể ở mục 6; mở đủ 16 page sau khi đổi |
| Suspense fallback nhấp nháy khi chuyển route | Thấp | Chỉ lazy 4 route nặng, không lazy toàn bộ |
| Gỡ deps trúng import động | Thấp | `grep` trước, build sau |

## 10. Không làm (YAGNI)

- Không thêm `manualChunks` vào `vite.config.ts` — đo sau phase 3 rồi tính; lazy route có thể đã đủ.
- Không thêm `@testing-library/react`.
- Không tách `index.css` (1194 dòng) thành nhiều file — chưa gây vấn đề nào trong review.
- Không memo hoá `columns` của antd Table — lợi ích không đo được.
- Không đụng `exam_hub_api/`.
