using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BHelp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start(Object sender, EventArgs e)
        {
            DateTime bdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);  // (initialize variable)
            var sdt = bdt.ToString("MM/dd/yyyy");
            HttpContext.Current.Session.Add("StartDate", sdt);
            string edt = new DateTime(bdt.Year, bdt.Month, day: DateTime.DaysInMonth(bdt.Year, bdt.Month)).ToString("MM/dd/yyyy");
            HttpContext.Current.Session.Add("EndDate", edt);
        }

        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }
    }
}
