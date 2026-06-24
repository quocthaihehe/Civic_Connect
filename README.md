# CivicConnect 🏛️

> **CivicConnect** là nền tảng **Chính quyền điện tử (e-Government)** toàn diện, đóng vai trò cầu nối thông minh giữa **người dân** và **cơ quan chức năng** TP. Hồ Chí Minh. Hệ thống số hoá toàn bộ quy trình tiếp nhận và xử lý phản ánh, cung cấp dịch vụ công trực tuyến, kết nối cộng đồng và tích hợp **Trí tuệ nhân tạo Google Gemini AI** để nâng cao chất lượng quản lý đô thị.

---

## 🌟 Tính năng theo Phân hệ

### 👤 Portal Người dân

#### Tài khoản & Định danh (KYC)
- Đăng ký / Đăng nhập an toàn qua ASP.NET Identity.
- **Hệ thống định danh KYC (Know Your Citizen)** 3 cấp độ:
  - **Unverified:** Tài khoản mới tạo, chưa xác minh.
  - **Level 1:** Xác thực số điện thoại OTP (qua CCCD/CMND).
  - **Level 2:** Nộp ảnh CCCD mặt trước/mặt sau.
  - **Level 3:** Selfie kèm CCCD (xác minh danh tính đầy đủ).
- **Xác thực 2 bước (2FA)** tùy chỉnh: Hỗ trợ qua Telegram, Discord, hoặc Authenticator App.
- **Hệ thống điểm công dân (CitizenPoints)** và **Điểm uy tín (TrustScore)** với các cấp huy hiệu: Tân binh → Đồng → Bạc → Vàng → Kim cương.
- Quản lý hồ sơ cá nhân chi tiết (ảnh đại diện, địa chỉ hành chính theo tỉnh/quận/phường).
- Thông báo cảnh báo nổi bật nếu tài khoản **chưa được xác thực KYC**.

#### Gửi Phản ánh (Issues)
- Giao diện wizard **4 bước** trực quan và thân thiện:
  1. **Chọn danh mục**: Giao thông, Môi trường, An ninh, Hạ tầng, Hành chính, và **Khác** (nhập thủ công).
  2. **Vị trí & Minh chứng**: Bản đồ tương tác (Leaflet.js) cho phép click hoặc kéo ghim để chọn vị trí. Reverse Geocoding tự động lấy địa chỉ từ tọa độ. **Kiểm tra ranh giới địa phận TP.HCM tức thời** – hiện Modal cảnh báo ngay khi ghim nằm ngoài phạm vi. Tải lên tối đa 5 ảnh minh chứng với giao diện **Gallery** (1 ảnh lớn + thanh thumbnail).
  3. **Chi tiết**: Tiêu đề, mô tả, mức độ ưu tiên. Tùy chọn **Gửi ẩn danh**.
  4. **Xác nhận**: Xem lại tóm tắt trước khi gửi.
- Theo dõi tiến độ xử lý phản ánh đã gửi theo thời gian thực.
- Xem lịch sử chuyển trạng thái và bình luận của cán bộ.

#### Danh mục "Khác" – Luồng xử lý đặc biệt
- Người dân nhập thủ công tên danh mục khi chọn "Khác".
- Tên danh mục tùy chỉnh được lưu riêng vào trường `CustomCategoryName`.
- Admin/Cán bộ xem xét, phân loại và phân công xử lý dựa trên nội dung thực tế.

#### Dịch vụ công & Thông tin
- Tra cứu **Thủ tục hành chính** (AdministrativeProcedures) với thông tin đầy đủ, liên kết tải biểu mẫu.
- **Danh bạ Cơ quan** (AgencyDirectory): Địa chỉ, số điện thoại, giờ làm việc của các đơn vị chức năng.
- **Bản đồ cơ quan**: Hiển thị vị trí các đơn vị trực thuộc trên bản đồ.
- **Chính sách & Văn bản pháp luật**: Đọc, tìm kiếm và **Tóm tắt bằng AI (Gemini)** các văn bản dài phức tạp.

#### Cộng đồng
- **Diễn đàn thảo luận**: Tạo chủ đề, bình luận, bỏ phiếu (upvote/downvote).
- **Khảo sát (Polls)**: Tham gia các cuộc bỏ phiếu về các vấn đề cộng đồng.
- **Kiến nghị (Petitions)**: Ký tên ủng hộ các kiến nghị cộng đồng.
- **Sự kiện (Events)**: Xem và đăng ký tham gia sự kiện cộng đồng do chính quyền tổ chức.

