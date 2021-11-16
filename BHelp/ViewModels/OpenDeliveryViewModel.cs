using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class OpenDeliveryViewModel
    {
        public string ReportTitle { get; set; }
        public  string[,] OpenDeliveries { get; set; }
        public int OpenDeliveryCount { get; set; }
        public List<string> DeliveryDatesList { get; set; }
        public List<SelectListItem> DeliveryDatesSelectList { get; set; }
        public DateTime SelectedDeliveryDate { get; set; }
        public List<string> DriverList { get; set; }
        public List<SelectListItem> DriversSelectList { get; set; }
        public string SelectedDriverId { get; set; } 
        public List<Delivery> SelectedDeliveriesList { get; set; }
    }
}