using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
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
            if (Session["CurrentUserId"] == null) // set to logged-in user
            { Session["CurrentUserId"] = User.Identity.GetUserId(); }
            
            if (Session["IsIndividual"] == null)
            { Session["IsIndividual"] = HoursRoutines.IsIndividual(Session["CurrentUserId"].ToString()); }

            if (Session["HoursUserId"] == null) // initially set to logged-in user
            { Session["HoursUserId"] = Session["CurrentUserId"]; }
            if (userId != null)
            { Session["HoursUserId"] = userId; }

            if (Session["HoursDate"] == null) // initially set to today, or last Friday
            {
                var _hd =DateTime.Today;
                if (_hd.DayOfWeek == DayOfWeek.Saturday || _hd.DayOfWeek == DayOfWeek.Sunday)
                { _hd = HoursRoutines.GetPreviousFriday(_hd);}
                Session["HoursDate"] = _hd;
            }
            if (hoursDate != null)
            { Session["HoursDate"] = hoursDate; }


            var _id = Session["HoursUserId"].ToString();
            var _hoursUser = db.Users.Find(_id);
            var catName = HoursRoutines.GetCategoryName(_hoursUser.VolunteerCategory) ?? "(none)";
            var subcatName = _hoursUser.VolunteerSubcategory ?? "(none)";
            bool _isIndividual = (bool)Session["IsIndividual"];

            var entryDate = (DateTime)Session["HoursDate"];
            var wkBegin = HoursRoutines.GetPreviousMonday(entryDate);
            var wkBeginString = wkBegin.ToString("MM/dd/yyyy");
            var wkEnd = wkBegin.AddDays(4);
            var wkEndString = wkEnd.ToString("MM/dd/yyyy");

            var submitError = string.Empty;
            if (TempData["SubmitError"] != null) submitError = TempData["SubmitError"].ToString();

            var view = new VolunteerHoursViewModel
            {
                UserId = _hoursUser.Id,
                UserFullName = _hoursUser.FullName,
                Category = _hoursUser.VolunteerCategory ?? "(none)",
                Subcategory = _hoursUser.VolunteerSubcategory ?? "(none)",
                VolunteerName = _hoursUser.FullName,
                CategoryName = catName,
                SubcategoryName = subcatName,
                Date = entryDate,
                DateString = entryDate.ToString("MM/dd/yyyy"),
                WeekBeginningDate = wkBegin,
                WeekBeginningDateString = wkBeginString,
                WeekEndingDate = wkEnd,
                WeekEndingDateString = wkEndString,
                SubmitError = submitError,
                IsIndividual = _isIndividual,
                HoursList = new List<VolunteerHoursViewModel>(),
                CategoryList =HoursRoutines.GetHoursCategoriesSelectList(),
                SubcategoryList =HoursRoutines.GetHoursSubcategoriesSelectList(_hoursUser)
            };

            if (_isIndividual) // get hours for individual only
            {
                var recs = db.VolunteerHours
                    .Where(h => h.UserId == _hoursUser.Id
                                && h.Date >= wkBegin && h.Date <= wkEnd).ToList();
                foreach (var rec in recs)
                {
                    var newView = new VolunteerHoursViewModel
                    {
                        Id = rec.Id,
                        UserId = _hoursUser.Id,
                        UserFullName = _hoursUser.FullName,
                        CategoryName = HoursRoutines.GetCategoryName(rec.Category),
                        Subcategory = rec.Subcategory,
                        VolunteerName = _hoursUser.FullName,
                        Date = rec.Date,
                        DateString = rec.Date.ToString("MM/dd/yyyy"),
                        HoursString = rec.Hours.ToString(),
                        MinutesString = rec.Minutes.ToString()
                    };
                    view.HoursList.Add(newView);
                }
                // Save for Edit:
                TempData["IndividualViewModel"] = view;
            }

            // ============  not individual - can update all users and categories. ===========
            if (!_isIndividual) 
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

                view.UserList = HoursRoutines.SetSelectedItem(view.UserList, _hoursUser.FullName);
                view.CategoryList = HoursRoutines.SetSelectedItem(view.CategoryList, _hoursUser.VolunteerCategory);
                view.SubcategoryList = HoursRoutines.SetSelectedItem(view.SubcategoryList, _hoursUser.VolunteerSubcategory);
            }

            return View(view);
        }

        //POST: Volunteer Hours Create 
        [HttpPost, AllowAnonymous]
        public ActionResult Create([Bind(Include = "UserId,Category,Subcategory,"
                                                   + "Date,Hours,Minutes,IsIndividual")]
            VolunteerHoursViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Index", "Home");

            if (model.Hours == 0 && model.Minutes == 0)
            {
                TempData["SubmitError"] = "No time was submitted!";
                return RedirectToAction("Create");
            }

            if (model.IsIndividual)
            {
                var view = (VolunteerHoursViewModel)TempData["IndividualViewModel"];
                model.UserId = view.UserId;
                model.Category = view.Category;
                model.Subcategory = view.Subcategory;
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
                return RedirectToAction("Create", new{userId = model.UserId });
            }

            if (model.Subcategory == null) model.Subcategory = "(none)";
                
            var newRec = new VolunteerHours
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