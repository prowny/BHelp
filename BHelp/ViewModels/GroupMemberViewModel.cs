using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class GroupMemberViewModel
    {
        public int Id { get; set; }
        public int NameId { get; set; }
        public int ClientId { get; set; }
        public int GroupName { get; set; }
        public List<SelectListItem> GroupNameSelectList { get; set; }
        public int? SelectedGroupId { get; set; }
        public List<SelectListItem> ClientGroupMembers { get; set; }
        public List<SelectListItem> AllClients { get; set; }

    }
}