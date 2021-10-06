using DocumentFormat.OpenXml.Office.CoverPageProps;

namespace BHelp.ViewModels
{
    public class OpenDeliveryViewModel
    {
        public string ReportTitle { get; set; }
        public  string[,] OpenDeliveries { get; set; }
        public string[,,] TempOpenDeliveries { get; set; }
        public int  OpenDeliveryCount { get; set; }
        public string DriverName { get; set; }
    }
}