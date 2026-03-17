using System;
using System.Web.Mvc;
using HRWebApp.Scripts;
using System.Data.SqlClient;
using System.Globalization;
using System.Collections.Generic;
using HRWebApp.Models; // Đảm bảo nạp thư viện Model của bạn

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        private string connString = "Server=.;Database=HR;User Id=sa;Password=a123456*;";

        public ActionResult Index()
        {
            // Sử dụng kiểu EmployeeViewModel rõ ràng thay vì dynamic để tránh lỗi System.Object
            var employees = new List<EmployeeViewModel>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // 1. Lấy tổng số nhân viên
                    var cmdEmp = new SqlCommand("SELECT COUNT(*) FROM Personal", conn);
                    ViewBag.TotalEmployees = cmdEmp.ExecuteScalar();

                    // 2. Lấy tổng lương
                    var cmdSal = new SqlCommand("SELECT SUM(Salary) FROM Employment", conn);
                    var totalSalary = cmdSal.ExecuteScalar();
                    ViewBag.TotalSalary = (totalSalary == DBNull.Value) ? 0 : totalSalary;

                    // 3. Lấy danh sách 100 người gắn kết từ 2 bảng
                    string query = @"SELECT p.Employee_ID, p.First_Name, p.Last_Name, p.Gender, 
                                    p.Shareholder_Status, p.Ethnicity, e.Salary, e.Hire_Date 
                             FROM Personal p 
                             INNER JOIN Employment e ON p.Employee_ID = e.Employee_ID 
                             ORDER BY p.Employee_ID DESC";

                    var cmdList = new SqlCommand(query, conn);
                    using (SqlDataReader reader = cmdList.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string genderValue = reader["Gender"].ToString();
                            string shareholderValue = reader["Shareholder_Status"].ToString();

                            // Khởi tạo đối tượng theo đúng lớp EmployeeViewModel
                            employees.Add(new EmployeeViewModel
                            {
                                ID = reader["Employee_ID"].ToString(),
                                FullName = reader["First_Name"].ToString() + " " + reader["Last_Name"].ToString(),
                                Gender = (genderValue == "1") ? "Male" : "Female",
                                Shareholder = (shareholderValue == "1") ? "Yes" : "No",
                                Ethnicity = reader["Ethnicity"].ToString(),
                                Salary = reader["Salary"] != DBNull.Value ? Convert.ToDecimal(reader["Salary"]) : 0,
                                HireDate = reader["Hire_Date"] != DBNull.Value
                                           ? Convert.ToDateTime(reader["Hire_Date"]).ToString("dd/MM/yyyy")
                                           : "N/A"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi truy vấn: " + ex.Message;
                ViewBag.TotalEmployees = 0;
                ViewBag.TotalSalary = 0;
            }

            // Truyền danh sách employees đã định nghĩa kiểu rõ ràng vào View
            return View(employees);
        }

        public ActionResult SeedData()
        {
            try
            {
                DataSeeder.SeedDataTool(100);
                TempData["Message"] = "Admin Tool: Đã nạp thành công 100 bản ghi minh bạch vào cả hai hệ thống!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi hệ thống: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}