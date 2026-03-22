const express = require('express');
const mysql = require('mysql2/promise');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

// --- CẤU HÌNH KẾT NỐI (Giữ nguyên Port 3307 đã thông) ---
const pool = mysql.createPool({
    host: '127.0.0.1',
    user: 'root',
    password: '123456',
    database: 'payroll',
    port: 3307,
    waitForConnections: true,
    connectionLimit: 15,
    queueLimit: 0
});

app.get('/api/mysql/employees', async (req, res) => {
    try {
        // SQL lấy 100 dòng đầu tiên, bắt đầu tính ID từ 1 (giảm đi 1000)
        // Sử dụng dấu huyền cho tên bảng để tránh lỗi cú pháp
        const sql = `
            SELECT 
                (e.Employee_Number - 1000) AS id, 
                CONCAT(e.First_Name, ' ', e.Last_Name) AS fullName,
                e.SSN AS ssn,
                e.Vacation_Days AS vacationDays,
                p.Pay_Rate_Name AS payRole,
                p.Value AS salaryValue
            FROM \`employee\` AS e
            LEFT JOIN \`pay_rates\` AS p ON e.PayRates_id = p.idPay_Rates
            WHERE e.Employee_Number >= 1001
            ORDER BY id ASC
            LIMIT 100
        `;

        const [rows] = await pool.query(sql);
        
        console.log(`[${new Date().toLocaleTimeString()}] ✅ Đã lấy 100 dòng đầu tiên thành công.`);
        
        // Trả về dữ liệu phẳng (Array) để Dashboard dễ đọc
        res.status(200).json(rows);
        
    } catch (err) {
        console.error("❌ Lỗi truy vấn:", err.message);
        res.status(500).json({ error: "Lỗi hệ thống MySQL", detail: err.message });
    }
});

const PORT = 3000;
app.listen(PORT, () => {
    console.log(`--------------------------------------------------`);
    console.log(`🚀 SERVER ĐÃ CHẠY: http://127.0.0.1:${PORT}/api/mysql/employees`);
});