using System;
using System.ComponentModel.DataAnnotations;

namespace BHelp.Models
{
    public class VolunteerHours
    {
        public int Id { get; set; }
        
        [StringLength(128)]
        public string UserId { get; set; }

        [StringLength(128)]
        public string OriginatorUserId { get; set; }

        [StringLength(1)] 
        public string Category { get; set; }  // A, F, or M

        public string Subcategory { get; set; }

        public DateTime WeekEndingDate { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }
    }
}
