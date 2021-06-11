using System.Linq;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class UpdateMyProfileController : Controller
    {
        private readonly BHelpContext _db = new BHelpContext();

        // GET: UpdateMyProfile
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
        [HttpPost]
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

        public ActionResult ReturnToDashboard()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}