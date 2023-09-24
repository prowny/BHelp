using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Diagrams;

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
        public string[] BoxDriverId { get; set; }
        public string[] BoxDriverName { get; set; }
        public string[] BoxODId { get; set; }
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
        public bool[] BoxDriverSignup { get; set; }
        public bool[] BoxDriverUnsign { get; set; }
        public bool[] BoxBackupDriverSignup { get; set; }
        public bool[] BoxBackupDriverUnsign { get; set; }
        public string[] BoxHolidayDescription { get; set; }
        public bool IsDriverOnly { get; set; }
        public bool IsScheduler { get; set; }
        public bool AllowEdit { get; set; }
        public bool DriverConfirmed { get; set; }
        public DateTime CurrentDate { get; set; }  // to set mindate in datepicker
        public string CurrentUserId { get; set; }
        public List <SelectListItem> GroupList { get; set; }
        public List<BoxSignUp> BoxDriverSignUps { get; set; }
        public List<BoxSignUp> BoxDriverUnSignUps { get; set; }
        public List<BoxSignUp> BoxBackupDriverSignUps { get; set; }
        public List<BoxSignUp> BoxBackupDriverUnSignUps { get; set; }

        public Dictionary<int, bool> DriverSignup { get; set; }

        // create constructor to initialize the arrays
        public DriverScheduleViewModel()
        {
            BoxDriverId = new string[26];
            BoxDriverName = new string[26];
            BoxODId = new string[26];
            BoxDriverPhone = new string[26];
            BoxDriverPhone2 = new string[26];
            BoxDriverEmail = new string[26];
            BoxDriverConfirmed = new bool[26];
            BoxBackupDriverId = new string[26];
            BoxBackupDriverName = new string[26];
            BoxBackupDriverPhone = new string[26];
            BoxBackupDriverPhone2 = new string[26];
            BoxBackupDriverEmail = new string[26];
            BoxBackupDriver2Id = new string[26];
            BoxBackupDriver2Name = new string[26];
            BoxBackupDriver2Phone = new string[26];
            BoxBackupDriver2Phone2 = new string[26];
            BoxBackupDriver2Email = new string[26];
            BoxGroupId = new int[26];
            BoxGroupName = new string[26];
            BoxGroupDriverId = new string[26];
            BoxGroupDriverName = new string[26];
            BoxGroupDriverPhone = new string[26];
            BoxGroupDriverPhone2 = new string[26];
            BoxGroupDriverEmail = new string[26];
            BoxNote = new string[26];
            BoxHoliday = new bool[26];
            BoxHolidayDescription = new string[26];
            BoxDriverSignup = new bool[26];
            BoxDriverUnsign = new bool[26];
            BoxBackupDriverSignup = new bool[26];
            BoxBackupDriverUnsign = new bool[26];
            BoxDriverSignUps = new List<BoxSignUp>();
            BoxDriverUnSignUps = new List<BoxSignUp>();
            BoxBackupDriverSignUps = new List<BoxSignUp>();
            BoxBackupDriverUnSignUps = new List<BoxSignUp>();
            DriverSignup = new Dictionary<int, bool>();
           
            for (var i = 0; i < 26; i++)
            {
                var nbsu = new BoxSignUp { Id = i, Checked = false };
                BoxDriverSignUps.Add(nbsu);
                BoxDriverUnSignUps.Add(nbsu);
                BoxBackupDriverSignUps.Add(nbsu);
                BoxBackupDriverUnSignUps.Add(nbsu);

                DriverSignup.Add( i, false);
            }
        }
    }
}
