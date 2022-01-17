using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp
{
    public static class HoursRoutines
    {
        public static List<SelectListItem> GetHoursCategories()
        {
            List<SelectListItem> getHoursCategories = new List<SelectListItem>();
            var selListItem = new SelectListItem() { Value = "F", Text = @"F" };
            getHoursCategories.Add(selListItem);    // Food Service
            selListItem = new SelectListItem() { Value = "A", Text = @"A" };
            getHoursCategories.Add(selListItem);    // Administration
            selListItem = new SelectListItem() { Value = "M", Text = @"M" };
            getHoursCategories.Add(selListItem);     // Management
            return getHoursCategories;
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
        public static DateTime GetPreviousFriday(DateTime curDt)
        {
            DateTime lastFriday = curDt.AddDays(-1);
            while (lastFriday.DayOfWeek != DayOfWeek.Friday )
                lastFriday = lastFriday.AddDays(-1);
            return lastFriday;
        }

        public static string GetCategoryName(string cat)
        {
            switch (cat)
            {
                case "A": return "Administration";
                case "M": return "Management";
                case "F": return "Food Service";
                default: return "Food Service";
            }
        }
    }
}
