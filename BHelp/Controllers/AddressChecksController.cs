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
        [HttpGet]
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
            if (!ModelState.IsValid) return View(addressCheck);
            db.AddressChecks.Add(addressCheck);
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        // GET: AddressChecks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var addressCheck = db.AddressChecks.Find(id);
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
            if (!ModelState.IsValid) return View(addressCheck);
            db.Entry(addressCheck).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: AddressChecks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var addressCheck = db.AddressChecks.Find(id);
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
            var addressCheck = db.AddressChecks.Find(id);
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
