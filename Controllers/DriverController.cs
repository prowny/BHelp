using System;
using System.Collections.Generic;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class DriverController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Driver
        public ActionResult Index(DateTime? logDate, string userId)

        {
            if (!logDate.HasValue)
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
                logDate = cdt;
            }
            else
            {
                Session["CallLogDate"] = logDate.ToString();
            }

            var deliveryView = new DeliveryViewModel
            {
                LogDate = ((DateTime) logDate).Date,
                DeliveryList = new List<Delivery>()
            };
           
            if (userId.IsNullOrEmpty()) { System.Web.HttpContext.Current.User.Identity.GetUserId(); }
            // 09/09/2021: change to driver sees all open deliveries
            var deliveryList = new List<Delivery>(db.Deliveries).Where(d => d.Completed == false)
                .OrderBy(d => d.DeliveryDate).ThenBy(z => z.Zip)
                .ThenBy(n => n.LastName).ToList();
            foreach (var delivery in deliveryList)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.ClientNameAddress = client.LastName + ", " + client.FirstName
                       + " " + client.StreetNumber + " " + client.StreetName;
                    delivery.Notes = client.Notes;
                    var familyMembers = AppRoutines.GetFamilyMembers(client.Id);
                    delivery.HouseoldCount = familyMembers.Count;
                }

                if (delivery.DriverId != null)
                {
                    var driver = db.Users.Find(delivery.DriverId);
                    if (driver != null)  // Id could be "0"
                    {
                        delivery.DriverName = driver.FullName;
                    }
                    else
                    {
                        delivery.DriverName = "  - - -  ";
                    }
                }
                else
                {
                    delivery.DriverName = "  - - -  ";
                }
                deliveryView.DeliveryList.Add(delivery);
            }
            return View(deliveryView);
         }
        
        // GET: Driver/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliveries.Find(id);

            if (delivery != null)
            {
                delivery.DriversList = AppRoutines.GetDriversSelectList();

                foreach (var item in delivery.DriversList)
                {
                    if (item.Value == delivery.DriverId)
                    {
                        item.Selected = true;
                        break;
                    }
                }

                delivery.NumberOfKids2_17 = AppRoutines.GetNumberOfKids2_17(delivery.ClientId);
                return View(delivery);
            }
            return RedirectToAction("Index");
        }

        // POST: Driver/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FullBags,HalfBags,KidSnacks,GiftCards," +
                    "DateDelivered,Completed,DriverNotes,DeliveryDate,DriverId")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                var del = db.Deliveries.Find(delivery.Id);
                if (del != null)
                {
                    del.DriverId = delivery.DriverId;
                    del.FullBags = delivery.FullBags;
                    del.HalfBags = delivery.HalfBags;
                    del.KidSnacks = delivery.KidSnacks;
                    del.GiftCards = delivery.GiftCards;
                    del.Completed = delivery.Completed;
                    del.DriverNotes = delivery.DriverNotes;
                    del.DeliveryDate = delivery.DeliveryDate;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(delivery);
        }

        public ActionResult OpenDeliveriesToExcel()
        {
            var result = AppRoutines.OpenDeliveriesToExcel();
            return result;
        }

        private OpenDeliveryViewModel GetOpenDeliveryViewModel()
        {
            var odv = new OpenDeliveryViewModel
            {
                ReportTitle = "Bethesda Help Open Deliveries"
            };
            var deliveryList = new List<Delivery>(db.Deliveries).Where(d => d.Completed == false)
                .OrderBy(d => d.DeliveryDate)
                .ThenBy(d => d.DriverId)
                .ThenBy(z => z.Zip)
                .ThenBy(n => n.LastName).ToList();
            odv.OpenDeliveryCount = deliveryList.Count;
            odv.OpenDeliveries = new string[deliveryList.Count , 12];
            var i = 0;
            foreach (var del in deliveryList)
            {
                var client = db.Clients.Find(del.ClientId);
                odv.OpenDeliveries[i, 1] = del.DeliveryDate.ToShortDateString();

                var driver = db.Users.Find(del.DriverId);
                if (driver != null)
                {
                    odv.OpenDeliveries[i, 2] = driver.FullName;
                }
                odv.OpenDeliveries[i, 3] = del.Zip;
                odv.OpenDeliveries[i, 4] = del.LastName + ", " + del.FirstName;
                odv.OpenDeliveries[i, 5] = del.StreetNumber + " " + del.StreetName;
                odv.OpenDeliveries[i, 6] = del.City;
                odv.OpenDeliveries[i, 7] = del.Phone;
                if (client != null)
                {
                    var familyMemberCount = db.FamilyMembers.Count(c => c.ClientId == client.Id);
                    odv.OpenDeliveries[i, 8] = (familyMemberCount + 1).ToString();
                    odv.OpenDeliveries[i, 9] = client.Notes;
                }
                odv.OpenDeliveries[i, 10] = del.ODNotes;
                odv.OpenDeliveries[i, 11] = del.DriverNotes;
                i++;
            }
            return odv;
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
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
