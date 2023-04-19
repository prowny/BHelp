using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp
{
    public static class HoursRoutines
    {
        public static List<SelectListItem> GetActiveUsersSelectList()
        {
            var list = new List<SelectListItem>();
            using (var db = new BHelpContext())
            {
                var activeUsers = db.Users.OrderBy(n => n.LastName)
                    .Where(u => u.Active).ToList();
                foreach (var user in activeUsers)
                {
                    var usr = new SelectListItem() { Value = user.Id, Text = user.FullName };
                    list.Add(usr);
                }

                return list;
            }
        }
        public static List<SelectListItem> GetHoursCategoriesSelectList(bool? IsNonFoodServiceAdministration, bool IsNonFoodServiceManagement)
        {
            List<SelectListItem> getHoursCategories = new List<SelectListItem>();
            var selListItem = new SelectListItem() { Value = "F", Text = @"Food Program" };
            getHoursCategories.Add(selListItem);    // Food Program
            selListItem = new SelectListItem() { Value = "A", Text = @"Administration" };
            getHoursCategories.Add(selListItem);     // Administration 
            selListItem = new SelectListItem() { Value = "M", Text = @"Management" };
            getHoursCategories.Add(selListItem);     // Management
            if (IsNonFoodServiceAdministration != null && (bool)IsNonFoodServiceAdministration)
            {
                if (!IsNonFoodServiceManagement)
                { // remove Management
                    List<SelectListItem> _newSelectList = getHoursCategories
                        .Where(c => c.Value != "M").ToList();
                    getHoursCategories = _newSelectList;
                }
            }
            if (IsNonFoodServiceManagement)
            {
                if (IsNonFoodServiceAdministration != null && (bool)!IsNonFoodServiceAdministration)
                { // remove Administration
                    List<SelectListItem> _newSelectList = getHoursCategories
                        .Where(c => c.Value != "A").ToList();
                    getHoursCategories = _newSelectList;
                }
            }
            return getHoursCategories;
        }
        public static List<SelectListItem> GetHoursSubcategoriesSelectList(ApplicationUser usr)
        {
            List<SelectListItem> subCatList = new List<SelectListItem>();
            var selListItem = new SelectListItem() { Value = "(none)", Text = @"(none)" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Bagger", Text = @"Bagger" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Bagger Supervisor", Text = @"Bagger Supervisor" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Driver", Text = @"Driver" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Food Staff", Text = @"Food Staff" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Food Staff Supervisor", Text = @"Food Staff Supervisor" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "OD", Text = @"OD" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Scheduler", Text = @"Scheduler" };
            subCatList.Add(selListItem);
            selListItem = new SelectListItem() { Value = "Trainer", Text = @"Trainer" };
            subCatList.Add(selListItem);
            return subCatList;
        }
        public static string GetCategoryName(string catName)
        {
            switch (catName)
            {
                case "A": return "Administration";
                case "M": return "Management";
                case "F": return "Food Program";
                default: return "Food Program";
            }
        }
        public static DateTime GetPreviousFriday(DateTime curDt)
        {
            var lastFriday = curDt;
            while (lastFriday.DayOfWeek != DayOfWeek.Friday )
                lastFriday = lastFriday.AddDays(-1);
            return lastFriday;
        }
        public static DateTime GetPreviousMonday(DateTime curDt)
        {
            var lastMonday = curDt;
            while (lastMonday.DayOfWeek != DayOfWeek.Monday)
                lastMonday = lastMonday.AddDays(-1);
            return lastMonday;
        }
        public static bool IsIndividual(string usrId)
        {
            using (var db = new BHelpContext())
            {
                var usr = db.Users.Find(usrId);
                bool IsNonFoodServiceAdministration =
                    AppRoutines.UserIsInRole(usr.Id, "NonFoodServiceAdministrationHours");
                if (IsNonFoodServiceAdministration) return true;
                bool IsNonFoodServiceManagement = AppRoutines.UserIsInRole(usr.Id, "NonFoodServiceManagementHours");
                if (IsNonFoodServiceManagement) return true;
                bool isDeveloper = AppRoutines.UserIsInRole(usr.Id, "Developer");
                if (isDeveloper) return false;
                var isAdministrator = AppRoutines.UserIsInRole(usr.Id, "Administrator");
                if (isAdministrator) return false;
                var isStaff = AppRoutines.UserIsInRole(usr.Id, "Staff");
                if (isStaff) return false;
                var isPantryCoordinator = AppRoutines.UserIsInRole(usr.Id, "PantryCoordinator");
                if (isPantryCoordinator) return false;
                return true; // default unless in higher role
            }
        }

        public static bool IsNonFoodServiceAdministration(string usrId)
        {
            using (var db = new BHelpContext())
            {
                var usr = db.Users.Find(usrId);
                return AppRoutines.UserIsInRole(usr.Id, "NonFoodServiceAdministrationHours");
            }
        }

        public static bool IsNonFoodServiceManagement(string usrId)
        {
            using (var db = new BHelpContext())
            {
                var usr = db.Users.Find(usrId);
                return AppRoutines.UserIsInRole(usr.Id, "NonFoodServiceManagementHours");
            }
        }

        public static List<SelectListItem> SetSelectedItem( List<SelectListItem> list, string text)
        {
            foreach (var item in list)
            {
                item.Selected = false;
                if (item.Text == text) { item.Selected = true; }
            }
            return list;
        }
        public static List<VolunteerHoursTotalsViewModel> GetTotalsList(List<VolunteerHoursViewModel> list)
        {
            var sortedList = list.OrderBy(s => s.Subcategory)
                .ThenByDescending(c => c.Category).ToList();
            var returnList = new List<VolunteerHoursTotalsViewModel>();

            IEnumerable<VolunteerHoursTotalsViewModel> totalList = new List<VolunteerHoursTotalsViewModel>
            {
                new VolunteerHoursTotalsViewModel(){Category ="A", Subcategory = "(none)",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="M", Subcategory = "(none)",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Bagger",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Bagger Supervisor",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Driver",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Food Staff",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Food Staff Supervisor",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "OD",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Scheduler",PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", Subcategory = "Trainer",PeopleCount =0,TotalHours = 0}
            };
   
            foreach (var view in sortedList)
            {
                foreach (var total in totalList)
                {
                    if (view.Category == total.Category && view .Subcategory == total.Subcategory)
                    {
                        if (total.UserId == null)
                        {
                            total.UserId = view.UserId;
                            total.PeopleCount = 1;
                        }
                        if (view.UserId != total.UserId)
                        { total.PeopleCount++;}

                        total.TotalHours +=  view.Hours + view.Minutes / 60f;
                        break;
                    }
                }
            }
            foreach (var _view in totalList)
            {
                if (_view.TotalHours != 0)
                {
                    _view.CategoryName = GetCategoryName(_view.Category);
                    returnList.Add(_view);
                }
            }
            return returnList;
        }
        public static List<VolunteerHoursTotalsViewModel> GetSummaryTotalsList(List<VolunteerHoursViewModel> list)
        {
            var sortedList = list.OrderBy(s => s.Subcategory)
                .ThenByDescending(c => c.Category).ToList();
            var returnList = new List<VolunteerHoursTotalsViewModel>();

            IEnumerable<VolunteerHoursTotalsViewModel> catTotalList = new List<VolunteerHoursTotalsViewModel>
            {
                new VolunteerHoursTotalsViewModel(){Category ="A", CategoryName = "Administration", PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="M", CategoryName = "Management", PeopleCount =0,TotalHours = 0},
                new VolunteerHoursTotalsViewModel(){Category ="F", CategoryName = "Food Program", PeopleCount =0,TotalHours = 0},
            };

            // for checking duplicate PeopleCounts: (removed 10/01/202)
            //var catTotalEntriesSoFar = new List<VolunteerHoursViewModel>(); 

            foreach (var view in sortedList)
            {
                foreach (var catTotal in catTotalList)
                {
                    if (view.Category == catTotal.Category)
                    {
                        catTotal.TotalHours += view.Hours + view.Minutes / 60f;
                        catTotal.PeopleCount += view.PeopleCount;
                    }
                }
            }
            foreach (var _view in catTotalList)
            {
                if (_view.TotalHours != 0)
                {
                    _view.CategoryName = GetCategoryName(_view.Category);
                    returnList.Add(_view);
                }
            }
            return returnList;
        }
    }
}
