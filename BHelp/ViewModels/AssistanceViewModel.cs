﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class AssistanceViewModel : IEnumerable
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public int ClientId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Action { get; set; }
        public string Payee { get; set; }  // unused 07/07/2023
        public DateTime Date { get; set; }
        public int AmountInCents { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal AmountDecimal { get; set; }
        
        public string Note { get; set; }

        [DisplayName("Date of Birth")]
        [Column(TypeName = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime DateOfBirth { get; set; }

        [DisplayName("Street #")]
        public string StreetNumber { get; set; }

        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [DisplayName("City")]
        public string City { get; set; }

        [DisplayName("Zip")]
        public string Zip { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        [DisplayName("Permanent Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        // For display use:
        public int Age { get; set; }

        [DisplayName("Family Members")]
        public List<SelectListItem> FamilySelectList { get; set; }  // For display only
        [DisplayName("Family Members")]
        public List<FamilyMember> FamilyMembers { get; set; }  // For editing
        public string HouseholdMembersString { get; set; } // For display only
        public IEnumerable<SelectListItem> ZipCodes { get; set; }
        public string StreetToolTip { get; set; }
        public string CityToolTip { get; set; }
        public string PhoneToolTip { get; set; }
        public string NotesToolTip { get; set; }  // Household/Client notes
        public IEnumerable<Client> ClientLookupList { get; set; }
        public List<SelectListItem> ClientSelectList { get; set; }
        public IEnumerable<SelectListItem> IEnumerableClientSelectList { get; set; }
        public string SelectedClientId { get; set; }
        public int CategoryId { get; set; }
        public List<AssistancePayment> PaymentList { get; set; }
        public string FullName { get; set; } // for client name in financial assistance list
        public IEnumerable<SelectListItem> AssistanceCategoriesSelectList { get; set; }
        public List<string> CategoryList { get; set; } 
        public string DateString { get; set; }
        public string ReturnURL { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ActionCategory { get; set; }
        public string BtnDelete { get; set; }
        public string PaymentHistoryList { get; set; }
        public AssistanceDataViewModel PaymentData { get; set; }
        public List<SelectListItem> PaymentHistorySelectList { get; set; }
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