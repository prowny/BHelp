using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    public class DeliveriesController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Deliveries
        public ActionResult Index()
        {
            var listDeliveries = new List<Delivery>(db.Deliveries).Where(d =>  d.Status == 0)
                .OrderBy(z => z.Zip).ToList();
            var listDeliveryViewModels = new List<DeliveryViewModel>();
            foreach (var delivery in listDeliveries)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    var deliveryView = new DeliveryViewModel
                    {
                        Id = delivery.Id,
                        ClientId = client.Id,
                        DriverId = delivery.DriverId,
                        LogDate =delivery.LogDate,
                        NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(client.Id),
                        FamilyMembers = AppRoutines.GetFamilyMembers(client.Id),
                        FamilySelectList = AppRoutines.GetFamilySelectList(client.Id),
                        Kids = new List<FamilyMember>(),
                        Adults = new List<FamilyMember>(),
                        Seniors = new List<FamilyMember>()
                    };
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
                    if (delivery.FullBags != null) deliveryView.FullBags = (int) delivery.FullBags;
                    if (delivery.HalfBags != null) deliveryView.HalfBags = (int) delivery.HalfBags;
                    if (delivery.KidSnacks != null) deliveryView.KidSnacks = (int) delivery.KidSnacks;
                    if (delivery.GiftCards != null) deliveryView.GiftCards = (int) delivery.GiftCards;
                    if (delivery.GiftCardsEligible != null) deliveryView.GiftCardsEligible = (int) delivery.GiftCardsEligible;
                    deliveryView.DateLastDelivery = AppRoutines.GetLastDeliveryDate(client.Id);
                    deliveryView.DateLastGiftCard = AppRoutines.GetDateLastGiftCard(client.Id);
                    DateTime since1 = new DateTime(delivery.DeliveryDate.Year,
                        delivery.DeliveryDate.Month, 1);
                    DateTime thrudate = delivery.DeliveryDate.AddDays(-1);
                    deliveryView.GiftCardsThisMonth = GetGiftCardsSince(client.Id, since1, thrudate );
                    if (delivery.DriverId != null)
                    {
                        var driver = db.Users.Find(delivery.DriverId);
                        if (driver != null)
                        { deliveryView.DriverName = driver.FullName;}
                        else
                        {
                            deliveryView.DriverName = "(nobody yet)";
                        }
                    }
                    
                    var ODid = delivery.ODId; 
                    if (!ODid.IsNullOrEmpty() && ODid != "0")
                    {
                        var user = db.Users.Find(ODid);
                        deliveryView.ODName = user.FullName;
                    };
                    deliveryView.FirstName = client.FirstName;
                    deliveryView.LastName = client.LastName;
                    deliveryView.StreetNumber = client.StreetNumber;
                    deliveryView.StreetName = client.StreetName;
                    // (full length on mouseover)    \u00a0 is the Unicode character for NO-BREAK-SPACE.
                    deliveryView.StreetToolTip = client.StreetName.Replace(" ", "\u00a0");
                    deliveryView.City = client.City;
                    deliveryView.CityToolTip = client.City.Replace(" ", "\u00a0");
                    deliveryView.Zip = client.Zip;
                    deliveryView.Phone = client.Phone;
                    deliveryView.PhoneToolTip = client.Phone.Replace(" ", "\u00a0");
                    string s;
                    if (client.Notes != null)
                    {
                        deliveryView.Notes = client.Notes;
                        deliveryView.NotesToolTip = client.Notes.Replace(" ", "\u00a0");
                        s = deliveryView.Notes;
                        s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                        deliveryView.Notes = s;
                    }

                    if (delivery.ODNotes != null)
                    {
                        deliveryView.ODNotes = delivery.ODNotes;
                        deliveryView.ODNotesToolTip = delivery.ODNotes.Replace(" ", "\u00a0");
                        s = deliveryView.ODNotes;
                        s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                        deliveryView.ODNotes = s;
                    }

                    if (delivery.DriverNotes != null)
                    {
                        deliveryView.DriverNotes = delivery.DriverNotes;
                        deliveryView.DriverNotesToolTip = deliveryView.DriverNotes.Replace(" ", "\u00a0");
                        s  = deliveryView.DriverNotes; // For display, abbreviate to 12 characters:           
                        s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                        deliveryView.DriverNotes = s;
                    }

                    s = deliveryView.StreetName;
                    s = s.Length <= 9 ? s : s.Substring(0, 9) + "...";
                    deliveryView.StreetName = s;
                    s = deliveryView.City; // For display, abbreviate to 11 characters:           
                    s = s.Length <= 10 ? s : s.Substring(0, 10) + "...";
                    deliveryView.City = s;
                    s = deliveryView.Phone; // For display, abbreviate to 12 characters:           
                    s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                    deliveryView.Phone = s;
                    listDeliveryViewModels.Add(deliveryView);
                }
            }
            return View(listDeliveryViewModels);
        }
        public ActionResult OpenDeliveriesToCSV()
        {
            var result = AppRoutines.OpenDeliveriesToCSV();
            return result;
        }
        public ActionResult ExcelOpenDeliveries()
        {
            var result = AppRoutines.ExcelOpenDeliveries();
            return result;
        }
       
        // GET: Open Delivery Filters
        public ActionResult OpenFilters()
        {
            var listAllOpenDeliveries = db.Deliveries.Where(d => d.Status == 0).ToList();   // get all open deliveries
            var view = new OpenDeliveryViewModel()
            {
                OpenDeliveryCount = listAllOpenDeliveries.Count,
                DeliveryDatesList = new List<string>(), 
                DeliveryDatesSelectList = new List<SelectListItem>(),
                SelectedDeliveriesList = new List<Delivery>(),
                DriverList = new List<string>()
            };

            var distinctDatesList = listAllOpenDeliveries.Select(d => d.DateDelivered).Distinct().ToList();
            foreach (var y in distinctDatesList)
            {
                int delCountThisDate = listAllOpenDeliveries.Count(z => z.DateDelivered == y);
                view.DeliveryDatesList.Add ( y == null ? "-none-  (" + delCountThisDate  +")":
                    y.Value.ToString("MM/dd/yyyy") + " (" + delCountThisDate + ")");

                if (y != null)
                    view.DeliveryDatesSelectList.Add(new SelectListItem()
                        { Value = y.Value.ToString("MM/dd/yyyy"), Text = y.Value.ToString("MM/dd/yyyy") });
            }

            TempData["DeliveryDatesList"] = view.DeliveryDatesList;
            TempData["DeliveryDatesSelectList"] = view.DeliveryDatesSelectList;
            
            var distinctDriverIdList = listAllOpenDeliveries.Select(d => d.DriverId).Distinct().ToList();
            foreach (var y in distinctDriverIdList)
            {
                int delCountThisDriver = listAllOpenDeliveries.Count(z => z.DriverId == y);
                string driverName;
                var driver = db.Users.Find(y);
                if (driver == null)
                {
                    driverName = "(nobody yet)";
                }
                else
                {
                    driverName = driver.FullName;
                }
                view.DriverList.Add( driverName + " (" + delCountThisDriver + ")");
            }
            TempData["DriverList"] = view.DriverList;

            return View(view);
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OpenFilters(OpenDeliveryViewModel model, string btnByDate)
        {
            if (btnByDate != null)
            {
                var listAllOpenDeliveries = db.Deliveries.Where(d => d.Status == 0).ToList();   // get all open deliveries
                var view = new OpenDeliveryViewModel()
                {
                    OpenDeliveryCount = listAllOpenDeliveries.Count,
                    DeliveryDatesList = TempData["DeliveryDatesList"] as List<string>,
                    SelectedDeliveriesList = new List<Delivery>(),
                    DeliveryDatesSelectList =TempData["DeliveryDatesSelectList"] as List<SelectListItem>,
                    DriverList =TempData["DriverList"] as List<string>
                };
                TempData.Keep("DeliveryDatesList");
                TempData.Keep("DeliveryDatesSelectList");
                TempData.Keep("DriverList");

                var selectedDeliveries = db.Deliveries.Where(d => d.Status == 0
                                                          && d.DateDelivered == model.SelectedDeliveryDate).ToList();
                foreach (var del in selectedDeliveries)
                {
                    del.Checked = true;
                    del.DateDeliveredString = $"{del.DateDelivered:MM/dd/yyyy}";
                    
                    if (del.DeliveryDateODId != null)
                    {
                        var _od = db.Users.Find(del.DeliveryDateODId);
                        del.DeliveryDateODName = _od.FullName;
                    }
                    else
                    {
                        del.DeliveryDateODName ="(nobody yet)";
                    }

                    if (del.DriverId != null)
                    {
                        var _driver = db.Users.Find(del.DriverId);
                        del.DriverName = _driver.FullName;
                    }
                    else
                    {
                        del.DriverName = "(nobody yet)";
                    }

                    var _client = db.Clients.Find(del.ClientId);
                    if (_client != null)
                        del.ClientNameAddress = _client.LastName + ", " + _client.FirstName
                                                + " " + _client.StreetNumber + " " + _client.StreetName;

                    if (del.DeliveryDateODId != null)
                    {
                        var _ddod = db.Users.Find(del.DeliveryDateODId);
                        del.DeliveryDateODName = _ddod.FullName;
                    }
                    else
                    {
                        del.DeliveryDateODName = "(nobody yet)";
                    }
                    view.SelectedDeliveriesList.Add(del);
                }

                return View(view);
            }
            return null;
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
            return null;
        }

        // POST: Deliveries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "Id,ClientId,LogDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards")]
            Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                Client client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.FirstName = client.FirstName;
                    delivery.LastName = client.LastName;
                    delivery.StreetNumber = client.StreetNumber;
                    delivery.StreetName = delivery.StreetName;
                    delivery.NamesAgesInHH = AppRoutines.GetNamesAgesOfAllInHousehold(client.Id);
                    delivery.Zip = client.Zip;

                    db.Deliveries.Add(delivery);
                    db.SaveChanges();
                    return View(delivery);
                }
            }
            return RedirectToAction("Index");
        }

        // GET: Deliveries/Edit/5
        public ActionResult Edit(int? id, string desiredDeliveryDate)
        {
            switch (id)
            {
                case null:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                case 0: // coming from UpdateDesiredDeliveryDate
                    {
                    id = Convert.ToInt32(TempData["CurrentDeliveryId"]);
                    var del = db.Deliveries.Find(id);
                    if (del != null)
                    {
                        del.DeliveryDate = Convert.ToDateTime(desiredDeliveryDate);
                        db.SaveChanges();
                    }
                    break;
                }
            }
            TempData["CurrentDeliveryId"] = id.ToString();

            var delivery = db.Deliveries.Find(id);
            if (delivery == null) { return HttpNotFound(); }
            var viewModel = new DeliveryViewModel
            {
                Id = delivery.Id,
                ClientId = delivery.ClientId,
                LogDate = delivery.LogDate,
                ODId = delivery.ODId,
                DeliveryDateODId = delivery.DeliveryDateODId,
                ODList = AppRoutines.GetODSelectList(),
                ZipCodes = AppRoutines.GetZipCodesSelectList(),
                ODNotes = delivery.ODNotes,
                DriverId = delivery.DriverId,
                DriverName = GetDriverName(delivery.DriverId),
                DriverNotes = delivery.DriverNotes,
                DriversList = AppRoutines.GetDriversSelectList(),
                NamesAgesInHH = delivery.NamesAgesInHH,
                SnapshotFamily = GetSnapshotFamily(delivery.NamesAgesInHH),
                FamilySelectList = AppRoutines.GetFamilySelectList(delivery.ClientId),
                DatePriorDelivery = AppRoutines.GetPriorDeliveryDate(delivery.ClientId,delivery.LogDate),
                DateLastDelivery = GetLastGetDeliveryDate(delivery.Id),
                DeliveryDate = delivery.DeliveryDate, 
                DateDelivered = delivery.DateDelivered,
                Status = delivery.Status,
                HistoryStartDate = Convert.ToDateTime(Session["CallLogStartDate"]),
                HistoryEndDate = Convert.ToDateTime(Session["CallLogEndDate"])
            };
            switch (delivery.Status)
            {
                case 0:
                    viewModel.SelectedStatus = "Open";
                    break;
                case 1:
                    viewModel.SelectedStatus = "Delivered";
                    break;
                case 2:
                    viewModel.SelectedStatus = "Undelivered";
                    break;
            }

            if (Request.UrlReferrer != null)
            { viewModel.ReturnURL = Request.UrlReferrer.ToString(); }
         
            foreach (var item in viewModel.DriversList)
            { if (item.Value == viewModel.DriverId) { item.Selected = true; break; } }

            foreach (var item in viewModel.ODList)
            { if (item.Value == viewModel.ODId) { item.Selected = true; break; } }

            foreach (var item in viewModel.ZipCodes)
            { if (item.Value == viewModel.Zip) { item.Selected = true; break; } }

            viewModel.DeliveryDateODList = viewModel.ODList;
            foreach (var item in viewModel.DeliveryDateODList)
            { if (item.Value == viewModel.DeliveryDateODId) { item.Selected = true; break; } }

            if (delivery.Children != null) viewModel.KidsCount = (int) delivery.Children;
            if (delivery.Adults != null) viewModel.AdultsCount = (int) delivery.Adults;
            if (delivery.Seniors != null) viewModel.SeniorsCount = (int) delivery.Seniors;

           // viewModel.GiftCardsEligible = AppRoutines.GetGiftCardsEligible(delivery.ClientId, delivery.DeliveryDate);
           
            if (delivery.FullBags != null) viewModel.FullBags = (int)delivery.FullBags;
            if (delivery.HalfBags != null) viewModel.HalfBags = (int)delivery.HalfBags;
            if (delivery.KidSnacks != null) viewModel.KidSnacks = (int)delivery.KidSnacks;
            if (delivery.GiftCards != null) viewModel.GiftCards = (int)delivery.GiftCards;
            if (delivery.GiftCardsEligible != null) viewModel.GiftCardsEligible = (int)delivery.GiftCardsEligible;

            var client = db.Clients.Find(delivery.ClientId);
            if (client != null)
            {
                viewModel.Client = client;
                viewModel.ClientNameAddress = client.LastName + ", " + client.FirstName
                                              + " " + client.StreetNumber + " " + client.StreetName + " " + client.Zip;
                viewModel.Notes = client.Notes;
                viewModel.DateLastDelivery = AppRoutines.GetLastDeliveryDate(client.Id);
                viewModel.DateLastGiftCard = AppRoutines.GetDateLastGiftCard(client.Id);
                if (client.Notes != null)
                {
                    viewModel.NotesToolTip = client.Notes.Replace(" ", "\u00a0");
                    var s = viewModel.Notes;
                    s = s.Length <= 12 ? s : s.Substring(0, 12) + "...";
                    viewModel.Notes = s;
                }
            }

            viewModel.Zip = delivery.Zip;

            return View(viewModel);
        }

        // POST: Deliveries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "Id,ClientId,LogDate,Notes,FullBags,HalfBags,KidSnacks,GiftCards," +
            "DateDelivered,ODNotes,DriverNotes,GiftCardsEligible,DriverId,Completed," +
            "DeliveryDate,ODId,DeliveryDateODId,ReturnURL,SelectedStatus,Zip")] DeliveryViewModel delivery)
        {
            if (ModelState.IsValid)
            {
                var updateData  = db.Deliveries.Find(delivery.Id);

                if (updateData != null)
                {
                    Client client = db.Clients.Find(updateData.ClientId);
                    if (client != null) updateData.Zip = client.Zip;
                    updateData.DateDelivered = delivery.LogDate;
                    updateData.LogDate = delivery.LogDate;
                    updateData.FullBags = delivery.FullBags;
                    updateData.HalfBags = delivery.HalfBags;
                    updateData.KidSnacks = delivery.KidSnacks;
                    updateData.GiftCards = delivery.GiftCards;
                    updateData.GiftCardsEligible = delivery.GiftCardsEligible;
                    updateData.ODNotes = delivery.ODNotes;
                    updateData.DriverId = updateData.DriverId == "0" ? null : delivery.DriverId;
                    updateData.ODId = delivery.ODId;
                    updateData.DeliveryDateODId = delivery.DeliveryDateODId;
                    updateData.DriverNotes = delivery.DriverNotes;
                    updateData.DeliveryDate = delivery.DeliveryDate;
                    updateData.DateDelivered = delivery.DateDelivered;
                    updateData.Zip = delivery.Zip;
                    switch (delivery.SelectedStatus)
                    {
                        case "Open":
                            updateData.Status = 0;
                            break;
                        case "Delivered":
                            updateData.Status = 1;
                            break;
                        case "Undelivered":
                            updateData.Status = 2;
                            break;
                    }
                    
                    db.Entry(updateData).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if(delivery.ReturnURL.Contains("CallLogIndividual"))
                {
                    if (updateData != null)
                        return RedirectToAction("CallLogIndividual", new {clientId = updateData.ClientId});
                }

                if (delivery.ReturnURL.Contains("CallLogByLogDate"))
                { return RedirectToAction("CallLogByLogDate", 
                    new{startDate = Session["CallLogStartDate"], endDate = Session["CallLogEndDate"]}); }

                if (delivery.ReturnURL.Contains("CallLogByDateDelivered"))
                { return RedirectToAction("CallLogByDateDelivered"); }

                return RedirectToAction("Index");
            }
            return View(delivery);
        }
        
        // GET: Deliveries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }

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
        public ActionResult CallLogMenu()
        {
            return View();
        }
        public ActionResult CallLogIndividual(int? clientId)
        {
            var clientSelectList = new List<SelectListItem>();
            var clientList = db.Clients.OrderBy(c => c.LastName).ToList();
            foreach (var client in clientList)
            {
                if (!db.Deliveries.Any(d => d.ClientId == client.Id)) continue;
                var text = client.LastName + ", " + client.FirstName + " ";
                text += client.StreetNumber + " " + client.StreetName;
                var selListItem = new SelectListItem() { Value = client.Id.ToString(), Text = text };
                if (clientId == client.Id)
                {
                    selListItem.Selected = true;
                }
                clientSelectList.Add(selListItem);
            }
            var callLogView = new DeliveryViewModel { ClientSelectList = clientSelectList };
            if (clientId == null)
            {    return View(callLogView);}
            else
            {
                var deliveryList = db.Deliveries.Where(d => d.ClientId == clientId)
                    .OrderByDescending(d => d.DateDelivered).ToList();
                callLogView.DeliveryList = deliveryList;
                foreach (var del in callLogView.DeliveryList)
                {
                    del.DriverName = GetDriverName(del.DriverId);
                    if (del.Status == 1)
                    {
                        if (del.DateDelivered.HasValue)
                        {
                            del.DateDeliveredString = $"{del.DateDelivered:MM/dd/yyyy}";
                        }
                    }

                    del.LogDateString = $"{del.LogDate:MM/dd/yyyy}";
                    switch (del.Status)
                    {
                        case 0:
                            del.SelectedStatus = "Open";
                            del.DateDeliveredString = "";
                            break;
                        case 1:
                            del.SelectedStatus = "Delivered";
                            break;
                        case 2:
                            del.SelectedStatus = "Undelivered";
                            del.DateDeliveredString = "";
                            break;
                    }
                }
            }
            return View(callLogView);
        }

        [HttpPost]
        public ActionResult CallLogIndividual(string id)
        {
            var intClientId = Convert.ToInt32(id);
            return RedirectToAction("CallLogIndividual", new { clientId = intClientId });
        }
        public ActionResult CallLogByLogDate(DateTime? startDate, DateTime? endDate )
        {
            if (!startDate.HasValue || !endDate.HasValue)  // default to today and 1 week ago
            {
                startDate = DateTime.Today.AddDays(-7);
                endDate = DateTime.Today;
            }  
         
            List<Delivery> deliveries = db.Deliveries
                .Where(d => d.DateDelivered >= startDate && d.DateDelivered <= endDate)
                .OrderByDescending(d => d.DateDelivered).ToList();
            var callLogView = new DeliveryViewModel
            {
                DeliveryList = deliveries,
                HistoryStartDate = Convert.ToDateTime(startDate),
                HistoryEndDate = Convert.ToDateTime(endDate)
            };

            Session["CallLogStartDate"] = callLogView.HistoryStartDate;
            Session["CallLogEndDate"] = callLogView.HistoryEndDate;

            foreach (var del in callLogView.DeliveryList)
            {
                del.DriverName = GetDriverName(del.DriverId);
                if (del.DateDelivered.HasValue)
                {
                    del.DateDeliveredString = $"{del.DateDelivered:MM/dd/yyyy}";
                }
                del.LogDateString = $"{del.LogDate:MM/dd/yyyy}";
                switch (del.Status)
                {
                    case 0:
                        del.SelectedStatus = "Open";
                        break;
                    case 1:
                        del.SelectedStatus = "Delivered";
                        break;
                    case 2:
                        del.SelectedStatus = "Undelivered";
                        break;
                }
            }
            return View(callLogView);
        }
        public ActionResult CallLogByDateDelivered(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)  // default to today and 1 week ago
            {
                startDate = DateTime.Today.AddDays(-7);
                endDate = DateTime.Today;
            }

            Session["CallLogStartDate"] = startDate;
            Session["CallLogEndDate"] = endDate;

            List<Delivery> deliveries = db.Deliveries
                .Where(d => d.Status == 1 && d.DateDelivered >= startDate && d.DateDelivered <= endDate)
                .OrderByDescending(d => d.DateDelivered).ToList();
            var callLogView = new DeliveryViewModel
            {
                DeliveryList = deliveries,
                HistoryStartDate = Convert.ToDateTime(startDate),
                HistoryEndDate = Convert.ToDateTime(endDate),
            };

            foreach (var del in callLogView.DeliveryList)
            {
                del.DriverName = GetDriverName(del.DriverId);
                if (del.DateDelivered.HasValue)
                {
                    del.DateDeliveredString = $"{del.DateDelivered:MM/dd/yyyy}";
                }
                switch (del.Status)
                {
                    case 0:
                        del.SelectedStatus = "Open";
                        del.DateDeliveredString = "";
                        break;
                    case 1:
                        del.SelectedStatus = "Delivered";
                        break;
                    case 2:
                        del.SelectedStatus = "Undelivered";
                        del.DateDeliveredString = "";
                        break;
                }
            }
            return View(callLogView);
        }
        public ActionResult ReportsMenu()
        {
            return View();
        }
        public ActionResult CountyReport(string yy = "", string qtr = "")
        {
            int reportYear;
            int reportQuarter = 0;
            if (yy.IsNullOrEmpty() || qtr.IsNullOrEmpty())   // Default to this year, this quarter
            {
               reportYear = Convert.ToInt32(DateTime.Now.Year.ToString());
                var month = Convert.ToInt32(DateTime.Now.Month.ToString());
                if (month >= 1 && month <= 3) { reportQuarter = 1;}     
                if (month >= 4 && month <= 6) { reportQuarter = 2;}     
                if (month >= 7 && month <= 9) { reportQuarter = 3; }    
                if (month >= 10 && month <= 12) { reportQuarter = 4; }
            }
            else
            {
                reportYear = Convert.ToInt32(yy);
                reportQuarter = Convert.ToInt32(qtr);
            }

            var view = GetCountyReportView(reportYear, reportQuarter);
            return View(view);
        }
        public ActionResult HelperReport(string yy = "", string mm = "")
        {
            int reportYear;
            int reportMonth;
            if (yy.IsNullOrEmpty() || mm.IsNullOrEmpty())  // Default to this month
            {
                reportYear = Convert.ToInt32(DateTime.Now.Year.ToString());
                reportMonth = Convert.ToInt32(DateTime.Now.Month.ToString());
            }
            else
            {
                reportYear = Convert.ToInt32(yy);
                reportMonth = Convert.ToInt32(mm);
            }
        
            var view = GetHelperReportView(reportYear, reportMonth);
            return View(view);
        }
        public ActionResult HelperReportToExcel(string yy = "", string mm = "")
        {
            var year = Convert.ToInt32(yy);
            var month = Convert.ToInt32(mm);
            var view = GetHelperReportView(year, month);
            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);

            int activeRow = 1;
            var titleDate = new DateTime(year, month, 1);
            var title = "Bethesda Help, Inc. " + titleDate.ToString("MMMM") + " " + year
                        + " Delivery Totals";
            ws.Cell(activeRow, 1).SetValue(title);
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Time Period");
            for (int i = 0; i < view.ZipCodes.Count; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.ZipCodes[i]); }
            ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("Total Zip Codes");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue(view.MonthYear[0]);
            activeRow++;
            ws.Cell(activeRow, 1).SetValue(view.MonthYear[1]);
            activeRow++;

            for (var row = activeRow; row < activeRow + 19; row++)
            {
                ws.Cell(row, 1).SetValue(view.HelperTitles[row - 4]);
                for (var col = 1; col < view.ZipCodes.Count + 2; col++)
                {
                    ws.Cell(row, col + 1).SetValue(view.ZipCounts[ row - 4, col]);
                }
            }
           
            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = view.ReportTitle + ".xlsx" };
        }
        private static ReportsViewModel GetHelperReportView(int year, int month)
        {
            var view = new ReportsViewModel {Year = year, Month = month};
            if (DateTimeFormatInfo.CurrentInfo != null)
            {
                view.DateRangeTitle = DateTimeFormatInfo.CurrentInfo.GetMonthName(view.Month)
                                      + " " + view.Year.ToString() + " Delivery Totals";
                view.ReportTitle ="Bethesda Helper Data "
                                  + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(view.Month) 
                                  + " " + view.Year.ToString();
            }
            view.MonthYear = new string[2];
            var startDate = new DateTime(view.Year, view.Month, 1);
            view.MonthYear[0] = startDate.ToShortDateString();
            var daysInMonth = DateTime.DaysInMonth(view.Year, view.Month);
            var thruDate = new DateTime(view.Year, view.Month, daysInMonth);
            view.MonthYear[1] =thruDate.ToShortDateString();
            view.HelperTitles = new string[20];
            view.HelperTitles[1] = "# Total Food Lbs";
            view.HelperTitles[2] = "# Households Distinct Served";
            view.HelperTitles[3] = "# Distinct Residents Served";
            view.HelperTitles[4] = "# Distinct Residents <18";
            view.HelperTitles[5] = "# Distinct Adults 18-59";
            view.HelperTitles[6] = "# Distinct Residents 60+";
            view.HelperTitles[7] = "# Deliveries";
            view.HelperTitles[8] = "# Repeat Deliveries";
            view.HelperTitles[9] = "# First-Time Deliveries";
            view.HelperTitles[10] = "# Cumulative Residents Served Daily";
            view.HelperTitles[11] = "# Cumulative Residents Children <18";
            view.HelperTitles[12] = "# Cumulative Residents Adults 18-59";
            view.HelperTitles[13] = "# Cumulative Residents Seniors 60+";
            view.HelperTitles[14] = "# Full Bags (10 lbs per bag)";
            view.HelperTitles[15] = "# Half Bags (9 lbs per bag)";
            view.HelperTitles[16] = "# Snacks";
            view.HelperTitles[17] = "# Total Lbs of Full Bags";
            view.HelperTitles[18] = "# Total Lbs of Half Bags";
            view.HelperTitles[19] = "# Giant Gift Cards Disbursed";

            view.ZipCodes = AppRoutines.GetZipCodesList();
            view.ZipCounts = new int[20, view.ZipCodes.Count + 2]; // ZipCodes, Counts
            var totalDistinctHouseholds = 0;
            var totalCumulativeRepeatDeliveries = 0;
            var totalCumulativeFirstDeliveries = 0;
            var totalDistinctChildren = 0;
            var totalDistinctAdults = 0;
            var totalDistinctSeniors = 0;
            var totalDeliveries = 0;
            var totalCumulativeChildren = 0;
            var totalCumulativeAdults = 0;
            var totalCumulativeSeniors = 0;
            var totalFullBags = 0;
            var totalHalfBags = 0;
            var totalSnacks = 0;
            var totalCards = 0;
            using (var db = new BHelpContext())
            {
                for (int zip = 0; zip < view.ZipCodes.Count; zip++)
                {
                    var stringZip = view.ZipCodes[zip];
                    var deliveryData = db.Deliveries.Where(d =>  d.Status == 1 
                                                && d.Zip == stringZip && d.DateDelivered >= startDate
                                                && d.DateDelivered <= thruDate).ToList();
                    totalDeliveries += deliveryData.Count;
                    
                    List<int> distinctList = new List<int>();
                    var distinctChildren = 0; 
                    var distinctAdults = 0;
                    var distinctSeniors = 0;  
                    foreach (var del in deliveryData)
                    {
                        for (var i = 0; i < deliveryData.Count; i++)
                        {
                            if (distinctList.Contains(del.ClientId))
                                continue;
                            distinctChildren += del.Children.Value;
                            totalDistinctChildren += del.Children.Value;
                            distinctAdults += del.Adults.Value;
                            totalDistinctAdults += del.Adults.Value; 
                            distinctSeniors += del.Seniors.Value;
                            totalDistinctSeniors += del.Seniors.Value;
                            distinctList.Add(del.ClientId);
                        }
                    }

                    totalDistinctHouseholds += distinctList.Count;
                    var totalRepeatDeliveries = deliveryData.Count - distinctList.Count;
                    totalCumulativeRepeatDeliveries += totalRepeatDeliveries;

                    var totalFirstDeliveries = 0;
                    var cumulativeChildren = 0;
                    var cumulativeAdults = 0;
                    var cumulativeSeniors = 0;
                    var fullBags = 0;
                    var halfBags = 0;
                    var snacks = 0;
                    var cards = 0;
                    foreach (var del in deliveryData)
                    {
                        if (del.FirstDelivery)
                        {
                            totalFirstDeliveries++;
                            totalCumulativeFirstDeliveries++;
                        }
                        if (del.Children != null)
                        {
                            cumulativeChildren += del.Children.Value;
                            totalCumulativeChildren += del.Children.Value;
                        }

                        if (del.Adults != null)
                        {
                            cumulativeAdults += del.Adults.Value;
                            totalCumulativeAdults += del.Adults.Value;
                        }

                        if (del.Seniors != null)
                        {
                            cumulativeSeniors += del.Seniors.Value;
                            totalCumulativeSeniors += del.Seniors.Value;
                        }

                        if (del.FullBags != null)
                        {
                            fullBags += del.FullBags.Value;
                            totalFullBags += del.FullBags.Value;
                        }

                        if (del.HalfBags != null)
                        {
                            halfBags += del.HalfBags.Value;
                            totalHalfBags += del.HalfBags.Value;
                        }

                        if (del.KidSnacks != null)
                        {
                            snacks += del.KidSnacks.Value;
                            totalSnacks += del.KidSnacks.Value;
                        }

                        if (del.GiftCards != null)
                        {
                            cards += del.GiftCards.Value;
                            totalCards += del.GiftCards.Value;
                        }
                    }

                    var col = zip + 1; 
                    view.ZipCounts[1, col] = fullBags * 10 + halfBags *9;  // Total Food Lbs
                    view.ZipCounts[2, col] = distinctList.Count;
                    view.ZipCounts[3, col] = distinctChildren + distinctAdults + distinctSeniors; // Distinct Residents
                    view.ZipCounts[4, col] = distinctChildren;
                    view.ZipCounts[5, col] = distinctAdults;
                    view.ZipCounts[6, col] = distinctSeniors;
                    view.ZipCounts[7, col] = deliveryData.Count;    // Total Deliveries
                    view.ZipCounts[8, col] = totalRepeatDeliveries; // Repeat Deliveries
                    view.ZipCounts[9, col] = totalFirstDeliveries; // First-Time Deliveries
                    view.ZipCounts[10, col] = cumulativeChildren + cumulativeAdults + cumulativeSeniors;
                    view.ZipCounts[11, col] = cumulativeChildren;
                    view.ZipCounts[12, col] = cumulativeAdults;
                    view.ZipCounts[13, col] = cumulativeSeniors;
                    view.ZipCounts[14, col] = fullBags;
                    view.ZipCounts[15, col] = halfBags;
                    view.ZipCounts[16, col] = snacks;
                    view.ZipCounts[17, col] = fullBags * 10;
                    view.ZipCounts[18, col] = halfBags * 9;
                    view.ZipCounts[19, col] = cards;
                }
            }

            var totCol = view.ZipCodes.Count + 1;
            view.ZipCounts[1, totCol] = totalFullBags * 10 + totalHalfBags *9;  // Total Food Lbs
            view.ZipCounts[2, totCol] = totalDistinctHouseholds; // Distinct Households
            view.ZipCounts[3, totCol] = totalDistinctChildren + totalDistinctAdults + totalDistinctSeniors; // Distinct Residents
            view.ZipCounts[4, totCol] = totalDistinctChildren;
            view.ZipCounts[5, totCol] = totalDistinctAdults;
            view.ZipCounts[6, totCol] = totalDistinctSeniors;
            view.ZipCounts[7, totCol] = totalDeliveries;    // Total Deliveries
            view.ZipCounts[8, totCol] = totalCumulativeRepeatDeliveries; // Repeat Deliveries
            view.ZipCounts[9, totCol] = totalCumulativeFirstDeliveries; // First-Time Deliveries
            view.ZipCounts[10, totCol] = totalCumulativeChildren + totalCumulativeAdults + totalCumulativeSeniors;
            view.ZipCounts[11, totCol] = totalCumulativeChildren;
            view.ZipCounts[12, totCol] = totalCumulativeAdults;
            view.ZipCounts[13, totCol] = totalCumulativeSeniors;
            view.ZipCounts[14, totCol] = totalFullBags;
            view.ZipCounts[15, totCol] = totalHalfBags;
            view.ZipCounts[16, totCol] = totalSnacks;
            view.ZipCounts[17, totCol] = totalFullBags * 10;
            view.ZipCounts[18, totCol] = totalHalfBags * 9;
            view.ZipCounts[19, totCol] = totalCards;

            return view;
        }
        private List<SelectListItem> GetSnapshotFamily(string listHH)
        {
            var i = 0;
            List<SelectListItem> familyList = new List<SelectListItem>();
            var listStrLineElements = listHH.Split(',').ToList();
            foreach (var mbr in listStrLineElements)
            {
                i++;
                var newListItem = new SelectListItem() { Value = i.ToString(), Text = mbr };
                familyList.Add(newListItem);
            }
            return familyList;
        }
        private ReportsViewModel GetCountyReportView(int yy, int qtr)
        {
            var view = new ReportsViewModel { Year = yy, Quarter = qtr };
            if (qtr == 1) { view.Months = new[] { 1, 2, 3 }; }
            if (qtr == 2) { view.Months = new[] { 4, 5, 6 }; }
            if (qtr == 3) { view.Months = new[] { 7, 8, 9 }; }
            if (qtr == 4) { view.Months = new[] { 10, 11, 12 }; }
            if (DateTimeFormatInfo.CurrentInfo != null)
            {
                view.MonthYear = new string[3];
                view.MonthYear[0] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(1 + 3 * (qtr - 1))
                    + " " + view.Year.ToString();
                view.MonthYear[1] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(2 + 3 * (qtr - 1))
                    + " " + view.Year.ToString();
                view.MonthYear[2] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(3 + 3 * (qtr - 1))
                    + " " + view.Year.ToString();
                view.DateRangeTitle = view.MonthYear[0] + " through " + view.MonthYear[2];
                view.ReportTitle = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(1 + 3 * (qtr - 1))
                                   + "-" + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(3 + 3 * (qtr - 1))
                                   + " " + view.Year.ToString() + " County Report";
            }

            view.ZipCodes = AppRoutines.GetZipCodesList();
            // Load Counts - extra zip code is for totals column.
            view.Counts = new int[13, view.ZipCodes.Count + 1, 6]; //Month, ZipCodes, Counts
            for (int i = 0; i < 3; i++)
            {
                var mY = view.MonthYear[i].Split(' ');
                var mo = DateTime.ParseExact(mY[0], "MMMM", CultureInfo.CurrentCulture).Month;
                var startDate = Convert.ToDateTime(mo.ToString() + "/01/" + mY[1]);
                DateTime endDate;
                if (mo == 12)
                {
                    var enDt = Convert.ToDateTime("12/31/" + mY[1]);
                    endDate = enDt.AddDays(1);
                }
                else
                {
                    endDate = Convert.ToDateTime((mo + 1).ToString() + "/01/" + mY[1]);
                }
               
                var deliveries = db.Deliveries
                    .Where(d => d.Status == 1 && d.DateDelivered >= startDate 
                                && d.DateDelivered < endDate).ToList();
                 
                foreach (var delivery in deliveries)
                {
                    var t = view.ZipCodes.Count;  // Extra zip code column is for totals
                    for (var j = 0; j < view.ZipCodes.Count; j++)
                    {
                        if (delivery.Zip == view.ZipCodes[j])
                        {
                            view.Counts[mo, j, 0]++; view.Counts[mo, t, 0]++;  // month, zip, # of families
                            var c = Convert.ToInt32(delivery.Children);
                            var a = Convert.ToInt32(delivery.Adults);
                            var s = Convert.ToInt32(delivery.Seniors);
                            view.Counts[mo, j, 1] += c; view.Counts[mo, t, 1] += c;
                            view.Counts[mo, j, 2] += a; view.Counts[mo, t, 2] += a;
                            view.Counts[mo, j, 3] += s; view.Counts[mo, t, 3] += s;
                            view.Counts[mo, j, 4] += (a + c + s); view.Counts[mo, t, 4] += (a + c + s);  // # of residents
                            var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                            view.Counts[mo, j, 5] += lbs; view.Counts[mo, t, 5] += lbs;  // pounds of food
                        }
                    }
                }
            }
            return view;
        }
        public ActionResult CountyReportToExcel(int yy, int qtr)
        {
            var view = GetCountyReportView(yy, qtr);
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue("Bethesda Help, Inc.");
            activeRow ++;
            ws.Cell(activeRow, 1).SetValue(view.DateRangeTitle);
            activeRow += 2;
            for(int mo = 0; mo < 3; mo++)
            {
                ws.Cell(activeRow, 1).SetValue(view.MonthYear[mo]);
                ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("TOTAL");
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("Zip Code");
                for (int i = 0; i < view.ZipCodes.Count; i++)
                {
                    ws.Cell(activeRow, i +2).SetValue(view.ZipCodes[i]); ;
                }
                ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("All Zip Codes");
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Families");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 0]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Children (&#60;18)");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 1]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Adults(&#62;=18 and &#60;60");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 2]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Seniors (&#62;=60)");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 3]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Residents");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 4]);
                }
                activeRow ++;
                ws.Cell(activeRow, 1).SetValue("# of Pounds of Food");
                for (int i = 0; i < view.ZipCodes.Count + 1; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 5]);
                }
                activeRow += 2;
            }
            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = view.ReportTitle +".xlsx" };
        }
        private  DateTime? GetLastGetDeliveryDate(int id)
        {
            DateTime? dt = db.Deliveries.Where(d => d.DateDelivered != null
                                                    && d.Id == id)
                .OrderByDescending(x => x.DeliveryDate).Select(d => d.DateDelivered)
                .FirstOrDefault();

            // ReSharper disable once UseNullPropagation
            if (dt == null)
            {
                return null;
            }
            else
            {
                return (DateTime) dt;
            }
        }
        private int GetGiftCardsSince(int clientId, DateTime dt1, DateTime dt2)
        {
            var total = 0;
            var dList = db.Deliveries.Where(d => d.ClientId == clientId
                                      && d.DateDelivered >= dt1 && d.DateDelivered <= dt2 )
                .Select(g => g.GiftCards).ToList();
            foreach(var i in dList)
            {
                if (i != null)
                {
                    var gc = (int) i;
                    total += gc;
                }
            }
            return total;
        }
        private string GetDriverName(string id)
        {
            if (id != null)
            {
                var driver = db.Users.Find(id);
                if (driver != null) return driver.FullName;
            }

            return "(nobody yet)";
        }
        public ActionResult QuorkReport(string endingDate = "")
        {
            DateTime endDate;
            if (endingDate.IsNullOrEmpty())
            {
                // Ends on a Saturday - weekday Monday is 1, Saturday is 6
                // If today is a  Saturday, default to this week
                int weekDay = Convert.ToInt32(DateTime.Today.DayOfWeek);
                if (weekDay >= 6) // Default to this this Saturday, else Saturday last week
                { endDate = DateTime.Today.AddDays(6 - weekDay); }
                else
                {
                    DateTime lastSaturday = DateTime.Now.AddDays(-1);
                    while (lastSaturday.DayOfWeek != DayOfWeek.Saturday) lastSaturday = lastSaturday.AddDays(-1);
                    endDate = lastSaturday;
                }
            }
            else
            {
                endDate =Convert.ToDateTime(endingDate);
            }

            var view = GetQuorkReportView(endDate);
            return View(view);
        }
        private ReportsViewModel GetQuorkReportView(DateTime endDate)
        {
            DateTime startDate = endDate.AddDays(-6); 
            var view = new ReportsViewModel()
            {
                BeginDate = startDate,
                EndDate = endDate
            };
            view.EndDateString = view.EndDate.ToString("M-d-yy");
            view.DateRangeTitle= startDate.ToShortDateString() + " - " + endDate.ToShortDateString();
            view.ReportTitle = view.EndDateString + " QORK Weekly Report";
            
            view.ZipCodes = AppRoutines.GetZipCodesList();
            // Load Counts - extra zip code is for totals column.
            view.Counts = new int[1, view.ZipCodes.Count + 1, 8]; // 0 (unused), ZipCodes, Counts
            var deliveries = db.Deliveries
                .Where(d => d.Status == 1 &&d.DateDelivered >= startDate
                                          && d.DateDelivered < endDate).ToList();

            foreach (var delivery in deliveries)
            {
                var zipCount = view.ZipCodes.Count;  // Extra zip code column is for totals
                for (var j = 0; j < view.ZipCodes.Count; j++)
                {
                    if (delivery.Zip == view.ZipCodes[j])
                    {
                        var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                        view.Counts[0, j, 0] += lbs; view.Counts[0, zipCount, 0] += lbs;   //pounds of food
                        view.Counts[0, j, 1] ++; view.Counts[0, zipCount, 1] ++; //# unique households served
                        var c = Convert.ToInt32(delivery.Children);
                        var a = Convert.ToInt32(delivery.Adults);   
                        var s = Convert.ToInt32(delivery.Seniors);  
                        view.Counts[0, j, 2] += (a + c + s); view.Counts[0, zipCount, 2] += (a + c + s);    //# residents served
                        view.Counts[0, j, 3] += c; view.Counts[0, zipCount, 3] += c; //# children
                        view.Counts[0, j, 4] += s; view.Counts[0, zipCount, 4] += s;  //# seniors
                        view.Counts[0, j, 5] = 0; view.Counts[0, zipCount, 5]= 0;  //#staff worked  ZERO!!!
                        view.Counts[0, j, 6] = 0; view.Counts[0, zipCount, 6] = 0;   //# staff hours   ZERO!!!
                        view.Counts[0, j, 7]++; view.Counts[0, zipCount, 7]++;  //# deliveries
                    }
                }
            }
            return view;
        }
        public ActionResult QuorkReportToExcel(string endingDate)
        {
            var endDate = Convert.ToDateTime(endingDate);
            var view = GetQuorkReportView(endDate);
            var workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);

            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue("Bethesda Help, Inc. Quork Report");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Time Period");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue(view.DateRangeTitle);
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Zip Codes");
            for (int i = 0; i < view.ZipCodes.Count; i++)
            {
                ws.Cell(activeRow, i + 2).SetValue(view.ZipCodes[i]);
            }
            ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("Total Zip Codes");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("Total Food Lbs)");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 0]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# HH Served (No Repeat Clients in Time Period)");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0,i,1]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Residents Served");   
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 2]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Residents <18");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 3]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Residents >60");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 4]); }
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Staff Worked");
            activeRow++;  
            ws.Cell(activeRow, 1).SetValue("/# Staff Hours");
            activeRow++;
            ws.Cell(activeRow, 1).SetValue("# Deliveries");
            for (int i = 0; i < view.ZipCodes.Count + 1; i++)
            { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 7]); }

            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = view.ReportTitle + ".xlsx" };
        }
        public ActionResult SaturdayNext(DateTime saturday)
        {
            saturday = saturday.AddDays(7);
            return RedirectToAction("QuorkReport", new { endingDate = saturday.ToShortDateString() });
        }
        public ActionResult SaturdayPrevious(DateTime saturday)
        {
            saturday = saturday.AddDays(-7);
            return RedirectToAction("QuorkReport", new{endingDate = saturday.ToShortDateString()});
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
        public ActionResult ReturnToReportsMenu()
        {
            return RedirectToAction("ReportsMenu");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
