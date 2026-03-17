using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http; 

namespace HRWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // 1. Kích hoạt cấu hình Web API (PHẢI ĐẶT TRÊN CÙNG)
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // 2. Các cấu hình mặc định của MVC
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}