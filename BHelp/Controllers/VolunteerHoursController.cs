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
        [AllowAnonymous]
        public ActionResult Index() // shows Enter / Maintain menu
        {
            if (!User.IsInRole("Administrator") && !User.IsInRole("Staff"))
            { return RedirectToAction("Create");}
            return View();
        }

        // GET: Create Volunteer Hours Entry
        [AllowAnonymous]
        public ActionResult
            Create(DateTime? hoursDate, string userId) // hoursDate will be non-null if a new date is requested by the view
        {
            if (TempData["HoursDate"] != null)
            {
                hoursDate = Convert.ToDateTime(TempData["HoursDate"]);
                if(TempData["CurrentUserId"] != null){ userId = (string)TempData ["CurrrentUserId"];}
            }

            var _curUsrId = User.Identity.GetUserId();
            var _id = userId ??_curUsrId;
            TempData["CurrentUserId"] = _id;

            var usr = db.Users.Find(_id);
            var catName = HoursRoutines.GetCategoryName(usr.VolunteerCategory) ?? "(none)";
            var subcatName = usr.VolunteerSubcategory ?? "(none)";
            bool isIndividual = HoursRoutines.IsIndividual(_curUsrId);

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
                wkEnd = wkBegin.AddDays(6);
                wkEndString = wkEnd.ToString("MM/dd/yyyy");
            }

            var submitError = string.Empty;
            if (TempData["SubmitError"] != null) submitError = TempData["SubmitError"].ToString();

            var view = new VolunteerHoursViewModel
            {
                UserId = usr.Id,
                UserFullName = usr.FullName,
                Category = usr.VolunteerCategory ?? "(none)",
                Subcategory = usr.VolunteerSubcategory ?? "(none)",
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
                HoursList = new List<VolunteerHoursViewModel>(),
                CategoryList =HoursRoutines.GetHoursCategoriesSelectList(),
                SubcategoryList =HoursRoutines.GetHoursSubcategoriesSelectList(usr)
            };

            if (isIndividual) // get hours for individual only
            {
                var recs = db.VolunteerHours
                    .Where(h => h.UserId == usr.Id
                                && h.Date >= wkBegin && h.Date <= wkEnd).ToList();
                foreach (var rec in recs)
                {
                    var newView = new VolunteerHoursViewModel
                    {
                        Id = rec.Id,
                        UserId = usr.Id,
                        UserFullName = usr.FullName,
                        CategoryName = HoursRoutines.GetCategoryName(rec.Category),
                        Subcategory = rec.Subcategory,
                        VolunteerName = usr.FullName,
                        Date = rec.Date,
                        DateString = rec.Date.ToString("MM/dd/yyyy"),
                        HoursString = rec.Hours.ToString(),
                        MinutesString = rec.Minutes.ToString()
                    };
                    view.HoursList.Add(newView);
                }
            }

            // ============  not individual - can update all users and categories. ===========
            if (!isIndividual) 
            {
                if (Session["ActiveUsers"] == null)
                {
                    var activeUsers = HoursRoutines.GetActiveUsersSelectList();
                    Session["ActiveUsers"] = activeUsers;
                    view.UserList = activeUsers;
                }
                else
                {
                    view.UserList = (List<SelectListItem>)Session["ActiveUsers"];
                }

                var recs = db.VolunteerHours
                    .Where(h => h.Date >= wkBegin && h.Date <= wkEnd).ToList();
                foreach (var rec in recs)
                {
                    var newView = new VolunteerHoursViewModel
                    {
                        Id = rec.Id,
                        UserId = rec.UserId,
                        CategoryName = HoursRoutines.GetCategoryName(rec.Category),
                        Subcategory = rec.Subcategory,
                        Date = rec.Date,
                        DateString = rec.Date.ToString("MM/dd/yyyy"),
                        HoursString = rec.Hours.ToString(),
                        MinutesString = rec.Minutes.ToString()
                    };
                    var _usr = db.Users.Find(rec.UserId);
                    if (_usr != null)
                    { newView.VolunteerName = _usr.FullName; }
                    view.HoursList.Add(newView);
                }

                view.UserList = HoursRoutines.SetSelectedItem(view.UserList, usr.FullName);
                view.CategoryList = HoursRoutines.SetSelectedItem(view.CategoryList, usr.VolunteerCategory);
                view.SubcategoryList = HoursRoutines.SetSelectedItem(view.SubcategoryList, usr.VolunteerSubcategory);
            }

            return View(view);
        }

        //POST: Volunteer Hours Create 
        [HttpPost, AllowAnonymous]
        public ActionResult Create([Bind(Include = "UserId,Category,Subcategory,"
                                                   + "Date,Hours,Minutes")]
            VolunteerHoursViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

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

            if (model.Subcategory == null) model.Subcategory = "(none)";
                
            var newRec = new VolunteerHours()
            {
                UserId = model.UserId,
                OriginatorUserId = System.Web.HttpContext.Current.User.Identity.GetUserId(),
                Category = model.Category,
                Subcategory = model.Subcategory,
                Date = model.Date,
                Hours = model.Hours,
                Minutes = model.Minutes
            };

            db.VolunteerHours.Add(newRec);
            db.SaveChanges();
            return RedirectToAction("Create", new{hoursDate = newRec.Date });
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

            var submitError = string.Empty;
            if (TempData["SubmitError"] != null) submitError = TempData["SubmitError"].ToString();

            var view = new VolunteerHoursViewModel
            {
                Id = rec.Id, 
                UserId = rec.UserId,
                OriginatorUserId = rec.OriginatorUserId,
                IsIndividual = HoursRoutines.IsIndividual(currentUser.Id),
                CategoryName = HoursRoutines.GetCategoryName(rec.Category),
                Subcategory = rec.Subcategory,
                SubcategoryName = hoursUser.VolunteerSubcategory ?? "(none)",
                VolunteerName = hoursUser.FullName,
                Date = rec.Date,
                DateString = rec.Date.ToString("MM/dd/yyyy"),
                Hours = rec.Hours,
                HoursString = rec.Hours.ToString(),
                Minutes = rec.Minutes,
                MinutesString = rec.Minutes.ToString(),
                CategoryList = HoursRoutines.GetHoursCategoriesSelectList(),
                SubcategoryList = HoursRoutines.GetHoursSubcategoriesSelectList(hoursUser),
                SubmitError = submitError
            };

            return View(view);
        }

        //POST: Volunteer Hours Edit 
        [HttpPost, AllowAnonymous]
        public ActionResult Edit([Bind(Include = "Id,UserId,OriginatorUserId,Category,Subcategory,"
                                                 + "Date,Hours,Minutes,btnSave,btnDelete")]
            VolunteerHoursViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            if (model.BtnDelete != null)
            {
                var hrsRec = db.VolunteerHours.Find(model.Id);
                if (hrsRec == null) return RedirectToAction("Edit", new { recId = model.Id });
                db.VolunteerHours.Remove(hrsRec);
                db.SaveChanges();
                TempData["HoursDate"] = hrsRec.Date.ToString("MM/dd/yyyy");
                return RedirectToAction("Create");
            }

            if (model.Hours == 0 && model.Minutes == 0)
            {
                TempData["SubmitError"] = "No time was submitted!";
                return RedirectToAction("Edit", new{ recId = model.Id});
            }

            var rec = db.VolunteerHours.Find(model.Id);
            if (rec  == null) return RedirectToAction("Index", "Home");
            rec.UserId = model.UserId;
            rec.OriginatorUserId = model.OriginatorUserId;
            rec.Category = model.Category;
            rec.Subcategory = model.Subcategory;
            rec.Date = model.Date;
            rec.Hours = model.Hours;
            rec.Minutes = model.Minutes;
            db.SaveChanges();
            return RedirectToAction("Create");
        }
    }
}