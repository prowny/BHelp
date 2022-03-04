using System.Collections.Generic;
using System.Web.Mvc;

namespace BHelp.ViewModels
{
    public class GroupMemberViewModel
    {
        public int Id { get; set; }
        public int NameId { get; set; }
        public int ClientId { get; set; }
        public string GroupName { get; set; }
        public List<SelectListItem> GroupNameSelectList { get; set; }
        public int? SelectedGroupId { get; set; }
        public string SelectedGroupName { get; set; }
        public List<SelectListItem> GroupMemberSelectList { get; set; }
        public string SelectedMemberId { get; set; }
        public List<SelectListItem> AllClients { get; set; }
        public string SelectedClientId { get; set; }
        public string ClientNameAddress { get; set; }
    }
}