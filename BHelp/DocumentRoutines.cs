using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using BHelp.Models;

namespace BHelp
{
    public class DocumentRoutines
    {
        private abstract class NewDocumentsController
        {
            private readonly BHelpContext db;

            //public ActionResult Index();`
            //public ActionResult Upload();

            List<SelectListItem> menuCategories = LoadMenuCategories();
            public ActionResult Upload(Document doc, HttpPostedFileBase postedFile)
            { return null; }

            [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
            [ValidateAntiForgeryToken]
            [HttpGet]
            public ActionResult ViewDocument(int? id) { return null; }
            public ActionResult Edit(int? id) { return null; }
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit([Bind(Include = "Id,Title,MenuCategory")] Document document)
            { return null; }
        public ActionResult Delete(int? id) { return null; }
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public ActionResult DeleteConfirmed(int id) { return null; }

            public ActionResult ViewDocumentCategory(string menuCat) { return null; }

            private static List<SelectListItem> LoadMenuCategories()
            {
                var selList = new List<SelectListItem>();
                var role = new SelectListItem() { Value = "Everyone", Text = "Everyone" };
                selList.Add(role);
                role = new SelectListItem() { Value = "Administrator", Text = "Administrator" };
                selList.Add(role);
                role = new SelectListItem() { Value = "Staff", Text = "Staff" };
                selList.Add(role);
                role = new SelectListItem() { Value = "OfficerOfTheDay", Text = "OD" };
                selList.Add(role);
                role = new SelectListItem() { Value = "Bagger", Text = "Bagger" };
                selList.Add(role);
                role = new SelectListItem() { Value = "Driver", Text = "Driver" };
                selList.Add(role);
                role = new SelectListItem() { Value = "Reports", Text = "Reports" };
                selList.Add(role);
                role = new SelectListItem() { Value = "PantryCoordinator", Text = "Pantry Coordinator" };
                selList.Add(role);
                role = new SelectListItem() { Value = "DriverScheduler", Text = "DriverScheduler" };
                selList.Add(role);
                role = new SelectListItem() { Value = "Trainer", Text = "Trainer" };
                selList.Add(role);
                return selList;
            }

            protected abstract void Dispose(bool disposing);
        };
    }
}

