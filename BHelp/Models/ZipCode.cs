using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BHelp.Models
{
    public class ZipCode    
    {
        public int Id { get; set; }

        [DisplayName("Zip Code"), MaxLength (5)]
        public string Zip { get; set; }
    }
}