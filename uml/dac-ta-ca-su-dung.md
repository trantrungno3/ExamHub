# Đặc tả các ca sử dụng (Use Case)

> Trích xuất từ tài liệu đồ án: *1621050490 Trần Văn Trung - Đồ án 12-08-2026.docx* (mục 2.2 – Xây dựng sơ đồ ca sử dụng).
> Hệ thống: Website tạo sinh đề thi tự động.

## Các tác nhân của hệ thống

Hệ thống có **3 tác nhân**:

- **Quản trị viên**: người quản trị hệ thống, chịu trách nhiệm vận hành cấu hình toàn bộ nền tảng. Quản lý và cấu hình (thêm, sửa, xoá) danh mục có trong hệ thống (lớp học, môn học, chủ đề, độ khó). Phân quyền cho giáo viên và học sinh.
- **Giáo viên**: người dùng thao tác với ngân hàng câu hỏi (thêm, sửa, xoá); tạo mẫu đề thi (cấu hình số câu, tỉ lệ độ khó theo từng phần); sinh đề thi tự động từ mẫu (một hoặc nhiều đề cùng lúc, không trùng câu hoặc đảo thứ tự); xem kết quả và chấm điểm tự luận cho học sinh.
- **Học sinh**: người dùng xem danh sách đề thi được giao, làm bài thi, nộp bài, nhận kết quả bài thi.

## Danh sách ca sử dụng

| # | Ca sử dụng | Actor chính |
|---|-----------|-------------|
| 1 | Đăng nhập | Quản trị viên, Giáo viên, Học sinh |
| 2 | Đăng xuất | Quản trị viên, Giáo viên, Học sinh |
| 3 | Thêm trường học | Quản trị viên |
| 4 | Chỉnh sửa trường học | Quản trị viên |
| 5 | Xoá trường học | Quản trị viên |
| 6 | Xem trường học | Quản trị viên |
| 7 | Tìm kiếm trường học | Quản trị viên |
| 8 | Thêm khoá học | Quản trị viên |
| 9 | Chỉnh sửa khoá học | Quản trị viên |
| 10 | Xoá khoá học | Quản trị viên |
| 11 | Xem khoá học và danh sách lớp | Quản trị viên |
| 12 | Tìm kiếm khoá học | Quản trị viên |
| 13 | Thêm học sinh vào lớp | Quản trị viên |
| 14 | Phân công giáo viên | Quản trị viên |
| 15 | Cập nhật giáo viên chủ nhiệm | Quản trị viên |
| 16 | Thêm cấu hình danh mục | Quản trị viên |
| 17 | Chỉnh sửa cấu hình danh mục | Quản trị viên |
| 18 | Xoá cấu hình danh mục | Quản trị viên |
| 19 | Xem cấu hình danh mục | Quản trị viên |
| 20 | Tìm kiếm cấu hình danh mục | Quản trị viên |
| 21 | Duyệt câu hỏi | Quản trị viên, Giáo viên |
| 22 | Tạo câu hỏi | Giáo viên |
| 23 | Chỉnh sửa câu hỏi | Giáo viên |
| 24 | Xoá câu hỏi | Giáo viên |
| 25 | Xem câu hỏi | Giáo viên |
| 26 | Tìm kiếm câu hỏi | Giáo viên |
| 27 | Import câu hỏi hàng loạt từ Excel | Giáo viên |
| 28 | Tạo mẫu đề thi | Giáo viên |
| 29 | Sinh đề thi từ mẫu | Giáo viên |
| 30 | Sinh nhiều mã đề | Giáo viên |
| 31 | Xuất và xem trước đề thi | Giáo viên |
| 32 | Giao kỳ thi cho lớp | Giáo viên |
| 33 | Học sinh làm bài thi | Học sinh |
| 34 | Chấm điểm | Học sinh, Giáo viên |
| 35 | Thêm cấu hình đề thi | Giáo viên |
| 36 | Chỉnh sửa cấu hình đề thi | Giáo viên |
| 37 | Xoá cấu hình đề thi | Giáo viên |
| 38 | Xem cấu hình đề thi | Giáo viên |
| 39 | Tìm kiếm cấu hình đề thi | Giáo viên |
| 40 | Cập nhật trạng thái đề thi | Giáo viên |
| 41 | Thêm người dùng | Quản trị viên |
| 42 | Chỉnh sửa người dùng | Quản trị viên |
| 43 | Xoá người dùng | Quản trị viên |
| 44 | Xem người dùng | Quản trị viên |
| 45 | Tìm kiếm người dùng | Quản trị viên |

---

## 1. Đăng nhập

- **Mô tả**: Cho phép Quản trị viên, Giáo viên, Học sinh đăng nhập vào hệ thống website tạo sinh đề thi tự động và sử dụng các chức năng được phân quyền.
- **Actor chính**: Quản trị viên, Giáo viên, Học sinh
- **Các Use Case bao gồm (include)**: Xác thực JWT; Kiểm tra phân quyền.
- **Điều kiện tiên quyết**:
  - Quản trị viên, Giáo viên, Học sinh đã có tài khoản trong hệ thống.
  - Hệ thống hoạt động bình thường.
- **Điều kiện sau**:
  - Đăng nhập thành công và chuyển đến trang tương ứng với quyền của tài khoản.
  - Vẫn ở trang đăng nhập nếu đăng nhập thất bại.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi người dùng truy cập vào trang đăng nhập của hệ thống.
2. Hệ thống hiển thị trang đăng nhập với mẫu gồm: Tên đăng nhập, Mật khẩu, Nút đăng nhập, Liên kết Quên mật khẩu, Liên kết Đăng ký ngay.
3. Người dùng điền thông tin đăng nhập vào ô tương ứng.
4. Người dùng bấm nút đăng nhập.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu đăng nhập (trường bắt buộc không để trống, định dạng hợp lệ).
6. Hệ thống gọi use case **Xác thực JWT** để kiểm tra thông tin đăng nhập.
7. Sau khi xác thực thành công, hệ thống gọi use case **Kiểm tra phân quyền**.
8. Hệ thống ghi nhận thời gian đăng nhập.
9. Hệ thống chuyển hướng người dùng đến trang tương ứng với quyền của mình.

**Luồng sự kiện thay thế:**
- *Bước 5 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi bên dưới ô nhập liệu; người dùng chỉnh sửa và quay lại bước 4.
- *Bước 6 – Sai tên đăng nhập hoặc mật khẩu:* Hệ thống hiển thị thông báo sai thông tin; người dùng vẫn ở trang đăng nhập và có thể thử lại.

---

## 2. Đăng xuất

- **Mô tả**: Cho phép Quản trị viên, Giáo viên, Học sinh đăng xuất khỏi hệ thống, huỷ phiên làm việc hiện tại.
- **Actor chính**: Quản trị viên, Giáo viên, Học sinh
- **Điều kiện tiên quyết**: Người dùng đã đăng nhập thành công vào hệ thống.
- **Điều kiện sau**:
  - Phiên làm việc (token JWT) bị huỷ.
  - Người dùng được chuyển về trang đăng nhập.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi người dùng bấm nút Đăng xuất trên giao diện.
