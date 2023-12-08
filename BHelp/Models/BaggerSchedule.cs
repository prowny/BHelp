using System;
using System.ComponentModel.DataAnnotations;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace BHelp.Models
{
    public class BaggerSchedule
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [StringLength(128)]
        public string BaggerId { get; set; }

        [StringLength(128)]
        public string PartnerId { get; set; }  // optional for husband-wife team
                                               
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }
    }
}