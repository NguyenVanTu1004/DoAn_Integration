/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package springapp.web.dao;

import java.util.List;
import org.hibernate.Session;
import org.springframework.stereotype.Repository;
import springapp.web.model.HibernateUtil;
import springapp.web.model.Employee;

/**
 *
 * @author ASUS
 */
@Repository
public class EmployeeDao {

    public long sumPay_Rates() {
        Session session = HibernateUtil.getSessionFactory().getCurrentSession();
        session.beginTransaction();

        String sql
                = "SELECT SUM(p.pay_amount) "
                + "FROM employee e "
                + "JOIN pay_rates p ON e.PayRates_id = p.idPay_Rates";

        Object result = session.createSQLQuery(sql).uniqueResult();

        session.getTransaction().commit();

        if (result == null) {
            return 0;
        }
        return ((Number) result).longValue();
    }
}
