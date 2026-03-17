using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HRWebApp.Models;

namespace HRWebApp.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index(string searchString)
        {
            using (var db = new HRDB())
            {
                // Bước 1: Khởi tạo truy vấn và chọn đích danh các cột chắc chắn có
                // Sử dụng AsNoTracking() để EF không cần theo dõi sự thay đổi, giúp tải 500k dòng nhanh hơn
                var query = db.Personals.AsNoTracking()
                            .Join(db.Employments.AsNoTracking(),
                                  p => p.Employee_ID,
                                  e => e.Employee_ID,
                                  (p, e) => new
                                  {
                                      EmpID = p.Employee_ID,
                                      FName = p.First_Name,
                                      LName = p.Last_Name,
                                      Gen = p.Gender,
                                      Eth = p.Ethnicity,
                                      HDate = e.Hire_Date
                                  });

                // Bước 2: Lọc dữ liệu ngay tại SQL Server nếu có từ khóa tìm kiếm
                if (!String.IsNullOrEmpty(searchString))
                {
                    query = query.Where(x => x.FName.Contains(searchString)
                                          || x.LName.Contains(searchString));
                }

                // Bước 3: Tải toàn bộ 500.000 dòng về bộ nhớ RAM
                // OrderBy giúp dữ liệu sắp xếp ngăn nắp theo mã nhân viên
                var results = query.OrderBy(x => x.EmpID).Take(500000).ToList();

                // Bước 4: Chuyển đổi sang ViewModel để hiển thị ra View
                var model = results.Select(x => new EmployeeViewModel
                {
                    ID = (int)x.EmpID,
                    FullName = x.FName + " " + x.LName,
                    Gender = x.Gen == true ? "Male" : "Female",
                    Ethnicity = x.Eth,
                    Salary = 0, // Đang đợi tích hợp từ máy Lâm
                    Vacation_Days = 0,
                    Hire_Date = x.HDate
                }).ToList();

                // Gửi thông tin thống kê lên Dashboard
                ViewBag.TotalUsers = db.Personals.Count().ToString("N0");
                ViewBag.CurrentFilter = searchString;

                return View(model);
            }
        }
    }
}