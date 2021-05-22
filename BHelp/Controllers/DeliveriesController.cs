using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            var listDeliveryViewModels=new List<DeliveryViewModel>();
            foreach (var delivery in listDeliveries)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    var deliveryView = new DeliveryViewModel();
                    deliveryView.ClientId = client.Id;
                    deliveryView.Client = client;
                    deliveryView.DeliveryDate = Convert.ToDateTime(Session["CallLogDate"]);
                    deliveryView.FamilyMembers = GetFamilyMembers(client.Id);
                    deliveryView.FamilySelectList = GetFamilySelectList(client.Id);
                    deliveryView.Kids = new List<FamilyMember>();
                    deliveryView.Adults = new List<FamilyMember>();
                    deliveryView.Seniors = new List<FamilyMember>();
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
                    deliveryView.FullBags = 0;
                    deliveryView.HalfBags = 0;
                    deliveryView.KIdSnacks = 0;
                    deliveryView.GiftCards = 0;
                    deliveryView.GiftCardsEligible = 0;     // !!! calculate this value
                    deliveryView.DateLastDelivery = DateTime.Today.AddDays(-7); // !!! calculate this value
                    deliveryView.DateLastGiftCard = DateTime.Today.AddDays(-7); // !!! calculate this value
                    deliveryView.DriverName = "";    // !!! calculate this value   
                    var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    if (userIid != null)
                    {
                        var user = db.Users.Find(userIid);
                        deliveryView.User = user;
                    };
                    listDeliveryViewModels.Add(deliveryView);
                }
            }
            return View(listDeliveryViewModels);
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

        private static List<SelectListItem> GetFamilySelectList(int clientId)
        {
            List<SelectListItem> householdList = new List<SelectListItem>();
            using (var db = new BHelpContext())
            {
                var client = db.Clients.Find(clientId);
                if (client != null)
                {
                    client.FamilyMembers = GetFamilyMembers(clientId);
                    foreach (var mbr in client.FamilyMembers)
                    {
                        var text = mbr.FirstName + " " + mbr.LastName + "/" +
                                   AppRoutines.GetAge(mbr.DateOfBirth, DateTime.Today);
                        var selListItem = new SelectListItem() {Value = mbr.FirstName, Text = text};
                        householdList.Add(selListItem);
                    }
                }
                return (householdList);
            }
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
            if (delivery != null) db.Deliveries.Remove(delivery);
            db.SaveChanges();
            return RedirectToAction("Index");
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
