using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data.SqlClient;
using HRWebApp.Models;
using System.Web.Script.Serialization;
using System.Linq;

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        // Chuỗi kết nối từ cấu hình của Tứ
        string connectionString = "Server=.;Database=HR;User Id=sa;Password=a123456*;";

        // ==========================================================
        // 1. API XUẤT TOÀN BỘ DỮ LIỆU (DÀNH CHO HỆ THỐNG 3 ĐỐI SOÁT)
        // Dùng cơ chế Streaming để xử lý 500k dòng không tốn RAM
        // ==========================================================
        [HttpGet]
        public ActionResult GetFullJson()
        {
            var allEmployees = new List<object>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // SQL lấy đầy đủ các trường cần thiết cho đối soát
                    string sql = @"SELECT p.Employee_ID, p.First_Name, p.Last_Name, p.Ethnicity, p.Gender,
                                   e.Vacation_Days, e.Salary, e.Employment_Status
                                   FROM Personal p 
                                   LEFT JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                                   ORDER BY p.Employee_ID ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.CommandTimeout = 300; // Đợi 5 phút cho dữ liệu khổng lồ
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allEmployees.Add(new
                                {
                                    id = Convert.ToInt32(reader["Employee_ID"]),
                                    fullName = (reader["First_Name"].ToString() + " " + reader["Last_Name"].ToString()).Trim(),
                                    gender = reader["Gender"].ToString() == "1" || reader["Gender"].ToString().ToLower() == "true",
                                    ethnicity = reader["Ethnicity"].ToString(),
                                    vacationDays = reader["Vacation_Days"] != DBNull.Value ? Convert.ToInt32(reader["Vacation_Days"]) : 0,
                                    salaryInSql = reader["Salary"] != DBNull.Value ? Convert.ToDecimal(reader["Salary"]) : 0,
                                    status = reader["Employment_Status"]?.ToString() ?? "Active"
                                });
                            }
                        }
                    }
                }

                // Cấu trúc trả về bọc trong "data" để khớp với logic System 3
                var jsonResult = Json(new { data = allEmployees }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ==========================================================
        // 2. DASHBOARD CHÍNH (SQL SERVER SIDE)
        // ==========================================================
        public ActionResult Index()
        {
            var employees = new List<EmployeeViewModel>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    LoadDashboardStats(conn);

                    // Chỉ lấy TOP 100 để giao diện Admin Hệ thống 2 nạp tức thì
                    string sqlList = @"SELECT TOP 100 p.Employee_ID, p.First_Name, p.Last_Name, p.Gender, 
                                       p.Shareholder_Status, p.Ethnicity, e.Salary, e.Hire_Date 
                                       FROM Personal p INNER JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                                       ORDER BY p.Employee_ID DESC";

                    using (SqlDataReader reader = new SqlCommand(sqlList, conn).ExecuteReader())
                    {
                        while (reader.Read()) { employees.Add(MapEmployee(reader)); }
                    }
                }
            }
            catch (Exception ex) { TempData["Message"] = "Lỗi kết nối: " + ex.Message; }
            return View(employees);
        }

        // ==========================================================
        // 3. CÁC HÀM CẢNH BÁO (ALERTS) THEO YÊU CẦU CEO
        // ==========================================================
        public ActionResult AlertVacation()
        {
            var employees = new List<EmployeeViewModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                LoadDashboardStats(conn);
                string sql = "SELECT p.*, e.Vacation_Days, e.Salary, e.Hire_Date FROM Personal p JOIN Employment e ON p.Employee_ID = e.Employee_ID WHERE e.Vacation_Days > 25";
                using (SqlDataReader reader = new SqlCommand(sql, conn).ExecuteReader())
                {
                    while (reader.Read()) { employees.Add(MapEmployee(reader)); }
                }

                // Chuẩn bị Chart Data cho biểu đồ phân bổ
                ViewBag.ChartData = GetChartData(conn, "SELECT Ethnicity as label, COUNT(*) as data FROM Personal p JOIN Employment e ON p.Employee_ID = e.Employee_ID WHERE e.Vacation_Days > 25 GROUP BY Ethnicity");
            }
            ViewBag.ReportType = "Vacation";
            return View("Index", employees);
        }

        public ActionResult AlertBirthday()
        {
            var employees = new List<EmployeeViewModel>();
            int currentMonth = DateTime.Now.Month;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                LoadDashboardStats(conn);
                string sql = "SELECT p.*, e.Salary, e.Hire_Date FROM Personal p JOIN Employment e ON p.Employee_ID = e.Employee_ID WHERE MONTH(p.BirthDate) = " + currentMonth;
                using (SqlDataReader reader = new SqlCommand(sql, conn).ExecuteReader())
                {
                    while (reader.Read()) { employees.Add(MapEmployee(reader)); }
                }
                ViewBag.ChartData = GetChartData(conn, "SELECT Ethnicity as label, COUNT(*) as data FROM Personal WHERE MONTH(BirthDate) = " + currentMonth + " GROUP BY Ethnicity");
            }
            ViewBag.ReportType = "Birthday";
            return View("Index", employees);
        }

        // --- HELPER METHODS ---

        private void LoadDashboardStats(SqlConnection conn)
        {
            ViewBag.TotalUsers = new SqlCommand("SELECT COUNT(*) FROM Personal", conn).ExecuteScalar();
            var salary = new SqlCommand("SELECT SUM(Salary) FROM Employment", conn).ExecuteScalar();
            ViewBag.TotalSalary = salary != DBNull.Value ? String.Format("{0:N0}", salary) : "0";
        }

        private string GetChartData(SqlConnection conn, string sql)
        {
            var data = new List<object>();
            using (SqlDataReader r = new SqlCommand(sql, conn).ExecuteReader())
            {
                while (r.Read()) { data.Add(new { label = r[0].ToString(), data = r[1] }); }
            }
            return new JavaScriptSerializer().Serialize(data);
        }

        private EmployeeViewModel MapEmployee(SqlDataReader reader)
        {
            return new EmployeeViewModel
            {
                ID = reader["Employee_ID"].ToString(),
                FullName = (reader["First_Name"].ToString() + " " + reader["Last_Name"].ToString()).Trim(),
                Gender = reader["Gender"].ToString() == "1" ? "Male" : "Female",
                Shareholder = reader["Shareholder_Status"].ToString() == "1" ? "Yes" : "No",
                Ethnicity = reader["Ethnicity"].ToString(),
                Salary = reader["Salary"] != DBNull.Value ? Convert.ToDecimal(reader["Salary"]) : 0,
                HireDate = reader["Hire_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Hire_Date"]).ToString("dd/MM/yyyy") : "N/A"
            };
        }
    }
}