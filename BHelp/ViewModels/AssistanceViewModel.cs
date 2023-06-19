using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class AssistanceViewModel
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public int AmountInCents { get; set; }
        public string Note { get; set; }
        public List<SelectListItem> ClientSelectList { get; set; }
        public string ReturnUrl { get; set; }
        public string StringDollarAmount
        {
            get => $"${AmountInCents / 100}.{AmountInCents % 100:00}";
            set => throw new NotImplementedException();
        }
        // string.Format("${0}.{1:00}", AmountInCents / 100, AmountInCents % 100);
        // means format with $sign amount/100, amount modulo 100
        // where modulo is 'the remainder when divided by'  100
    }
}