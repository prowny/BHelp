using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure.Design;
using System.Linq;
using System.Web;

namespace BHelp.ViewModels
{
    public class ReportsViewModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime BeginMonth { get; set; }
        public DateTime EndMonth { get; set; }

        [Range(1, 4)] public int Quarter { get; set; }
        [Range( 2020, 2040)] public int Year { get; set; }

    }
}