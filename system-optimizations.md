# xamHub — Đề xuất tối ưu hệ thống

> Đánh giá từ source hiện tại ngày **2026-09-16**. Tài liệu này chỉ đề xuất và sắp xếp ưu tiên, chưa thay đổi logic ứng dụng.

## 1. Tóm tắt ưu tiên


| Ưu tiên | Việc cần làm                                                | Lợi ích chính                              | Ước lượng |
| ------- | ----------------------------------------------------------- | ------------------------------------------ | --------- |
| P0      | Thu hồi và di chuyển toàn bộ secret khỏi Git                | Chặn lộ DB/JWT/storage credentials         | S–M       |
| P0      | Đồng bộ API refresh token giữa web và backend               | Khôi phục refresh session đang lỗi runtime | S         |
| P1      | Nâng/cập nhật dependency có CVE và sửa EF version mismatch  | Giảm rủi ro supply-chain/runtime           | M         |
| P1      | Làm atomic luồng bắt đầu/nộp/lưu bài                        | Tránh vượt lượt, tạo hai bài, mất đáp án   | M–L       |
| P1      | Siết CORS, rate limit, refresh token và route authorization | Giảm bề mặt tấn công                       | M         |
| P1      | Hoàn thiện CI và đưa lint về xanh                           | Ngăn regression trước merge                | M         |
| P1      | Chuẩn hóa một nguồn schema/migration                        | Tránh môi trường có schema khác nhau       | M         |
| P2      | Code-split frontend và bỏ devtools khỏi production          | Giảm bundle 2,7 MB, tải trang nhanh hơn    | M         |
| P2      | Tối ưu query/index/pagination                               | Ổn định khi dữ liệu tăng                   | M         |
| P2      | Thêm integration/E2E/contract tests                         | Bắt lỗi xuyên tầng                         | M–L       |
| P2      | Hoàn thiện Docker/health/observability                      | Deploy và vận hành tin cậy hơn             | M         |


`S`: dưới 1 ngày, `M`: 1–3 ngày, `L`: trên 3 ngày; chỉ là ước lượng tương đối.

## 2. P0 — cần xử lý ngay

### 2.1 Thu hồi secret đã commit

**Bằng chứng**

- `ExamHub.API/appsettings.json` và `appsettings.Development.json` chứa connection string, JWT signing secrets, Salt, MinIO/Kafka credentials.
- `compose.yaml` chứa mật khẩu mặc định.
- `credentials.json` và `credentials1.json` đang được Git track.

**Rủi ro**

Secret đã từng commit phải được coi là đã lộ, kể cả khi repository là private hoặc chỉ đổi file ở commit mới. JWT key lộ cho phép giả mạo token; credential DB/MinIO có thể làm lộ hoặc sửa dữ liệu.

**Khuyến nghị**

1. Rotate toàn bộ DB, JWT access/refresh, MinIO, Mongo, RabbitMQ/Kafka credentials.
2. Xóa secret khỏi lịch sử Git bằng `git filter-repo` hoặc BFG, sau đó force-push có phối hợp.
3. Commit `appsettings.example.json` chỉ có placeholder; local dùng .NET User Secrets hoặc `.env` không track.
4. Production dùng secret manager của nền tảng triển khai; inject qua environment variables.
5. Bật secret scanning trong CI và pre-commit (`gitleaks`/`trufflehog`).

**Tiêu chí hoàn tất**: scan toàn repository và history không còn secret thật; credential cũ không còn đăng nhập được.

### 2.2 Sửa hợp đồng refresh token không khớp

**Bằng chứng**

- Backend khai báo `GET /api/Auth/refresh-token`, nhận cả access token và refresh token trong `TokenModel` từ query string, rồi chỉ trả access token mới dạng `string`.
- Frontend gọi `POST /api/Auth/refresh` với body chỉ có `{ refreshToken }`, nhưng lại parse response như một `TokenModel` gồm cả access/refresh token.
- `AppLayout` chủ động gọi `refresh()` khi access token hết hạn hoặc còn dưới 5 phút.

**Tác động**

Build vẫn pass vì đây là mismatch HTTP runtime, nhưng refresh sẽ nhận 404/405; người dùng bị logout khi access token hết hạn. Nếu dùng query string theo backend hiện tại, token còn có thể lọt vào access log, proxy history hoặc telemetry URL.

