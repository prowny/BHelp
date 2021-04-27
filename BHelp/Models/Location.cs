using System.ComponentModel;
using System.ComponentModel .DataAnnotations ;
namespace BHelp.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Street Number")]
        public string StreetNumber { get; set; }

        [Required]
        [DisplayName("Street Name")]
        public string StreetName { get; set; }

        [Required]
        [DisplayName("City")]
        public string City { get; set; }

        [Required]
        [DisplayName("Zip Code")]
        public string Zip { get; set; }

    }
}