#### Quyên góp & Tài trợ
- Gửi đóng góp quỹ cộng đồng qua cổng thanh toán **MoMo**.
- Xem lịch sử giao dịch và số dư quỹ.

---

### 🛡️ Admin Dashboard (Cán bộ & Quản trị viên)

#### Bảng Điều hành (Dashboard)
- Hiển thị các KPI tổng quan: Tổng phản ánh, phản ánh đang xử lý, số người dùng, tỷ lệ hoàn thành SLA.
- **Biểu đồ phân tích (Analytics)**: Xu hướng phản ánh theo thời gian, phân bố theo danh mục, hiệu suất xử lý của từng cán bộ (Chart.js).
- **Báo cáo Tài chính/Quỹ**: Tổng hợp thu chi quỹ quyên góp.
- **Nhật ký Hệ thống**: Xem lịch sử hoạt động realtime trên Dashboard.

#### Quản lý Phản ánh
- **Danh sách (DataTable)**: Bảng lọc, tìm kiếm, sắp xếp nâng cao với phân trang.
- **Bảng Kanban**: Quản lý phản ánh theo cột trạng thái (Mới nhận → Đã phân công → Đang xử lý → Hoàn tất / Từ chối). Kéo thả thẻ bằng **SortableJS** với hiệu ứng mượt mà và auto-scroll. Hỗ trợ cuộn nội dung trong từng cột khi cần.
  - Cán bộ B1 (trực ban) xem toàn bộ, chỉ có thể chuyển sang "Đã phân công".
  - Cán bộ B2 (quản lý) phân công cán bộ xử lý trước khi chuyển trạng thái.
  - Cán bộ B5 (xử lý) chuyển sang "Hoàn tất" hoặc "Từ chối".
  - Mỗi lần thay đổi trạng thái đều hiển thị **thông báo Toast** rõ ràng.
- **Chi tiết Phản ánh**: Xem ảnh minh chứng, lịch sử trạng thái, thêm bình luận nội bộ, phân công cán bộ xử lý.
- **Smart Routing (Phân luồng thông minh)**: Thiết lập quy tắc tự động phân công phản ánh về đúng đơn vị dựa trên từ khóa hoặc danh mục.

#### Quản lý Người dùng
- Xem danh sách, tìm kiếm, và duyệt/từ chối hồ sơ xác thực KYC của người dân.
- Khoá/mở khoá tài khoản, thêm lý do hạn chế.
- Xem chi tiết hồ sơ, điểm uy tín, lịch sử hoạt động của từng người dùng.

#### Quản lý Cán bộ & Tổ chức
- Quản lý danh sách **Cán bộ (Officials)** và phân quyền vai trò chi tiết.
- Quản lý **Đơn vị hành chính (GovernmentUnits)** và sơ đồ tổ chức.
- **Lịch trực / Ca trực (Shifts)**: Sắp xếp lịch làm việc và trực ban khoa học.

#### Quản lý Nội dung
- Đăng tải/chỉnh sửa **Chính sách và Văn bản** (Policies).
- Gửi **Thông báo hàng loạt** (Mass Notifications) tới nhóm người dùng hoặc toàn bộ hệ thống.
- Quản lý **Tin tức & Bài viết** trên portal người dân.
- Cấu hình **SLA (Service Level Agreement)**: Thiết lập thời gian xử lý tối đa theo từng mức độ ưu tiên.

#### Cấu hình & Hệ thống
- Cài đặt thông tin tổ chức, logo, thông số hoạt động.
- **Nhật ký Audit Logs**: Ghi lại và hiển thị chi tiết mọi thao tác trong hệ thống (ai làm gì, lúc nào).
- **Giám sát Hiệu năng (Health Monitor)**: Theo dõi tình trạng CPU, RAM và thời gian phản hồi của server trực tuyến.
- Quản lý **Chế độ Bảo trì (Maintenance Mode)**.

---

## 💻 Công nghệ Sử dụng

| Thành phần | Công nghệ |
|---|---|
| **Backend** | C# / ASP.NET Core 8 MVC |
| **ORM / Database** | Entity Framework Core 8 + SQL Server |
| **Identity** | ASP.NET Core Identity (mở rộng tùy biến) |
| **Realtime** | SignalR (NotificationHub, DonationHub) |
| **Frontend User** | Bootstrap 5, CSS thuần, Tailwind CSS |
| **Frontend Admin** | Tabler UI (Dashboard Premium) |
| **Bản đồ** | Leaflet.js, OpenStreetMap, CARTO, ArcGIS |
| **Biểu đồ** | Chart.js |
| **Drag & Drop** | SortableJS |
| **Trí tuệ Nhân tạo** | Google Gemini API (`GeminiAiService`) |
| **Thanh toán** | MoMo Payment Gateway |
| **Lưu trữ ảnh** | Cloudinary |
| **Email** | Dịch vụ SMTP (EmailService) |
| **Background Jobs** | IHostedService (SlaCheckerService) |
| **Timezone** | UTC → UTC+7 (Giờ Việt Nam) |

