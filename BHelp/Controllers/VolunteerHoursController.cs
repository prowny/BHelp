using Castle.Core.Internal;
using System.Web.Mvc;

namespace BHelp.Controllers
{
    public class VolunteerHoursController : Controller
    {
        // GET: VolunteerHours
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}