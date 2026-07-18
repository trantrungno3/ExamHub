# Bulk User Import via Excel — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let an Admin bulk-create user accounts on `app/users` by uploading an `.xlsx` file, with per-row error reporting and partial import.

**Architecture:** Full-stack feature mirroring the existing question bulk-import. Backend: a dedicated `UserBulkImportService` (ClosedXML) + two `UserController` endpoints (`POST bulk-import`, `GET bulk-import/template`). Frontend: a `UserBulkImportModal` + `userService` methods + a button on `UserPage`. The template `.xlsx` is generated server-side (single source of truth, no new frontend dependency).

**Tech Stack:** .NET / ASP.NET Core, ClosedXML, C# records; React 19, Ant Design v6, TypeScript, fetch-based `AuthHttp`.

> **Testing note:** This repo has **no backend test project** and the existing question importer shipped without automated tests. To stay consistent with the codebase and avoid unilaterally scaffolding a test project, verification here is **compiler + manual end-to-end**. Adding an xUnit project for `UserBulkImportService` is a reasonable follow-up if the team wants it.

---

## File Structure

**Backend (ExamHub.API / ExamHub.Core):**
- Create: `ExamHub.Core/DataTransferObjects/User/BulkUserImportDto.cs` — request/response DTOs.
- Create: `ExamHub.Core/Application/Services/IUserBulkImportService.cs` — service contract.
- Create: `ExamHub.Core/Infrastructure/Persistence/Services/Implementations/UserBulkImportService.cs` — ClosedXML parsing + template builder.
- Modify: `ExamHub.Core/DependencyContainer.cs` — register the service.
- Modify: `ExamHub.API/Controllers/UserController.cs` — inject service + add 2 endpoints.

**Frontend (exam_hub_web):**
- Modify: `src/services/requestService.ts` — add a `getBlob` helper to the HTTP factory.
- Modify: `src/services/userService.ts` — add `bulkImport` + `downloadTemplate`.
- Create: `src/pages/user/UserBulkImportModal.tsx` — the modal UI.
- Modify: `src/pages/user/UserPage.tsx` — button + modal wiring.

Types (`BulkImportResult`, `BulkImportRowError`) already exist as ambient global interfaces in `src/types/question.d.ts` and are reused as-is — no new type file.

---

## Task 1: Backend DTOs

**Files:**
- Create: `ExamHub.Core/DataTransferObjects/User/BulkUserImportDto.cs`

Reuses the existing `BulkImportRowError` record (defined in `ExamHub.Core.DataTransferObjects.Question`) rather than redefining it.

- [ ] **Step 1: Create the DTO file**

```csharp
using ExamHub.Core.DataTransferObjects.Question;
using Microsoft.AspNetCore.Http;

namespace ExamHub.Core.DataTransferObjects.User;

/// <summary>Request bulk import người dùng từ file XLSX.</summary>
public record BulkUserImportRequest(
    IFormFile File,
    string DefaultPassword);

/// <summary>Kết quả tổng hợp sau khi import người dùng.</summary>
public record BulkUserImportResponse(
    int SuccessCount,
    int ErrorCount,
    IReadOnlyList<BulkImportRowError> Errors);
```

- [ ] **Step 2: Build to verify it compiles**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj`
Expected: Build succeeded (0 errors).

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.Core/DataTransferObjects/User/BulkUserImportDto.cs
git commit -m "feat(api): add bulk user import DTOs"
```

---

## Task 2: Service interface

**Files:**
- Create: `ExamHub.Core/Application/Services/IUserBulkImportService.cs`

- [ ] **Step 1: Create the interface**

