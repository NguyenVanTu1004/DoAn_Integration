using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Data.SqlClient;
using HRWebApp.Models;
using HRWebApp.Scripts;
using System.Web.Script.Serialization;
using System.Linq; // Thêm để dùng Sum, Select nếu cần

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        string connectionString = "Server=.;Database=HR;User Id=sa;Password=a123456*;";

        // ==========================================================
        // API XUẤT TOÀN BỘ DỮ LIỆU JSON (DÀNH CHO HỆ THỐNG 3 ĐỐI SOÁT)
        // URL: /Admin/GetFullJson
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
                    // Truy vấn lấy sạch dữ liệu từ 2 bảng gốc của SQL Server
                    string sql = @"SELECT p.Employee_ID, p.First_Name, p.Last_Name, p.Ethnicity, 
                                   e.Vacation_Days, e.Salary 
                                   FROM Personal p 
                                   LEFT JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                                   ORDER BY p.Employee_ID ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        // Tăng timeout lên 180 giây để không bị ngắt quãng khi đọc 500k dòng
                        cmd.CommandTimeout = 180;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allEmployees.Add(new
                                {
                                    id = Convert.ToInt32(reader["Employee_ID"]),
                                    fullName = (reader["First_Name"].ToString() + " " + reader["Last_Name"].ToString()).Trim(),
                                    ethnicity = reader["Ethnicity"].ToString(),
                                    vacationDays = reader["Vacation_Days"] != DBNull.Value ? Convert.ToDouble(reader["Vacation_Days"]) : 0,
                                    salaryInSql = reader["Salary"] != DBNull.Value ? Convert.ToDecimal(reader["Salary"]) : 0
                                });
                            }
                        }
                    }
                }

                // Mở rộng giới hạn độ dài JSON để không bị lỗi khi dữ liệu quá lớn
                var jsonResult = Json(allEmployees, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void LoadDashboardStats(SqlConnection conn)
        {
            string sqlTotalUsers = "SELECT COUNT(*) FROM Personal";
            var total = new SqlCommand(sqlTotalUsers, conn).ExecuteScalar();
            ViewBag.TotalUsers = total != null ? Convert.ToInt32(total) : 0;

            string sqlTotalSalary = "SELECT SUM(Salary) FROM Employment";
            var salary = new SqlCommand(sqlTotalSalary, conn).ExecuteScalar();
            ViewBag.TotalSalary = (salary != DBNull.Value && salary != null)
                                  ? String.Format("{0:N0}", salary)
                                  : "0";
        }

        public ActionResult Index()
        {
            var employees = new List<EmployeeViewModel>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    LoadDashboardStats(conn);

                    string sqlList = @"SELECT TOP 100 p.Employee_ID, p.First_Name, p.Last_Name, p.Gender, 
                                       p.Shareholder_Status, p.Ethnicity, e.Salary, e.Hire_Date 
                                       FROM Personal p INNER JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                                       ORDER BY p.Employee_ID DESC";

                    using (SqlDataReader reader = new SqlCommand(sqlList, conn).ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(MapEmployee(reader));
                        }
                    }
                }
            }
            catch (Exception ex) { TempData["Message"] = "Lỗi: " + ex.Message; }
            return View(employees);
        }

        public ActionResult AlertVacation()
        {
            var employees = new List<EmployeeViewModel>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var vacChart = new List<object>();
                string sqlChart = "SELECT Ethnicity, COUNT(*) as Total FROM Personal p JOIN Employment e ON p.Employee_ID = e.Employee_ID WHERE e.Vacation_Days > 25 GROUP BY Ethnicity";
                using (SqlDataReader r = new SqlCommand(sqlChart, conn).ExecuteReader())
                {
                    while (r.Read()) { vacChart.Add(new { label = r["Ethnicity"].ToString(), data = r["Total"] }); }
                }
                ViewBag.ChartData = new JavaScriptSerializer().Serialize(vacChart);
                ViewBag.ReportType = "Vacation";
                LoadDashboardStats(conn);

                string sql = @"SELECT p.*, e.Vacation_Days, e.Salary, e.Hire_Date 
                               FROM Personal p JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                               WHERE e.Vacation_Days > 25 ORDER BY e.Vacation_Days DESC";

                using (SqlDataReader reader = new SqlCommand(sql, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var emp = MapEmployee(reader);
                        emp.Ethnicity = reader["Vacation_Days"].ToString();
                        employees.Add(emp);
                    }
                }
            }
            ViewBag.StatusMessage = "CẢNH BÁO: Nhân viên nghỉ phép trên 25 ngày";
            return View("Index", employees);
        }

        public ActionResult AlertBirthday()
        {
            var employees = new List<EmployeeViewModel>();
            var chartData = new List<object>();
            int currentMonth = DateTime.Now.Month;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                LoadDashboardStats(conn);

                string sqlList = @"SELECT p.*, e.Salary, e.Hire_Date FROM Personal p 
                                   JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                                   WHERE MONTH(p.BirthDate) = @Month ORDER BY DAY(p.BirthDate) ASC";
                SqlCommand cmdList = new SqlCommand(sqlList, conn);
                cmdList.Parameters.AddWithValue("@Month", currentMonth);
                using (SqlDataReader reader = cmdList.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var emp = MapEmployee(reader);
                        emp.HireDate = Convert.ToDateTime(reader["BirthDate"]).ToString("dd/MM/yyyy");
                        employees.Add(emp);
                    }
                }

                string sqlChart = @"SELECT DAY(BirthDate) as Day, COUNT(*) as Total 
                                    FROM Personal WHERE MONTH(BirthDate) = @Month 
                                    GROUP BY DAY(BirthDate) ORDER BY DAY(BirthDate)";
                SqlCommand cmdChart = new SqlCommand(sqlChart, conn);
                cmdChart.Parameters.AddWithValue("@Month", currentMonth);
                using (SqlDataReader r = cmdChart.ExecuteReader())
                {
                    while (r.Read())
                    {
                        chartData.Add(new { label = "Ngày " + r["Day"], data = r["Total"] });
                    }
                }
            }
            ViewBag.ChartData = new JavaScriptSerializer().Serialize(chartData);
            ViewBag.ChartTitle = "Mật độ sinh nhật tháng " + currentMonth;
            ViewBag.ReportType = "Birthday";
            return View("Index", employees);
        }

        public ActionResult AlertAnniversary()
        {
            var employees = new List<EmployeeViewModel>();
            var chartData = new List<object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                LoadDashboardStats(conn);
                string sql = @"SELECT p.*, e.* FROM Personal p JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                               WHERE DATEDIFF(day, GETDATE(), DATEADD(year, DATEDIFF(year, e.Hire_Date, GETDATE()), e.Hire_Date)) BETWEEN 0 AND 30";
                using (SqlDataReader reader = new SqlCommand(sql, conn).ExecuteReader())
                {
                    while (reader.Read()) { employees.Add(MapEmployee(reader)); }
                }
                string sqlChart = @"SELECT DATEDIFF(YEAR, Hire_Date, GETDATE()) as Years, COUNT(*) as Total 
                                    FROM Employment GROUP BY DATEDIFF(YEAR, Hire_Date, GETDATE()) ORDER BY Years";
                using (SqlDataReader r = new SqlCommand(sqlChart, conn).ExecuteReader())
                {
                    while (r.Read())
                    {
                        chartData.Add(new { label = r["Years"] + " năm", data = r["Total"] });
                    }
                }
            }
            ViewBag.ChartData = new JavaScriptSerializer().Serialize(chartData);
            ViewBag.ReportType = "Anniversary";
            ViewBag.StatusMessage = "Kỷ niệm ngày vào làm trong 30 ngày tới";
            return View("Index", employees);
        }

        private EmployeeViewModel MapEmployee(SqlDataReader reader)
        {
            return new EmployeeViewModel
            {
                ID = reader["Employee_ID"].ToString(),
                FullName = reader["First_Name"].ToString() + " " + reader["Last_Name"].ToString(),
                Gender = reader["Gender"].ToString() == "1" ? "Male" : "Female",
                Shareholder = reader["Shareholder_Status"].ToString() == "1" ? "Yes" : "No",
                Ethnicity = reader["Ethnicity"].ToString(),
                Salary = reader["Salary"] != DBNull.Value ? Convert.ToDecimal(reader["Salary"]) : 0,
                HireDate = reader["Hire_Date"] != DBNull.Value ? Convert.ToDateTime(reader["Hire_Date"]).ToString("dd/MM/yyyy") : "N/A"
            };
        }

        public ActionResult SeedData()
        {
            try
            {
                DataSeeder.SeedDataTool(500000);
                TempData["Message"] = "Đã nạp thêm 500k dòng dữ liệu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi nạp dữ liệu: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}