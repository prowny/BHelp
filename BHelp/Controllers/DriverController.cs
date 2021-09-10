using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;
using ClosedXML.Excel;
using Castle.Core.Internal;

namespace BHelp.Controllers
{
    public class DriverController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: Driver
        public ActionResult Index(DateTime? logDate, string userId)

        {
            if (!logDate.HasValue)
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
                logDate = cdt;
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
           
            if (userId.IsNullOrEmpty()) { System.Web.HttpContext.Current.User.Identity.GetUserId(); }
            // 09/09/2021: change to driver eses all open deliveries
            var deliveryList = new List<Delivery>(db.Deliveries).Where(d => d.Completed == false)
                .OrderBy(d => d.DeliveryDate).ThenBy(z => z.Zip)
                .ThenBy(n => n.LastName).ToList();
            foreach (var delivery in deliveryList)
            {
                var client = db.Clients.Find(delivery.ClientId);
                if (client != null)
                {
                    delivery.ClientNameAddress = client.LastName + ", " + client.FirstName
                       + " " + client.StreetNumber + " " + client.StreetName;
                    delivery.Notes = client.Notes;
                    var familyMembers = AppRoutines.GetFamilyMembers(client.Id);
                    delivery.HouseoldCount = familyMembers.Count;
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
        public ActionResult Edit([Bind(Include = "Id,FullBags,HalfBags,KidSnacks,GiftCards," +
                    "DateDelivered,Completed,DriverNotes,DeliveryDate")] Delivery delivery)
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
                    del.DeliveryDate = delivery.DeliveryDate;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(delivery);
        }

        public ActionResult OpenDeliveriesToExcel()
        {
            var view = GetOpenDeliveryViewModel();
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.ReportTitle);
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue(view.ReportTitle);
            ws.Cell(activeRow, 2).SetValue(DateTime.Today.ToShortDateString());
            activeRow++;
            ws.Cell(activeRow, 2).SetValue("Delivery Date");
            ws.Cell(activeRow, 3).SetValue("Zip Code");
            ws.Cell(activeRow, 4).SetValue("Client");
            ws.Cell(activeRow, 5).SetValue("Address");
            ws.Cell(activeRow, 6).SetValue("City");
            ws.Cell(activeRow, 7).SetValue("Phone");
            ws.Cell(activeRow, 8).SetValue("# in HH");
            ws.Cell(activeRow, 9).SetValue("Client Notes");
            ws.Cell(activeRow, 10).SetValue("OD Notes");
            ws.Cell(activeRow, 11).SetValue("Driver Notes");
            //activeRow++;

            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = view.ReportTitle + ".xlsx" };
        }

        private OpenDeliveryViewModel GetOpenDeliveryViewModel()
        {
            var odv = new OpenDeliveryViewModel
            {
                ReportTitle = "Bethesda Help Open Deliveries"
            };
            var deliveryList = new List<Delivery>(db.Deliveries).Where(d => d.Completed == false)
                .OrderBy(d => d.DeliveryDate).ThenBy(z => z.Zip)
                .ThenBy(n => n.LastName).ToList();
            odv.OpenDeliveries = new string[deliveryList.Count , 11];
            var i = 0;
            foreach (var del in deliveryList)
            {
                var client = db.Clients.Find(del.ClientId);
                odv.OpenDeliveries[i, 0] = del.DeliveryDate.ToShortDateString();
                odv.OpenDeliveries[i, 1] = del.Zip;
                odv.OpenDeliveries[i, 2] = del.LastName = ", " + del.FirstName;
                odv.OpenDeliveries[i, 3] = del.StreetNumber + " " + del.StreetName;
                odv.OpenDeliveries[i, 4] = del.City;
                odv.OpenDeliveries[i, 5] = del.Phone;
                odv.OpenDeliveries[i, 6] = del.HouseoldCount.ToString();
                if (client != null) odv.OpenDeliveries[i, 7] = client.Notes;
                odv.OpenDeliveries[i, 8] = del.ODNotes;
                odv.OpenDeliveries[i, 9] = del.DriverNotes;
                i++;
            }
            return odv;
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
