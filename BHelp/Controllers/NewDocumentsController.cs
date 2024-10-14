using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class NewDocumentsController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: NewDocuments
        public ActionResult Index()
        {
            var Documents = System.IO.Directory.GetFiles("../../Documents/");

            var view = new List<string>();
            string[] files = Directory.GetFiles("~/Images");
            foreach (var file in files)
            {
                view.Add(file);
            }
            return View((IEnumerable<NewDocument>)view);
        }

       
        // GET: NewDocuments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NewDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,MenuCategory,FileName,OriginatorId")] NewDocument newDocument)
        {
            if (ModelState.IsValid)
            {
                db.NewDocuments.Add(newDocument);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(newDocument);
        }

        // GET: NewDocuments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewDocument newDocument = db.NewDocuments.Find(id);
            if (newDocument == null)
            {
                return HttpNotFound();
            }
            return View(newDocument);
        }

        // POST: NewDocuments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,MenuCategory,FileName,OriginatorId")] NewDocument newDocument)
        {
            if (ModelState.IsValid)
            {
                db.Entry(newDocument).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(newDocument);
        }

        // GET: NewDocuments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewDocument newDocument = db.NewDocuments.Find(id);
            if (newDocument == null)
            {
                return HttpNotFound();
            }
            return View(newDocument);
        }

        // POST: NewDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var newDocument = db.NewDocuments.Find(id);
            db.NewDocuments.Remove(newDocument);
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
