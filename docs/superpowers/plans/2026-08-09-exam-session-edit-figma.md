# Sửa kỳ thi (ExamSessionEditPage) — Figma Sync Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Cập nhật màn "Sửa/Tạo kỳ thi" (`ExamSessionEditPage.tsx`) khớp bố cục 2 cột của Figma frame "12 — Sửa kỳ thi", bao gồm thêm cột "Sĩ số" (cần field mới ở backend), thao tác "Phân tích", và gộp nút "Xuất bản" vào card thông tin.

**Architecture:** Backend bổ sung `StudentCount` vào `AssignmentResponse` (đếm `CohortMember` active theo cohort/section). Frontend đổi layout 1 cột → 2 cột (trái = form thông tin + Lưu/Xuất bản; phải = Đề trong kỳ thi + Giao lớp/khoá), dùng card admin trắng thay `paper-panel` giấy ấm, thêm cột Sĩ số, thêm nút Phân tích mở `AnalyticsDrawer`.

**Tech Stack:** ASP.NET Core .NET 10 + EF Core (BE); React 19 + AntD 6.3 + Tailwind v4 + TanStack Query (FE).

## Global Constraints

- Mỗi thao tác thêm/sửa/xoá vẫn đi qua validate hiện có ở service (không nới lỏng).
- Không push lên origin; merge local theo yêu cầu riêng.
- BE verify bằng `dotnet build` (dùng `-p:OutDir=obj/verifyN/` nếu API đang chạy khoá bin). FE verify bằng `npx tsc --noEmit` + `npx vite build`.
- Giữ nguyên `is_active`, `pickMode` options hiện có.

---

### Task 1: Backend — thêm StudentCount vào AssignmentResponse

**Files:**
- Modify: `exam_hub_api/ExamHub.Core/DataTransferObjects/ExamSession/ExamSessionDtos.cs:69`
- Modify: `exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamSessionRepositories.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs`
- Modify: `exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs:62-70`

**Interfaces:**
- Produces: `AssignmentResponse` với tham số cuối `int StudentCount`; repo method `Task<int> CountStudentsForAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default)`.

- [ ] **Step 1: Thêm `StudentCount` vào record**

Sửa dòng 69 của `ExamSessionDtos.cs`:

```csharp
public sealed record AssignmentResponse(Guid Id, int? CohortId, string? CohortName, int? CohortClassId, string? CohortClassName, string? SchoolName, int StudentCount);
```

- [ ] **Step 2: Khai báo repo method trong interface**

Trong `IExamSessionRepositories.cs`, thêm vào interface repository (gần các method assignment như `AddAssignmentAsync`):

```csharp
/// <summary>Đếm số HS active thuộc phạm vi 1 assignment (cả khoá hoặc 1 lớp/section).</summary>
Task<int> CountStudentsForAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default);
```

- [ ] **Step 3: Cài đặt repo method**

Trong `ExamSessionRepository.cs`, thêm method (dùng đúng mô hình membership: `CohortMember.CohortId` + `Section` khớp `CohortClass.Section`):

```csharp
public async Task<int> CountStudentsForAssignmentAsync(ExamSessionAssignment a, CancellationToken ct = default)
{
    if (a.CohortClassId != null && a.CohortClass != null)
    {
        var cid = a.CohortClass.CohortId;
        var section = a.CohortClass.Section;
        return await _db.Set<CohortMember>()
            .CountAsync(m => m.IsActive && m.CohortId == cid && m.Section != null && m.Section == section, ct);
    }
    if (a.CohortId != null)
    {
        var cid = a.CohortId.Value;
        return await _db.Set<CohortMember>()
            .CountAsync(m => m.IsActive && m.CohortId == cid, ct);
    }
    return 0;
}
```

Đảm bảo `using ExamHub.Core.Domain.Entities;` đã có (CohortMember). Nếu chưa, thêm.

- [ ] **Step 4: Map StudentCount trong service GetDetailAsync**

Trong `ExamSessionService.cs`, đổi khối build `assignments` (dòng 62-70) từ `.Select(...)` đồng bộ sang tính count bất đồng bộ:

