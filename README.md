📊 Strategic HR & Payroll System - Full Integration
Hệ thống tích hợp đối soát nhân sự & lương đa nền tảng (Spring Boot, Node.js, .NET MVC)

Hệ thống được xây dựng để giải quyết bài toán đối soát dữ liệu quy mô lớn (Big Data handling) giữa hệ thống Quản lý nhân sự (HR) và hệ thống Tính lương (Payroll), cung cấp Dashboard điều hành thời gian thực cho CEO.

📂 1. Kiến trúc Hệ thống (System Architecture)
Dự án là một hệ sinh thái gồm 3 thành phần chính hoạt động phối hợp:

Hệ thống 1: Payroll Core (Node.js & MySQL)

Thư mục: API_MySQL_NodeJS

Vai trò: Quản lý dữ liệu lương gốc. Cung cấp RESTful API cho hệ thống đối soát.

Hệ thống 2: HR Management (ASP.NET MVC & SQL Server)

Thư mục: HRWebApp

Vai trò: Quản lý hồ sơ nhân sự, thông tin cá nhân và hợp đồng lao động.

Hệ thống 3: Executive Dashboard & Integration Layer (.NET Framework)

Thư mục: ExecutiveDashboard_System3

Vai trò: "Bộ não" của dự án. Thực hiện Data Reconciliation (Đối soát), xử lý 500.000 dòng dữ liệu bằng kỹ thuật tối ưu hiệu suất và hiển thị biểu đồ phân tích.

🚀 2. Hướng dẫn Cài đặt & Cấu hình
🟢 Bước 1: Cấu hình API Payroll (Node.js)
Truy cập thư mục API_MySQL_NodeJS.

Chạy lệnh: npm install để cài đặt thư viện.

Cấu hình file server.js: Cập nhật user và password MySQL của máy bạn.

Khởi động: node server.js (Mặc định chạy tại cổng 3000).

🔵 Bước 2: Cấu hình HR & Dashboard (Visual Studio)
Mở file Solution (.sln) bằng Visual Studio.

NuGet Restore: Chuột phải vào Solution -> Restore NuGet Packages.

Web.config: Cập nhật connectionString trong cả 2 project HRWebApp và ExecutiveDashboard_System3 để kết nối đúng SQL Server cục bộ.

🗄️ Bước 3: Thiết lập Cơ sở dữ liệu
SQL Server: Tạo DB tên HR. Chạy script HR_Full_Data.sql. (Lưu ý: File nặng ~732MB, hãy đảm bảo ổ cứng còn trống).

MySQL: Chạy script tạo bảng trong thư mục API để khởi tạo dữ liệu lương.

🖥️ 3. Chi tiết Hệ thống thứ 3: Executive Dashboard (Trọng tâm)
Đây là phần quan trọng nhất để trình bày với CEO. Hệ thống thực hiện các tác vụ kỹ thuật cao:

⚙️ Chức năng cốt lõi:
Data Synchronization: Sử dụng thư viện SignalR để cập nhật tiến độ đối soát lên màn hình theo thời gian thực (Real-time Progress Bar).

High-Performance Auditing: Áp dụng thuật toán so sánh song song để đối soát 500.000 bản ghi giữa SQL Server và API Node.js mà không gây treo trình duyệt.

CEO Analytics: Hiển thị tổng quỹ lương, biến động nhân sự và tỷ lệ sai lệch dữ liệu qua biểu đồ trực quan.

🛠 Quy trình vận hành đối soát:
Trigger Audit: Nhấn nút "EXECUTE SYSTEM AUDIT".

Processing: Hệ thống sẽ gửi yêu cầu lấy dữ liệu từ API (Cổng 3000), sau đó so khớp với bảng Employment trong SQL Server dựa trên Employee_ID.

Visualization: * Màu xanh (Verified): Dữ liệu khớp 100%.

Màu đỏ (Discrepancy): Cảnh báo sai lệch lương hoặc thiếu hụt hồ sơ.

🕹️ 4. Kế hoạch Kiểm thử (Test Plan)
Để đảm bảo hệ thống không bị lỗi kết nối, hãy tuân thủ thứ tự:

Bật MySQL & SQL Server.

Chạy API Node.js (node server.js).

Nhấn F5 chạy Dashboard từ Visual Studio.

Kiểm tra: Truy cập http://localhost:44300/ và thực hiện đồng bộ dữ liệu. Nếu thấy thông báo "Audit Completed" là thành công.

⚠️ Lưu ý về bảo mật & hiệu suất
Hệ thống sử dụng BCrypt để mã hóa mật khẩu trong bảng UserAccount.

Sử dụng Pagination (Phân trang) khi hiển thị danh sách 500.000 nhân viên để tối ưu bộ nhớ RAM của trình duyệt.

Dự án thực hiện bởi: Tứ, Thiện, Lâm (Software Engineering Student - DTU)
