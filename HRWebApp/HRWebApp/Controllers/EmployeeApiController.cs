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
        public IHttpActionResult GetAll(int start = 0, int length = 10)
        {
            try
            {
                // TỐI ƯU: Xuất dữ liệu thô, không theo dõi (AsNoTracking) để tăng tốc
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Database.CommandTimeout = 300;

                // 1. Tạo Query cơ bản (Chưa thực thi xuống DB)
                var query = (from p in db.Personals.AsNoTracking()
                             join e in db.Employments.AsNoTracking() on p.Employee_ID equals e.Employee_ID into details
                             from det in details.DefaultIfEmpty()
                             select new
                             {
                                 id = (int)p.Employee_ID,
                                 fullName = (p.First_Name + " " + p.Last_Name).Trim(),
                                 gender = p.Gender == true ? "Male" : "Female", // Khớp với nhãn biểu đồ
                                 ethnicity = p.Ethnicity,
                                 salaryInSql = (decimal?)(det != null ? det.Salary : 0) ?? 0,
                                 vacationDays = (int?)(det != null ? det.Vacation_Days : 0) ?? 0,
                                 status = det != null ? det.Employment_Status : "Active"
                             });

                // 2. Đếm tổng số bản ghi (Cần thiết cho DataTables)
                int totalRecords = query.Count();

                // 3. Phân trang tại Server: Chỉ lấy đúng số lượng cần thiết
                var pagedData = query.OrderBy(x => x.id)
                                     .Skip(start)  // Bỏ qua các dòng trước đó
                                     .Take(length) // Lấy đúng số dòng của trang hiện tại
                                     .ToList();

                // 4. Trả về đúng cấu trúc Object mà DataTables yêu cầu
                return Ok(new
                {
                    draw = Request.GetQueryNameValuePairs().FirstOrDefault(x => x.Key == "draw").Value,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi phân trang: " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}