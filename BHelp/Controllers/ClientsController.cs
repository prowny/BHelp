using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Clients
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
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
                    Age = AppRoutines.GetAge(client.DateOfBirth),
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
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult Create()
        {
            var viewModel = new ClientViewModel {FamilyMembers = new List<FamilyMember>()};
            // Add Empty family members:  
            for (int i = 0; i < 10; i++)
            {
                var newMember = new FamilyMember
                { Id = i };
                viewModel.FamilyMembers.Add(newMember);
            }
            viewModel.ZipCodes = AppRoutines.GetZipCodesSelectList();

            if (Request.UrlReferrer != null)
            {
                viewModel.ReturnURL = Request.UrlReferrer.ToString();
            }

            return View(viewModel);
        }

        // POST: Clients/Create
        [HttpPost,Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Age,StreetNumber,"
                              + "StreetName,City,Zip,Phone,Notes,FamilyMembers,"
                              + "ReturnURL" )] ClientViewModel client)
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

                if (client.ReturnURL.Contains("OD"))
                {
                    return RedirectToAction("Index", "OD");
                }

                return RedirectToAction("Index");
            }

            client.ZipCodes = AppRoutines.GetZipCodesSelectList();
            return View(client);
        }

        // GET: Clients/Edit/5
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
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
            client.Age = AppRoutines.GetAge(client.DateOfBirth);
            foreach (var item in client.ZipCodes)
            {
                if (item.Value == client.Zip)
                {
                    item.Selected = true;
                    break;
                }
            }
            if (Request.UrlReferrer != null)
            {
                client.ReturnURL = Request.UrlReferrer.ToString();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Active,FirstName,LastName,Age," +
                           "StreetNumber,StreetName,City,Zip,Phone,Notes,ReturnURL")] Client client)
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
                if (client.ReturnURL.Contains("LatestDeliveries"))
                {
                    return RedirectToAction("ActiveClientsLatestDeliveries");
                }
                return RedirectToAction("Index");
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
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
        [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
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

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult ClientListToCSV()
        {
            //var view = GetClientViewModel();
            var view = Utilities.GetAllClientsListModel();
            const int columns = 22;
            var curMonth = DateTime.Now.ToString("MMMM");
            var curYear = DateTime.Now.Year.ToString();

            var sb = new StringBuilder();
            sb.Append(view.ReportTitle + ',');
            sb.Append(DateTime.Today.ToShortDateString() + ',');
            sb.AppendLine();

            sb.Append("Active,Last Name,First Name,Age,Street #,Street Name,City,Zip,");
            sb.Append("Phone,Children,Adults,Seniors,Adults Names/Ages,Kids Names/Ages,");
            sb.Append("# In HH,Notes,Date Last Delivery,Date Last Gift Card,");
            sb.Append("Next Eligeble for Food on,Next Eligible for Gift Card(s) on,");
            sb.Append("# Deliveries " + curMonth + " " + curYear + ",");
            sb.Append("Internal Client ID");
            sb.AppendLine();

            for (var i = 0; i < view.ClientCount; i++)
            {
                for (var col = 1; col < columns +1; col++)
                {
                    if (view.ClientStrings[i, col] != null)
                    {
                        // Remove carriage returns. line feeds, tabs:
                        view.ClientStrings[i,col]= Regex.Replace(view.ClientStrings[i, col], @"\t|\n|\r", "");
                        if (view.ClientStrings[i, col].Contains(","))
                        {
                            sb.Append(view.ClientStrings[i, col].Replace(",", ";") + ",");
                        }
                        else
                        {
                            sb.Append(view.ClientStrings[i, col] + ",");
                        }
                    }
                    else
                    {
                        sb.Append(view.ClientStrings[i, col] + ",");
                    }
                }
                sb.AppendLine();
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=" + view.ReportTitle + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
            return null;
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult ClientListToExcel()
        {
            var view = GetClientViewModel();
            int columns = 22; 
            var curMonth = DateTime.Now.ToString("MMMM");
            var curYear = DateTime.Now.Year.ToString();
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue(view.ReportTitle + "  " + DateTime.Today.ToShortDateString());
            ws.Cell(activeRow, 1).Style.Alignment.WrapText = true;
            ws.Cell(activeRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Range(ws.Cell(activeRow, 1), ws.Cell(activeRow, 3)).Merge();
            
            ws.Cell(activeRow, 10).SetValue("#");
            ws.Cell(activeRow, 11).SetValue("#");
            ws.Cell(activeRow, 12).SetValue("#");
            ws.Cell(activeRow, 15).SetValue("#");
            ws.Cell(activeRow, 17).SetValue("Date");
            ws.Cell(activeRow, 18).SetValue("Date");
            ws.Cell(activeRow, 19).SetValue("Next");
            ws.Cell(activeRow, 20).SetValue("Next");
            ws.Cell(activeRow, 21).SetValue("Deliveies");
            ws.Cell(activeRow, 22).SetValue("Internal");
            for (var i = 10; i < columns + 1; i++)
            {
                ws.Cell(activeRow, i).Style.Font.Bold = true;
                ws.Cell(activeRow, i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }
            
            activeRow++;
            ws.Columns("1").Width = 5;
            ws.Cell(activeRow, 1).SetValue("Active");
            ws.Columns("2").Width = 12;
            ws.Cell(activeRow, 2).SetValue("Last Name");
            ws.Columns("3").Width = 12;
            ws.Cell(activeRow, 3).SetValue("First Name");
            ws.Columns("4").Width = 4;
            ws.Cell(activeRow, 4).SetValue("Age");
            ws.Columns("5").Width = 7;
            ws.Cell(activeRow, 5).SetValue("Street #");
            ws.Columns("6").Width = 15;
            ws.Cell(activeRow, 6).SetValue("Street Name");
            ws.Columns("7").Width = 10;
            ws.Cell(activeRow, 7).SetValue("City");
            ws.Columns("8").Width = 6;
            ws.Cell(activeRow, 8).SetValue("Zip");
            ws.Columns("9").Width = 13;
            ws.Cell(activeRow, 9).SetValue("Phone");
            ws.Columns("10").Width = 3;
            ws.Cell(activeRow, 10).SetValue("C");
            ws.Columns("11").Width = 3;
            ws.Cell(activeRow, 11).SetValue("A");
            ws.Columns("12").Width = 3;
            ws.Cell(activeRow, 12).SetValue("S");
            ws.Columns("13").Width = 20;
            ws.Cell(activeRow, 13).SetValue("Adults Names/Ages");
            ws.Columns("14").Width = 20;
            ws.Cell(activeRow, 14).SetValue("Kids Names/Ages");
            ws.Columns("15").Width = 3;
            ws.Cell(activeRow, 15).SetValue("HH");
            ws.Columns("16").Width = 20;
            ws.Cell(activeRow, 16).SetValue("Notes");
            ws.Columns("17").Width =13;
            ws.Cell(activeRow, 17).SetValue("Date Last Delivery");
            ws.Columns("18").Width = 13;
            ws.Cell(activeRow, 18).SetValue("Last Gift Card");
            ws.Columns("19").Width = 13;
            ws.Cell(activeRow, 19).SetValue("Eligible for Food on");
            ws.Columns("20").Width = 13;
            ws.Cell(activeRow, 20).SetValue("Eligible for Gift Card(s) on");
            ws.Cell(activeRow, 21).SetValue(curMonth + " " + curYear );
            ws.Columns("22").Width = 7;
            ws.Cell(activeRow, 22).SetValue("ClientID");

            for (var i = 1; i < columns + 1; i++)
            {
                ws.Cell(activeRow, i).Style.Font.Bold = true;
                ws.Cell(activeRow, i).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            for (var i = 0; i < view.ClientCount; i++)
            {
                activeRow++;
                for (var j = 1; j < columns + 1; j++)
                {
                    ws.Cell(activeRow, j).SetValue(view.ClientStrings[i, j]);
                    ws.Cell(activeRow, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                    ws.Cell(activeRow, j).Style.Alignment.WrapText = true;
                }
            }
            
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
            var columns = 22;  // ! add one for seniors names/ages?

            using (var db = new BHelpContext())
            {
                var clientList  = new List<Client>(db.Clients )
                    .OrderBy(d => d.LastName).ToList();
                cvm.ClientCount = clientList.Count;
                cvm.ClientStrings = new string[clientList.Count, columns + 1];
                var i = 0;
                foreach (var cli in clientList)
                {
                    cvm.Id = cli.Id;
                    cvm.ClientStrings[i, 1] = cli.Active.ToString();
                    cvm.ClientStrings[i, 2] = cli.LastName;
                    cvm.ClientStrings[i, 3] = cli.FirstName;
                    var age = AppRoutines.GetAge(cli.DateOfBirth);
                    cvm.ClientStrings[i, 4] = age.ToString();
                    cvm.ClientStrings[i, 5] = cli.StreetNumber;
                    cvm.ClientStrings[i, 6] = cli.StreetName;
                    cvm.ClientStrings[i, 7] = cli.City;
                    cvm.ClientStrings[i, 8] = cli.Zip;
                    cvm.ClientStrings[i, 9] = cli.Phone;
                    var familyList = db.FamilyMembers  // Add HH to familyList
                        .Where(f => f.ClientId == cli.Id).ToList();
                    cvm.ClientStrings[i, 10] = GetChildrenCount(familyList);
                    cvm.ClientStrings[i, 11] = GetAdultCount(familyList);
                    cvm.ClientStrings[i, 12] = GetSeniorCount(age, familyList); // Age of Head of Household
                    cvm.ClientStrings[i, 13] = GetAdultNamesAges(cli.Id, familyList);
                    cvm.ClientStrings[i, 14] = GetKidsNamesAges(familyList);
                    var numberInHousehold = familyList.Count;
                    cvm.ClientStrings[i, 15] = numberInHousehold.ToString();
                    cvm.ClientStrings[i, 16] = cli.Notes;
                    var lastDD = AppRoutines.GetLastDeliveryDate(cli.Id);
                    if (lastDD.Year < 2000)
                    { cvm.ClientStrings[i, 17] = " - - "; }
                    else
                    { cvm.ClientStrings[i, 17] = lastDD.ToShortDateString(); }

                    var lastGC = AppRoutines.GetDateLastGiftCard(cli.Id);
                    if (lastGC.Year < 2000)
                    { cvm.ClientStrings[i, 18] = " - - "; }
                    else
                    { cvm.ClientStrings[i, 18] = lastGC.ToShortDateString(); }

                    var nextEDD = AppRoutines.GetNextEligibleDeliveryDate(cli.Id, DateTime.Now);
                    if (nextEDD.Year < 2000)
                    { cvm.ClientStrings[i, 19] =" (now)"; }
                    else
                    { cvm.ClientStrings[i, 19] = nextEDD.ToShortDateString(); }

                    var nextGCED = AppRoutines.GetNextGiftCardEligibleDate(cli.Id, DateTime.Now);
                    if (nextGCED.Year < 2000)
                    { cvm.ClientStrings[i, 20] = " (now)"; }
                    else
                    { cvm.ClientStrings[i, 20] = nextGCED.ToShortDateString(); }

                    cvm.ClientStrings[i, 21] = AppRoutines.GetDeliveriesCountThisMonth(cli.Id, DateTime.Now).ToString();
                    
                    cvm.ClientStrings[i,22] = cli.Id.ToString();
               
                    i++;
                }
                return cvm;
            }
        }

        private static string GetChildrenCount(List<FamilyMember> familyList)
        {
            var result = 0; // No Head of Household in Children count
            foreach (var mbr in familyList)
            {
                var age = AppRoutines.GetAge(mbr.DateOfBirth);
                if (age <= 17)
                { result++; }
            }
            return result.ToString();
        }

        private static string GetAdultCount(List<FamilyMember> familyList)
        {
            var result = 1; // Add Head of Household to Adults count
            foreach (var mbr in familyList)
            {
                var age = AppRoutines.GetAge(mbr.DateOfBirth);
                if (age > 17 && age < 60)
                { result++; }
            }
            return result.ToString();
        }

        private static string GetSeniorCount(int ageOfHeadOfHousehold, List<FamilyMember> familyList)
        {
            var result = 0;
            if (ageOfHeadOfHousehold >= 60) { result = 1;}
            foreach (var mbr in familyList)
            {
                var age = AppRoutines.GetAge(mbr.DateOfBirth);
                if (age >= 60)
                { result++; }
            }
            return result.ToString();
        }

        private static string GetAdultNamesAges(int clientId, List<FamilyMember> familyList)
        {
            var strResult = "";
            using (var db = new BHelpContext())
            { var client = db.Clients.Find(clientId);
                if (client != null)
                {
                    strResult = client.FirstName + " " + client.LastName + "/";
                    strResult += AppRoutines.GetAge(client.DateOfBirth).ToString();
                }
            }
            
            foreach (var mbr in familyList)
            {
                var age = AppRoutines.GetAge(mbr.DateOfBirth);
                if (age >= 18)
                {
                    strResult += ", " + mbr.FirstName + " " + mbr.LastName + "/";
                    strResult += AppRoutines.GetAge(mbr.DateOfBirth).ToString();
                }
            }
            return strResult;
        }

        private static string GetKidsNamesAges(List<FamilyMember> familyList)
        {
            var strResult = "";
            foreach (var mbr in familyList)
            {
                var age = AppRoutines.GetAge(mbr.DateOfBirth);
                if (age <= 17)
                {
                    if (strResult.Length != 0)
                    { strResult += ", "; }
                    strResult += mbr.FirstName + " " + mbr.LastName + "/";
                    strResult += AppRoutines.GetAge(mbr.DateOfBirth).ToString();
                }
            }
            return strResult;
        }

        public ActionResult ActiveClientsLatestDeliveries()
        {
            var view = GetLatestDeliveriesViewModel();
            return View(view);
        }

        private List<DeliveryViewModel> GetLatestDeliveriesViewModel()
        {
            var query = from c in db.Clients
                from d in db.Deliveries
                    .Where(e => e.ClientId == c.Id && e.Status == 1 && c.Active)
                    .OrderByDescending( f => f.DateDelivered).Take(1)
                select new
                {
                    d.DateDelivered,
                    c.Id,
                    c.LastName,
                    c.FirstName,
                    c.StreetNumber,
                    c.StreetName,
                    c.City,
                    c.Zip
                };
            var delRecs = query
                .OrderBy(d => d.DateDelivered);

            var view = new List<DeliveryViewModel>();
            foreach (var rec in delRecs)
            {
                var dt = rec.DateDelivered ?? DateTime.Now;
                DeliveryViewModel del = new DeliveryViewModel()
                {
                    ClientId = rec.Id,
                    DateDeliveredString = dt.ToString("MM/dd/yyyy"),
                    LastName = rec.LastName,
                    FirstName = rec.FirstName,
                    City = rec.City,
                    StreetNumber = rec.StreetNumber,
                    StreetName = rec.StreetName,
                    Zip = rec.Zip,
                    ReturnURL = "LatestDeliveries" 
                };
                view.Add(del);
            }
            return view;
        }

        public ActionResult ActiveClientsLatestDeliveriesToCSV()
        {
            var view = GetLatestDeliveriesViewModel();
            var sb = new StringBuilder();
            sb.Append("Active Clients Latest Deliveries" + ',');
            sb.Append(DateTime.Today.ToShortDateString() + ',');
            sb.AppendLine();

            sb.Append("Delivery Date,Last Name,First Name,Street #,Street Name,City,Zip");
            sb.AppendLine();

            for (var i = 0; i < view.Count; i++)
            {
                sb.Append(view [i].DateDeliveredString + ",");
                sb.Append(view[i].LastName + "," + view[i].FirstName + ",");
                sb.Append(view[i].StreetNumber + ",");
                sb.Append(view[i].StreetName.Replace(",", ";") + ",");
                sb.Append(view[i].City + "," + view[i].Zip);
                sb.AppendLine();
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename=LatestDeliveries" + DateTime.Today.ToShortDateString() + ".csv");
            response.ContentType = "text/plain";
            response.Write(sb.ToString());
            response.End();
            return null;
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
