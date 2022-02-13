using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BHelp.Models;

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
            var selListItem = new SelectListItem() { Value = "F", Text = @"Food Service" };
            getHoursCategories.Add(selListItem);    // Food Service
            selListItem = new SelectListItem() { Value = "A", Text = @"Administration" };
            getHoursCategories.Add(selListItem);    // Administration
            selListItem = new SelectListItem() { Value = "M", Text = @"Management" };
            getHoursCategories.Add(selListItem);     // Management
            return getHoursCategories;
        }
        public static List<SelectListItem> GetHoursSubcategoriesSelectList(ApplicationUser usr)
        {
            List<SelectListItem> subCatList = new List<SelectListItem>();
            var selListItem = new SelectListItem() { Value = "(none)", Text = @"(none)" };
            subCatList.Add(selListItem);

            if (usr.VolunteerCategory == "M" || usr.VolunteerCategory == "A")
            { return subCatList; }

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
            return subCatList;
        }
        public static string GetCategoryName(string catName)
        {
            switch (catName)
            {
                case "A": return "Administration";
                case "M": return "Management";
                case "F": return "Food Service";
                default: return "Food Service";
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
    }
}
