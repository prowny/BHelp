using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class DriverScheduleViewModel
    {
        public DriverScheduleViewModel()
        {
            //BoxDDL = new object[6, 6, 3];  // row, col, ddl1/ddl2
            //BoxDDLDriverId = new string[6, 6, 3];
            //BoxIndexDriverId = new string[51];
            //BoxNote = new string[26];
            //Date = new DateTime();
            //DriversSchedule = new List<DriverScheduleViewModel>();
        }
        public int Id { get; set; }
        
        public DateTime Date { get; set; }
        public string DayString { get; set; }

        [StringLength(128)]
        public string DriverId { get; set; }

        [StringLength(128)]
        public string BackupDriverId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public List<SelectListItem> DriverList { get; set; }
        public List<SelectListItem> BackupDriverList { get; set; }
        public List<DriverScheduleViewModel> DriversSchedule { get; set; }
        public DateTime[,] BoxDay { get; set; }
        public object[,,] BoxDDL { get; set; }
        public string[,,] BoxDDLDriverId { get; set; }
        public string[] BoxIndexDriverId { get; set; }

        [DataType(DataType.MultilineText)]
        public string[] BoxNote { get; set; }
       
    }
}