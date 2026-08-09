# Đồng bộ UI ExamHub theo Figma — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cập nhật giao diện `exam_hub_web` khớp thiết kế Figma: token màu/font thống nhất, shell admin chuẩn, trang học sinh chuyển sang style xanh + Inter.

**Architecture:** Tokens-first — thêm AntD `ConfigProvider` + Tailwind `@theme` + font Inter (lan toả toàn app), rồi chuẩn hoá CSS shell admin, rồi component dùng chung, cuối cùng chuyển trang học sinh. Chỉ đụng lớp trình bày (CSS + class + ConfigProvider), không đổi logic.

**Tech Stack:** React 19, AntD 6.3, Tailwind v4 (CSS `@theme`), Vite, TypeScript.

## Global Constraints

- Không đổi logic nghiệp vụ, API, route, cấu trúc dữ liệu — chỉ presentation.
- Palette (verbatim): primary `#3a74f5`, primary-soft `#e9ecfe`, success `#1ea375`/soft `#dff5ed`, danger `#e74242`/soft `#fee5e5`, warning `#d98a00`/soft `#fff4e5`, ink `#191d27`, text `#1d2129`, muted `#6f7788`, faint `#9aa2b1`, border `#eceef2`, surface `#f5f5f6`, line `#f0f1f4`, sidebar-bg `#191d27`, sidebar-line `#2a3040`, nav-idle `#acb4c0`, student-bg `#f5f4f1`.
- Font: **Inter** 400/500/600/700.
- "Build sạch" = `cd exam_hub_web && npx tsc --noEmit` không lỗi.
- Verify trực quan: `npm run dev` rồi đối chiếu frame Figma (file `Rz9AFnw0McsXm6HFIspSyG`).
- Commit thường xuyên trên nhánh `feat/ui-figma-sync`; message kết `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`.

---

## File Structure

- `src/App.tsx` — bọc `ConfigProvider` (theme token).
- `src/index.css` — `@import` Inter, `@theme` biến màu, `body` font, `.brand-panel`, `.sidebar*`, `.top-bar*`, khối student (`exam-*`, `sheet-*`, `result-*`).
- `src/layouts/AppLayout.tsx` — chỉ đổi class width sidebar nếu cần (logic giữ nguyên).
- `src/layouts/StudentLayout.tsx` — top-bar xanh.
- `src/components/StatusTag.tsx` — mới, pill trạng thái.
- `src/pages/student/*` — SessionList, Pool, Profile, ExamCover, ExamTaking, ExamResult.

---

## Task 1: Phase 1 — Design tokens (font Inter + ConfigProvider + Tailwind @theme)

**Files:**
- Modify: `src/App.tsx`
- Modify: `src/index.css` (dòng 1 `@import`; dòng ~10 `body`; dòng ~25 `.brand-panel`)

**Interfaces:**
- Produces: biến CSS `--color-primary` … dùng ở các phase sau; AntD theme token toàn cục.

- [ ] **Step 1: Thêm font Inter + biến @theme trong `index.css`**

Đầu file, cạnh `@import Lora`, thêm Inter và khối `@theme` (Tailwind v4):

```css
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');
@import url('https://fonts.googleapis.com/css2?family=Lora:ital,wght@0,400;0,500;0,600;0,700;1,400;1,500&display=swap');
@import "tailwindcss";

@theme {
  --color-primary: #3a74f5;
  --color-primary-soft: #e9ecfe;
  --color-success: #1ea375;
  --color-success-soft: #dff5ed;
  --color-danger: #e74242;
  --color-danger-soft: #fee5e5;
  --color-warning: #d98a00;
  --color-warning-soft: #fff4e5;
  --color-ink: #191d27;
  --color-muted: #6f7788;
  --color-border: #eceef2;
  --color-surface: #f5f5f6;
  --color-sidebar: #191d27;
}
```

Đổi `body { font-family }` sang Inter:

```css
body {
  margin: 0; padding: 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
```

Đổi `.brand-panel` màu nền:

```css
.brand-panel { @apply w-[44%] flex flex-col justify-center px-14 py-16 text-white bg-[#3a74f5]; }
```

- [ ] **Step 2: Thêm `ConfigProvider` vào `App.tsx`**

