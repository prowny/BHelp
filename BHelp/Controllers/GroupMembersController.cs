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
        public ActionResult Index(int? gpId)
        {
            var groupMembersView = new GroupMemberViewModel()
            {
                GroupNameSelectList = new List<SelectListItem>(),
                ClientGroupMembers = new List<SelectListItem>(),
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
                groupMembersView.SelectedGroupId = (int)gpId;
                var clientGroupMembers = db.GroupMembers.Where(g => g.NameId == gpId).ToList();
                foreach(var member in clientGroupMembers)
                {
                    var client = db.Clients.Find(member.ClientId);
                    if (client != null)
                        groupMembersView.ClientGroupMembers.Add(new SelectListItem()
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
        public ActionResult GetGroupMembers(int groupId)
        {
            return RedirectToAction("Index", new { gpId = groupId });
            //var groupMembersView = new GroupMemberViewModel()
            //{
            //    ClientGroupMembers = new List<SelectListItem>()
            //};
            //var groupMembers = db.GroupMembers
            //    .Where(gp => gp.NameId == groupId).ToList();
            //foreach (var member in groupMembers)
            //{
            //    var client = db.Clients.Find(member.ClientId);
            //    if (client != null)
            //    {
            //        groupMembersView.ClientGroupMembers.Add(new SelectListItem()
            //            { Text = client.FullName, Value = client.Id.ToString(), Selected = false });
            //    }
            //}

            //try
            //{
            //    var json = JsonConvert.SerializeObject(groupMembersView.ClientGroupMembers
            //        , Formatting.Indented);
            //    return Content(json, "application/json");
            //}
            //catch (Exception ex)
            //{
            //    var x = ex;
            //    return null;
            //}
        }

        public ActionResult AddGroupMember(int groupId, int clientId)
        {
            var newMember = new GroupMember()
            {
                NameId = groupId,
                ClientId = clientId
            };
            db.GroupMembers.Add(newMember);
            db.SaveChanges();
          
            return RedirectToAction("Index", new{gpId = groupId});
        }
        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}
