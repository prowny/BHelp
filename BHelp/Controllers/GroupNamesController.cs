using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Castle.Core.Internal;

namespace BHelp.Controllers
{
    public class GroupNamesController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();
        // GET: Group
        public ActionResult Index()
        {
            var groupNamesList = db.GroupNames.OrderBy(n => n.Name).ToList();
            var groupNamesView = new GroupName
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: Group/Create
        [HttpPost]
        public ActionResult Create([Bind(Include = "Id,Name")] GroupName groupName)
        {
            if (ModelState.IsValid)
            {
                db.GroupNames.Add(groupName);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(groupName);
        }

        // GET: Group/Edit/5
        public ActionResult Edit(int id)
        {
            GroupName groupName = db.GroupNames.Find(id);
            if (groupName == null) return HttpNotFound();
            
            return View(groupName);
        }

        // POST: Group/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, GroupName groupName)
        {
            if (!ModelState.IsValid) return View(groupName);
            if (groupName.Name == null) { groupName.Name = ""; }
            db.Entry(groupName).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Group/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Group/Delete/5
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
        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}
