using System;
using System.ComponentModel.DataAnnotations;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace BHelp.Models
{
    public class ODSchedule
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } 

        [StringLength(128)]
        public string ODId { get; set; }
        
        [DataType(DataType.MultilineText), StringLength(4096)]
        public string Note { get; set; }

        public bool ODConfirmed { get; set; }
    }
}