using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class DeliveriesController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Deliveries
        public ActionResult Index()
        {
            var listDeliveries = new List<Delivery>(db.Deliveries).Where(d => d.DateDelivered == null).OrderBy(d => d.DeliveryDate).ToList();
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
                        DeliveryDate = Convert.ToDateTime(Session["CallLogDate"]),
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
                        if (driver != null) deliveryView.DriverName = driver.FullName;
                    }
                    else
                    {
                        deliveryView.DriverName = "(nobody yet)";  
                    }

                    var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    if (userIid != null)
                    {
                        var user = db.Users.Find(userIid);
                        deliveryView.User = user;
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
                DriverNotes = delivery.DriverNotes,
                FamilyMembers = AppRoutines.GetFamilyMembers(delivery.ClientId),
                FamilySelectList = AppRoutines.GetFamilySelectList(delivery.ClientId)
            };
  
            if (delivery.DateDelivered != null) viewModel.DateDelivered = (DateTime) delivery.DateDelivered;


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

            return View(viewModel);
        }

        // POST: Deliveries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id,ClientId,DeliveryDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards," +
                            "DateDelivered,ODNotes,DriverNotes,GiftCardsEligible")]
            DeliveryViewModel delivery)
        {
            if (ModelState.IsValid)
            {
                var updateData  = db.Deliveries.Find(delivery.Id);

                if (updateData != null)
                {
                    updateData.DeliveryDate = delivery.DeliveryDate;
                    updateData.FullBags = delivery.FullBags;
                    updateData.HalfBags = delivery.HalfBags;
                    updateData.KidSnacks = delivery.KidSnacks;
                    updateData.GiftCards = delivery.GiftCards;
                    updateData.ODNotes = delivery.ODNotes;
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

        public ActionResult CountyReport()
        {
            // Default to this year, this quarter
            var view = new ReportsViewModel
                {Year = Convert.ToInt32(DateTime.Now.Year.ToString())};
            var month = Convert.ToInt32(DateTime.Now.Month.ToString());
            if (month >= 1 && month <= 3) { view.Quarter = 1; }
            if (month >= 4 && month <= 6) { view.Quarter = 2; }
            if (month >= 7 && month <= 9) { view.Quarter = 3; }
            if (month >= 10 && month <= 12) { view.Quarter = 4; }

            if (DateTimeFormatInfo.CurrentInfo != null)
            {
                view.MonthYear = new string[3];
                view.MonthYear[0] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(1 + (3 * (view.Quarter - 1)))
                    + " " + view.Year.ToString();
                view.MonthYear[1] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(2 + (3 * (view.Quarter - 1)))
                    + " " + view.Year.ToString();
                view.MonthYear[2] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(3 + (3 * (view.Quarter - 1)))
                    + " " + view.Year.ToString();
                view.DateRangeTitle = view.MonthYear[0] + " through " + view.MonthYear[2];
            }
            view.ZipCodes = AppRoutines.GetZipCodesList();
            // Load MonthlyCounts
            view.MonthlyCounts=new int [13,view.ZipCodes.Count,6]; //Month, ZipsCodes, Counts
            for (int i = 0; i < 3; i++)
            {
                var mY = view.MonthYear[i].Split(' ');
                var mo = DateTime.ParseExact(mY[0], "MMMM", CultureInfo.CurrentCulture).Month;
                var deliveries = db.Deliveries
                    .Where(d =>d.DeliveryDate.Year == view.Year && d.DeliveryDate.Month == mo)
                        .Join(db.Clients, del => del.ClientId, cli => cli.Id,
                                     (del, cli) => new
                                     {
                                         zip = cli.Zip,
                                         children = del.Children, adults = del.Adults, seniors = del.Seniors, 
                                         fullBags = del.FullBags, halfBags = del.HalfBags, kidSnacks=del.KidSnacks
                                     }
                                 ).ToList();

                foreach (var delivery in deliveries)
                {
                    for (var j = 0; j < view.ZipCodes.Count; j++)
                    {
                        if (delivery.zip == view.ZipCodes[j])
                        {
                            view.MonthlyCounts[mo, j, 1] ++; // month, zip, # of families
                            var c = Convert.ToInt32(delivery.children);
                            var a = Convert.ToInt32(delivery.adults);
                            var s = Convert.ToInt32(delivery.seniors);
                            view.MonthlyCounts[mo, j, 2] += c;
                            view.MonthlyCounts[mo, j, 3] += a;
                            view.MonthlyCounts[mo, j, 4] += s;
                            view.MonthlyCounts[mo, j, 5] += (a + c + s);  // # of residents
                            view.MonthlyCounts[mo, j, 6] = 0;  // pounds of food
                        }
                    }
                }
                var x = deliveries[0];
                //{
                //    // Switch by Zip Code
                //    //if(delivery.Z)
                //}
            }
            return View(view);
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

