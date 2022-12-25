using System.Collections.Generic;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using System.Linq;
using System;
using BHelp.Models;

namespace BHelp
{
    public static class HolidayRoutines
    {
        private static readonly BHelpContext db = new BHelpContext();
        public static List<Holiday> GetHolidays(int year)
        {
            string[] listWeekDays = GetWeekdayArray();
            var Holidays = new List<Holiday>();
            var _holidayList = db.Holidays.ToList();
            foreach (var hol in _holidayList)
            {
                if (hol.Repeat == 0) // Fixed Date, no repeats
                {
                    var _holiday = new Holiday()
                    {
                        CalculatedDate = hol.FixedDate,
                        FixedDate = hol.FixedDate,
                        Description = hol.Description
                    };
                    Holidays.Add(_holiday);
                }

                if (hol.Repeat == 1) // Annual Month-Day
                {
                    var _holiday = new Holiday()
                    {
                        CalculatedDate = AdjustForWeekendHoliday(new DateTime(year, hol.Month, hol.Day).Date),
                        Description = hol.Description
                    };
                    Holidays.Add(_holiday);
                }

                if (hol.Repeat == 2) // Annual Month-WeekNumber-WeekDay
                {
                    var dtDayOfWeek = listWeekDays[hol.Weekday];
                    var dtDay = (from day in Enumerable.Range(1, 31)
                        where (new DateTime(year, hol.Month, day).DayOfWeek.ToString() == dtDayOfWeek)
                                 select day).ElementAt(hol.WeekNumber +1);
                    var _holiday = new Holiday()
                    {
                        CalculatedDate = new DateTime(year, hol.Month, dtDay),
                        Description = hol.Description
                    };
                    Holidays.Add(_holiday);
                }
            }

            return (Holidays);
        }

        private static DateTime AdjustForWeekendHoliday(DateTime holiday)
        {
            if (holiday.DayOfWeek == DayOfWeek.Saturday)
            {
                return holiday.AddDays(-1);
            }
            else if (holiday.DayOfWeek == DayOfWeek.Sunday)
            {
                return holiday.AddDays(1);
            }
            else
            {
                return holiday;
            }
        }
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

        public static bool IsHoliday(DateTime dt, List<Holiday> holidays)
        {
            // check 4th of July for proper year:
            var july4th = holidays.FirstOrDefault(j => j.CalculatedDate.Month == 7
                                                       && j.CalculatedDate.Day == 4);
            if (july4th != null)
            {
                if (july4th.CalculatedDate.Year != dt.Year) // need to reloadholidays (year change)
                {
                    holidays = GetHolidays(dt.Year);
                }
            }

            foreach (var hol in holidays)
            {
                if (dt == hol.CalculatedDate)
                {
                    return true;
                }
            }
            return false;
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