using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
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
            var listDeliveries =new List<Delivery>(db.Deliveries.Where(d => d.DateDelivered == null)).ToList();
           
                var deliveriesView = new List<DeliveryViewModel>();
                foreach (var delivery in listDeliveries)
                {
                    
                    var client = db.Clients.Find(delivery.ClientId);
                    if (client != null)
                    {
                        var deliveryView = new DeliveryViewModel();
                        deliveryView.ClientId = client.Id;
                        deliveryView.DeliveryDate = Convert.ToDateTime(Session["CallLogDate"]);
                        deliveryView.FamilyMembers = GetFamilyMembers(client.Id);
                        foreach (var mbr in deliveryView.FamilyMembers)
                        {
                            mbr.Age = AppRoutines.GetAge(DateTime.Today, mbr.DateOfBirth);
                            if (mbr.Age < 18) { deliveryView.Kids.Add(mbr); }
                            if (mbr.Age >= 18 && mbr.Age < 60) { deliveryView.Adults.Add(mbr); }
                            if(mbr.Age >= 60) { deliveryView.Seniors.Add(mbr);}
                        }
                        deliveryView.FullBags = 0;
                        deliveryView.HalfBags = 0;
                        deliveryView.KIdSnacks = 0;
                        deliveryView.GiftCards = 0;
                        deliveryView.GiftCardsEligible = 0;     // !!! calculate this value
                        deliveryView.DateLastDelivery = DateTime.Today.AddDays(-7); // !!! calculate this value
                        deliveryView.DateLastGiftCard = DateTime.Today.AddDays(-7); // !!! calculate this value
                        var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        if (userIid != null)
                        {
                            var user = db.Users.Find(userIid);
                            deliveryView.User = user;
                        };
                }
            }

                return View(listDeliveries);
            }
        private static List<FamilyMember> GetFamilyMembers(int clientId)
        {
            var familyMembers = new List<FamilyMember>();   // For editiing
            using (var db = new BHelpContext())
            {
                var client = db.Clients.Find(clientId);
                var sqlString = "SELECT * FROM FamilyMembers ";
                sqlString += "WHERE Active > 0 AND ClientId =" + clientId;
                var familyList = db.Database.SqlQuery<FamilyMember>(sqlString).ToList();
                if (client != null)
                {
                    FamilyMember headOfHousehold = new FamilyMember()
                    {
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        DateOfBirth = client.DateOfBirth,
                    };
                    familyList.Add(headOfHousehold);
                }

                foreach (FamilyMember member in familyList)
                {
                    member.Age = AppRoutines.GetAge(member.DateOfBirth, DateTime.Today);
                    member.NameAge = member.FirstName + " " + member.LastName + "/" + member.Age;
                    familyMembers.Add(member);
                }
            }
            return familyMembers;
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
            return View();
        }

        // POST: Deliveries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClientId,DeliveryDate,Notes,FullBags,HalfBags,KIdSnacks,GiftCards")] Delivery delivery)
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
            Delivery delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // POST: Deliveries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClientId,DeliveryDate,Notes,FullBags,HalfBags,KIdSnacks,GiftCards")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                db.Entry(delivery).State = EntityState.Modified;
                db.SaveChanges();
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
            db.Deliveries.Remove(delivery);
            db.SaveChanges();
            return RedirectToAction("Index");
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
