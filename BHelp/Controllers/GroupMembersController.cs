using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    [Authorize]
    public class GroupMembersController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        // GET: GroupMembers
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Index()
        {
            var groupMembersView = new GroupMemberViewModel()
            {
                GroupNameSelectList = new List<SelectListItem>(),
                GroupMemberSelectList = new List<SelectListItem>(),
                AllClients = new List<SelectListItem>(),
                SelectedGroupId = null
            };
            var groupNamesList = db.GroupNames.OrderBy(n => n.Name).ToList();
            foreach (var gName in groupNamesList)
            {
                groupMembersView.GroupNameSelectList.Add(new SelectListItem()
                    {Text=gName.Name, Value = gName.Id.ToString(), Selected = false });
            }

            return View(groupMembersView);
        }

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Remove (int? clientId)
        {
            var _gpId = Convert.ToInt32(Session["GroupId"]);
            var memberList = db.GroupMembers.Where(g => g.NameId == _gpId).ToList();
            foreach (var mbr in memberList)
            {
                if (mbr.ClientId == clientId)
                {
                    GroupMember member = db.GroupMembers.First(m => m.NameId == _gpId
                                                                    && m.ClientId == clientId);
                    if (member != null)
                    {
                        db.GroupMembers.Remove(member);
                        db.SaveChanges();
                    }
                }
            }
            return RedirectToAction("MaintainGroupMembers", new { groupId = _gpId });
        }

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult ChangeClientSortOrder(string sortOrder)
        {
            switch (sortOrder)
            {
                case "LastName":
                    Session["ClientSortOrder"] = 0;
                    break;
                case "StreetName":
                    Session["ClientSortOrder"] = 1;
                    break;
            }
            return RedirectToAction("MaintainGroupMembers",
                new {groupId = (int)TempData["SelectedGroupId"]});
        }

        // GET: GroupMembers/Create
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Create()
        {
            return View();
        }
        
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult MaintainGroupMembers(int? groupId)
        {
            if (Session["ClientSortOrder"] == null) Session["ClientSortOrder"] = 0;  // deffault LastName
            var memberViewModel = new GroupMemberViewModel()
            {
                SelectedGroupId=groupId,
                AllClients = new List<SelectListItem>(),
                GroupMemberSelectList = new List<SelectListItem>(),
                ClientSortOrder = (int)Session["ClientSortOrder"]
            };

            memberViewModel.SelectedSortOrder = "LastName";
            if (memberViewModel.ClientSortOrder == 1)
            { memberViewModel.SelectedSortOrder = "StreetName"; }

            if (groupId != null)
            {
                TempData["SelectedGroupId"] = groupId;
                Session["GroupId"] = groupId.ToString();
                var group = db.GroupNames.Find(groupId);
                if (group != null)
                {
                    memberViewModel.NameId = (int)groupId;  
                    memberViewModel.SelectedGroupName = group.Name;
                    memberViewModel.GroupMemberSelectList = GetGroupMembers(group.Id);
                    memberViewModel.AllClients = GetAllClients((int)Session["ClientSortOrder"]);
                }
            }
            
            return View(memberViewModel);
        }
        public List<SelectListItem> GetGroupMembers(int groupId)
        {
            var groupMemberSelectList = new List<SelectListItem>();
            var clientGroupMembers = db.GroupMembers
                .Where(g => g.NameId == groupId).ToList();
            foreach (var member in clientGroupMembers)
            {
                var client = db.Clients.Find(member.ClientId);
                if (client != null)
                {
                    var _text = client.LastFirstName + " " + client.StreetNumber + " ";
                    _text += client.StreetName;
                    groupMemberSelectList.Add(new SelectListItem()
                        { Text = _text, Value = client.Id.ToString(), Selected = false });
                }
            }

            return groupMemberSelectList;
        }

        public List<SelectListItem> GetAllClients(int sortBy)
        {
            List<Client> clientList;
            var allClientsSelectList = new List<SelectListItem>()
                { new SelectListItem { Text = @"-Select Client to Add To Group-", Value = "0" } };
            if (sortBy == 0)
            {
                clientList = db.Clients.OrderBy(n => n.LastName)
                    .ThenBy(n => n.FirstName).Where(a => a.Active).ToList();
            }
            else
            {
                clientList = db.Clients.OrderBy(n => n.StreetName)
                    .ThenBy(n => n.LastName).ThenBy(n => n.FirstName).Where(a => a.Active).ToList();
            }

            foreach (var client in clientList)
            {
                string text;
                if (sortBy == 0)
                { 
                     text = client.LastName + ", " + client.FirstName + " "
                     + client.StreetNumber + " " + client.StreetName;
                }
                else
                {
                     text = client.StreetNumber + " " + client.StreetName + " "
                     + client.LastName + ", " + client.FirstName;
                }
                allClientsSelectList.Add(new SelectListItem()
                    { Text = text, Value = client.Id.ToString(), Selected = false });
            }

            return allClientsSelectList;
        }
        
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult AddGroupMember(string clientId)
        {
            var gpId = Convert.ToInt32(TempData["SelectedGroupId"]);
            if (clientId != null) // Add to group unless duplicate
            {
                var iClientId = Convert.ToInt32(clientId);
                var dup = false;
                var grpMembers = db.GroupMembers
                    .Where(m => m.NameId == gpId).ToList();
                foreach (var mbr in grpMembers)
                {
                    if (mbr.ClientId == iClientId ) dup = true;
                }

                if (dup == false)
                {
                    var newMember = new GroupMember()
                    {
                        ClientId = iClientId,
                        NameId = gpId
                    };
                    db.GroupMembers.Add(newMember);
                    db.SaveChanges();
                }
            }
            
            return RedirectToAction("MaintainGroupMembers", new {groupId = gpId});
        }

        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult AddGroupMember([Bind(Include= "AllClients")] GroupMemberViewModel model)
        {
            var clientId = model.ClientId;
            var gpId = Convert.ToInt32(Session["GroupId"]);
            var newMember = new GroupMember()
            {
                NameId = gpId,
                ClientId = clientId
            };
            db.GroupMembers.Add(newMember);
            db.SaveChanges();

            model = new GroupMemberViewModel()
            {
                GroupMemberSelectList = GetGroupMembers(gpId),
                AllClients = GetAllClients((int)Session["ClientSortOrder"])
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateDeliveries(int nameId)
        {
            return null;
        }
    }
}
