# Đồng bộ màn Mẫu đề thi (06A) + Tạo mẫu đề (06B) theo Figma — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Steps dùng checkbox `- [ ]`.

**Goal:** Cập nhật `ExamTemplatePage` (06A) + `CreateExamTemplatePage` (06B) khớp Figma: stat cards, cột Đảo/Chống trùng + chấm màu + StatusTag; form thêm Mô tả, header phần thi tối, ô % màu + chip tổng 100%, nút "Lưu & Sinh đề ngay".

**Architecture:** Chỉ 1 endpoint BE mới (stats); còn lại thuần FE (dữ liệu `shuffleQuestions/preventDuplicate/isActive/description/totalQuestions` đã có sẵn trong `ExamTemplate`).

**Tech Stack:** ASP.NET Core + EF, React 19, AntD 6.

**Base branch:** `feat/exam-template-figma` (off `main`).

## Global Constraints

- Palette: primary `#3a74f5`, success `#1ea375`, warning `#d98a00`, danger `#e74242`, purple `#8b5cf6`, muted `#6f7788`.
- Tái dùng `components/StatusTag.tsx` (đã có ở main).
- Verify: BE `dotnet build`; FE `npx tsc --noEmit`+`npm run build`; đối chiếu frame `Rz9AFnw0McsXm6HFIspSyG` node 4:391 (06A) + 4:622 (06B).
- Commit kết `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`.

## Ngoài phạm vi
- Bỏ qua sublabel schema trong mockup ("title VARCHAR"…).

---

## Task 1: BE — API thống kê mẫu đề thi

**Files:** `DataTransferObjects/**/ExamTemplateDto.cs` (thêm record); `IExamTemplateRepository`+impl; `IExamTemplateService`+impl; `ExamTemplateController.cs`.

**Interfaces:** `record ExamTemplateStatsResponse(int TotalTemplates, int ActiveTemplates, int TotalExamsGenerated, int AvgQuestions)`; `GET api/exam-templates/stats`.

- [ ] **Step 1: DTO** — thêm `ExamTemplateStatsResponse(int TotalTemplates, int ActiveTemplates, int TotalExamsGenerated, int AvgQuestions)`.
- [ ] **Step 2: Repo** — thêm `Task<ExamTemplateStatsResponse> GetStatsAsync(ct)` vào interface + impl (EF trên `Set` = ExamTemplates, `Db.Exams` cho đề sinh):
```csharp
public async Task<ExamTemplateStatsResponse> GetStatsAsync(CancellationToken ct = default)
{
    var total     = await Set.CountAsync(ct);
    var active    = await Set.CountAsync(x => x.IsActive, ct);
    var generated = await Db.Exams.CountAsync(ct);
    var avg = total == 0 ? 0
        : (int)Math.Round(await Set.Where(x => x.TotalQuestions != null)
              .Select(x => (double?)x.TotalQuestions).AverageAsync(ct) ?? 0);
    return new ExamTemplateStatsResponse(total, active, generated, avg);
}
```
(Xác minh tên DbSet `Db.Exams` trong AppDbContext — đã có `DbSet<Exam> Exams`.)
- [ ] **Step 3: Service** — thêm `GetStatsAsync` passthrough vào interface + impl.
- [ ] **Step 4: Controller** — mở `ExamTemplateController.cs`, khớp route (`api/exam-templates`?), thêm:
```csharp
[HttpGet("stats")]
public async Task<ActionResult<RequestResponse<ExamTemplateStatsResponse>>> GetStats(CancellationToken ct)
{
    var s = await service.GetStatsAsync(ct);
    return Ok(RequestResponse<ExamTemplateStatsResponse>.Success("Lấy thống kê thành công!", s, 1));
}
```
- [ ] **Step 5: Build** — Core + API (OutDir khác nếu API đang chạy). 0 error.
- [ ] **Step 6: Commit** — `git commit -m "feat(be): API thống kê mẫu đề thi (stats)"`.

---

## Task 2: FE — service + hook stats + type

**Files:** `types/*.d.ts`; `services/examTemplateService.ts`; `hooks/queries/useExamTemplates.ts`.

- [ ] **Step 1: Type** — `interface ExamTemplateStats { totalTemplates; activeTemplates; totalExamsGenerated; avgQuestions }`.
- [ ] **Step 2: Service** — `getStats() => AuthHttp.get<ExamTemplateStats>('/exam-templates/stats')` (khớp basePath thực tế trong file).
- [ ] **Step 3: Hook** — `useExamTemplateStatsQuery()` (queryKey `['examTemplateStats']`).
- [ ] **Step 4: Build** — `npx tsc --noEmit`. 0 error.
- [ ] **Step 5: Commit** — `git commit -m "feat(fe): service/hook stats mẫu đề thi"`.

