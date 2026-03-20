using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using HRWebApp.Models;
using System.Data.Entity;
using System.Net.Http;

namespace HRWebApp.Controllers
{
    [RoutePrefix("api/employees")]
    public class EmployeeApiController : ApiController
    {
        private HRDB db = new HRDB();

        [HttpGet]
        [Route("getall")]
        public IHttpActionResult GetAll()
        {
            try
            {
                // TỐI ƯU: Tắt các tính năng không cần thiết để xuất dữ liệu thô nhanh nhất
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Database.CommandTimeout = 300; // Đợi 5 phút cho 500k dòng

                // Truy vấn JOIN và trả về List trực tiếp
                var data = (from p in db.Personals.AsNoTracking()
                            join e in db.Employments.AsNoTracking() on p.Employee_ID equals e.Employee_ID into details
                            from det in details.DefaultIfEmpty()
                            orderby p.Employee_ID ascending
                            select new
                            {
                                id = (int)p.Employee_ID,
                                fullName = (p.First_Name + " " + p.Last_Name).Trim(),
                                gender = p.Gender == true,
                                ethnicity = p.Ethnicity,
                                salaryInSql = (decimal?)(det != null ? det.Salary : 0) ?? 0,
                                vacationDays = (int?)(det != null ? det.Vacation_Days : 0) ?? 0,
                                status = det != null ? det.Employment_Status : "Active"
                            }).ToList();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi xuất mảng dữ liệu: " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}