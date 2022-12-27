using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class Holiday
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public int Repeat { get; set; } // 0: does not repeat, 1: annual day, 2: annual weekday 
        public DateTime FixedDate { get; set; } // non-repeating date
        public int Month { get; set; } // 0: none, 1 - 12: repeating annual month
        public int Day { get; set; } // 0: none, 1 -31: repeating annual day of month
        public int Weekday { get; set; } // 0: none, 1 - 5: Mon-Tue-Wed-Thu-Fri
        public int WeekNumber { get; set; } // 0: none, 1- 4: 1st, 2nd, 3rd, 4th, last: 5
        public DateTime EffectiveDate { get; set; }
        
        [NotMapped]
        public string MonthDay { get; set; } // for sorting holiday list
        [NotMapped] 
        public string[] RepeatList { get; set; }
        [NotMapped]
        public string[] MonthList { get; set; } // Jan - Dec
        [NotMapped]
        public string[] WeekDayList { get; set; } // Mon - Fri
        [NotMapped]
        public string[] WeekDayNumber { get; set; } // 1st - 4th, Last
        [NotMapped]
        public DateTime CalculatedDate { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Repeats { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Months { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> Days { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> WeekDays { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem> WeekDayNumbers { get; set; }
        [NotMapped] 
        public string WeekDayName { get; set; } // for display in Holidays Index 
    }
}