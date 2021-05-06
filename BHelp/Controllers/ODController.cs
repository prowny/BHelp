using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using Castle.Core.Internal;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BHelp.Controllers
{
    public class ODController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Household
        public ActionResult Index(string callLogDate)
        {
            if (callLogDate.IsNullOrEmpty())
            {
                DateTime cdt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day); 
                var cdts = cdt.ToString("MM/dd/yyyy");
                Session["CallLogDate"] = cdts;
            }
            else
            {
                Session["CallLogDate"] = callLogDate;
            }
            
            var userIid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var user = db.Users.Find(userIid);
            Session["CurrentUserFullName"] = user.FullName;
            var householdView = new List<HouseholdViewModel>();
            return View(householdView);
        }
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}