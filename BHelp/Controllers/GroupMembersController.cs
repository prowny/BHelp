using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;
using System;
using Newtonsoft.Json;

namespace BHelp.Controllers
{
    public class GroupMembersController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        // GET: GroupMembers
        public ActionResult Index()
        {;
            var groupMembersView = new GroupMember
            {
                GroupNameSelectList = new List<SelectListItem>()
            };
            var groupNamesList = db.GroupNames.OrderBy(n => n.Name).ToList(); 
            foreach (var gName in groupNamesList)
            {
                groupMembersView.GroupNameSelectList.Add(new SelectListItem()
                    {Text=gName.Name, Value = gName.Id.ToString(), Selected = false});
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
            var groupMember = new GroupMember();
            var groupMembers = db.GroupMembers
                .Where(gp => gp.Id == groupId).ToList();
            for (var index = 0; index < groupMembers.Count; index++)
            {
                var clientId = groupMembers[index];
                var client = db.Clients.Find(clientId);
                groupMember.ClientGroupMembers.Add(client);
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

    }
}
