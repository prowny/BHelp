﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class VolunteerHoursViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string OriginatorUserId { get; set; }
        public string VolunteerName { get; set; }
        public DateTime Date { get; set; }
        public string DateString { get; set; }
        public DateTime WeekBeginningDate { get; set; }
        public string WeekBeginningDateString { get; set; }
        public DateTime WeekEndingDate { get; set; }
        public string WeekEndingDateString { get; set; }
        public string Category { get; set; }  // A, F, or M
        public string CategoryName { get; set; }
        public string CategoryId { get; set; }
        public string Subcategory { get; set; }
        public string SubcategoryName { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int PeopleCount { get; set; }
        public string SubmitError { get; set; }
        public List<SelectListItem> UserSelectList { get; set; }
        public List<SelectListItem> CategorySelectList { get; set; }
        public List<SelectListItem> SubcategorySelectList { get; set; }
        public List<VolunteerHoursViewModel> HoursList { get; set; }
        public List<VolunteerHoursTotalsViewModel> TotalsList { get; set; }
        public Boolean IsIndividual { get; set; }  // Individual sees only defaults (User, Cat, Subcat)
        public Boolean IsNonFoodServiceAdministration { get; set; }
        public Boolean IsNonFoodServiceManagement { get; set; }
        public string UserFullName { get; set; }
        public string OriginatorFullName { get; set; }
        public string HoursString { get; set; }
        public string MinutesString { get; set; }
        public string PeopleCountString { get; set; }
        public string BtnSave { get; set; }
        public string BtnDelete { get; set; }
        public string A_MCategory { get; set; } // Administration or Management
    }
}