using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HRWebApp.Models;

namespace HRWebApp.Controllers
{
    public class EmployeesApiController : Controller
    {
        [HttpGet]
        public JsonResult GetIntegratedData()
        {
            // Khởi tạo danh sách trống để tránh lỗi null khi trả về
            var resultList = new List<object>();

            try
            {
                using (var hrDb = new HRDB())
                {
                    // Tắt Proxy để tránh lỗi vòng lặp dữ liệu
                    hrDb.Configuration.ProxyCreationEnabled = false;

                    // CHỈ LẤY 100 DÒNG (Để tránh lỗi Command Definition do dữ liệu quá lớn)
                    var hrData = hrDb.Personals.AsNoTracking()
                                     .OrderBy(p => p.Employee_ID)
                                     .Take(100)
                                     .ToList();

                    foreach (var h in hrData)
                    {
                        resultList.Add(new
                        {
                            id = h.Employee_ID,
                            fullName = (h.First_Name + " " + h.Last_Name).Trim(),
                            gender = h.Gender == true ? "Male" : "Female",
                            ethnicity = h.Ethnicity,
                            salary = 0,             // Tạm thời để 0 vì MySQL đang lỗi
                            vacation = 15,
                            status = "HR System Only"
                        });
                    }
                }
                // Trả về dữ liệu HR thành công
                return Json(new { data = resultList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Nếu vẫn lỗi, trả về thông báo lỗi cụ thể để xử lý
                return Json(new { data = new List<object>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}