**Khuyến nghị**

- Chuẩn hóa thành `POST /api/Auth/refresh-token` với body DTO rõ ràng, gửi đủ dữ liệu mà backend thực sự cần.
- Trả cùng shape `TokenModel` mà `authStore.setTokens` đang dùng; xác định rõ có rotate refresh token hay giữ token hiện tại.
- Thêm API contract/integration test và test frontend mock server cho luồng access-token expiry.
- Dài hạn: lưu refresh token trong cookie `HttpOnly`, `Secure`, `SameSite`; chỉ giữ access token ngắn hạn trong memory.

**Tiêu chí hoàn tất**: test tự động chứng minh token hết hạn được refresh một lần, request được retry và không logout ngoài ý muốn.

## 3. P1 — độ tin cậy và an toàn

### 3.1 Cập nhật dependency và khóa chất lượng build

`dotnet test --no-restore` hiện pass 82/82 nhưng báo:

- `EFCore.NamingConventions 9.0.0` yêu cầu EF Core `< 10`, trong khi dự án resolve EF Core `10.0.4`.
- Các dependency transitively resolve `Microsoft.OpenApi 2.3.0`, `Snappier 1.0.0` có cảnh báo high severity và `SharpCompress 0.30.1` có cảnh báo moderate.

**Khuyến nghị**

1. Dùng `dotnet list package --vulnerable --include-transitive` cho cả ExamHub và `trungtv_core`.
2. Nâng/thay package ở nguồn phụ thuộc gốc; không chỉ suppress NU190x.
3. Căn `EFCore.NamingConventions` với EF Core 10 hoặc bỏ package nếu PostgreSQL naming đã được map đầy đủ.
4. Biến NU1608/NU1903 thành lỗi CI sau khi cleanup.
5. Bổ sung Dependabot/Renovate và lịch kiểm tra dependency.

### 3.2 Bảo đảm atomicity và concurrency cho lượt thi

**Bằng chứng**

- `ExamSessionService.StartAsync` thực hiện chuỗi read `GetInProgress → CountSubmittedAttempts → CreateSubmission` nhưng không transaction/lock.
- Schema chỉ có index `(session_id, student_id)`, chưa có unique constraint cho một bài `InProgress` hoặc cho `attempt_no`.
- Hai request đồng thời có thể cùng thấy chưa có bài dở/chưa hết lượt và cùng tạo submission.
- `ExamSubmissionService.SubmitAsync` thêm/cập nhật submission, chấm và ghi answers qua nhiều `SaveChangesAsync` riêng.
- `SubmissionAnswerRepository.ReplaceForSubmissionAsync` xóa rồi insert nhưng không tự mở transaction; lỗi giữa hai bước có thể để bài làm trống.

**Khuyến nghị**

- Đưa start/submit/autosave vào transaction ngắn.
- Thêm partial unique index cho một `InProgress` trên `(session_id, student_id)` và unique `(session_id, student_id, attempt_no)` khi `session_id IS NOT NULL`.
- Dùng optimistic concurrency token hoặc `SELECT ... FOR UPDATE`/serializable transaction khi cấp lượt.
- Với autosave, dùng upsert theo `(submission_id, exam_question_id)` thay cho delete-all/insert-all.
- Thêm test cạnh tranh chạy hai request đồng thời và test rollback khi insert answer lỗi.

### 3.3 Siết authentication, authorization và bề mặt HTTP

**Bằng chứng**

- CORS hiện `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`.
- JWT access/refresh token được persist trong `localStorage`, nên XSS có thể đọc cả hai.
- Các route student full-screen (`/student/exam`, `/take`, `/result`) không nằm trong `ProtectedRoute`/`StudentLayout`.
- `StudentLayout` chỉ kiểm tra đã đăng nhập, không kiểm tra role Student.
- API fallback bắt đăng nhập là nền tảng tốt, nhưng các endpoint cần tiếp tục kiểm tra ownership theo school/subject/class/submission.

**Khuyến nghị**

