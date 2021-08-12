using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;
using Castle.Core.Internal;

namespace BHelp.Controllers
{
    public class DriverController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Driver
        public ActionResult Index(DateTime? logDate, string userId)

        {
            var logYear = DateTime.Today.Year;
            var logMonth = DateTime.Today.Month;
            var logDay = DateTime.Today.Day;
            //string cdts1 = "";
            //string cdts2 = "";
            if (!logDate.HasValue)
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
                logDate = cdt;
                logYear = cdt.Year;
                logMonth = cdt.Month;
                logDay = cdt.Day;
                //cdts1 = cdt.ToString("yyyy-MM-dd");
                //cdts2 = cdt.AddDays(1).ToString("yyyy-MM-dd");
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
           
            if (userId.IsNullOrEmpty()) { userId = System.Web.HttpContext.Current.User.Identity.GetUserId(); }
            // Get around Date and DateTime differences:
            var deliveryList = db.Deliveries.Where(d => d.LogDate.Year == logYear 
                         && d.LogDate.Month == logMonth && d.LogDate.Day == logDay
                         && d.DriverId == userId).OrderByDescending(z => z.Zip).ToList();

            foreach (var delivery in deliveryList)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.ClientNameAddress = client.LastName + ", " + client.FirstName
                       + " " + client.StreetNumber + " " + client.StreetName + " " + client.Zip;
                    delivery.Notes = client.Notes;
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
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // POST: Driver/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FullBags,HalfBags,KidSnacks,GiftCards,DateDelivered,Completed,DriverNotes")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                var del = db.Deliveries.Find(delivery.Id);
                if (del != null)
                {
                    del.FullBags = delivery.FullBags;
                    del.HalfBags = delivery.HalfBags;
                    del.KidSnacks = delivery.KidSnacks;
                    del.GiftCards = delivery.GiftCards;
                    del.Completed = delivery.Completed;
                    del.DriverNotes = delivery.DriverNotes;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(delivery);
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
