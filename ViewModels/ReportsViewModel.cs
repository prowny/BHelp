using System;
using System.Collections.Generic;

namespace BHelp.ViewModels
{
    public class ReportsViewModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EndDateString { get; set; }   // For use by Quork report
        public DateTime BeginMonth { get; set; }
        public DateTime EndMonth { get; set; }

        public string ReportTitle { get; set; }  //  e.g. "Apr-Jun 2021 County Report"
        [Range(1, 4)] public int Quarter { get; set; }
        [Range( 2020, 2050)] public int Year { get; set; }
        public string DateRangeTitle { get; set; }  // e.g. "April 2021 through June 2021"
        public string[] MonthYear { get; set; }
        public string ZipCode { get; set; }
        public List<string> ZipCodes { get; set; }
        public int[,,] Counts { get; set; } // Note 3-dimensional: Month, Zip, Amount  
        public int[] Months { get; set; }   // month numbers for the quarter e.g. 1,2,3 or 7,8,9
    }
}