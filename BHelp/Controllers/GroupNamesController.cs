using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    [Authorize]
    public class GroupNamesController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        // GET: Group
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Index()
        {
            var groupNamesList = db.GroupNames.OrderBy(n => n.Name).ToList();
            var groupNamesView = new GroupNameViewModel
            {
                GroupNameList = groupNamesList
            };
            return View(groupNamesView);
        }

        // GET: Group/Create
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Group/Create
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Create([Bind(Include = "Id,Name,CBagDelivery")] GroupNameViewModel groupName)
        {
            if (ModelState.IsValid)
            {
                var gpNm = new GroupName
                {
                    Name = groupName.Name.Trim(),
                    CBagDelivery = groupName.CBagDelivery
                };
                db.GroupNames.Add(gpNm);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(groupName);
        }

        // GET: Group/Edit/5
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Edit(int id)
        {
            var groupName = db.GroupNames.Find(id);
            if (groupName == null) return HttpNotFound();
            
            var groupNameView = new GroupNameViewModel
            {
                Name = groupName.Name,
                CBagDelivery =groupName.CBagDelivery
            };
            return View(groupNameView);
        }

        // POST: Group/Edit/5I
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Edit([Bind(Include = "Id, Name,CBagDelivery ")] GroupNameViewModel groupName)
        {
            if (!ModelState.IsValid) return View(groupName);
            var groupNameRec = db.GroupNames.Find(groupName.Id);
            if (groupNameRec != null)
            {
                var dupNameRec = db.GroupNames.FirstOrDefault(n => n.Name ==
                                                 groupName.Name && n.Id != groupNameRec.Id);
                if (dupNameRec != null)
                {
                    groupName.ErrorMessage = "That group name already exists!";
                    return View(groupName);
                }
                groupNameRec.Name = groupName.Name.Trim();
                groupNameRec.CBagDelivery = groupName.CBagDelivery;
                db.SaveChanges();
            }
            
            return RedirectToAction("Index");
        }

        // GET: Group/Delete/5
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Delete(int id)
        {
            var groupName = db.GroupNames.Find(id);
            if (groupName == null) return View();
            var view = new GroupNameViewModel()
            {  
                Id =id,
                Name = groupName.Name,
                CBagDelivery = groupName.CBagDelivery 
            };
            return View(view);
        }

        // POST: Group/Delete/5
        [HttpPost, ActionName("Delete"),Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult DeleteConfirmed(int id)
        {
            var gpMembers = db.GroupMembers.Where(m => m.Id == id).ToList();
            foreach (var mbr in gpMembers)
            {
                db.GroupMembers.Remove(mbr);
            }
            var gpNameRec = db.GroupNames.Find(id);
            if (gpNameRec != null) db.GroupNames.Remove(gpNameRec);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
