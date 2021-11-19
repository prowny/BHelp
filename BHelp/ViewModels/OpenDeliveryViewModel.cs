using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class OpenDeliveryViewModel
    {
        public string ReportTitle { get; set; }
        public  string[,] OpenDeliveries { get; set; }
        public int OpenDeliveryCount { get; set; }
        public List<string> DistinctDeliveryDatesList { get; set; }
        public List<SelectListItem> DistinctDeliveryDatesSelectList { get; set; }
        public DateTime SelectedDistinctDeliveryDate { get; set; }
        public List<string> DistinctDriverList { get; set; }
        public List<SelectListItem> DistinctDriversSelectList { get; set; }
        public string SelectedDistinctDriverId { get; set; } 

        public List<Delivery> SelectedDeliveriesList { get; set; }

        public DateTime ReplacementDeliveryDate { get; set; } 
        
        public List<SelectListItem> DriversSelectList { get; set; }

        [StringLength(128)]
        public string ReplacementDriverId { get; set; }

        public List<SelectListItem> ODSelectList { get; set; }

        [StringLength(128)]
        public string ReplacementDeliveryDateODId { get; set; }
    }
}