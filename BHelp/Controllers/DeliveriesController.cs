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
    [Authorize]
    public class DeliveriesController : Controller
    {
        // GET:  Open Deliveries
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay,PantryCoordinator")]
        public ActionResult Index()
        {
            using var db = new BHelpContext();
            var listDeliveries = new List<Delivery>(db.Deliveries)
                .Where(d => d.Status == 0)
                .OrderBy(d => d.DateDelivered).ThenBy(z => z.Zip)
                .ThenBy(n => n.StreetNumber).ThenBy(s => s.StreetName).ToList();

            var duplicateClientIds = from x in listDeliveries
                group x by x.ClientId
                into g
                let count = g.Count()
                select new { Count = count, Id = g.First().ClientId };

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
                        LogDate = delivery.LogDate,
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

                        if (mbr.Age is >= 18 and < 60)
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
                    deliveryView.FullBags = delivery.FullBags;
                    deliveryView.HalfBags = delivery.HalfBags;
                    deliveryView.KidSnacks = delivery.KidSnacks;
                    deliveryView.GiftCards = delivery.GiftCards;
                    deliveryView.GiftCardsEligible = delivery.GiftCardsEligible;
                    deliveryView.DateLastDelivery = AppRoutines.GetLastDeliveryDate(client.Id);
                    deliveryView.DateLastGiftCard = AppRoutines.GetDateLastGiftCard(client.Id);

                    var dateDelivered = DateTime.Today.AddDays(-1);
                    if (delivery.DateDelivered != null)
                    {
                        deliveryView.DateDelivered = delivery.DateDelivered;
                        dateDelivered = delivery.DateDelivered.Value;
                    }

                    var since1 = new DateTime(dateDelivered.Year, dateDelivered.Month, 1);
                    var thrudate = dateDelivered.AddDays(-1);
                    deliveryView.GiftCardsThisMonth = GetGiftCardsSince(client.Id, since1, thrudate);

                    if (delivery.DateDelivered != null)
                    {
                        deliveryView.DateDeliveredString = delivery.DateDelivered.Value.ToString("MM/dd/yyyy");
                    }

                    // Check for EligiibilityRulesException:
                    deliveryView.NextDeliveryEligibleDate = AppRoutines.GetNextEligibleDeliveryDate
                        (delivery.ClientId, DateTime.Today);
                    if (deliveryView.DateDelivered < deliveryView.NextDeliveryEligibleDate)
                        deliveryView.EligiibilityRulesException = true;

                    // ReSharper disable once PossibleMultipleEnumeration
                    var clientIds = duplicateClientIds.ToList();
                    if (duplicateClientIds != null)
                        foreach (var dup in clientIds)
                        {
                            if (dup.Id == deliveryView.ClientId && dup.Count > 1)
                            {
                                deliveryView.EligiibilityRulesException = true;
                            }
                        }

                    if (deliveryView.GiftCards > 0)
                    {
                        deliveryView.NextGiftCardEligibleDate = AppRoutines.GetNextGiftCardEligibleDate
                            (deliveryView.ClientId, deliveryView.DateLastGiftCard);
                        if (deliveryView.DateDelivered < deliveryView.NextGiftCardEligibleDate
                            || deliveryView.GiftCards > deliveryView.GiftCardsEligible)
                            deliveryView.EligiibilityRulesException = true;
                    }

                    if (delivery.DriverId != null)
                    {
                        var driver = db.Users.Find(delivery.DriverId);
                        deliveryView.DriverName = driver != null ? driver.FullName : "(nobody yet)";
                    }

                    var ODid = delivery.ODId;
                    if (!ODid.IsNullOrEmpty() && ODid != "0")
                    {
                        var user = db.Users.Find(ODid);
                        if (user != null) deliveryView.ODName = user.FullName;
                    }

                    ;
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
                    deliveryView.Email = client.Email;
                    deliveryView.EmailToolTip = client.Email.Replace(" ", "\u00a0");
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
                        s = deliveryView.DriverNotes; // For display, abbreviate to 12 characters:           
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
                    s = deliveryView.Email; // For display, abbreviate to 15 characters:           
                    s = s.Length <= 15 ? s : s.Substring(0, 15) + "...";
                    deliveryView.Email = s;
                    listDeliveryViewModels.Add(deliveryView);
                }
            }

            return View(listDeliveryViewModels);
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult OpenDeliveriesToCSV()
        {
            var result = AppRoutines.OpenDeliveriesToCSV(null);
            return result;
        }

        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult ExcelOpenDeliveries()
        {
            var result = AppRoutines.ExcelOpenDeliveries(null);
            return result;
        }
       
        // GET: Open Delivery Filters
        [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        public ActionResult OpenFilters(string btnAllCheckAll)
        {
            using var db = new BHelpContext();
            var listAllOpenDeliveries = db.Deliveries.Where(d => d.Status == 0)
                .OrderBy(d => d.DateDelivered).ThenBy(z => z.Zip)
                .ThenBy(n => n.LastName).ToList(); // get all open deliveries
            var view = new OpenDeliveryViewModel()
            {
                OpenDeliveryCount = listAllOpenDeliveries.Count,
                DistinctDeliveryDatesList = new List<string>(),
                DistinctDeliveryDatesSelectList = new List<SelectListItem>(),
                SelectedDeliveriesList = new List<Delivery>(),
                DistinctDriverList = new List<string>(),
                DistinctDriversSelectList = new List<SelectListItem>(),
                DistinctDeliveryDatesODList = new List<SelectListItem>(),
                ReplacementDeliveryDate = DateTime.Today,
                DriversSelectList = new List<SelectListItem>(),
                ODSelectList = new List<SelectListItem>()
            };

            foreach (var openD in listAllOpenDeliveries)
            {
                var del = db.Deliveries.Find(openD.Id);
                if (del != null)
                {
                    openD.DeliveryDateODName = del.FirstName + " " + del.LastName;
                }
            }

            var distinctDatesList = listAllOpenDeliveries.Select(d => d.DateDelivered).Distinct().ToList();
            foreach (var dt in distinctDatesList)
            {
                int delThisDateCount = listAllOpenDeliveries.Count(d => d.DateDelivered == dt);
                view.DistinctDeliveryDatesList.Add(dt == null
                    ? "-none-  (" + delThisDateCount + ")"
                    : dt.Value.ToString("MM/dd/yyyy") + " (" + delThisDateCount + ")");

                if (dt != null)
                {
                    // Get delDate ODIDs for each distinct del date
                    foreach (var del in listAllOpenDeliveries)
                    {
                        if (del.DateDelivered == dt && del.DeliveryDateODName != null)
                            view.DistinctDeliveryDatesODList.Add(new SelectListItem()
                                { Value = dt.Value.ToString("MM/dd/yyyy"), Text = del.DeliveryDateODName });
                    }

                    view.DistinctDeliveryDatesSelectList.Add(new SelectListItem()
                        { Value = dt.Value.ToString("MM/dd/yyyy"), Text = dt.Value.ToString("MM/dd/yyyy") });
                }
            }

            Session["DistinctDeliveryDatesList"] = view.DistinctDeliveryDatesList;
            Session["DistinctDeliveryDatesSelectList"] = view.DistinctDeliveryDatesSelectList;

            var distinctDriverIdList = listAllOpenDeliveries.Select(d => d.DriverId).Distinct().ToList();
            foreach (var drId in distinctDriverIdList)
            {
                int delCountThisDriver = listAllOpenDeliveries.Count(d => d.DriverId == drId);

                var driver = db.Users.Find(drId);
                if (driver != null)
                {
                    var driverName = driver.FullName + " (" + delCountThisDriver + ")";
                    view.DistinctDriverList.Add(driverName);
                    view.DistinctDriversSelectList.Add(new SelectListItem { Value = drId, Text = driverName });
                }
                else
                {
                    var driverName = "(nobody yet)" + " (" + delCountThisDriver + ")";
                    view.DistinctDriversSelectList.Add(new SelectListItem { Value = "0", Text = driverName });
                }
            }

            TempData["DistinctDriverList"] = view.DistinctDriverList;
            TempData["DistinctDriversSelectList"] = view.DistinctDriversSelectList;

            view.DriversSelectList = AppRoutines.GetDriversSelectList();
            view.ReplacementDriverId = view.DriversSelectList[0].Value;
            TempData["DriversSelectList"] = view.DriversSelectList;

            view.ODSelectList = AppRoutines.GetODSelectList();
            view.ReplacementDeliveryDateODId = view.ODSelectList[0].Value;
            TempData["ODSelectList"] = view.ODSelectList;

            if (btnAllCheckAll != "True") return View(view);
            {
                var selectedDeliveries = db.Deliveries
                    .Where(d => d.Status == 0).OrderBy(d => d.DateDelivered)
                    .ThenBy(z => z.Zip).ThenBy(n => n.LastName).ToList();
                view = LoadSelectedDeliveriesIntoView(view, selectedDeliveries, btnAllCheckAll);
                view.ButtonGroupName = "All";
            }
            return View(view);
        }

        // POST: 
        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
        [ValidateAntiForgeryToken]
        public ActionResult OpenFilters(OpenDeliveryViewModel model,
            string btnAllCheckAll, string btnAllClearAll,
            string btnByDateCheckAll, string btnByDateClearAll,
            string btnByDriverCheckAll, string btnByDriverClearAll,
            string btnReplacementDeliveryDate, string btnReplacementDriverId,
            string btnReplacementDeliveryDateODId, string btnSetStatusToDelivered,
            string btnExcelOpenSelected, string btnCSVOpenSelected)
        {
            ModelState.Clear(); // if not cleared, checkboxfor IsChecked displays incorrectly
            var view = GetOpenDeliveryViewModel(model);
            view.BtnAllCheckAll = btnAllCheckAll;
            view.BtnAllClearAll = btnAllClearAll;
            view.BtnByDateCheckAll = btnByDateCheckAll;
            view.BtnByDateClearAll = btnByDateClearAll;
            view.BtnByDriverCheckAll = btnByDriverCheckAll;
            view.BtnByDriverClearAll = btnByDriverClearAll;

            var db = new BHelpContext();
            if (btnAllCheckAll != null || btnAllClearAll != null)
            {
                var selectedDeliveries = db.Deliveries
                    .Where(d => d.Status == 0).OrderBy(d => d.DateDelivered)
                    .ThenBy(z => z.Zip).ThenBy(n => n.LastName).ToList();
                view = LoadSelectedDeliveriesIntoView(view, selectedDeliveries, btnAllCheckAll);
                view.ButtonGroupName = $"All";
                return View(view);
            }
            
            if (btnByDateCheckAll != null || btnByDateClearAll != null)
            {
                var selectedDeliveries = db.Deliveries.Where(d => d.Status == 0
                                                                  && d.DateDelivered ==
                                                                  model.SelectedDistinctDeliveryDate)
                    .OrderBy(z => z.Zip).ThenBy(n => n.LastName).ToList();
                view = LoadSelectedDeliveriesIntoView(view, selectedDeliveries, btnByDateCheckAll);
                view.ButtonGroupName = $"ByDate";
                return View(view);
            }

            if (btnByDriverCheckAll != null || btnByDriverClearAll != null)
            {
                var selDistinctDriverId = model.SelectedDistinctDriverId;
                if (selDistinctDriverId == "0")
                {
                    selDistinctDriverId = null;
                }
                
                var selectedDeliveries = db.Deliveries
                    .OrderBy(d => d.DateDelivered).ThenBy(z => z.Zip)
                    .ThenBy(n => n.LastName)
                    .Where(d => d.Status == 0 && d.DriverId == selDistinctDriverId).ToList();
                view = LoadSelectedDeliveriesIntoView(view, selectedDeliveries, btnByDriverCheckAll);
                view.ButtonGroupName = $"ByDriver";
                return View(view);
            }
            
            if (btnReplacementDeliveryDate != null)
            {
                var selectedDeliveries = (List<Delivery>)TempData["SelectedDeliveriesList"];
                if (selectedDeliveries != null)
                {
                    for (var i = 0; i < selectedDeliveries.Count; i++)
                    {
                        // selected deliveries count may have changed
                        if (i < model.SelectedDeliveriesList.Count)
                        {
                            selectedDeliveries[i].IsChecked = model.SelectedDeliveriesList[i].IsChecked;
                        }
                    }

                    foreach (var rec in selectedDeliveries)
                    {
                        if (rec.IsChecked)
                        {
                            db.SetDeliveryDate(rec.Id, view.ReplacementDeliveryDate);
                        }
                    }

                    return RedirectToAction("OpenFilters", new { btnCheckAll = "True" });
                }
            }

            if (btnReplacementDriverId != null)
            {
                var selectedDeliveries = (List<Delivery>)TempData["SelectedDeliveriesList"];
                for (var i = 0; i < selectedDeliveries.Count; i++)
                {
                    // selected deliveries count may have changed
                    if (i < model.SelectedDeliveriesList.Count)
                    {
                        selectedDeliveries[i].IsChecked = model.SelectedDeliveriesList[i].IsChecked;
                    }
                }
                
                foreach (var rec in selectedDeliveries)
                {
                    if (rec.IsChecked)
                    {
                        if (model.ReplacementDriverId != null)
                        {
                            if (model.ReplacementDriverId == "0" 
                                || model.ReplacementDriverId.Contains("nobody"))
                                view.ReplacementDriverId = null;
                        }

                        db.SetDeliveryDriver(rec.Id, view.ReplacementDriverId);
                    }
                }

                return RedirectToAction("OpenFilters", new { btnCheckAll = "True" });
            }

            if (btnReplacementDeliveryDateODId != null)
            {
                var selectedDeliveries = (List<Delivery>)TempData["SelectedDeliveriesList"];
                for (var i = 0; i < selectedDeliveries.Count; i++)
                {
                    // selected deliveries count may have changed
                    if (i < model.SelectedDeliveriesList.Count)
                    {
                        selectedDeliveries[i].IsChecked = model.SelectedDeliveriesList[i].IsChecked;
                    }
                }

                foreach (var rec in selectedDeliveries)
                {
                    if (rec.IsChecked)
                    {
                        if (model.ReplacementDeliveryDateODId != null)
                        {
                            if (model.ReplacementDeliveryDateODId == "0"
                                || model.ReplacementDeliveryDateODId.Contains("nobody"))
                                model.ReplacementDeliveryDateODId = null;
                        }

                        db.SetDeliveryDateODId(rec.Id, model.ReplacementDeliveryDateODId);
                    }
                }

                return RedirectToAction("OpenFilters", new { btnCheckAll = "True" });
            }

            if (btnSetStatusToDelivered != null)
            {
                List<Delivery> selectedDeliveries = (List<Delivery>)TempData["SelectedDeliveriesList"];
                for (var i = 0; i < selectedDeliveries.Count; i++)
                {
                    // selected deliveries count may have changed
                    if (i < model.SelectedDeliveriesList.Count)
                    {
                        selectedDeliveries[i].IsChecked = model.SelectedDeliveriesList[i].IsChecked;
                    }
                }
                
                foreach (var rec in selectedDeliveries)
                {
                    if (rec.IsChecked)
                    {
                        var dlv = db.Deliveries.Find(rec.Id);
                        if (view.ReplacementDriverId == "0") view.ReplacementDriverId = null;
                        if (dlv != null)
                        {
                            // Don't mark as delivered if no products:
                            if (rec.FullBags > 0 || rec.HalfBags > 0 || rec.KidSnacks > 0 || rec.GiftCards > 0)
                            {
                                db.SetDeliveryStatus(rec.Id, 1);
                            }
                        }
                    }
                }

                return RedirectToAction("OpenFilters", new { btnCheckAll = "True" });
            }

            if (btnExcelOpenSelected != null)
            {
                // Change TempData to Session
                Session["SelectedDeliveriesList"] = TempData["SelectedDeliveriesList"];
                var selectedDeliveries = (List<Delivery>)Session["SelectedDeliveriesList"];
                if (selectedDeliveries == null) return null;

                var selectedOpens = GetSelectedOpens(model);
                var result = AppRoutines.ExcelOpenDeliveries(selectedOpens);

                return result;
            }

            if (btnCSVOpenSelected != null)
            {
                Session["SelectedDeliveriesList"] = TempData["SelectedDeliveriesList"];
                var selectedDeliveries = (List<Delivery>)Session["SelectedDeliveriesList"];
                if (selectedDeliveries == null) return null;

                var selectedOpens = GetSelectedOpens(model);
                var result = AppRoutines.OpenDeliveriesToCSV(selectedOpens);
                return result;
            }

            return RedirectToAction("OpenFilters");
            }
        

        private OpenDeliveryViewModel GetSelectedOpens(OpenDeliveryViewModel model)
            {
                var selectedDeliveries = (List<Delivery>)TempData["SelectedDeliveriesList"];
                if (selectedDeliveries == null) return null;
                var odv = new OpenDeliveryViewModel
                {
                    SelectedDeliveriesList = new List<Delivery>(),
                    OpenDeliveries = new string[selectedDeliveries.Count, 20],
                    ReportTitle = "Bethesda Help Open Deliveries",
                    OpenDeliveryCount = selectedDeliveries.Count
                };
                for (var i = 0; i < selectedDeliveries.Count; i++)
                {
                    // selected deliveries count may have changed
                    if (i < model.SelectedDeliveriesList.Count)
                    {
                        selectedDeliveries[i].IsChecked = model.SelectedDeliveriesList[i].IsChecked;
                    }
                } // Set IsChecked flags

                var checkedSelectedDeliveries = new List<Delivery>();
                var j = -1;
                foreach (var rec in selectedDeliveries)
                {
                    if (rec.IsChecked)
                    {
                        j++;
                        if (rec.DateDelivered != null)
                        {
                            odv.OpenDeliveries[j, 1] = rec.DateDelivered.Value.ToString("MM/dd/yyyy");
                        }

                        odv.OpenDeliveries[j, 2] = rec.DriverName;
                        odv.OpenDeliveries[j, 3] = rec.Zip;
                        odv.OpenDeliveries[j, 4] = rec.Client.FullName;
                        odv.OpenDeliveries[j, 5] = rec.StreetNumber + " " + rec.StreetName;
                        odv.OpenDeliveries[j, 6] = rec.City;
                        odv.OpenDeliveries[j, 7] = rec.Phone;

                    var familyMembers = AppRoutines.GetFamilyMembers(rec.ClientId);
                    var kidCount = AppRoutines.GetNumberOfChildren(rec.Client.Id);
                        odv.OpenDeliveries[j, 8] = kidCount.ToString();
                        odv.OpenDeliveries[j, 9] = AppRoutines.GetNumberOfAdults(rec.Client.Id).ToString();
                        odv.OpenDeliveries[j, 10] = AppRoutines.GetNumberOfSeniors(rec.Client.Id).ToString();
                        odv.OpenDeliveries[j, 11] = familyMembers.Count.ToString();
                        odv.OpenDeliveries[j, 12] = AppRoutines.GetNamesAgesOfAllInHousehold(rec.Client.Id);
                        odv.OpenDeliveries[j, 13] = rec.FullBags.ToString();
                        odv.OpenDeliveries[j, 14] = rec.HalfBags.ToString();
                        odv.OpenDeliveries[j, 15] = rec.KidSnacks.ToString();
                        odv.OpenDeliveries[j, 16] = rec.GiftCards.ToString();
                        odv.OpenDeliveries[j, 17] = rec.Client.Notes;
                        odv.OpenDeliveries[j, 18] = rec.ODNotes + " " + rec.DriverNotes;

                        checkedSelectedDeliveries.Add(rec); // for get distinct OD list
                    }
                }

                odv.DistinctDeliveryDatesODList =
                    AppRoutines.GetDistinctDeliveryDatesOdList(checkedSelectedDeliveries);
                odv.OpenDeliveryCount = j + 1;
                odv.ReportTitle = "BHELP Deliveries";

                return odv;
            }

            private OpenDeliveryViewModel LoadSelectedDeliveriesIntoView(OpenDeliveryViewModel view,
                List<Delivery> selectedDeliveries, string btnCheckAll)
            {
                var duplicateClientIds = from x in selectedDeliveries 
                    group x by x.ClientId into g
                    let count = g.Count()
                    select new { Count = count, Id = g.First().ClientId };

                foreach (var del in selectedDeliveries)
                {
                    del.IsChecked = btnCheckAll != null;

                    del.DateDeliveredString = $"{del.DateDelivered:MM/dd/yyyy}";
                    del.DeliveryDateODName = AppRoutines.GetUserFullName(del.DeliveryDateODId);
                    del.DriverName = AppRoutines.GetUserFullName(del.DriverId);
                    del.Client = GetClientData(del.ClientId);
                    del.EligiibilityRulesException = false;

                    // Check for EligiibilityRulesException:
                    var nextDeliveryEligibleDate = AppRoutines.GetNextEligibleDeliveryDate
                        (del.ClientId, DateTime.Today);
                    if (del.DateDelivered < nextDeliveryEligibleDate)
                        del.EligiibilityRulesException = true;

                    // ReSharper disable once PossibleMultipleEnumeration
                    var clientIds = duplicateClientIds.ToList();
                    foreach (var dup in clientIds)
                    {
                        if (dup.Id == del.ClientId && dup.Count > 1)
                        {
                            del.EligiibilityRulesException = true;
                        }
                    }
                    
                    if (del.GiftCards > 0)
                    {
                        var dateLastGiftCard = AppRoutines.GetDateLastGiftCard(del.ClientId);
                        var nextGiftCardEligibleDate = AppRoutines.GetNextGiftCardEligibleDate
                            (del.ClientId, dateLastGiftCard);
                        var dt = del.DateDelivered ?? DateTime.Now;
                        var giftCardsEligible = AppRoutines.GetGiftCardsEligible(del.ClientId, dt);
                        if (del.DateDelivered < nextGiftCardEligibleDate || del.GiftCards > giftCardsEligible)
                            del.EligiibilityRulesException = true;
                    }

                    if (del.FullBags == 0 && del.HalfBags == 0 && del.KidSnacks == 0 && del.GiftCards == 0)
                    {
                        del.AllZeroProducts = true;
                    }

                    view.SelectedDeliveriesList.Add(del);
                }

                view.DriversSelectList = TempData["DriversSelectList"] as List<SelectListItem>;
                TempData.Keep("DriversSelectList");
                view.ODSelectList = TempData["ODSelectList"] as List<SelectListItem>;
                TempData.Keep("ODSelectList");
                TempData["SelectedDeliveriesList"] = selectedDeliveries;
                TempData.Keep("SelectedDeliveriesList");
                return view;
            }
            private OpenDeliveryViewModel GetOpenDeliveryViewModel(OpenDeliveryViewModel view)
            {
                using var db = new BHelpContext();
                var listAllOpenDeliveries = db.Deliveries.Where(d => d.Status == 0)
                    .OrderBy(d => d.DateDelivered).ThenBy(z => z.Zip)
                    .ThenBy(n => n.StreetNumber).ThenBy(s => s.StreetName) // added 10/13/2022
                    .ThenBy(n => n.LastName).ToList(); // get all open deliveries
                if (view.ReplacementDriverId == "0") view.ReplacementDriverId = null;
                var newView = new OpenDeliveryViewModel
                {
                    OpenDeliveryCount = listAllOpenDeliveries.Count,
                    DistinctDeliveryDatesList = Session["DistinctDeliveryDatesList"] as List<string>,
                    SelectedDeliveriesList = new List<Delivery>(),
                    DistinctDeliveryDatesSelectList =
                        Session["DistinctDeliveryDatesSelectList"] as List<SelectListItem>,
                    DistinctDriverList = TempData["DistinctDriverList"] as List<string>,
                    DistinctDriversSelectList = TempData["DistinctDriversSelectList"] as List<SelectListItem>,
                    ReplacementDeliveryDate = view.ReplacementDeliveryDate,
                    ReplacementDriverId = view.ReplacementDriverId,
                    DriversSelectList = TempData["DriversSelectList"] as List<SelectListItem>,
                    ODSelectList = TempData["OdSelectList"] as List<SelectListItem>
                };
                if (view.ReplacementDeliveryDate == DateTime.MinValue)
                {
                    newView.ReplacementDeliveryDate = DateTime.Today;
                }

                TempData.Keep("DistinctDeliveryDatesList");
                TempData.Keep("DistinctDriverList");
                TempData.Keep("DriversSelectList");
                TempData.Keep("ODSelectList");

                var newSort = false;
                if (newView.DistinctDeliveryDatesSelectList != null)

                    foreach (var dt in newView.DistinctDeliveryDatesSelectList)
                    {
                        if (dt.Value == view.SelectedDistinctDeliveryDate.ToString("MM/dd/yyyy"))
                        {
                            dt.Selected = true;
                            var dts = Convert.ToDateTime(dt.Value);
                            newSort = true;
                            newView.SelectedDistinctDeliveryDate = dts;
                            break;
                        }
                    }

                if (newSort) // newSort needed because DropDownListFor doesn't respond to .Selected
                {
                    // put selected value at top of list
                    var newSortedList = new List<SelectListItem>();
                    foreach (var dt in newView.DistinctDeliveryDatesSelectList)
                    {
                        if (dt.Selected)
                        {
                            newSortedList.Add(dt);
                            break;
                        }
                    }

                    foreach (var dt in newView.DistinctDeliveryDatesSelectList)
                    {
                        if (!dt.Selected)
                        {
                            newSortedList.Add(dt);
                        }
                    }

                    newView.DistinctDeliveryDatesSelectList = newSortedList;
                }

                Session["DistinctDeliveryDatesSelectList"] = newView.DistinctDeliveryDatesSelectList;

                if (newView.DistinctDriversSelectList != null)
                {
                    foreach (var dr in newView.DistinctDriversSelectList)
                    {
                        if (view.SelectedDistinctDriverId != null &&
                            view.SelectedDistinctDriverId.Contains("nobody"))
                        {
                            view.SelectedDistinctDriverId = null;
                        }

                        if (dr.Value == view.SelectedDistinctDriverId)
                        {
                            dr.Selected = true;
                            break;
                        }
                    }
                }

                TempData["DistinctDriversSelectList"] = newView.DistinctDriversSelectList;
            
                return newView;
            }

            private Client GetClientData(int id)
            {
                var _client = AppRoutines.GetClientRecord(id);
                if (_client != null)
                {
                    _client.ClientNameAddress = _client.LastName + ", " + _client.FirstName
                                                + " " + _client.StreetNumber + " " + _client.StreetName;
                    _client.NameAddressToolTip = _client.ClientNameAddress.Replace(" ", "\u00a0");
                    var s = _client.ClientNameAddress;
                    s = s.Length <= 30 ? s : s.Substring(0, 30) + "...";
                    _client.ClientNameAddress = s;
                    return _client;
                }

                return null;
            }

            // GET: Deliveries/Edit/5
            [HttpGet, Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult Edit(int? id, string returnURL,
                DateTime? _NewDateDelivered, string _ODId, string _DeliveryDateODId,
                string _ODNotes, string _DriverNotes, string _Zip,
                string _Status, int? _FullBags, int? _HalfBags, int? _KidSnacks,
                int? _GiftCards, int? _HolidayGiftCards,
                DateTime? _HistoryStartDate, DateTime? _HistoryEndDate )
            {
                if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                
                // ================= routine to handle DateDelivered change:
                if (_NewDateDelivered != null)
                {
                    var _view = (DeliveryViewModel)Session["CurrentDeliveryViewModel"];
                    _view.DateDelivered = _NewDateDelivered;
                    if(_ODId != null) _view.ODId = _ODId;
                    if(_DeliveryDateODId != null) _view.DeliveryDateODId = _DeliveryDateODId;
                    if(_ODNotes != null) _view.ODNotes = _ODNotes;
                    if (_DriverNotes != null) _view.DriverNotes = _DriverNotes;
                    if (_Zip != null) _view.Zip = _Zip;
                    if (_Status != null) _view.SelectedStatus = _Status;
                    if (_FullBags != null) _view.FullBags = (int)_FullBags;
                    if (_HalfBags != null) _view.HalfBags = (int)_HalfBags;
                    if (_KidSnacks != null) _view.KidSnacks = (int)_KidSnacks;
                    if (_GiftCards != null) _view.GiftCards = (int)_GiftCards;
                    if (_HolidayGiftCards != null) _view.HolidayGiftCards = (int)_HolidayGiftCards;
                    if (_HistoryStartDate != null) _view.HistoryStartDate = _HistoryStartDate;
                    if (_HistoryEndDate != null) _view.HistoryEndDate = _HistoryEndDate;

                    var odSched = AppRoutines.GetODSchedule((DateTime)_NewDateDelivered);
                    if (odSched == null)
                    {
                        _view.DeliveryDateODId = null; // (nobody yet)
                    }
                    else
                    {
                        _view.DeliveryDateODId = odSched.ODId;
                    }

                    _view.DriverId = null; // (nobody yet)

                    // Reset selected item in DeliveryDateODList
                    foreach (var item in _view.DeliveryDateODList)
                    {
                        if (item.Value == _view.DeliveryDateODId)
                        {
                            item.Selected = true;
                        }
                        else
                        {
                            item.Selected = false;
                        }
                    }

                    // Reset selected item in DriversList
                    foreach (var item in _view.DriversList)
                    {
                        if (item.Value == _view.DriverId)
                        {
                            item.Selected = true;
                        }
                        else
                        {
                            item.Selected = false;
                        }
                    }

                    // Reset selected item in ZipCodes
                    foreach (var item in _view.ZipCodes)
                    {
                        if (item.Value == _view.Zip)
                        { 
                            item.Selected = true;
                        }
                        else
                        {
                            item.Selected = false;
                        }
                    }

                    Session["CurrentDeliveryViewModel"] = _view;
                    return View(_view);
                } //===================== ( _NewDateDelivered != null) =============
           
                Session["CurrentDeliveryId"] = id.ToString();
                var delivery = AppRoutines.GetDeliveryRecord((int)id);
                if (delivery == null) return HttpNotFound();

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
                    DriverName = AppRoutines.GetUserFullName(delivery.DriverId),
                    DriverNotes = delivery.DriverNotes,
                    DriversList = AppRoutines.GetDriversSelectList(),
                    ClientSelectList =ClientRoutines .GetClientSelectList((int)id),
                    NamesAgesInHH = delivery.NamesAgesInHH,
                    SnapshotFamily = GetSnapshotFamily(delivery.NamesAgesInHH),
                    FamilySelectList = AppRoutines.GetFamilySelectList(delivery.ClientId),
                    DatePriorDelivery = AppRoutines.GetPriorDeliveryDate(delivery.ClientId, delivery.LogDate),
                    DateLastDelivery = GetLastGetDeliveryDate(delivery.Id),
                    DateDelivered = delivery.DateDelivered,
                    Status = delivery.Status,
                    HistoryStartDate = Convert.ToDateTime(Session["CallLogStartDate"]),
                    HistoryEndDate = Convert.ToDateTime(Session["CallLogEndDate"]),
                    ReturnURL = returnURL
                };
                viewModel.DeliveryDateODList = viewModel.ODList;

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
                {
                    var retUrl = Request.UrlReferrer.ToString();
                    if (retUrl.Contains('?'))
                    {
                        var i = retUrl.IndexOf('?');
                        retUrl = retUrl.Substring(0, i);
                    }
                    viewModel.ReturnURL =retUrl;
                }

                foreach (var item in viewModel.DriversList)
                {
                    if (item.Value == viewModel.DriverId)
                    {
                        item.Selected = true;
                        break;
                    }
                }

                foreach (var item in viewModel.ODList)
                {
                    if (item.Value == viewModel.ODId)
                    {
                        item.Selected = true;
                        break;
                    }
                }

                foreach (var item in viewModel.ZipCodes)
                {
                    if (item.Value == viewModel.Zip)
                    {
                        item.Selected = true;
                        break;
                    }
                }
                
                foreach (var item in viewModel.DeliveryDateODList)
                {
                    if (item.Value == viewModel.DeliveryDateODId)
                    {
                        item.Selected = true;
                        break;
                    }
                }

                viewModel.KidsCount = delivery.Children;
                viewModel.AdultsCount = delivery.Adults;
                viewModel.SeniorsCount = delivery.Seniors;

                viewModel.FullBags = delivery.FullBags;
                viewModel.HalfBags = delivery.HalfBags;
                viewModel.KidSnacks = delivery.KidSnacks;
                viewModel.GiftCards = delivery.GiftCards;
                viewModel.HolidayGiftCards = delivery.HolidayGiftCards;
                viewModel.GiftCardsEligible = delivery.GiftCardsEligible;

                var client = AppRoutines.GetClientRecord(delivery.ClientId);
                //db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    viewModel.Client = client;
                    viewModel.ClientNameAddress = client.LastName + ", " + client.FirstName
                                                  + " " + client.StreetNumber + " " + client.StreetName + " " +
                                                  client.Zip;
                    viewModel.Phone = client.Phone;
                    viewModel.Email = client.Email;
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
                else
                {
                    viewModel.FirstName = delivery.FirstName;
                    viewModel.LastName = delivery.LastName;
                    return RedirectToAction("ClientNotFound", viewModel);
                }

                viewModel.Zip = delivery.Zip;
                Session["CurrentDeliveryViewModel"] = viewModel; // save for returning with new DateDelivered
                return View(viewModel);
            }

            public ActionResult ClientNotFound (DeliveryViewModel view)
            {
                using var db = new BHelpContext();
                var clientSelectList = new List<SelectListItem>();
                var clientList = db.Clients.OrderBy(c => c.LastName).ToList();
                const int maxTextLength = 70;
                foreach (var client in clientList)
                {
                    var text = client.LastName + ", " + client.FirstName + " ";
                    text += client.StreetNumber + " " + client.StreetName;
                    if (text.Length > maxTextLength) text = text.Substring(0, maxTextLength) + "...";
                    var selListItem = new SelectListItem()
                        { Value = client.Id.ToString(), Text = text, Selected = false };
                    clientSelectList.Add(selListItem);
                }

                view.ClientSelectList = clientSelectList;
                view.DateDeliveredString = $"{view.DateDelivered:MM/dd/yyyy}";
                return View(view);
            }

            public ActionResult DeleteDelivery (int _id, string _returnURL)
            {
                using var db = new BHelpContext();
                var delivery = db.Deliveries.Find(_id);
                if (delivery != null) db.Deliveries.Remove(delivery);
                db.SaveChanges();

                if (_returnURL.Contains("UpdateHousehold"))
                {
                    return RedirectToAction("Index", "OD");
                }

                if (_returnURL.Contains("CallLogByLogDate"))
                {
                    return RedirectToAction("CallLogByLogDate");
                }

                if (_returnURL.Contains("CallLogIndividual"))
                {
                    return RedirectToAction("CallLogIndividual");
                }

                return RedirectToAction("Index");
            }
        
            public ActionResult AdviseCannotSave(int _id)
            {  // (because status = 1 and all zero products)
                var view = new DeliveryViewModel { Id = _id };  
                return View(view);
            }

            public ActionResult AdviseODIdRequired(int _id)
            {  // (because Delivery Date OD not selected.)
                var view = new DeliveryViewModel { Id = _id };
                return View(view);
            }

            // POST: Deliveries/Edit/5
            [HttpPost,Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            [ValidateAntiForgeryToken]
            public ActionResult EditPost(DeliveryViewModel delivery)
            {
                // DriverId and DeliveryDateODId are used in Edit dropdowns and return a
                // text value of null when 'nobody yet' is selected:
                if (delivery.DeliveryDateODId == null)
                {
                    // Save any updates:
                    //var rec = AppRoutines.GetDeliveryRecord(delivery.Id);
                    using var _db = new BHelpContext();
                    {
                        var rec = _db.Deliveries.FirstOrDefault(i => i.Id == delivery.Id);
                        if (rec != null)
                        {
                            rec.DateDelivered = delivery.DateDelivered;
                            rec.LogDate = delivery.LogDate;
                            rec.FullBags = delivery.FullBags;
                            rec.HalfBags = delivery.HalfBags;
                            rec.KidSnacks = delivery.KidSnacks;
                            rec.HolidayGiftCards = delivery.HolidayGiftCards;
                            rec.GiftCards = delivery.GiftCards;
                            rec.ODId = delivery.ODId;
                            rec.Status = delivery.Status;
                            rec.ODNotes = delivery.ODNotes;
                            rec.DriverNotes = delivery.DriverNotes;
                            rec.DeliveryDateODId = delivery.DeliveryDateODId;
                            rec.DriverId = delivery.DriverId;
                            rec.Zip = delivery.Zip;
                            _db.SaveChanges();
                        }
                    }
                    // Reminder error - ODId required: 
                    return RedirectToAction("AdviseODIdRequired", new { _id = delivery.Id });
                }

                using var db = new BHelpContext();
                if (ModelState.IsValid)
                {
                    var updateData = db.Deliveries.Find(delivery.Id);

                    if (updateData != null)
                    {
                        var client = db.Clients.Find(updateData.ClientId);
                        if (client != null) updateData.Zip = client.Zip;
                        updateData.DateDelivered = delivery.DateDelivered;
                        updateData.LogDate = delivery.LogDate;
                        updateData.FullBags = delivery.FullBags;
                        updateData.HalfBags = delivery.HalfBags;
                        updateData.KidSnacks = delivery.KidSnacks;
                        updateData.GiftCards = delivery.GiftCards;
                        updateData .HolidayGiftCards= delivery.HolidayGiftCards;    
                        updateData.GiftCardsEligible = delivery.GiftCardsEligible;
                        updateData.ODNotes = delivery.ODNotes;
                        updateData.DriverId = delivery.DriverId;
                        updateData.ODId = delivery.ODId;
                        updateData.DeliveryDateODId = delivery.DeliveryDateODId;
                        updateData.DriverNotes = delivery.DriverNotes;
                        updateData.DateDelivered = delivery.DateDelivered;
                        updateData.SelectedStatus = delivery.SelectedStatus;
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

                        if (updateData.Status == 1 && updateData.FullBags == 0 && updateData.HalfBags == 0
                            && updateData.KidSnacks == 0 && updateData.GiftCards == 0
                            && updateData.HolidayGiftCards == 0)
                        {
                            // Cannot save delivery as completed with zero products: 
                            return RedirectToAction("AdviseCannotSave", new { _id = delivery.Id });
                        }

                        db.Entry(updateData).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (delivery.ReturnURL.Contains("CallLogIndividual"))
                    {
                        if (updateData != null)
                            return RedirectToAction("CallLogIndividual", new { clientId = updateData.ClientId });
                    }

                    if (delivery.ReturnURL.Contains("CallLogByLogDate"))
                    {
                        return RedirectToAction("CallLogByLogDate",
                            new { startDate = Session["CallLogStartDate"], endDate = Session["CallLogEndDate"] });
                    }

                    if (delivery.ReturnURL.Contains("CallLogByDateDelivered"))
                    {
                        return RedirectToAction("CallLogByDateDelivered");
                    }

                    if (delivery.ReturnURL.Contains("UpdateHousehold"))
                    {
                        return RedirectToAction("Index", "OD");
                    }

                    return RedirectToAction("Index");
                }

                return View(delivery);
            }
        
            // GET: Deliveries/Delete/5
            [Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay")]
            public ActionResult Delete(int? id, string returnURL)
            {
                if (id == null)
                { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }

                using var db = new BHelpContext();
                var delivery = db.Deliveries.Find(id);
                if (delivery == null)
                {
                    return HttpNotFound();
                }

                delivery.ReturnURL = returnURL;
                return View(delivery);
            }

            [HttpPost]
            public ActionResult Assign(DeliveryViewModel model)
            {
                var _id = model.Id;
                if (_id == 0)
                { return new HttpStatusCodeResult(HttpStatusCode.BadRequest); }

                using var db = new BHelpContext();
                var delivery = db.Deliveries.Find(_id);

                if (delivery == null)
                {
                    return HttpNotFound();
                }

                delivery.ClientId = model.ClientId;
                db.SaveChanges();

                if (model.ReturnURL.Contains("UpdateHousehold"))
                {
                    return RedirectToAction("Index", "OD");
                }

                if (model.ReturnURL.Contains("CallLogByLogDate"))
                {
                    return RedirectToAction("CallLogByLogDate");
                }

                if (model.ReturnURL.Contains("CallLogIndividual"))
                {
                    return RedirectToAction("CallLogIndividual");
                }

                return RedirectToAction("Index");
            }

            // POST: Deliveries/Delete/5
            [HttpPost, Authorize(Roles = "Administrator,Staff,Developer,OfficerOfTheDay"), ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public ActionResult DeleteConfirmed(int id, string returnURL)
            {
                return RedirectToAction("DeleteDelivery", new {_id = id, _returnURL = returnURL});
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult CallLogMenu()
            {
                return View();
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult CallLogIndividual(int? clientId)
            {
                using var db = new BHelpContext();
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
                {
                    return View(callLogView);
                }

                var deliveryList = db.Deliveries.Where(d => d.ClientId == clientId)
                    .OrderByDescending(d => d.DateDelivered).ToList();
                callLogView.DeliveryList = deliveryList;
                callLogView.ClientId = (int)clientId;

                foreach (var del in callLogView.DeliveryList)
                {
                    del.ODName = AppRoutines.GetUserFullName(del.ODId);
                    del.DeliveryDateODName = AppRoutines.GetUserFullName(del.DeliveryDateODId);
                    del.DriverName = AppRoutines.GetUserFullName(del.DriverId);
                    if (del.Status == 1) // Show delivery date
                    {
                        if (del.DateDelivered.HasValue)
                        {
                            del.DateDeliveredString = $"{del.DateDelivered:MM/dd/yyyy}";
                        }
                    } // Show delivery date

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

                    var familyList = AppRoutines.GetFamilyMembers(del.ClientId); // includes HH
                    if (familyList != null)
                    {
                        del.Children = 0;
                        del.Seniors = 0;
                        del.Adults = 0;
                        del.HouseoldCount = familyList.Count();
                        foreach (var member in familyList)
                        {
                            if (member.Age <= 17) del.Children++;
                            if (member.Age >= 60) del.Seniors++;
                            if (member.Age is >= 18 and <= 59) del.Adults++;
                        }
                    }

                    var fullWeight = del.FullBags * 10 + del.HalfBags * 9;
                    del.PoundsOfFood = fullWeight;
                }

                if (User.IsInRole("Administrator") || User.IsInRole("Staff"))
                {
                    callLogView.OkToEdit = true;
                }

                Session["CallLogIndividualList"] = callLogView;
                return View(callLogView);
            }

            [HttpPost, Authorize(Roles = "Reports,Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult CallLogIndividual(string id)
            {
                if (id == "") id ="0";
                var intClientId = Convert.ToInt32(id);
                return RedirectToAction("CallLogIndividual", new { clientId = intClientId });
            }
            public void CallLogByIndividualToCSV(bool allData)
            {
                if (Session["CallLogIndividualList"] != null)
                {
                    var view = Session["CallLogIndividualList"] as DeliveryViewModel;
                    try
                    {
                        if (view != null)
                            view.ReportTitle = "CallLog " + view.DeliveryList[0].LastName
                                                          + view.DeliveryList[0].FirstName.Substring(0, 1)
                                                          + " " + DateTime.Today.ToString("MM-dd-yy");
                    }
                    catch (Exception)
                    {
                        if (view != null) view.ReportTitle = " CallLog" + DateTime.Today.ToString("MM-dd-yy");
                    }

                    AppRoutines.CallLogHistoryResultToCSV(view, allData);
                    Session["CallLogIndividualList"] = null;
                }
            }

            [Authorize(Roles = "Reports,Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult CallLogByLogDate(DateTime? startDate, DateTime? endDate )
            {
                using var db = new BHelpContext();
                Session["CallLogIndividualList"] = null;
                if (!startDate.HasValue || !endDate.HasValue) // default to today and 1 week ago
                {
                    startDate = DateTime.Today.AddDays(-7);
                    endDate = DateTime.Today;
                }

                var deliveries = db.Deliveries
                    .Where(d => d.LogDate >= startDate && d.LogDate <= endDate)
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
                    del.DriverName = AppRoutines.GetUserFullName(del.DriverId);
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

                    var familyList = AppRoutines.GetFamilyMembers(del.ClientId); // includes HH
                    if (familyList != null)
                    {
                        del.Children = 0;
                        del.Seniors = 0;
                        del.Adults = 0;
                        del.HouseoldCount = familyList.Count();
                        foreach (var member in familyList)
                        {
                            if (member.Age <= 17) del.Children++;
                            if (member.Age >= 60) del.Seniors++;
                            if (member.Age >= 18 && member.Age <= 59) del.Adults++;
                        }
                    }

                    if (del.Status == 2) //  kill any amounts if undelivered
                    {
                        del.FullBags = 0;
                        del.HalfBags = 0;
                        del.KidSnacks = 0;
                        del.GiftCards = 0;
                        del.HolidayGiftCards = 0;
                }

                var fullWeight = del.FullBags * 10 + del.HalfBags * 9;
                del.PoundsOfFood = fullWeight;
                callLogView.TotalHouseholdCount += del.HouseoldCount;
                callLogView.TotalChildren += del.Children;
                callLogView.TotalAdults += del.Adults;
                callLogView.TotalSeniors += del.Seniors;
                callLogView.TotalFullBags += del.FullBags;
                callLogView.TotalHalfBags += del.HalfBags;
                callLogView.TotalKidSnacks += del.KidSnacks;
                callLogView.TotalGiftCards += del.GiftCards;
                callLogView.TotalHolidayGiftCards += del.HolidayGiftCards;
                callLogView.TotalPoundsOfFood += del.PoundsOfFood;
                }

                Session["CallLogByLogDateList"] = callLogView;
                return View(callLogView);
            }
            public void CallLogByLogDateToCSV( bool allData)
            {
                if (Session["CallLogByLogDateList"] != null)
                {
                    var view = Session["CallLogByLogDateList"] as DeliveryViewModel;
                    try
                    {
                        if (view != null)
                        {
                            var startDate = (Convert.ToDateTime(Session["CallLogStartDate"])).ToString("MM-dd-yy");
                            var endDate= (Convert.ToDateTime(Session["CallLogEndDate"])).ToString("MM-dd-yy");

                            view.ReportTitle = "CallLog " + startDate + " to " + endDate;
                        }
                    }
                    catch (Exception)
                    {
                        if (view != null) view.ReportTitle = " CallLog" + DateTime.Today.ToString("MM-dd-yy");
                    }

                    AppRoutines.CallLogHistoryResultToCSV(view, allData);
                    Session["CallLogByLogDateList"] = null;
                }
            }
            public ActionResult CallLogByDateDelivered(DateTime? startDate, DateTime? endDate)
            {
                using var db = new BHelpContext();
                if (!startDate.HasValue || !endDate.HasValue) // default to today and 1 week ago
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
                    del.DriverName = AppRoutines.GetUserFullName( del.DriverId);
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

                    var familyList = AppRoutines.GetFamilyMembers(del.ClientId); // includes HH
                    if (familyList != null)
                    {
                        del.Children = 0;
                        del.Seniors = 0;
                        del.Adults = 0;
                        del.HouseoldCount = familyList.Count();
                        foreach (var member in familyList)
                        {
                            if (member.Age <= 17) del.Children++;
                            if (member.Age >= 60) del.Seniors++;
                            if (member.Age >= 18 && member.Age <= 59) del.Adults++;
                        }

                        if (del.Status == 2) //  kill any amounts if undelivered
                        {
                            del.FullBags = 0;
                            del.HalfBags = 0;
                            del.KidSnacks = 0;
                            del.GiftCards = 0;
                            del.HolidayGiftCards = 0;
                        }

                    var fullWeight = del.FullBags * 10 + del.HalfBags * 9;
                    del.PoundsOfFood = fullWeight;
                    }

                    callLogView.TotalHouseholdCount += del.HouseoldCount;
                    callLogView.TotalChildren += del.Children;
                    callLogView.TotalAdults += del.Adults;
                    callLogView.TotalSeniors += del.Seniors;
                    callLogView.TotalFullBags += del.FullBags;
                    callLogView.TotalHalfBags += del.HalfBags;
                    callLogView.TotalKidSnacks += del.KidSnacks;
                    callLogView.TotalGiftCards += del.GiftCards;
                    callLogView.TotalHolidayGiftCards += del.HolidayGiftCards;
                    callLogView.TotalPoundsOfFood += del.PoundsOfFood;
                }

                Session["CallLogByDateDeliveredList"] = callLogView;
                return View(callLogView);
            }

            public void  CallLogByDateDeliveredToCSV(bool allData)
            {
                if (Session["CallLogByDateDeliveredList"] != null)
                {
                    var view = Session["CallLogByDateDeliveredList"] as DeliveryViewModel;
                    try
                    {
                        if (view != null)
                        {
                            var startDate = (Convert.ToDateTime(Session["CallLogStartDate"])).ToString("MM-dd-yy");
                            var endDate = (Convert.ToDateTime(Session["CallLogEndDate"])).ToString("MM-dd-yy");

                            view.ReportTitle = "CallLog " + startDate + " to " + endDate;
                        }
                    }
                    catch (Exception)
                    {
                        if (view != null) view.ReportTitle = " CallLog" + DateTime.Today.ToString("MM-dd-yy");
                    }

                    AppRoutines.CallLogHistoryResultToCSV(view, allData);
                    Session["CallLogByDateDeliveredList"] = null;
                }
            }

            [Authorize(Roles = "Reports,Administrator,Staff,Developer")]
            public ActionResult CountyReport(string yy = "", string qtr = "")
            {
                int reportYear;
                int reportQuarter = 0;
                if (yy.IsNullOrEmpty() || qtr.IsNullOrEmpty())   // Default to this year, this quarter
                {
                    reportYear = Convert.ToInt32(DateTime.Now.Year.ToString());
                    var month = Convert.ToInt32(DateTime.Now.Month.ToString());
                    if (month is >= 1 and <= 3) { reportQuarter = 1;}     
                    if (month is >= 4 and <= 6) { reportQuarter = 2;}     
                    if (month is >= 7 and <= 9) { reportQuarter = 3; }    
                    if (month is >= 10 and <= 12) { reportQuarter = 4; }
                }
                else
                {
                    reportYear = Convert.ToInt32(yy);
                    reportQuarter = Convert.ToInt32(qtr);
                }

                var view = GetCountyReportView(reportYear, reportQuarter);
                return View(view);
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Reports")]
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
                { ws.Cell(activeRow, i + 2).SetValue(view.ZipCodes[i]).Style.Font.SetBold(true); }
                ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("Total Zip Codes")
                    .Style.Font.SetBold(true);
                activeRow++;
                ws.Cell(activeRow, 1).SetValue(view.MonthYear[0]).Style.Font.SetBold(true);
                activeRow++;
                ws.Cell(activeRow, 1).SetValue(view.MonthYear[1]).Style.Font.SetBold(true);
                
                activeRow++;
                for (var row = activeRow; row < activeRow + 10; row++) // First 10 data rows
                {
                    ws.Cell(row, 1).SetValue(view.HelperTitles[row - 4]);
                    for (var col = 1; col < view.ZipCodes.Count + 2; col++)
                    {
                        ws.Cell(row, col + 1).SetValue(view.ZipCounts[row - 4, col]);
                    }
                }

                activeRow += 11;
                const string dhText = "Distinct Households and Residents Served (NOT reported in the Helper)";
                ws.Cell(activeRow, 1).SetValue(dhText).Style.Font.SetBold();
                ws.Range(ws.Cell(activeRow, 1), ws.Cell(activeRow, 3)).Merge();

                activeRow++;
                for (var row = activeRow; row < activeRow + 9; row++) // Last 9 data rows
                {
                    ws.Cell(row, 1).SetValue(view.HelperTitles[row - 6]);
                    for (var col = 1; col < view.ZipCodes.Count + 2; col++)
                    {
                        ws.Cell(row, col + 1).SetValue(view.ZipCounts[row - 6, col]);
                    }
                }

                ws.Columns().AdjustToContents();
                var ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    { FileDownloadName = view.ReportTitle + ".xlsx" };
            }

            public ActionResult HelperReportToCSV(string yy = "", string mm = "")
            {
                var year = Convert.ToInt32(yy);
                var month = Convert.ToInt32(mm);
                var view = GetHelperReportView(year, month);
   
                var result = AppRoutines.HelperReportToCSV(view);
                return result;
            }

            private static ReportsViewModel GetHelperReportView(int year, int month)
            {
                var view = new ReportsViewModel {Year = year, Month = month};
                view.DateRangeTitle = DateTimeFormatInfo.CurrentInfo.GetMonthName(view.Month)
                                      + " " + view.Year.ToString() + " Delivery Totals";
                view.ReportTitle ="Bethesda Helper Data "
                                  + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(view.Month) 
                                  + " " + view.Year;
                view.MonthYear = new string[2];
                var startDate = new DateTime(view.Year, view.Month, 1);
                view.MonthYear[0] = startDate.ToShortDateString();
                var daysInMonth = DateTime.DaysInMonth(view.Year, view.Month);
                var thruDate = new DateTime(view.Year, view.Month, daysInMonth);
                view.MonthYear[1] =thruDate.ToShortDateString();
                view.HelperTitles = new string[21];
                view.HelperTitles[1] = "# Total Food Lbs";
                view.HelperTitles[2] = "# Deliveries";
                view.HelperTitles[3] = "# Cumulative Residents Served Daily";
                view.HelperTitles[4] = "# Cumulative Residents Children <18";
                view.HelperTitles[5] = "# Cumulative Residents Adults 18-59";
                view.HelperTitles[6] = "# Cumulative Residents Seniors 60+";
                view.HelperTitles[7] = "# A Bags (10 lbs per bag)";
                view.HelperTitles[8] = "# B Bags (9 lbs per bag)";
                view.HelperTitles[9] = "# Snacks";
                view.HelperTitles[10] = "# Giant Gift Cards Disbursed";
                view.HelperTitles[11] = "# Households Distinct Served";
                view.HelperTitles[12] = "# Distinct Residents Served";
                view.HelperTitles[13] = "# Distinct Residents <18";
                view.HelperTitles[14] = "# Distinct Adults 18-59";
                view.HelperTitles[15] = "# Distinct Residents 60+";
                view.HelperTitles[16] = "# Repeat Deliveries";
                view.HelperTitles[17] = "# First-Time Deliveries";
                view.HelperTitles[18] = "# Households Receiving 2 Gift Cards";
                view.HelperTitles[19] = "# Total Lbs of A Bags";
                view.HelperTitles[20] = "# Total Lbs of B Bags";

                view.ZipCodes = AppRoutines.GetZipCodesList();
                view.ZipCounts = new int[21, view.ZipCodes.Count + 2]; // ZipCodes, Counts
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
                var totalHouseholds2GiftCards = 0;  
                var totalFullBags = 0;
                var totalHalfBags = 0;
                var totalSnacks = 0;
                var totalCards = 0;
                using var db = new BHelpContext();
                for (var zip = 0; zip < view.ZipCodes.Count; zip++)
                {
                    var stringZip = view.ZipCodes[zip];
                    var deliveryData = db.Deliveries.Where(d =>  d.Status == 1 
                                                                 && d.Zip == stringZip && d.DateDelivered >= startDate
                                                                 && d.DateDelivered <= thruDate).ToList();
                    totalDeliveries += deliveryData.Count;
                    
                    var distinctList = new List<int>();
                    var distinctChildren = 0; 
                    var distinctAdults = 0;
                    var distinctSeniors = 0;  
                    foreach (var del in deliveryData)
                    {
                        for (var i = 0; i < deliveryData.Count; i++)
                        {
                            if (distinctList.Contains(del.ClientId))
                                continue;
                            distinctChildren += del.Children;
                            totalDistinctChildren += del.Children;
                            distinctAdults += del.Adults;
                            totalDistinctAdults += del.Adults; 
                            distinctSeniors += del.Seniors;
                            totalDistinctSeniors += del.Seniors;
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
                    var households2GiftCards = 0;
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

                        if (del.GiftCards >= 2)
                        {
                            households2GiftCards++;
                            totalHouseholds2GiftCards++;
                        }
                          
                        cumulativeChildren += del.Children;
                        totalCumulativeChildren += del.Children;
                        cumulativeAdults += del.Adults;
                        totalCumulativeAdults += del.Adults;
                        cumulativeSeniors += del.Seniors;
                        totalCumulativeSeniors += del.Seniors;
                        fullBags += del.FullBags;
                        totalFullBags += del.FullBags;
                        halfBags += del.HalfBags;
                        totalHalfBags += del.HalfBags;
                        snacks += del.KidSnacks;
                        totalSnacks += del.KidSnacks;
                        cards += del.GiftCards + del.HolidayGiftCards;
                        totalCards += del.GiftCards + del.HolidayGiftCards;
                    }

                    var col = zip + 1; 
                    view.ZipCounts[1, col] = fullBags * 10 + halfBags *9;  // Total Food Lbs
                    view.ZipCounts[2, col] = deliveryData.Count;    // Total Deliveries
                    view.ZipCounts[3, col] = cumulativeChildren + cumulativeAdults + cumulativeSeniors;
                    view.ZipCounts[4, col] = cumulativeChildren;
                    view.ZipCounts[5, col] = cumulativeAdults;
                    view.ZipCounts[6, col] = cumulativeSeniors;
                    view.ZipCounts[7, col] = fullBags;
                    view.ZipCounts[8, col] = halfBags;
                    view.ZipCounts[9, col] = snacks;
                    view.ZipCounts[10, col] = cards;

                    view.ZipCounts[11, col] = distinctList.Count;
                    view.ZipCounts[12, col] = distinctChildren + distinctAdults + distinctSeniors; // Distinct Residents
                    view.ZipCounts[13, col] = distinctChildren;
                    view.ZipCounts[14, col] = distinctAdults;
                    view.ZipCounts[15, col] = distinctSeniors;
                    view.ZipCounts[16, col] = totalRepeatDeliveries; // Repeat Deliveries
                    view.ZipCounts[17, col] = totalFirstDeliveries; // First-Time Deliveries
                    view.ZipCounts[18, col] = households2GiftCards;
                    view.ZipCounts[19, col] = fullBags * 10;
                    view.ZipCounts[20, col] = halfBags * 9;
                }

                var totCol = view.ZipCodes.Count + 1;
                view.ZipCounts[1, totCol] = totalFullBags * 10 + totalHalfBags *9;  // Total Food Lbs
                view.ZipCounts[2, totCol] = totalDeliveries;    // Total Deliveries
                view.ZipCounts[3, totCol] = totalCumulativeChildren + totalCumulativeAdults + totalCumulativeSeniors;
                view.ZipCounts[4, totCol] = totalCumulativeChildren;
                view.ZipCounts[5, totCol] = totalCumulativeAdults;
                view.ZipCounts[6, totCol] = totalCumulativeSeniors;
                view.ZipCounts[7, totCol] = totalFullBags;
                view.ZipCounts[8, totCol] = totalHalfBags;
                view.ZipCounts[9, totCol] = totalSnacks;
                view.ZipCounts[10, totCol] = totalCards;

                view.ZipCounts[11, totCol] = totalDistinctHouseholds; // Distinct Households
                view.ZipCounts[12, totCol] = totalDistinctChildren + totalDistinctAdults + totalDistinctSeniors; // Distinct Residents
                view.ZipCounts[13, totCol] = totalDistinctChildren;
                view.ZipCounts[14, totCol] = totalDistinctAdults;
                view.ZipCounts[15, totCol] = totalDistinctSeniors;
                view.ZipCounts[16, totCol] = totalCumulativeRepeatDeliveries; // Repeat Deliveries
                view.ZipCounts[17, totCol] = totalCumulativeFirstDeliveries; // First-Time Deliveries
                view.ZipCounts[18, totCol] = totalHouseholds2GiftCards;
                view.ZipCounts[19, totCol] = totalFullBags * 10;
                view.ZipCounts[20, totCol] = totalHalfBags * 9;

                return view;
            }
            private static List<SelectListItem> GetSnapshotFamily(string listHH)
            {
                var i = 0;
                var familyList = new List<SelectListItem>();
                var listStrLineElements = listHH.Split(',').ToList();
                foreach (var mbr in listStrLineElements)
                {
                    i++;
                    var newListItem = new SelectListItem() { Value = i.ToString(), Text = mbr };
                    familyList.Add(newListItem);
                }
                return familyList;
            }
            private static ReportsViewModel GetCountyReportView(int yy, int qtr)
            {
                using var db = new BHelpContext();
                var view = new ReportsViewModel { Year = yy, Quarter = qtr };
                if (qtr == 1)
                {
                    view.Months = new[] { 1, 2, 3 };
                }

                if (qtr == 2)
                {
                    view.Months = new[] { 4, 5, 6 };
                }

                if (qtr == 3)
                {
                    view.Months = new[] { 7, 8, 9 };
                }

                if (qtr == 4)
                {
                    view.Months = new[] { 10, 11, 12 };
                }

                view.MonthYear = new string[3];
                view.MonthYear[0] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(1 + 3 * (qtr - 1))
                    + " " + view.Year;
                view.MonthYear[1] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(2 + 3 * (qtr - 1))
                    + " " + view.Year;
                view.MonthYear[2] =
                    DateTimeFormatInfo.CurrentInfo.GetMonthName(3 + 3 * (qtr - 1))
                    + " " + view.Year;
                view.DateRangeTitle = view.MonthYear[0] + " through " + view.MonthYear[2];
                view.ReportTitle = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(1 + 3 * (qtr - 1))
                                   + "-" + DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(3 + 3 * (qtr - 1))
                                   + " " + view.Year.ToString() + " County Report";

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
                        var t = view.ZipCodes.Count; // Extra zip code column is for totals
                        for (var j = 0; j < view.ZipCodes.Count; j++)
                        {
                            if (delivery.Zip == view.ZipCodes[j])
                            {
                                view.Counts[mo, j, 0]++;
                                view.Counts[mo, t, 0]++; // month, zip, # of families
                                var c = Convert.ToInt32(delivery.Children);
                                var a = Convert.ToInt32(delivery.Adults);
                                var s = Convert.ToInt32(delivery.Seniors);
                                view.Counts[mo, j, 1] += c;
                                view.Counts[mo, t, 1] += c;
                                view.Counts[mo, j, 2] += a;
                                view.Counts[mo, t, 2] += a;
                                view.Counts[mo, j, 3] += s;
                                view.Counts[mo, t, 3] += s;
                                view.Counts[mo, j, 4] += (a + c + s);
                                view.Counts[mo, t, 4] += (a + c + s); // # of residents
                                var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                                view.Counts[mo, j, 5] += lbs;
                                view.Counts[mo, t, 5] += lbs; // pounds of food
                            }
                        }
                    }
                }

                view.CountyTitles = new string[7];
                view.CountyTitles[0] = "Zip Code";
                view.CountyTitles[1] = "# of Families";
                view.CountyTitles[2] = "# of Children (<18)";
                view.CountyTitles[3] = "# of Adults (>=18 and <60)";
                view.CountyTitles[4] = "# of Seniors (>=60)";
                view.CountyTitles[5] = "# of Residents";
                view.CountyTitles[6] = "# of Pounds of Food";
                return view;
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Reports")]
            public ActionResult CountyReportToExcel(int yy, int qtr)
            {
                var view = GetCountyReportView(yy, qtr);
                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add(view.ReportTitle);
                var activeRow = 1;
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
                    for (var i = 0; i < view.ZipCodes.Count; i++)
                    {
                        ws.Cell(activeRow, i +2).SetValue(view.ZipCodes[i]); ;
                    }
                    ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("All Zip Codes");
                    activeRow ++;
                    ws.Cell(activeRow, 1).SetValue("# of Families");
                    for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 0]);
                    }
                    activeRow ++;
                    ws.Cell(activeRow, 1).SetValue("# of Children (" + "<"+ "18)");
                    for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 1]);
                    }
                    activeRow ++;
                    ws.Cell(activeRow, 1).SetValue("# of Adults(" + ">" + "=18 and " + "<" + "60)");
                    for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 2]);
                    }
                    activeRow ++;
                    ws.Cell(activeRow, 1).SetValue("# of Seniors (" + ">" + "=60)");
                    for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 3]);
                    }
                    activeRow ++;
                    ws.Cell(activeRow, 1).SetValue("# of Residents");
                    for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 4]);
                    }
                    activeRow ++;
                    ws.Cell(activeRow, 1).SetValue("# of Pounds of Food");
                    for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                    {
                        ws.Cell(activeRow, i + 2).SetValue(view.Counts[view.Months[mo], i, 5]);
                    }
                    activeRow += 2;
                }
                ws.Columns().AdjustToContents();
                var ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    { FileDownloadName = view.ReportTitle +".xlsx" };
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Reports")]
            public ActionResult CountyReportToCSV(int yy, int qtr)
            {
                var view = GetCountyReportView(yy, qtr);
                var result = AppRoutines.CountyReportToCSV(view);
                return result;
            }

            private  DateTime? GetLastGetDeliveryDate(int id)
            {
                using var db = new BHelpContext();
                var dt = db.Deliveries.Where(d => d.DateDelivered != null
                                                  && d.Id == id)
                    .OrderByDescending(x => x.DateDelivered).Select(d => d.DateDelivered)
                    .FirstOrDefault();

                // ReSharper disable once UseNullPropagation
                if (dt == null)
                {
                    return null;
                }
                else
                {
                    return (DateTime)dt;
                }
            }
            private static int GetGiftCardsSince(int clientId, DateTime dt1, DateTime dt2)
            {
                using var db = new BHelpContext();
                var total = 0;
                var dList = db.Deliveries.Where(d => d.ClientId == clientId
                    && d.DateDelivered >= dt1 && d.DateDelivered <= dt2)
                    .Select(g => g.GiftCards).ToList();
                foreach (var i in dList)
                {
                    var gc = i;
                    total += gc;
                }

                return total;
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Reports")]
            public ActionResult QuorkReport(string endingDate = "")
            {
                DateTime endDate;
                if (endingDate.IsNullOrEmpty())
                {
                    // Ends on a Saturday - weekday Monday is 1, Saturday is 6
                    // If today is a  Saturday, default to this week
                    var weekDay = Convert.ToInt32(DateTime.Today.DayOfWeek);
                    if (weekDay >= 6) // Default to this this Saturday, else Saturday last week
                    { endDate = DateTime.Today.AddDays(6 - weekDay); }
                    else
                    {
                        var lastSaturday = DateTime.Now.AddDays(-1);
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
                using var db = new BHelpContext();
                var startDate = endDate.AddDays(-6);
                var view = new ReportsViewModel()
                {
                    BeginDate = startDate,
                    EndDate = endDate
                };
                view.EndDateString = view.EndDate.ToString("M-d-yy");
                view.DateRangeTitle = startDate.ToShortDateString() + " - " + endDate.ToShortDateString();
                view.ReportTitle = view.EndDateString + " QORK Weekly Report";

                view.ZipCodes = AppRoutines.GetZipCodesList();
                // Load Counts - extra zip code is for totals column.
                view.Counts = new int[1, view.ZipCodes.Count + 1, 8]; // 0 (unused), ZipCodes, Counts
                var deliveries = db.Deliveries
                    .Where(d => d.Status == 1 && d.DateDelivered >= startDate
                                              && d.DateDelivered < endDate).ToList();

                foreach (var delivery in deliveries)
                {
                    var zipCount = view.ZipCodes.Count; // Extra zip code column is for totals
                    for (var j = 0; j < view.ZipCodes.Count; j++)
                    {
                        if (delivery.Zip == view.ZipCodes[j])
                        {
                            var lbs = Convert.ToInt32(delivery.FullBags * 10 + delivery.HalfBags * 9);
                            view.Counts[0, j, 0] += lbs;
                            view.Counts[0, zipCount, 0] += lbs; //pounds of food
                            view.Counts[0, j, 1]++;
                            view.Counts[0, zipCount, 1]++; //# unique households served
                            var c = Convert.ToInt32(delivery.Children);
                            var a = Convert.ToInt32(delivery.Adults);
                            var s = Convert.ToInt32(delivery.Seniors);
                            view.Counts[0, j, 2] += (a + c + s);
                            view.Counts[0, zipCount, 2] += (a + c + s); //# residents served
                            view.Counts[0, j, 3] += c;
                            view.Counts[0, zipCount, 3] += c; //# children
                            view.Counts[0, j, 4] += s;
                            view.Counts[0, zipCount, 4] += s; //# seniors
                            view.Counts[0, j, 5] = 0;
                            view.Counts[0, zipCount, 5] = 0; //#staff worked  ZERO!!!
                            view.Counts[0, j, 6] = 0;
                            view.Counts[0, zipCount, 6] = 0; //# staff hours   ZERO!!!
                            view.Counts[0, j, 7]++;
                            view.Counts[0, zipCount, 7]++; //# deliveries
                        }
                    }
                }

                return view;
            }

            [Authorize(Roles = "Administrator,Staff,Developer,Reports")]
            public ActionResult QuorkReportToExcel(string endingDate)
            {
                var endDate = Convert.ToDateTime(endingDate);
                var view = GetQuorkReportView(endDate);
                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add(view.ReportTitle);

                var activeRow = 1;
                ws.Cell(activeRow, 1).SetValue("Bethesda Help, Inc. Quork Report");
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("Time Period");
                activeRow++;
                ws.Cell(activeRow, 1).SetValue(view.DateRangeTitle);
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("Zip Codes");
                for (var i = 0; i < view.ZipCodes.Count; i++)
                {
                    ws.Cell(activeRow, i + 2).SetValue(view.ZipCodes[i]);
                }
                ws.Cell(activeRow, view.ZipCodes.Count + 2).SetValue("Total Zip Codes");
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("Total Food Lbs)");
                for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 0]); }
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("# HH Served (No Repeat Clients in Time Period)");
                for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0,i,1]); }
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("# Residents Served");   
                for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 2]); }
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("# Residents <18");
                for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 3]); }
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("# Residents >60");
                for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 4]); }
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("# Staff Worked");
                activeRow++;  
                ws.Cell(activeRow, 1).SetValue("/# Staff Hours");
                activeRow++;
                ws.Cell(activeRow, 1).SetValue("# Deliveries");
                for (var i = 0; i < view.ZipCodes.Count + 1; i++)
                { ws.Cell(activeRow, i + 2).SetValue(view.Counts[0, i, 7]); }

                ws.Columns().AdjustToContents();
                var ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;
                return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    { FileDownloadName = view.ReportTitle + ".xlsx" };
            }
        
            [Authorize(Roles = "Reports,Administrator,Staff,Developer,Driver,OfficerOfTheDay")]
            public ActionResult ReturnToReportsMenu()
            {
                return RedirectToAction("ReportsMenu", "Reports");
            }

            public ActionResult EditDateDeliveredChange(string parameters)
            {
                var data = parameters.Split(Convert.ToChar("|"));
                
                var idData = data[0].Split(Convert.ToChar("="));
                var newDateDelivered = data[1].Split(Convert.ToChar("="));
                var urlData = data[2].Split(Convert.ToChar("="));
                var odidData =data[3].Split(Convert.ToChar("="));
                var deliverydateodidData = data[4].Split(Convert.ToChar("="));
                var odnotesData = data[5].Split(Convert.ToChar("="));
                var drivernotesData = data[6].Split(Convert.ToChar("="));
                var zipData = data[7].Split(Convert.ToChar("="));
                var statusData = data[8].Split(Convert.ToChar("="));
                var fullbagsData = data[9].Split(Convert.ToChar("="));
                var halfbagsData = data[10].Split(Convert.ToChar("="));
                var kidsnacksData = data[11].Split(Convert.ToChar("="));
                var giftcardsData = data[12].Split(Convert.ToChar("="));
                var holidaygiftcardsData = data[12].Split(Convert.ToChar("="));

            return RedirectToAction("Edit", new{ id = idData[1],
                    _NewDateDelivered = newDateDelivered[1],
                    returnUrl = urlData[1],
                    _ODId = odidData[1], 
                    _DeliveryDateOID = deliverydateodidData[1],
                    _ODNotes = odnotesData[1],
                    _DriverNotes = drivernotesData[1],
                    _Zip = zipData[1],
                    _Status = statusData[1],
                    _FullBags = fullbagsData[1],
                    _HalfBags = halfbagsData[1],
                    _KidSnacks = kidsnacksData[1],
                    _GiftCards = giftcardsData[1],
                    _HolidayGiftCards = holidaygiftcardsData[1]
                });
            }

            public ActionResult ChangeDeliveryClient(string parameters)
            {
                using var _db = new BHelpContext();
                var data = parameters.Split(Convert.ToChar("|"));
                var delRecId = Convert.ToInt32((data[0]));
                var newClientId = Convert .ToInt32(data[1]);
                var returnURL = data[2];
                var delRec = _db.Deliveries.Find(delRecId);
                if (delRec != null)
                {
                    var existingClient = _db.Clients.Find(delRec.ClientId);
                    var newClient = _db.Clients.Find(newClientId);
                    if (existingClient != null && newClient != null)
                    {
                        var view = new DeliveryViewModel()
                        {
                            OldClient = existingClient,
                            NewClient = newClient,
                            ReturnURL = returnURL,
                            HistoryStartDate = (DateTime?)Session["CallLogStartDate"],
                            HistoryEndDate = (DateTime?)Session["CallLogEndDate"],
                            Parameters = parameters
                    };
                        return View(view);
                    }
                }

                if (returnURL.Contains("Individual")){ return RedirectToAction("CallLogIndividual");}
                if (returnURL.Contains("LogDate")) { return RedirectToAction("CallLogByLogDate"); }
                if (returnURL.Contains("DateDelivered")) { return RedirectToAction("CallLogByDateDelivered"); }

                return RedirectToAction("CallLogMenu"); // default
            }

            public ActionResult ChangeDeliveryClientConfirmed(string parameters)
            {
                using var _db = new BHelpContext();
                var data = parameters.Split(Convert.ToChar("|"));
                var recId = Convert.ToInt32((data[0]));
                var newClientId = Convert.ToInt32(data[1]);
                var returnURL = data[2];
                var client = _db.Clients.Find(newClientId);
                var del = _db.Deliveries.Find(recId);
                if (del != null && client != null)
                {
                    del.ClientId = newClientId;
                    del.FirstName = client.FirstName;
                    del.LastName = client.LastName;
                    del.StreetNumber = client.StreetNumber;
                    del.StreetName = client.StreetName;
                    del.City = client.City;
                    del.Zip = client.Zip;
                    del.Phone = client.Phone;
                    var familyList = AppRoutines.GetFamilyMembers(newClientId);
                    var namesAgesInHH = "";
                    foreach (var mbr in familyList)
                    {
                        namesAgesInHH += mbr.NameAge;
                    }
                    del.NamesAgesInHH = namesAgesInHH;
                    _db.SaveChanges();
                }

                var startDt = Session["CallLogStartDate"];
                var endDt = Session["CallLogEndDate"];
            if (returnURL.Contains("Individual"))  return RedirectToAction("CallLogIndividual"); 
            if (returnURL.Contains("LogDate")) return RedirectToAction("CallLogByLogDate", new {startDate = startDt , endDate = endDt });
            if (returnURL.Contains("DateDelivered")) return RedirectToAction("CallLogByDateDelivered", new { startDate = startDt, endDate = endDt }); 

            return RedirectToAction("CallLogMenu");
            }
    }
}
