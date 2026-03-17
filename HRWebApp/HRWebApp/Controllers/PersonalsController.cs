using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using HRWebApp.Models;

namespace HRWebApp.Controllers
{
    public class PersonalsController : Controller
    {
        private HRDB db = new HRDB();

        // GET: Personals
        public ActionResult Index(string reportType = "Default")
        {
            // 1. Xử lý AJAX từ DataTables (Dành cho tìm kiếm/phân trang 500,000 dòng)
            if (Request["draw"] != null)
            {
                int start = Convert.ToInt32(Request["start"] ?? "0");
                int length = Convert.ToInt32(Request["length"] ?? "10");
                string draw = Request["draw"]; // Lấy draw ra biến riêng để làm gọn JSON
                string searchValue = Request["search[value]"];

                // Sử dụng AsNoTracking để tối ưu tốc độ đọc cho dữ liệu lớn
                var query = db.Personals.Include(p => p.Employment).AsNoTracking().AsQueryable();

                // 2. Lọc theo loại báo cáo
                if (reportType == "Vacation")
                {
                    query = query.Where(p => p.Employment != null && p.Employment.Vacation_Days > 25);
                }

                // 3. Tìm kiếm theo tên
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(p => p.First_Name.Contains(searchValue) || p.Last_Name.Contains(searchValue));
                }

                // 4. Lấy dữ liệu phân trang
                var dataList = query.OrderBy(p => p.Employee_ID).Skip(start).Take(length).ToList();

                // 5. Định dạng dữ liệu trả về cho View (Sửa lỗi IDE0037 bằng cách viết gọn Member name)
                var data = dataList.Select(p => new {
                    ID = p.Employee_ID,
                    FullName = p.First_Name + " " + p.Last_Name,
                    Gender = p.Gender == 1 ? "Male" : "Female",
                    Shareholder = p.Shareholder_Status == 1 ? "Yes" : "No"
                });

                // Trả về JSON theo cấu trúc DataTables yêu cầu
                return Json(new
                {
                    draw,
                    recordsTotal = db.Personals.Count(),
                    recordsFiltered = query.Count(),
                    data
                }, JsonRequestBehavior.AllowGet);
            }
            var initialData = db.Personals.AsNoTracking().OrderBy(p => p.Employee_ID).Take(100).ToList();
            ViewBag.ReportType = reportType;
            return View(initialData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}