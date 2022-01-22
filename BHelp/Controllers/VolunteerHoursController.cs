using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    [Authorize]
    public class VolunteerHoursController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: VolunteerHours Menu
        [AllowAnonymous]
        public ActionResult Index() // shows Enter / Maintain menu
        { return View(); }

        // GET: Volunteer Hours Entry
        [AllowAnonymous]
        public ActionResult Create(DateTime? hoursDate)  // hoursDate will be non-null if a new date is requested by the view
        {
            var usr = db.Users.Find(User.Identity.GetUserId());
            var catName = HoursRoutines.GetCategoryName(usr.VolunteerCategory);
            var subcatName = usr.VolunteerSubcategory ?? "(none)";
            bool isIndividual = HoursRoutines.IsIndividual(usr.Id);
            
            var entryDate = DateTime.Today;
            DateTime wkBegin;
            string wkBeginString;
            DateTime wkEnd;
            string wkEndString;
            if (hoursDate == null)
            {
                wkBegin = HoursRoutines.GetPreviousSaturday(DateTime.Today);
                wkBeginString = wkBegin.ToString("MM/dd/yyyy");
                wkEnd = wkBegin.AddDays(6);
                wkEndString = wkEnd.ToString("MM/dd/yyyy");
            }
            else
            {
                entryDate = hoursDate.Value;
                wkBegin = HoursRoutines.GetPreviousSaturday(hoursDate.Value);
                wkBeginString = wkBegin.ToString("MM/dd/yyyy");
                wkEnd =wkBegin.AddDays(6);
                wkEndString = wkEnd.ToString("MM/dd/yyyy");
            }

            var submitError = string.Empty;
            if (TempData["SubmitError"] != null) submitError = TempData["SubmitError"].ToString();
            
            var view = new VolunteerHoursViewModel
            {
                UserId = usr.Id,
                UserFullName = usr.FullName,
                Category = usr.VolunteerCategory,
                Subcategory = usr.VolunteerSubcategory,
                VolunteerName = usr.FullName,
                CategoryName = catName,
                SubcategoryName = subcatName,
                Date = entryDate,
                DateString = entryDate.ToString("MM/dd/yyyy"),
                WeekBeginningDate = wkBegin,
                WeekBeginningDateString = wkBeginString,
                WeekEndingDate = wkEnd,
                WeekEndingDateString = wkEndString,
                SubmitError = submitError,
                IsIndividual = isIndividual,
                HoursList = new List<VolunteerHoursViewModel>()
            };

            if (isIndividual)  // get hours for individual only
            {
                var recs = db.VolunteerHours
                    .Where(h => h.UserId  == usr.Id
                                && h.Date >= wkBegin && h.Date <= wkEnd).ToList();
                foreach (var rec in recs)
                {
                    var newView = new VolunteerHoursViewModel
                    {
                        Id=rec.Id,
                        UserId = usr.Id,
                        UserFullName = usr.FullName,
                        CategoryName = HoursRoutines .GetCategoryName(rec.Category),
                        Subcategory = rec.Subcategory,
                        VolunteerName = usr.FullName,
                        Date = rec.Date,
                        DateString = rec.Date .ToString("MM/dd/yyyy"),
                        HoursString = rec.Hours.ToString(),
                        MinutesString = rec.Minutes.ToString()
                    };
                    view.HoursList.Add(newView);
                }
            }

            return View(view);
        }

        //POST: Volunteer Hours Entry 
        [HttpPost, AllowAnonymous]
        public ActionResult Create([Bind(Include = "UserId,Category,Subcategory,"
                           + "Date,Hours,Minutes")] VolunteerHoursViewModel model)
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
                                                               && r.Date == model.Date
                                                               && r.Category == model.Category
                                                               && r.Subcategory == model.Subcategory);

            if (oldRec != null)
            {
                TempData["SubmitError"] = "A record for this date & categories was already submitted!";
                return RedirectToAction("Create");
            }

            var newRec = new VolunteerHours()
            {
                UserId = model.UserId,
                OriginatorUserId =System.Web.HttpContext.Current.User.Identity.GetUserId(),
                Category =model.Category,
                Subcategory = model.Subcategory,
                Date = model.Date,
                Hours=model.Hours,
                Minutes = model.Minutes
            };

            db.VolunteerHours.Add(newRec);
            db.SaveChanges();
            return RedirectToAction("Create");
        }

        //GET Edit Volunteer Hours
        [AllowAnonymous]
        public ActionResult Edit(int recId)
        {
            var rec = db.VolunteerHours.Find(recId);
            if (rec == null) return null;
            var currentUser = db.Users.Find(User.Identity.GetUserId());
            var hoursUser = db.Users.Find(rec.UserId);
            if (hoursUser == null) return null;
            var view = new VolunteerHoursViewModel
            {
                UserId = rec.UserId,
                IsIndividual = HoursRoutines.IsIndividual(currentUser.Id),
                CategoryName = HoursRoutines.GetCategoryName(rec.Category),
                Subcategory = rec.Subcategory,
                SubcategoryName = hoursUser.VolunteerSubcategory ?? "(none)",
                VolunteerName = hoursUser.FullName,
                Date = rec.Date,
                DateString = rec.Date.ToString("MM/dd/yyyy"),
                HoursString = rec.Hours.ToString(),
                MinutesString = rec.Minutes.ToString(),
                CategoryList = HoursRoutines.GetHoursCategories(rec.Category),
                SubcategoryList = HoursRoutines.GetHoursSubcategories()
            };

            return View(view);
        }
    }
}