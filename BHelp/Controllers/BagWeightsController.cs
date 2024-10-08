﻿using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class BagWeightsController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: BagWeights
        public ActionResult Index()
        {
            var weightlist = db.BagWeights
                .OrderBy(d => d.EffectiveDate ).ToList();
            foreach (var item in weightlist)
            {
                item.EffectiveDateString = item.EffectiveDate.ToString("MM/dd/yyyy");
            }
            return View(weightlist);
        }
        
        // GET: BagWeights/Create
        public ActionResult Create()
        {
            var view = new BagWeight
            {
                APounds = 0,
                BPounds = 0,
                CPounds = 0,
                EffectiveDate = DateTime.Today
            };
              
            return View(view);
        }

        // POST: BagWeights/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,APounds,BPounds,CPounds,EffectiveDate")] BagWeight bagWeight)
        {
            if (!ModelState.IsValid) return View(bagWeight);
            var checkList = db.BagWeights.ToList();
            foreach (var item in checkList)
            {
                if (item.EffectiveDate == bagWeight.EffectiveDate)
                {
                    ModelState.AddModelError("EffectiveDate",
                   "Date Already Exists in Another Record!");
                    return View(bagWeight);
                }
            }

            if (bagWeight.EffectiveDate < DateTime.Today.AddDays(-13))
            {
                ModelState.AddModelError("EffectiveDate",
                    "Cannot create record that is two weeks older than today!");
                return View(bagWeight);
            }

            db.BagWeights.Add(bagWeight);
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        // GET: BagWeights/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bagWeights = db.BagWeights.Find(id);
            if (bagWeights == null)
            {
                return HttpNotFound();
            }
            return View(bagWeights);
        }

        // POST: BagWeights/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,APounds,BPounds,CPounds,EffectiveDate")] BagWeight bagWeight)
        {
            if (!ModelState.IsValid) return View(bagWeight);

            var checkList = db.BagWeights.ToList();
            foreach (var item in checkList)
            {
                if (item.Id == bagWeight.Id ) continue;
                if(item.EffectiveDate == bagWeight.EffectiveDate)
                {
                    ModelState.AddModelError("EffectiveDate", "Date Already Exists in Another Record!");
                    return View(bagWeight);
                }
            }

            AppRoutines.UpdateBagWeights(bagWeight);
            return RedirectToAction("Index");
        }

        // GET: BagWeights/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bagWeight = db.BagWeights.Find(id);
            if (bagWeight == null) { return HttpNotFound(); }
            bagWeight.EffectiveDateString = bagWeight.EffectiveDate.ToString("MM/dd/yyyy");
            return View( bagWeight );
        }

        // POST: BagWeights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var bagWeight = db.BagWeights.Find(id);
            if (bagWeight == null) return RedirectToAction("Index");
            db.BagWeights.Remove(bagWeight);
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
