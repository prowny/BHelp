using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class OpenDeliveryViewModel
    {
        public string ReportTitle { get; set; }
        public  string[,] OpenDeliveries { get; set; }
        public string[,,] TempOpenDeliveries { get; set; }
        public int OpenDeliveryCount { get; set; }
        public  string[] UniqueDeliveryDateStrings { get; set; }
        public List<string> DeliveryDatesList { get; set; }
        public List<string> DriverList { get; set; }
    }
}