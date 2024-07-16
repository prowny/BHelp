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
            var zipcodeList = AppRoutines.GetZipCodesListNew(); 
            return View(zipcodeList);
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
            var zipCodeList = AppRoutines .GetZipCodesListNew();
            if (zipCodeList.Any(z => z.Zip == zipCode.Zip)) // check for duplicates
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
            var zipCode = db.ZipCodes.Find(id);
            if (zipCode == null)
            {
                return HttpNotFound();
            }
            return View(zipCode);
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
