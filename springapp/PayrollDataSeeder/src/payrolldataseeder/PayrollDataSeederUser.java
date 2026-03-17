/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package payrolldataseeder;

/**
 *
 * @author ASUS
 */
import com.github.javafaker.Faker;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.util.Locale;

public class PayrollDataSeederUser {
    
    /**
     * @param args the command line arguments
     */
    static final String DB_URL = "jdbc:mysql://localhost:3307/payroll?useSSL=false&useUnicode=true&characterEncoding=UTF-8";
    static final String USER = "root";
    static final String PASS = "Lelam1234%"; 
    public static void main(String[] args) throws Exception{
        Class.forName("com.mysql.jdbc.Driver");
        try (Connection conn = DriverManager.getConnection(DB_URL, USER, PASS)) {
            conn.setAutoCommit(false);
            
            Faker faker = new Faker(new Locale("en"));
            String sql = "INSERT INTO users(User_Name, Password, Email, Active) VALUES (?, ?, ?, ?)";
            PreparedStatement ps = conn.prepareStatement(sql);
            
            int batchSize = 1000;
            
            for (int i = 1; i <= 500000; i++) {
                ps.setString(1, faker.name().username() + i);// tranhs bị trùng các name với nahu nên dùng i
                ps.setString(2, "123456");
                ps.setString(3, faker.internet().emailAddress());
                ps.setInt(4, 1);
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

        System.out.println("DONE 500K USERS");
    }
    
}