```csharp
using ExamHub.Core.DataTransferObjects.User;

namespace ExamHub.Core.Application.Services;

/// <summary>
/// Dịch vụ import người dùng hàng loạt từ file Excel (.xlsx).
/// </summary>
/// <remarks>
/// Định dạng cột (dòng 1 là tiêu đề, dữ liệu từ dòng 2):
/// <list type="number">
/// <item>UserName (bắt buộc, duy nhất)</item>
/// <item>DisplayName (bắt buộc)</item>
/// <item>Email (tuỳ chọn)</item>
/// <item>PhoneNumber (tuỳ chọn)</item>
/// <item>Sex (tuỳ chọn — "Nam"/"Nữ" hoặc "0"/"1"; trống = Nam)</item>
/// <item>Role (tuỳ chọn — Admin/Teacher/Student, nhiều vai trò cách nhau bởi dấu phẩy)</item>
/// </list>
/// Mỗi dòng xử lý độc lập; dòng lỗi được báo cáo, dòng hợp lệ vẫn được lưu (import một phần).
/// Mật khẩu lấy từ <c>DefaultPassword</c> — áp cho mọi tài khoản.
/// </remarks>
public interface IUserBulkImportService
{
    /// <summary>Phân tích file Excel và tạo các tài khoản hợp lệ.</summary>
    Task<BulkUserImportResponse> ImportAsync(BulkUserImportRequest request, CancellationToken ct = default);

    /// <summary>Tạo file Excel mẫu (chỉ có dòng tiêu đề và một dòng ví dụ).</summary>
    byte[] BuildTemplate();
}
```

- [ ] **Step 2: Build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Application/Services/IUserBulkImportService.cs
git commit -m "feat(api): add IUserBulkImportService contract"
```

---

## Task 3: Service implementation (ClosedXML)

**Files:**
- Create: `ExamHub.Core/Infrastructure/Persistence/Services/Implementations/UserBulkImportService.cs`

Mirrors `BulkImportService.cs` helper style (`Cell`, `RowIsEmpty`, private `BulkImportRowException`). Depends on the existing `IUserManagementService`.

- [ ] **Step 1: Create the implementation**

```csharp
using ClosedXML.Excel;
using ExamHub.Core.Application.Services;
using ExamHub.Core.DataTransferObjects.Question;
using ExamHub.Core.DataTransferObjects.User;

namespace ExamHub.Core.Infrastructure.Persistence.Services.Implementations;

/// <summary>Triển khai import người dùng hàng loạt từ Excel bằng ClosedXML.</summary>
public class UserBulkImportService(IUserManagementService userService) : IUserBulkImportService
{
    private const int HeaderRows = 1;
    private const int ColumnCount = 6;

    private static readonly string[] Headers =
        ["UserName", "DisplayName", "Email", "PhoneNumber", "Sex", "Role"];

