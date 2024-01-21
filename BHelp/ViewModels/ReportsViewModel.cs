using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Range(2020, 2050)] public int Year { get; set; }
        [Range(1, 12)] public int Month { get; set; }
        public string DateRangeTitle { get; set; }  // e.g. "April 2021 through June 2021"
        public string MMyyyy { get; set; }  // used in Bethesda Helper report
        public string HelperReportType { get; set; }  // Monthly, Quarterly, Yearly
        public string SelectedHelperReportType { get; set; }  // for Radio Buttons
        public string[] MonthYear { get; set; }
        public string ZipCode { get; set; }
        public List<string> ZipCodes { get; set; }
        public int[,,] Counts { get; set; } // Note 3-dimensional: Month, Zip, Amount  
        public int[] Months { get; set; }   // month numbers for the quarter e.g. 1,2,3 or 7,8,9
        public int[,] ZipCounts { get; set; } // for Bethesda Helper Data Report
        public string[] HelperTitles { get; set; }
        public string[] QORKTitles { get; set; }
        public string[] CountyTitles { get; set; }
        public int ZipCount { get; set; } // for New QORK Report 02/22
        public string[,] HoursTotal { get; set; } // for New QORK Report 02/22
        public bool ShowHoursTotals { get; set; } // for New QORK Report 02/22

        // for Weekly Info Report 04/23  
        public bool[] BoxHoliday { get; set; }
        public string[] BoxHolidayDescription { get; set; }

        public string[] BoxDateDay { get; set; } 
        public string[] BoxDriverName { get; set; }
        public string[] BoxDriverId { get; set; }
        public string[] BoxDriverPhone { get; set; }
        public string[] BoxDriverEmail { get; set; }
        public string[] BoxBackupDriverId { get; set; }
        public string[] BoxBackupDriverName { get; set; }
        public string[] BoxBackupDriverPhone { get; set; }
        public string[] BoxBackupDriverEmail { get; set; }
        public string[] BoxGroupName { get; set; }
        public string[] BoxGroupDriverId { get; set; }
        public string[] BoxGroupDriverName { get; set; }
        public string[] BoxGroupDriverPhone { get; set; }
        public string[] BoxGroupDriverEmail { get; set; }

        public string[] BoxODName { get; set; }
        public string[] BoxODId { get; set; }
        public string[] BoxODPhone { get; set; }
        public string[] BoxODEmail { get; set; }
        public string[] BoxODOddEvenMsg { get; set; }
    }
}