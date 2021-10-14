using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace BHelp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MaintainUsers()
        {
            return RedirectToAction("Index", "Users");
        }

        public ActionResult UpdateMyProfile()
        {
            return RedirectToAction("Edit", "UpdateMyProfile");
        }

        public ActionResult ChangeMyPassword()
        {
            return RedirectToAction("ResetPassword", "Account");
        }

        public ActionResult MaintainUserRoles()
        {
            return RedirectToAction("Index", "UserRoles");
        }

        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GoToBHelpSite()
        {
            return Redirect("https://www.bethesdahelp.org");
        }
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

        public ActionResult UploadClients()
        {
            Utilities.UploadClients();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GetZipCodes()
        {
            AppRoutines.GetZipCodesSelectList();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UploadDeliveries()
        {
            Utilities.UploadDeliveries();
            return RedirectToAction("Index", "Home");
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
        public ActionResult TestMySql()
        {
            Utilities.TestMySql();
            return RedirectToAction("Index", "Home");
        }

    }
}

