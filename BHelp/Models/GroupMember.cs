using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace BHelp.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int NameId { get; set; }
        public int ClientId { get; set; }

        [NotMapped]
        public int GroupName { get; set; }

        [NotMapped]
        public List<SelectListItem> GroupNameSelectList { get; set; }

        [NotMapped]
        public List<Client> ClientGroupMembers { get; set; }

        [NotMapped]
        public List<Client> AllClients { get; set; }
    }
}
