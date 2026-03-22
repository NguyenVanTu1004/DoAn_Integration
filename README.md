📊 Strategic HR & Payroll System - Full Integration
Hệ thống tích hợp đối soát nhân sự & lương đa nền tảng

Chào mừng team đã tham gia dự án! Đây là hệ thống đối soát nhân sự và lương tích hợp Dashboard điều hành dành cho CEO. Hệ thống được xây dựng trên mô hình 3 lớp tích hợp:

📂 1. Cấu trúc các Hệ thống (System Architecture)
Dựa trên sơ đồ thư mục, hệ thống được chia làm 3 thành phần chính:

Hệ thống 1: SpringApp (Payroll Core)

Thư mục: SpringApp / API_MySQL_NodeJS

Nhiệm vụ: Quản lý dữ liệu lương gốc trên MySQL. Chạy API Node.js để cung cấp dữ liệu đối soát.

Hệ thống 2: HR System (Personal & Employment)

Thư mục: HRWebApp

Nhiệm vụ: Quản lý hồ sơ nhân sự, ngày vào làm và thông tin cá nhân trên SQL Server.

Hệ thống 3: Executive Dashboard (Integration Layer)

Thư mục: ExecutiveDashboard_System3

Nhiệm vụ: "Trái tim" của dự án. Thực hiện logic đối soát (Reconciliation), xử lý 500.000 dòng dữ liệu và hiển thị biểu đồ phân tích cho CEO.

🚀 2. Hướng dẫn Cài đặt & Cấu hình
🟢 Bước 1: Cấu hình Hệ thống 1 (SpringApp - API Payroll)
Truy cập thư mục API_MySQL_NodeJS.

Mở Terminal/Command Prompt tại đây và chạy: npm install.

Mở file server.js, cập nhật thông tin user và password MySQL của máy bạn.

Khởi động API: node server.js (Mặc định chạy tại cổng 3000).

🔵 Bước 2: Cấu hình Hệ thống 2 & 3 (HR & Dashboard)
Mở file Solution (.sln) bằng Visual Studio.

Khôi phục thư viện: Vào Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution. Đảm bảo các gói Bogus (tạo dữ liệu giả) và Entity Framework đã được cài đặt.

Cấu hình kết nối: Mở file Web.config trong HRWebApp, sửa connectionString SQL Server cho khớp với máy bạn (Server=.; Database=HR;...).

Thiết lập cổng: Đảm bảo Project Url được đặt là http://localhost:44300/.

🗄️ 3. Triển khai Cơ sở dữ liệu (Database Setup)
Team cần nạp dữ liệu từ các file script đính kèm trong thư mục gốc:

SQL Server (HR System): Mở SSMS, tạo Database tên HR. Mở file HR_Full_Data.sql (dung lượng ~732MB) và nhấn F5 (Execute). Quá trình này sẽ nạp 500.000 bản ghi nhân sự.

MySQL (Payroll System): Chạy script tạo bảng lương tương ứng trong MySQL để API Node.js có dữ liệu đối soát.

🕹️ 4. Quy trình Vận hành & Kiểm thử (Test Plan)
Để hệ thống không bị lỗi kết nối giữa các nền tảng, hãy thực hiện theo đúng thứ tự:

Khởi động các Service: Bật MySQL Server và SQL Server.

Chạy API nguồn: Chạy lệnh - node server.js tại thư mục API.

Khởi chạy Dashboard: Nhấn F5 trong Visual Studio để chạy ExecutiveDashboard_System3.

Thực hiện Đối soát:

Tại màn hình Dashboard, nhấn nút "EXECUTE SYSTEM AUDIT".

Hệ thống sẽ gọi API từ SpringApp, so sánh với dữ liệu trong HR System.

Kết quả sẽ hiển thị nhãn "VERIFIED" màu xanh nếu lương khớp nhau 100%.
