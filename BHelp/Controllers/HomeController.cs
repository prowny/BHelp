using System;
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
        [AllowAnonymous]
        public ActionResult Index()
        {
            ViewData["vmPassword"]  = AppRoutines.GetVoicemailPassword();
            return View();
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

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

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
            [Bind(Include = "VoicemailPassword,InfoText")]
            VoicemailPasswordViewModel view)
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
    }
}

