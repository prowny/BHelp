using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;

namespace BHelp.Controllers
{
    public class LoginsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Logins
        public ActionResult Index()
        {
            var model = db.Logins.OrderByDescending(x => x.DateTime).Take(500);

            return View(model);
        }

        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
