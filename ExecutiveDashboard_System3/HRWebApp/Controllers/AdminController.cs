using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HRWebApp.Models;
using System.Data.Entity;
using System3_Integration;

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        // ============================================================
        // 1. TRANG CHỦ DASHBOARD
        // ============================================================
        public ActionResult Index(IEnumerable<EmployeeViewModel> filteredList = null)
        {
            using (var db = new HRDB())
            {
                // Nếu có danh sách lọc từ các nút Alert thì dùng, không thì để trống (DataTable sẽ nạp qua Ajax sau)
                List<EmployeeViewModel> displayList = filteredList?.ToList() ?? new List<EmployeeViewModel>();

                try
                {
                    // 1. Tổng nhân sự (Chỉ lấy Count nên rất nhanh)
                    var totalUsers = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees").FirstOrDefault();
                    ViewBag.TotalUsers = totalUsers;

                    // 2. Tổng quỹ lương thực tế
                    var totalSalaryValue = db.Database.SqlQuery<decimal?>("SELECT SUM(Salary) FROM SyncEmployees").FirstOrDefault() ?? 0;

                    if (totalSalaryValue >= 1000000000000m)
                        ViewBag.TotalSalary = String.Format("{0:0.##}T $", totalSalaryValue / 1000000000000m);
                    else if (totalSalaryValue >= 1000000000m)
                        ViewBag.TotalSalary = String.Format("{0:0.##}B $", totalSalaryValue / 1000000000m);
                    else
                        ViewBag.TotalSalary = String.Format("{0:N0} $", totalSalaryValue);

                    // 3. NGHIỆP VỤ CEO: Tính toán các thông số Alert
                    ViewBag.OverVacationCount = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees WHERE Vacation_Days > 25").FirstOrDefault();
                    ViewBag.BirthdayCount = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees WHERE Employee_ID IN (SELECT Employee_ID FROM Personal WHERE MONTH(BirthDate) = MONTH(GETDATE()))").FirstOrDefault();
                    ViewBag.AnniversaryCount = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees WHERE Employee_ID IN (SELECT Employee_ID FROM Employment WHERE MONTH(Hire_Date) = MONTH(GETDATE()))").FirstOrDefault();

                    // 4. Vacation Liability
                    var totalVacation = db.Database.SqlQuery<int?>("SELECT SUM(Vacation_Days) FROM SyncEmployees").FirstOrDefault() ?? 0;
                    ViewBag.TotalVacationDays = totalVacation;

                    ViewBag.AvgBenefits = "5,250";
                }
                catch (Exception ex)
                {
                    ViewBag.TotalSalary = "$0 (Offline)";
                    TempData["Error"] = "Lỗi kết nối CSDL: " + ex.Message;
                }

                ViewBag.ChartData = "[{ label: 'White', data: 40 }, { label: 'Asian', data: 30 }, { label: 'Black', data: 20 }, { label: 'Other', data: 10 }]";

                return View(displayList);
            }
        }

        // ============================================================
        // 2. API LẤY DỮ LIỆU PHÂN TRANG (SERVER-SIDE PAGINATION)
        // Lấy đúng 20 bản ghi mỗi lần để tránh treo máy
        // ============================================================
        [HttpPost]
        public ActionResult GetEmployeesData()
        {
            // Đọc tham số từ DataTable gửi lên
            int draw = Convert.ToInt32(Request.Form["draw"] ?? "1");
            int start = Convert.ToInt32(Request.Form["start"] ?? "0");
            int length = 20; // Nghiệp vụ của Tứ: Ép lấy đúng 20 bản ghi

            // Nhận tham số lọc từ các nút bấm Alert (nếu có)
            string filterType = Request.Form["filterType"] ?? "";

            using (var db = new HRDB())
            {
                try
                {
                    // Xây dựng điều kiện lọc SQL
                    string whereClause = " WHERE 1=1 ";
                    if (filterType == "Vacation") whereClause += " AND Vacation_Days > 25 ";
                    else if (filterType == "Birthday") whereClause += " AND Employee_ID IN (SELECT Employee_ID FROM Personal WHERE MONTH(BirthDate) = MONTH(GETDATE())) ";
                    else if (filterType == "Anniversary") whereClause += " AND Employee_ID IN (SELECT Employee_ID FROM Employment WHERE MONTH(Hire_Date) = MONTH(GETDATE())) ";

                    // Đếm tổng số bản ghi sau khi lọc
                    int totalRecords = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees").FirstOrDefault();
                    int filteredRecords = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees" + whereClause).FirstOrDefault();

                    // Truy vấn "Cắt lát" 20 bản ghi dùng OFFSET - FETCH
                    string sql = $@"
                        SELECT 
                            Employee_ID AS id, 
                            First_Name + ' ' + Last_Name AS fullName, 
                            Ethnicity AS ethnicity,
                            Gender AS gender,
                            Salary AS salaryInSql, 
                            Vacation_Days AS vacationDays,
                            Employment_Status AS status
                        FROM SyncEmployees
                        {whereClause}
                        ORDER BY Employee_ID ASC
                        OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY";

                    var data = db.Database.SqlQuery<HrEmployeeDto>(sql, start, length).ToList();

                    return Json(new
                    {
                        draw = draw,
                        recordsTotal = totalRecords,
                        recordsFiltered = filteredRecords,
                        data = data
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { error = "Lỗi nạp dữ liệu: " + ex.Message });
                }
            }
        }

        // ============================================================
        // 3. CÁC HÀM XỬ LÝ ALERT
        // ============================================================
        public ActionResult AlertAnniversary()
        {
            ViewBag.ReportType = "Anniversary";
            return Index();
        }

        public ActionResult AlertVacation()
        {
            ViewBag.ReportType = "Vacation";
            return Index();
        }

        public ActionResult AlertBirthday()
        {
            ViewBag.ReportType = "Birthday";
            return Index();
        }

        // ============================================================
        // 4. NÚT BẤM FETCH & SYNC DATA (AJAX)
        // ============================================================
        [HttpPost]
        public ActionResult SeedData()
        {
            try
            {
                var syncService = new EmployeeSyncService();
                syncService.SyncAllEmployees();
                return Json(new { success = true, message = "Đồng bộ dữ liệu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Thất bại: " + ex.Message });
            }
        }
    }
}