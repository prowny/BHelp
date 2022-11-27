using System;
using System.Threading.Tasks;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp.Controllers
{
    public class ResetAnyPasswordController : Controller
    {
        private readonly BHelpContext _db = new BHelpContext();
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
        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
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
