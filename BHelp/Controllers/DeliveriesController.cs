using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    public class DeliveriesController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Deliveries
        public ActionResult Index()
        {
            var listDeliveries = new List<Delivery>(db.Deliveries).Where(d => d.DateDelivered == null)
                .OrderBy(d => d.DeliveryDate).ThenBy(z => z.Zip).ToList();
            // to add order .ThenBy(Zip) means storing the Zip in Client.cs AND Delivery/cs
            var listDeliveryViewModels = new List<DeliveryViewModel>();
            foreach (var delivery in listDeliveries)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    var deliveryView = new DeliveryViewModel
                    {
                        Id = delivery.Id,
                        ClientId = client.Id,
                        Client = client,
                        DriverId = delivery.DriverId,
                        DeliveryDate =delivery.DeliveryDate,
                        FamilyMembers = AppRoutines.GetFamilyMembers(client.Id),
                        FamilySelectList = AppRoutines.GetFamilySelectList(client.Id),
                        Kids = new List<FamilyMember>(),
                        Adults = new List<FamilyMember>(),
                        Seniors = new List<FamilyMember>()
                    };
                    foreach (var mbr in deliveryView.FamilyMembers)
                    {
                        mbr.Age = AppRoutines.GetAge(mbr.DateOfBirth, DateTime.Today);
                        if (mbr.Age < 18)
                        {
                            deliveryView.Kids.Add(mbr);
                            deliveryView.KidsCount += 1;
                        }

                        if (mbr.Age >= 18 && mbr.Age < 60)
                        {
                            deliveryView.Adults.Add(mbr);
                            deliveryView.AdultsCount += 1;
                        }

                        if (mbr.Age >= 60)
                        {
                            deliveryView.Seniors.Add(mbr);
                            deliveryView.SeniorsCount += 1;
                        }
                    }

                    deliveryView.KidsCount = deliveryView.Kids.Count();
                    deliveryView.AdultsCount = deliveryView.Adults.Count();
                    deliveryView.SeniorsCount = deliveryView.Seniors.Count();
                    if (delivery.FullBags != null) deliveryView.FullBags = (int) delivery.FullBags;
                    if (delivery.HalfBags != null) deliveryView.HalfBags = (int) delivery.HalfBags;
                    if (delivery.KidSnacks != null) deliveryView.KidSnacks = (int) delivery.KidSnacks;
                    if (delivery.GiftCards != null) deliveryView.GiftCards = (int) delivery.GiftCards;
                    deliveryView.GiftCardsEligible = 0; // !!! calculate this value
                    deliveryView.DateLastDelivery = DateTime.Today.AddDays(-7); // !!! calculate this value
                    deliveryView.DateLastGiftCard = DateTime.Today.AddDays(-7); // !!! calculate this value
                    if (delivery.DriverId != null)
                    {
                        var driver = db.Users.Find(delivery.DriverId);
                        if (driver != null)
                        { deliveryView.DriverName = driver.FullName;}
                        else
                        {
                            deliveryView.DriverName = "(nobody yet)";
                        }
                    }
                    
                    var ODid = delivery.ODId;          //System.Web.HttpContext.Current.User.Identity.GetUserId();
                    if (ODid != null)
                    {
                        var user = db.Users.Find(ODid);
                        deliveryView.ODName = user.FullName;
                    };
                    deliveryView.FirstName = client.FirstName;
                    deliveryView.LastName = client.LastName;
                    deliveryView.StreetNumber = client.StreetNumber;
                    deliveryView.StreetName = client.StreetName;
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    deliveryView.StreetToolTip = client.StreetName.Replace(" ", "\u00a0");
                    deliveryView.City = client.City;
                    deliveryView.CityToolTip = client.City.Replace(" ", "\u00a0");
                    deliveryView.Zip = client.Zip;
                    deliveryView.Phone = client.Phone;
                    deliveryView.PhoneToolTip = client.Phone.Replace(" ", "\u00a0");
                    string s;
                    if (client.Notes != null)
                    {
                        deliveryView.Notes = client.Notes;
                        deliveryView.NotesToolTip = client.Notes.Replace(" ", "\u00a0");
                        s = deliveryView.Notes;
                        s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                        deliveryView.Notes = s;
                    }

                    if (delivery.ODNotes != null)
                    {
                        deliveryView.ODNotes = delivery.ODNotes;
                        deliveryView.ODNotesToolTip = delivery.ODNotes.Replace(" ", "\u00a0");
                        s = deliveryView.ODNotes;
                        s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                        deliveryView.ODNotes = s;
                    }

                    if (delivery.DriverNotes != null)
                    {
                        deliveryView.DriverNotes = delivery.DriverNotes;
                        deliveryView.DriverNotesToolTip = deliveryView.DriverNotes.Replace(" ", "\u00a0");
                        s  = deliveryView.DriverNotes; // For display, abbreviate to 12 characters:           
                        s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                        deliveryView.DriverNotes = s;
                    }

                    s = deliveryView.StreetName;
                    s = s.Length <= 9 ? s : s.Substring(0, 9) + "...";
                    deliveryView.StreetName = s;
                    s = deliveryView.City; // For display, abbreviate to 11 characters:           
                    s = s.Length <= 10 ? s : s.Substring(0, 10) + "...";
                    deliveryView.City = s;
                    s = deliveryView.Phone; // For display, abbreviate to 12 characters:           
                    s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                    deliveryView.Phone = s;
                    listDeliveryViewModels.Add(deliveryView);
                }
            }
            return View(listDeliveryViewModels);
        }
 

        // GET: Deliveries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Delivery delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }

            return View(delivery);
        }

        // GET: Deliveries/Create
        public ActionResult Create()
        {
            return null;
            //return View();
        }

        // POST: Deliveries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Id,ClientId,DeliveryDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards")]
            Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                Client client = db.Clients.Find(delivery.ClientId);
                if (client != null) delivery.Zip = client.Zip;
                db.Deliveries.Add(delivery);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(delivery);
        }

        // GET: Deliveries/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            var viewModel = new DeliveryViewModel()
            {
                Id = delivery.Id,
                ClientId = delivery.ClientId,
                DeliveryDate = Convert.ToDateTime(delivery.DeliveryDate.ToString("MM/dd/yyyy")),
                ODNotes = delivery.ODNotes,
                DriverId = delivery.DriverId,
                DriverNotes = delivery.DriverNotes,
                FamilyMembers = AppRoutines.GetFamilyMembers(delivery.ClientId),
                FamilySelectList = AppRoutines.GetFamilySelectList(delivery.ClientId)
            };
  
            if (delivery.DateDelivered != null) viewModel.DateDelivered = (DateTime) delivery.DateDelivered;

            viewModel.DriversList = AppRoutines.GetDriversSelectList();
            foreach (var item in viewModel.DriversList)
            {
                if (item.Value == viewModel.DriverId)
                {
                    item.Selected = true;
                    break;
                }
            }

            foreach (var mbr in viewModel.FamilyMembers)
            {
                mbr.Age = AppRoutines.GetAge(mbr.DateOfBirth, DateTime.Today);
                if (mbr.Age < 18) { viewModel.KidsCount += 1; }
                if (mbr.Age >= 18 && mbr.Age < 60) { viewModel.AdultsCount += 1; }
                if (mbr.Age >= 60) {viewModel.SeniorsCount += 1; }
            }
            var client = db.Clients.Find(delivery.ClientId);
            if (client != null)
            {
                viewModel.Client = client;
                viewModel.ClientNameAddress = client.LastName + ", " + client.FirstName
                                              + " " + client.StreetNumber + " " + client.StreetName + " " + client.Zip;
                viewModel.Notes = client.Notes;
            }

            if (delivery.FullBags != null) viewModel.FullBags = (int) delivery.FullBags;
            if (delivery.HalfBags != null) viewModel.HalfBags = (int) delivery.HalfBags;
            if (delivery.KidSnacks != null) viewModel.KidSnacks = (int) delivery.KidSnacks;
            if (delivery.GiftCardsEligible != null) viewModel.GiftCardsEligible = (int) delivery.GiftCardsEligible;
            viewModel.Zip = delivery.Zip;
            return View(viewModel);
        }

        // POST: Deliveries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id,ClientId,DeliveryDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards," +
                            "DateDelivered,ODNotes,DriverNotes,GiftCardsEligible,DriverId")]
            DeliveryViewModel delivery)
        {
            if (ModelState.IsValid)
            {
                var updateData  = db.Deliveries.Find(delivery.Id);

                if (updateData != null)
                {
                    Client client = db.Clients.Find(updateData.ClientId);
                    if (client != null) updateData.Zip = client.Zip;
                    updateData.DeliveryDate = delivery.DeliveryDate;
                    updateData.FullBags = delivery.FullBags;
                    updateData.HalfBags = delivery.HalfBags;
                    updateData.KidSnacks = delivery.KidSnacks;
                    updateData.GiftCards = delivery.GiftCards;
                    updateData.ODNotes = delivery.ODNotes;
                    updateData.DriverId = delivery.DriverId;
                    updateData.DriverNotes = delivery.DriverNotes;
                    updateData.DateDelivered = delivery.DateDelivered;
                    db.Entry(updateData).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(delivery);
        }

        // GET: Deliveries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Delivery delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }

            return View(delivery);
        }

        // POST: Deliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Delivery delivery = db.Deliveries.Find(id);
            if (delivery != null) db.Deliveries.Remove(delivery);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ReportsMenu()
        {
            return View();
        }

        public ActionResult CountyReport(string yy = "", string qtr = "")
        {
            int reportYear;
            int reportQuarter = 0;
            if (yy.IsNullOrEmpty() || qtr.IsNullOrEmpty())   // Default to this year, this quarter
            {
               reportYear = Convert.ToInt32(DateTime.Now.Year.ToString());
                var month = Convert.ToInt32(DateTime.Now.Month.ToString());
                if (month >= 1 && month <= 3) { reportQuarter = 1;}     
                if (month >= 4 && month <= 6) { reportQuarter = 2;}     
                if (month >= 7 && month <= 9) { reportQuarter = 3; }    
                if (month >= 10 && month <= 12) { reportQuarter = 4; }
            }
            else
            {
                reportYear = Convert.ToInt32(yy);
                reportQuarter = Convert.ToInt32(qtr);
            }

            var view = GetCountyReportView(reportYear, reportQuarter);
            return View(view);
        }

        private ReportsViewModel GetCountyReportView(int yy, int qtr)
        {
            var view = new ReportsViewModel()
            { Year = yy, Quarter = qtr };
            if (qtr == 1) { view.Months = new[] { 1, 2, 3 }; }
            if (qtr == 2) { view.Months = new[] { 4, 5, 6 }; }
            if (qtr == 3) { view.Months = new[] { 7, 8, 9 }; }
            if (qtr == 4) { view.Months = new[] { 10, 11, 12 }; }
            if (DateTimeFormatInfo.CurrentInfo != null)
            {
                view.MonthYear = new string[3];
                view.MonthYear[0] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(1 + 3 * (qtr - 1))
                    + " " + view.Year.ToString();
                view.MonthYear[1] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(2 + 3 * (qtr - 1))
                    + " " + view.Year.ToString();
                view.MonthYear[2] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(3 + 3 * (qtr - 1))
                    + " " + view.Year.ToString();
                view.DateRangeTitle = view.MonthYear[0] + " through " + view.MonthYear[2];
                view.ReportTitle = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(1 + 3 * (qtr - 1))
                                   + "-" + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(3 + 3 * (qtr - 1))
                                   + " " + view.Year.ToString() + " County Report";
            }

            view.ZipCodes = AppRoutines.GetZipCodesList();
            // Load MonthlyCounts - extra zip code is for totals column.
            view.MonthlyCounts = new int[13, view.ZipCodes.Count + 1, 6]; //Month, ZipCodes, Counts
            for (int i = 0; i < 3; i++)
            {
                var mY = view.MonthYear[i].Split(' ');
                var mo = DateTime.ParseExact(mY[0], "MMMM", CultureInfo.CurrentCulture).Month;
                var startDate = Convert.ToDateTime(mo.ToString() + "/01/" + mY[1]);
                DateTime endDate;
                if (mo == 12)
                {
                    var enDt = Convert.ToDateTime("12/31/" + mY[1]);
                    endDate = enDt.AddDays(1);
                }
                else
                {
                    endDate = Convert.ToDateTime((mo + 1).ToString() + "/01/" + mY[1]);
                }
               
                var deliveries = db.Deliveries
                    .Where(d => d.DateDelivered >= startDate && d.DateDelivered < endDate).ToList();
                 
                foreach (var delivery in deliveries)
                {
                    var t = view.ZipCodes.Count;  // Extra zip code column is for totals
                    for (var j = 0; j < view.ZipCodes.Count; j++)
                    {
                        if (delivery.Zip == view.ZipCodes[j])
                        {
                            view.MonthlyCounts[mo, j, 0]++; view.MonthlyCounts[mo, t, 0]++;  // month, zip, # of families
                            var c = Convert.ToInt32(delivery.Children);
                            var a = Convert.ToInt32(delivery.Adults);
                            var s = Convert.ToInt32(delivery.Seniors);
                            view.MonthlyCounts[mo, j, 1] += c; view.MonthlyCounts[mo, t, 1] += c;
                            view.MonthlyCounts[mo, j, 2] += a; view.MonthlyCounts[mo, t, 2] += a;
                            view.MonthlyCounts[mo, j, 3] += s; view.MonthlyCounts[mo, t, 3] += s;
                            view.MonthlyCounts[mo, j, 4] += (a + c + s); view.MonthlyCounts[mo, t, 4] += (a + c + s);  // # of residents
                            var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                            view.MonthlyCounts[mo, j, 5] += lbs; view.MonthlyCounts[mo, t, 5] += lbs;  // pounds of food
                        }
                    }
                }
            }
            return view;
        }

        public ActionResult CountyReportToExcel(int yy, int qtr)
        {
            var view = GetCountyReportView(yy, qtr);
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue("Bethesda Help, Inc.");
            activeRow ++;
            ws.Cell(activeRow, 1).SetValue(view.DateRangeTitle);
            activeRow += 2;
            for(int mo = 0; mo < 3; mo++)
            {
                ws.Cell(activeRow, 1).SetValue(view.MonthYear[mo]);
                ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("TOTAL");
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("Zip Code");
                for (int i = 0; i < view.ZipCodes.Count; i++)
                {
                    ws.Cell(activeRow, i +2).SetValue(view.ZipCodes[i]); ;
                }
                ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("All Zip Codes");
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Families");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.MonthlyCounts[view.Months[mo], i, 0]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Children (&#60;18)");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.MonthlyCounts[view.Months[mo], i, 1]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Adults(&#62;=18 and &#60;60");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.MonthlyCounts[view.Months[mo], i, 2]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Seniors (&#62;=60)");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.MonthlyCounts[view.Months[mo], i, 3]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Residents");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.MonthlyCounts[view.Months[mo], i, 4]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Pounds of Food");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.MonthlyCounts[view.Months[mo], i, 5]);
                }
                activeRow += 2;
            }
            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = view.ReportTitle +".xlsx" };
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ReturnToReportsMenu()
        {
            return RedirectToAction("ReportsMenu");
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