    private static readonly IReadOnlyDictionary<string, string> AllowedRoles =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = "Admin",
            ["teacher"] = "Teacher",
            ["student"] = "Student",
        };

    /// <inheritdoc/>
    public async Task<BulkUserImportResponse> ImportAsync(
        BulkUserImportRequest request, CancellationToken ct = default)
    {
        var errors = new List<BulkImportRowError>();
        var seenUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var success = 0;

        await using var stream = request.File.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        for (int row = HeaderRows + 1; row <= lastRow; row++)
        {
            ct.ThrowIfCancellationRequested();

            if (RowIsEmpty(sheet, row)) continue;

            try
            {
                var (createRequest, roles) = ParseRow(sheet, row, request.DefaultPassword, seenUserNames);

                if (await userService.CheckUserNameExistAsync(createRequest.UserName))
                    throw new BulkImportRowException($"Tên đăng nhập '{createRequest.UserName}' đã tồn tại.");

                var created = await userService.CreateAsync(createRequest)
                    ?? throw new BulkImportRowException("Không thể tạo tài khoản (lỗi lưu dữ liệu).");

                if (roles.Length > 0)
                    await userService.SetRolesAsync(created.Id, roles);

                success++;
            }
            catch (BulkImportRowException ex)
            {
                errors.Add(new BulkImportRowError(row, ex.Message));
            }
        }

        return new BulkUserImportResponse(success, errors.Count, errors);
    }

    /// <inheritdoc/>
    public byte[] BuildTemplate()
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Users");

        for (int c = 0; c < Headers.Length; c++)
        {
            var cell = sheet.Cell(1, c + 1);
            cell.Value = Headers[c];
            cell.Style.Font.Bold = true;
        }

        // Dòng ví dụ
        sheet.Cell(2, 1).Value = "nguyenvana";
        sheet.Cell(2, 2).Value = "Nguyễn Văn A";
        sheet.Cell(2, 3).Value = "a@example.com";
        sheet.Cell(2, 4).Value = "0900000000";
        sheet.Cell(2, 5).Value = "Nam";
        sheet.Cell(2, 6).Value = "Student";

        sheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static (CreateUserRequest, string[]) ParseRow(
        IXLWorksheet sheet, int row, string defaultPassword, HashSet<string> seenUserNames)
    {
        var userName = Cell(sheet, row, 1);
        if (string.IsNullOrWhiteSpace(userName))
            throw new BulkImportRowException("Cột UserName không được để trống.");
        // In-file duplicate detection: Add returns false if this UserName was already seen.
        if (!seenUserNames.Add(userName))
            throw new BulkImportRowException($"Tên đăng nhập '{userName}' bị trùng trong file.");

        var displayName = Cell(sheet, row, 2);
        if (string.IsNullOrWhiteSpace(displayName))
            throw new BulkImportRowException("Cột DisplayName không được để trống.");

        var email = Cell(sheet, row, 3);
        var phone = Cell(sheet, row, 4);
        var sex = ParseSex(Cell(sheet, row, 5));
        var roles = ParseRoles(Cell(sheet, row, 6));

        var request = new CreateUserRequest
        {
            UserName = userName,
            Password = defaultPassword,
            DisplayName = displayName,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            Sex = sex,
        };

        return (request, roles);
    }

    /// <summary>"Nữ"/"1"/"true" => true (Nữ); còn lại => false (Nam).</summary>
    private static bool ParseSex(string raw)
    {
        var v = raw.Trim().ToLowerInvariant();
        return v is "nữ" or "nu" or "1" or "true" or "female" or "f";
    }

    private static string[] ParseRoles(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return [];
        var result = new List<string>();
        foreach (var part in raw.Split([',', ';', '/'], StringSplitOptions.RemoveEmptyEntries))
        {
            var key = part.Trim();
            if (!AllowedRoles.TryGetValue(key, out var canonical))
                throw new BulkImportRowException($"Vai trò '{key}' không hợp lệ (chỉ Admin/Teacher/Student).");
            if (!result.Contains(canonical)) result.Add(canonical);
        }
        return [.. result];
    }

    private static string Cell(IXLWorksheet sheet, int row, int col)
        => sheet.Cell(row, col).GetString().Trim();

    private static bool RowIsEmpty(IXLWorksheet sheet, int row)
    {
        for (int c = 1; c <= ColumnCount; c++)
            if (!string.IsNullOrWhiteSpace(Cell(sheet, row, c)))
                return false;
        return true;
    }

    /// <summary>Lỗi cấp dòng — gom vào báo cáo, không dừng cả import.</summary>
    private sealed class BulkImportRowException(string message) : Exception(message);
}
```

- [ ] **Step 2: Build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.Core/Infrastructure/Persistence/Services/Implementations/UserBulkImportService.cs
git commit -m "feat(api): implement UserBulkImportService with ClosedXML"
```

---

## Task 4: Register the service in DI

**Files:**
- Modify: `ExamHub.Core/DependencyContainer.cs:107`

- [ ] **Step 1: Add the registration**

In `AddAppServices()`, add the line immediately after the existing `IUserManagementService` registration (line 107):

```csharp
            return services
                .AddScoped<IUserManagementService, UserManagementService>()
                .AddScoped<IUserBulkImportService, UserBulkImportService>()
                // Config / Lookup
```

- [ ] **Step 2: Build**

Run: `dotnet build exam_hub_api/ExamHub.Core/ExamHub.Core.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_api/ExamHub.Core/DependencyContainer.cs
git commit -m "feat(api): register IUserBulkImportService in DI"
```

---

## Task 5: UserController endpoints

**Files:**
- Modify: `ExamHub.API/Controllers/UserController.cs`

- [ ] **Step 1: Inject the service into the primary constructor**

Change the class declaration (line 14):

