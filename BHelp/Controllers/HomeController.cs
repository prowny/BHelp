using System.Web;
using System.Web.Mvc;
using BHelp.ViewModels;
using Castle.Core.Internal;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace BHelp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [Authorize(Roles = "Administrator,Staff,Developer,OD,Driver")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult MaintainUsers()
        {
            return RedirectToAction("Index", "Users");
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OD,Driver")]
        public ActionResult UpdateMyProfile()
        {
            return RedirectToAction("Edit", "UpdateMyProfile");
        }

        [Authorize(Roles = "Administrator,Staff,Developer,OD,Driver")]
        public ActionResult ChangeMyPassword()
        {
            return RedirectToAction("ResetPassword", "Account");
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult ViewAdminDocuments()
        {
            var upperBound = 3;
            var view = new DocumentsViewModel
            {
                DocNames = new string[upperBound, 2], // Display Name, File Name
                DocNamesUpperBound = upperBound
            };
            view.DocNames[0, 0] = "Administrator Manual";
            view.DocNames[0, 1] = "/Documents/BH-administrator-manual.pdf";
            view.DocNames[1, 0] = "Program Description";
            view.DocNames[1, 1] = "/Documents/BH-program-description.pdf";
            view.DocNames[2, 0] = "Retrieve Database Tables";
            view.DocNames[2, 1] = "/Documents/BH-retrieve-tables.pdf";
            return View(view);
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult MaintainUserRoles()
        {
            return RedirectToAction("Index", "UserRoles");
        }

        public ActionResult GoToBHelpSite()
        {
            return Redirect("https://www.bethesdahelp.org");
        }
        public ActionResult GoToHostGatorSite()
        {
            return Redirect("https://www.bethesdahelpfd.org");
        }

        [AllowAnonymous]
        public ActionResult LogOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Login", "Account");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public ActionResult GetZipCodes()
        {
            AppRoutines.GetZipCodesSelectList();
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }

        public ActionResult UploadClients()
        {
            Utilities.UploadClients();
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
        public ActionResult UploadDeliveries()
        {
            Utilities.UploadDeliveries();
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
        public ActionResult CopyClientZipToDelivery()
        {
            Utilities.CopyClientZipToDelivery();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult CopySnapshotDataToDelivery()
        {
            Utilities.CopySnapshotDataToDelivery();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult SetFirstDeliveries()
        {
            Utilities.SetFirstDeliveries();
            return RedirectToAction("Index", "Home");
        }
        public ActionResult SetStatusFlags()
        {
            Utilities.SetStatusFlags();
            return RedirectToAction("Index", "Home");
        }
    }
}