---

## 📂 Cấu trúc Dự án

```text
CivicConnect.Web/
├── Areas/
│   ├── Admin/                    # Toàn bộ giao diện & logic Quản trị
│   │   ├── Controllers/          # AdminController, IssueManagementController...
│   │   └── Views/
│   │       ├── Dashboard/        # Trang tổng quan & phân tích
│   │       ├── IssueManagement/  # DataTable, Kanban, RoutingRules, Details
│   │       ├── Users/            # Quản lý người dùng & KYC
│   │       ├── Officials/        # Quản lý cán bộ & ca trực
│   │       ├── Units/            # Quản lý đơn vị hành chính
│   │       ├── Content/          # Quản lý tin tức, thông báo
│   │       ├── Policies/         # Quản lý văn bản pháp luật
│   │       ├── Reports/          # Báo cáo thống kê
│   │       └── System/           # Cấu hình, Audit Logs, Health Monitor
│   └── Identity/                 # Đăng nhập, Đăng ký, Hồ sơ, KYC, 2FA
│
├── Controllers/
│   ├── HomeController.cs         # Trang chủ, thông báo
│   ├── IssuesController.cs       # Gửi & tra cứu phản ánh
│   ├── CommunityController.cs    # Diễn đàn, Khảo sát, Kiến nghị, Sự kiện
│   ├── PolicyController.cs       # Tra cứu chính sách & tóm tắt AI
│   ├── PublicServicesController.cs # Thủ tục hành chính & danh bạ
│   ├── DonationController.cs     # Quyên góp quỹ & MoMo
│   ├── MapController.cs          # API bản đồ đơn vị
│   └── OfficialController.cs     # Thông tin cán bộ công khai
│
├── Models/
│   ├── Entities/                 # Bảng CSDL (Issue, ApplicationUser, Shift...)
│   ├── ViewModels/               # DTO cho View
│   ├── Enums/                    # KYCLevel, IssueStatus, IssuePriority...
│   └── Ai/                       # Model phục vụ AI
│
├── Services/
│   ├── GeminiAiService.cs        # Tích hợp Google Gemini AI
│   ├── IssueService.cs           # Logic nghiệp vụ phản ánh & kiểm tra ranh giới
│   ├── MomoService.cs            # Cổng thanh toán MoMo
│   ├── PhotoService.cs           # Lưu ảnh lên Cloudinary
│   ├── EmailService.cs           # Gửi email (SMTP)
│   ├── NotificationService.cs    # Thông báo realtime (SignalR)
│   ├── SlaCheckerService.cs      # Background job kiểm tra SLA
│   └── BackgroundJobs/           # Các tác vụ nền khác
│
├── Hubs/
│   ├── NotificationHub.cs        # SignalR Hub cho thông báo realtime
│   └── DonationHub.cs            # SignalR Hub cho cập nhật quyên góp realtime
│
├── Data/
│   ├── AppDbContext.cs           # EF Core DbContext
│   ├── DbInitializer.cs          # Seeder dữ liệu ban đầu
│   └── Migrations/               # Lịch sử migration CSDL (14 phiên bản)
│
├── Helpers/                      # Các helper/extension dùng chung
├── Repositories/                 # Repository pattern (tuỳ chọn)
│
└── wwwroot/
    ├── css/
    │   ├── admin-theme.css       # Tùy biến giao diện trang Admin
    │   └── topbar.css            # Topbar & layout người dùng
    └── js/
        ├── issue-create.js       # Logic wizard, Leaflet map, gallery ảnh
        └── ...
```

---

## 🗃️ Lịch sử Cơ sở dữ liệu (Migrations)

