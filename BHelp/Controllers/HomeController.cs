using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using Castle.Core.Internal;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace BHelp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            var view = new ApplicationUser()
                {
                    ShowAdminDocumentsMenu = false,
                    ShowDriverDocumentsMenu = false,
                    ShowODDocumentsMenu = false,
                    ShowStaffDocumentsMenu = false,
                    ShowEveryoneDocumentsMenu = false,
                    ShowReportsDocumentsMenu = false,
                    ShowPantryCoordinatorDocumentsMenu = false
                };
            var db = new BHelpContext();
            var docList = db.Documents.ToList();
            var menuCatString = "";
            foreach (var doc in docList)
            { menuCatString += doc.MenuCategory; }

            if (menuCatString.Contains("Admin")) view.ShowAdminDocumentsMenu = true;
            if (menuCatString.Contains("Driver")) view.ShowDriverDocumentsMenu = true;
            if (menuCatString.Contains("Officer")) view.ShowODDocumentsMenu = true;
            if (menuCatString.Contains("Staff")) view.ShowStaffDocumentsMenu = true;
            if (menuCatString.Contains("Everyone")) view.ShowEveryoneDocumentsMenu = true;
            if (menuCatString.Contains("Reports")) view.ShowReportsDocumentsMenu = true;
            if (menuCatString.Contains("PantryCoordinator")) view.ShowPantryCoordinatorDocumentsMenu = true;

            ViewData["vmPassword"]  = AppRoutines.GetVoicemailPassword();
            return View(view);
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult MaintainUsers()
        {
            return RedirectToAction("Index", "Users");
        }

        [AllowAnonymous]
        public ActionResult UpdateMyProfile()
        {
            return RedirectToAction("Edit", "UpdateMyProfile");
        }

        [AllowAnonymous]
        public ActionResult ChangeMyPassword()
        {
            return RedirectToAction("ResetPassword", "Account");
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult MaintainUserRoles()
        {
            return RedirectToAction("Index", "UserRoles");
        }

        public ActionResult GoToHostGatorSite()
        {
            return Redirect("https://www.bethesdahelpfd.org");
        }

        [AllowAnonymous] public ActionResult LogOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Login", "Account");
        }
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
        public ActionResult GetZipCodes()
        {
            AppRoutines.GetZipCodesSelectList();
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
        public ActionResult GetLatestDeliveries()
        {
            Utilities.GetLatestDeliveries();
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

        // GET: Reset Voicemail Password/Edit
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult ResetVoicemailPassword()
        {
            var _vmPassword = AppRoutines.GetVoicemailPassword();
            var _infoLines = AppRoutines.GetVoicemailInfoLines();
            var view = new VoicemailPasswordViewModel
            {
                VoicemailPassword = _vmPassword,
                InfoText = _infoLines
            };
            return View(view);
        }

        // POST: Reset Voicemail Password/Edit
        [HttpPost, Authorize(Roles = "Administrator,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetVoicemailPassword(
            [Bind(Include = "VoicemailPassword,InfoText")] VoicemailPasswordViewModel view)
        {
            if (!ModelState.IsValid) return View(view);
            var file = AppDomain.CurrentDomain.BaseDirectory
                       + "/App_Data/BHelpVoicemailCredentials.txt";
            var replacementText = "";
            foreach (string line in view.InfoText)
            {
                replacementText += line + "\r\n";
            }
            replacementText += view.VoicemailPassword;
            using (var sw = new System.IO.StreamWriter((file), false))
            {
             sw.Write(replacementText);
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult MaintainDocuments()
        {

            return null;
        }
    }
}

