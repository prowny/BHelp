using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace BHelp.ViewModels
{
    public class DriverScheduleViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        
        [StringLength(128)]
        public string DriverId { get; set; }

        [StringLength(128)]
        public string BackupDriverId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
        public IEnumerable<DriverScheduleViewModel> DriversSchedule { get; set; }
        public DateTime [,] BoxDay { get; set; }
        public object[,,] BoxDDL { get; set; }
        public string[] DDL1Idx { get; set; }
        public string[] DDL2Idx { get; set; }
        public string[,,] BoxDDLDriverId { get; set; }
        public string [] BoxIndexDriverId { get; set; }

        [DataType(DataType.MultilineText)]
        public string [] BoxNote { get; set; }
       
    }
}