```csharp
public class UserController(
    IUserManagementService userService,
    IUserBulkImportService bulkUserImportService) : AuthorizeControllerBase
```

- [ ] **Step 2: Add the two endpoints**

Add after the `Create` action (after line 48), before the `Update` action:

```csharp
    /// <summary>Import người dùng hàng loạt từ file Excel (.xlsx)</summary>
    [HttpPost("bulk-import")]
    public async Task<ActionResult<RequestResponse<BulkUserImportResponse>>> BulkImport(
        [FromForm] BulkUserImportRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(RequestResponse<object>.Error("File import không được để trống."));
        if (string.IsNullOrWhiteSpace(request.DefaultPassword))
            return BadRequest(RequestResponse<object>.Error("Mật khẩu mặc định không được để trống."));

        var result = await bulkUserImportService.ImportAsync(request, ct);
        return Ok(RequestResponse<BulkUserImportResponse>.Success(
            $"Import hoàn tất: {result.SuccessCount} thành công, {result.ErrorCount} lỗi.",
            result, result.SuccessCount));
    }

    /// <summary>Tải file Excel mẫu để import người dùng</summary>
    [HttpGet("bulk-import/template")]
    public IActionResult DownloadImportTemplate()
    {
        var bytes = bulkUserImportService.BuildTemplate();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "user-import-template.xlsx");
    }
```

- [ ] **Step 3: Add the using directive if missing**

Ensure the top of the file has (the DTO namespace is already imported via `ExamHub.Core.DataTransferObjects.User`; `BulkUserImportRequest`/`BulkUserImportResponse` live there). Confirm this line is present:

```csharp
using ExamHub.Core.DataTransferObjects.User;
```

- [ ] **Step 4: Build the whole API**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`
Expected: Build succeeded (0 errors).

- [ ] **Step 5: Commit**

```bash
git add exam_hub_api/ExamHub.API/Controllers/UserController.cs
git commit -m "feat(api): add bulk-import and template endpoints to UserController"
```

---

## Task 6: Frontend — `getBlob` HTTP helper

**Files:**
- Modify: `src/services/requestService.ts:135` (inside the `createHttp` factory, next to `postForm`)

Needed because template download is Admin-only and must send the auth header.

- [ ] **Step 1: Add `getBlob` to the factory**

Add this method inside the object returned by `createHttp`, right after `postForm`:

```typescript
        async getBlob(path: string): Promise<Blob> {
            const res = await fetchWithTimeout(buildUrl(path), {
                method: 'GET', headers: buildFormHeaders(auth),
            })
            if (!res.ok) throw new Error('Không thể tải file từ máy chủ.')
            return res.blob()
        },
```

- [ ] **Step 2: Type-check**

Run: `cd exam_hub_web; npx tsc -b`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_web/src/services/requestService.ts
git commit -m "feat(web): add getBlob helper to http factory"
```

---

## Task 7: Frontend — userService methods

**Files:**
- Modify: `src/services/userService.ts`

- [ ] **Step 1: Add `bulkImport` and `downloadTemplate`**

Add these two entries to the `userService` object (after `setRoles`):

```typescript
    bulkImport:       (file: File, defaultPassword: string) => {
        const form = new FormData()
        form.append('file', file)
        form.append('defaultPassword', defaultPassword)
        return AuthHttp.postForm<BulkImportResult>(`${BASE}/bulk-import`, form)
    },
    downloadTemplate: ()                                    => AuthHttp.getBlob(`${BASE}/bulk-import/template`),
```

(`BulkImportResult` is an ambient global interface from `src/types/question.d.ts` — no import needed.)

- [ ] **Step 2: Type-check**

Run: `cd exam_hub_web; npx tsc -b`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_web/src/services/userService.ts
git commit -m "feat(web): add bulkImport and downloadTemplate to userService"
```

---

## Task 8: Frontend — UserBulkImportModal

**Files:**
- Create: `src/pages/user/UserBulkImportModal.tsx`

Mirrors `src/pages/questions/BulkImportModal.tsx` but with a default-password field and a template-download button; no react-query hook (calls `userService` directly, matching `UserPage`'s existing style).

- [ ] **Step 1: Create the modal**

```tsx
import {useState} from 'react'
import {Alert, Button, Input, Modal, Upload, message} from 'antd'
import type {UploadFile} from 'antd'
import {DownloadOutlined, InboxOutlined} from '@ant-design/icons'
import {userService} from '../../services/userService'

