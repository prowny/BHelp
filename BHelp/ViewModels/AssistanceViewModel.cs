using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CsvHelper;
using DocumentFormat.OpenXml.Office.CoverPageProps;

namespace BHelp.ViewModels
{
    public class AssistanceViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public float Amount { get; set; }
        public string Note { get; set; }
    }
}