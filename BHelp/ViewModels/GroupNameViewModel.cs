using System.Collections.Generic;
using System.ComponentModel;
using BHelp.Models;

namespace BHelp.ViewModels
{
    public class GroupNameViewModel
    {
        public int Id { get; set; }

        [DisplayName("Group Name")]
        public string Name { get; set; }
        public IEnumerable<GroupName> GroupNameList { get; set; }
        public List<int> GroupMembersIdList { get; set; }
        public string ErrorMessage { get; set; }
    }
}