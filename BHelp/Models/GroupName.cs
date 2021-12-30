using System.ComponentModel;

namespace BHelp.Models
{
    public class GroupName
    {
        public int Id { get; set; }

        [DisplayName ("Group Name")]
        public string Name { get; set; }
    }
}