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
import static payrolldataseeder.PayrollDataSeederUser.DB_URL;

/**
 *
 * @author ASUS
 */
public class PayrollDataSeederPay_Rates {

    static final String DB_URL = "jdbc:mysql://localhost:3307/payroll?useSSL=false&useUnicode=true&characterEncoding=UTF-8";
    static final String USER = "root";
    static final String PASS = "Lelam1234%";

    public static void main(String[] args) throws Exception {
        Class.forName("com.mysql.jdbc.Driver");
        try (Connection conn = DriverManager.getConnection(DB_URL, USER, PASS)) {
            conn.setAutoCommit(false);

            Faker faker = new Faker(new Locale("en"));
            String sql = "INSERT INTO pay_rates(idPay_Rates, Pay_Rate_Name, value, Tax_Percentage, Pay_Type, Pay_Amount, PT_Level_C) VALUES (?, ?, ?, ?, ?, ?, ?)";
            PreparedStatement ps = conn.prepareStatement(sql);

            double baseSalary = faker.number().numberBetween(5000000, 30000000);
            String[] levels = {"Intern", "Staff", "Senior Staff", "Leader", "Manager"};
            String basePay_Rate_Name = levels[faker.number().numberBetween(0, levels.length)];
            int Pay_type = faker.number().numberBetween(1, 11);
            double tax = faker.number().randomDouble(2, 5, 20); // 5–20%
            int batchSize = 1000;

            for (int i = 3; i <= 500000; i++) {
                ps.setInt(1, i);
                ps.setString(2, basePay_Rate_Name);// tranhs bị trùng các name với nahu nên dùng i
                ps.setDouble(3, baseSalary);
                ps.setDouble(4, tax);
                ps.setInt(5, Pay_type);
                ps.setDouble(6, baseSalary * (Pay_type - tax / 100));
                ps.setInt(7, faker.number().numberBetween(1, 4));
                ps.addBatch();

                if (i % batchSize == 0) {
                    ps.executeBatch();
                    conn.commit();
                    System.out.println("Inserted: " + i);
                }
            }

            ps.executeBatch();
            conn.commit();
        }

        System.out.println("DONE 500K Pay__Rates");
    }
}
