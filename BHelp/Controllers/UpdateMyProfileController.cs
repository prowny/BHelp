using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.ViewModels;
using Castle.Core.Internal;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    [Authorize]
    public class UpdateMyProfileController : Controller
    {
        private readonly BHelpContext _db = new BHelpContext();

        // GET: UpdateMyProfile
        [Authorize(Roles = "Administrator,Developer,Staff,OD,Driver")]
        public ActionResult Edit()
        {
            var userId = User.Identity.GetUserId();
            var user = (from u in _db.Users where u.Id == userId select u).Single();
            UpdateMyProfileViewModel model = new UpdateMyProfileViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Title = user.Title,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        // POST: UpdateMyProfile/Edit
        [HttpPost, Authorize(Roles = "Administrator,Developer,Staff,OD,Driver")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Title,PhoneNumber,Email")] UpdateMyProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.Find(viewModel.Id);
                if (user != null)
                {
                    user.FirstName = viewModel.FirstName;
                    user.LastName = viewModel.LastName;
                    user.Title = viewModel.Title;
                    user.PhoneNumber = viewModel.PhoneNumber;
                    user.Email = viewModel.Email;

                    _db.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Administrator,Developer,Staff,OD,Driver")]
        public ActionResult ReturnToDashboard()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("Index", "Home");
        }
    }
}