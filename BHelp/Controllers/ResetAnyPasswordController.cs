using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Castle.Core.Internal;
using Newtonsoft.Json;

namespace BHelp.Controllers
{
    [Authorize]
    public class ResetAnyPasswordController : Controller
    {
        private readonly BHelpContext _db = new BHelpContext();

        [Authorize(Roles = "Administrator,Developer")]
        public async Task<ActionResult> Reset(string userId, string newPassword)
        {
            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(_db);
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(store);
            string hashedNewPassword = userManager.PasswordHasher.HashPassword(newPassword);
            ApplicationUser cUser = await store.FindByIdAsync(userId);
            await store.SetPasswordHashAsync(cUser, hashedNewPassword);
            await store.UpdateAsync(cUser);

            return View();
        }

        public ActionResult GetUserInfo(string id)
        {
            ApplicationUser user = (from u in _db.Users where u.Id == id select u).Single();
            try
            {
                String json = JsonConvert.SerializeObject(user, Formatting.Indented);
                return Content(json, "application/json");
            }
            catch (Exception)
            {
                return null;
            }
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
