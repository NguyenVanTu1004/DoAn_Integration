/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package payrolldataseeder;

import com.github.javafaker.Faker;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.util.Locale;
import static payrolldataseeder.PayrollDataSeederPay_Rates.DB_URL;

/**
 *
 * @author ASUS
 */
public class PayrollDataSeederEmployee {

    static final String DB_URL = "jdbc:mysql://localhost:3307/payroll?useSSL=false&useUnicode=true&characterEncoding=UTF-8";
    static final String USER = "root";
    static final String PASS = "Lelam1234%";

    public static void main(String[] args) throws Exception {
        Class.forName("com.mysql.jdbc.Driver");
        try (Connection conn = DriverManager.getConnection(DB_URL, USER, PASS)) {
            conn.setAutoCommit(false);

            Faker faker = new Faker(new Locale("en"));
            String sql
                    = "INSERT INTO employee "
                    + "(Employee_Number, idEmployee, Last_Name, First_Name, SSN, Pay_Rate, PayRates_id, Vacation_Days, Paid_To_Date, Paid_Last_Year) "
                    + "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            PreparedStatement ps = conn.prepareStatement(sql);
            int employeeNumberStart = 1000;
            String[] levels = {"1.5", "2.5", "3.33", "4.6", "1.7", "2.4"};
            String basePay_Rate_Name = levels[faker.number().numberBetween(0, levels.length)];
            int batchSize = 1000;

            for (int i = 1; i <= 500000; i++) {
                int employeeNumber = employeeNumberStart + i; 
                ps.setInt(1, employeeNumber); // idEmployee (business id, không phải PK)
                ps.setInt(2, i);
                ps.setString(3, faker.name().lastName());
                ps.setString(4, faker.name().firstName());

                ps.setLong(5, faker.number().numberBetween(100000000L, 999999999L)); // SSN

                ps.setString(6, basePay_Rate_Name);

                ps.setInt(7, faker.number().numberBetween(1, 500000)); // PayRates_id

                ps.setInt(8, faker.number().numberBetween(0, 20)); // Vacation days

                ps.setDouble(9, faker.number().randomDouble(2, 0, 99));
                ps.setDouble(10, faker.number().randomDouble(2, 0, 99));

                ps.addBatch();

                if (i % batchSize == 0) {
                    ps.executeBatch();
                    conn.commit();
                    System.out.println("Inserted employee: " + i);
                }
            }

            ps.executeBatch();
            conn.commit();
        }

        System.out.println("DONE 500K EMPLOYEE");
    }
}
