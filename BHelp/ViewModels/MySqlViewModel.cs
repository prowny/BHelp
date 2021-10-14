using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DocumentFormat.OpenXml.Wordprocessing;

namespace BHelp.ViewModels
{
    public class MySqlViewModel
    {
        public int PKID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}