2. Hệ thống xác nhận yêu cầu đăng xuất.
3. Hệ thống huỷ token JWT hiện tại của người dùng.
4. Hệ thống xoá thông tin phiên làm việc khỏi bộ nhớ.
5. Hệ thống chuyển hướng người dùng về trang đăng nhập.

---

## 3. Thêm trường học

- **Mô tả**: Cho phép Quản trị viên thêm mới thông tin trường học vào hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền quản lý trường học.
- **Điều kiện sau**: Trường học mới được thêm thành công vào hệ thống.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý trường và bấm Thêm mới.
2. Hệ thống hiển thị form nhập liệu: tên trường, mã trường, địa chỉ, liên hệ.
3. Quản trị viên điền thông tin trường học.
4. Quản trị viên bấm nút Lưu.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu nhập vào.
6. Hệ thống lưu thông tin trường học mới vào cơ sở dữ liệu.
7. Hệ thống hiển thị thông báo thêm thành công và cập nhật danh sách trường.

**Luồng sự kiện thay thế:**
- *Bước 5 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Quản trị viên chỉnh sửa và lưu lại.

---

## 4. Chỉnh sửa trường học

- **Mô tả**: Cho phép Quản trị viên chỉnh sửa thông tin trường học đã tồn tại trong hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; trường học cần sửa đã tồn tại.
- **Điều kiện sau**: Thông tin trường học được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý trường.
2. Hệ thống hiển thị danh sách trường học hiện có.
3. Quản trị viên chọn trường cần sửa và bấm nút Chỉnh sửa.
4. Hệ thống hiển thị form nhập liệu với dữ liệu hiện tại.
5. Quản trị viên cập nhật thông tin và bấm Lưu.
6. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
7. Hệ thống cập nhật thông tin trường học vào cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo cập nhật thành công.

**Luồng sự kiện thay thế:**
- *Bước 6 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Quản trị viên chỉnh sửa và lưu lại.

---

## 5. Xoá trường học

- **Mô tả**: Cho phép Quản trị viên xoá một trường học khỏi hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; trường học cần xoá đã tồn tại.
- **Điều kiện sau**: Trường học được xoá khỏi hệ thống.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý trường.
2. Hệ thống hiển thị danh sách trường học.
3. Quản trị viên chọn trường cần xoá và bấm nút Xoá.
4. Hệ thống hiển thị hộp thoại xác nhận xoá.
5. Quản trị viên xác nhận xoá.
6. Hệ thống kiểm tra trường học có đang được sử dụng (còn khoá học hoặc lớp học liên kết) hay không.
7. Hệ thống xoá trường học khỏi cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo xoá thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 6 – Trường học đang được sử dụng:* Hệ thống hiển thị cảnh báo trường đang có khoá học hoặc lớp học liên kết; Quản trị viên chọn huỷ thao tác hoặc xác nhận xoá bắt buộc.

---

## 6. Xem trường học

- **Mô tả**: Cho phép Quản trị viên xem thông tin chi tiết của một trường học trong hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; trường học cần xem đã tồn tại.
- **Điều kiện sau**: Thông tin chi tiết trường học được hiển thị đầy đủ.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý trường.
2. Hệ thống hiển thị danh sách trường học.
3. Quản trị viên chọn trường cần xem.
4. Hệ thống truy vấn thông tin chi tiết trường học từ cơ sở dữ liệu.
5. Hệ thống hiển thị đầy đủ thông tin: tên trường, mã trường, địa chỉ, liên hệ, số khoá học/lớp học liên quan.

---

## 7. Tìm kiếm trường học

- **Mô tả**: Cho phép Quản trị viên tìm kiếm trường học theo tên hoặc mã trường.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có ít nhất một trường học trong hệ thống.
- **Điều kiện sau**: Danh sách trường học phù hợp với tiêu chí tìm kiếm được hiển thị.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý trường.
2. Hệ thống hiển thị ô tìm kiếm với bộ lọc: tên trường, mã trường.
3. Quản trị viên nhập từ khoá và/hoặc chọn bộ lọc cần thiết.
4. Quản trị viên bấm nút tìm kiếm.
5. Hệ thống truy vấn cơ sở dữ liệu theo các tiêu chí đã chọn.
6. Hệ thống hiển thị danh sách trường học phù hợp.

**Luồng sự kiện thay thế:**
- *Bước 6 – Không tìm thấy kết quả:* Hệ thống hiển thị thông báo không tìm thấy; Quản trị viên điều chỉnh tiêu chí và thử lại.

---

## 8. Thêm khoá học

- **Mô tả**: Cho phép Quản trị viên tạo khoá học mới; hệ thống tự động sinh các lớp học theo cấu hình khoá học.
- **Actor chính**: Quản trị viên
- **Các Use Case bao gồm (include)**: Sinh lớp học tự động.
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; thông tin trường học đã được khởi tạo.
- **Điều kiện sau**:
  - Khoá học mới được tạo thành công.
  - Các lớp học được sinh tự động theo cấu hình khoá học.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý khoá học và bấm Tạo khoá học mới.
2. Hệ thống hiển thị form nhập liệu: tên khoá học, năm học, số lớp, ký hiệu lớp.
3. Quản trị viên điền đầy đủ thông tin khoá học và bấm Lưu.
4. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
5. Hệ thống lưu khoá học và gọi use case **Sinh lớp học tự động** để tạo các lớp theo cấu hình.
6. Hệ thống hiển thị thông báo tạo thành công kèm danh sách lớp vừa được sinh.

**Luồng sự kiện thay thế:**
- *Bước 4 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Quản trị viên chỉnh sửa và lưu lại.

---

## 9. Chỉnh sửa khoá học

- **Mô tả**: Cho phép Quản trị viên chỉnh sửa thông tin của một khoá học đã tồn tại.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; khoá học cần sửa đã tồn tại.
- **Điều kiện sau**: Thông tin khoá học được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý khoá học.
2. Hệ thống hiển thị danh sách khoá học hiện có.
3. Quản trị viên chọn khoá học cần sửa và bấm Chỉnh sửa.
4. Hệ thống hiển thị form với dữ liệu hiện tại.
5. Quản trị viên cập nhật thông tin (tên khoá học, năm học) và bấm Lưu.
6. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
7. Hệ thống cập nhật khoá học vào cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo cập nhật thành công.

**Luồng sự kiện thay thế:**
- *Bước 6 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Quản trị viên chỉnh sửa và lưu lại.

---

## 10. Xoá khoá học

- **Mô tả**: Cho phép Quản trị viên xoá một khoá học không còn sử dụng khỏi hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; khoá học cần xoá đã tồn tại.
- **Điều kiện sau**: Khoá học được xoá khỏi hệ thống.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý khoá học.
2. Hệ thống hiển thị danh sách khoá học.
3. Quản trị viên chọn khoá học cần xoá và bấm Xoá.
4. Hệ thống hiển thị hộp thoại xác nhận xoá.
5. Quản trị viên xác nhận xoá.
6. Hệ thống kiểm tra khoá học có lớp học đang hoạt động hay không.
7. Hệ thống xoá khoá học khỏi cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo xoá thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 6 – Khoá học còn lớp đang hoạt động:* Hệ thống hiển thị cảnh báo; Quản trị viên chọn huỷ thao tác hoặc xác nhận xoá bắt buộc.

---

## 11. Xem khoá học và danh sách lớp

