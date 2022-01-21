using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    [Authorize]
    public class VolunteerHoursController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: VolunteerHours Menu
        [Authorize(Roles = "Administrator,Developer,Staff,OfficerOfTheDay,Driver")]
        public ActionResult Index() // shows Enter / Maintain menu
        { return View(); }

        // GET: Volunteer Hours Entry
        [Authorize(Roles = "Administrator,Developer,Staff,OfficerOfTheDay,Driver")]
        public ActionResult Create(DateTime? friday)  // friday will be non-null if a new date is requested by the view
        {
            var usr = db.Users.Find(User.Identity.GetUserId());
            var catName = HoursRoutines.GetCategoryName(usr.VolunteerCategory);
            var subcatName = usr.VolunteerSubcategory ?? "(none)";
            bool isIndividual = true; // default unless in higher role
            bool isDeveloper =AppRoutines.UserIsInRole(usr.Id,"Developer");
            if (isDeveloper)
            {
                bool isAdministrator = AppRoutines.UserIsInRole(usr.Id, "Administrator");
                if (isAdministrator)
                {
                    bool isStaff = AppRoutines.UserIsInRole(usr.Id, "Staff");
                    if (isStaff)
                    {
                        isIndividual = false;  // can enter hours for anyone, any category 
                    }
                }
            }
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
                SubmitError = submitError,
                IsIndividual = isIndividual,
                HoursList = new List<VolunteerHours>()
            };

            if (isIndividual)  // get hours for individual only
            {
                var recs = db.VolunteerHours
                    .Where(h => h.UserId  == usr.Id
                                && h.Category == usr.VolunteerCategory
                                && h.Subcategory == usr.VolunteerSubcategory).ToList();
                foreach (var rec in recs)
                {
                    view .HoursList.Add(rec);
                }
            }

            return View(view);
        }

        //POST: Volunteer Hours Entry 
        [HttpPost, Authorize(Roles = "Administrator,Developer,Staff,OfficerOfTheDay,Driver")]
        public ActionResult Create([Bind(Include = "UserId,Category,Subcategory,"
                           + "WeekEndingDate,Hours,Minutes")] VolunteerHoursViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index","Home");

            if (model.Hours == 0 && model.Minutes == 0)
            {
                TempData["SubmitError"] = "No time was submitted!";
                return RedirectToAction("Create");
            }

            // Look for duplicate record:
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
            return RedirectToAction("Index","Home");
        }

        [Authorize(Roles = "Administrator,Developer,Staff,OfficerOfTheDay,Driver")]
        public ActionResult PreviousFriday(DateTime _friday)
        {
            return RedirectToAction("Create",new{friday = _friday.AddDays(-7)});
        }

        [Authorize(Roles = "Administrator,Developer,Staff,OfficerOfTheDay,Driver")]
        public ActionResult NextFriday(DateTime _friday)
        {
            return RedirectToAction("Create", new{friday =_friday.AddDays(7)});
        }
    }
}