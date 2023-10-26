using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Documents
        public ActionResult Index()
        {
            var view = db.Documents.OrderBy(d => d.MenuCategory).ToList();
            foreach (var doc in view.Where(doc => doc.MenuCategory == "OfficerOfTheDay"))
            {
                doc.MenuCategory = "OD";
            }

            return View(view);
        }

            // GET: Documents/Upload
            public ActionResult Upload()
        {
            var view = new Document { Categories = LoadMenuCategories()};
            if (TempData["DocumentTitleError"] != null)
            {view.TitleErrorMessage = TempData["DocumentTitleError"].ToString(); }

            if (TempData["DocumentFileError"] != null)
            {
                view.FileErrorMessage = TempData["DocumentFileError"].ToString();
                view.Title=TempData ["DocumentTitle"].ToString();
            }

            return View(view);
        }

        private static List<SelectListItem> LoadMenuCategories()
        {
            var selList = new List<SelectListItem>();
            var role = new SelectListItem() { Value = "Everyone", Text = @"Everyone" };
            selList.Add(role);
            role = new SelectListItem() { Value = "Administrator", Text = @"Administrator" };
            selList.Add(role);
            role = new SelectListItem() { Value = "Staff", Text = @"Staff" };
            selList.Add(role);
            role = new SelectListItem() { Value = "OfficerOfTheDay", Text = @"OD" };
            selList.Add(role);
            role = new SelectListItem() { Value = "Driver", Text = @"Driver" };
            selList.Add(role); 
            role = new SelectListItem() { Value = "Reports", Text = @"Reports" };
            selList.Add(role);
            role = new SelectListItem() { Value = "PantryCoordinator", Text = @"Pantry Coordinator" };
            selList.Add(role);
            role = new SelectListItem() { Value = "DriverScheduler", Text = @"DriverScheduler" };
            selList.Add(role);
            role = new SelectListItem() { Value = "Trainer", Text = @"Trainer" };
            selList.Add(role);
            return selList;
        }

        // POST: Documents/Upload
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(Document doc, HttpPostedFileBase postedFile)
        {
            if (ModelState.IsValid)
            {
                if (doc.Title.IsNullOrEmpty())
                {
                    TempData["DocumentTitleError"] = "Title Required!";
                    return RedirectToAction("Upload");
                }

                if(postedFile == null)
                {
                    TempData ["DocumentTitle"] = doc.Title;
                    TempData["DocumentFileError"] = "File Required!";
                    return RedirectToAction("Upload");
                }

                var fileData = new MemoryStream();
                postedFile.InputStream.CopyTo(fileData);
                var newDoc = new Document()
                {
                    Title =doc.Title,
                    MenuCategory=doc.MenuCategory,
                    FileName = postedFile.FileName,
                    OriginatorId = User.Identity.GetUserId(),
                    FileContent = fileData.ToArray()  
                };
                db.Documents.Add(newDoc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
   
        // GET: Document View
        [HttpGet]
        public ActionResult ViewDocument(int? id)
        {
            var doc = db.Documents.Find(id);
            if (doc != null)
            {
                var contentType = MimeMapping.GetMimeMapping(doc.FileName);
                return new FileContentResult(doc.FileContent, contentType);
            }
            return null;
        }

        // GET: Documents/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var document = db.Documents.Find(id);
            if (document == null) return HttpNotFound();

            document.Categories = LoadMenuCategories();
            foreach (var cat in document.Categories)
            {
                if (cat.Value == document.MenuCategory)
                {
                    cat.Selected = true;
                    break;
                }
            }
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,MenuCategory")] Document document)
        {
            if (ModelState.IsValid)
            {
                var updateData = db.Documents.Find(document.Id);
                if (updateData != null)
                {
                    updateData.Title = document.Title;
                    updateData.MenuCategory = document.MenuCategory;
                    db.Entry(updateData).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Edit", new{ id = document.Id });
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
            var document = db.Documents.Find(id);
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
            var document = db.Documents.Find(id);
            if (document != null) db.Documents.Remove(document);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ViewDocumentCategory(string menuCat)
        {
            var docList = db.Documents.Where(d => d.MenuCategory == menuCat).ToList();
            var view = new DocumentsViewModel()
            {
                MenuCategory = menuCat,
                DocNamesUpperBound = docList.Count,
                DocNames = new string[docList.Count],
                DocIds = new int[docList.Count]
            };
            for (var i = 0; i < docList.Count; i++)
            {
                view.DocNames[i] = docList[i].Title;
                view.DocIds[i] = docList[i].Id;
            }

            return View(view);
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
