using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Castle.Core.Internal;

namespace BHelp
{
    public static class HoursRoutines
    {
        public static List<SelectListItem> GetHoursCategories(string cat)
        {
            string selectCat = "";
            if (!cat.IsNullOrEmpty()) selectCat = cat;
            List<SelectListItem> getHoursCategories = new List<SelectListItem>();
            var selListItem = new SelectListItem() { Value = "F", Text = @"F" };
            if (selectCat == "F") selListItem.Selected = true; 
            getHoursCategories.Add(selListItem);    // Food Service
            selListItem = new SelectListItem() { Value = "A", Text = @"A" };
            if (selectCat == "A") selListItem.Selected = true;
            getHoursCategories.Add(selListItem);    // Administration
            selListItem = new SelectListItem() { Value = "M", Text = @"M" };
            if (selectCat == "M") selListItem.Selected = true;
            getHoursCategories.Add(selListItem);     // Management
            return getHoursCategories;
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
        public static List<SelectListItem> GetHoursSubcategories()
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
            return subCatList;
        }
        public static List<SelectListItem> GetUsersSelectList()
        {
            var db = new BHelpContext();
            var usrList = db.Users.Where(u => u.Active)
                .OrderBy(u => u.LastName).ToList();
            var usrsSelectList = new List<SelectListItem>();
            foreach (var user in usrList)
            {
                var item = new SelectListItem() { Text = user.FullName, Value = user.Id, Selected = false };
                usrsSelectList.Add(item);
            }

            return usrsSelectList;
        }
        public static DateTime GetPreviousSaturday(DateTime curDt)
        {
            var lastSaturday = curDt;
            while (lastSaturday.DayOfWeek != DayOfWeek.Saturday)
                lastSaturday = lastSaturday.AddDays(-1);
            return lastSaturday;
        }
        public static Boolean IsIndividual(string usrId)
        {
            var db = new BHelpContext();
            var usr = db.Users.Find(usrId);
            bool isDeveloper = AppRoutines.UserIsInRole(usr.Id, "Developer");
            if (isDeveloper)
            {
                var isAdministrator = AppRoutines.UserIsInRole(usr.Id, "Administrator");
                if (isAdministrator)
                {
                    var isStaff = AppRoutines.UserIsInRole(usr.Id, "Staff");
                    if (isStaff)
                    {
                        return false; // can enter hours for anyone, any category 
                    }
                }
            }
            return true;  // default unless in higher role
        }

        public static List<SelectListItem> SetSelectedSubcategory( List<SelectListItem> list, string subcategory)
        {
            foreach (var item in list)
            {
                if (item.Text == subcategory)
                {
                    item.Selected = true;
                    break;
                }
            }
            return list;
        }
    }
}
