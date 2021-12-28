using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace BHelp.Models
{
    public class GroupName
    {
        public int Id { get; set; }

        [DisplayName ("Group Name")]
        public string Name { get; set; }

        [NotMapped]
        public IEnumerable<GroupName> GroupNameList { get; set; }
    }
}