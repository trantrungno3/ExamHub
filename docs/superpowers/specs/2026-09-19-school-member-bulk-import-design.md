# School Member Bulk Add and Excel Import Design

## Intent

School administrators need to add several existing ExamHub accounts to a school without repeating the current one-user workflow. They must also be able to upload an Excel file, review which rows are valid, and import only the valid rows. The feature never creates user accounts.

Success means:

- admins can add multiple existing Admin, Teacher, or Student accounts manually;
- teachers become `SchoolMember` records;
- students become `CohortMember` records with a cohort and class section;
- Excel preview reports every valid and invalid row before any write;
- final import revalidates current data and writes valid rows even when other rows fail;
- school-member and student lists refresh after successful writes.

## Existing System to Reuse

- `UserBulkImportModal` supplies the existing upload, template-download, partial-success, and row-error UX.
- `UserBulkImportService` supplies the existing ClosedXML row-processing pattern.
- `BulkImportRowError` remains the shared row-error contract.
- `SchoolMemberService` owns Admin/Teacher membership writes.
- `CohortMemberService` owns Student membership writes and section normalization/validation.
- React Query keys in `useSchoolMembers.ts` and `useCohortMembers.ts` remain the refresh mechanism.

No database schema change, temporary upload storage, frontend Excel parser, or new frontend dependency is required.

## User Experience

The existing “Thêm thành viên” action opens a dedicated modal with two tabs.

### Select Users

1. The administrator selects `Admin`, `Teacher`, or `Student`.
2. The user selector supports multiple accounts and only offers accounts with the selected global role.
3. Accounts already represented in the target school are unavailable.
4. For `Student`, one cohort and one section are required and apply to every selected account.
5. For `Admin` and `Teacher`, cohort and section controls are hidden.
6. Submitting sends one bulk JSON request. The modal displays any per-user failures and refreshes affected lists when at least one row succeeds.

Manual Admin support preserves the existing capability. Excel deliberately excludes Admin to prevent accidental bulk grants of administrative access.

### Import Excel

1. The administrator downloads a generated template or selects an `.xlsx` file.
2. The frontend only verifies that a file exists, has an `.xlsx` name, is non-empty, and is no larger than 10 MB.
3. The frontend uploads `schoolId + file` to the preview endpoint.
4. The backend parses and validates the file without writing.
5. The modal shows summary counts and one row per spreadsheet row with status and error messages.
6. “Import dòng hợp lệ” is enabled only when the preview contains a valid row.
7. Confirmation uploads the same file again. The backend reparses and revalidates against current database state, then writes only rows still valid.
8. The modal shows the final success/error result using the same partial-import style as user import and refreshes both school members and students when at least one row succeeds.

The preview is read-only. Users correct the spreadsheet and upload it again rather than editing a second copy of the data in the browser.

## Excel Contract

The first worksheet contains these columns in this order:

| Column | Teacher | Student |
| --- | --- | --- |
| `UserName` | Required | Required |
| `Role` | `Teacher` | `Student` |
| `CohortName` | Empty | Required |
| `Section` | Empty | Required |

The generated template contains the header row and one Teacher and one Student example row. Parsing trims cell values. Role and username matching are case-insensitive. Section is normalized to uppercase. Cohort name matching is trimmed and case-insensitive within the selected school.

Empty rows are ignored. As in the current user importer, the first occurrence of a username is processed and later occurrences receive a “trùng trong file” row error.

## Backend Validation

### File-level validation

- file is present, non-empty, `.xlsx`, and at most 10 MB;
- a first worksheet exists;
- the four expected headers are present in the required order;
- malformed workbooks and invalid headers fail the request before row processing.

File-level failures return HTTP 400 through the existing error response shape.

### Shared row validation

- `UserName` and `Role` are required;
- the account exists and is not deleted;
- the account's global roles contain the requested role;
- later occurrences of the same username in the file/request fail as duplicates;
- an existing active or inactive membership counts as already present;
- preview performs no writes;
- import executes the same validation again immediately before each write.

### Teacher rows

- role must be `Teacher` for Excel; manual bulk also permits `Admin`;
- `CohortName` and `Section` must be empty for Excel Teacher rows;
- no `SchoolMember` may already exist for the same school and user;
- a valid row is written through `SchoolMemberService`.

### Student rows

- role must be `Student`;
- `CohortName` and `Section` are required;
- the cohort name resolves to exactly one active cohort in the selected school;
- ambiguous duplicate cohort names produce a row error rather than choosing one;
- section must be valid for that cohort using the existing section normalization/range rule;
- the student must not already have an active or inactive `CohortMember` in any cohort belonging to the selected school;
- the service never silently moves or reactivates a student;
- a valid row is written through `CohortMemberService`.

## API Design

All new endpoints require an authenticated global Admin. Upload endpoints use the existing `write-heavy` rate-limit policy.

