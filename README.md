# Civic Connect - Hệ Thống Phản Ánh Hiện Trường & Kết Nối Cộng Đồng

Civic Connect là nền tảng số hóa hỗ trợ kết nối trực tiếp giữa **Công dân** và **Cơ quan chính quyền** địa phương nhằm giải quyết nhanh chóng các vấn đề phát sinh trong đời sống đô thị (Giao thông, Môi trường, An ninh trật tự, Hạ tầng công cộng...). Nền tảng được xây dựng theo kiến trúc hiện đại, giao diện kính mờ (glassmorphism) sang trọng và thân thiện.

---

## 📋 Mục Lục Slide Thuyết Trình (Đề Xuất)
1. **Đặt Vấn Đề & Ý Tưởng Dự Án**
2. **Kiến Trúc & Công Nghệ Sử Dụng**
3. **Vai Trò & Tính Năng Phân Hệ: Công Dân**
4. **Vai Trò & Tính Năng Phân Hệ: Cán Bộ Xử Lý (Official)**
5. **Vai Trò & Tính Năng Phân Hệ: Quản Trị Viên (Admin)**
6. **Các Tính Năng Nổi Bật & Công Nghệ Đặc Đặc Trưng**

---

## 🛠️ Công Nghệ Sử Dụng (Technology Stack)
* **Backend:** ASP.NET Core Monolithic MVC (hỗ trợ .NET 9.0 Roll-forward).
* **Database & ORM:** SQLite Database kết hợp Entity Framework Core (tự động Migrate và Seed tài khoản thử nghiệm).
* **Frontend:** HTML5, CSS3 (Custom Glassmorphism Design, Dark/Light Themes), Bootstrap 5, Javascript, jQuery.
* **Bản đồ số:** Leaflet API (Tích hợp OpenStreetMap và Lớp bản đồ nhiệt Heatmap).
* **Realtime Communication:** SignalR (Đẩy thông báo tức thời và cập nhật quyên góp realtime).
* **Thanh toán điện tử:** Tích hợp Cổng thanh toán ví điện tử Momo API.
* **Xử lý hình ảnh:** Cloudinary API lưu trữ ảnh đám mây kết hợp thư viện `heic2any` (Client-side) và `Magick.NET` (Server-side) hỗ trợ giải mã ảnh định dạng `.heic` từ iPhone.

---

## 👥 Các Vai Trò & Tính Năng Chi Tiết (Nội Dung Chính Cho Slide)

### 1. Phân Hệ Công Dân (Citizens) - "Tai Mắt" Đô Thị
* **Gửi phản ánh nhanh chóng:** Nhập tiêu đề, mô tả chi tiết, gắn danh mục lỗi (Giao thông, Môi trường, An ninh, Hạ tầng,...).
* **Tương tác vị trí & Bản đồ số:** Chọn vị trí sự cố trực quan trên bản đồ Leaflet hoặc nhập địa chỉ để tự động lấy tọa độ GPS.
* **Upload ảnh chất lượng cao:** Hỗ trợ tải lên nhiều ảnh hiện trường, tự động nhận diện và chuyển đổi ảnh `.heic` từ iPhone trực tiếp tại Client giúp tối ưu dung lượng.
* **Biểu quyết cộng đồng:** Upvote / Downvote các phản ánh của người khác để thể hiện mức độ quan tâm của cộng đồng (là cơ sở để tính điểm ưu tiên xử lý).
* **Thảo luận công khai:** Đăng bình luận dưới phản ánh để chia sẻ thông tin hoặc góp ý giải pháp.
* **Đánh giá mức độ hài lòng (Satisfaction Rating):** Đánh giá 1-5 sao kèm ý kiến phản hồi sau khi cơ quan chức năng hoàn thành xử lý sự cố.
* **Đóng góp Quỹ cộng đồng:** Xem danh sách các chiến dịch quyên góp công ích và quyên góp tiền trực tiếp qua Momo.

### 2. Phân Hệ Cán Bộ Cơ Quan (Officials) - Tiếp Nhận & Giải Quyết
* **Quản lý danh sách nhiệm vụ:** Tiếp nhận các phản ánh thuộc địa bàn quản lý (Phường/Xã, Quận/Huyện) và lĩnh vực chuyên môn.
* **Cập nhật tiến độ xử lý:** Đổi trạng thái xử lý (Đang xử lý -> Đã giải quyết), đính kèm biên bản nghiệm thu hoặc ảnh kết quả thực tế.
* **Chuyển cấp xử lý (Escalation):** Tự động chuyển tiếp phản ánh lên cấp cao hơn (Quận/Huyện hoặc Sở ban ngành) nếu sự việc vượt quá thẩm quyền giải quyết của đơn vị.
* **Phản hồi chính thức:** Đăng câu trả lời chính thức của cơ quan với nhãn **[Cán bộ]** nổi bật màu vàng trong phần thảo luận công khai.

