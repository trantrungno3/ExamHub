# Đồng bộ 6 màn học sinh theo Figma — Implementation Plan

> **For agentic workers:** Steps use checkbox (`- [ ]`) syntax. Sửa **code theo Figma**. Mọi phần **thiếu dữ liệu backend thì ẩn** (không bịa số).

**Goal:** Chỉnh 6 màn học sinh cho khớp mockup Figma (file `Rz9AFnw0McsXm6HFIspSyG`).

**Tech Stack:** React 19, AntD 6.3, Tailwind v4 (utility + `@apply` trong `index.css`), react-router.

## Global Constraints
- Sửa code theo Figma; **ẩn** phần không có data backend (Bloom, xếp hạng, SBD, thời gian làm, giáo viên ra đề, thumbnail…).
- Merge local; **không push origin** nếu chưa xác nhận.
- Ưu tiên AntD `<Form>`/control của AntD thay vì div thủ công (khi hợp lý).
- Verify mỗi task: `npx tsc --noEmit` + `npx vite build` trong `exam_hub_web`.
- Không đụng file `category/*/index.tsx` (đang lỗi sẵn, ngoài phạm vi).
- `ProfileCard` dùng chung 3 màn (Admin/GV/HS) → **không đập cấu trúc**, chỉ chỉnh nhẹ nếu không phá 2 màn kia.

## Figma ↔ Code map
| Frame | File |
|---|---|
| 17 — HS: Kỳ thi của tôi (74:2) | `pages/student/StudentSessionListPage.tsx` |
| 18 — HS: Chọn đề (74:50) | `pages/student/StudentSessionPoolPage.tsx` |
| 20 — HS: Hồ sơ (74:147) | `pages/profile/StudentProfilePage.tsx` |
| 07A — Trang bìa (6:2) | `pages/student/ExamCoverPage.tsx` |
| 07C — Kết quả (6:239) | `pages/student/ExamResultPage.tsx` |
| 07B — Làm bài (6:70) | `pages/student/ExamTakingPage.tsx` |

---

### Task 1: Màn 17 — Kỳ thi của tôi (nhẹ)

**Files:** Modify `pages/student/StudentSessionListPage.tsx`

- [ ] Nút hành động chuyển thành **full-width** (`block`) đặt dưới cùng card; thêm mũi tên `→` (`ArrowRightOutlined`) cho nút active ("Vào thi", "Chọn đề", "Tiếp tục").
- [ ] Bỏ dòng "Lượt còn lại: x/y" và nút "Xem kết quả" khỏi footer card (giữ `SessionResultsModal` mount nhưng không cần trigger — hoặc bỏ luôn nếu không còn nơi mở). Giữ logic remaining để quyết định disable ("Hết lượt").
- [ ] Ngày hiển thị: nếu `openAt` và `closeAt` cùng ngày → `dd/MM/yyyy · HH:mm–HH:mm`; khác ngày → giữ `dd/MM HH:mm → dd/MM HH:mm`. Viết helper `fmtRange(openAt, closeAt)`.
- [ ] Giữ eyebrow "Phòng thi" (amber) + title + sub như cũ (đã khớp).
- [ ] Verify: `npx tsc --noEmit && npx vite build`. Commit.

### Task 2: Màn 18 — Chọn đề (nhẹ)

**Files:** Modify `pages/student/StudentSessionPoolPage.tsx`, `pages/student/StudentSessionListPage.tsx` (truyền state)

- [ ] Layout đổi **1 cột full-width** (bỏ `md:grid-cols-2`, dùng `flex flex-col gap-3`).
- [ ] Header: thay icon back bằng **link chữ** "‹ Quay lại kỳ thi của tôi" (màu `#3a74f5`), tiêu đề "**Chọn đề để làm**", subtitle mô tả.
- [ ] Subtitle lấy tên kỳ thi từ `location.state` (truyền `{title, subjectName, gradeLevelName}` khi bấm "Chọn đề" ở màn 17). Nếu không có state → subtitle generic "Chọn một đề bên dưới để bắt đầu".
- [ ] Mỗi row: thumbnail placeholder (ô vuông bo góc `#eef1ff` + icon `FileTextOutlined`, KHÔNG dùng ảnh vì không có data) bên trái + title `Đề số n (examCode)` + tag trạng thái dưới title; bên phải nút "**Bắt đầu →**" (hoặc "Tiếp tục" nếu inProgress), completed → nút disabled "Đã làm".
- [ ] Tag completed chỉ hiển thị "Đã hoàn thành" (KHÔNG kèm điểm — pool item không có điểm).
- [ ] Verify + commit.

### Task 3: Màn 20 — Hồ sơ (nhẹ)

**Files:** Modify `pages/profile/StudentProfilePage.tsx`

