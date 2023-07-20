using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.Models;
using BHelp.ViewModels;
using BHelp.DataAccessLayer;
using Microsoft.Ajax.Utilities;
using System.Text;

namespace BHelp.Controllers
{
    public class AssistancePaymentsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        //GET: AssistancePayments
        public ActionResult AssistancePaymentsMenu()
        {
            return View();
        }

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

        // GET: Payments by Individual
        public ActionResult PaymentsByIndividual(int? clientId)
        {  
             var clientLookupList = db.Clients.ToList();
             var rawList = new List<Client>(); // unsorted payment clients
             var view = new AssistanceViewModel
             {
                 ClientSelectList = new List<SelectListItem>(),
                 ReturnURL = "PaymentsByIndividual"
             };
             var clientIdList = db.AssistancePayments
                        .DistinctBy(i => i.ClientId).ToList();

            foreach (var pymnt in clientIdList) // get client data for each client id
            {
                var client = clientLookupList.FirstOrDefault(i => i.Id == pymnt.ClientId);
                rawList.Add(client);
            }

            rawList = new List<Client>(rawList.OrderBy(n => n.LastName)
                .ThenBy(f => f.FirstName)); // rewList now contains sorted clients

            foreach (var client in rawList) // now create selectlist items
            {
                var newItem = new SelectListItem
                {
                    Value = client.Id.ToString(),
                    Text = client.LastName + ", " + client.FirstName + "  " + client.StreetNumber + " " + client.StreetName
                    + " " + client.City 
                };
                view.ClientSelectList.Add(newItem);
            }
            if (clientId == null) return View(view);
                

            // clientId != null; load client's payment history:
            var _paymentList = db.AssistancePayments.Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.Date).ToList();

            var categoryList = AppRoutines.GetAssistanceCategoriesList();

            foreach (var pymt in _paymentList)
            {
                pymt.DateString = pymt.Date.ToString("MM/dd/yyyy");
                var c = Convert.ToByte(pymt.Category);
                pymt.ActionCategory = categoryList[c - 1];
                //pymt.StringDollarAmount = pymt.StringDollarAmount.Replace(".-", "."); // replace negative modulus
            }
            view.PaymentList = _paymentList;
            view.ClientId = (int)clientId;
            view.ReturnURL = "PaymentsByIndividual";