### 3. Phân Hệ Quản Trị Viên (Admin) - Kiểm Duyệt & Vận Hành Hệ Thống
* **Kiểm duyệt phản ánh & Gắn "Tích Xanh" (Verified Badge):**
  - Xem và duyệt toàn bộ phản ánh của người dân gửi lên.
  - Gắn nhãn **Tích xanh xác thực** (`IsVerified`) đối với các thông tin đã được kiểm chứng thực tế, hiển thị nổi bật trên toàn hệ thống.
* **Điều phối & Tái phân công:** Thay đổi đơn vị tiếp nhận (chuyển giao giữa các cơ quan hành chính) nếu phát hiện phân sai địa bàn hoặc sai lĩnh vực.
* **Quản lý Quỹ quyên góp cộng đồng:**
  - Tạo mới chiến dịch quyên góp (Mục tiêu số tiền, thời hạn, mô tả chi tiết).
  - Tạm ngưng/Kích hoạt trạng thái hoạt động của chiến dịch.
  - Thống kê chi tiết lịch sử giao dịch nạp tiền qua Momo của người dân theo thời gian thực.
* **Soạn thảo chính sách & Tin tức pháp luật:** Viết bài đăng mới, phân loại theo dạng (Thông báo, Nghị định, Thông tư, Tin tức) để cập nhật thông tin pháp luật đô thị đến người dân.
* **Kiểm duyệt bình luận:** Xóa các bình luận spam, thô tục hoặc gây chia rẽ trực tiếp ngay tại trang chi tiết phản ánh.
* **Quản lý thực thể hệ thống:** 
  - Khóa/Mở khóa hoạt động của các tài khoản công dân/cán bộ.
  - Quản lý danh sách các cơ quan chính quyền liên kết.
  - Xem thống kê điểm uy tín hài lòng trung bình (Ratings) của từng đơn vị cơ quan.

---

## 🚀 Các Tính Năng Công Nghệ Nâng Cao (Highlight Features)

| Tính năng | Giải pháp công nghệ | Ý nghĩa thực tiễn |
| :--- | :--- | :--- |
| **Bản đồ nhiệt (Heatmap Layer)** | Thư viện `Leaflet.heat` | Hiển thị trực quan mật độ sự cố trên bản đồ, giúp chính quyền nhận biết nhanh các "điểm nóng" cần ưu tiên cải tạo hạ tầng. |
| **Điểm ưu tiên tự động (Priority Score)** | Thuật toán tính điểm nền (Hosted Service) | Tự động tính điểm khẩn cấp dựa trên: Số lượng upvote + Thời gian trôi qua + Mức độ nghiêm trọng, tránh tình trạng phản ánh bị trôi hoặc bỏ sót. |
| **Nhắc nhở quá hạn (Deadline Check)** | Background Job định kỳ | Tự động quét thời hạn giải quyết dựa trên cấp cơ quan và gửi cảnh báo quá hạn xử lý để duy trì trách nhiệm giải trình. |
| **Xử lý ảnh HEIC thông minh** | Javascript `heic2any` | Giúp người dùng iOS (iPhone/iPad) chụp ảnh hiện trường gửi trực tiếp mà không cần qua khâu chuyển đổi thủ công phức tạp. |
| **Đẩy dữ liệu Realtime** | ASP.NET Core SignalR | Hiển thị thông báo tức thời cho người dùng khi phản ánh được tiếp nhận/giải quyết và cập nhật thanh tiến độ quyên góp ngay lập tức. |

---

## 🔑 Tài Khoản Thử Nghiệm Hệ Thống (Demo Accounts)
*Mật khẩu đăng nhập chung cho tất cả các tài khoản thử nghiệm:* **`Admin@123456`**

1. **Quản trị viên (Admin):** `admin@gmail.com`
2. **Cán bộ Phường (Official Ward):** `canbo.phuong@gmail.com`
3. **Cán bộ Quận (Official District):** `canbo.quan@gmail.com`
4. **Công dân (Citizen):** `citizen@gmail.com`
