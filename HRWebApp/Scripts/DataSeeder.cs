using System;
using System.Data;
using System.Data.SqlClient;
using Bogus; // Thư viện tạo dữ liệu giả
using System.Collections.Generic;

namespace HRWebApp.Scripts
{
    public class DataSeeder
    {
        private static string connString = "Server=.;Database=HR;User Id=sa;Password=a123456*;";

        public static void SeedDataTool(int rowCount)
        {
            // 1. Cấu hình Faker (Bogus) để tạo dữ liệu tiếng Anh/Việt
            var faker = new Faker("en");

            // 2. Tạo DataTable cho bảng Personal
            DataTable personalTable = new DataTable();
            personalTable.Columns.Add("Employee_ID", typeof(int));
            personalTable.Columns.Add("First_Name", typeof(string));
            personalTable.Columns.Add("Last_Name", typeof(string));
            personalTable.Columns.Add("Gender", typeof(int));
            personalTable.Columns.Add("Shareholder_Status", typeof(int));
            personalTable.Columns.Add("Ethnicity", typeof(string));

            // 3. Tạo DataTable cho bảng Employment
            DataTable employmentTable = new DataTable();
            employmentTable.Columns.Add("Employee_ID", typeof(int));
            employmentTable.Columns.Add("Salary", typeof(decimal));
            employmentTable.Columns.Add("Hire_Date", typeof(DateTime));

            // 4. Vòng lặp tạo 500.000 dòng vào bộ nhớ (DataTable)
            for (int i = 1; i <= rowCount; i++)
            {
                // Dữ liệu cho bảng Personal
                personalTable.Rows.Add(
                    i,
                    faker.Name.FirstName(),
                    faker.Name.LastName(),
                    faker.PickRandom(0, 1),
                    faker.PickRandom(0, 1),
                    faker.PickRandom("White", "Black", "Asian", "Hispanic")
                );

                // Dữ liệu cho bảng Employment
                employmentTable.Rows.Add(
                    i,
                    faker.Finance.Amount(3000, 20000),
                    faker.Date.Past(10)
                );
            }

            // 5. Sử dụng SqlBulkCopy để đẩy dữ liệu cực nhanh vào SQL
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Nạp bảng Personal
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                        {
                            bulkCopy.DestinationTableName = "Personal";
                            bulkCopy.BatchSize = 10000; // Mỗi đợt nạp 10k dòng
                            bulkCopy.BulkCopyTimeout = 600; // 10 phút timeout cho chắc
                            bulkCopy.WriteToServer(personalTable);
                        }

                        // Nạp bảng Employment
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                        {
                            bulkCopy.DestinationTableName = "Employment";
                            bulkCopy.BatchSize = 10000;
                            bulkCopy.BulkCopyTimeout = 600;
                            bulkCopy.WriteToServer(employmentTable);
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}