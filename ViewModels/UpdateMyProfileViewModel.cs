using System;

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

        [JsonProperty, DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }
    }
}