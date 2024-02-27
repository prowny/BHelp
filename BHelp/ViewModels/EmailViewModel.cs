using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class EmailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string EmailText { get; set; }

        [AllowHtml]
        public string HtmlContent { get; set; }
        public List<EmailRecipient> Recipients { get; set; }
    }
}