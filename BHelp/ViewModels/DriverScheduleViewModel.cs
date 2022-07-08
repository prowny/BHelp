using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

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

        [StringLength(128)]
        public string BackupDriver2Id { get; set; }

        public string BackupDriver2Name { get; set; }

        public int GroupId { get; set; }
        public string GroupName { get; set; }

        [StringLength(128)]
        public string GroupDriverId { get; set; }

        public string GroupDriverName { get; set; }
      
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public bool IsHoliday { get; set; }
        public string DayString { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public List<SelectListItem> DriverList { get; set; }
        public List<SelectListItem> BackupDriverList { get; set; }
        public List<SelectListItem> BackupDriver2List { get; set; }
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

        public string[] BoxBackupDriver2Id { get; set; }
        public string[] BoxBackupDriver2Name { get; set; }
        public string[] BoxBackupDriver2Phone { get; set; }
        public string[] BoxBackupDriver2Phone2 { get; set; }
        public string[] BoxBackupDriver2Email { get; set; }

        public int[] BoxGroupId { get; set; }
        public string[] BoxGroupName { get; set; }

        public string[] BoxGroupDriverId { get; set; }
        public string[] BoxGroupDriverName { get; set; }
        public string[] BoxGroupDriverPhone { get; set; }
        public string[] BoxGroupDriverPhone2 { get; set; }
        public string[] BoxGroupDriverEmail { get; set; }

        [DataType(DataType.MultilineText)]
        public string[] BoxNote { get; set; }

        public bool[] BoxHoliday { get; set; }

        public bool IsDriverOnly { get; set; }
        public bool IsScheduler { get; set; }
        public bool AllowEdit { get; set; }
        public bool DriverConfirmed { get; set; }
        public DateTime CurrentDate { get; set; }  // to set mindate in datepicker
        public string CurrentUserId { get; set; }
        public List <SelectListItem> GroupList { get; set; }
    }
}