| Phiên bản | Mô tả |
|---|---|
| `InitialCreate` | Khởi tạo toàn bộ schema ban đầu |
| `AddPoliciesTable` | Thêm bảng văn bản pháp luật |
| `AddDonationTables` | Thêm bảng quyên góp & danh mục quỹ |
| `AddPolicyDetailsFields` | Bổ sung trường chi tiết cho Policy |
| `AddIssueRatingFields` | Thêm đánh giá phản ánh |
| `AddTrustScore` | Thêm điểm uy tín người dùng |
| `UpgradeAdminAreaSchema` | Nâng cấp schema Admin (GovernmentUnit, Shift...) |
| `AddPhase5Entities` | Thêm SmartRoutingRule, AuditLog, SystemSetting |
| `Phase6_Community` | Thêm các bảng cộng đồng (Forum, Poll, Petition...) |
| `AddPolicyAiSummaryTable` | Bảng lưu tóm tắt AI cho chính sách |
| `AddMorePoliciesSeedData` | Thêm dữ liệu mẫu văn bản pháp luật |
| `UpgradeCommunityAnd2FA` | Thêm xác thực 2FA tùy chỉnh & nâng cấp cộng đồng |
| `AddProvinceBoundaryAndEncryptCitizenId` | Thêm bảng ranh giới tỉnh, mã hoá CCCD |
| `AddCustomCategoryName` | Thêm trường `CustomCategoryName` cho danh mục "Khác" |

---

## 🚀 Hướng dẫn Cài đặt & Khởi chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code
- SQL Server (LocalDB hoặc SQL Server Express)
- Node.js (nếu cần build Tailwind CSS)

### Các bước thực hiện
1. **Clone dự án** về máy:
   ```bash
   git clone <repository-url>
   ```
2. **Cấu hình `appsettings.json`**: Sao chép từ file mẫu `appsettings.Example.json` và điền đầy đủ:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "<Chuỗi kết nối SQL Server của bạn>"
     },
     "Gemini": { "ApiKey": "<Google Gemini API Key>" },
     "Cloudinary": { "CloudName": "...", "ApiKey": "...", "ApiSecret": "..." },
     "Momo": { "... (cấu hình cổng MoMo)" }
   }
   ```
3. **Cập nhật Cơ sở dữ liệu** (tạo toàn bộ schema + dữ liệu mẫu):
   ```bash
   # Mở Package Manager Console trong Visual Studio
   Update-Database
   ```
4. **Khởi chạy ứng dụng**:
   - Nhấn **F5** (Debug) hoặc **Ctrl + F5** (No Debug) trong Visual Studio.
   - Hoặc chạy bằng CLI: `dotnet run`
5. **Truy cập hệ thống**: Mở trình duyệt tại `https://localhost:7043`

### Tài khoản mặc định (sau khi seed)
| Vai trò | Email | Mật khẩu |
|---|---|---|
| Super Admin | `admin@civicconnect.vn` | *(Xem DbInitializer)* |
| Người dân | Tự đăng ký | — |

---

## 🎨 Thiết kế Giao diện

- **Responsive**: Hoạt động trên Desktop, Tablet và Mobile.
- **Dark Mode**: Hỗ trợ chuyển đổi sáng/tối trên toàn bộ Admin Dashboard.
- **Màu sắc đặc trưng**: Xanh dương chủ đạo (Primary Blue `#206bc4`) xuyên suốt toàn hệ thống.
- **Glassmorphism & Micro-animations**: Hiệu ứng kính mờ, đổ bóng mềm, transition mượt mà.
- **Toast Notifications**: Thông báo nhanh không chặn giao diện cho mọi thao tác.
- **Thời gian Việt Nam**: Toàn bộ dữ liệu hiển thị theo múi giờ **UTC+7 (Giờ Hà Nội)**.
- **Modal cảnh báo thông minh**: Các lỗi nghiệp vụ quan trọng (vị trí ngoài phạm vi, KYC chưa xác thực...) hiển thị qua Modal với hiệu ứng blur nền.

---

## ⚙️ Kiến trúc Hệ thống

```
┌─────────────────────────────────────────────────┐
│                   Người dùng                    │
│         (Trình duyệt Web / Mobile)              │
└──────────────────────┬──────────────────────────┘
                       │ HTTPS
┌──────────────────────▼──────────────────────────┐
│             ASP.NET Core 8 MVC                  │
│  Controllers ──► Services ──► Repositories      │
│       │               │                         │
│    SignalR        AI (Gemini)                    │
│    (Realtime)     MoMo / Cloudinary              │
└──────────────────────┬──────────────────────────┘
                       │ EF Core
┌──────────────────────▼──────────────────────────┐
│           SQL Server Database                   │
│  (Somee.com Cloud hoặc LocalDB)                 │
└─────────────────────────────────────────────────┘
```

---

**© 2026 CivicConnect** — Nền tảng xây dựng nhằm nâng cao chất lượng sống và minh bạch quản lý đô thị tại TP. Hồ Chí Minh.