1. CORS allowlist theo environment; production chỉ cho origin web chính thức.
2. Chuyển refresh token sang secure HttpOnly cookie, rotation mỗi lần refresh, revoke khi logout/đổi mật khẩu.
3. Thêm rate limiting cho login, register, refresh, upload, bulk import và exam start/submit.
4. Bọc toàn bộ student route trong guard `allowedRoles=['Student']`; admin/teacher route cũng khai báo role ở router để UX nhất quán.
5. Rà từng endpoint mutate/read nhạy cảm bằng ma trận `role × ownership`; thêm test 401/403 và cross-tenant access.
6. Thêm security headers (CSP, HSTS production, `X-Content-Type-Options`, frame policy) tại reverse proxy/API.

### 3.4 Đưa lint và CI về trạng thái bắt buộc

Hiện trạng kiểm chứng:

- Backend tests: pass 82/82.
- Frontend build: pass.
- Frontend lint: fail 7 error, 2 warning (`AuthProvider`, router exports, effect state updates, hook dependencies và callback trong `UserPage`).
- Không thấy workflow CI ở root.

**Khuyến nghị**

- Sửa toàn bộ lint hiện tại trước, không hạ rule để “làm xanh”.
- CI tối thiểu trên mỗi PR:
  1. secret scan;
  2. `dotnet restore/build/test`;
  3. NuGet vulnerability audit;
  4. `pnpm install --frozen-lockfile`;
  5. `pnpm lint && pnpm build`;
  6. dependency/license audit.
- Cache NuGet/pnpm nhưng không cache output build không kiểm soát.
- Bảo vệ branch: chỉ merge khi pipeline pass.

### 3.5 Chọn một nguồn sự thật cho schema

`database_schema.sql` được Docker dùng để init DB, trong khi app có EF migration. Hai cơ chế cùng tồn tại dễ tạo DB local/test/prod khác nhau.

**Khuyến nghị**

- Chọn EF migrations làm nguồn chuẩn; compose chỉ tạo database rồi chạy migration bằng init job/API startup có kiểm soát.
- Nếu bắt buộc giữ SQL snapshot, generate nó từ migrations trong CI và fail khi diff.
- Thêm migration smoke test từ database trống và upgrade test từ release gần nhất.
- Không chỉnh trực tiếp production schema ngoài migration có review.

## 4. P2 — hiệu năng và khả năng mở rộng

### 4.1 Code-split frontend

Build production hiện tạo một JS chunk khoảng **2,702 kB** (gzip **815 kB**) và Vite cảnh báo vượt 500 kB. `routes/index.tsx` import eager tất cả page; React Query Devtools cũng luôn được import/render.

**Khuyến nghị**

- Dùng `React.lazy`/route lazy cho từng nhóm auth, admin, exams, school và student.
- Chỉ load `ReactQueryDevtools` trong development qua dynamic import.
- Tách vendor chunks hợp lý cho Ant Design, TipTap/KaTeX và Recharts sau khi đo bundle analyzer.
- Import icon/component theo nhu cầu; tránh barrel import làm kéo cả package.

**Mục tiêu ban đầu**: entry JS gzip dưới 250–350 kB; chức năng nặng chỉ tải khi vào route tương ứng.

### 4.2 Tối ưu search câu hỏi để dùng index

Schema đã tạo GIN full-text index trên `to_tsvector('simple', content_plain)`, nhưng `QuestionRepository.GetPagedAsync` đang tìm bằng `ILIKE '%keyword%'` trên `Content`/`ContentPlain`. Dạng wildcard đầu chuỗi thường không dùng B-tree và không tận dụng GIN full-text hiện có.

**Khuyến nghị**

- Chuyển query sang `to_tsvector(...) @@ plainto_tsquery(...)` và rank kết quả; thống nhất text-search configuration phù hợp tiếng Việt.
- Nếu cần substring chính xác, bật `pg_trgm` và tạo GIN trigram index trên `content_plain`.
- Dùng `EXPLAIN (ANALYZE, BUFFERS)` với dữ liệu gần production trước/sau thay đổi.

### 4.3 Tối ưu EF query cho kỳ thi và danh sách lớn

`ExamSessionRepository` có query read-only không `AsNoTracking`, đồng thời `Include` nhiều collection (`Exams`, `Assignments`) trước khi count/page. Điều này tăng tracking overhead và có thể tạo kết quả join phình to.

**Khuyến nghị**

