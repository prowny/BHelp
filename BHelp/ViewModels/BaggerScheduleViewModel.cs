using BHelp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class BaggerScheduleViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TodayYearMonth { get; set; }

        [StringLength(128)]
        public string BaggerId { get; set; }
        public string BaggerName { get; set; }

        [StringLength(128)]
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public bool IsHoliday { get; set; }
        public bool IsFriSatSun { get; set; }
        public string DayString { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public List<SelectListItem> BaggerList { get; set; }  // for DropDownList SelectList
        public List<ApplicationUser> BaggerDataList { get; set; } // for Phone/Email with index matching BaggerList
        public List<SelectListItem>PartnerList { get; set; }  // for DropDownList SelectList
        public List<ApplicationUser> PartnerDataList { get; set; } // for Phone/Email with index matching PartnerList
        public List<BaggerScheduleViewModel> BaggersSchedule { get; set; }
        public DateTime[,] BoxDay { get; set; }  // Boxes refer to cells on calendar
        public string[] BoxBaggerName { get; set; }
        public string[] BoxBaggerId { get; set; }
        public string[] BoxBaggerPhone { get; set; }
        public string[] BoxBaggerPhone2 { get; set; }
        public string[] BoxBaggerEmail { get; set; }
        public string[] BoxPartnerName { get; set; }  // Optonal husband-wife teammate
        public string[] BoxPartnerId { get; set; }
        public string[] BoxPartnerPhone { get; set; }
        public string[] BoxPartnerPhone2 { get; set; }
        public string[] BoxPartnerEmail { get; set; }
        public bool[] BoxHoliday { get; set; }
        public string[] BoxHolidayDescription { get; set; }
        public bool[] BoxFriSatSun { get; set; }

        [DataType(DataType.MultilineText)]
        public string[] BoxNote { get; set; }

        public bool AllowEdit { get; set; }  // for view-only users
        public string CurrentUserId { get; set; }

        public string OldBaggerId { get; set; }
        public bool IsScheduler { get; set; } // can update everything
        public bool IsBaggerOnly { get; set; }  // is date-restricted.
    }
}
