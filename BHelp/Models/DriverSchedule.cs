using System;
using System.ComponentModel.DataAnnotations;

namespace BHelp.Models
{
    public class DriverSchedule
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [StringLength(128)]
        public string DriverId { get; set; }

        [StringLength(128)]
        public string BackupDriverId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
    }
}