using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Script.Serialization;
using System.Net;
using System.Configuration;
using System.Linq;

namespace System3_Integration
{
    public class EmployeeSyncService
    {
        private readonly string _hrApiUrl = "http://localhost:19335/api/employees/getall";
        private readonly string _payrollApiUrl = "http://localhost:3000/api/mysql/employees";
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["HRDB"]?.ConnectionString;

        public void SyncAllEmployees()
        {
            SyncAndReconcile();
        }

        public void SyncAndReconcile()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // TLS 1.2
            JavaScriptSerializer serializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                try
                {
                    // --- BƯỚC 1: LẤY DỮ LIỆU TỪ HR (SQL SERVER) ---
                    // Ép API nhả hết 500k dòng bằng tham số length
                    Console.WriteLine("Đang kéo dữ liệu từ HR System...");
                    string hrJson = client.DownloadString(_hrApiUrl + "?start=0&length=500000");

                    // SỬA: Phải bóc lớp vỏ .data của API C# mới
                    var hrWrapper = serializer.Deserialize<Dictionary<string, object>>(hrJson);
                    var hrList = serializer.ConvertToType<List<HrEmployeeDto>>(hrWrapper["data"]);

                    // --- BƯỚC 2: LẤY DỮ LIỆU TỪ PAYROLL (NODEJS/MYSQL) ---
                    Console.WriteLine("Đang kéo dữ liệu từ Payroll System...");
                    string payrollJson = client.DownloadString(_payrollApiUrl + "?limit=500000");

                    // SỬA: Phải bóc lớp vỏ .data của API Node.js mới
                    var payrollWrapper = serializer.Deserialize<Dictionary<string, object>>(payrollJson);
                    var payrollList = serializer.ConvertToType<List<PayrollEmployeeDto>>(payrollWrapper["data"]);

                    // --- BƯỚC 3: ĐỐI SOÁT (MAPPING) ---
                    // Dùng Dictionary để mapping O(1) - cực nhanh cho 500k dòng
                    var payrollDict = payrollList.ToDictionary(p => p.id, p => p);
                    List<FinalIntegratedEmployee> finalData = new List<FinalIntegratedEmployee>();

                    foreach (var hr in hrList)
                    {
                        var emp = new FinalIntegratedEmployee
                        {
                            Employee_ID = hr.id,
                            FullName = hr.fullName ?? "Unknown",
                            Ethnicity = hr.ethnicity ?? "N/A",
                            Vacation_Days = hr.vacationDays,
                            Employment_Status = hr.status ?? "Active",
                            Gender = hr.gender
                        };

                        if (payrollDict.ContainsKey(hr.id))
                        {
                            emp.Salary = payrollDict[hr.id].salaryValue;
                            emp.Sync_Note = "Khớp (Lương lấy từ MySQL)";
                        }
                        else
                        {
                            emp.Salary = hr.salaryInSql;
                            emp.Sync_Note = "Lệch: Thiếu dữ liệu Payroll";
                        }
                        finalData.Add(emp);
                    }

                    // --- BƯỚC 4: LƯU VÀO DATABASE HỆ THỐNG 3 ---
                    ClearOldData();
                    SaveToDatabaseFast(finalData);
                    Console.WriteLine("✅ Thành công! Đã đối soát {0} nhân viên.", finalData.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi chi tiết: " + ex.ToString());
                    throw new Exception("Lỗi đối soát: " + ex.Message);
                }
            }
        }

        private void ClearOldData()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                new SqlCommand("TRUNCATE TABLE SyncEmployees", conn).ExecuteNonQuery();
            }
        }

        private void SaveToDatabaseFast(List<FinalIntegratedEmployee> data)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                {
                    bulkCopy.DestinationTableName = "SyncEmployees";
                    bulkCopy.BatchSize = 5000; // Tối ưu việc đẩy dữ liệu theo lô

                    bulkCopy.ColumnMappings.Add("Employee_ID", "Employee_ID");
                    bulkCopy.ColumnMappings.Add("First_Name", "First_Name");
                    bulkCopy.ColumnMappings.Add("Last_Name", "Last_Name");
                    bulkCopy.ColumnMappings.Add("Gender", "Gender");
                    bulkCopy.ColumnMappings.Add("Ethnicity", "Ethnicity");
                    bulkCopy.ColumnMappings.Add("Salary", "Salary");
                    bulkCopy.ColumnMappings.Add("Vacation_Days", "Vacation_Days");
                    bulkCopy.ColumnMappings.Add("Employment_Status", "Employment_Status");

                    bulkCopy.WriteToServer(ToDataTable(data));
                }
            }
        }

        private DataTable ToDataTable(List<FinalIntegratedEmployee> items)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Employee_ID", typeof(int));
            dt.Columns.Add("First_Name", typeof(string));
            dt.Columns.Add("Last_Name", typeof(string));
            dt.Columns.Add("Gender", typeof(bool));
            dt.Columns.Add("Ethnicity", typeof(string));
            dt.Columns.Add("Salary", typeof(decimal));
            dt.Columns.Add("Vacation_Days", typeof(int));
            dt.Columns.Add("Employment_Status", typeof(string));

            foreach (var item in items)
            {
                string firstName = "Unknown";
                string lastName = "";
                if (!string.IsNullOrEmpty(item.FullName))
                {
                    string[] parts = item.FullName.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    firstName = parts[0];
                    if (parts.Length > 1) lastName = parts[1];
                }

                dt.Rows.Add(item.Employee_ID, firstName, lastName, item.Gender, item.Ethnicity, item.Salary, item.Vacation_Days, item.Employment_Status);
            }
            return dt;
        }
    }

    // Các DTO giữ nguyên như thiết kế của bạn
    public class HrEmployeeDto
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public bool gender { get; set; }
        public string ethnicity { get; set; }
        public decimal salaryInSql { get; set; }
        public int vacationDays { get; set; }
        public string status { get; set; }
    }

    public class PayrollEmployeeDto
    {
        public int id { get; set; }
        public decimal salaryValue { get; set; }
    }

    public class FinalIntegratedEmployee
    {
        public int Employee_ID { get; set; }
        public string FullName { get; set; }
        public bool Gender { get; set; }
        public string Ethnicity { get; set; }
        public decimal Salary { get; set; }
        public int Vacation_Days { get; set; }
        public string Employment_Status { get; set; }
        public string Sync_Note { get; set; }
    }
}