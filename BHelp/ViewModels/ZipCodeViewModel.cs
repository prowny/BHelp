using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BHelp.ViewModels
{
    public class ZipCodeViewModel
    {
        public int Id { get; set; }

        [DisplayName("Zip Code"), MaxLength(5)]
        public string Zip { get; set; }
    }
}