- **Mô tả**: Cho phép Quản trị viên xem thông tin chi tiết khoá học và danh sách lớp học theo năm học.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; khoá học cần xem đã tồn tại.
- **Điều kiện sau**: Thông tin khoá học và danh sách lớp học được hiển thị đầy đủ.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý khoá học.
2. Hệ thống hiển thị danh sách khoá học.
3. Quản trị viên chọn khoá học cần xem.
4. Hệ thống truy vấn thông tin chi tiết khoá học và danh sách lớp học thuộc khoá học.
5. Hệ thống hiển thị đầy đủ thông tin khoá học kèm danh sách lớp theo năm học.

---

## 12. Tìm kiếm khoá học

- **Mô tả**: Cho phép Quản trị viên tìm kiếm khoá học theo tên hoặc năm học.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có ít nhất một khoá học trong hệ thống.
- **Điều kiện sau**: Danh sách khoá học phù hợp với tiêu chí tìm kiếm được hiển thị.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Quản lý khoá học.
2. Hệ thống hiển thị ô tìm kiếm với bộ lọc: tên khoá học, năm học.
3. Quản trị viên nhập từ khoá và/hoặc chọn bộ lọc cần thiết.
4. Quản trị viên bấm nút tìm kiếm.
5. Hệ thống truy vấn cơ sở dữ liệu theo các tiêu chí đã chọn.
6. Hệ thống hiển thị danh sách khoá học phù hợp.

**Luồng sự kiện thay thế:**
- *Bước 6 – Không tìm thấy kết quả:* Hệ thống hiển thị thông báo không tìm thấy; Quản trị viên điều chỉnh tiêu chí và thử lại.

---

## 13. Thêm học sinh vào lớp

- **Mô tả**: Cho phép Quản trị viên thêm học sinh vào một lớp học cụ thể.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; lớp học và tài khoản học sinh đã được khởi tạo.
- **Điều kiện sau**: Học sinh được thêm vào lớp học tương ứng.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang chi tiết lớp học và chọn Thêm học sinh.
2. Hệ thống hiển thị danh sách học sinh chưa thuộc lớp.
3. Quản trị viên chọn một hoặc nhiều học sinh từ danh sách và xác nhận.
4. Hệ thống kiểm tra tính hợp lệ (học sinh chưa thuộc lớp khác cùng khối).
5. Hệ thống lưu bản ghi vào cơ sở dữ liệu.
6. Hệ thống hiển thị thông báo thành công và cập nhật danh sách thành viên lớp.

**Luồng sự kiện thay thế:**
- *Bước 4 – Học sinh đã thuộc lớp khác:* Hệ thống hiển thị cảnh báo học sinh đã thuộc lớp khác cùng khối; Quản trị viên chọn bỏ qua học sinh đó hoặc huỷ thao tác.

---

## 14. Phân công giáo viên

- **Mô tả**: Cho phép Quản trị viên phân công giáo viên phụ trách môn học cho một lớp học.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; lớp học và tài khoản giáo viên đã được khởi tạo.
- **Điều kiện sau**: Giáo viên được phân công môn dạy cho lớp.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang chi tiết lớp học và chọn Phân công giáo viên.
2. Hệ thống hiển thị danh sách giáo viên và môn học.
3. Quản trị viên chọn giáo viên, môn học và xác nhận.
4. Hệ thống kiểm tra tính hợp lệ (không trùng phân công môn học đã có trong lớp).
5. Hệ thống lưu thông tin phân công vào cơ sở dữ liệu.
6. Hệ thống hiển thị thông báo thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 4 – Trùng phân công môn học:* Hệ thống hiển thị cảnh báo môn học đã được phân công cho giáo viên khác trong lớp; Quản trị viên chọn giáo viên khác hoặc huỷ thao tác.

---

## 15. Cập nhật giáo viên chủ nhiệm

- **Mô tả**: Cho phép Quản trị viên cập nhật giáo viên chủ nhiệm cho một lớp học.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; lớp học và tài khoản giáo viên đã được khởi tạo.
- **Điều kiện sau**: Giáo viên chủ nhiệm được cập nhật cho lớp.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang chi tiết lớp học và chọn Cập nhật GVCN.
2. Hệ thống hiển thị danh sách giáo viên có thể chọn làm giáo viên chủ nhiệm.
3. Quản trị viên chọn giáo viên chủ nhiệm mới và xác nhận.
4. Hệ thống kiểm tra tính hợp lệ.
5. Hệ thống cập nhật giáo viên chủ nhiệm cho lớp.
6. Hệ thống hiển thị thông báo cập nhật thành công.

---

## 16. Thêm cấu hình danh mục

- **Mô tả**: Cho phép Quản trị viên thêm mới một mục danh mục dùng chung trong hệ thống (môn học, chủ đề/chương, độ khó, loại câu hỏi, cấp độ Bloom).
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền cấu hình danh mục hệ thống.
- **Điều kiện sau**: Mục danh mục mới được thêm thành công và có thể sử dụng ngay trong các chức năng liên quan.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Cấu hình danh mục.
2. Hệ thống hiển thị các nhóm danh mục: Môn học, Chủ đề/chương, Độ khó, Loại câu hỏi, Cấp độ Bloom.
3. Quản trị viên chọn nhóm danh mục cần quản lý và bấm Thêm mới.
4. Hệ thống hiển thị form nhập liệu: tên mục, mô tả.
5. Quản trị viên điền thông tin và bấm Lưu.
6. Hệ thống kiểm tra tính hợp lệ và không trùng tên.
7. Hệ thống lưu mục danh mục mới vào cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo thêm thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 6 – Dữ liệu không hợp lệ hoặc trùng tên:* Hệ thống hiển thị thông báo lỗi; Quản trị viên chỉnh sửa và lưu lại.

---

## 17. Chỉnh sửa cấu hình danh mục

- **Mô tả**: Cho phép Quản trị viên chỉnh sửa thông tin một mục danh mục đã tồn tại.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền cấu hình danh mục hệ thống.
- **Điều kiện sau**: Thông tin mục danh mục được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Cấu hình danh mục.
2. Hệ thống hiển thị danh sách mục trong nhóm danh mục đã chọn.
3. Quản trị viên chọn mục cần sửa và bấm Chỉnh sửa.
4. Hệ thống hiển thị form với dữ liệu hiện tại.
5. Quản trị viên cập nhật thông tin và bấm Lưu.
6. Hệ thống kiểm tra tính hợp lệ và không trùng tên.
7. Hệ thống cập nhật mục danh mục vào cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo cập nhật thành công.

**Luồng sự kiện thay thế:**
- *Bước 6 – Dữ liệu không hợp lệ hoặc trùng tên:* Hệ thống hiển thị thông báo lỗi; Quản trị viên chỉnh sửa và lưu lại.

---

## 18. Xoá cấu hình danh mục

