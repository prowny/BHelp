using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class AssistancePaymentsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: AssistancePayments
        public ActionResult Index()
        {
            return View(db.AssistancePayments.ToList());
        }

        // GET: AssistancePayments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AssistancePayments/Create.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ClientId,Action,AmountInCents,Note")] AssistancePayment assistancePayment)
        {
            if (ModelState.IsValid)
            {
                //db.AssistancePayments.Add(assistancePayment);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(assistancePayment);
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
        public ActionResult Edit([Bind(Include = "Id,ClientId,Action,AmountInCents,Note")] AssistancePayment assistancePayment)
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
