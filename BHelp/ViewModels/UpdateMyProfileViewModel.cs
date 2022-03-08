using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace BHelp.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateMyProfileViewModel
    {
        [JsonProperty] public string Id { get; set; }

        [JsonProperty, DisplayName("First Name")]
        public string FirstName { get; set; }

        [JsonProperty, DisplayName("Last Name")]
        public string LastName { get; set; }

        [JsonProperty] public String Title { get; set; }

        [JsonProperty] public string Email { get; set; }

        [JsonProperty] public string UserName { get; set; }

        [JsonProperty, DisplayName("Phone Number (Preferred)")]
        public string PhoneNumber { get; set; }

        [JsonProperty, DisplayName("Phone Number (Alternate)")]
        public string PhoneNumber2 { get; set; }

        [JsonProperty, DisplayName("Address")]
        public string Address { get; set; }

        [JsonProperty, DisplayName("City")]
        public string City { get; set; }

        [JsonProperty, DisplayName("State")]
        public string State { get; set; }

        [JsonProperty, DisplayName("Zip Code")]
        public string Zip { get; set; }

        [JsonProperty]
        public List<SelectListItem> States { get; set; }
    }
}