using System.ComponentModel.DataAnnotations;

namespace BHelp.ViewModels
{
    public class VoicemailPasswordViewModel
    {
        [Required (ErrorMessage = "Password Required")]
        public string VoicemailPassword { get; set; }
        public string[] InfoText { get; set; }
    }
}