using System.Web.Mvc;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class DriverScheduleController : Controller
    {
        // GET: DriverSchedule
        public ActionResult Index()
        {
            var view = new DriverScheduleViewModel()
            {
                MonthName = "APRIL",
                Year = 2022
            };

            return View(view);
        }

        public ActionResult PreviousMonth(int month, int year)
        {
            return null;
        }

        public ActionResult NextMonth(int month, int year)
        {
            return null;
        }
    }
}