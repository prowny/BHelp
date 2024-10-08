using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BHelp.Models
{
    public class GroupName
    {
        public int Id { get; set; }

        [DisplayName("Group Name"), StringLength(128)]
        public string Name { get; set; }

        public bool CBagDelivery { get; set; } 
    }
}