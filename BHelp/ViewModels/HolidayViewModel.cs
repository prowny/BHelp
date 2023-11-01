using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class HolidayViewModel
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public int Repeat { get; set; } // 0: does not repeat, 1: annual day, 2: annual weekday 
        public DateTime FixedDate { get; set; } // non-repeating date
        public int Month { get; set; } // 0: none, 1 - 12: repeating annual month
        public int Day { get; set; } // 0: none, 1 -31: repeating annual day of month
        public int Weekday { get; set; } // 0: none, 1 - 5: Mon-Tue-Wed-Thu-Fri
        public int WeekNumber { get; set; } // 0: none, 1- 4: 1st, 2nd, 3rd, 4th, last: 5
        public DateTime EffectiveDate { get; set; }
        public string WeekDayName { get; set; } // for display in Holidays Index 
        public string MonthDay { get; set; } // for sorting holiday list
        public string[] RepeatList { get; set; }
        public string[] MonthList { get; set; } // Jan - Dec
        public string[] WeekDayList { get; set; } // Mon - Fri
        public IEnumerable<SelectListItem> Repeats { get; set; }
        public IEnumerable<SelectListItem> Months { get; set; }
        public IEnumerable<SelectListItem> Days { get; set; }
        public IEnumerable<SelectListItem> WeekDays { get; set; }
        public IEnumerable<SelectListItem> WeekDayNumbers { get; set; }
    }
}