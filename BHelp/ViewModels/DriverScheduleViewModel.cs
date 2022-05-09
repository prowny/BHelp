using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class DriverScheduleViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [StringLength(128)]
        public string DriverId { get; set; }

        public string DriverName { get; set; }

        [StringLength(128)]
        public string BackupDriverId { get; set; }

        public string BackupDriverName { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public string DayString { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public List<SelectListItem> DriverList { get; set; }
        public List<SelectListItem> BackupDriverList { get; set; }
        public List<DriverScheduleViewModel> DriversSchedule { get; set; }
        public DateTime[,] BoxDay { get; set; }
        public string[] BoxDriverName { get; set; }
       
        public string[] BoxDriverId { get; set; }
        public string[] BoxDriverPhone { get; set; }
        public string[] BoxDriverPhone2 { get; set; }
        public string[] BoxDriverEmail { get; set; }
        public bool[] BoxDriverConfirmed { get; set; }

        public string[] BoxBackupDriverId { get; set; }
        public string[] BoxBackupDriverName { get; set; }
        public string[] BoxBackupDriverPhone { get; set; }
        public string[] BoxBackupDriverPhone2 { get; set; }
        public string[] BoxBackupDriverEmail { get; set; }

        [DataType(DataType.MultilineText)]
        public string[] BoxNote { get; set; }
        public bool IsDriverOnly { get; set; }
        public bool IsScheduler { get; set; }
        public bool AllowEdit { get; set; }
        public bool DriverConfirmed { get; set; }
        public DateTime CurrentDate { get; set; }  // to set mindate in datepicker
        public string CurrentUserId { get; set; }
    }
}