type Props = {
    open: boolean
    onClose: () => void
    onImported: () => void
}

export function UserBulkImportModal({open, onClose, onImported}: Props) {
    const [fileList, setFileList] = useState<UploadFile[]>([])
    const [password, setPassword] = useState('')
    const [result, setResult] = useState<BulkImportResult | null>(null)
    const [submitting, setSubmitting] = useState(false)

    const file = fileList[0]?.originFileObj as File | undefined
    const canSubmit = !!file && password.trim().length > 0

    const reset = () => {
        setFileList([])
        setPassword('')
        setResult(null)
    }

    const handleClose = () => {
        reset()
        onClose()
    }

    const handleDownloadTemplate = async () => {
        try {
            const blob = await userService.downloadTemplate()
            const url = URL.createObjectURL(blob)
            const a = document.createElement('a')
            a.href = url
            a.download = 'user-import-template.xlsx'
            a.click()
            URL.revokeObjectURL(url)
        } catch {
            message.error('Không thể tải file mẫu')
        }
    }

    const handleSubmit = async () => {
        if (!file || !password.trim()) return
        setSubmitting(true)
        try {
            const res = await userService.bulkImport(file, password.trim())
            if (res.data) {
                setResult(res.data)
                if (res.data.successCount > 0) onImported()
            } else {
                message.error(res.message || 'Import thất bại')
            }
        } catch {
            message.error('Có lỗi xảy ra khi import')
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <Modal
            title="Nhập người dùng từ Excel (.xlsx)"
            open={open}
            onCancel={handleClose}
            width={560}
            footer={[
                <Button key="cancel" onClick={handleClose}>Đóng</Button>,
                <Button
                    key="submit"
                    type="primary"
                    disabled={!canSubmit}
                    loading={submitting}
                    onClick={handleSubmit}
                >
                    Bắt đầu import
                </Button>,
            ]}
        >
            <div className="flex flex-col gap-3 mt-4">
                <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-500">
                        Cột: UserName, DisplayName, Email, PhoneNumber, Sex, Role
                    </span>
                    <Button size="small" icon={<DownloadOutlined/>} onClick={handleDownloadTemplate}>
                        Tải file mẫu
                    </Button>
                </div>

                <div>
                    <label className="form-label">Mật khẩu mặc định (áp cho mọi tài khoản)</label>
                    <Input.Password
                        placeholder="Nhập mật khẩu mặc định"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                    />
                </div>

                <Upload.Dragger
                    accept=".xlsx"
                    maxCount={1}
                    fileList={fileList}
                    beforeUpload={() => false}
                    onChange={({fileList: fl}) => setFileList(fl.slice(-1))}
                >
                    <p className="ant-upload-drag-icon"><InboxOutlined/></p>
                    <p className="ant-upload-text">Kéo thả hoặc bấm để chọn file .xlsx</p>
                </Upload.Dragger>

                {result && (
                    <Alert
                        type={result.errorCount > 0 ? 'warning' : 'success'}
                        showIcon
                        message={`Đã tạo ${result.successCount} tài khoản, ${result.errorCount} lỗi`}
                        description={result.errors.length > 0 && (
                            <ul className="list-disc pl-4 max-h-40 overflow-auto">
                                {result.errors.map((e, i) => (
                                    <li key={i}>Dòng {e.rowNumber}: {e.message}</li>
                                ))}
                            </ul>
                        )}
                    />
                )}
            </div>
        </Modal>
    )
}
```

- [ ] **Step 2: Type-check**

Run: `cd exam_hub_web; npx tsc -b`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add exam_hub_web/src/pages/user/UserBulkImportModal.tsx
git commit -m "feat(web): add UserBulkImportModal"
```

---

## Task 9: Frontend — wire into UserPage

**Files:**
- Modify: `src/pages/user/UserPage.tsx`

- [ ] **Step 1: Import the modal and an icon**

Add to the antd icons import (line 4) the `UploadOutlined` icon, and import the modal near the other modal imports (after line 9):

```tsx
import {DeleteOutlined, EditOutlined, KeyOutlined, PlusOutlined, SearchOutlined, TeamOutlined, UploadOutlined} from '@ant-design/icons'
```

```tsx
import {UserBulkImportModal} from './UserBulkImportModal'
```

- [ ] **Step 2: Add import-modal state**

After the `lockingId` state (line 22), add:

```tsx
    const [importOpen, setImportOpen] = useState(false)
```

- [ ] **Step 3: Add the "Nhập từ Excel" button**

Replace the header action block (the `<Button type="primary" ...>Thêm người dùng</Button>` at lines 192-194) with a two-button group:

```tsx
                <div className="flex items-center gap-2">
                    <Button icon={<UploadOutlined/>} onClick={() => setImportOpen(true)}>
                        Nhập từ Excel
                    </Button>
                    <Button type="primary" icon={<PlusOutlined/>} onClick={() => setModal({type: 'form', record: null})}>
                        Thêm người dùng
                    </Button>
                </div>
```

- [ ] **Step 4: Render the modal**

Add just before the closing `</div>` of the page (after the `<RolesModal .../>` block, ~line 231):

```tsx
            <UserBulkImportModal
                open={importOpen}
                onClose={() => setImportOpen(false)}
                onImported={fetchData}
            />
```

- [ ] **Step 5: Type-check and lint**

Run: `cd exam_hub_web; npx tsc -b; npm run lint`
Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add exam_hub_web/src/pages/user/UserPage.tsx
git commit -m "feat(web): add bulk import button and modal to UserPage"
```

---

## Task 10: End-to-end manual verification

**Files:** none (verification only)

- [ ] **Step 1: Build both sides**

Run: `dotnet build exam_hub_api/ExamHub.API/ExamHub.API.csproj`
Run: `cd exam_hub_web; npm run build`
Expected: both succeed.

- [ ] **Step 2: Run the app and log in as an Admin**

Start the API and the web dev server (`npm run dev`). Navigate to `app/users`.

- [ ] **Step 3: Download the template**

Click **Nhập từ Excel → Tải file mẫu**. Expected: `user-import-template.xlsx` downloads with header row `UserName, DisplayName, Email, PhoneNumber, Sex, Role` and one example row.

- [ ] **Step 4: Import a mixed file**

Fill the template with ~4 rows: 2 valid (one with `Role=Teacher`, one blank role), 1 with a blank `UserName`, 1 duplicating an existing username. Enter a default password, upload, click **Bắt đầu import**.

Expected result alert: `Đã tạo 2 tài khoản, 2 lỗi`, with row errors listing the blank-UserName row and the duplicate row. The user table refreshes and shows the 2 new accounts with correct roles.

- [ ] **Step 5: Verify login with default password**

Log out, log in as one imported user with the default password. Expected: login succeeds.

- [ ] **Step 6: Final commit (if any verification tweaks were needed)**

```bash
git add -A
git commit -m "chore: bulk user import verification tweaks"
```

(Skip if nothing changed.)

---

## Self-Review Notes

- **Spec coverage:** Excel columns (Task 3 `ParseRow`/`Headers`), default password (Task 1 DTO + Task 3), Role column with multi-role + validation (Task 3 `ParseRoles`), template endpoint (Tasks 3+5), partial import with row errors (Task 3), Admin-only (controller already `[Authorize(Roles="Admin")]`), frontend modal + button + template download (Tasks 6–9), verification (Task 10). All spec sections mapped.
- **Type consistency:** `BulkUserImportRequest`/`BulkUserImportResponse` used identically across Tasks 1/2/3/5; frontend reuses ambient `BulkImportResult`/`BulkImportRowError`; `getBlob` defined in Task 6 and consumed in Task 7.
- **Deviation from TDD:** documented at top — no test project exists; verification is compiler + manual E2E, matching the existing question importer.
