using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
                    ShowPantryCoordinatorDocumentsMenu = false,
                    ShowBaggerDocumentsMenu = false
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
            if (menuCatString.Contains("Bagger")) view.ShowBaggerDocumentsMenu = true;

            if (view.ShowEveryoneDocumentsMenu)
            { // get Everyone docs Title and Index for direct access on main menu
                var listEveryoneDocs = db.Documents.Where(c => c.MenuCategory == "Everyone" ).ToList();
                view.EveryoneDocTitles = new string[listEveryoneDocs.Count];
                view.EveryoneDocIds = new int[listEveryoneDocs.Count];
                var i = -1;
                foreach (var doc in listEveryoneDocs)
                {
                    i++;
                    view.EveryoneDocTitles[i] = doc.Title;
                    view.EveryoneDocIds[i] = doc.Id;
                }
            }

            var _pwd_odId = AppRoutines.GetVoicemailPassword();
            var pw_od = _pwd_odId.Split(' ');
            ViewData["VoiceMailMessage"]  = pw_od[0] + " Call or Text " + AppRoutines.GetUserFullName(pw_od[1]) 
                                + " " + AppRoutines.GetUserPhone(pw_od[1]);
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
            return String.IsNullOrEmpty(User.Identity.Name) ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }

        // GET: Reset Voicemail Password/Edit
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult ResetVoicemailPassword()
        {
            var _pwd_odId = (AppRoutines.GetVoicemailPassword()).Split(' ');
            var _VoiceMailMessage = _pwd_odId[0]; 
            var _vmOdId = _pwd_odId[1];
            var _infoLines = AppRoutines.GetVoicemailInfoLines();
            var _od = new BHelpContext().Users.Find(_vmOdId);
            var view = new VoicemailPasswordViewModel
            {
                VoicemailPassword = _VoiceMailMessage,
                OldVoicemailPassword = _VoiceMailMessage,
                LoginKeyReceiverName = _od?.FullName,
                LoginKeyReceiverId = _vmOdId,
                InfoText = _infoLines,
                UserList = AppRoutines.GetActiveUserSelectList() 
            };
            foreach (var item in view.UserList)
            {
                if (item.Value == _vmOdId )
                {
                    item.Selected = true;
                }
            }
            return View(view);
        }

        // POST: Reset Voicemail Password/Edit
        [HttpPost, Authorize(Roles = "Administrator,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetVoicemailPassword(
            [Bind(Include = "VoicemailPassword,OldVoicemailPassword,LoginKeyReceiverId,InfoText")] VoicemailPasswordViewModel view)
        {
            if (!ModelState.IsValid) return View(view);
            var file = AppDomain.CurrentDomain.BaseDirectory
                       + "/App_Data/BHelpVoicemailCredentials.txt";
            
            if (!String.IsNullOrEmpty(view.VoicemailPassword))
            {
                view.InfoText[4] = view.VoicemailPassword;
            }
            view.InfoText[6] = view.LoginKeyReceiverId;

            var replacementText = "";
            foreach (var line in view.InfoText)
            {
                replacementText += line + "\r\n";
            }
            
            using (var sw = new System.IO.StreamWriter((file), false))
            {
             sw.Write(replacementText);
            }

            var content = "Voicemail password changed to " + view.VoicemailPassword
                                                           + " by user " + User.Identity.Name;
            // ReSharper disable once UnusedVariable
            var task = SendEmail("prowny@aol.com", content);

            return RedirectToAction("Index", "Home");
        }
        private static async Task SendEmail(string address, string htmlContent)   // Send individual email   
        {
            using var msg = new MailMessage();
            msg.From = new MailAddress("Admin@BethesdaHelpFd.org", "BHELP Developer");
            msg.To.Add(new MailAddress(address, "BHELP Team Member"));
            msg.Subject = "Voicemail Password Change";
            msg.Body = htmlContent;
            msg.IsBodyHtml = true;
            msg.Priority = MailPriority.Normal;
            using var mailClient = new SmtpClient("BethesdaHelpFd.org", 587);
            mailClient.Credentials = new NetworkCredential("Admin@BethesdaHelpFd.org", "Qo3YQoyYAghL*U.W-KEU");
            await mailClient.SendMailAsync(msg);
        }
    }
}