```tsx
import { ConfigProvider } from 'antd'
import { RouterProvider } from 'react-router-dom'
import { router } from './routes'
import { AuthProvider } from './AuthProvider'

const theme = {
  token: {
    colorPrimary: '#3a74f5',
    colorSuccess: '#1ea375',
    colorError: '#e74242',
    colorWarning: '#d98a00',
    colorLink: '#3a74f5',
    colorTextHeading: '#191d27',
    fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    borderRadius: 8,
  },
  components: {
    Table: { headerBg: '#f5f5f6', headerColor: '#6f7788', borderColor: '#f0f1f4' },
    Button: { controlHeight: 38 },
  },
}

export default function App() {
  return (
    <ConfigProvider theme={theme}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </ConfigProvider>
  )
}
```

- [ ] **Step 3: Build sạch**

Run: `cd exam_hub_web && npx tsc --noEmit`
Expected: không lỗi. Nếu AntD v6 báo token/prop lạ (vd `headerBg`), tra Context7 `antd` v6 `ConfigProvider theme` và sửa tên token cho khớp.

- [ ] **Step 4: Verify trực quan**

`npm run dev` → mở Login + 1 trang bảng (Người dùng). Kỳ vọng: nút/link primary màu `#3a74f5`, chữ font Inter, brand-panel login xanh mới. Chụp so với frame `01 Đăng nhập` + `08 Người dùng`.

- [ ] **Step 5: Commit**

```bash
git add src/App.tsx src/index.css
git commit -m "feat(ui): phase 1 — design tokens (Inter, ConfigProvider #3a74f5)"
```

---

## Task 2: Phase 2 — Shell admin (sidebar + top-bar)

**Files:**
- Modify: `src/index.css` (`.sidebar` ~104, `.sidebar-logo-icon` ~112, `.sidebar-nav-item` ~125, `.sidebar-nav-item--active` ~131, `.sidebar-footer` ~135, `.top-bar*` ~145)
- Modify (nếu cần): `src/layouts/AppLayout.tsx` (class `w-44`→ tương ứng 240px)

**Interfaces:**
- Consumes: biến màu từ Task 1.

- [ ] **Step 1: Cập nhật CSS sidebar theo Figma**

Trong `index.css`:

```css
.sidebar { @apply w-60 flex flex-col shrink-0; background: #191d27; }
.sidebar-logo-icon { @apply w-[34px] h-[34px] rounded-lg flex items-center justify-center text-[13px] font-bold text-white; background: #3a74f5; }
.sidebar-logo-name { @apply text-white font-semibold; }
.sidebar-nav-item { @apply flex items-center gap-3 w-full px-4 py-2.5 rounded-lg text-[14px] font-medium; color: #acb4c0; }
.sidebar-nav-item:hover { background: rgba(255,255,255,0.06); color: #e6e9ef; }
.sidebar-nav-item--active { background: #e9ecfe !important; color: #3a74f5 !important; }
.sidebar-footer { border-top: 1px solid #2a3040; }
```

(Giữ nguyên các thuộc tính layout khác nếu đã có; chỉ đổi width/màu/active.)

- [ ] **Step 2: Cập nhật `.top-bar*`**

```css
.top-bar { @apply h-[60px] bg-white border-b flex items-center justify-between px-7; border-color: #eceef2; }
.top-bar-title { @apply text-[18px] font-semibold; color: #191d27; }
.top-bar-subtitle { @apply text-[13px]; color: #6f7788; }
.top-bar-avatar { @apply w-9 h-9 rounded-full flex items-center justify-center text-[12px] font-semibold; background: #e9ecfe; color: #3a74f5; }
```

- [ ] **Step 3: Đồng bộ width trong `AppLayout.tsx` nếu class cứng**

Nếu JSX không hardcode width (dùng `.sidebar`), bỏ qua. Nếu có, đảm bảo khớp `.sidebar w-60`.

- [ ] **Step 4: Build sạch** — `npx tsc --noEmit`.

- [ ] **Step 5: Verify** — `npm run dev` → Dashboard + Người dùng: sidebar tối 240px, item active nền xanh nhạt/chữ xanh, header khớp frame `03`/`08`.

- [ ] **Step 6: Commit**

