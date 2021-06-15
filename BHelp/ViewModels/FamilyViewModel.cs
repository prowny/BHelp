using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class FamilyViewModel

    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public bool Active { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }
        public int Age { get; set; }
        public List<FamilyMember> FamilyMembers { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }
    }
}