```csharp
var assignments = new List<AssignmentResponse>(s.Assignments.Count);
foreach (var a in s.Assignments)
{
    var count = await _repo.CountStudentsForAssignmentAsync(a, ct);
    assignments.Add(new AssignmentResponse(
        a.Id,
        a.CohortId,
        a.CohortClass?.Cohort?.Name ?? a.Cohort?.Name,
        a.CohortClassId,
        a.CohortClass?.ClassName,
        a.CohortClass?.Cohort?.School?.Name ?? a.Cohort?.School?.Name,
        count));
}
```

- [ ] **Step 5: Build backend**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj -p:OutDir=obj/verify1/`
Expected: Build succeeded, 0 errors. (Nếu MSB file-lock do API đang chạy → vẫn coi là pass nếu chỉ lỗi MSB3021/MSB3027.)

- [ ] **Step 6: Commit**

```bash
git add exam_hub_api/ExamHub.Core/DataTransferObjects/ExamSession/ExamSessionDtos.cs exam_hub_api/ExamHub.Core/Domain/Interfaces/IExamSessionRepositories.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Repositories/Implementations/ExamSessionRepository.cs exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/ExamSessionService.cs
git commit -m "feat(exam-session): add StudentCount to assignment response"
```

---

### Task 2: Frontend types — thêm studentCount

**Files:**
- Modify: `exam_hub_web/src/types/examSession.d.ts:38-45`

**Interfaces:**
- Consumes: `AssignmentResponse.StudentCount` từ Task 1.
- Produces: `SessionAssignment.studentCount: number`.

- [ ] **Step 1: Thêm field vào interface**

Trong `SessionAssignment` (dòng 38-45), thêm:

```typescript
interface SessionAssignment {
    id: string
    cohortId?: number
    cohortName?: string
    cohortClassId?: number
    cohortClassName?: string
    schoolName?: string
    studentCount: number
}
```

- [ ] **Step 2: Verify tsc**

Run: `cd exam_hub_web; npx tsc --noEmit`
Expected: 0 lỗi mới (các lỗi category/*/index.tsx tiền tồn không tính).

---

### Task 3: Frontend — restructure ExamSessionEditPage theo Figma 2 cột

**Files:**
- Modify: `exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx`

**Interfaces:**
- Consumes: `SessionAssignment.studentCount`, `AnalyticsDrawer` (`../exams/AnalyticsDrawer` — export `AnalyticsDrawer({examId, onClose})`).

- [ ] **Step 1: Import AnalyticsDrawer + đổi nền wrapper**

Thêm import ở đầu file:

```typescript
import {AnalyticsDrawer} from './AnalyticsDrawer'
```

Đổi wrapper ngoài (dòng ~122) từ nền giấy ấm sang nền admin chuẩn + bố cục 2 cột. Thay:

```tsx
<div className="exam-admin-bg flex-1 overflow-auto p-6">
    <div className="flex flex-col gap-4 max-w-4xl mx-auto">
```

thành:

```tsx
<div className="flex-1 overflow-auto p-6">
    <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 max-w-7xl mx-auto items-start">
