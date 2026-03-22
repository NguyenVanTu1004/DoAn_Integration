📊 Strategic HR & Payroll System - Full Integration
Chào mừng team đã tham gia dự án! Đây là hệ thống đối soát nhân sự và lương tích hợp Dashboard điều hành dành cho CEO. Để hệ thống chạy được hoàn hảo, mọi người vui lòng thực hiện theo các bước sau:

🛠 1. Yêu cầu hệ thống (Prerequisites)
Trước khi chạy, hãy đảm bảo máy bạn đã cài:

Visual Studio 2019/2022 (Có cài gói ASP.NET and web development).

Node.js (Phiên bản 16.x trở lên).

SQL Server (Để chạy HR System) & MySQL (Để chạy Payroll System).

Git (Để quản lý code).

🚀 2. Hướng dẫn cài đặt & Cấu hình thư viện
Hệ thống 1: API Payroll (Node.js - Cổng 3000)
Hệ thống này quản lý dữ liệu lương trên MySQL.

Mở Terminal tại thư mục API_MySQL_NodeJS.

Chạy lệnh: npm install để tự động cài các thư viện: express, mysql2, morgan, cors.

Mở file server.js, sửa lại thông tin user và password của MySQL máy bạn.

Chạy lệnh: node server.js để khởi động API.

Hệ thống 2: Data Integration (C# - Thư viện Bogus)
Đây là nơi xử lý logic đối soát và tạo dữ liệu ảo 500k dòng.

Mở Solution bằng Visual Studio.

Cài đặt Bogus: Nếu Visual Studio không tự nhận diện, hãy vào Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution.

Tìm và cài đặt/cập nhật gói Bogus (Thư viện tạo dữ liệu giả mà Tứ đã dùng).

Nhấn Rebuild Solution để đảm bảo các DLL được nạp đúng.

Hệ thống 3: HRWebApp Dashboard (ASP.NET MVC - Cổng 44300)
Giao diện hiển thị Dashboard và kết quả đối soát.

Kiểm tra file Web.config: Sửa connectionString SQL Server cho khớp với máy cá nhân.

Đảm bảo cổng chạy là 44300 (Vào Project Properties -> Web -> Project Url: http://localhost:44300/).

🗄️ 3. Cấu hình Database
Team cần chạy file Script SQL đính kèm để tạo cấu trúc bảng:

SQL Server: Chạy script tạo bảng Personal, Employment, Benefits và bảng kết quả SyncEmployees.

MySQL: Chạy script tạo bảng lương tương ứng.

🕹️ 4. Quy trình chạy Test hệ thống
Để không bị lỗi 404 hoặc lỗi kết nối, hãy chạy theo thứ tự:

Bật MySQL & SQL Server.

Chạy Node.js API (Terminal: node server.js).

Chạy HRWebApp (Nhấn F5 trong Visual Studio).

Truy cập: http://localhost:44300/Admin/Index.

Nhấn "RUN FULL RECONCILIATION" để bắt đầu đối soát 500.000 dòng dữ liệu.
