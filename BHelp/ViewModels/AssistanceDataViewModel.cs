using System;
using System.Collections.Generic;
namespace BHelp.ViewModels
{
    public class AssistanceDataViewModel  // For Single Client Date-Range History
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EarliestPaymentDate { get; set; }
        public string PaymentHistoryList { get; set; }
        public int NumberOfPayments { get; set; }
        public List<int> TotalsByCategoryInCents { get; set; }
        public List<string> TotalsByCategoryString { get; set; }
        public string GrandTotalString { get; set; }
        public List<string> CategoryList { get; set; }
    }
}