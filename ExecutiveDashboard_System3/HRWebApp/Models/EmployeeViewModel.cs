using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRWebApp.Models
{
        public class EmployeeViewModel
        {
            // Nhóm thông tin lấy từ Thiện (SQL Server)
            public int ID { get; set; }
            public string FullName { get; set; }
            public string Gender { get; set; }
            public string Ethnicity { get; set; }

            // Nhóm thông tin lấy từ Lâm (MySQL)
            public decimal Salary { get; set; }
            public int Vacation_Days { get; set; }
            public DateTime? Hire_Date { get; set; }
        }   
}