- [ ] Thêm subtitle "Thông tin cá nhân và kết quả học tập" dưới title "Hồ sơ của tôi".
- [ ] **Bỏ** khối bảng "Khoá học đang tham gia" (Figma không có). Bỏ import/logic `cohortMemberService`, `useQuery`, `StatusTag`, `Table` nếu không còn dùng.
- [ ] Giữ 2 StatBox (đã khớp) + `ProfileCard`.
- [ ] Verify + commit.

### Task 4: Màn 07A — Trang bìa (nhẹ)

**Files:** Modify `pages/student/ExamCoverPage.tsx`

- [ ] 4 stat card: thêm **icon phía trên** + **value** + **nhãn dưới** (căn giữa). Dùng data có thật: Thời gian (`ClockCircleOutlined`), Số câu hỏi (`QuestionCircleOutlined`), Tổng điểm (`BarChartOutlined`), Mã đề (`FileTextOutlined`). **Bỏ "Ngày thi"** (không có data).
- [ ] Dòng info dùng **nhãn pill xám bên trái** + value bên phải. Chỉ giữ field có data: Mã đề thi, Môn học, Thí sinh, Lớp, Năm học. **Bỏ** Giáo viên ra đề / Hình thức / Điểm đạt (không có data).
- [ ] Giữ warning box + checkbox + nút "Bắt đầu làm bài".
- [ ] Verify + commit.

### Task 5: Màn 07C — Kết quả (vừa, ẩn phần thiếu data)

**Files:** Modify `pages/student/ExamResultPage.tsx`

- [ ] Điểm dạng **vòng tròn** (tái dùng class `.result-score` đã có) — giữ.
- [ ] Ô thống kê: **bỏ "Thời gian làm"** (không data). Giữ 3 ô: Số câu đúng / Số câu sai / Bỏ trống (grid 3 cột — đã có).
- [ ] **Bỏ** khối "Phân tích Bloom's Taxonomy" (không data) — không thêm.
- [ ] **Bỏ** footer xếp hạng (không data).
- [ ] Nút: đổi thành **2 nút** có data thật — "Về trang chủ" (`/student/exams`) + "Thi lại" (điều hướng lại kỳ thi để làm lại nếu còn lượt; nếu không có sessionId thì chỉ 1 nút "Về danh sách kỳ thi"). "Xem đáp án chi tiết" = toggle hiện/ẩn khối "Chi tiết theo câu" hiện có.
- [ ] Verify + commit.

### Task 6: Màn 07B — Làm bài (NẶNG — làm full theo Figma)

**Files:** Modify `pages/student/ExamTakingPage.tsx`, `index.css` (thêm class theme tối)

**Mục tiêu UX theo Figma:**
- Theme **tối** cho khu đề (nền navy, chữ trắng), top bar sáng, timer đỏ góc phải.
- **1 câu/màn**: chỉ render câu `activeIdx`; nút "← Câu trước" / "Câu tiếp →" + nút **flag đánh dấu** (client-side state, không cần backend).
- Panel phải: thông tin HS (tên, lớp — **bỏ SBD** vì không data), 3 số liệu (Đã trả lời / Chưa trả lời / Đã đánh dấu), thanh **tiến độ %** (tính từ answeredCount), **lưới câu hỏi** nút số: xanh=đã trả lời, lam=đang xem, hổ phách=đã đánh dấu, xám=chưa; click để nhảy câu.
- Nút "Nộp bài thi" xanh lá dưới panel.

- [ ] Thay layout scroll-tất-cả bằng single-question: state `activeIdx`, `flagged: Set<string>`, render 1 `questions[activeIdx]`.
- [ ] Điều hướng: prev/next cập nhật `activeIdx` (clamp 0..len-1); nút flag toggle `flagged`.
- [ ] Panel phải: số liệu answered/unanswered/flagged, progress bar, grid nút số (màu theo trạng thái), click nút → set `activeIdx`.
- [ ] CSS: thêm nhóm class tối (vd `.take-dark`, `.take-option`, `.take-option--active`, `.take-grid-cell--answered/--current/--flagged`) trong `index.css`.
- [ ] Giữ nguyên logic submit/auto-submit/hết giờ hiện có; chỉ đổi trình bày & điều hướng.
- [ ] Bỏ tag độ khó "TB/Phần" (không data).
- [ ] Verify: `npx tsc --noEmit && npx vite build`; đối chiếu screenshot Figma. Commit.

---

## Self-review
- Mọi field/nút chỉ dùng data đã có trong `Exam`/`MySession`/`SessionPoolItem`/`ExamSubmission` (xem `types/*.d.ts`); phần thiếu → ẩn. ✔
- Không sửa `ProfileCard` phá Admin/GV. ✔
- Không đụng `category/*/index.tsx`. ✔
