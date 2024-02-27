using System.ComponentModel.DataAnnotations;

namespace BHelp.Models
{
    public class AddressCheck
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(8095)]
        public string Note { get; set; }
    }
}