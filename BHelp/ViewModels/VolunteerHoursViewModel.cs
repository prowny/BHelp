using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.ViewModels
{
    public class VolunteerHoursViewModel
    {
        [StringLength(128)]
        public string VolunteerId { get; set; }

        [Column(TypeName = "Date")]
        public DateTime WeekEnding { get; set; }
        public Single Hours { get; set; }
    }
}