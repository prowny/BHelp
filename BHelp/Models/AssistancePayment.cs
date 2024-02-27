using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class AssistancePayment
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public byte CategoryId { get; set; }

        [StringLength(256)]
        public string Action { get; set; }
        //public int AmountInCents { get; set; }
        public DateTime Date { get; set; }

        [StringLength(256)]
        public string Note { get; set; }
        public decimal AmountDecimal { get; set; }

        [NotMapped] public string LastName { get; set; }
        [NotMapped] public string FirstName { get; set; }
        [NotMapped] public string City { get; set; }
        [NotMapped] public string Zip { get; set; }
        [NotMapped] public string NoteToolTip { get; set; }
        [NotMapped] public string DateString { get; set; }
        [NotMapped] public string FullName { get; set; }
        [NotMapped] public string ActionCategory { get; set; }
        [NotMapped] public string ActionToolTip { get; set;}
        [NotMapped] public string AddressString { get; set; }
        [NotMapped] public string AddressToolTip { get; set; }
        [NotMapped] public string StringDollarAmount { get; set; }
    }
}
