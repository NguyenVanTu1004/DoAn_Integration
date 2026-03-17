using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRWebApp.Models
{
    public class EmployeeViewModel
    {
        public string ID { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Shareholder { get; set; }
        public string Ethnicity { get; set; }
        public decimal Salary { get; set; }
        public string HireDate { get; set; }
    }
}