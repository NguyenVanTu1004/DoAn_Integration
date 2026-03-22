using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HRWebApp.Models;
using System.Data.Entity;
using System3_Integration;
using System.Web.Script.Serialization;

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        // ============================================================
        // 1. TRANG CHỦ DASHBOARD
        // ============================================================
        public ActionResult Index()
        {
            // Tối ưu: Không truyền List vào View nữa vì đã có AJAX nạp 20 dòng một
            using (var db = new HRDB())
            {
                try
                {
                    // 1. Tổng nhân sự (Count nhanh)
                    var totalUsers = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees").FirstOrDefault();
                    ViewBag.TotalUsers = totalUsers;

                    // 2. Tổng quỹ lương thực tế (Định dạng tiền tệ thông minh)
                    var totalSalaryValue = db.Database.SqlQuery<decimal?>("SELECT SUM(Salary) FROM SyncEmployees").FirstOrDefault() ?? 0;

                    if (totalSalaryValue >= 1000000000m)
                        ViewBag.TotalSalary = String.Format("{0:0.##}B $", totalSalaryValue / 1000000000m);
                    else
                        ViewBag.TotalSalary = String.Format("{0:N0} $", totalSalaryValue);

                    // 3. NGHIỆP VỤ CEO: Các chỉ số Alert
                    ViewBag.OverVacationCount = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees WHERE Vacation_Days > 25").FirstOrDefault();

                    // Lọc tháng hiện tại từ DB (Tránh nạp cả bảng lên RAM)
                    int currentMonth = DateTime.Now.Month;
                    ViewBag.BirthdayCount = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM SyncEmployees WHERE Employee_ID IN (SELECT Employee_ID FROM Personal WHERE MONTH(BirthDate) = @p0)", currentMonth).FirstOrDefault();

                    ViewBag.AnniversaryCount = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM SyncEmployees WHERE Employee_ID IN (SELECT Employee_ID FROM Employment WHERE MONTH(Hire_Date) = @p0)", currentMonth).FirstOrDefault();

                    // 4. Vacation Liability
                    ViewBag.TotalVacationDays = db.Database.SqlQuery<int?>("SELECT SUM(Vacation_Days) FROM SyncEmployees").FirstOrDefault() ?? 0;

                    ViewBag.AvgBenefits = "5,250"; // Số liệu giả định theo yêu cầu

                    // Gán mặc định ReportType nếu không có từ các Action Alert
                    if (ViewBag.ReportType == null) ViewBag.ReportType = "All";
                }
                catch (Exception ex)
                {
                    ViewBag.TotalSalary = "$0 (Offline)";
                    TempData["Error"] = "Lỗi Dashboard: " + ex.Message;
                }

                return View();
            }
        }

        // ============================================================
        // 2. API LẤY DỮ LIỆU PHÂN TRANG (SERVER-SIDE PAGINATION)
        // Chìa khóa để load 500k bản ghi trong 0.2 giây
        // ============================================================
        [HttpPost]
        public ActionResult GetEmployeesData()
        {
            int draw = Convert.ToInt32(Request.Form["draw"] ?? "1");
            int start = Convert.ToInt32(Request.Form["start"] ?? "0");
            int length = Convert.ToInt32(Request.Form["length"] ?? "20");
            string filterType = Request.Form["filterType"] ?? "All";

            using (var db = new HRDB())
            {
                try
                {
                    string whereClause = " WHERE 1=1 ";
                    if (filterType == "Vacation") whereClause += " AND Vacation_Days > 25 ";
                    else if (filterType == "Birthday") whereClause += " AND Employee_ID IN (SELECT Employee_ID FROM Personal WHERE MONTH(BirthDate) = MONTH(GETDATE())) ";
                    else if (filterType == "Anniversary") whereClause += " AND Employee_ID IN (SELECT Employee_ID FROM Employment WHERE MONTH(Hire_Date) = MONTH(GETDATE())) ";

                    // Tổng số bản ghi (Toàn cục và sau khi lọc)
                    int totalRecords = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees").FirstOrDefault();
                    int filteredRecords = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM SyncEmployees" + whereClause).FirstOrDefault();

                    // TRUY VẤN CẮT LÁT (OFFSET/FETCH) - Chỉ lấy đúng 20 dòng
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
                    return Json(new { error = ex.Message });
                }
            }
        }

        // ============================================================
        // 3. CÁC HÀM XỬ LÝ ALERT (HƯỚNG CEO ĐẾN DASHBOARD CÙNG BỘ LỌC)
        // ============================================================
        public ActionResult AlertAnniversary() { ViewBag.ReportType = "Anniversary"; return Index(); }
        public ActionResult AlertVacation() { ViewBag.ReportType = "Vacation"; return Index(); }
        public ActionResult AlertBirthday() { ViewBag.ReportType = "Birthday"; return Index(); }

        // ============================================================
        // 4. NÚT BẤM FETCH & SYNC DATA (XỬ LÝ ĐỐI SOÁT HỆ THỐNG 3)
        // ============================================================
        [HttpPost]
        public ActionResult SeedData()
        {
            try
            {
                var syncService = new EmployeeSyncService();
                syncService.SyncAllEmployees(); // Gọi Service đối soát từ Hệ thống 3
                return Json(new { success = true, message = "Đã đối soát lương và đồng bộ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Thất bại: " + ex.Message });
            }
        }
    }
}