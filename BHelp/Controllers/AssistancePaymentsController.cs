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
                var newRec = new AssistancePayment()
                {
                    Date = assistancePayment.Date,
                    ClientId = assistancePayment.ClientId, 
                    Action = assistancePayment.Action,
                    AmountInCents = (int)(assistancePayment.AmountDecimal * 100),
                    Note = assistancePayment.Note 
                };

                db.AssistancePayments.Add(newRec);
                db.SaveChanges();
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
            if (assistancePayment == null) return HttpNotFound();
            var view = new AssistanceViewModel()
            {
                ClientSelectList = new List<SelectListItem>(),
                ClientId = assistancePayment .ClientId,
                Date =assistancePayment .Date,
                Action = assistancePayment.Action,
                Note =assistancePayment.Note
            };
           
            //foreach (var cli in db.Clients.OrderBy(n => n.LastName)
            //             .ThenBy(f => f.FirstName).ToList())
            //{
            //    var item = new SelectListItem()
            //    {
            //        Value = cli.Id.ToString(),
            //        Text = cli.LastName + ", " + cli.FirstName
            //    };
            //    if (cli.Id == assistancePayment.ClientId)
            //    {
            //        item.Selected = true;
            //        view.SelectedClientId = cli.Id.ToString();
            //    }
            //    else
            //    {
            //        item.Selected = false;
            //    }
            //    view.ClientSelectList.Add(item);
            //}

            List<SelectListItem> clientList = (from c in db.Clients
                    .OrderBy(n => n.LastName)
                    .ThenBy(f => f.FirstName).AsEnumerable()
                select new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.LastFirstName
                }).ToList();
            clientList.Find(c => c.Value == assistancePayment.ClientId.ToString()).Selected = true;
            view.ClientSelectList = clientList;

            var amt = Convert.ToSingle(assistancePayment.AmountInCents);
            view.AmountDecimal = (decimal)(amt / 100);

            return View(view);
        }

        // POST: AssistancePayments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClientId,Date,Action,AmountDecimal,Note")] 
            AssistanceViewModel assistancePayment)
        {
            if (ModelState.IsValid)
            {
                var aRec = db.AssistancePayments.Single(p =>p.Id  == assistancePayment.Id);
                if (aRec != null)
                {
                    aRec.Date = assistancePayment.Date;
                    aRec.ClientId = assistancePayment.ClientId;
                    aRec.Action = assistancePayment.Action;
                    aRec.AmountInCents = (int)(assistancePayment.AmountDecimal * 100);
                    aRec.Note = assistancePayment.Note;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            var view = new AssistanceViewModel();
            return View(view);
        }

        // GET: AssistancePayments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssistancePayment assistancePayment = db.AssistancePayments.Find(id);
            if (assistancePayment == null) return HttpNotFound();
            var view = new AssistanceViewModel()
            {
                Date = assistancePayment.Date,
                Action = assistancePayment.Action,
                AmountInCents =assistancePayment.AmountInCents, // auto fills StringDollarAmount
                Note = assistancePayment.Note
            };
            var client = db.Clients.Find(assistancePayment.ClientId);
            if (client == null) return View(view);
            view.LastName = client.LastName;
            view.FirstName = client.FirstName;

            return View(view);
        }

        // POST: AssistancePayments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var assistancePayment = db.AssistancePayments.Find(id);
            if (assistancePayment != null) db.AssistancePayments.Remove(assistancePayment);
            db.SaveChanges();
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