- Dùng `AsNoTracking()` cho toàn bộ query chỉ đọc.
- Count trên query không Include; page ID trước rồi projection DTO cần thiết.
- Dùng `AsSplitQuery()` khi thật sự cần nhiều collection include.
- Tránh load full entities khi response chỉ cần count/name/status.
- Theo dõi slow query và bổ sung composite index theo filter/sort thực tế.

### 4.4 Chuẩn hóa pagination và giới hạn đầu vào

Một số repository trả toàn bộ danh sách (`GetBySubject`, submissions theo student/session, category `GetAll`). Các endpoint phân trang nhận `pageSize` raw, chưa thấy clamp tối đa. Frontend còn lấy cố định 100 đề để lọc client-side tại `StudentExamListPage` và `ExamSessionEditPage`, nên dữ liệu thứ 101 trở đi bị bỏ qua.

**Khuyến nghị**

- Tạo `PagedRequest` chung: `page >= 1`, `1 <= pageSize <= 100`.
- Phân trang ở server cho submissions/exams/users; lookup nhỏ mới được `GetAll`.
- Tạo endpoint student aggregate/paged thay vì tải 100 exams rồi join submissions trên client.
- Dùng cursor pagination cho lịch sử lớn nếu offset bắt đầu chậm.

### 4.5 Tối ưu pool câu hỏi theo quy mô

Thiết kế hiện tại cache toàn bộ ID pool 2 phút rồi copy, Fisher–Yates toàn bộ danh sách và lấy `count`. Cách này tốt khi pool nhỏ/vừa và tránh `ORDER BY random()` trong DB, nhưng CPU/memory tăng tuyến tính theo pool.

**Khuyến nghị khi pool lớn**

- Đo p95 kích thước pool và thời gian shuffle trước khi đổi.
- Chỉ shuffle partial đến `count` phần tử hoặc dùng sampling không thay thế.
- Giới hạn kích thước cache item; thêm hit/miss/eviction metrics.
- Chống cache stampede bằng single-flight/distributed lock ngắn.
- Giữ invalidation theo cả topic và subject như `QuestionPoolCache.KeysForQuestion` hiện tại.

## 5. P2 — maintainability, test và vận hành

### 5.1 Bổ sung test xuyên tầng

Backend có test nghiệp vụ tốt cho auth claims, generator, session, grading và delete guard, nhưng chủ yếu là unit/service test. Frontend chưa có test script.

**Khuyến nghị**

- Backend integration tests với Testcontainers PostgreSQL/Redis/MinIO cho migration, repository, auth 401/403, upload và transaction.
- Contract tests cho request/response quan trọng, đặc biệt auth refresh, enum casing và timestamp.
- Frontend: Vitest + Testing Library cho auth store, request errors, form và query invalidation.
- E2E Playwright cho 4 hành trình: login; tạo/sinh/giao đề; học sinh làm-nộp; giáo viên chấm-finalize.
- Thêm regression test cho concurrent exam start và autosave/submit race.

### 5.2 Chuẩn hóa dependency và package manager

- Backend phụ thuộc path tương đối ra sibling `trungtv_core`, nên repository không build độc lập.
- `global.json` cho phép `rollForward: latestMajor`, có thể làm môi trường dùng SDK major khác dự kiến.
- Frontend commit đồng thời `package-lock.json` và `pnpm-lock.yaml`.

**Khuyến nghị**

- Publish TVT Core thành private NuGet với version cố định, hoặc dùng Git submodule/monorepo có CI rõ ràng.
- Chọn pnpm và xóa `package-lock.json`, hoặc ngược lại; CI dùng frozen lockfile.
- Siết SDK roll-forward theo chính sách release của đội.
- Xóa package ASP.NET 2.3 cũ nếu không còn được dùng trực tiếp; rà dependency transitively kéo version cũ.

### 5.3 Hoàn thiện Docker/deployment

Compose hiện chỉ thực sự chạy dependency; API/frontend/nginx bị comment. PostgreSQL không mount `pgdata`, MinIO storage cũng không persistent; một số image dùng tag `latest`; credential mặc định nằm trong YAML.

**Khuyến nghị**

- Tạo `compose.dev.yaml` chạy đầy đủ API + web + dependency bằng env file mẫu.
- Mount volume PostgreSQL/MinIO/Mongo/Redis phù hợp và kiểm tra backup/restore.
- Pin image theo version/digest, thêm healthcheck cho mọi service và `depends_on` đúng điều kiện.
- Tách cấu hình dev/staging/prod; không expose port database ra ngoài ở production.
- Chạy container non-root, read-only filesystem nơi phù hợp và scan image trong CI.

