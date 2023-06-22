using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class AssistanceViewModel : IEnumerable
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public int AmountInCents { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal AmountDecimal { get; set; }
        
        public string Note { get; set; }
        public IEnumerable<Client> ClientLookupList { get; set; }
        public List<SelectListItem> ClientSelectList { get; set; }
        public IEnumerable<SelectListItem> IEnumerableClientSelectList { get; set; }
        public string SelectedClientId { get; set; }
        public List<AssistancePayment> PaymentList { get; set; }

        public string StringDollarAmount
        {
            get => $"${AmountInCents / 100}.{AmountInCents % 100:00}";
            set => throw new NotImplementedException();
        }
        // string.Format("${0}.{1:00}", AmountInCents / 100, AmountInCents % 100);
        // means format with $sign amount/100, amount modulo 100
        // where modulo is 'the remainder when divided by'  100
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}