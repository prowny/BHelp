using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;

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

        // GET: Group/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Group/Create
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Group/Create
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Create([Bind(Include = "Id,Name")] GroupNameViewModel groupName)
        {
            if (ModelState.IsValid)
            {
                GroupName gpNm = new GroupName { Name = groupName.Name };
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
            GroupName groupName = db.GroupNames.Find(id);
            if (groupName == null) return HttpNotFound();
            
            var groupNameView = new GroupNameViewModel { Name = groupName.Name };
            return View(groupNameView);
        }

        // POST: Group/Edit/5
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Edit([Bind(Include = "Id, Name")] GroupNameViewModel groupName)
        {
            if (!ModelState.IsValid) return View(groupName);
            if (groupName.Name == null) { groupName.Name = ""; }
            db.Entry(groupName).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Group/Delete/5
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Group/Delete/5
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
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

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}