- **Mô tả**: Cho phép Quản trị viên xoá một mục danh mục không còn sử dụng khỏi hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền cấu hình danh mục hệ thống.
- **Điều kiện sau**: Mục danh mục được xoá khỏi hệ thống.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên chọn mục danh mục cần xoá.
2. Hệ thống hiển thị hộp thoại xác nhận xoá.
3. Quản trị viên xác nhận xoá.
4. Hệ thống kiểm tra mục danh mục có đang được sử dụng hay không.
5. Hệ thống xoá mục danh mục khỏi cơ sở dữ liệu.
6. Hệ thống hiển thị thông báo xoá thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 4 – Mục danh mục đang được sử dụng:* Hệ thống hiển thị cảnh báo mục đang được sử dụng, không thể xoá; Quản trị viên chọn huỷ thao tác.

---

## 19. Xem cấu hình danh mục

- **Mô tả**: Cho phép Quản trị viên xem thông tin chi tiết của một mục danh mục trong hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền cấu hình danh mục hệ thống.
- **Điều kiện sau**: Thông tin chi tiết mục danh mục được hiển thị đầy đủ.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Cấu hình danh mục.
2. Hệ thống hiển thị danh sách mục danh mục.
3. Quản trị viên chọn mục cần xem chi tiết.
4. Hệ thống truy vấn thông tin chi tiết mục từ cơ sở dữ liệu.
5. Hệ thống hiển thị đầy đủ thông tin: tên, mô tả, trạng thái, ngày tạo.

---

## 20. Tìm kiếm cấu hình danh mục

- **Mô tả**: Cho phép Quản trị viên tìm kiếm mục danh mục theo từ khoá hoặc trạng thái.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền cấu hình danh mục hệ thống.
- **Điều kiện sau**: Danh sách mục danh mục phù hợp với tiêu chí tìm kiếm được hiển thị.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào mục Cấu hình danh mục.
2. Hệ thống hiển thị ô tìm kiếm và bộ lọc.
3. Quản trị viên nhập từ khoá và chọn điều kiện lọc.
4. Quản trị viên bấm Tìm kiếm.
5. Hệ thống truy vấn danh mục theo tiêu chí đã chọn.
6. Hệ thống hiển thị danh sách mục phù hợp.

**Luồng sự kiện thay thế:**
- *Bước 6 – Không tìm thấy kết quả:* Hệ thống hiển thị thông báo không tìm thấy; Quản trị viên điều chỉnh từ khoá và tìm lại.

---

## 21. Duyệt câu hỏi

- **Mô tả**: Cho phép Quản trị viên hoặc Giáo viên có quyền xem xét, phê duyệt hoặc từ chối các câu hỏi đang ở trạng thái chờ duyệt trong ngân hàng câu hỏi.
- **Actor chính**: Quản trị viên, Giáo viên
- **Điều kiện tiên quyết**: Đã đăng nhập và có quyền duyệt câu hỏi; tồn tại ít nhất một câu hỏi ở trạng thái Chờ duyệt.
- **Điều kiện sau**:
  - Câu hỏi được phê duyệt chuyển sang trạng thái Đã duyệt và có thể dùng để sinh đề.
  - Câu hỏi bị từ chối chuyển sang trạng thái Từ chối kèm lý do.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên/Giáo viên truy cập vào mục Duyệt câu hỏi.
2. Hệ thống hiển thị danh sách câu hỏi đang chờ duyệt.
3. Người duyệt chọn một câu hỏi để xem xét.
4. Hệ thống hiển thị đầy đủ nội dung câu hỏi, đáp án, phân loại Bloom và độ khó.
5. Người duyệt xem xét và chọn hành động: Phê duyệt hoặc Từ chối.
6. Nếu từ chối, người duyệt nhập lý do từ chối.
7. Hệ thống cập nhật trạng thái câu hỏi tương ứng.
8. Hệ thống thông báo kết quả duyệt và cập nhật danh sách.

---

## 22. Tạo câu hỏi

- **Mô tả**: Cho phép Giáo viên tạo câu hỏi mới trong ngân hàng câu hỏi, bao gồm phân loại theo Bloom, độ khó và tuỳ chọn đính kèm hình ảnh.
- **Actor chính**: Giáo viên
- **Các Use Case bao gồm (include)**: Phân loại Bloom và độ khó.
- **Các Use Case mở rộng (extend)**: Upload ảnh câu hỏi.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; danh mục môn học, chủ đề, loại câu hỏi đã được Quản trị viên cấu hình.
- **Điều kiện sau**:
  - Câu hỏi mới được lưu ở trạng thái Chờ duyệt.
  - Câu hỏi sẵn sàng để được duyệt và đưa vào ngân hàng câu hỏi.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn Tạo câu hỏi mới trong ngân hàng câu hỏi.
2. Hệ thống hiển thị form nhập liệu câu hỏi.
3. Giáo viên chọn loại câu hỏi: trắc nghiệm một đáp án, trắc nghiệm nhiều đáp án, hoặc tự luận.
4. Giáo viên nhập nội dung câu hỏi và các đáp án tương ứng.
5. Giáo viên chọn môn học, chủ đề, chương liên quan.
6. Hệ thống thực hiện use case **Phân loại Bloom và độ khó**: Giáo viên chọn cấp độ Bloom và mức độ khó.
7. Nếu cần, Giáo viên thực hiện use case mở rộng **Upload ảnh câu hỏi** để đính kèm hình ảnh minh hoạ.
8. Giáo viên bấm nút lưu câu hỏi.
9. Hệ thống kiểm tra tính hợp lệ và lưu câu hỏi ở trạng thái Chờ duyệt.
10. Hệ thống hiển thị thông báo tạo thành công.

**Luồng sự kiện thay thế:**
- *Bước 9 – Dữ liệu câu hỏi không hợp lệ:* Hệ thống hiển thị thông báo lỗi (thiếu nội dung câu hỏi, đáp án đúng hoặc phân loại); Giáo viên bổ sung thông tin còn thiếu và lưu lại.

---

## 23. Chỉnh sửa câu hỏi

- **Mô tả**: Cho phép Giáo viên chỉnh sửa nội dung câu hỏi đã tạo trong ngân hàng câu hỏi.
- **Actor chính**: Giáo viên
- **Các Use Case mở rộng (extend)**: Upload ảnh câu hỏi.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; câu hỏi cần sửa đã tồn tại và thuộc quyền quản lý của Giáo viên.
- **Điều kiện sau**: Câu hỏi được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên tìm kiếm và chọn câu hỏi cần sửa.
2. Hệ thống hiển thị nội dung chi tiết câu hỏi.
3. Giáo viên bấm Sửa.
4. Hệ thống hiển thị form chỉnh sửa với dữ liệu hiện tại.
5. Giáo viên cập nhật nội dung, đáp án, phân loại.
6. Nếu cần, Giáo viên thực hiện use case mở rộng **Upload ảnh câu hỏi** để thay ảnh minh hoạ.
7. Giáo viên bấm Lưu.
8. Hệ thống kiểm tra tính hợp lệ và cập nhật câu hỏi vào cơ sở dữ liệu.
9. Hệ thống hiển thị thông báo cập nhật thành công.

**Luồng sự kiện thay thế:**
- *Bước 8 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Giáo viên chỉnh sửa và lưu lại.

---

## 24. Xoá câu hỏi

- **Mô tả**: Cho phép Giáo viên xoá câu hỏi đã tạo khỏi ngân hàng câu hỏi.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; câu hỏi cần xoá đã tồn tại và thuộc quyền quản lý của Giáo viên.
- **Điều kiện sau**:
  - Câu hỏi được xoá khỏi ngân hàng câu hỏi thành công.
  - Nếu câu hỏi đã được dùng trong đề thi, hệ thống cảnh báo trước khi xoá.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên tìm kiếm và chọn câu hỏi cần xoá.
