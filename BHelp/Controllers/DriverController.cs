using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    [Authorize]
    public class DriverController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Driver
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult Index(DateTime? logDate, string userId)

        {
            if (!logDate.HasValue)
            {
                var cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
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
           
            if (String.IsNullOrEmpty(userId)) { System.Web.HttpContext.Current.User.Identity.GetUserId(); }
            var deliveryList = new List<Delivery>(db.Deliveries).Where(d => d.Status == 0)
                .OrderBy(d => d.DateDelivered).ThenBy(z => z.Zip)
                .ThenBy(s => s.StreetNumber)
                .ThenBy(n => n.StreetName).ToList();  // changed 11/21/2022
            foreach (var delivery in deliveryList)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.City = client.City;
                    delivery.ClientNameAddress = client.LastName + ", " + client.FirstName
                       + " " + client.StreetNumber + " " + client.StreetName;
                    delivery.Notes = client.Notes;
                    delivery.Phone = client.Phone;  
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
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }

            var delivery = db.Deliveries.Find(id);
            if (delivery == null) return RedirectToAction("Index");
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
            switch (delivery.Status)
            {
                case 0:
                    delivery.SelectedStatus = "Open";
                    break;
                case 1:
                    delivery.SelectedStatus = "Delivered";
                    break;
                case 2:
                    delivery.SelectedStatus = "Undelivered";
                    break;
            }
            return View(delivery);
        }

        // POST: Driver/Edit
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FullBags,HalfBags,KidSnacks, " 
                     + "GiftCards,HolidayGiftCards,DateDelivered, "
                     + "DriverNotes,DriverId,SelectedStatus")] Delivery delivery)
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
                    del.HolidayGiftCards = delivery.HolidayGiftCards;

                    // Only Administrators may see and edit HolidayGiftCards outside of NOV-DEC  12-30-2023
                    if (delivery.DateDelivered != null)
                    {
                        var delMonth = delivery.DateDelivered.Value.Month;
                        if (delMonth != 11 && delMonth != 12 && !User.IsInRole("Administrator")) 
                        {
                            del.HolidayGiftCards = 0;
                        }
                    }

                    del.DriverNotes = delivery.DriverNotes;
                    del.DateDelivered = delivery.DateDelivered;
                    switch (delivery.SelectedStatus)
                    {
                        case "Open":
                            del.Status = 0;
                            break;
                        case "Delivered":
                            del.Status = 1;
                            break;
                        case "Undelivered":
                            del.Status = 2;
                            break;
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(delivery);
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult ExcelOpenDeliveries()
        {
            var result = AppRoutines.OpenDeliveriesToExcel(null);
            return result;
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult OpenDeliveriesToCSV()
        {
            var result = AppRoutines.OpenDeliveriesToCSV(null);
            return result;
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult OpenFilters()
        {
            return RedirectToAction("OpenFilters", "Deliveries",new{btnAllCheckAll="True"});
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
