using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp
{
    public static class HolidayRoutines
    {
        public static string[] GetRepeatArray()
        {
            var repeatList = new string[3];
            repeatList[0] = "Does not repeat";
            repeatList[1] = "Annually on fixed month/day";
            repeatList[2] = "Annually on fixed month/week/day";
            return repeatList;
        }

        public static string[] GetMonthArray()
        {
            var monthList = new string[13];
            monthList[1] = "January"; monthList[2] = "February"; monthList[3] = "March";
            monthList[4] = "April"; monthList[5] = "May"; monthList[6] = "June";
            monthList[7] = "July"; monthList[8] = "August"; monthList[9] = "September";
            monthList[10] = "October"; monthList[11] = "November"; monthList[12] = "December";
            return monthList;
        }
        
        public static string[] GetWeekdayArray()
        {
            var weekdayList = new string[5];
            weekdayList[0] = "Monday"; weekdayList[1] = "Tuesday"; weekdayList[2] = "Wednesday";
            weekdayList[3] = "Thursday"; weekdayList[4] = "Friday";
            return weekdayList;
        }
        public static string[] GetWeekdayNumberArray()
        {
            var weekdayNumber = new string[5];
            weekdayNumber[0] = "First"; weekdayNumber[1] = "Second"; 
            weekdayNumber[2] = "Third"; weekdayNumber[3] = "Fourth";
            weekdayNumber[4] = "Last";
            return weekdayNumber;
        }

        public static List<SelectListItem> GetRepeatsSelectList()
        {
            var repeatList = GetRepeatArray();
            var repeats = new List<SelectListItem>();
            var i = -1;
            foreach (var repeat in repeatList )
            {
                i++;
                var rep = new SelectListItem
                    {
                        Value = i.ToString(),
                        Text = repeat
                    };
                repeats.Add(rep);
            }
            return repeats;
        }

        public static List<SelectListItem> GetMonthsSelectList()
        {
            var monthList = GetMonthArray();
            var months = new List<SelectListItem>();
            var i = 0;
            foreach (var month in monthList)
            {
                if (i == 0) { i++; continue; }
                var rep = new SelectListItem
                {
                    Value = i.ToString(),
                    Text = month
                };
                if (i == 1)
                {
                    rep.Selected = true;
                }
                months.Add(rep);
                i++;
            }
            return months;
        }

        public static List<SelectListItem> GetWeekDayNumberSelectList()
        {
            var weekdayNumberList = GetWeekdayNumberArray();
            var weekdayNumbers = new List<SelectListItem>();
            var i = -1;
            foreach (var number in weekdayNumberList)
            {
                i++;
                var rep = new SelectListItem
                {
                    Value = i.ToString(),
                    Text = number
                };
                weekdayNumbers.Add(rep);
            }
            return weekdayNumbers;
        }

        public static List<SelectListItem> GetDaysSelectList()
        {
            List<SelectListItem> days = new List<SelectListItem>();
            for (int i = 1; i < 32; i++)
            {
                days.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }
            return days;
        }

        public static List<SelectListItem> GetWeekDaySelectList()
        {
            var weekdayList = GetWeekdayArray();
            var weekdayDayList = new List<SelectListItem>();
            var i = -1;
            foreach (var number in weekdayList)
            {
                i++;
                var rep = new SelectListItem
                {
                    Value = i.ToString(),
                    Text = number
                };
                weekdayDayList.Add(rep);
            }
            return weekdayDayList;
        }
    }
}