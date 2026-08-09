# Đồng bộ UI ExamHub theo Figma mockup — Design Spec

**Ngày:** 2026-08-09
**Nhánh:** `feat/ui-figma-sync`
**Figma:** `Rz9AFnw0McsXm6HFIspSyG` — "ExamHub — Mockup UI" (16 frame gốc + 29 frame đã dựng)

## 1. Mục tiêu

Cập nhật giao diện app React (`exam_hub_web`) cho khớp thiết kế Figma: thống nhất design token (màu primary, font Inter), chuẩn hoá shell admin (sidebar + header), và chuyển trang học sinh từ phong cách "giấy thi" (Lora serif, nền ấm) sang phong cách Figma (top-bar xanh + Inter).

Ngoài phạm vi: thay đổi logic/nghiệp vụ, API, cấu trúc route, thêm tính năng mới. Đây thuần tuý là lớp trình bày (presentation).

## 2. Hiện trạng (gap)

| Khía cạnh | Hiện tại | Figma |
|---|---|---|
| Primary color | `#1677ff` (AntD default), login `#2563EB` | `#3a74f5` |
| Font | system stack + Lora serif (trang HS) | Inter toàn bộ |
| AntD theme | Chưa có `ConfigProvider` | Cần token hoá |
| Sidebar | `w-44` (176px), nền `#1c2131` | 240px, nền `#191d27`, active `#e9ecfe`/`#3a74f5` |
| Trang HS | `exam-desk` Lora, nền ấm | top-bar xanh + Inter, nền `#f5f4f1` |

**Stack:** AntD `^6.3.7`, React `19`, Tailwind v4 (CSS `@theme`, không có file config), Vite. Style tập trung ở `src/index.css` (718 dòng). `App.css` là template Vite mặc định (không dùng).

## 3. Bảng màu & token (nguồn chân lý)

```
primary   #3a74f5   primary-soft  #e9ecfe
success   #1ea375   success-soft  #dff5ed
danger    #e74242   danger-soft   #fee5e5
warning   #d98a00   warning-soft  #fff4e5
ink       #191d27   (tiêu đề / sidebar)
text      #1d2129   muted #6f7788   faint #9aa2b1
border    #eceef2   surface #f5f5f6   line #f0f1f4
sidebar-bg #191d27  sidebar-line #2a3040  nav-idle #acb4c0
student-bg #f5f4f1
```

Font: **Inter** 400/500/600/700.

## 4. Thiết kế theo phase

### Phase 1 — Design tokens (nền tảng)
- **Font Inter:** thêm `@import` Google Fonts Inter (giữ/loại Lora tuỳ còn dùng) trong `index.css`; đổi `body { font-family }` sang Inter.
- **Tailwind `@theme`:** khai báo biến màu (`--color-primary`, `--color-success`, …) để thay dần các `bg-[#...]` arbitrary.
- **AntD `ConfigProvider`** tại `src/App.tsx`, bọc quanh `RouterProvider`:
  - `token`: `colorPrimary #3a74f5`, `colorSuccess #1ea375`, `colorError #e74242`, `colorLink #3a74f5`, `colorTextHeading #191d27`, `fontFamily "Inter, ..."`, `borderRadius 8`.
  - `components.Table`: `headerBg #f5f5f6`, `headerColor #6f7788`, `borderColor #f0f1f4`.
  - `components.Button`: `controlHeight 38`.
- Đổi login `brand-panel` `#2563EB → #3a74f5` (`index.css`).
- **Verify:** login + 1 trang bảng đổi sang xanh #3a74f5, font Inter.

### Phase 2 — Shell admin (`AppLayout` + CSS)
- `src/index.css` `.sidebar*`:
  - `.sidebar` width `176→240px`, nền `#191d27`.
  - `.sidebar-logo-icon` nền `#3a74f5`, chữ trắng.
  - `.sidebar-nav-item` idle chữ `#acb4c0`; hover nền nhạt.
  - `.sidebar-nav-item--active` nền `#e9ecfe`, chữ `#3a74f5`, bo `8px`.
  - `.sidebar-footer` divider `#2a3040`.
  - `.top-bar` tiêu đề `#191d27` 18px/600, phụ đề `#6f7788` 13px; avatar tròn.
