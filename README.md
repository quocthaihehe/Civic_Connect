# CivicConnect 🏛️

**CivicConnect** là một nền tảng "Chính quyền điện tử" toàn diện, đóng vai trò như một cầu nối thông minh giữa người dân và các cơ quan chức năng. Hệ thống giúp số hóa quy trình phản ánh kiến nghị, cung cấp dịch vụ công trực tuyến, xây dựng cộng đồng số và đặc biệt được tích hợp Trí tuệ nhân tạo (Google Gemini AI) để nâng cao hiệu suất xử lý thông tin.

---

## 🌟 Tính năng Nổi bật

### 1. Dành cho Người dân (User Portal)
- **Hệ thống Tài khoản & KYC:** Đăng ký/đăng nhập an toàn, phân quyền tài khoản theo các cấp độ định danh (KYC Level 1, 2, 3) để đảm bảo tính xác thực của người dùng.
- **Gửi Phản ánh (Issues):** Giao diện gửi phản ánh 4 bước trực quan. Hỗ trợ định vị vị trí hiện trường qua bản đồ tương tác (Leaflet.js) và tải lên nhiều hình ảnh minh chứng. Tùy chọn gửi ẩn danh.
- **Tra cứu Dịch vụ công:** Cung cấp thông tin chi tiết về các thủ tục hành chính, danh bạ các cơ quan ban ngành, giúp người dân dễ dàng tra cứu.
- **Cộng đồng & Tương tác:** 
  - Tham gia diễn đàn thảo luận.
  - Bỏ phiếu cho các khảo sát (Polls).
  - Ký tên ủng hộ các kiến nghị cộng đồng (Petitions).
  - Tham gia các sự kiện xã hội.
- **Tóm tắt Chính sách AI:** Sử dụng Trí tuệ nhân tạo để tóm tắt các văn bản luật, chính sách dài dòng thành các ý chính ngắn gọn, dễ hiểu cho người dân.

### 2. Dành cho Cơ quan quản lý (Admin Dashboard)
- **Bảng Điều hành & Thống kê (Dashboard):** Hiển thị tổng quan các chỉ số, biểu đồ phân tích chuyên sâu (Analytics) và báo cáo tài chính/quỹ (Finance) bằng Chart.js.
- **Quản lý Phản ánh chuyên nghiệp:**
  - **Bảng Dữ liệu:** Danh sách phản ánh chi tiết, dễ dàng lọc và tìm kiếm.
  - **Bảng Kanban:** Quản lý tiến độ xử lý phản ánh theo phương pháp Agile (Kéo - thả các thẻ công việc giữa các cột trạng thái).
  - **Smart Routing (Phân luồng thông minh):** Thiết lập các quy tắc tự động chuyển hướng phản ánh về đúng cơ quan thụ lý dựa trên từ khóa hoặc loại danh mục.
- **Tổ chức & Cán bộ:** Quản lý cơ quan liên kết, hồ sơ cán bộ, và sắp xếp Lịch trực/Ca trực (Shifts) một cách khoa học.
- **Quản lý Nội dung:** Đăng tải tin tức, thông báo hàng loạt tới người dân, quản lý các danh mục và quy chuẩn thời gian xử lý (SLA).
- **Cấu hình & Giám sát Hệ thống:**
  - Cài đặt thông số chung.
  - Xem **Nhật ký hoạt động (Audit Logs)** chi tiết của mọi thao tác trong hệ thống.
  - **Giám sát hiệu năng (Health)**: Theo dõi tình trạng máy chủ, CPU, RAM trực tuyến.

---

## 💻 Công nghệ Sử dụng

- **Backend:** C# / ASP.NET Core 8 MVC
- **Database:** Entity Framework Core
- **Frontend (Giao diện):** 
  - Người dùng: CSS thuần, Bootstrap 5, Tailwind CSS
  - Quản trị: **Tabler UI** (Dashboard template cao cấp)
- **Bản đồ:** Leaflet.js & OpenStreetMap
- **Biểu đồ:** Chart.js
- **Trí tuệ Nhân tạo (AI):** Cổng kết nối API Google Gemini (`GeminiAiService`).

---

## 📂 Cấu trúc Dự án (Nổi bật)

```text
CivicConnect.Web/
├── Areas/
│   ├── Admin/                # Toàn bộ giao diện và logic của trang Quản trị viên
│   └── Identity/             # Quản lý Đăng nhập, Đăng ký, Quên mật khẩu
├── Controllers/              # Xử lý logic điều hướng (Issues, Community, PublicServices...)
├── Models/
│   ├── Entities/             # Các bảng trong Cơ sở dữ liệu (Issue, AuditLog, Shift...)
│   └── Ai/                   # Các Model phục vụ cho tính năng AI
├── Services/                 # Chứa logic nghiệp vụ (GeminiAiService...)
├── Views/                    # Giao diện người dùng (Cộng đồng, Dịch vụ công, Phản ánh...)
├── wwwroot/                  # Chứa tài nguyên tĩnh: CSS, JS, Images
│   └── css/
│       ├── admin-theme.css   # Giao diện tùy biến cho trang Admin
│       └── topbar.css        # Giao diện thanh Topbar của user
└── appsettings.json          # Tệp cấu hình hệ thống (Chuỗi kết nối DB, API Key)
```

---

## 🚀 Hướng dẫn Cài đặt & Khởi chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- Visual Studio 2022 hoặc VS Code
- SQL Server (Hoặc LocalDB)

### Các bước thực hiện
1. **Sao chép (Clone) dự án** về máy.
2. Mở Solution bằng Visual Studio.
3. Cấu hình tệp `appsettings.json`:
   - Cập nhật `DefaultConnection` trong phần `ConnectionStrings` trỏ tới Database của bạn.
   - Thêm `ApiKey` của Google Gemini vào phần cấu hình `Gemini`.
4. **Cập nhật Cơ sở dữ liệu:** Mở Package Manager Console (PMC) và chạy lệnh:
   ```bash
   Update-Database
   ```
5. Nhấn **F5** hoặc `Ctrl + F5` để khởi chạy ứng dụng.

---

## 🎨 Ghi chú về Thiết kế Giao diện
Dự án được chăm chút kỹ lưỡng về mặt UX/UI:
- **Responsive:** Hoạt động trơn tru trên cả Desktop và Mobile.
- **Dark Mode:** Hỗ trợ giao diện sáng/tối tự động trên bảng điều khiển Quản trị.
- **Vi mạch & Glassmorphism:** Áp dụng các phong cách thiết kế hiện đại (Nền kính mờ, hiệu ứng nổi) giúp giao diện sang trọng, thân thiện.

---

**© 2026 CivicConnect.** Nền tảng được xây dựng nhằm nâng cao chất lượng sống và sự minh bạch trong quản lý đô thị.
