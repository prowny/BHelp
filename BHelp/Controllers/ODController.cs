using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BHelp.Controllers
{
    public class ODController : Controller
    {
        private BHelpContext db = new BHelpContext();

        // GET: Household
        public ActionResult Index()
        {
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