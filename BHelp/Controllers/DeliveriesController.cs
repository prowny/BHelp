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
        private readonly BHelpContext db = new BHelpContext();

        // GET: Deliveries
        public ActionResult Index()
        {
            var listDeliveries = new List<Delivery>(db.Deliveries).Where(d =>  d.Completed == false)
                .OrderBy(z => z.Zip).ToList();
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
                        DriverId = delivery.DriverId,
                        LogDate =delivery.LogDate,
                        NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(client.Id),
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
                    if (delivery.GiftCardsEligible != null) deliveryView.GiftCardsEligible = (int) delivery.GiftCardsEligible;
                    deliveryView.DateLastDelivery = AppRoutines.GetLastDeliveryDate(client.Id);
                    deliveryView.DateLastGiftCard = AppRoutines.GetDateLastGiftCard(client.Id);
                    DateTime since1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    deliveryView.GiftCardsThisMonth = GetGiftCardsSince(client.Id, since1);
                    int hhTotal = deliveryView.KidsCount + deliveryView.AdultsCount + deliveryView.SeniorsCount;
                    deliveryView.GiftCardsEligible = GetGiftCardsEligible(client.Id, hhTotal);
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
            [Bind(Include = "Id,ClientId,LogDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards")]
            Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                Client client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.FirstName = client.FirstName;
                    delivery.LastName = client.LastName;
                    delivery.StreetNumber = client.StreetNumber;
                    delivery.StreetName = delivery.StreetName;
                    delivery.NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(client.Id);
                    delivery.Zip = client.Zip;

                    db.Deliveries.Add(delivery);
                    db.SaveChanges();
                    return View(delivery);
                }
            }
            return RedirectToAction("Index");
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
                LogDate = Convert.ToDateTime(delivery.LogDate.ToString("MM/dd/yyyy")),
               
                ODNotes = delivery.ODNotes,
                DriverId = delivery.DriverId,
                DriverNotes = delivery.DriverNotes,
                FamilyMembers = AppRoutines.GetFamilyMembers(delivery.ClientId),
                FamilySelectList = AppRoutines.GetFamilySelectList(delivery.ClientId)
            };

            viewModel.DateLastDelivery = GetLastDeliveryDate(delivery.Id);

            viewModel.DateDelivered = delivery.DateDelivered;

            viewModel.DriversList = AppRoutines.GetDriversSelectList();
            foreach (var item in viewModel.DriversList)
            {
                if (item.Value == viewModel.DriverId)
                {
                    item.Selected = true;
                    break;
                }
            }

            if (delivery.Children != null) viewModel.KidsCount = (int) delivery.Children;
            if (delivery.Adults != null) viewModel.AdultsCount = (int) delivery.Adults;
            if (delivery.Seniors != null) viewModel.SeniorsCount = (int) delivery.Seniors;
            var client = db.Clients.Find(delivery.ClientId);
            if (client != null)
            {
                viewModel.Client = client;
                viewModel.ClientNameAddress = client.LastName + ", " + client.FirstName
                                              + " " + client.StreetNumber + " " + client.StreetName + " " + client.Zip;
                viewModel.Notes = client.Notes;
                viewModel.DateLastDelivery = AppRoutines.GetLastDeliveryDate(client.Id);
                viewModel.DateLastGiftCard = AppRoutines.GetDateLastGiftCard(client.Id);
                int? hhTotal = delivery.Children + delivery.Adults + delivery.Seniors;
                viewModel.GiftCardsEligible = GetGiftCardsEligible(client.Id, hhTotal);
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
            [Bind(Include = "Id,ClientId,LogDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards," +
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
                    updateData.LogDate = delivery.LogDate;
                    updateData.FullBags = delivery.FullBags;
                    updateData.HalfBags = delivery.HalfBags;
                    updateData.KidSnacks = delivery.KidSnacks;
                    updateData.GiftCards = delivery.GiftCards;
                    updateData.GiftCardsEligible = delivery.GiftCardsEligible;
                    updateData.ODNotes = delivery.ODNotes;
                    updateData.DriverId = delivery.DriverId;
                    updateData.DriverNotes = delivery.DriverNotes;
                    if (delivery.DateDelivered != null)
                    {
                        updateData.DateDelivered = (DateTime) delivery.DateDelivered;
                        updateData.Completed = true;
                    }
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

        public ActionResult CallLogMenu()
        {
            return View();
        }

        public ActionResult CallLogIndividual(int? clientId)
        {
            List<SelectListItem> clientSelectList = new List<SelectListItem>();
            var clientList = db.Clients.OrderBy(c => c.LastName).ToList();
            foreach (var client in clientList)
            {
                if (db.Deliveries.Any(d => d.ClientId == client.Id))
                { 
                    var text = client.LastName + ", " + client.FirstName + " ";
                text += client.StreetNumber + " " + client.StreetName;
                var selListItem = new SelectListItem() { Value = client.Id.ToString(), Text = text };
                if (clientId == client.Id)
                {
                    selListItem.Selected = true;
                }
                clientSelectList.Add(selListItem);
                }
            }
            var callLogView = new DeliveryViewModel { ClientSelectList = clientSelectList };
            if (clientId != null)
            {
                var deliveryList = db.Deliveries.Where(d => d.ClientId == clientId)
                    .OrderByDescending(d => d.LogDate).ToList();
                callLogView.DeliveryList = deliveryList;
            }
            return View(callLogView);
        }
         
        [HttpPost]
        public ActionResult CallLogIndividual(string id)
        {
            var intClientId = Convert.ToInt32(id);
            return RedirectToAction("CallLogIndividual", new { clientId = intClientId });
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
            // Load Counts - extra zip code is for totals column.
            view.Counts = new int[13, view.ZipCodes.Count + 1, 6]; //Month, ZipCodes, Counts
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
                            view.Counts[mo, j, 0]++; view.Counts[mo, t, 0]++;  // month, zip, # of families
                            var c = Convert.ToInt32(delivery.Children);
                            var a = Convert.ToInt32(delivery.Adults);
                            var s = Convert.ToInt32(delivery.Seniors);
                            view.Counts[mo, j, 1] += c; view.Counts[mo, t, 1] += c;
                            view.Counts[mo, j, 2] += a; view.Counts[mo, t, 2] += a;
                            view.Counts[mo, j, 3] += s; view.Counts[mo, t, 3] += s;
                            view.Counts[mo, j, 4] += (a + c + s); view.Counts[mo, t, 4] += (a + c + s);  // # of residents
                            var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                            view.Counts[mo, j, 5] += lbs; view.Counts[mo, t, 5] += lbs;  // pounds of food
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
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 0]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Children (&#60;18)");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 1]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Adults(&#62;=18 and &#60;60");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 2]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Seniors (&#62;=60)");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 3]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Residents");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 4]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Pounds of Food");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 5]);
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

        private  DateTime? GetLastDeliveryDate(int id)
        {
            DateTime? dt = db.Deliveries.Where(d => d.DateDelivered != null
                                                    && d.Id == id && d.Completed)
                .OrderByDescending(x => x.DeliveryDate).Select(d => d.DateDelivered)
                .FirstOrDefault();

            // ReSharper disable once UseNullPropagation
            if (dt == null)
            {
                return null;
            }
            else
            {
                return (DateTime) dt;
            }
        }

        private int GetGiftCardsEligible(int id, int? hhTotal)
        {
            // GIFT CARDS ELIGIBLE:
            // 1 per week maximum
            // 1 per household of 3 or fewer
            // 2 per household of 4 or more
            // 3 max per calendar month;
            var eligible = 0;
            var totalThisWeek = GetGiftCardsSince(id, DateTime.Today.AddDays(-7));
            DateTime since1 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var totalThisMonth = GetGiftCardsSince(id, since1);
            var numberInHousehold = hhTotal;
            if (numberInHousehold < 4)   // 1 per household of 3 or fewer
            {
                eligible = 1;
                if (eligible + totalThisMonth > 3) eligible = 0;
            }
            if (numberInHousehold > 3)    // 2 per household of 4 or more
            {
                eligible = 3 - totalThisMonth;
                if (eligible > 2) eligible = 2;
            }
            if (totalThisWeek > 0) eligible = 0;   // 1 per week maximum
            return eligible;
         }

         private int GetGiftCardsSince(int id, DateTime dt)
        {
            var total = 0;
            var dList = db.Deliveries.Where(d => d.Id == id && d.Completed
                                     && d.DeliveryDate >= dt).Select(g => g.GiftCards).ToList();
            foreach(var i in dList)
            {
                if (i != null)
                {
                    var gc = (int) i;
                    total += gc;
                }
            }
            return total;
        }
        public ActionResult QuorkReport(string endingDate = "")
        {
            DateTime endDate;
            if (endingDate.IsNullOrEmpty())
            {
                // Ends on a Saturday - weekday Monday is 1, Saturday is 6
                // If today is a  Saturday, default to this week
                int weekDay = Convert.ToInt32(DateTime.Today.DayOfWeek);
                if (weekDay >= 6) // Default to this this Saturday, else Saturday last week
                { endDate = DateTime.Today.AddDays(6 - weekDay); }
                else
                {
                    DateTime lastSaturday = DateTime.Now.AddDays(-1);
                    while (lastSaturday.DayOfWeek != DayOfWeek.Saturday) lastSaturday = lastSaturday.AddDays(-1);
                    endDate = lastSaturday;
                }
            }
            else
            {
                endDate =Convert.ToDateTime(endingDate);
            }

            var view = GetQuorkReportView(endDate);
            return View(view);
        }

        private ReportsViewModel GetQuorkReportView(DateTime endDate)
        {
            DateTime startDate = endDate.AddDays(-6); 
            var view = new ReportsViewModel()
            {
                BeginDate = startDate,
                EndDate = endDate
            };
            view.EndDateString = view.EndDate.ToString("M-d-yy");
            view.DateRangeTitle= startDate.ToShortDateString() + " - " + endDate.ToShortDateString();
            view.ReportTitle = view.EndDateString + " QORK Weekly Report";
            
            view.ZipCodes = AppRoutines.GetZipCodesList();
            // Load Counts - extra zip code is for totals column.
            view.Counts = new int[1, view.ZipCodes.Count + 1, 8]; // 0 (unused), ZipCodes, Counts
            var deliveries = db.Deliveries
                .Where(d => d.DateDelivered >= startDate && d.DateDelivered < endDate).ToList();

            foreach (var delivery in deliveries)
            {
                var zipCount = view.ZipCodes.Count;  // Extra zip code column is for totals
                for (var j = 0; j < view.ZipCodes.Count; j++)
                {
                    if (delivery.Zip == view.ZipCodes[j])
                    {
                        var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                        view.Counts[0, j, 0] += lbs; view.Counts[0, zipCount, 0] += lbs;   //pounds of food
                        view.Counts[0, j, 1] ++; view.Counts[0, zipCount, 1] ++; //# unique households served
                        var c = Convert.ToInt32(delivery.Children);
                        var a = Convert.ToInt32(delivery.Adults);   
                        var s = Convert.ToInt32(delivery.Seniors);  
                        view.Counts[0, j, 2] += (a + c + s); view.Counts[0, zipCount, 2] += (a + c + s);    //# residents served
                        view.Counts[0, j, 3] += c; view.Counts[0, zipCount, 3] += c; //# children
                        view.Counts[0, j, 4] += s; view.Counts[0, zipCount, 4] += s;  //# seniors
                        view.Counts[0, j, 5] = 0; view.Counts[0, zipCount, 5]= 0;  //#staff worked  ZERO!!!
                        view.Counts[0, j, 6] = 0; view.Counts[0, zipCount, 6] = 0;   //# staff hours   ZERO!!!
                        view.Counts[0, j, 7]++; view.Counts[0, zipCount, 7]++;  //# deliveries
                    }
                }
            }
            return view;
        }

        public ActionResult QuorkReportToExcel(string endingDate)
        {
            var endDate = Convert.ToDateTime(endingDate);
            var view = GetQuorkReportView(endDate);
            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);

            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue("Bethesda Help, Inc. Quork Reportr");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Time Period");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue(view.DateRangeTitle);
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Zip Codes");
            for (int i = 0; i < view.ZipCodes.Count; i++)
            {
                ws.Cell(activeRow, i + 2).SetValue(view.ZipCodes[i]);
            }
            ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("Total Zip Codes");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Total Food Lbs)");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 0]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# HH Served (No Repeat Clients in Time Period)");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0,i,1]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Residents Served");   
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 2]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Residents <18");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 3]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Residents >60");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 4]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Staff Worked");
            activeRow++;  
            ws.Cell(activeRow, 1).SetValue("/# Staff Hours");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Deliveries");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 7]); }

            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = view.ReportTitle + ".xlsx" };
        }
        public ActionResult SaturdayNext(DateTime saturday)
        {
            saturday = saturday.AddDays(7);
            return RedirectToAction("QuorkReport", new { endingDate = saturday.ToShortDateString() });
        }
        public ActionResult SaturdayPrevious(DateTime saturday)
        {
            saturday = saturday.AddDays(-7);
            return RedirectToAction("QuorkReport", new{endingDate = saturday.ToShortDateString()});
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
            { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
