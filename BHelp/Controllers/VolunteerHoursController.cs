using System;
using System.Linq;
using Castle.Core.Internal;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class VolunteerHoursController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: VolunteerHours Menu
        public ActionResult Index() // shows Enter / Maintain menu
        { return View(); }

        // GET: Volunteer Hours Entry
        public ActionResult Create(DateTime? friday)
        {
            var usr = db.Users.Find(User.Identity.GetUserId());
            var catName = HoursRoutines.GetCategoryName(usr.VolunteerCategory);
            var subcatName = usr.VolunteerSubcategory ?? "(none)";
            DateTime wkEnd;
            string wkEndString;
           
            if (friday == null)
            {
                  wkEnd = HoursRoutines.GetPreviousFriday(DateTime.Today);
                  wkEndString = wkEnd.ToString("MM/dd/yyyy");
            }
            else
            {
                 wkEnd = friday.Value;
                 wkEndString = wkEnd.ToString("MM/dd/yyyy");
            }

            var submitError = string.Empty;
            if (TempData["SubmitError"] != null)
            {
                submitError = TempData["SubmitError"].ToString();
            }
            var view = new VolunteerHoursViewModel
            {
                UserId = usr.Id,
                Category = usr.VolunteerCategory,
                Subcategory = usr.VolunteerSubcategory,
                VolunteerName = usr.FullName,
                CategoryName = catName,
                SubcategoryName = subcatName,
                WeekEndingDate = wkEnd,
                WeekEndingDateString = wkEndString,
                SubmitError = submitError
            };

            return View(view);
        }

        //POST: Volunteer Hours Entry 
        [HttpPost]
        public ActionResult Create([Bind(Include = "UserId,Category,Subcategory,"
                           + "WeekEndingDate,Hours,Minutes")] VolunteerHoursViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("ReturnToDashboard");

            if (model.Hours == 0 && model.Minutes == 0)
            {
                TempData["SubmitError"] = "No time was submitted!";
                return RedirectToAction("Create");
            }

            var oldRec = db.VolunteerHours
                .FirstOrDefault(r => r.UserId == model.UserId
                                                               && r.WeekEndingDate == model.WeekEndingDate
                                                               && r.Category == model.Category
                                                               && r.Subcategory == model.Subcategory);

            if (oldRec != null)
            {
                TempData["SubmitError"] = "A record for this date was already submitted!";
                return RedirectToAction("Create");
            }

            var newRec = new VolunteerHours()
            {
                UserId = model.UserId,
                OriginatorUserId =System.Web.HttpContext.Current.User.Identity.GetUserId(),
                Category =model.Category,
                Subcategory = model.Subcategory,
                WeekEndingDate = model.WeekEndingDate,
                Hours=model.Hours,
                Minutes = model.Minutes
            };

            db.VolunteerHours.Add(newRec);
            db.SaveChanges();
            return RedirectToAction("ReturnToDashboard");
        }

        //private ActionResult CreateSuccess()
        //{
        //    return RedirectToAction("ReturnToDashboard");
        //}
        public ActionResult PreviousFriday(DateTime _friday)
        {
            return RedirectToAction("Create",new{friday = _friday.AddDays(-7)});
        }

        public ActionResult NextFriday(DateTime _friday)
        {
            return RedirectToAction("Create", new{friday =_friday.AddDays(7)});
        }

        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }

    }
}