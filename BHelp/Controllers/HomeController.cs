using System.Linq;
using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.Models;
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

            //var _pwd_odId = AppRoutines.GetVoicemailPassword();
            //var pw_od = _pwd_odId.Split(' ');
            //ViewData["VoiceMailMessage"]  = pw_od[0] + " Call or Text " + AppRoutines.GetUserFullName(pw_od[1]) 
            //                    + " " + AppRoutines.GetUserPhone(pw_od[1]);
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
            return string.IsNullOrEmpty(User.Identity.Name) ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}

