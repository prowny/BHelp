using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class ZipCodesController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: ZipCodes
        public ActionResult Index()
        {
            return View(db.ZipCodes.OrderBy(z => z.Zip).ToList());
        }
        
        // GET: ZipCodes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ZipCodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Zip")] ZipCode zipCode)
        {
            if (!ModelState.IsValid) return View(zipCode);
            var zipCodeList = AppRoutines .GetZipCodesList();
            if (zipCodeList.Any(z => z == zipCode.Zip)) // check for duplicates
            {
                return View(zipCode);
            }

            var newZipCode = new ZipCode { Zip = zipCode.Zip };
                db.ZipCodes.Add(newZipCode);
                db.SaveChanges();
                return RedirectToAction("Index");
        }
      
        // GET: ZipCodes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var zipRec = db.ZipCodes.Find(id);
            if (zipRec == null)
            {
                return HttpNotFound();
            }
            return View(zipRec);
        }

        // POST: ZipCodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var zipCode = db.ZipCodes.Find(id);
            if (zipCode != null) db.ZipCodes.Remove(zipCode);
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
