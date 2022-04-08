using System;
using System.Web.Mvc;
using BHelp.ViewModels;
using DocumentFormat.OpenXml.Drawing;
using Org.BouncyCastle.Utilities;

namespace BHelp.Controllers
{
    public class DriverScheduleController : Controller
    {
        // GET: DriverSchedule
        public ActionResult Index()
        {
            var view = new DriverScheduleViewModel();
            if (Session["DriverScheduleMonthYear"] == null)
            {
                view.Month = DateTime.Today.Month;
                view.Year = DateTime.Today.Year;
                view.MonthName = Strings.ToUpperCase(DateTime.Today.ToString("MMMM"));
                Session["DriverScheduleMonthYear"] = view.Month.ToString("00") + view.Year;
            }
            else
            {
                var x = Session["DriverScheduleMonthYear"].ToString();
                view.Month = Convert.ToInt32(x.Substring(0, 2));
                view.Year = Convert.ToInt32(x.Substring(2, 4));
                var fakeDate = new DateTime(view.Year, view.Month, 1);
                view.MonthName = Strings.ToUpperCase(fakeDate.ToString("MMMM"));
            }
        

            return View(view);
        }

        public ActionResult PreviousMonth(int month, int year)
        {
            month = month - 1;
            if (month < 1)
            {
                month = 12;
                year = year - 1;
            }
            Session["DriverScheduleMonthYear"] = month.ToString("00") + year;
            return RedirectToAction( "Index");
        }

        public ActionResult NextMonth(int month, int year)
        {
            month = month + 1;
            if (month >12)
            {
                month = 1;
                year = year + 1;
            }
            Session["DriverScheduleMonthYear"] = month.ToString("00") + year;
            return RedirectToAction("Index");
        }
    }
}