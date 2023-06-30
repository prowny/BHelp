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
        public ActionResult SearchClient(string searchString, int? selectedId)
        {
            if (searchString != null)
            {
                TempData["SearchResults"] = AppRoutines.SearchClients(searchString);
            }

            var paymentView = new AssistanceViewModel();
            return View(paymentView);
        }

        // GET: AssistancePayments
        public ActionResult Index()
        {
            var payments = db.AssistancePayments
                .OrderByDescending(d => d.Date).ToList();
            var clientList = AppRoutines.GetAllClientsList();
            var categoryList = AppRoutines.GetAssistanceCategoriesList();
            foreach (var pymt in payments)
            {
                var rec = clientList.SingleOrDefault(obj => obj.Id == pymt.ClientId);
                if (rec != null) pymt.FullName = rec.FirstName + " " + rec.LastName;

                if (pymt.Note != null)
                {
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    pymt.NoteToolTip = pymt.Note.Replace(" ", "\u00a0");
                    var s = pymt.Note; // For display, abbreviate to 12 characters:           
                    s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                    pymt.Note = s;
                }

                pymt.DateString = pymt.Date.ToString("MM/dd/yyyy");

                var c = Convert.ToByte(pymt.Category);
                pymt.ActionCategory = categoryList[c- 1];
            }

            var paymentsView = new AssistanceViewModel
            {
                PaymentList = payments
            };

            return View(paymentsView);
        }

        // GET: AssistancePayments/Create
        public ActionResult Create(int? clientId)
        {
            //var cats = AppRoutines.GetAssistanceCategoriesList();
            //var selCats = AppRoutines.GetAssistanceCategoriesSelectList();
            var view = new AssistanceViewModel
            {
                ClientSelectList =new List<SelectListItem>(),
                Date = DateTime.Today,
                AssistanceCategoriesSelectList= AppRoutines.GetAssistanceCategoriesSelectList()
            };
            
            foreach (var cli in db.Clients.OrderBy(n => n.LastName)
                         .ThenBy(f => f.FirstName).ToList())
            {
                if (clientId != null && clientId == cli.Id)
                {
                    view.ClientSelectList.Add(new SelectListItem
                    {
                        Value = cli.Id.ToString(),
                        Text = cli.LastName + ", " + cli.FirstName,
                        Selected = true
                    });
                }
                else
                {
                    view.ClientSelectList.Add(new SelectListItem
                    {
                    Value = cli.Id.ToString(), Text = cli.LastName + ", " + cli.FirstName,
                    Selected = false
                    });
                }
            }

            return View(view);
        }

        // POST: AssistancePayments/Create.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Date,ClientId,Action,CategoryId,Payee,AmountDecimal,Note")]
            AssistanceViewModel assistancePayment)
        {
            if (ModelState.IsValid)
            {
                var newRec = new AssistancePayment()
                {
                    Date = assistancePayment.Date,
                    ClientId = assistancePayment.ClientId,
                    Category = Convert .ToByte(assistancePayment .CategoryId),
                    Action = assistancePayment.Action,
                    Payee =assistancePayment.Payee,
                    AmountInCents = (int)(assistancePayment.AmountDecimal * 100),
                    Note = assistancePayment.Note 
                };

                db.AssistancePayments.Add(newRec);
                db.SaveChanges();
                return RedirectToAction("SearchClient");
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

            var clientList = (from c in db.Clients
                    .OrderBy(n => n.LastName)
                    .ThenBy(f => f.FirstName).AsEnumerable()
                select new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.LastFirstName
                }).ToList();
            clientList.Find(c => c.Value == assistancePayment.ClientId.ToString())
                .Selected = true;
            
            var actionList = AppRoutines.GetAssistanceCategoriesSelectList();
            actionList.Find(c => c.Value == assistancePayment.Category.ToString())
                .Selected = true;

            var amt = Convert.ToSingle(assistancePayment.AmountInCents);
           
            var view = new AssistanceViewModel()
            {
                ClientSelectList = clientList,
                ClientId = assistancePayment.ClientId,
                Date = assistancePayment .Date,
                Action = assistancePayment.Action,
                CategoryId =assistancePayment.Category,
                AssistanceCategoriesSelectList = actionList,
                AmountDecimal = (decimal)(amt / 100),
                Payee = assistancePayment.Payee, 
                Note =assistancePayment.Note
            };

            return View(view);
        }

        // POST: AssistancePayments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ClientId,Date,CategoryId,Action,Payee,AmountDecimal,Note")] 
            AssistanceViewModel assistancePayment)
        {
            if (ModelState.IsValid)
            {
                var aRec = db.AssistancePayments.Single(p =>p.Id  == assistancePayment.Id);
                if (aRec != null)
                {
                    aRec.Date = assistancePayment.Date;
                    aRec.ClientId = assistancePayment.ClientId;
                    aRec.Category = Convert.ToByte( assistancePayment.CategoryId);
                    aRec.Action = assistancePayment.Action;
                    aRec.Payee = assistancePayment.Payee;
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