### Template

`GET /api/schoolmember/bulk-import/template`

Returns `school-member-import-template.xlsx`.

### Preview

`POST /api/schoolmember/bulk-import/preview`

Multipart fields:

- `schoolId: int`
- `file: IFormFile`

Response data:

```text
SchoolMemberImportPreviewResponse
  validCount: int
  errorCount: int
  rows: SchoolMemberImportRowResult[]

SchoolMemberImportRowResult
  rowNumber: int
  userName: string
  role: string
  cohortName: string?
  section: string?
  isValid: bool
  errors: string[]
```

### Import

`POST /api/schoolmember/bulk-import`

Accepts the same multipart fields as preview. The response follows the existing bulk-user-import result shape:

```text
SchoolMemberBulkResult
  successCount: int
  errorCount: int
  errors: BulkImportRowError[]
```

Rows are independent. A row-level validation or persistence failure is added to `errors`; it does not roll back successful rows.

### Manual bulk add

`POST /api/schoolmember/bulk`

JSON request:

```text
SchoolMemberBulkAddRequest
  schoolId: int
  role: Admin | Teacher | Student
  userIds: Guid[]
  cohortId: int?      // required only for Student
  section: string?    // required only for Student
```

The response is `SchoolMemberBulkResult`, using the selected-user position as the row number for error reporting. It runs the same account, role, membership, cohort, and section rules as Excel import.

## Backend Components

A focused bulk membership service coordinates parsing and row validation. It depends on existing user, school-member, cohort-member, and cohort abstractions instead of duplicating their write behavior. Preview and import share one parse/validate path; import adds only the write step.

Repository queries are extended only where current contracts cannot answer the rules efficiently:

- resolve users by normalized username for a batch;
- find all school memberships regardless of active state;
- determine whether a student has any cohort membership in the selected school;
- resolve active cohorts by normalized name within one school.

The controller owns HTTP/file boundary checks and delegates row work to the service. ClosedXML remains server-side.

## Frontend Components

- Extract the add flow from `SchoolDetailPage.tsx` into a school-member modal component.
- Extend `schoolMemberService.ts` with template, preview, import, and manual-bulk calls.
- Extend `useSchoolMembers.ts` with bulk mutations that invalidate both school-member and cohort-member school keys after any success.
- Add TypeScript request/result types alongside the existing school types.
- Reuse Ant Design `Upload.Dragger`, `Alert`, `Table`, `Tabs`, `Select`, and the context-aware `App.useApp()` message API.

The page remains responsible for opening the modal and providing `schoolId`, users, cohorts, and current memberships. The modal owns its form, selected file, preview, submission state, and reset-on-close behavior.

## Error Handling and State Changes

- Preview never mutates database state.
- Import does not trust preview results and always revalidates.
- A membership created after preview appears as a row error in the final import result.
- Unexpected persistence failures are converted to row errors so other valid rows continue.
- Cancellation stops remaining work without undoing rows already committed, matching the existing partial-import model.
- After any successful row, the frontend refreshes `schoolMembers/bySchool` and `cohortMembers/bySchool`.
- A completely invalid preview keeps the import button disabled.

## Testing Strategy

Backend unit tests cover:

- valid Teacher preview/import;
- valid Student preview/import with cohort-name and section normalization;
- unknown or deleted account;
- global role mismatch;
- duplicate username later in the same file;
- existing active and inactive SchoolMember;
- existing Student membership in another cohort of the same school;
- missing, unknown, inactive, or ambiguous cohort name;
- invalid section;
- Teacher row containing cohort/class data;
- mixed valid/invalid rows producing partial success;
- import revalidation catching a membership created after preview;
- malformed file and invalid headers;
- manual bulk Admin, Teacher, and Student routing.

Frontend tests cover the small file-validation helper: missing file, wrong extension, empty file, oversized file, and valid `.xlsx`. Existing frontend lint, test, and production build commands verify the component and types. The complete backend test suite verifies existing membership and token-claim behavior remains intact.

## Non-goals

- creating user accounts from this import;
- importing Admin through Excel;
- editing spreadsheet rows inside the preview modal;
- moving students between cohorts/classes;
- reactivating inactive memberships;
- persisting previews or uploaded files;
- changing database schema or existing single-add endpoints.

## Acceptance Criteria

- An admin can select multiple existing accounts and add them in one action.
- Student bulk-add requires and validates cohort/class assignment.
- The downloadable Excel template uses the agreed four columns.
- Uploading Excel shows row-level preview without database writes.
- Invalid rows show their spreadsheet line number and actionable Vietnamese error text.
- Confirming import revalidates and writes only valid rows.
- Existing membership, role, cohort, and section rules are enforced server-side.
- Successful Teacher and Student rows appear in their respective school-detail lists without a page reload.
- No user account, database table, temporary file, or frontend Excel dependency is created by this feature.
