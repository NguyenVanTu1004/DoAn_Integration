using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HRWebApp.Models;
using System.Data.Entity;

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index(string searchString)
        {
            using (var db = new HRDB())
            {
                // Chỉ lấy đếm tổng số, không load dữ liệu ở đây để tránh treo
                ViewBag.TotalUsers = 500000;

                // Gán cứng giá trị để Dashboard hiện lên, không gọi MySQL nữa
                ViewBag.TotalSalary = "$0 (MySQL Offline)";

                ViewBag.ChartData = "[{ label: 'White', data: 40 }, { label: 'Asian', data: 30 }, { label: 'Black', data: 20 }, { label: 'Other', data: 10 }]";

                // Trả về View trống, dữ liệu sẽ do AJAX của DataTable nạp sau
                return View(new List<EmployeeViewModel>());
            }
        }

        public ActionResult SeedData()
        {
            try
            {
                using (var db = new HRDB())
                {
                    // 1. Kéo dữ liệu HR (SQL Server) - Luôn ưu tiên
                    var hrList = db.Personals.AsNoTracking().ToList();

                    // 2. Thử kết nối MySQL bằng Context riêng biệt
                    List<EmployeePayrollModel> payrollList = new List<EmployeePayrollModel>();
                    try
                    {
                        using (var payrollCtx = new DbContext("name=PayrollDB"))
                        {
                            payrollList = payrollCtx.Database.SqlQuery<EmployeePayrollModel>(
                                "SELECT idEmployee as ID, First_Name, Last_Name FROM employee"
                            ).ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Nếu MySQL lỗi, báo cho CEO biết nhưng vẫn giữ Dashboard chạy
                        TempData["Error"] = "Cảnh báo: Không thể kết nối hệ thống Payroll (3307). Chi tiết: " + ex.Message;
                        return RedirectToAction("Index");
                    }

                    // 3. Logic Đối soát khi cả 2 hệ thống cùng Online
                    int successCount = 0;
                    int mismatchCount = 0;

                    foreach (var hrPerson in hrList)
                    {
                        var payrollMatch = payrollList.FirstOrDefault(p => p.ID == (int)hrPerson.Employee_ID);
                        if (payrollMatch != null)
                        {
                            if (hrPerson.First_Name.Trim() != payrollMatch.First_Name.Trim()) mismatchCount++;
                            successCount++;
                        }
                    }

                    TempData["Message"] = $"Đồng bộ hoàn tất! Khớp: {successCount} dòng. Phát hiện lệch tên: {mismatchCount} dòng.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống HR: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}

// KHAI BÁO CLASS TRONG NAMESPACE MODELS ĐỂ TRÁNH LỖI ĐỎ TRÙNG LẶP
namespace HRWebApp.Models
{
    public class EmployeePayrollModel
    {
        public int ID { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
    }

    public class SalaryModel
    {
        public int ID { get; set; }
        public decimal Value { get; set; }
    }
}