```

(Nhớ đóng đủ 2 `</div>` như cũ ở cuối.)

- [ ] **Step 2: Card thông tin — đổi sang card admin + gộp nút Xuất bản**

Đổi `className="paper-panel"` của `<Form>` → `className="bg-white rounded-xl border p-5 border-[#eceef2]"`, và `paper-panel-title` → `text-[15px] font-semibold text-[#191d27]`.

Đổi footer nút (dòng ~162-164) từ 1 nút thành cụm Lưu + Xuất bản. Thay khối `<Button type="primary" ... onClick={handleSave}>...</Button>` bằng:

```tsx
<div className="flex items-center gap-3 mt-2">
    <Button type="primary" loading={create.isPending || update.isPending} onClick={handleSave}>
        {isEdit ? 'Lưu thay đổi' : 'Tạo & tiếp tục'}
    </Button>
    {isEdit && detail && (
        <Button
            className="border-[#1ea375] text-[#1ea375]"
            disabled={isPublished}
            loading={publish.isPending}
            onClick={() => publish.mutate(detail.id)}
        >
            {isPublished ? 'Đã xuất bản' : 'Xuất bản'}
        </Button>
    )}
</div>
```

- [ ] **Step 3: Gói cột phải + bỏ panel "Phát hành" riêng**

Bọc `PoolSection` + `AssignmentSection` vào 1 div cột phải và **xoá** khối `<div className="paper-panel flexitems-center...">...Phát hành...</div>` (dòng ~173-182, đã thay bằng nút Xuất bản ở Step 2). Thay khối `{isEdit && detail && (<> ... </>)}` bằng:

```tsx
{isEdit && detail && (
    <div className="flex flex-col gap-4">
        <PoolSection sessionId={detail.id} exams={detail.exams}
                     subjectId={detail.subjectId} gradeLevelId={detail.gradeLevelId}/>
        <AssignmentSection sessionId={detail.id} assignments={detail.assignments}/>
    </div>
)}
```

Lưu ý: khi **chưa** `isEdit` (tạo mới) cột phải trống — chấp nhận được (Figma chỉ áp dụng cho bản edit đã có dữ liệu).

- [ ] **Step 4: PoolSection — đổi card style + thêm thao tác "Phân tích"**

Trong `PoolSection`, đổi `className="paper-panel flex flex-col gap-3"` → `className="bg-white rounded-xl border p-5 border-[#eceef2] flex flex-col gap-3"`, `paper-panel-title` → `text-[15px] font-semibold text-[#191d27]`, và đổi nút "Thêm đề" sang primary: `<Button type="primary" icon={<PlusOutlined/>} ...>Thêm đề</Button>`.

Thêm state drawer + cột "Phân tích" trong cột actions. Ở đầu `PoolSection`:

```tsx
const [analyticsExamId, setAnalyticsExamId] = useState<string>()
```

Đổi cột actions (dòng ~204-212) thành:

```tsx
{
    title: 'Thao tác', key: 'actions', width: 140,
    render: (_, e) => (
        <div className="flex items-center gap-3">
            <button className="text-[13px] hover:underline" style={{color: '#3a74f5'}}
                    onClick={() => setAnalyticsExamId(e.examId)}>Phân tích</button>
            <Popconfirm title="Gỡ đề khỏi kỳ thi?" okText="Gỡ" cancelText="Hủy"
                        onConfirm={() => removeExam.mutate({id: sessionId, examId: e.examId})}>
                <button className="btn-delete">Gỡ</button>
            </Popconfirm>
        </div>
    ),
},
```

Và render drawer trước khi đóng div cuối của `PoolSection` (cạnh `AddExamsModal`):

```tsx
<AnalyticsDrawer examId={analyticsExamId} onClose={() => setAnalyticsExamId(undefined)}/>
```

- [ ] **Step 5: AssignmentSection — đổi card style + thay cột "Phạm vi" bằng "Sĩ số"**

Trong `AssignmentSection`, đổi `className="paper-panel flex flex-col gap-3"` → `className="bg-white rounded-xl border p-5 border-[#eceef2] flex flex-col gap-3"`, `paper-panel-title` → `text-[15px] font-semibold text-[#191d27]`.

Thay cột `Phạm vi` (dòng ~308-311) bằng cột `Sĩ số`:

```tsx
{
    title: 'Sĩ số', key: 'studentCount', width: 90, align: 'center',
    render: (_, a) => a.studentCount,
},
```

- [ ] **Step 6: Verify FE build**

Run: `cd exam_hub_web; npx tsc --noEmit`
Expected: 0 lỗi mới.

Run: `cd exam_hub_web; npx vite build`
Expected: ✓ built (exit 0).

- [ ] **Step 7: Kiểm tra bằng mắt (tùy chọn) + Commit**

```bash
git add exam_hub_web/src/types/examSession.d.ts exam_hub_web/src/pages/exams/ExamSessionEditPage.tsx
git commit -m "feat(exam-session): match Sửa kỳ thi layout to Figma (2-col, Sĩ số, Phân tích)"
```

---

## Self-Review

- **Spec coverage:** Bố cục 2 cột (Task 3 Step 1,3) ✓; card admin thay paper-panel (Step 2,4,5) ✓; Xuất bản trong footer (Step 2) ✓; Phân tích action (Step 4) ✓; cột Sĩ số (Task 1 BE + Task 2 type + Task 3 Step 5) ✓.
- **Type consistency:** `studentCount: number` (FE) ↔ `int StudentCount` (BE record, tham số cuối) ✓. `AnalyticsDrawer` prop `examId?/onClose` khớp ✓.
- **Placeholder scan:** không có TBD/TODO; mọi bước có code cụ thể.
- **Rủi ro:** N+1 CountAsync trong GetDetailAsync — chấp nhận vì số assignment/kỳ thi nhỏ.
