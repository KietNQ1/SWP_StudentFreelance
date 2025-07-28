# StudentFreelance

## Hướng dẫn cấu hình và chạy dự án

### 1. Tạo file cấu hình `appsettings.json`

Tại thư mục gốc dự án, tạo file `appsettings.json` với nội dung mẫu như sau:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StudentFreelanceDb;User Id=your_user;Password=your_password;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "StudentFreelance",
    "SenderEmail": "your_email@gmail.com",
    "SenderPassword": "your_email_password"
  },
  "PayOS": {
    "ClientId": "your_payos_client_id",
    "ApiKey": "your_payos_api_key",
    "ChecksumKey": "your_payos_checksum_key"
  }
}
```

**Lưu ý:**
- Thay các giá trị `your_user`, `your_password`, `your_email@gmail.com`, `your_email_password`, `your_payos_client_id`, ... bằng thông tin thực tế của bạn.
- Nếu dùng SQL Server Express, chuỗi kết nối có thể là: `Server=localhost\\SQLEXPRESS;Database=StudentFreelanceDb;Trusted_Connection=True;`

### 2. Quản lý database với Entity Framework (dotnet ef)

#### a) Tạo migration mới

Khi bạn thay đổi model hoặc muốn tạo migration mới:
```sh
dotnet ef migrations add TenMigrationMoi
```
Ví dụ:
```sh
dotnet ef migrations add InitDb
```

#### b) Cập nhật database theo migration mới nhất

```sh
dotnet ef database update
```

#### c) Xem danh sách migration đã có
```sh
dotnet ef migrations list
```

> **Lưu ý:**
> - Đảm bảo đã cài đặt Entity Framework CLI: `dotnet tool install --global dotnet-ef`
> - Nếu dùng Visual Studio, có thể chạy các lệnh này trong Package Manager Console hoặc terminal.
> - Nếu gặp lỗi về kết nối, kiểm tra lại chuỗi kết nối trong `appsettings.json`.

### 2.1. Thiết lập database mới hoàn toàn (dành cho lần đầu clone về hoặc reset dữ liệu)

Nếu bạn vừa clone dự án từ git hoặc muốn tạo lại database sạch:

```sh
dotnet ef migrations add Create
# (Chỉ cần nếu chưa có migration đầu tiên, nếu đã có thì bỏ qua)

dotnet ef database drop
# Xác nhận bằng 'y' nếu được hỏi

dotnet ef database update
```

Sau đó chạy dự án:
```sh
dotnet run
```

> **Lưu ý:**
> - Nếu migration đầu tiên đã tồn tại, chỉ cần chạy `dotnet ef database drop` rồi `dotnet ef database update`.
> - Nếu bạn muốn giữ dữ liệu cũ, KHÔNG chạy lệnh drop.

### 3. Chạy dự án

```sh
dotnet run
```

Truy cập: [http://localhost:5000](http://localhost:5000) hoặc cổng được hiển thị trên terminal.

---

## Thông tin thêm
- Nếu cần cấu hình môi trường phát triển, có thể chỉnh sửa file `appsettings.Development.json` tương tự.
- Để gửi email, cần bật "App Password" hoặc "Less secure app access" cho tài khoản Gmail.
- Để tích hợp PayOS, đăng ký tài khoản và lấy thông tin tại [PayOS](https://payos.vn/). 