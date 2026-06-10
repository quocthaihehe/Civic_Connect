# Cẩm Nang Làm Việc Nhóm Với Git - Dự Án CivicConnect

Tài liệu này cung cấp các lệnh Git chuẩn để các thành viên trong nhóm thao tác trên dự án một cách an toàn và đồng bộ.

## 1. Clone dự án (Lấy code về máy)

Để tải toàn bộ dự án từ kho lưu trữ về máy cục bộ, sử dụng lệnh sau:

```bash
git clone https://github.com/quocthaihehe/Civic_Connect.git
```

Sau khi clone, hãy di chuyển vào thư mục chứa dự án (nơi có file `.sln`):
```bash
cd Civic_Connect/CivicConnect
```

## 2. Vào nhánh làm việc

Kiểm tra danh sách các nhánh hiện có trong dự án:
```bash
git branch -a
```

Di chuyển vào nhánh làm việc chung của nhóm `feature/setup-core`:
```bash
git checkout feature/setup-core
```
*(Lưu ý: Trên các phiên bản Git mới, bạn cũng có thể dùng lệnh `git switch feature/setup-core`)*

## 3. Kéo code (Pull)

**Quan trọng:** Trước khi bắt đầu viết code mới, luôn nhớ cập nhật code mới nhất từ nhánh `feature/setup-core` trên GitHub về máy để tránh xung đột (conflict):

```bash
git pull origin feature/setup-core
```

## 4. Đẩy code (Push)

Sau khi hoàn thành tính năng và muốn đẩy code của bạn lên nhánh `feature/setup-core` trên GitHub, hãy thực hiện lần lượt các lệnh sau:

Bước 1: Thêm tất cả các thay đổi vào khu vực chờ (staging area)
```bash
git add .
```

Bước 2: Tạo commit với thông điệp rõ ràng, mô tả những gì bạn đã làm
```bash
git commit -m "Mô tả ngắn gọn nhưng đầy đủ về những thay đổi của bạn"
```

Bước 3: Đẩy code lên nhánh trên GitHub
```bash
git push origin feature/setup-core
```
