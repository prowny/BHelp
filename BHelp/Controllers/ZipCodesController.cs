using System.Collections.Generic;
using System.Data.Entity;
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
            var zipcodeList = new List<ZipCode>();
            if (db.ZipCodes.Any())
            {
                zipcodeList = db.ZipCodes.ToList();
                zipcodeList = zipcodeList.OrderBy(z => z.Zip).ToList();
            }

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
            if (ModelState.IsValid)
            {
                var newZipCode = new ZipCode { Zip = zipCode.Zip };
                db.ZipCodes.Add(newZipCode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(zipCode);
        }

        // GET: ZipCodes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ZipCode zipCode = db.ZipCodes.Find(id);
            if (zipCode == null)
            {
                return HttpNotFound();
            }
            return View(zipCode);
        }

        // POST: ZipCodes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Zip")] ZipCode zipCode)
        {
            if (ModelState.IsValid)
            {
                db.Entry(zipCode).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(zipCode);
        }

        // GET: ZipCodes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ZipCode zipCode = db.ZipCodes.Find(id);
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