```bash
git add src/index.css src/layouts/AppLayout.tsx
git commit -m "feat(ui): phase 2 — admin shell sidebar + top-bar theo Figma"
```

---

## Task 3: Phase 3 — StatusTag + chuẩn hoá badge/tag bảng

**Files:**
- Create: `src/components/StatusTag.tsx`
- Modify: các trang có tag trạng thái (Users, ExamList, ExamSessionList, School*) — thay `<Tag color=...>` bằng `<StatusTag>`.

**Interfaces:**
- Produces: `StatusTag({ status, label }: { status: 'success'|'danger'|'warning'|'default'; label: string })`.

- [ ] **Step 1: Tạo component `StatusTag.tsx`**

```tsx
const MAP = {
  success: { bg: '#dff5ed', fg: '#1ea375' },
  danger:  { bg: '#fee5e5', fg: '#e74242' },
  warning: { bg: '#fff4e5', fg: '#d98a00' },
  default: { bg: '#eef0f3', fg: '#6f7788' },
} as const

export function StatusTag({status, label}: {status: keyof typeof MAP; label: string}) {
  const c = MAP[status]
  return (
    <span style={{background: c.bg, color: c.fg}}
          className="inline-flex items-center rounded-full px-2.5 py-0.5 text-[12px] font-medium">
      {label}
    </span>
  )
}
```

- [ ] **Step 2: Áp dụng vào bảng trạng thái**

Ví dụ ở `UserPage` cột Trạng thái: map `isActive ? <StatusTag status="success" label="Hoạt động"/> : <StatusTag status="default" label="Khoá"/>`. Làm tương tự ExamList (`published→success`, `draft→warning`, `archived→default`), ExamSessionList, SchoolList/Detail.

- [ ] **Step 3: Build sạch** — `npx tsc --noEmit`.

- [ ] **Step 4: Verify** — badge trên các bảng khớp màu Figma (frame 08/10/11/14/15).

- [ ] **Step 5: Commit**

```bash
git add src/components/StatusTag.tsx src/pages
git commit -m "feat(ui): phase 3 — StatusTag pill trạng thái đồng bộ Figma"
```

---

## Task 4: Phase 4a — StudentLayout top-bar xanh + nền student Inter

**Files:**
- Modify: `src/layouts/StudentLayout.tsx`
- Modify: `src/index.css` (khối `exam-*`, `sheet-*`, `result-*`: Lora→Inter, nền ấm→`#f5f4f1`/trắng)

- [ ] **Step 1: Top-bar xanh trong `StudentLayout.tsx`**

Thay `<header className="bg-white border-b ...">` bằng top-bar xanh:

```tsx
<header className="h-16 px-6 flex items-center justify-between" style={{background: '#3a74f5'}}>
  <div className="flex items-center gap-2.5">
    <div className="w-[30px] h-[30px] rounded-md bg-white flex items-center justify-center text-[12px] font-bold" style={{color: '#3a74f5'}}>EH</div>
    <span className="font-semibold text-white">ExamHub</span>
  </div>
  <div className="flex items-center gap-4 text-white">
    <button className="text-right leading-tight" onClick={() => navigate('/student/profile')}>
      <div className="text-[13px] font-medium">{user?.displayName ?? user?.userName}</div>
      <div className="text-[12px]" style={{color: '#cdd9fb'}}>Học sinh</div>
    </button>
    <div className="w-8 h-8 rounded-full flex items-center justify-center text-[13px] font-semibold" style={{background:'#eaf0ff', color:'#3a74f5'}}>
      {(user?.displayName ?? user?.userName ?? 'A').charAt(0).toUpperCase()}
    </div>
    <Button size="small" ghost icon={<LogoutOutlined/>} onClick={handleLogout}>Đăng xuất</Button>
  </div>
</header>
```

Đổi wrapper `main` nền: `<main className="flex-1 overflow-auto" style={{background:'#f5f4f1'}}>`.

- [ ] **Step 2: Chuyển khối student CSS sang Inter**