- Không đổi cấu trúc JSX `AppLayout.tsx` (chỉ class/CSS); giữ nguyên logic menu/refresh token.
- **Verify:** dashboard + users — sidebar 240px tối, active state xanh khớp frame Figma.

### Phase 3 — Thành phần dùng chung
- **`StatusTag`** (component nhỏ) render pill trạng thái: `active/published → success-soft`, `locked/closed → danger-soft`, `draft → warning-soft`, `default → gray`. Thay các `<Tag>` trạng thái rải rác ở bảng (Users, ExamList, Sessions, School…).
- Card bảng bo `10px`, viền `#eceef2`; nút "Sửa" link xanh, "Xóa" đỏ (đã theo token).
- Đa số tự động theo Phase 1; chỉ chỉnh những chỗ hardcode màu cũ.
- **Verify:** badge/tag trên các bảng khớp màu Figma.

### Phase 4 — Trang học sinh (Lora-giấy → Figma xanh/Inter)
- **`StudentLayout.tsx`:** header trắng → **top-bar xanh `#3a74f5`** full-width: logo trắng "EH ExamHub" trái; tên + lớp + avatar tròn phải. `main` nền `#f5f4f1`.
- **`index.css`:** thay khối `exam-desk`/`exam-list-*`/`exam-ticket-*`/`sheet-*`/`result-*` từ Lora serif → Inter; nền ấm → `#f5f4f1`/trắng; điểm nhấn xanh `#3a74f5`, xanh lá `#1ea375`.
- **Trang cụ thể (đối chiếu frame Figma):**
  - `StudentSessionListPage` → lưới "vé" 2 cột (frame 17): title, meta môn/khung giờ, badge trạng thái, nút hành động.
  - `StudentSessionPoolPage` → list chọn đề (frame 18).
  - `StudentProfilePage` → stat box (Số đề đã làm / Điểm TB) + card hồ sơ (frame 20).
  - `ExamCoverPage` (07A), `ExamTakingPage` (07B), `ExamResultPage` (07C): hero xanh, card trắng Inter; giữ vòng điểm ở Result.
- **Verify:** từng trang HS đối chiếu screenshot frame Figma tương ứng.

## 5. Files dự kiến thay đổi

- `src/App.tsx` — thêm `ConfigProvider`.
- `src/index.css` — font, `@theme`, `.sidebar*`, `.top-bar*`, login, khối student.
- `src/layouts/AppLayout.tsx` — (nếu cần) class sidebar width.
- `src/layouts/StudentLayout.tsx` — top-bar xanh.
- `src/pages/student/*` — SessionList, Pool, Profile, ExamCover, ExamTaking, ExamResult.
- `src/components/StatusTag.tsx` — mới.
- Rà `grep bg-\[#` để thay các màu hardcode lệch token.

## 6. Kiểm thử

Không có test UI tự động sẵn. Xác minh bằng:
1. `npm run build` (hoặc `tsc`) không lỗi type.
2. `npm run dev`, mở lần lượt: Login, Dashboard, Người dùng, Đề đã tạo, Kỳ thi, Trường & Lớp, và các trang HS (Kỳ thi của tôi, Pool, Cover, Taking, Result, Hồ sơ HS).
3. Đối chiếu trực quan với frame Figma tương ứng; chỉnh sai lệch spacing/màu.

## 7. Rủi ro

- AntD v6 tên token có thể khác v5 — kiểm chứng qua docs/Context7 khi cấu hình `ConfigProvider`.
- Chuyển trang HS đụng nhiều CSS `exam-*`/Lora — làm theo phase, verify từng trang, tránh vỡ layout.
- Tailwind v4 dùng `@theme` trong CSS (không phải `tailwind.config.js`) — khai báo biến đúng cú pháp v4.