2. Hệ thống hiển thị nội dung chi tiết câu hỏi.
3. Giáo viên bấm Xoá.
4. Hệ thống hiển thị hộp thoại xác nhận xoá.
5. Giáo viên xác nhận xoá.
6. Hệ thống kiểm tra câu hỏi có đang dùng trong đề thi hay không.
7. Hệ thống xoá câu hỏi khỏi ngân hàng câu hỏi và ghi nhận vào cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo xoá thành công.

**Luồng sự kiện thay thế:**
- *Bước 6 – Câu hỏi đang được sử dụng trong đề thi:* Hệ thống cảnh báo câu hỏi đang được dùng trong đề thi đã giao; Giáo viên chọn huỷ thao tác xoá hoặc xác nhận xoá bắt buộc.

---

## 25. Xem câu hỏi

- **Mô tả**: Cho phép Giáo viên xem thông tin chi tiết của một câu hỏi trong ngân hàng câu hỏi.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; câu hỏi cần xem đã tồn tại trong ngân hàng câu hỏi.
- **Điều kiện sau**: Thông tin chi tiết câu hỏi được hiển thị đầy đủ.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang ngân hàng câu hỏi.
2. Hệ thống hiển thị danh sách câu hỏi hiện có.
3. Giáo viên chọn câu hỏi cần xem.
4. Hệ thống truy vấn thông tin chi tiết câu hỏi từ cơ sở dữ liệu.
5. Hệ thống hiển thị đầy đủ thông tin câu hỏi: nội dung, đáp án, cấp độ Bloom, độ khó và trạng thái.

---

## 26. Tìm kiếm câu hỏi

- **Mô tả**: Cho phép Giáo viên tìm kiếm và lọc câu hỏi trong ngân hàng câu hỏi theo nhiều tiêu chí khác nhau.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; ngân hàng câu hỏi có ít nhất một câu hỏi.
- **Điều kiện sau**: Danh sách câu hỏi phù hợp với tiêu chí tìm kiếm được hiển thị.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào mục Ngân hàng câu hỏi.
2. Hệ thống hiển thị giao diện tìm kiếm với các bộ lọc: Môn học, Chủ đề/Chương, Cấp độ Bloom, Độ khó, Loại câu hỏi, Trạng thái duyệt, Từ khoá nội dung.
3. Giáo viên nhập từ khoá và/hoặc chọn các bộ lọc cần thiết.
4. Giáo viên bấm nút tìm kiếm.
5. Hệ thống truy vấn cơ sở dữ liệu theo các tiêu chí đã chọn.
6. Hệ thống hiển thị danh sách câu hỏi phù hợp kèm thông tin tóm tắt.
7. Giáo viên có thể bấm vào câu hỏi để xem chi tiết hoặc thực hiện thao tác sửa/xoá.

**Luồng sự kiện thay thế:**
- *Bước 6 – Không tìm thấy câu hỏi phù hợp:* Hệ thống hiển thị thông báo Không tìm thấy câu hỏi phù hợp; Giáo viên điều chỉnh tiêu chí và thử lại.

---

## 27. Import câu hỏi hàng loạt từ Excel

- **Mô tả**: Cho phép Giáo viên import hàng loạt câu hỏi vào hệ thống thông qua file Excel theo mẫu định sẵn.
- **Actor chính**: Giáo viên
- **Các Use Case bao gồm (include)**: Parse file Excel; Phân loại Bloom và độ khó.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; đã chuẩn bị file Excel theo đúng mẫu hệ thống quy định.
- **Điều kiện sau**:
  - Câu hỏi hợp lệ được import thành công vào ngân hàng câu hỏi ở trạng thái Chờ duyệt.
  - Báo cáo kết quả import hiển thị số lượng thành công và lỗi.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn chức năng Import câu hỏi từ Excel.
2. Hệ thống hiển thị hướng dẫn import và nút tải file mẫu.
3. Giáo viên tải file mẫu (nếu cần), điền dữ liệu câu hỏi vào file.
4. Giáo viên tải lên file Excel đã điền.
5. Hệ thống thực hiện use case **Parse file Excel** để đọc và phân tích nội dung file.
6. Hệ thống thực hiện use case **Phân loại Bloom và độ khó** cho từng câu hỏi dựa trên dữ liệu trong file.
7. Hệ thống kiểm tra từng câu hỏi: định dạng, trường bắt buộc, danh mục hợp lệ.
8. Hệ thống lưu các câu hỏi hợp lệ vào ngân hàng câu hỏi.
9. Hệ thống hiển thị báo cáo kết quả: số câu thành công, số câu lỗi và danh sách lỗi chi tiết.

**Luồng sự kiện thay thế:**
- *Bước 5 – File không đúng định dạng:* Hệ thống hiển thị thông báo lỗi (file không phải Excel hoặc sai mẫu); Giáo viên kiểm tra lại file và tải lên lại.

---

## 28. Tạo mẫu đề thi

- **Mô tả**: Cho phép Giáo viên tạo mẫu đề thi bao gồm cấu hình các phần thi, tỉ lệ độ khó và bộ lọc theo cấp độ Bloom.
- **Actor chính**: Giáo viên
- **Các Use Case bao gồm (include)**: Cấu hình phần thi; Cài tỉ lệ độ khó.
- **Các Use Case mở rộng (extend)**: Cài lọc Bloom.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; danh mục môn học, chủ đề và cấu hình Bloom, độ khó đã được Quản trị viên thiết lập.
- **Điều kiện sau**: Mẫu đề thi được lưu vào hệ thống và sẵn sàng để sinh đề.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn Tạo mẫu đề thi mới.
2. Hệ thống hiển thị form thiết lập mẫu đề: tên mẫu, môn học, thời gian làm bài, tổng số câu.
3. Giáo viên điền thông tin chung của mẫu đề.
4. Hệ thống thực hiện use case **Cấu hình phần thi**: Giáo viên thêm và cấu hình từng phần thi (tên phần, số câu, loại câu hỏi).
5. Trong từng phần thi, hệ thống thực hiện use case **Cài tỉ lệ độ khó**: Giáo viên phân bổ tỉ lệ phần trăm câu theo mức dễ/trung bình/khó.
6. Nếu cần, Giáo viên thực hiện use case mở rộng **Cài lọc Bloom** để giới hạn cấp độ tư duy cho từng phần.
7. Giáo viên bấm lưu mẫu đề.
8. Hệ thống kiểm tra tính hợp lệ (tổng số câu, tỉ lệ phải bằng 100%).
9. Hệ thống lưu mẫu đề và thông báo thành công.

**Luồng sự kiện thay thế:**
- *Bước 8 – Cấu hình không hợp lệ:* Hệ thống hiển thị thông báo lỗi (tổng số câu không khớp hoặc tỉ lệ không bằng 100%); Giáo viên chỉnh sửa cấu hình và lưu lại.

---

## 29. Sinh đề thi từ mẫu