### 5.4 Observability và vận hành

Hiện có logging/middleware từ TVT Core và config Mongo, nhưng source ExamHub chưa thể hiện rõ health, metric, distributed trace hoặc SLO.

**Khuyến nghị**

- Thêm `/health/live` và `/health/ready` kiểm tra PostgreSQL, Redis, MinIO; Rabbit/Mongo tùy dependency thực tế.
- OpenTelemetry cho HTTP, EF/Dapper, Redis và external storage; xuất trace/metric tới backend giám sát.
- Structured logs có correlation ID, user ID đã hash/ẩn và session/submission ID; tuyệt đối không log token/answer nhạy cảm.
- Metric nghiệp vụ: exam generation latency/failure, insufficient-question rate, autosave error, submit latency, grading queue, cache hit ratio.
- Đặt alert/SLO ban đầu cho p95 API, tỷ lệ 5xx và submit failures.

### 5.5 Làm rõ ranh giới module

`ExamHub.Core` hiện chứa Domain, DTO, EF/Dapper, cache và service implementations; `AppDbContext` là file cấu hình rất lớn. Cấu trúc vẫn dùng được ở quy mô hiện tại nhưng khó cô lập dependency/test khi tiếp tục mở rộng.

**Khuyến nghị theo nhu cầu, không refactor lớn ngay**

- Tách Fluent configuration thành `IEntityTypeConfiguration<T>` theo feature.
- Nhóm code theo vertical slice (`Questions`, `ExamGeneration`, `Sessions`, `Submissions`) thay vì chỉ theo loại kỹ thuật khi thêm chức năng mới.
- Giữ domain/application không phụ thuộc trực tiếp storage/framework; infrastructure implement contract.
- Chỉ tách project vật lý khi có lợi ích build/test/dependency rõ ràng.

## 6. Lộ trình đề xuất

### Giai đoạn 1 — 1 đến 2 ngày

- Rotate/remove secrets và bật secret scan.
- Sửa refresh contract, thêm test.
- Sửa lint hiện tại; thiết lập CI cơ bản.
- Audit và nâng dependency có CVE nghiêm trọng.

### Giai đoạn 2 — 3 đến 5 ngày

- Transaction + unique constraints cho start/submit/autosave.
- CORS/rate limit/cookie refresh/role guards.
- Chuẩn hóa migration và test DB từ trắng.

### Giai đoạn 3 — 1 sprint

- Route code-splitting, devtools development-only.
- Query projection/AsNoTracking, full-text search và pagination limits.
- Integration tests + 4 luồng E2E chính.
- Docker dev hoàn chỉnh, health checks và telemetry cơ bản.

## 7. Cách đo hiệu quả

Không đánh giá tối ưu chỉ bằng cảm giác; nên chốt baseline và đo lại:


| Mảng        | Chỉ số                                                                |
| ----------- | --------------------------------------------------------------------- |
| Security    | 0 secret trong scan; 0 high CVE; test 401/403 pass                    |
| Reliability | 0 duplicate attempt trong concurrency test; autosave rollback an toàn |
| Frontend    | entry gzip, LCP, route load time, số request khi mở trang             |
| API         | p50/p95/p99 latency, 5xx rate, DB query count và rows scanned         |
| Generator   | thời gian sinh 1/20 đề, cache hit ratio, insufficient pool rate       |
| Submission  | autosave/submit success rate, submit p95, grading backlog             |
| Delivery    | CI duration, lint/build/test pass rate, deployment rollback rate      |


## 8. Không nên làm ngay

- Không chuyển microservices khi chưa có bottleneck tổ chức/vận hành rõ ràng.
- Không thay PostgreSQL/Redis chỉ vì tối ưu giả định; hệ thống hiện đã có index và cache nền tảng hợp lý.
- Không refactor toàn bộ repository/service cùng lúc; ưu tiên các luồng P0/P1 có test.
- Không tăng `chunkSizeWarningLimit` để che cảnh báo bundle; phải code-split hoặc chứng minh kích thước chấp nhận được.
- Không suppress vulnerability/version warning nếu chưa xác định dependency gốc và bản vá.

