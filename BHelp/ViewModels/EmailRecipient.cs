using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BHelp.ViewModels
{
    public class EmailRecipient
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool Checked { get; set; }
    }
}