            return View(view);
        }

        // GET: AssistancePaymentsByDate
        public ActionResult PaymentsByDate(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue) // default to today and 1 week ago
            {
                startDate = DateTime.Today.AddDays(-7);
                endDate = DateTime.Today;
            }
            TempData["PaymentStartDate"] = startDate;
            TempData["PaymentEndDate"] = endDate;
            var payments = db.AssistancePayments
                .Where(d => d.Date >= startDate && d.Date <= endDate)
                .OrderByDescending(d => d.Date).ToList();
            
            var paymentView = new AssistanceViewModel()
            {
                PaymentList = payments,
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                ReturnURL ="PaymentsByDate"
            };

            var categoryList = AppRoutines.GetAssistanceCategoriesList();
            var clientLookupList = db.Clients.ToList();
            foreach (var pymnt in paymentView.PaymentList)
            {
                pymnt.DateString = pymnt.Date.ToString("MM/dd/yyyy");
                pymnt.ActionCategory = categoryList[pymnt.Category - 1];
                var client = clientLookupList.FirstOrDefault(i => i.Id == pymnt.ClientId);
                if (client != null)
                {
                    pymnt.LastName = client.LastName;
                    pymnt.FirstName = client.FirstName;
                    pymnt.AddressString = client.StreetNumber + " " + client.StreetName + " "
                        + client.City + " "  + client.Zip;
                    pymnt.AddressToolTip = (client.StreetNumber + " " + client.StreetName + " "
                                           + client.City + " " + client.Zip).Replace(" ", "\u00a0");

                    if (pymnt.Note != null)
                    {
                        pymnt.NoteToolTip = pymnt.Note.Replace(" ", "\u00a0");
                        var s = pymnt.Note; // For display, abbreviate to 20 characters:           
                        s = s.Length <= 20 ? s : s.Substring(0, 20) + "...";
                        pymnt.Note = s;
                    }

                    if (pymnt.Action != null)
                    {
                        pymnt.ActionToolTip = pymnt.Action.Replace(" ", "\u00a0");
                        var s = pymnt.Action; // For display, abbreviate to 20 characters:           
                        s = s.Length <= 20 ? s : s.Substring(0, 20) + "...";
                        pymnt.Action = s;
                    }

                    if (pymnt.AddressString != null)
                    {
                        var s = pymnt.AddressString;  // For display, abbreviate to 20 characters:       
                        s = s.Length <= 20 ? s : s.Substring(0, 20) + "...";
                        pymnt.AddressString = s;
                    }
                }
            }

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
                    PaymentList = payments,
                    ReturnURL = "PaymentsByIndividual"
                };

                return View(paymentsView);
            }

        // GET: AssistancePayments/Create
        public ActionResult Create(int? clientId, DateTime? startDate, DateTime? endDate   )
            {
                if (clientId != null) 
                { Session["AssistanceClientId"] = clientId; }
                else
                { clientId = (int)Session["AssistanceClientId"]; }
                var view = new AssistanceViewModel
                {
                    ClientId = (int)clientId,
                    Date = DateTime.Today,
                    AssistanceCategoriesSelectList= AppRoutines.GetAssistanceCategoriesSelectList()
                };
                
                var client = AppRoutines.GetClientRecord(view.ClientId);
                if (client != null)
                {
                    view.FullName = client.FirstName + " " + client.LastName;
                    view.StreetNumber = client.StreetNumber;
                    view.StreetName = client.StreetName;
                    var familyNembers = AppRoutines.GetFamilyMembers(client.Id);
                    
                    var mbrCount = 0;
                    foreach (var mbr in familyNembers)
                    {
                        if (mbrCount == 0)
                        {
                            view.HouseholdMembersString = view.FullName;
                            mbrCount = 1;
                        }
                        else
                        {
                            view.HouseholdMembersString += " / " + mbr.FirstName + " " + mbr.LastName;
                        }
                    }
                }

                // Load view PaymentData
                view.PaymentHistorySelectList = AppRoutines.GetPaymentHistorySelectList((int)clientId, view.StartDate, view.EndDate);
                view.PaymentData = AppRoutines.GetAssistancePaymentData((int)clientId, startDate, endDate);

                return View(view);
            }

            // POST: AssistancePayments/Create.
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create([Bind(Include = "Date,ClientId,Action,CategoryId,AmountDecimal,Note")]
                AssistanceViewModel assistancePayment)
            {
                if (ModelState.IsValid)
                {
                    var newRec = new AssistancePayment()
                    {
                        Date = assistancePayment.Date,
                        ClientId = assistancePayment.ClientId,
                        Category = Convert .ToByte(assistancePayment.CategoryId),
                        Action = assistancePayment.Action,
                        AmountDecimal = assistancePayment .AmountDecimal,
                        Note = assistancePayment.Note 
                    };

                    db.AssistancePayments.Add(newRec);
                    db.SaveChanges();
                    return RedirectToAction("SearchClient");
                }

                return View();
            }

            // GET: AssistancePayments/Edit/5
            public ActionResult Edit(int? id, string returnURL)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var assistancePayment = db.AssistancePayments.Find(id);
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
            
                var view = new AssistanceViewModel()
                {
                    ClientSelectList = clientList,
                    ClientId = assistancePayment.ClientId,
                    Date = assistancePayment .Date,
                    Action = assistancePayment.Action,
                    CategoryId =assistancePayment.Category,
                    AssistanceCategoriesSelectList = AppRoutines.GetAssistanceCategoriesSelectList(),
                    AmountDecimal = assistancePayment.AmountDecimal,
                    Note =assistancePayment.Note, 
                    ReturnURL = returnURL 
                };

                return View(view);
            }

            // POST: AssistancePayments/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit([Bind(Include = "Id,ClientId,Date,CategoryId,Action," +
                                                     "AmountDecimal,Note,btnDelete,ReturnURL")] 
                AssistanceViewModel assistancePayment)
            {
                if (ModelState.IsValid)
                {
                    if (assistancePayment.BtnDelete != null)
                    {
                        var pymntRec = db.AssistancePayments.Find(assistancePayment.Id);
                        if (pymntRec == null) return RedirectToAction("Edit", new { recId = assistancePayment.Id });
                        db.AssistancePayments.Remove(pymntRec);
                        db.SaveChanges();

                        if (assistancePayment.ReturnURL.Contains("Individual"))
                        {
                            return RedirectToAction("PaymentsByIndividual");

                        }
                        if (assistancePayment.ReturnURL.Contains("Date"))
                        {
                            return RedirectToAction("PaymentsByDate");
                        }

                        return RedirectToAction("Index");
                    }


                    var aRec = db.AssistancePayments.Single(p =>p.Id  == assistancePayment.Id);
                    if (aRec != null)
                    {
                        aRec.Date = assistancePayment.Date;
                        aRec.ClientId = assistancePayment.ClientId;
                        aRec.Category = Convert.ToByte( assistancePayment.CategoryId);
                        aRec.Action = assistancePayment.Action;
                        aRec.AmountDecimal = assistancePayment.AmountDecimal;
                        aRec.Note = assistancePayment.Note;
                        db.SaveChanges();
                    }

                    if (assistancePayment.ReturnURL == null) return RedirectToAction("PaymentsByIndividual");
                    
                    if (assistancePayment.ReturnURL.Contains("Individual"))
                    {
                        return RedirectToAction("PaymentsByIndividual");
                    }

                    if (assistancePayment.ReturnURL.Contains("Date"))
                    {
                        return RedirectToAction("PaymentsByDate");
                    }

                    return RedirectToAction("PaymentsByIndividual");
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

            public ActionResult PaymentsByIndividualToCSV()
            {
                return null;
            }

            public ActionResult PaymentsByDateToCSV()
            {
                var startDate = Convert.ToDateTime(TempData["PaymentStartDate"]);
                var endDate = Convert.ToDateTime(TempData["PaymentEndDate"]);
                var payments = db.AssistancePayments
                    .Where(d => d.Date >= startDate && d.Date <= endDate)
                    .OrderByDescending(d => d.Date).ToList();

                var categoryList = AppRoutines.GetAssistanceCategoriesList();
                var clientLookupList = db.Clients.ToList();
            
                foreach (var pymnt in payments)
                {
                    pymnt.DateString = pymnt.Date.ToString("MM/dd/yyyy");
                    pymnt.ActionCategory = categoryList[pymnt.Category - 1];
                    var client = clientLookupList.FirstOrDefault(i => i.Id == pymnt.ClientId);
                    if (client != null)
                    {
                        pymnt.LastName = client.LastName;
                        pymnt.FirstName = client.FirstName;
                        pymnt.AddressString = client.StreetNumber + " " + client.StreetName;
                        pymnt.ActionCategory = categoryList[pymnt.Category - 1];
                        pymnt.City = client.City;
                        pymnt.Zip = client.Zip;

                    }
                }

                float Total = 0;
                var sb = new StringBuilder();
            
                sb.Append("Payment Dates:" + ',' + ',' + startDate.ToShortDateString()
                          + " - " + endDate .ToShortDateString());
                sb.AppendLine();
                sb.Append("Last Name" + ',');
                sb.Append("First Name" + ',');
                sb.Append("Street Address" + ',');
                sb.Append("City" + ',');
                sb.Append("Zip Code" + ',');
                sb.Append("Category" + ',');
                sb.Append("Action" + ',');
                sb.Append("Date" + ',');
                sb.Append("Amount  Paid" + ',');
                sb.Append("Notes");
                sb.AppendLine();

                foreach (var pymnt in payments)
                {
                    sb.Append(pymnt.LastName + ',');
                    sb.Append(pymnt.FirstName + ',');
                    if (pymnt.AddressString.Contains(","))
                    {
                        sb.Append("\"" + pymnt.AddressString + "\"" + ",");
                    }
                    else
                    {
                        sb.Append(pymnt.AddressString  + ",");
                    }
                    sb.Append(pymnt.City + ',');
                    sb.Append(pymnt.Zip + ',');
                    sb.Append(pymnt.ActionCategory + ',');
                    if (pymnt.Action == null)
                    {sb.Append(',');}
                    else
                    {
                        if (pymnt.Action.Contains(","))
                        {
                            sb.Append("\"" + pymnt.Action + "\"" + ",");
                        }
                        else
                        {
                            sb.Append(pymnt.Action + ",");
                        }
                    }

                    sb.Append(pymnt.Date.ToString("MM/dd/yyyy") + ',');
                    sb.Append(pymnt.AmountDecimal.ToString("C") + ","); 
                    Total += (float)pymnt.AmountDecimal;
                    if (pymnt.Note == null)
                    { sb.Append(' ');}
                    else
                    if (pymnt.Note.Contains(","))
                    {
                        sb.Append("\"" + pymnt.Note + "\"" + ",");
                    }
                    else
                    {
                        sb.Append(pymnt.Note + ",");
                    }
                    sb.AppendLine();
                }
                sb.Append(",,,,,,,TOTAL,");
                sb.Append(Total);
                var response = System.Web.HttpContext.Current.Response;
                response.BufferOutput = true;
                response.Clear();
                response.ClearHeaders();
                response.ContentEncoding = Encoding.Unicode;
                response.AddHeader("content-disposition", "attachment;filename="
                                                          + "PaymentList" + DateTime.Today.ToString("MM-dd-yy") + ".csv");
                response.ContentType = "text/plain";
                response.Write(sb);
                response.End();
                return RedirectToAction("PaymentsByDate", new{ startDate, endDate});
            }

            public ActionResult ConvertPaymentsToDecimal()
            {
                Utilities.ConvertPaymentsToDecimal();
                return null;
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
