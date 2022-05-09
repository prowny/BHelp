using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class ODScheduleViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [StringLength(128)]
        public string ODId { get; set; }
        public string ODName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public string DayString { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public List<SelectListItem> ODList { get; set; }  // for DropDownList SelectList
        public List<ApplicationUser> ODDataList { get; set; } // for Phone/Email with index matching ODList
        public List<ODScheduleViewModel> ODsSchedule { get; set; }
        public DateTime[,] BoxDay { get; set; }  // Boxes refer to cells on calendar
        public string[] BoxODName { get; set; }
        public string[] BoxODId { get; set; }
        public string[] BoxODPhone { get; set; }
        public string[] BoxODPhone2 { get; set; }
        public string[] BoxODEmail { get; set; }
        public bool[] BoxODConfirmed { get; set; }

        [DataType(DataType.MultilineText)]
        public string[] BoxNote { get; set; }
        
        public bool AllowEdit { get; set; }  // for view-only users
        public bool ODConfirmed { get; set; }
        public string CurrentUserId { get; set; }
        
        public string OldODId { get; set; }
        public bool IsScheduler { get; set; } // can update everything
        public bool IsODOnly { get; set; }  // is date-restricted.
    }
}