Trong `index.css`, các rule `font-family: 'Lora'...` thuộc khối student (`.exam-list-title`, `.exam-ticket-title`, `.sheet-*`, `.result-*`) → đổi `font-family: 'Inter', sans-serif;` hoặc bỏ (kế thừa body). Nền ấm (`#faf8f3`, `#f6f4ee`, gradient `#eae7df`) → `#f5f4f1`/`#ffffff`. Giữ layout, chỉ đổi font/màu nền + điểm nhấn `#3a74f5`/`#1ea375`.

- [ ] **Step 3: Build sạch** — `npx tsc --noEmit`.

- [ ] **Step 4: Verify** — mở 1 trang HS bất kỳ: top-bar xanh, font Inter, nền `#f5f4f1`.

- [ ] **Step 5: Commit**

```bash
git add src/layouts/StudentLayout.tsx src/index.css
git commit -m "feat(ui): phase 4a — student layout top-bar xanh + Inter"
```

---

## Task 5: Phase 4b — StudentSessionList (lưới vé) + Pool

**Files:**
- Modify: `src/pages/student/StudentSessionListPage.tsx` (đối chiếu frame 17)
- Modify: `src/pages/student/StudentSessionPoolPage.tsx` (frame 18)

- [ ] **Step 1: SessionList — lưới vé 2 cột**

Eyebrow "PHÒNG THI" (`#c98a2b`), title "Kỳ thi của tôi" (Inter bold 30, `#2a2520`/`#191d27`), subtitle muted. Mỗi vé: card trắng bo 16, viền `#eae7e0`; title semibold; meta `📖 Môn · Lớp`, `📅 khung giờ`; badge trạng thái (StatusTag); nút hành động primary/disabled theo `availability` (Vào thi / Chọn đề / Chưa mở / Hết lượt). Giữ nguyên logic `startAndGo`, navigate pool.

- [ ] **Step 2: Pool — list chọn đề**

Nút "‹ Quay lại", title "Chọn đề để làm", list card mỗi đề: icon 📄, `Đề số N (mã)`, badge (Chưa làm/Đã hoàn thành), nút "Bắt đầu →"/"Đã làm" (disabled nếu completed). Giữ logic `start`.

- [ ] **Step 3: Build sạch** — `npx tsc --noEmit`.

- [ ] **Step 4: Verify** — đối chiếu frame 17, 18.

- [ ] **Step 5: Commit**

```bash
git add src/pages/student/StudentSessionListPage.tsx src/pages/student/StudentSessionPoolPage.tsx
git commit -m "feat(ui): phase 4b — student session list + pool theo Figma"
```

---

## Task 6: Phase 4c — StudentProfile

**Files:**
- Modify: `src/pages/student/StudentProfilePage.tsx` (frame 20)

- [ ] **Step 1: Layout hồ sơ HS**

Title "Hồ sơ của tôi" + subtitle; 2 stat box (Số đề đã làm `#eef1ff`/`#3a74f5`, Điểm TB `#e7f7ef`/`#1ea375`); card hồ sơ: avatar tròn, tên semibold, badge "Học sinh · Lớp", nút "Sửa hồ sơ", các field (Tên hiển thị, Tên đăng nhập, Email, SĐT, Lớp). Giữ nguyên data hooks/StatBox logic, chỉ đổi trình bày.

- [ ] **Step 2: Build sạch** — `npx tsc --noEmit`.
- [ ] **Step 3: Verify** — frame 20.
- [ ] **Step 4: Commit**

```bash
git add src/pages/student/StudentProfilePage.tsx
git commit -m "feat(ui): phase 4c — student profile theo Figma"
```

---

## Task 7: Phase 4d — ExamCover / ExamTaking / ExamResult

**Files:**
- Modify: `src/pages/student/ExamCoverPage.tsx` (07A), `ExamTakingPage.tsx` (07B), `ExamResultPage.tsx` (07C)

- [ ] **Step 1: ExamCover (07A)**

Hero xanh `#3a74f5` phía trên: tiêu đề đề trắng, meta lớp/năm học; 4 stat card pastel (Thời gian/Số câu/Tổng điểm/Ngày thi); card trắng "Thông tin bài thi" (Mã đề, Môn, GV ra đề, Hình thức, Điểm đạt); alert cảnh báo đếm giờ; checkbox "Tôi đã đọc..."; nút "Bắt đầu làm bài →". Font Inter. Giữ logic start.