- **Mô tả**: Cho phép Giáo viên sinh đề thi thực tế từ mẫu đề đã tạo, sử dụng thuật toán lấy mẫu phân tầng (Stratified Sampling) và xáo trộn (Fisher-Yates Shuffle).
- **Actor chính**: Giáo viên
- **Các Use Case bao gồm (include)**: Lấy pool câu hỏi; Stratified Sampling; Fisher-Yates Shuffle; Lưu snapshot câu hỏi.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; mẫu đề thi đã được tạo và lưu; ngân hàng câu hỏi có đủ câu hỏi đã duyệt thoả mãn cấu hình mẫu.
- **Điều kiện sau**:
  - Đề thi được sinh thành công với đúng số lượng, tỉ lệ và phân loại câu hỏi theo mẫu.
  - Snapshot nội dung câu hỏi tại thời điểm sinh đề được lưu lại để đảm bảo tính toàn vẹn.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn mẫu đề thi và bấm Sinh đề.
2. Hệ thống thực hiện use case **Lấy pool câu hỏi**: truy vấn ngân hàng theo môn học, chủ đề, độ khó và Bloom từ cấu hình mẫu.
3. Hệ thống thực hiện use case **Stratified Sampling**: chọn đúng số câu hỏi theo từng tầng độ khó và Bloom trong pool.
4. Hệ thống thực hiện use case **Fisher-Yates Shuffle**: xáo trộn thứ tự câu hỏi và đáp án ngẫu nhiên.
5. Hệ thống thực hiện use case **Lưu snapshot câu hỏi**: lưu toàn bộ nội dung câu hỏi tại thời điểm sinh đề.
6. Hệ thống tạo bản ghi đề thi với mã đề duy nhất.
7. Hệ thống hiển thị đề thi vừa sinh và cho phép Giáo viên xem trước.

**Luồng sự kiện thay thế:**
- *Bước 2 – Không đủ câu hỏi trong pool:* Hệ thống hiển thị cảnh báo pool câu hỏi không đủ thoả mãn cấu hình mẫu; Giáo viên điều chỉnh cấu hình mẫu hoặc bổ sung câu hỏi vào ngân hàng.

---

## 30. Sinh nhiều mã đề

- **Mô tả**: Cho phép Giáo viên sinh nhiều mã đề thi khác nhau từ cùng một mẫu, mỗi mã đề có thứ tự câu hỏi và đáp án được xáo trộn độc lập.
- **Actor chính**: Giáo viên
- **Các Use Case bao gồm (include)**: Sinh đề từ mẫu.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; mẫu đề thi đã tồn tại và ngân hàng câu hỏi đủ điều kiện sinh đề.
- **Điều kiện sau**: Nhiều mã đề thi được sinh thành công, mỗi mã đề có nội dung câu hỏi giống nhau nhưng thứ tự khác nhau.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn mẫu đề và bấm Sinh nhiều mã đề.
2. Hệ thống hiển thị form yêu cầu nhập số lượng mã đề cần sinh.
3. Giáo viên nhập số lượng mã đề và xác nhận.
4. Hệ thống lặp lại use case **Sinh đề từ mẫu** cho mỗi mã đề, mỗi lần xáo trộn độc lập.
5. Hệ thống gán mã đề (mã số, màu sắc hoặc ký hiệu) cho từng đề.
6. Hệ thống hiển thị danh sách tất cả mã đề vừa sinh và cho phép xuất hàng loạt.

---

## 31. Xuất và xem trước đề thi

- **Mô tả**: Cho phép Giáo viên xem trước đề thi dưới dạng preview và xuất đề thi ra file PDF hoặc Word để in ấn, lưu trữ.
- **Actor chính**: Giáo viên
- **Các Use Case bao gồm (include)**: Lưu file vào MinIO.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; đề thi đã được sinh thành công và tồn tại trong hệ thống.
- **Điều kiện sau**:
  - File PDF hoặc Word của đề thi được tạo thành công và lưu vào MinIO.
  - Giáo viên nhận được đường dẫn tải file.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn đề thi cần xuất và bấm Xem trước hoặc Xuất đề.
2. Nếu chọn Xem trước: Hệ thống hiển thị đề thi trực tiếp trên trình duyệt với đầy đủ định dạng.
3. Nếu chọn Xuất PDF: Hệ thống render đề thi ra file PDF với định dạng chuẩn.
4. Nếu chọn Xuất Word: Hệ thống tạo file DOCX với nội dung đề thi.
5. Hệ thống thực hiện use case **Lưu file vào MinIO**: tải file lên hệ thống lưu trữ và lấy đường dẫn.
6. Hệ thống trả về đường dẫn tải file cho Giáo viên.
7. Giáo viên tải file về máy.

---

## 32. Giao kỳ thi cho lớp

- **Mô tả**: Cho phép Giáo viên giao kỳ thi cho lớp học cụ thể, thiết lập thời gian mở/đóng bài thi.
- **Actor chính**: Giáo viên
- **Các Use Case mở rộng (extend)**: Xem trước đề thi.
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; đề thi đã được sinh và Giáo viên có quyền quản lý lớp học đó.
- **Điều kiện sau**: Đề thi được giao cho lớp học và học sinh có thể truy cập để làm bài trong thời gian quy định.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên chọn kỳ thi và bấm Giao kỳ cho lớp.
2. Hệ thống hiển thị form giao kỳ: chọn lớp học, ngày mở bài, ngày đóng bài, thời gian làm bài.
3. Nếu cần, Giáo viên thực hiện use case mở rộng **Xem trước đề thi** để kiểm tra lại nội dung trước khi giao.
4. Giáo viên chọn lớp học đích và thiết lập thời gian.
5. Giáo viên bấm xác nhận giao kỳ.
6. Hệ thống kiểm tra tính hợp lệ của thời gian và lớp học.
7. Hệ thống lưu thông tin giao kỳ và gửi thông báo đến học sinh trong lớp.
8. Hệ thống hiển thị xác nhận giao kỳ thành công.

**Luồng sự kiện thay thế:**
- *Bước 6 – Thời gian hoặc lớp học không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Giáo viên điều chỉnh thời gian hoặc lớp học và xác nhận lại.

---

## 33. Học sinh làm bài thi

- **Mô tả**: Mô tả luồng Học sinh xem đề được giao, thực hiện làm bài thi trực tuyến và nộp bài.
- **Actor chính**: Học sinh
- **Các Use Case bao gồm (include)**: Chấm điểm tự động.
- **Điều kiện tiên quyết**: Học sinh đã đăng nhập; Giáo viên đã giao kỳ thi cho lớp của Học sinh và bài thi đang trong thời gian mở.
- **Điều kiện sau**:
  - Bài làm của Học sinh được lưu vào hệ thống.
  - Hệ thống tự động chấm điểm ngay sau khi nộp.
  - Học sinh có thể xem kết quả sau khi bài được chấm xong.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Học sinh đăng nhập và truy cập vào mục Đề thi của tôi.
2. Hệ thống hiển thị danh sách đề thi được giao kèm trạng thái và thời gian còn lại.
3. Học sinh chọn đề thi cần làm và bấm Bắt đầu làm bài.
4. Hệ thống kiểm tra điều kiện: bài thi đang trong thời gian mở, Học sinh chưa làm hoặc được phép làm lại.
5. Hệ thống hiển thị giao diện làm bài với đồng hồ đếm ngược.
6. Học sinh đọc đề và trả lời từng câu hỏi.
7. Hệ thống tự động lưu tạm bài làm định kỳ để tránh mất dữ liệu.
8. Học sinh bấm Nộp bài hoặc hết giờ thì hệ thống tự động nộp.
9. Hệ thống lưu bài làm và gọi use case **Chấm điểm tự động**.
10. Hệ thống hiển thị thông báo nộp bài thành công.

