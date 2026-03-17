using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http; // QUAN TRỌNG: Chỉ dùng thư viện này cho API
using HRWebApp.Models;

namespace HRWebApp.Controllers
{
    [RoutePrefix("api/employees")]
    public class EmployeeApiController : ApiController
    {
        [HttpGet]
        [Route("getall")]
        public IHttpActionResult GetAllEmployees(string search = "")
        {
            using (var db = new HRDB())
            {
                try
                {
                    // Truy vấn đúng 4 trường dữ liệu cũ của Tứ
                    var query = from p in db.Personals.AsNoTracking()
                                join e in db.Employments.AsNoTracking() on p.Employee_ID equals e.Employee_ID
                                select new
                                {
                                    // Ép kiểu (int) để ID hiện số nguyên sạch sẽ, không bị 500001.0
                                    id = (int)p.Employee_ID,
                                    fullName = p.First_Name + " " + p.Last_Name,
                                    gender = p.Gender == 1 ? "Male" : "Female",
                                    ethnicity = p.Ethnicity
                                };

                    if (!string.IsNullOrEmpty(search))
                    {
                        query = query.Where(x => x.fullName.Contains(search));
                    }

                    // Sắp xếp và lấy 500k dòng nhanh nhất
                    var data = query.OrderBy(x => x.id).Take(500000).ToList();

                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }
    }
}