using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class AddressChecksController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: AddressChecks
        public ActionResult Index()
        {
            return View(db.AddressChecks.ToList());
        }

        // GET: AddressChecks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AddressChecks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Address,Note")] AddressCheck addressCheck)
        {
            if (ModelState.IsValid)
            {
                db.AddressChecks.Add(addressCheck);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(addressCheck);
        }

        // GET: AddressChecks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddressCheck addressCheck = db.AddressChecks.Find(id);
            if (addressCheck == null)
            {
                return HttpNotFound();
            }
            return View(addressCheck);
        }

        // POST: AddressChecks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Address,Note")] AddressCheck addressCheck)
        {
            if (ModelState.IsValid)
            {
                db.Entry(addressCheck).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(addressCheck);
        }

        // GET: AddressChecks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AddressCheck addressCheck = db.AddressChecks.Find(id);
            if (addressCheck == null)
            {
                return HttpNotFound();
            }
            return View(addressCheck);
        }

        // POST: AddressChecks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AddressCheck addressCheck = db.AddressChecks.Find(id);
            if (addressCheck != null) db.AddressChecks.Remove(addressCheck);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
