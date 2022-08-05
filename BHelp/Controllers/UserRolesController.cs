using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly BHelpContext _db = new BHelpContext();

        // GET: Roles
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Index()
        {
            var model = new AddEditUserRolesViewModel
            {
                UserNameRoles = new List<UserNameRole>()
            };

            var users = _db.Users.OrderBy(u => u.LastName).ToList();
            foreach (var user in users)
            {
                foreach (var role in user.Roles)
                {
                    var roleName = (from r in _db.Roles where (r.Id == role.RoleId) select r).Single();
                    UserNameRole userNameRole = new UserNameRole
                    {
                        Name = user.UserName + ": " + user.FirstName + " "
                                + user.LastName + " / " + roleName.Name,
                        Id = user.Id + "|" + roleName.Id,
                        Email = user.Email
                    };
                    model.UserNameRoles.Add(userNameRole);
                }
            }
            return View(model);
        }

        // GET: Roles/Create
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Create()
        {
            AddEditUserRolesViewModel viewModel = new AddEditUserRolesViewModel();

            var userRoles = (from r in _db.Roles.OrderBy(n => n.Name) select r).ToList();
            List<SelectListItem> rolesList = new List<SelectListItem>();
            foreach (var role in userRoles)
            {
                if (role.Name != "Developer")
                {
                    rolesList.Add(new SelectListItem() { Text = role.Name, Value = role.Id });
                }
            }

            var users = (from u in _db.Users.OrderBy(n => n.LastName) select u).ToList();
            List<SelectListItem> usersList = new List<SelectListItem>();
            foreach (var user in users)
            {
                usersList.Add(new SelectListItem() { Text = user.UserName + @" " + user.FirstName + @" " + user.LastName, Value = user.Id });
            }

            viewModel.UserRoles = rolesList;
            viewModel.Users = usersList;

            return View(viewModel);
        }

        // POST: Roles/Create
        [HttpPost, Authorize(Roles = "Administrator,Developer")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AddEditUserRolesViewModel model, object msgbox)
        {
            if (ModelState.IsValid)
            {
                var userStore = new UserStore<ApplicationUser>(_db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                var role = _db.Roles.Find(model.UserRole.Id);

                try
                {
                    userManager.AddToRole(model.User.Id, role.Name);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error assigning role: " + ex.Message);
                }

                // Force new session variable lookup in ODSchedules id OD:
                if (role.Name == "OfficerOfTheDay") Session["ODSelectList"] = null;
                return RedirectToAction("Index", "UserRoles");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: Roles/Edit/5
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string[] splitId = id.Split('|');
            var userId = splitId[0];
            var roleId = splitId[1];
            var role = (from r in _db.Roles where (r.Id == roleId) select r).Single();
            if (role == null)
            {
                return HttpNotFound();
            }
            var model = new AddEditUserRolesViewModel();

            var user = _db.Users.Find(userId);
            model.Name = user.FirstName + " " + user.LastName;

            //List<SelectListItem> listRoles = new List<SelectListItem>();
            //var userRoles = (from r in _db.Roles select r).ToList();
            return View(model);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Delete(string id)   // contains userId | roleId
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            string[] splitId = id.Split('|');
            var userId = splitId[0];
            var roleId = splitId[1];
            var role = (from r in _db.Roles where (r.Id == roleId) select r).Single();
            ApplicationUser user = (from u in _db.Users.Where(u => u.Id == userId) select u).Single();
            if (user == null)
            {
                return HttpNotFound();
            }
            AddEditUserRolesViewModel model = new AddEditUserRolesViewModel
            {
                User = user,
                Name = role.Name
            };

            return View(model);
        }

        // POST: UserRoles/Delete/5
        [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrator,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(AddEditUserRolesViewModel model)
        {
            string[] splitId = model.Id.Split('|');
            var userId = splitId[0];
            var roleId = splitId[1];
            var roles = new string[1];
            var role = _db.Roles.Find(roleId);
            roles[0] = role.Name;

            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
            await userManager.RemoveFromRolesAsync(userId, roles).ConfigureAwait(false);
            return RedirectToAction("Index");
        }
    }
}
