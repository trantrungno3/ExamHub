# Đồng bộ màn Ngân hàng câu hỏi theo Figma 05A — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (hoặc subagent-driven-development) để thực thi từng task. Steps dùng checkbox `- [ ]`.

**Goal:** Cập nhật `QuestionBankPage` khớp Figma frame 05A: 4 stat card, dải chú thích Bloom, chip Bloom đánh số/màu, chip Loại/Độ khó pastel, badge trạng thái + "Bỏ duyệt", checkbox chọn dòng + thao tác hàng loạt, dòng phụ ở ô nội dung.

**Architecture:** Thêm API đếm câu hỏi ở backend; frontend dựng lại phần trình bày `QuestionBankPage` (tái dùng `StatusTag`, theme đã có). Bulk verify/delete thực hiện client-side bằng lặp mutation per-id sẵn có.

**Tech Stack:** ASP.NET Core + EF, React 19, AntD 6, TanStack Query.

**Base branch:** `feat/ui-figma-sync` (đã có theme #3a74f5/Inter + `components/StatusTag.tsx`). Tạo nhánh con `feat/question-bank-figma` từ đó, hoặc commit trực tiếp lên `feat/ui-figma-sync`.

## Global Constraints

- Chỉ presentation + 1 endpoint đọc (đếm) + 1 endpoint unverify; không đổi schema.
- Palette Figma: primary `#3a74f5`, success `#1ea375`/soft `#dff5ed`, warning `#d98a00`/soft `#fff4e5`, danger `#e74242`/soft `#fee5e5`, muted `#6f7788`.
- Bloom (theo `code`, 6 cấp) — map số + màu: 1.Nhớ `#1ea375`, 2.Hiểu `#3a74f5`, 3.Vận dụng `#d98a00`, 4.Phân tích `#8b5cf6`, 5.Đánh giá `#e74242`, 6.Sáng tạo `#0ea5a5`. **Task 3 Step 1 phải đọc `code` thực tế** của cognitive_levels (seed/DB) để khớp key.
- Verify: BE `dotnet build`; FE `npx tsc --noEmit` + `npm run build`; đối chiếu screenshot frame 05A (`Rz9AFnw0McsXm6HFIspSyG` node 4:2).
- Commit message kết `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`.

## Ngoài phạm vi

- Filter "Môn học" (query BE chưa có `subjectId`) — giữ filter **Chủ đề** hiện tại.

---

## File Structure

- BE: `QuestionController.cs` (+ stats, +unverify), `IQuestionService`/impl, `IQuestionRepository`/impl (CountByStatus), DTO `QuestionStatsDto`.
- FE: `types/question.d.ts` (+QuestionStats), `services/questionService.ts` (+getStats, +unverify), `hooks/queries/useQuestions.ts` (+useQuestionStatsQuery, +useUnverifyMutation), `pages/questions/QuestionBankPage.tsx` (dựng lại UI).

---

## Task 1: BE — API đếm câu hỏi theo trạng thái

**Files:** `DataTransferObjects/Question/QuestionStatsDto.cs` (new); `IQuestionRepository`+impl; `IQuestionService`+impl; `QuestionController.cs`.

**Interfaces:**
- Produces: `record QuestionStatsResponse(int Total, int Verified, int Unverified, int Inactive)`; repo `Task<QuestionStatsResponse> GetStatsAsync(ct)`; service passthrough; `GET api/question/stats`.

- [ ] **Step 1: DTO** — tạo `QuestionStatsResponse(int Total, int Verified, int Unverified, int Inactive)`.

- [ ] **Step 2: Repository** — thêm `GetStatsAsync` vào interface + impl (EF trên `Set`/`Db.Questions`):
```csharp
public async Task<QuestionStatsResponse> GetStatsAsync(CancellationToken ct = default)
{
    var total    = await Set.CountAsync(ct);
    var verified = await Set.CountAsync(q => q.IsVerified, ct);
    var inactive = await Set.CountAsync(q => !q.IsActive, ct);
    return new QuestionStatsResponse(total, verified, total - verified, inactive);
}
```

- [ ] **Step 3: Service** — thêm `GetStatsAsync` (passthrough repo) vào interface + impl.

- [ ] **Step 4: Controller** — thêm endpoint (mở `QuestionController.cs`, khớp route hiện có, thường `api/question`):
```csharp
[HttpGet("stats")]
[Authorize]
public async Task<ActionResult<RequestResponse<QuestionStatsResponse>>> GetStats(CancellationToken ct = default)
{
    var s = await service.GetStatsAsync(ct);
    return Ok(RequestResponse<QuestionStatsResponse>.Success("Lấy thống kê thành công!", s, 1));
}
```

- [ ] **Step 5: Build** — `cd exam_hub_api && dotnet build ExamHub.Core/ExamHub.Core.csproj` + build API (ra thư mục khác nếu instance đang chạy khoá bin). Expected: 0 error.

- [ ] **Step 6: Commit** — `git commit -m "feat(be): API đếm câu hỏi theo trạng thái (stats)"`.

---

## Task 2: BE — endpoint bỏ duyệt (unverify)

**Files:** `IQuestionRepository`+impl (hoặc dùng update sẵn có), `IQuestionService`+impl, `QuestionController.cs`.

- [ ] **Step 1: Kiểm verify hiện tại** — đọc endpoint `verify` trong `QuestionController` + service `VerifyAsync`. Nếu có `SetVerifiedAsync(id, bool)` thì tái dùng; nếu chỉ có verify=true, thêm `UnverifyAsync(id)` (set IsVerified=false, VerifiedBy/At=null).

- [ ] **Step 2: Endpoint** — `PATCH api/question/{id:guid}/unverify` → `service.UnverifyAsync(id, ct)` → `RequestResponse<bool>.Success("Đã bỏ duyệt!", true, 1)`.

- [ ] **Step 3: Build** — `dotnet build`. Expected: 0 error.

- [ ] **Step 4: Commit** — `git commit -m "feat(be): endpoint bỏ duyệt câu hỏi"`.

---

## Task 3: FE — stat cards + dải Bloom

**Files:** `types/question.d.ts`, `services/questionService.ts`, `hooks/queries/useQuestions.ts`, `pages/questions/QuestionBankPage.tsx`.

- [ ] **Step 1: Đọc Bloom codes** — chạy `docker exec postgres psql -U admin -d examhub -c "SELECT id, code, name FROM cognitive_levels ORDER BY id"` để lấy `code` thực; lập map `BLOOM` = {code: {num, color, short}} theo Global Constraints.

- [ ] **Step 2: Type + service + hook** — `interface QuestionStats { total; verified; unverified; inactive }`; `questionService.getStats()` → `AuthHttp.get<QuestionStats>('/question/stats')`; `useQuestionStatsQuery()` (queryKey `['questionStats']`).

- [ ] **Step 3: Stat cards** — trong `QuestionBankPage`, trên filter, thêm hàng 4 thẻ (card trắng bo 12, viền, icon vuông màu + số lớn + nhãn):
  - Tổng câu hỏi (`#3a74f5`), Đã duyệt (`#1ea375`), Chờ duyệt (`#d98a00`), Không HĐ (`#e74242`). Số từ `useQuestionStatsQuery`.

- [ ] **Step 4: Dải chú thích Bloom** — dưới filter: `Bloom:` + 6 chip `{num}.{short}` màu theo map.

- [ ] **Step 5: Verify** — `npx tsc --noEmit`; `npm run dev` đối chiếu phần trên của frame 05A.

- [ ] **Step 6: Commit** — `git commit -m "feat(fe): question bank — stat cards + dải Bloom"`.

---

## Task 4: FE — bảng: chip Bloom/Loại/Độ khó + badge trạng thái + Bỏ duyệt

**Files:** `pages/questions/QuestionBankPage.tsx`, `hooks/queries/useQuestions.ts` (+unverify mutation).

- [ ] **Step 1: Bloom chip** — cột Bloom render chip `{num}.{name}` nền soft + chữ theo màu Bloom (map từ `cognitiveLevelId`→code, hoặc join cognitives list).

- [ ] **Step 2: Chip Loại/Độ khó** — map màu pastel: Loại theo `questionTypeName` (ví dụ TN→blue, Tự luận→indigo, Đ/S→teal, Nối cột→amber, Điền→pink); Độ khó theo code (easy→success, medium→warning, hard→danger, very_hard→danger đậm). Thay `Tag` bằng chip `StatusTag`-style hoặc span nền soft.

- [ ] **Step 3: Cột Duyệt** — đổi tiêu đề "Trạng thái"→"Duyệt"; dùng `StatusTag` (`success`=Đã duyệt, `warning`=Chờ duyệt).

- [ ] **Step 4: Action "Bỏ duyệt"** — thêm `useUnverifyMutation` (gọi `questionService.unverify(id)`, invalidate `questions`+`questionStats`); cột Thao tác: `Sửa` · (Đã duyệt → `Bỏ duyệt`, Chưa → `Duyệt`) · `Xóa`.

- [ ] **Step 5: Verify** — `npx tsc --noEmit`; đối chiếu các cột bảng frame 05A.

- [ ] **Step 6: Commit** — `git commit -m "feat(fe): question bank — chip Bloom/Loại/Độ khó + badge + bỏ duyệt"`.

---

## Task 5: FE — checkbox chọn dòng + thanh thao tác hàng loạt + dòng phụ nội dung

**Files:** `pages/questions/QuestionBankPage.tsx`.

- [ ] **Step 1: Row selection** — `Table` thêm `rowSelection={{selectedRowKeys, onChange}}`; state `selectedRowKeys: string[]`.

- [ ] **Step 2: Bulk bar** — khi `selectedRowKeys.length > 0`, hiện thanh phía trên bảng: "Đã chọn N" + nút **Duyệt hàng loạt** + **Xoá hàng loạt** (Popconfirm). Thực thi client-side:
```ts
const bulkVerify = async () => {
  await Promise.all(selectedRowKeys.map(id => verifyMutation.mutateAsync(id)))
  setSelectedRowKeys([])
}
// tương tự bulkDelete với deleteMutation
```
Sau khi xong invalidate `questions` + `questionStats`.

- [ ] **Step 3: Dòng phụ ô Nội dung** — cột "Nội dung câu hỏi" render 2 dòng: nội dung (đậm) + dòng phụ nhỏ (chủ đề `topicName` hoặc nguồn `source`), màu `#9aa2b1`.

- [ ] **Step 4: Verify** — `npx tsc --noEmit` + `npm run build` (exit 0, bỏ qua lỗi có sẵn ngoài file này nếu có); `npm run dev` đối chiếu tổng thể frame 05A (chọn dòng → thanh bulk; duyệt/xoá hàng loạt cập nhật stat).

- [ ] **Step 5: Commit** — `git commit -m "feat(fe): question bank — checkbox + thao tác hàng loạt + dòng phụ nội dung"`.

---

## Self-Review

- **Gap coverage:** A(stat)→T1/T3, B(Bloom legend)→T3, C(checkbox+bulk)→T5, D(dòng phụ)→T5, E(Bloom chip)→T4, F(badge+Bỏ duyệt)→T2/T4, G(chip màu)→T4. Đủ (H filter Môn học: ngoài scope, đã nêu).
- **Placeholder:** code cụ thể cho stats/bulk; map Bloom cần đọc `code` thực (T3 Step 1) — đã chỉ rõ cách lấy.
- **Type consistency:** `QuestionStatsResponse`(BE)/`QuestionStats`(FE); mutation `useUnverifyMutation` khớp service `unverify`.
- **Rủi ro:** route `QuestionController` cần xác minh (T1 Step 4); bulk client-loop nhiều request với lựa chọn lớn — chấp nhận cho bản này.
