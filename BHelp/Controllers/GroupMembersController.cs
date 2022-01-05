using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;

namespace BHelp.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        // GET: GroupMembers
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

            //if (gpId != null)
            //{
            //    Session["GroupId"] = gpId.ToString();
            //    gpId = Convert.ToInt32(Session["GroupId"]);
            //    groupMembersView.SelectedGroupId = (int)gpId;
            //    var clientGroupMembers = db.GroupMembers
            //        .Where(g => g.NameId == gpId).ToList();
            //    foreach(var member in clientGroupMembers)
            //    {
            //        var client = db.Clients.Find(member.ClientId);
            //        if (client != null)
            //            groupMembersView.GroupMemberSelectList.Add(new SelectListItem()
            //                { Text = client.LastName + @", " + client.FirstName, Value = client.Id.ToString(), Selected = false});
            //    }
            //}

            //var allClientList = db.Clients.OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ToList();
            //groupMembersView.AllClients.Add(new SelectListItem { Text = @"-Select Client to Add To Group-", Value = "0" });
            //foreach (var client in allClientList)
            //{
            //    var text = client.LastName + ", " + client.FirstName + " ";
            //    text += client.StreetNumber + " " + client.StreetName;
            //    groupMembersView.AllClients.Add(new SelectListItem()
            //        {Text = text, Value = client.Id.ToString(),Selected = false});
            //}
            
            return View(groupMembersView);
        }

        [HttpPost]
        public ActionResult Index ([Bind(Include= "SelectedMemberId")] GroupMemberViewModel model)
        {
            //Create([Bind(Include = "Id,ClientId,Active,FirstName,LastName,DateOfBirth")] FamilyMember familyMember)
            var _gpId = Convert.ToInt32(Session["GroupId"]);
            var _mbrId = Convert.ToInt32(model.SelectedMemberId);
            var memberList = db.GroupMembers.Where(g => g.NameId == _gpId).ToList();
            foreach (var mbr in memberList)
            {
                if (mbr.ClientId == _mbrId)
                {
                    GroupMember member = db.GroupMembers.First(m => m.NameId == _gpId
                                                                    && m.ClientId == _mbrId);
                    if (member != null)
                    {
                        db.GroupMembers.Remove(member);
                        db.SaveChanges();
                    }
                }
            }
            
            return RedirectToAction("MaintainGroupMembers", new { groupId = _gpId });
        }

        // GET: GroupMembers/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GroupMembers/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET: GroupMembers/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GroupMembers/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: GroupMembers/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GroupMembers/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult MaintainGroupMembers(int? groupId)
        {
            var memberViewModel = new GroupMemberViewModel()
            {
                SelectedGroupId=groupId,
                AllClients = new List<SelectListItem>(),
                GroupMemberSelectList = new List<SelectListItem>()
            };
            if (groupId != null)
            {
                Session["GroupId"] = groupId.ToString();
                var group = db.GroupNames.Find(groupId);
                if (group != null)
                {
                    memberViewModel.SelectedGroupName = group.Name;
                    memberViewModel.GroupMemberSelectList = GetGroupMembers(group.Id);
                    memberViewModel.AllClients = GetAllClients();
                }
                
                //var clientGroupMembers = db.GroupMembers
                //    .Where(g => g.NameId == groupId).ToList();
                //foreach (var member in clientGroupMembers)
                //{
                //    var client = db.Clients.Find(member.ClientId);
                //    if (client != null)
                //        memberViewModel.GroupMemberSelectList.Add(new SelectListItem()
                //        {
                //            Text = client.LastName + @", " + client.FirstName, Value = client.Id.ToString(),
                //            Selected = false
                //        });
                //}
            }

            //var allClientList = db.Clients.OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ToList();
            //memberViewModel.AllClients.Add(new SelectListItem { Text = @"-Select Client to Add To Group-", Value = "0" });
            //foreach (var client in allClientList)
            //{
            //    var text = client.LastName + ", " + client.FirstName + " ";
            //    text += client.StreetNumber + " " + client.StreetName;
            //    memberViewModel.AllClients.Add(new SelectListItem()
            //        { Text = text, Value = client.Id.ToString(), Selected = false });
            //}
            
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
                    groupMemberSelectList.Add(new SelectListItem()
                        { Text = client.LastName + @", " + client.FirstName, Value = client.Id.ToString(), Selected = false });
            }

            return groupMemberSelectList;
        }

        public List<SelectListItem> GetAllClients()
        {
            var allClientsSelectList = new List<SelectListItem>();
            var clientList = db.Clients.OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ToList();
            allClientsSelectList.Add(new SelectListItem { Text = @"-Select Client to Add To Group-", Value = "0" });
            foreach (var client in clientList)
            {
                var text = client.LastName + ", " + client.FirstName + " ";
                text += client.StreetNumber + " " + client.StreetName;
                allClientsSelectList.Add(new SelectListItem()
                    { Text = text, Value = client.Id.ToString(), Selected = false });
            }

            return allClientsSelectList;
        }
        
        public ActionResult AddGroupMember(int clientId)
        {
            var gpId = Convert.ToInt32(Session["GroupId"]);
            var newMember = new GroupMember()
            {
                NameId = gpId,
                ClientId = clientId
            };
            db.GroupMembers.Add(newMember);
            db.SaveChanges();

            var model = new GroupMemberViewModel()
            {
                GroupMemberSelectList = GetGroupMembers(gpId),
                AllClients = GetAllClients()
            };
            return Json(model, JsonRequestBehavior.AllowGet);
            // return RedirectToAction("MaintainGroupMembers", new{ groupId = newMember.NameId });
        }

        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}
