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
            var listWeekDays = GetWeekdayArray();
            var Holidays = new List<Holiday>();
            var _holidayList = db.Holidays.ToList();
            var rangeTo = 28;
            List<int> rangeList31 = new List<int>() { 1, 3, 5, 7, 8, 10, 12 }; // Jan, Mar, May, Jul, Aug, Oct, Dec 
            List<int> rangeList30 = new List<int>() { 4, 6, 9, 11 }; // Apr, Jun, Sep, Nov
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
                    //var _holiday = new Holiday()
                    //{
                    //    CalculatedDate = AdjustForWeekendHoliday(new DateTime(year, hol.Month, hol.Day)),
                    //    Description = hol.Description,
                    //    EffectiveDate = hol.EffectiveDate
                    //};
                    //if (_holiday.CalculatedDate >= hol.EffectiveDate)
                    //{
                    //    Holidays.Add(_holiday);
                    //}
                }

                if (hol.Repeat == 2) // Annual Month-WeekNumber-WeekDay (like Thanksgiving)
                {
                    if (hol.Month == 2)
                    {
                        rangeTo = 28;
                        if (DateTime.IsLeapYear(year)) { rangeTo = 29; }
                    }
                    if (rangeList30.IndexOf(hol.Month) != -1) { rangeTo = 30; }
                    if (rangeList31.IndexOf(hol.Month) != -1) { rangeTo = 31; }
                    var dtDayOfWeek = listWeekDays[hol.Weekday];
                    var dtDay = 0;
                    var weekdayCount = 0;
                    for (var i = 1; i < rangeTo + 1; i++)
                    {
                        var dtDate = new DateTime(year, hol.Month, i);
                        if (dtDate.DayOfWeek.ToString() != dtDayOfWeek) continue;
                        weekdayCount++;
                        if (weekdayCount != hol.WeekNumber + 1) continue;
                        dtDay = i;
                        break;
                    }

                    // routine fails for 4th Thursday: ************
                    //var dtDay = (from day in Enumerable.Range(1, rangeTo)
                    //             where (new DateTime(year, hol.Month, day).DayOfWeek.ToString() == dtDayOfWeek)
                    //             select day).ElementAt(hol.WeekNumber +1);
                    
                    var _holiday = new Holiday
                    {
                        CalculatedDate = new DateTime(year, hol.Month, dtDay),
                        Description = hol.Description,
                        EffectiveDate = hol.EffectiveDate
                    };
                    if (_holiday.CalculatedDate >= hol.EffectiveDate)
                    {
                        Holidays.Add(_holiday);
                    }
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
            // check if ANY calculated date in proper year:
            foreach (var hol in holidays)
            {
                if (hol.CalculatedDate.Year == dt.Year) { break; }
                // else load requested year's holidays:
                holidays = GetHolidays(dt.Year);
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
            var weekdayList = new string[6];
            weekdayList[0] = "Monday"; weekdayList[1] = "Tuesday"; weekdayList[2] = "Wednesday";
            weekdayList[3] = "Thursday"; weekdayList[4] = "Friday";
            weekdayList[5] = "Last"; 
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