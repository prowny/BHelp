using System;
using System.Collections.Generic;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using System.IO;

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
            UsersInRolesReportViewModel report = GetUsersInRolesReport();
            return View(report);
        }

        public ActionResult VolunteerDatesReportToExcel()
        {
            UsersInRolesReportViewModel report = GetUsersInRolesReport();
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add("Users In Roles");
            int activeRow = 1;
            ws.Cell(activeRow, 3).SetValue(report.Report[0][0][0]).Style.Font.SetBold();
            ws.Cell(activeRow, 6).SetValue(report.Report[0][0][5]).Style.Font.SetBold();
            activeRow ++;
            for (int i = 1; i < report.Report.Count; i++)
            {
                for(int j = 0; j < report.Report[i].Count; j++)
                {
                    activeRow++;
                    if (j == 0)
                    {
                        ws.Cell(activeRow, 1).SetValue(report.Report[i][j][0]).Style.Font.SetBold();
                        ws.Cell(activeRow, 2).SetValue(report.Report[i][j][1]).Style.Font.SetBold();
                        ws.Cell(activeRow, 3).SetValue(report.Report[i][j][2]).Style.Font.SetBold();
                        ws.Cell(activeRow, 4).SetValue(report.Report[i][j][3]).Style.Font.SetBold();
                        ws.Cell(activeRow, 5).SetValue(report.Report[i][j][4]).Style.Font.SetBold();
                        ws.Cell(activeRow, 6).SetValue(report.Report[i][j][5]).Style.Font.SetBold();
                    }
                    else
                    {
                        ws.Cell(activeRow, 1).SetValue(report.Report[i][j][0]);
                        ws.Cell(activeRow, 2).SetValue(report.Report[i][j][1]);
                        ws.Cell(activeRow, 3).SetValue(report.Report[i][j][2]);
                        ws.Cell(activeRow, 4).SetValue(report.Report[i][j][3]);
                        ws.Cell(activeRow, 5).SetValue(report.Report[i][j][4]);
                        ws.Cell(activeRow, 6).SetValue(report.Report[i][j][5]);
                    }
                }
            }
            ws.Columns().AdjustToContents();
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName =  "Volunteer Start Dates.xlsx" };
        }
        private UsersInRolesReportViewModel GetUsersInRolesReport()
        {
            var report = new UsersInRolesReportViewModel { Report = new List<List<string[]>>() };
            List<string[]> headerLines = new List<string[]>
            {new[] {DateTime.Today.ToShortDateString(), "", "", "", "", "Volunteer Start and End Dates"}};
            report.Report.Add(headerLines);

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
                    List<string[]> lines = new List<string[]>();
                    string str0 = role.Name;
                    if (role.Name == "OfficerOfTheDay") { str0 = "OD"; }
                    lines.Add(new[] { str0, "", "", "Start", "End", "Notes" });
                    foreach (var usr in usersInRole)
                    {
                        var str4 = usr.BeginDate.Year.ToString();
                        if (str4 == "1900") { str4 = ""; }
                        var str5 = usr.LastDate.Year.ToString();
                        // Has to be one year of disuse or inactive to show Ending Year
                        if (usr.Active == true && (usr.LastDate > DateTime.Today.AddYears(-1) || str5 == "1900")) { str5 = ""; }
                        lines.Add(new[] { usr.FirstName, usr.LastName, usr.Email, str4, str5, usr.Notes });
                    }
                    lines.Add(new[] { "", "", "", "", "", "" });   // Space between Roles
                    report.Report.Add(lines);
                }
            }
            // Add "Others" when users have no roles:
            var sqlString = "SELECT DISTINCT UserId FROM AspNetUserRoles";
            var rolesUserIdList = _db.Database.SqlQuery<string>(sqlString).ToList();
            List<string[]> otherLines = new List<string[]>
            {
                new[] { "Others", "", "", "Start", "End", "Notes" }
            };
            foreach (var user in userList)
            {
                var matchingIds = rolesUserIdList
                    .Where(i => i.Contains(user.Id));
                if (!matchingIds.Any())
                {
                    var str4 = user.BeginDate.Year.ToString();
                    if (str4 == "1900") { str4 = ""; }
                    var str5 = user.LastDate.Year.ToString();
                    // Has to be inactive or one year of disuse to show Ending Year
                    if (user.Active==true &&(user.LastDate > DateTime.Today.AddYears(-1) || str5 == "1900")) { str5 = ""; }
                    otherLines.Add(new[] { user.FirstName, user.LastName, user.Email, str4, str5, user.Notes });
                }
            }

            if (otherLines.Count > 0)
            {
                otherLines.Add(new[] { "", "", "", "", "", "" });   // Space between Roles
                report.Report.Add(otherLines);
            }
            return report;
        }
        public ActionResult ReturnToReportsMenu()
        {
            return RedirectToAction("ReportsMenu","Deliveries");
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