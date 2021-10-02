using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    public class ClientsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Clients
        public ActionResult Index()
        {
            var clientList = db.Clients.OrderBy(c => c.LastName).ToList();
            var clientView = new List<ClientViewModel>();
            foreach (var client in clientList)
            {
                if (client.Phone == null)
                {
                    client.Phone = "";}
                var household = new ClientViewModel()
                {
                    Id = client.Id,
                    Active = client.Active,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    Age = AppRoutines.GetAge(client.DateOfBirth,DateTime.Today),
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    StreetToolTip = client.StreetName.Replace(" ", "\u00a0"),
                    City = client.City,
                    CityToolTip = client.City.Replace(" ", "\u00a0"),
                    Zip = client.Zip,
                    Phone = client.Phone,
                    PhoneToolTip = client.Phone.Replace(" ", "\u00a0"),
                    Notes = client.Notes,
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    NotesToolTip = client.Notes.Replace(" ", "\u00a0")
                };
                var s = household.StreetName; // For display, abbreviate to 10 characters:           
                s = s.Length <= 10 ? s : s.Substring(0, 10) + "...";
                household.StreetName = s;
                s = household.City; // For display, abbreviate to 11 characters:           
                s = s.Length <= 11 ? s : s.Substring(0, 11) + "...";
                household.City = s;
                s = household.Phone; // For display, abbreviate to 12 characters:           
                s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                household.Phone = s;
                s = household.Notes; // For display, abbreviate to 12 characters:           
                s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                household.Notes = s;
                clientView.Add(household);
            }
            return View(clientView);
        }

        // GET: Clients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // GET: Clients/Create
        public ActionResult Create()
        {
            var viewModel = new ClientViewModel {FamilyMembers = new List<FamilyMember>()};
            // Empty family members:  
            for (int i = 0; i < 10; i++)
            {
                var newMember = new FamilyMember
                { Id = i };
                viewModel.FamilyMembers.Add(newMember);
            }
            viewModel.ZipCodes = AppRoutines.GetZipCodesSelectList();
            return View(viewModel);
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Age,StreetNumber,StreetName,City,Zip,Phone,Notes,FamilyMembers")] ClientViewModel client)
        {
            if (ModelState.IsValid)
            {
                var newClient = new Client()
                {
                    Active = true,
                    DateOfBirth = DateTime.Today.AddYears(-client.Age),
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    StreetNumber = client.StreetNumber,
                    StreetName = client.StreetName,
                    Phone = client.Phone + "",
                    City = client.City,
                    Zip = client.Zip,
                    Notes = client.Notes + ""
                };
                
                db.Clients.Add(newClient);
                db.SaveChanges();
                int clientId = newClient.Id;

                foreach (var member in client.FamilyMembers)
                {
                    if (member.FirstName != null || member.LastName != null)
                    {
                        var newMember = new FamilyMember()
                        {
                            ClientId = clientId,
                            FirstName = member.FirstName,
                            LastName = member.LastName,
                            DateOfBirth = DateTime.Today.AddYears(-member.Age),
                            Active = true
                        };
                        db.FamilyMembers.Add(newMember);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("Index");
            }

            return View(client);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            client.ZipCodes = AppRoutines.GetZipCodesSelectList();
            client.Age = AppRoutines.GetAge(client.DateOfBirth, DateTime.Today);
            foreach (var item in client.ZipCodes)
            {
                if (item.Value == client.Zip)
                {
                    item.Selected = true;
                    break;
                }
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Active,FirstName,LastName,Age," +
                           "StreetNumber,StreetName,City,Zip,Phone,Notes")] Client client)
        {
            if (ModelState.IsValid)
            {
                if (client.FirstName == null) { client.FirstName = ""; }
                if (client.LastName == null) { client.LastName = ""; }
                if (client.StreetNumber == null) { client.StreetNumber = ""; }
                if (client.StreetName == null) { client.StreetName = ""; }
                if (client.City == null) { client.City = ""; }
                if (client.Phone == null) { client.Phone = ""; }
                if (client.Notes == null) { client.Notes = ""; }
                client.DateOfBirth=DateTime.Today.AddYears(-client.Age);
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null) { return HttpNotFound(); }
           
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var openDeliveries = db.Deliveries.Where(d => d.ClientId == id && d.DateDelivered == null).ToList();
            if (openDeliveries.Count > 0)
            {
                ModelState.AddModelError("", @"Client has open deliveries that must be removed first.");
                return View();
            }
            var client = db.Clients.Find(id);
            if (client != null) db.Clients.Remove(client);
            db.FamilyMembers.RemoveRange(db.FamilyMembers.Where(m => m.ClientId == id));
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ClientListToExcel()
        {
            var view = GetClientViewModel();
            int columns = 21;
            var curMonth = DateTime.Now.ToString("MMMM");
            var curYear = DateTime.Now.Year.ToString();
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue(view.ReportTitle);
            ws.Cell(activeRow, 1).Style.Font.Bold = true;
            ws.Cell(activeRow, 2).SetValue(DateTime.Today.ToShortDateString());
            ws.Cell(activeRow, 2).Style.Font.Bold = true;

            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Active");
            ws.Cell(activeRow, 2).SetValue("Last Name");
            ws.Cell(activeRow, 3).SetValue("First Name");
            ws.Cell(activeRow, 4).SetValue("Age");
            ws.Cell(activeRow, 5).SetValue("Street #");
            ws.Cell(activeRow, 6).SetValue("Street Name");
            ws.Cell(activeRow, 7).SetValue("City");
            ws.Cell(activeRow, 8).SetValue("Zip");
            ws.Cell(activeRow, 9).SetValue("Phone");
            ws.Cell(activeRow, 10).SetValue("Children");
            ws.Cell(activeRow, 11).SetValue("Adults");
            ws.Cell(activeRow, 12).SetValue("Seniors");
            ws.Cell(activeRow, 13).SetValue("Adults Names/Ages");
            ws.Cell(activeRow, 14).SetValue("Kids Names/Ages");
            ws.Cell(activeRow, 15).SetValue("# in HH");
            ws.Cell(activeRow, 16).SetValue("Notes");
            ws.Cell(activeRow, 17).SetValue("Date Last Delivery");
            ws.Cell(activeRow, 18).SetValue("Date Last Gift Card");
            ws.Cell(activeRow, 19).SetValue("Next Eligible for Food on");
            ws.Cell(activeRow, 20).SetValue("Next Eligible for Gift Card(s) on");
            ws.Cell(activeRow, 21).SetValue("# Deliveries " + curMonth + " " + curYear );
            for (var i = 1; i < columns + 1; i++)
            { ws.Cell(activeRow, i).Style.Font.Bold = true; }

            for (var i = 0; i < view.ClientCount; i++)
            {
                activeRow++;
                for (var j = 1; j < columns; j++)
                {
                    ws.Cell(activeRow, j).SetValue(view.ClientStrings[i, j]);
                }
            }

            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            var fileName = "BH Running Client List " + DateTime.Now.ToShortDateString();
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = fileName + ".xlsx" };
        }

        private static ClientViewModel GetClientViewModel()
        {
            var cvm = new ClientViewModel { ReportTitle  = "BH Food Client List" };
            var columns = 21;

            using (var db = new BHelpContext())
            {
                var clientList  = new List<Client>(db.Clients )
                    .OrderBy(d => d.LastName).ToList();
                cvm.ClientCount = clientList.Count;
                cvm.ClientStrings = new string[clientList.Count, columns];
                var i = 0;
                foreach (var cli in clientList)
                {
                    cvm.ClientStrings[i, 1] = cli.Active.ToString();
                    cvm.ClientStrings[i, 2] = cli.LastName;
                    cvm.ClientStrings[i, 3] = cli.FirstName;
                    var age = AppRoutines.GetAge(cli.DateOfBirth, DateTime.Today);
                    cvm.ClientStrings[i, 4] = age.ToString();
                    cvm.ClientStrings[i, 5] = cli.StreetNumber;
                    cvm.ClientStrings[i, 6] = cli.StreetName;
                    cvm.ClientStrings[i, 7] = cli.City;
                    cvm.ClientStrings[i, 8] = cli.Zip;
                    cvm.ClientStrings[i, 9] = cli.Phone;


                    cvm.ClientStrings[i, 16] = cli.Notes;
                    i++;
                }
                return cvm;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
