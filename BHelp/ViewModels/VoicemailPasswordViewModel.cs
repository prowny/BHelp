using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class VoicemailPasswordViewModel
    {
        public string VoicemailPassword { get; set; }
        public string OldVoicemailPassword { get; set; }
        public string[] InfoText { get; set; }

        [StringLength(128)] 
        public string LoginKeyReceiverId { get; set; } // user Id
        public string LoginKeyReceiverName { get; set; } // Full name
        public IEnumerable<SelectListItem> UserList { get; set; }
    }
}