**Luồng sự kiện thay thế:**
- *Bước 8 – Hết giờ làm bài:* Khi đồng hồ về 0, hệ thống tự động nộp bài làm hiện tại và hiển thị thông báo "Hết thời gian - bài đã được nộp tự động".

---

## 34. Chấm điểm

- **Mô tả**: Mô tả quá trình chấm điểm bài thi của Học sinh. Sau khi nộp bài, hệ thống tự động chấm câu trắc nghiệm; phần tự luận được Giáo viên chấm thủ công.
- **Actor chính**: Học sinh, Giáo viên
- **Các Use Case bao gồm (include)**: Chấm điểm tự động.
- **Các Use Case mở rộng (extend)**: Chấm điểm thủ công.
- **Điều kiện tiên quyết**: Học sinh đã nộp bài thi thành công; đáp án đúng đã được lưu cùng snapshot câu hỏi.
- **Điều kiện sau**:
  - Điểm tự động được tính và lưu vào kết quả bài thi.
  - Điểm thủ công (nếu có) được Giáo viên cập nhật vào kết quả.
  - Học sinh có thể xem điểm sau khi chấm xong.

**Luồng sự kiện chính:**
1. Use case bắt đầu ngay sau khi Học sinh nộp bài (use case Nộp bài kích hoạt).
2. Hệ thống thực hiện use case **Chấm điểm tự động**: so sánh đáp án Học sinh với đáp án đúng cho toàn bộ câu trắc nghiệm.
3. Hệ thống tính tổng điểm phần tự động và lưu vào kết quả bài thi.
4. Nếu đề thi có câu tự luận, hệ thống chuyển bài thi sang trạng thái Chờ chấm tay.
5. Giáo viên nhận thông báo có bài thi cần chấm tay và thực hiện use case mở rộng **Chấm điểm thủ công**.
6. Giáo viên nhập điểm cho từng câu tự luận.
7. Hệ thống cộng điểm thủ công vào điểm tự động và tính điểm tổng kết.
8. Hệ thống cập nhật trạng thái bài thi thành Đã chấm và lưu kết quả.

---

## 35. Thêm cấu hình đề thi

- **Mô tả**: Cho phép Giáo viên thêm mới một cấu hình đề thi xác định cấu trúc, số lượng câu hỏi và tỉ lệ độ khó cho đề thi.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; danh mục môn học và chủ đề đã được cấu hình.
- **Điều kiện sau**: Cấu hình đề thi mới được lưu vào hệ thống và sẵn sàng để sinh đề.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang cấu hình đề thi và chọn thêm mới.
2. Hệ thống hiển thị mẫu điền thông tin cấu hình đề thi.
3. Giáo viên điền thông tin cấu hình: tên cấu hình, môn học, tổng số câu, thời gian làm bài, tỉ lệ độ khó.
4. Giáo viên bấm nút Lưu.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu nhập vào.
6. Hệ thống lưu cấu hình đề thi vào cơ sở dữ liệu.
7. Hệ thống hiển thị thông báo thêm cấu hình thành công.

**Luồng sự kiện thay thế:**
- *Bước 5 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Giáo viên chỉnh sửa và lưu lại.

---

## 36. Chỉnh sửa cấu hình đề thi

- **Mô tả**: Cho phép Giáo viên chỉnh sửa thông tin của một cấu hình đề thi đã tồn tại.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; cấu hình đề thi cần chỉnh sửa đã tồn tại.
- **Điều kiện sau**: Thông tin cấu hình đề thi được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang quản lý cấu hình đề thi.
2. Hệ thống hiển thị danh sách cấu hình đề thi hiện có.
3. Giáo viên chọn cấu hình cần chỉnh sửa và bấm vào nút chỉnh sửa.
4. Hệ thống hiển thị mẫu điền thông tin với dữ liệu hiện tại của cấu hình.
5. Giáo viên chỉnh sửa thông tin cần thay đổi.
6. Giáo viên bấm nút Lưu.
7. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
8. Hệ thống cập nhật thông tin cấu hình vào cơ sở dữ liệu.
9. Hệ thống hiển thị thông báo cập nhật thành công.

**Luồng sự kiện thay thế:**
- *Bước 7 – Dữ liệu không hợp lệ:* Hệ thống hiển thị thông báo lỗi; Giáo viên chỉnh sửa và lưu lại.

---

## 37. Xoá cấu hình đề thi

- **Mô tả**: Cho phép Giáo viên xoá một cấu hình đề thi không còn sử dụng khỏi hệ thống.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; cấu hình đề thi cần xoá đã tồn tại và không đang được sử dụng để sinh đề thi hiện hành.
- **Điều kiện sau**: Cấu hình đề thi được xoá khỏi hệ thống.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang quản lý cấu hình đề thi.
2. Hệ thống hiển thị danh sách cấu hình đề thi hiện có.
3. Giáo viên chọn cấu hình cần xoá và bấm nút xoá.
4. Hệ thống hiển thị hộp thoại xác nhận xoá.
5. Giáo viên xác nhận xoá.
6. Hệ thống xoá cấu hình đề thi khỏi cơ sở dữ liệu.
7. Hệ thống hiển thị thông báo xoá thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 6 – Cấu hình đang được sử dụng:* Hệ thống hiển thị cảnh báo cấu hình đang được sử dụng trong đề thi đã sinh; Giáo viên chọn huỷ hoặc xác nhận xoá bắt buộc.

---

## 38. Xem cấu hình đề thi

- **Mô tả**: Cho phép Giáo viên xem thông tin chi tiết của một cấu hình đề thi trong hệ thống.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; cấu hình đề thi cần xem đã tồn tại.
- **Điều kiện sau**: Thông tin chi tiết cấu hình đề thi được hiển thị đầy đủ.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang quản lý cấu hình đề thi.
2. Hệ thống hiển thị danh sách cấu hình đề thi.
3. Giáo viên chọn cấu hình cần xem.
4. Hệ thống truy vấn thông tin chi tiết cấu hình từ cơ sở dữ liệu.
5. Hệ thống hiển thị đầy đủ thông tin cấu hình: tên, môn học, số câu, thời gian, tỉ lệ độ khó, cấu trúc phần thi.

---

## 39. Tìm kiếm cấu hình đề thi

- **Mô tả**: Cho phép Giáo viên tìm kiếm cấu hình đề thi theo các tiêu chí như tên cấu hình, môn học hoặc trạng thái.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; có ít nhất một cấu hình đề thi trong hệ thống.
- **Điều kiện sau**: Danh sách cấu hình đề thi phù hợp với tiêu chí tìm kiếm được hiển thị.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang quản lý cấu hình đề thi.
2. Hệ thống hiển thị giao diện tìm kiếm với các bộ lọc: tên cấu hình, môn học, trạng thái.
3. Giáo viên nhập từ khoá và/hoặc chọn bộ lọc cần thiết.
4. Giáo viên bấm nút tìm kiếm.
5. Hệ thống truy vấn cơ sở dữ liệu theo các tiêu chí đã chọn.
6. Hệ thống hiển thị danh sách cấu hình đề thi phù hợp.

