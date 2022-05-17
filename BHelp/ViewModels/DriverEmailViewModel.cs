using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class DriverEmailViewModel
    {
        public int Id { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }
       
        public string Title { get; set; }
        public string EmailText { get; set; }

        [AllowHtml]
        public string HtmlContent { get; set; }

        public string Subject { get; set; }
        public List<DriverEmailRecipient>  Recipients { get; set; }
       
        public string MonthYear { get; set; }

    }
}