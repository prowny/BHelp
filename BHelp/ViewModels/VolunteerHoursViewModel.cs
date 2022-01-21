using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class VolunteerHoursViewModel
    {
        [StringLength(128)]
        public string UserId { get; set; }

        [StringLength(128)]
        public string OriginatorUserId { get; set; }
        public string VolunteerName { get; set; }
        public DateTime WeekEndingDate { get; set; }
        public string WeekEndingDateString { get; set; }

        [StringLength(1)]
        public string Category { get; set; }  // A, F, or M
        public string CategoryName { get; set; }
        public string Subcategory { get; set; }
        public string SubcategoryName { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public string SubmitError { get; set; }
        public List<SelectListItem> UserList { get; set; }
        public List<SelectListItem> CategoryList { get; set; }
        public List<SelectListItem> SubcategoryList { get; set; }
        public List<VolunteerHoursViewModel> HoursList { get; set; }
        public Boolean IsIndividual { get; set; }  // Individual sees only defaults (User, Cat, Subcat)
        public string UserFullName { get; set; }
        public string OriginatorFullName { get; set; }
        public string HoursString { get; set; }
        public string MinutesString { get; set; }
    }
}