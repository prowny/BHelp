using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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
    }
}