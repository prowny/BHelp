using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using Newtonsoft.Json;

namespace BHelp.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        // GET: GroupMembers
        public ActionResult Index(int? gpId)
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
                bool _selected = gName.Id == gpId;
                groupMembersView.GroupNameSelectList.Add(new SelectListItem()
                    {Text=gName.Name, Value = gName.Id.ToString(), Selected = _selected });
            }

            if (gpId != null)
            {
                Session["GroupId"] = gpId.ToString();
                gpId = Convert.ToInt32(Session["GroupId"]);
                groupMembersView.SelectedGroupId = (int)gpId;
                var clientGroupMembers = db.GroupMembers
                    .Where(g => g.NameId == gpId).ToList();
                foreach(var member in clientGroupMembers)
                {
                    var client = db.Clients.Find(member.ClientId);
                    if (client != null)
                        groupMembersView.GroupMemberSelectList.Add(new SelectListItem()
                            { Text = client.LastName + @", " + client.FirstName, Value = client.Id.ToString(), Selected = false});
                }
            }

            var allClientList = db.Clients.OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ToList();
            groupMembersView.AllClients.Add(new SelectListItem { Text = @"-Select Client to Add To Group-", Value = "0" });
            foreach (var client in allClientList)
            {
                var text = client.LastName + ", " + client.FirstName + " ";
                text += client.StreetNumber + " " + client.StreetName;
                groupMembersView.AllClients.Add(new SelectListItem()
                    {Text = text, Value = client.Id.ToString(),Selected = false});
            }
            
            return View(groupMembersView);
        }

        [HttpPost]
        public ActionResult Index(GroupMemberViewModel model)
        {
            var gpId = Convert.ToInt32(Session["GroupId"]);
            var mbrId = Convert.ToInt32(model.SelectedMemberId);
            List<GroupMember> memberList = db.GroupMembers.Where(g => g.NameId == gpId).ToList();
            foreach (var mbr in memberList)
            {
                if (mbr.ClientId == mbrId)
                {
                    GroupMember member = db.GroupMembers.First(m => m.NameId == gpId
                                                                    && m.ClientId == mbrId);
                    if (member != null)
                    {
                        db.GroupMembers.Remove(member);
                        db.SaveChanges();
                    }
                }
            }
            
            return RedirectToAction("Index", new {gpid=gpId} );
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

        // POST: GroupMembers/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
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

            return View(memberViewModel);
        }
        public ActionResult GetGroupMembers(int groupId)
        {
            var groupMember = new GroupMemberViewModel
            {
                GroupMemberSelectList = new List<SelectListItem>()
            };

            var clientGroupMembers = db.GroupMembers
                .Where(g => g.NameId == groupId).ToList();
            foreach (var member in clientGroupMembers)
            {
                var client = db.Clients.Find(member.ClientId);
                if (client != null)
                    groupMember.GroupMemberSelectList.Add(new SelectListItem()
                        { Text = client.LastName + @", " + client.FirstName, Value = client.Id.ToString(), Selected = false });
            }

            try
            {
                String json = JsonConvert.SerializeObject(groupMember, Formatting.Indented);
                return Content(json, "application/json");
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ActionResult AddGroupMember(int clientId)
        {
            var _gpId = Convert.ToInt32(Session["GroupId"]);
            var newMember = new GroupMember()
            {
                NameId = _gpId,
                ClientId = clientId
            };
            db.GroupMembers.Add(newMember);
            db.SaveChanges();
          
            return RedirectToAction("Index", new{ gpId = _gpId });
        }
        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}
