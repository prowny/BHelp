using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class BagWeight
    {
        public int Id { get; set; }

        [DisplayName("A Bag Weight in Pounds")]
        public decimal APounds { get; set; }

        [DisplayName("B Bag Weight in Pounds")]
        public decimal BPounds { get; set; }

        [DisplayName("C Bag Weight in Pounds")]
        public decimal CPounds { get; set; }

        [DisplayName("Effective Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime EffectiveDate { get; set; }

        [NotMapped]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public string EffectiveDateString { get; set; }
    }
}