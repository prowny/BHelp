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
            var db = new BHelpContext();
            var activeUsers = db.Users.OrderBy(n => n.LastName)
                .Where(u => u.Active).ToList();
            foreach (var user in activeUsers)
            {
                var usr = new SelectListItem() { Value = user.Id, Text = user.FullName };
                list.Add(usr);
            }
            return list;
        }
        public static List<SelectListItem> GetHoursCategoriesSelectList()
        {
            List<SelectListItem> getHoursCategories = new List<SelectListItem>();
            var selListItem = new SelectListItem() { Value = "F", Text = @"Food Program" };
            getHoursCategories.Add(selListItem);    // Food Program
            selListItem = new SelectListItem() { Value = "A", Text = @"Administration" };
            getHoursCategories.Add(selListItem);     // Administration 
            selListItem = new SelectListItem() { Value = "M", Text = @"Management" };
            getHoursCategories.Add(selListItem);     // Management
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
            var db = new BHelpContext(); 
            var usr = db.Users.Find(usrId);
            bool isDeveloper = AppRoutines.UserIsInRole(usr.Id, "Developer");
            if (isDeveloper) return false;
            var isAdministrator = AppRoutines.UserIsInRole(usr.Id, "Administrator");
            if (isAdministrator) return false;
            var isStaff = AppRoutines.UserIsInRole(usr.Id, "Staff");
            if (isStaff) return false;
            var isNonFoodSeviceAdmin = AppRoutines.UserIsInRole(usr.Id, "NonFoodServiceAdministrationHours");
            if (isNonFoodSeviceAdmin) return false;
            var isNonFoodSeviceMgmnt = AppRoutines.UserIsInRole(usr.Id, "NonFoodServiceManagementHours");
            if (isNonFoodSeviceMgmnt) return false;
            return true;  // default unless in higher role       
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

            // for checking duplicate PeopleCounts:
            var catTotalEntriesSoFar = new List<VolunteerHoursViewModel>(); 

            foreach (var view in sortedList)
            {
                foreach (var catTotal in catTotalList)
                {
                    if (view.Category == catTotal.Category)
                    {
                        catTotal.TotalHours += view.Hours + view.Minutes / 60f;
                        // if this user has duplicate Cat & subcat this period, don't add to peoplecount
                        var okToAddPeople = true;
                        foreach (var _entry in catTotalEntriesSoFar)
                        {
                            if (view.Category == _entry.Category 
                                && view.Subcategory == _entry.Subcategory 
                                && view.UserId == _entry.UserId 
                                && view.PeopleCount == 1) // not a bulk entry
                            {
                                okToAddPeople = false;
                                break;
                            }
                        }
                        if(okToAddPeople) catTotal.PeopleCount += view.PeopleCount;
                        catTotalEntriesSoFar.Add(view);
                        break;
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