---

## Task 3: FE — 06A ExamTemplatePage (stat cards + bảng)

**Files:** `pages/exams/ExamTemplatePage.tsx`.

- [ ] **Step 1: Stat cards** — hàng 4 thẻ (giống QuestionBank `StatCard`): Tổng mẫu (`#3a74f5`), Đang dùng (`#1ea375`), Tổng đề sinh (`#8b5cf6`), TB câu (`#d98a00`) từ `useExamTemplateStatsQuery`.
- [ ] **Step 2: Cột bảng** — sửa `columns`:
  - Ô "Tên mẫu đề": thêm **chấm màu** trước tên (xanh nếu `isActive`, xám nếu tắt).
  - Cột **Lớp**: badge nhỏ (VD "L10") thay vì text (dùng `gradeLevelName` hoặc rút gọn).
  - Đổi "Thời gian" → **TG** hiển thị `"{durationMinutes}'"`.
  - Bỏ cột **Người tạo**.
  - Thêm cột **Đảo** (`shuffleQuestions` → ✓ xanh/✗ xám) và **Chống trùng** (`preventDuplicate` → ✓/✗).
  - Cột **Trạng thái**: `StatusTag` (`isActive` → success "Hoạt động" / default "Ẩn").
  - Thao tác: **Sửa** (xanh) · **Sinh đề** (xanh lá) · **Xóa** (đỏ) — đúng thứ tự Figma.
- [ ] **Step 3: Build** — `npx tsc --noEmit`; đối chiếu frame 06A.
- [ ] **Step 4: Commit** — `git commit -m "feat(fe): 06A mẫu đề thi — stat cards + cột Đảo/Chống trùng + StatusTag"`.

---

## Task 4: FE — 06B CreateExamTemplatePage (form + section + nút)

**Files:** `pages/exams/CreateExamTemplatePage.tsx`.

- [ ] **Step 1: Field Mô tả** — thêm `Form.Item label="Mô tả" name="description"` (`Input.TextArea rows={2}`) dưới Tiêu đề, trước lưới Lớp/Môn. (Body đã có `description` — chỉ thêm UI.)
- [ ] **Step 2: Header phần thi** — mỗi section: header **thanh tối** (nền `#191d27`, chữ trắng) `"Phần {La Mã}: {tên}"` + nút × trắng. Số La Mã từ index (I, II, III…). Giữ input "Tên phần" bên trong hoặc gộp vào header.
- [ ] **Step 3: Ô % màu + chip tổng** — 4 `InputNumber` bọc màu: Dễ `#dff5ed`/`#1ea375`, TB `#fff4e5`/`#d98a00`, Khó `#fee5e5`/`#e74242`, RK `#f3ecfe`/`#8b5cf6`. Dưới đó chip **"Tổng: {pctEasy+TB+Khó+RK}% {✓ Hợp lệ | ✗}"** (xanh nếu =100, đỏ nếu ≠) tính trực tiếp từ `Form.useWatch('sections')` + "Điểm/câu: {scorePerQuestion}".
- [ ] **Step 4: Nút "Lưu & Sinh đề ngay"** — action-bar thêm nút xanh lá: `handleSubmit` với cờ → sau `create` thành công, `navigate('/app/generate?templateId=' + res.data.id)` thay vì về list. Chỉ hiện khi tạo mới (không phải edit).
- [ ] **Step 5: Build** — `npx tsc --noEmit` + `npm run build` exit 0; đối chiếu frame 06B (tạo mẫu, kiểm chip tổng 100%, nút Lưu&Sinh đề).
- [ ] **Step 6: Commit** — `git commit -m "feat(fe): 06B tạo mẫu đề — Mô tả, section header tối, ô % màu + chip tổng, Lưu & Sinh đề ngay"`.

---

## Self-Review

- **Gap coverage:** A(stat)→T1/T3, B(cột+chấm+badge)→T3, C(TG/StatusTag/actions)→T3, F(Mô tả)→T4, G(header tối)→T4, H(ô % màu+chip)→T4, I(Lưu&Sinh đề)→T4. Đủ.
- **Không cần BE cho cột Đảo/Chống trùng/Mô tả** — dữ liệu đã có trong `ExamTemplate`/response.
- **Type nhất quán:** `ExamTemplateStatsResponse`(BE) ↔ `ExamTemplateStats`(FE).
- **Rủi ro:** route controller (`api/exam-templates` vs `api/examtemplate`) — Task 1 Step 4/Task 2 Step 2 xác minh basePath thực tế; "Đang dùng" hiểu là `isActive` (nếu muốn = "đã sinh ≥1 đề" thì đổi query ở T1 Step 2).
