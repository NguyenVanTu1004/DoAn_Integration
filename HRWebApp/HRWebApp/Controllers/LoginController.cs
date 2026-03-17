using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HRWebApp.Controllers
{
    public class LoginController : Controller
    {
        // 1. Chức năng Login (Hiện Form)
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // PHẦN QUAN TRỌNG: Xử lý khi nhấn nút Login trên giao diện
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Kiểm tra tài khoản (Sau này bạn có thể check trong DB)
            if (username == "admin" && password == "123")
            {
                Session["UserRole"] = "Admin";
                Session["UserName"] = "Quản trị viên";

                // Đăng nhập đúng thì chuyển hướng sang trang chủ Admin (File Index lúc nãy)
                return RedirectToAction("Index", "Admin");
            }

            // Nếu sai, báo lỗi ra màn hình
            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        // 2. Chức năng Profile
        public ActionResult Profile()
        {
            return View();
        }

        // 3. Chức năng All Users
        public ActionResult AllUsers()
        {
            return View();
        }

        // 4. Chức năng Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}