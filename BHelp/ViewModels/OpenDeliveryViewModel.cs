using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Office.CoverPageProps;

namespace BHelp.ViewModels
{
    public class OpenDeliveryViewModel
    {
        public string ReportTitle { get; set; }
        public  string[,] OpenDeliveries { get; set; }
        public int  OpenDeliveryCount { get; set; }
    }
}