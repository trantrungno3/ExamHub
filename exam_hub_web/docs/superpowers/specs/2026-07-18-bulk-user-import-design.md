# Bulk User Import via Excel — Design Spec

**Date:** 2026-07-18
**Status:** Approved
**Scope:** Full-stack (ExamHub.API + exam_hub_web)

## 1. Goal

Add a bulk user import feature to the `app/users` screen. An Admin uploads an
`.xlsx` file to create many user accounts at once. Each row is processed
independently: valid rows are saved, invalid rows are reported (partial import).

This mirrors the existing question bulk-import feature end-to-end
(`BulkImportModal.tsx` on the frontend, `BulkImportService` + ClosedXML on the
backend).

## 2. User Flow

1. On `app/users`, admin clicks **"Nhập từ Excel"** (next to "Thêm người dùng").
2. Modal opens. Admin:
   - Enters a **default password** (applied to every created account).
   - Optionally clicks **"Tải file mẫu"** to download the template.
   - Uploads a single `.xlsx` via drag-and-drop.
3. Admin clicks **"Bắt đầu import"**.
4. Result shows: `Đã tạo N tài khoản, M lỗi`, with a scrollable list of row errors
   (`Dòng {rowNumber}: {message}`).
5. On completion the user table refreshes.

## 3. Excel Format

Row 1 is the header; data starts at row 2. Fully-empty rows are skipped.

| Col | Field | Required | Notes |
|-----|-------------|----------|-------|
| 1 | UserName | ✅ | Login name; must be unique (in file and DB) |
| 2 | DisplayName | ✅ | |
| 3 | Email | — | |
| 4 | PhoneNumber | — | |
| 5 | Sex | — | `Nam`/`Nữ` or `0`/`1`; blank → Nam (false) |
| 6 | Role | — | `Admin`/`Teacher`/`Student`; blank → no role. Multiple allowed, comma/semicolon separated (e.g. `Teacher,Admin`). Unknown role names → row error |

Password is **not** in the file — the single default password chosen in the modal
is applied to every created account. Users change it later via the existing
reset-password flow.

## 4. Backend (ExamHub.API / ExamHub.Core)

### 4.1 DTOs (`ExamHub.Core/DataTransferObjects/User/`)

- `BulkUserImportRequest`
  - `IFormFile File`
  - `string DefaultPassword`
- `BulkUserImportResponse(int SuccessCount, int ErrorCount, IReadOnlyList<BulkImportRowError> Errors)`
- Reuse the existing `BulkImportRowError(int RowNumber, string Message)` shape.
  If it is currently nested under the question DTOs, promote it to a shared
  location (e.g. a common `BulkImportRowError` record) so both importers share it.
  Do **not** couple to question-specific DTOs.

### 4.2 Service (`ExamHub.Core/.../Implementations/UserBulkImportService.cs`)

New dedicated service — **not** a generalization of the question
`IBulkImportService` (that one is tightly coupled to question DTOs).

- `IUserBulkImportService.ImportAsync(BulkUserImportRequest request, string createdBy, CancellationToken ct)`
- Uses **ClosedXML** (`XLWorkbook`), same helpers as `BulkImportService`
  (`Cell`, `RowIsEmpty`, `TryInt`, private `BulkImportRowException`).
- Per-row processing:
  1. Skip fully-empty rows.
  2. Validate `UserName` non-empty → else row error.
  3. Parse `Sex` (`Nam`/`Nữ`/`0`/`1`), `Role` list (validate against
     `Admin`/`Teacher`/`Student`; unknown → row error).
  4. Track UserNames already seen in this file → duplicate-in-file → row error.
  5. Check `CheckUserNameExistAsync` → duplicate-in-DB → row error.
  6. Build `CreateUserRequest` (UserName, DefaultPassword, DisplayName, Email,
     PhoneNumber, Sex) → `userService.CreateAsync(request)`.
  7. If roles present → `userService.SetRolesAsync(created.Id, roles)`.
  8. Increment success; on `BulkImportRowException` collect the error.
