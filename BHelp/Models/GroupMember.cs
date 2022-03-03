using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int NameId { get; set; }
        public int ClientId { get; set; }

        [NotMapped]
        public string GroupName { get; set; }
    }
}
