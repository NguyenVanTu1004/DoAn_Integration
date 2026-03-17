using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Bogus;

namespace HRWebApp.Scripts
{
    public class DataSeeder
    {
        // Tứ lưu ý: Kiểm tra lại chuỗi kết nối này để khớp với DB của bạn
        private static string connString = "Server=.;Database=HR;User Id=sa;Password=a123456*;";

        // ============================================================
        // HÀM 1: LẤY DỮ LIỆU DÙNG KỸ THUẬT PHÂN TRANG (PAGING)
        // Dùng OFFSET - FETCH của SQL Server để lấy dữ liệu cực nhanh
        // Giúp doanh nghiệp thao tác trên 500k dòng mà không treo máy
        // ============================================================
        public static DataTable GetEmployeeDataWithPaging(int pageNumber, int pageSize)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // OFFSET: Bỏ qua (page-1)*pageSize dòng đầu
                    // FETCH NEXT: Lấy pageSize dòng tiếp theo
                    string sql = @"
                        SELECT p.*, e.Salary, e.Hire_Date, e.Employment_Status 
                        FROM Personal p
                        LEFT JOIN Employment e ON p.Employee_ID = e.Employee_ID
                        ORDER BY p.Employee_ID
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LỖI PHÂN TRANG: " + ex.Message);
            }
            return dt;
        }

        // ============================================================
        // HÀM 2: LẤY TỔNG SỐ DÒNG (DÙNG ĐỂ HIỂN THỊ THANH TRẠNG THÁI)
        // ============================================================
        public static int GetTotalEmployeeCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(Employee_ID) FROM Personal";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch { return 0; }
        }

        // ============================================================
        // HÀM 3: SEED DỮ LIỆU (ĐỒNG BỘ VỚI JAVA/NETBEANS QUA SEED 2026)
        // ============================================================
        public static void SeedDataTool(int totalRecords)
        {
            Stopwatch totalWatch = Stopwatch.StartNew();
            int successCount = 0;

            try
            {
                int batchSize = 10000;

                // CỰC KỲ QUAN TRỌNG: Phải dùng Seed cố định 2026 để khớp với MySQL
                Randomizer.Seed = new Random(2026);
                var faker = new Faker();

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Tìm ID bắt đầu để không bị trùng lặp khóa chính
                    int startId = GetMaxId(conn, "Personal") + 1;

                    for (int i = 0; i < totalRecords; i += batchSize)
                    {
                        using (SqlTransaction transaction = conn.BeginTransaction())
                        {
                            DataTable pTable = CreatePersonalSchema();
                            DataTable eTable = CreateEmploymentSchema();
                            int currentBatchLimit = Math.Min(batchSize, totalRecords - i);

                            for (int j = 0; j < currentBatchLimit; j++)
                            {
                                int currentId = startId + i + j;

                                // Bảng Personal - Dữ liệu khớp 100% nhờ Seed
                                pTable.Rows.Add(
                                    currentId,
                                    faker.Name.FirstName(),
                                    faker.Name.LastName(),
                                    faker.Random.Int(0, 1),
                                    faker.Random.Int(0, 1),
                                    faker.PickRandom("White", "Black", "Asian", "Hispanic"),
                                    faker.Date.Past(40, DateTime.Now.AddYears(-20))
                                );

                                // Bảng Employment
                                eTable.Rows.Add(
                                    currentId,
                                    Math.Round(faker.Finance.Amount(3000, 20000), 2),
                                    faker.Date.Past(10),
                                    faker.Random.Number(0, 30),
                                    faker.PickRandom("Full-time", "Part-time"),
                                    faker.PickRandom("Plan A", "Plan B", "Plan C")
                                );
                            }

                            try
                            {
                                PerformBulkCopy(conn, transaction, "Personal", pTable);
                                PerformBulkCopy(conn, transaction, "Employment", eTable);
                                transaction.Commit();
                                successCount += currentBatchLimit;
                            }
                            catch (Exception bulkEx)
                            {
                                transaction.Rollback();
                                Debug.WriteLine("LỖI BATCH: " + bulkEx.Message);
                                throw;
                            }
                        }
                        Debug.WriteLine($"[PROGRESS] Đã nạp: {successCount}/{totalRecords}");
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine("LỖI HỆ THỐNG SEED: " + ex.Message); }
            finally
            {
                totalWatch.Stop();
                Debug.WriteLine($"=== HOÀN TẤT: {successCount} dòng trong {totalWatch.Elapsed.TotalSeconds}s ===");
            }
        }

        // ============================================================
        // CÁC HÀM PHỤ TRỢ (HELPER METHODS)
        // ============================================================

        private static int GetMaxId(SqlConnection conn, string tableName)
        {
            string query = $"SELECT ISNULL(MAX(Employee_ID), 0) FROM {tableName}";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void PerformBulkCopy(SqlConnection conn, SqlTransaction trans, string tableName, DataTable data)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, trans))
            {
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BatchSize = 5000;
                bulkCopy.BulkCopyTimeout = 120; // Tăng timeout cho dữ liệu lớn

                foreach (DataColumn column in data.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                bulkCopy.WriteToServer(data);
            }
        }

        private static DataTable CreatePersonalSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Employee_ID", typeof(int));
            dt.Columns.Add("First_Name", typeof(string));
            dt.Columns.Add("Last_Name", typeof(string));
            dt.Columns.Add("Gender", typeof(int));
            dt.Columns.Add("Shareholder_Status", typeof(int));
            dt.Columns.Add("Ethnicity", typeof(string));
            dt.Columns.Add("BirthDate", typeof(DateTime));
            return dt;
        }

        private static DataTable CreateEmploymentSchema()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Employee_ID", typeof(int));
            dt.Columns.Add("Salary", typeof(decimal));
            dt.Columns.Add("Hire_Date", typeof(DateTime));
            dt.Columns.Add("Vacation_Days", typeof(int));
            dt.Columns.Add("Employment_Status", typeof(string));
            dt.Columns.Add("Benefit_Plan", typeof(string));
            return dt;
        }
    }
}