- Returns `BulkUserImportResponse(success, errors.Count, errors)`.
- Reuses the existing `IUserManagementService` (`CreateAsync`,
  `CheckUserNameExistAsync`, `SetRolesAsync`).

### 4.3 Template generation

- `IUserBulkImportService.BuildTemplate()` returns a `byte[]` of a ClosedXML
  workbook with the header row (UserName, DisplayName, Email, PhoneNumber, Sex,
  Role) and one example data row.
- Single source of truth for the column format — no committed binary, no new
  frontend dependency.

### 4.4 Controller (`UserController.cs`)

Controller is already `[Authorize(Roles = "Admin")]`.

- `POST api/users/bulk-import` — `[FromForm] BulkUserImportRequest`.
  - `400` if `File` null/empty or `DefaultPassword` blank.
  - Returns `RequestResponse<BulkUserImportResponse>` with success/error counts,
    matching the question `bulk-import` response envelope.
- `GET api/users/bulk-import/template` — streams the template `.xlsx`
  (`File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "user-import-template.xlsx")`).

### 4.5 DI registration

Register `IUserBulkImportService → UserBulkImportService` alongside the existing
`IBulkImportService` registration.

## 5. Frontend (exam_hub_web)

### 5.1 Types (`src/types/user.d.ts`)

Reuse `BulkImportResult` / `BulkImportRowError` (currently declared in
`question.d.ts`). Since `.d.ts` interfaces are global/ambient in this project,
they are already visible from `user.d.ts` — do not redeclare. If a rename is
warranted for clarity, keep a single shared declaration.

### 5.2 Service (`src/services/userService.ts`)

```ts
bulkImport: (file: File, defaultPassword: string) => {
    const form = new FormData()
    form.append('file', file)
    form.append('defaultPassword', defaultPassword)
    return AuthHttp.postForm<BulkImportResult>(`${BASE}/bulk-import`, form)
},
downloadTemplateUrl: `${BASE}/bulk-import/template`,   // or a blob GET helper
```

Template download: use an authenticated GET returning a blob and trigger a
browser download (the endpoint is Admin-only, so a plain anchor href without the
auth header will 401 — fetch as blob via `AuthHttp` then `URL.createObjectURL`).

### 5.3 Modal (`src/pages/user/UserBulkImportModal.tsx`)

Mirrors `BulkImportModal.tsx`:
- `Input.Password` for default password (required; enables submit).
- **"Tải file mẫu"** button → downloads template blob.
- `Upload.Dragger accept=".xlsx" maxCount={1}` with `beforeUpload={() => false}`.
- Submit disabled until file + non-empty password.
- Result `Alert` (`type` = warning if `errorCount > 0` else success) listing
  row errors.
- On success, calls an `onImported` callback so the page refetches.

### 5.4 Page wiring (`src/pages/user/UserPage.tsx`)

- Add **"Nhập từ Excel"** button next to "Thêm người dùng".
- Add modal open state; render `<UserBulkImportModal>`.
- `onImported` → `fetchData()`.

## 6. Error Handling

- Row-level errors never abort the batch (partial import).
- Duplicate usernames (in-file and in-DB), unknown roles, missing required
  fields → collected as row errors with the 1-based Excel row number.
- File-level failures (not `.xlsx`, empty file, blank password) → `400` with a
  Vietnamese message.

## 7. Testing

- **Backend:** unit-test `UserBulkImportService.ImportAsync` with an in-memory
  `XLWorkbook`: happy path, missing UserName, duplicate username (file + DB),
  unknown role, empty rows skipped, Sex parsing variants. Mock
  `IUserManagementService`.
- **Frontend:** manual verification — open modal, download template, upload a
  sample file with a mix of valid/invalid rows, confirm counts + error list +
  table refresh.

## 8. Out of Scope (YAGNI)

- Per-row password column, auto-generated passwords.
- Update-existing-user-on-import (import only creates).
- CSV support (xlsx only, matching questions).
- Async/background processing for very large files.
