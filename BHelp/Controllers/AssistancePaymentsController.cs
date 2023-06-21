using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class AssistancePaymentsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: AssistancePayments
        public ActionResult Index(string searchString, int? selectedId)
        {
            //if (searchString != null)
            //{ TempData["SearchResults"] = SearchHouseholds(searchString); }
            //var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            //if (userIid != null)
            //{
            //    var user = db.Users.Find(userIid);
            //    Session["CurrentUserFullName"] = user.FullName;Client
            //};
            var paymentView = new AssistanceViewModel()
            {
                ClientLookupList = db.Clients.ToList(),
                PaymentList = db.AssistancePayments.OrderByDescending(d => d.Date).ToList()
            };

            foreach (var pymnt in paymentView.PaymentList)
            {
                // Add client Name to payment list
                var cli = paymentView.ClientLookupList
                    .FirstOrDefault(i => i.Id == pymnt.ClientId);
                if (cli == null) continue;
                pymnt.LastName = cli.LastName;
                pymnt.FirstName = cli.FirstName;
            }

            //return View(houseHoldView);
            return View(paymentView);
        }

        // GET: AssistancePayments/Create
        public ActionResult Create()
        {
            var view = new AssistanceViewModel
            {
                ClientSelectList =new List<SelectListItem>(),
                Date = DateTime.Today
            };
            
            foreach (var cli in db.Clients.OrderBy(n => n.LastName)
                         .ThenBy(f => f.FirstName).ToList())
            {
                view.ClientSelectList.Add(new SelectListItem
                {
                    Value = cli.Id.ToString(),
                    Text = cli.LastName + ", " + cli.FirstName
                });
            }

            return View(view);
        }

        // POST: AssistancePayments/Create.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,ClientId,Action,AmountDecimal,Note")]
            AssistanceViewModel assistancePayment)
        {
            if (ModelState.IsValid)
            {
                //db.AssistancePayments.Add(assistancePayment);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View();
        }

        // GET: AssistancePayments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssistancePayment assistancePayment = db.AssistancePayments.Find(id);
            if (assistancePayment == null)
            {
                return HttpNotFound();
            }
            return View(assistancePayment);
        }

        // POST: AssistancePayments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClientId,Date,Action,AmountInCents,Note")] AssistancePayment assistancePayment)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(assistancePayment).State = EntityState.Modified;
                //db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(assistancePayment);
        }

        // GET: AssistancePayments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssistancePayment assistancePayment = db.AssistancePayments.Find(id);
            if (assistancePayment == null)
            {
                return HttpNotFound();
            }
            return View(assistancePayment);
        }

        // POST: AssistancePayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //var assistancePayment = db.AssistancePayments.Find(id);
            //if (assistancePayment != null) db.AssistancePayments.Remove(assistancePayment);
            //db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult CreateNewClient()
        {
            return RedirectToAction("Create","Clients");
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
