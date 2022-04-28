using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataType = System.ComponentModel.DataAnnotations.DataType;

namespace BHelp.Models
{
    public class ODSchedule
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } 

        [StringLength(128)]
        public string ODId { get; set; }
        
        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        [NotMapped] public string ODName { get; set; }
    }
}