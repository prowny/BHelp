using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class ReportsViewModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime BeginMonth { get; set; }
        public DateTime EndMonth { get; set; }

        [Range(1, 4)] public int Quarter { get; set; }
        [Range( 2020, 2050)] public int Year { get; set; }
        public string DateRangeTitle { get; set; }  // e.g. "April 2021 through June 2021"
        public string[] MonthYear { get; set; }
        public string ZipCode { get; set; }
        public List<string> ZipCodes { get; set; }
        public int [,,] MonthlyCounts { get; set; } // Note 3-dimensional
    }
}