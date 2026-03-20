const express = require('express');
const mysql = require('mysql2/promise');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

// --- CẤU HÌNH KẾT NỐI ---
const pool = mysql.createPool({
    host: 'localhost',
    user: 'root',
    password: '123456',
    database: 'payroll',
    port: 3307,
    waitForConnections: true,
    connectionLimit: 15, // Tăng giới hạn kết nối khi lấy dữ liệu lớn
    queueLimit: 0
});

app.get('/api/mysql/employees', async (req, res) => {
    try {
        // 1. Bỏ LIMIT 100 để lấy hết sạch dữ liệu
        // 2. Ép ID (Employee_Number - 1000) để bắt đầu từ 1
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
        `;

        const [rows] = await pool.query(sql);
        
        console.log(`[${new Date().toLocaleTimeString()}] ✅ Đã truy xuất TOÀN BỘ: ${rows.length} dòng dữ liệu.`);
        res.status(200).json(rows);
        
    } catch (err) {
        console.error("❌ Lỗi truy vấn:", err.message);
        res.status(500).json({ error: "Lỗi hệ thống MySQL", detail: err.message });
    }
});

const PORT = 3000;
app.listen(PORT, () => {
    console.log(`
     URL: http://127.0.0.1:3000/api/mysql/employees
    `);
});