﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataType = System.ComponentModel.DataAnnotations.DataType;

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

        [StringLength(128)]
        public string BackupDriver2Id { get; set; }

        public int? GroupId { get; set; }

        [StringLength(128)]
        public string GroupDriverId { get; set; }

        [DataType(DataType.MultilineText), StringLength(4096)]
        public string Note { get; set; }

        [NotMapped] public string DriverName { get; set; }

        [NotMapped] public string BackupDriverName { get; set; }

        [NotMapped] public string BackupDriver2Name { get; set; }

        [NotMapped] public string GroupDriverName { get; set; }
    }
}