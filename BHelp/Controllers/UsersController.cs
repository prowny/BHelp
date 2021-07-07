using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Net;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Microsoft.AspNet.Identity;

namespace BHelp.Controllers
{
    public class UsersController : Controller
    {
        readonly BHelpContext _db = new BHelpContext();

        // GET: Users
        public ActionResult Index()
        {
            return View(_db.Users.OrderBy(u => u.LastName));
        }
        // GET: Users/Edit/5
        public ActionResult Edit(string userName)
        {
            if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = (from u in _db.Users.Where(u => u.UserName == userName) select u).Single();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Active,FirstName,LastName,Title,PhoneNumber,Email,BeginDate,LastDate,Notes")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser saveUser = (from u in _db.Users.Where(u => u.Id == user.Id) select u).Single();
                saveUser.Active = user.Active;
                saveUser.UserName = user.UserName;
                saveUser.FirstName = user.FirstName;
                saveUser.LastName = user.LastName;
                saveUser.Title = user.Title;
                saveUser.PhoneNumber = user.PhoneNumber;
                saveUser.Email = user.Email;
                saveUser.BeginDate = user.BeginDate;
                saveUser.LastDate = user.LastDate;
                saveUser.Notes = user.Notes;

                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string userName)
        {
            if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = (from u in _db.Users.Where(u => u.UserName == userName) select u).Single();
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Create
        public ActionResult Create()
        {
            return RedirectToAction("Register", "Account");
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string userName)
        {
            ApplicationUser user = (from u in _db.Users.Where(u => u.UserName == userName) select u).Single();
            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult VolunteerDatesReport()
        {
            var rolesList = _db.Roles.OrderBy(r => r.Name).ToList();
            var userList = _db.Users.OrderBy(u => u.LastName).ToList();
            foreach (var role in rolesList)
            {
                var usersInRole = new List<ApplicationUser>();
                foreach (var user in userList)
                {
                    if (AppRoutines.UserIsInRole(user.Id, role.Name))
                   { usersInRole.Add(user); }
                }
                if (usersInRole.Count > 0)
                {

                }
            }

            return RedirectToAction("ReturnToDashboard");
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