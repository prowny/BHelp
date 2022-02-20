using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class DocumentsController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Documents
        public ActionResult Index()
        {
            return View(db.Documents.ToList());
        }

       // GET: Documents/Upload
        public ActionResult Upload()
        {
            var view = new Document { Categories = new List<SelectListItem>() };
            var role = new SelectListItem() { Value = "Everyone", Text = @"Everyone" };
            view.Categories.Add(role);
             role = new SelectListItem() {Value = "Administrator", Text = @"Administrator"};
            view.Categories.Add(role);
             role = new SelectListItem() { Value = "Staff", Text = @"Staff" };
            view.Categories.Add(role);
            role = new SelectListItem() { Value = "Driver", Text = @"Driver" };
            view.Categories.Add(role);
            role = new SelectListItem() { Value = "Reports", Text = @"Reports" };
            view.Categories.Add(role);
            return View(view);
        }

        // POST: Documents/Upload
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(Document doc, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                
                //db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
   

        // GET: Documents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,MenuCategory,FileName,OrginatorId")] Document document)
        {
            if (ModelState.IsValid)
            {
                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(document);
        }

        // GET: Documents/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }
            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Document document = db.Documents.Find(id);
            if (document != null) db.Documents.Remove(document);
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
