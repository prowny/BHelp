using System.Web;
using System.Web.Mvc;
using BHelp.DataAccessLayer;
using BHelp.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace BHelp.Controllers
{
    [Authorize]
    public class UpdateMyProfileController : Controller
    {
        private readonly BHelpContext _db = new BHelpContext();

        // GET: UpdateMyProfile
        [AllowAnonymous]
        public ActionResult Edit()
        {
            var user = System.Web.HttpContext.Current.GetOwinContext()
                .GetUserManager<ApplicationUserManager>()
                .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            if (user == null) return RedirectToAction("Logout", "Home");
            var model = new UpdateMyProfileViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Title = user.Title,
                PhoneNumber = user.PhoneNumber,
                PhoneNumber2 = user.PhoneNumber2,
                Address = user.Address,
                City = user .City ,
                State = user.State,
                Zip =user.Zip,
                States =AppRoutines .GetStatesSelectList()
            };
            foreach (var state in model.States)
            {
                if (model.State == state.Value)
                {
                    state.Selected = true;
                    break;
                }
            }
            return View(model);
        }

        // POST: UpdateMyProfile/Edit
        [HttpPost, Authorize(Roles = "Administrator,Developer,Staff,OfficerOfTheDay,Driver")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Title" +
                    ",PhoneNumber,PhoneNumber2,Email,Address,City,State,Zip")]
                    UpdateMyProfileViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.Find(viewModel.Id);
                if (user == null) return RedirectToAction("Index", "Home");
                user.FirstName = viewModel.FirstName;
                user.LastName = viewModel.LastName;
                user.Title = viewModel.Title;
                user.PhoneNumber = viewModel.PhoneNumber;
                user.PhoneNumber2 = viewModel.PhoneNumber2;
                user.Email = viewModel.Email;
                user.Address = viewModel.Address;
                user.City = viewModel.City;
                user.State = viewModel.State;
                user.Zip = viewModel.Zip;
                _db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}