**Luồng sự kiện thay thế:**
- *Bước 6 – Không tìm thấy kết quả:* Hệ thống hiển thị thông báo không tìm thấy; Giáo viên điều chỉnh tiêu chí và thử lại.

---

## 40. Cập nhật trạng thái đề thi

- **Mô tả**: Cho phép Giáo viên cập nhật trạng thái của đề thi (ví dụ: Bản nháp, Đang hoạt động, Đã kết thúc) trong hệ thống.
- **Actor chính**: Giáo viên
- **Điều kiện tiên quyết**: Giáo viên đã đăng nhập; đề thi cần cập nhật trạng thái đã tồn tại.
- **Điều kiện sau**: Trạng thái đề thi được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Giáo viên truy cập vào trang quản lý đề thi.
2. Hệ thống hiển thị danh sách đề thi kèm trạng thái hiện tại.
3. Giáo viên chọn đề thi cần cập nhật trạng thái.
4. Hệ thống hiển thị danh sách trạng thái có thể chọn.
5. Giáo viên chọn trạng thái mới cho đề thi.
6. Hệ thống kiểm tra tính hợp lệ của thay đổi trạng thái.
7. Hệ thống lưu trạng thái mới vào cơ sở dữ liệu.
8. Hệ thống hiển thị thông báo cập nhật trạng thái thành công.

**Luồng sự kiện thay thế:**
- *Bước 6 – Trạng thái không hợp lệ:* Hệ thống hiển thị thông báo lỗi (chuyển đổi trạng thái không được phép); Giáo viên chọn trạng thái phù hợp khác.

---

## 41. Thêm người dùng

- **Mô tả**: Cho phép Quản trị viên thêm mới tài khoản người dùng vào hệ thống và phân vai trò tương ứng.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có quyền quản lý người dùng.
- **Điều kiện sau**:
  - Tài khoản người dùng mới được tạo thành công.
  - Người dùng mới có thể đăng nhập với tài khoản vừa tạo.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang quản lý người dùng và chọn thêm người dùng.
2. Hệ thống hiển thị mẫu điền thông tin người dùng mới.
3. Quản trị viên điền thông tin: họ tên, email, tên đăng nhập, mật khẩu tạm, vai trò (Quản trị viên / Giáo viên / Học sinh).
4. Quản trị viên bấm nút Lưu.
5. Hệ thống kiểm tra tính hợp lệ của dữ liệu (email hợp lệ, tên đăng nhập chưa tồn tại).
6. Hệ thống tạo tài khoản mới và lưu vào cơ sở dữ liệu.
7. Hệ thống hiển thị thông báo thêm người dùng thành công.

**Luồng sự kiện thay thế:**
- *Bước 5 – Tên đăng nhập hoặc email đã tồn tại:* Hệ thống hiển thị thông báo lỗi trùng lặp; Quản trị viên chỉnh sửa thông tin và lưu lại.

---

## 42. Chỉnh sửa người dùng

- **Mô tả**: Cho phép Quản trị viên cập nhật thông tin tài khoản của người dùng trong hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; tài khoản người dùng cần chỉnh sửa đã tồn tại.
- **Điều kiện sau**: Thông tin tài khoản người dùng được cập nhật thành công.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang quản lý người dùng.
2. Hệ thống hiển thị danh sách người dùng trong hệ thống.
3. Quản trị viên chọn người dùng cần chỉnh sửa và bấm nút chỉnh sửa.
4. Hệ thống hiển thị mẫu điền thông tin với dữ liệu hiện tại của người dùng.
5. Quản trị viên cập nhật thông tin cần thay đổi: họ tên, vai trò, trạng thái tài khoản.
6. Quản trị viên bấm nút Lưu.
7. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
8. Hệ thống cập nhật thông tin người dùng vào cơ sở dữ liệu.
9. Hệ thống hiển thị thông báo cập nhật thành công.

---

## 43. Xoá người dùng

- **Mô tả**: Cho phép Quản trị viên xoá tài khoản người dùng khỏi hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; tài khoản người dùng cần xoá đã tồn tại.
- **Điều kiện sau**: Tài khoản người dùng được xoá khỏi hệ thống.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang quản lý người dùng.
2. Hệ thống hiển thị danh sách người dùng trong hệ thống.
3. Quản trị viên chọn người dùng cần xoá và bấm nút xoá.
4. Hệ thống hiển thị hộp thoại xác nhận xoá.
5. Quản trị viên xác nhận xoá.
6. Hệ thống xoá tài khoản người dùng khỏi cơ sở dữ liệu.
7. Hệ thống hiển thị thông báo xoá thành công và cập nhật danh sách.

**Luồng sự kiện thay thế:**
- *Bước 6 – Người dùng đang có dữ liệu liên quan:* Hệ thống hiển thị cảnh báo người dùng đang có dữ liệu liên quan (bài làm, đề thi); Quản trị viên chọn huỷ hoặc xác nhận xoá bắt buộc.

---

## 44. Xem người dùng

- **Mô tả**: Cho phép Quản trị viên xem thông tin chi tiết của một tài khoản người dùng trong hệ thống.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; tài khoản người dùng cần xem đã tồn tại.
- **Điều kiện sau**: Thông tin chi tiết người dùng được hiển thị đầy đủ.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang quản lý người dùng.
2. Hệ thống hiển thị danh sách người dùng.
3. Quản trị viên chọn người dùng cần xem.
4. Hệ thống truy vấn thông tin chi tiết người dùng từ cơ sở dữ liệu.
5. Hệ thống hiển thị đầy đủ thông tin: họ tên, email, tên đăng nhập, vai trò, trạng thái tài khoản, lớp học liên quan.

---

## 45. Tìm kiếm người dùng

- **Mô tả**: Cho phép Quản trị viên tìm kiếm người dùng theo các tiêu chí như tên, email, vai trò hoặc trạng thái tài khoản.
- **Actor chính**: Quản trị viên
- **Điều kiện tiên quyết**: Quản trị viên đã đăng nhập; có ít nhất một tài khoản người dùng trong hệ thống.
- **Điều kiện sau**: Danh sách người dùng phù hợp với tiêu chí tìm kiếm được hiển thị.

**Luồng sự kiện chính:**
1. Use case bắt đầu khi Quản trị viên truy cập vào trang quản lý người dùng.
2. Hệ thống hiển thị giao diện tìm kiếm với các bộ lọc: họ tên, email, vai trò, trạng thái.
3. Quản trị viên nhập từ khoá và/hoặc chọn bộ lọc cần thiết.
4. Quản trị viên bấm nút tìm kiếm.
5. Hệ thống truy vấn cơ sở dữ liệu theo các tiêu chí đã chọn.
6. Hệ thống hiển thị danh sách người dùng phù hợp kèm thông tin tóm tắt.

**Luồng sự kiện thay thế:**
- *Bước 6 – Không tìm thấy kết quả:* Hệ thống hiển thị thông báo không tìm thấy; Quản trị viên điều chỉnh tiêu chí và thử lại.