- [ ] **Step 2: ExamTaking (07B)** — nền `#f5f4f1`, header đề + đồng hồ đếm ngược; câu hỏi card trắng Inter; palette câu. Giữ logic timer/observer/submit; chỉ đổi font/màu.

- [ ] **Step 3: ExamResult (07C)** — hero xanh "Bạn đã ĐẠT/KHÔNG ĐẠT" + check; card trắng: vòng điểm (giữ `.result-score`, đổi Inter), 4 ô (đúng/sai/bỏ trống/thời gian) pastel, "Phân tích theo Bloom" các bar; nút "Xem đáp án / Về trang chủ / Thi lại". Giữ logic.

- [ ] **Step 4: Build sạch** — `npx tsc --noEmit`.
- [ ] **Step 5: Verify** — frame 07A/07B/07C.
- [ ] **Step 6: Commit**

```bash
git add src/pages/student/ExamCoverPage.tsx src/pages/student/ExamTakingPage.tsx src/pages/student/ExamResultPage.tsx
git commit -m "feat(ui): phase 4d — exam cover/taking/result theo Figma"
```

---

## Task 8: Phase 5 — Chuyển CSS → SCSS

**Bổ sung theo yêu cầu (2026-08-09):** sau khi xong Phase 4, chuyển `index.css` sang SCSS để tổ chức biến/nesting tốt hơn.

**Files:**
- Rename: `src/index.css` → `src/index.scss`
- Modify: `src/main.tsx` (đổi `import './index.css'` → `import './index.scss'`)
- Modify: `package.json` (devDependency `sass`)

**Lưu ý rủi ro (Tailwind v4):** file dùng `@import "tailwindcss"`, `@theme`, `@apply` — là directive của Tailwind, xử lý bởi plugin `@tailwindcss/vite`. Sass xử lý `@import`/`@use` khác. Cách an toàn:
- Cài `sass`; Vite tự nhận `.scss`. Thứ tự: Vite chạy Sass trước rồi Tailwind plugin.
- Giữ nguyên `@import "tailwindcss";` ở đầu (Tailwind plugin vẫn nhận). Nếu Sass báo lỗi `@import` không tìm thấy file, chuyển phần Tailwind sang dùng cú pháp tương thích hoặc tách biến custom sang `_tokens.scss` `@use`.
- `@apply`/`@theme` để nguyên (Tailwind xử lý). Sass không đụng chúng nếu không phải cú pháp Sass.

- [ ] **Step 1: Cài `sass`**

Run: `cd exam_hub_web && npm i -D sass`

- [ ] **Step 2: Đổi tên file + import**

`git mv src/index.css src/index.scss`; sửa `main.tsx` import `'./index.scss'`.

- [ ] **Step 3: (Tuỳ) refactor biến + nesting**

Chuyển palette sang biến SCSS `$primary: #3a74f5; …` cho các khối custom (`.sidebar*`, `.top-bar*`, student), nesting các selector con. KHÔNG đụng `@theme`/`@apply`/`@import "tailwindcss"`.

- [ ] **Step 4: Build sạch** — `npm run build`. Nếu Sass lỗi với directive Tailwind, xử lý theo mục "Lưu ý rủi ro" (tách `_tokens.scss` + `@use`, giữ Tailwind ở file vào chính).

- [ ] **Step 5: Verify** — mở app, giao diện không đổi so với trước (chỉ đổi cách tổ chức style).

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/index.scss exam_hub_web/src/main.tsx exam_hub_web/package.json exam_hub_web/package-lock.json
git commit -m "refactor(ui): phase 5 — chuyển index.css sang SCSS"
```

---

## Self-Review

- **Spec coverage:** §1→Task1, §2→Task2, §3→Task3, §4→Task4-7, SCSS(bổ sung)→Task8. Đủ.
- **Placeholder:** không có TBD; code Phase 1 đầy đủ. Phase 4b–d mô tả theo frame + giữ logic (chi tiết JSX sẽ bám screenshot khi làm — chấp nhận vì là visual, verify bằng đối chiếu).
- **Type consistency:** `StatusTag` signature nhất quán giữa Task 3 định nghĩa và nơi dùng.
- **Rủi ro AntD v6:** Step 3 Task 1 có nhánh xử lý nếu token đổi tên.
