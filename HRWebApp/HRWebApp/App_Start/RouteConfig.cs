using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HRWebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // ĐỊNH NGHĨA ROUTE RIÊNG CHO EMPLOYEE API
            // URL này sẽ giúp link /api/employees/getall hoạt động dù tên file là EmployeeApiController
            routes.MapRoute(
                name: "EmployeeApiRoute",
                url: "api/employees/{action}/{id}",
                defaults: new { controller = "EmployeeApi", action = "getall", id = UrlParameter.Optional }
            );

            // ROUTE MẶC ĐỊNH
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}