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

        public ActionResult Index(string reportType = "Default")
        {
            // Xử lý nạp dữ liệu cho DataTables (image_3026bc.png)
            if (Request["draw"] != null)
            {
                int start = Convert.ToInt32(Request["start"] ?? "0");
                int length = Convert.ToInt32(Request["length"] ?? "10");
                string draw = Request["draw"];
                string searchValue = Request["search[value]"];

                var query = db.Personals.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(p => p.First_Name.Contains(searchValue) || p.Last_Name.Contains(searchValue));
                }

                int totalRecords = db.Personals.Count();
                int filteredRecords = query.Count();

                var dataList = query.OrderBy(p => p.Employee_ID).Skip(start).Take(length).ToList();

                var data = dataList.Select(p => new {
                    ID = p.Employee_ID,
                    FullName = p.First_Name + " " + p.Last_Name,
                    // Xử lý kiểu bit/bool từ DB sang chữ hiển thị
                    Gender = (p.Gender == true) ? "Male" : "Female",
                    Ethnicity = p.Ethnicity,
                    BirthDate = p.BirthDate.HasValue ? p.BirthDate.Value.ToString("dd/MM/yyyy") : "",
                    Shareholder = (p.Shareholder_Status == true) ? "Yes" : "No"
                });

                return Json(new { draw, recordsTotal = totalRecords, recordsFiltered = filteredRecords, data }, JsonRequestBehavior.AllowGet);
            }

            // Giao diện mặc định hiện 100 dòng đầu
            var initialData = db.Personals.AsNoTracking().OrderBy(p => p.Employee_ID).Take(100).ToList();
            return View(initialData);
        }
    }
}