using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Net;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using System.IO;
using System.Text;
using Castle.Core.Internal;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        readonly BHelpContext _db = new BHelpContext();

        // GET: Users
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Index()
        {
            return View(_db.Users.OrderBy(u => u.LastName));
        }
        // GET: Users/Edit/5
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Edit(string userName)
        {
            if (userName == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = (from u in _db.Users
                .Where(u => u.UserName == userName) select u).Single();
            if (user == null)
            {
                return HttpNotFound();
            }

            user.VolunteerCategories = HoursRoutines.GetHoursCategoriesSelectList(false, false);
            foreach (var cat in user.VolunteerCategories)
            {
                if (user.VolunteerCategory == cat.Value) cat.Selected = true;
            }

            user.VolunteerSubcategories = HoursRoutines.GetHoursSubcategoriesSelectList(user);
            foreach (var subCat in user.VolunteerSubcategories)
            {
                if (user.VolunteerSubcategory == subCat.Value) subCat.Selected = true;
            }

            user.States = AppRoutines.GetStatesSelectList();
            foreach (var state in user.States)
            {
                if (user.State == state.Value)
                {
                    state.Selected = true;
                }
            }

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost, Authorize(Roles = "Administrator,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Active,FirstName,LastName,"
                                + "Title,PhoneNumber,PhoneNumber2,Email,BeginDate,LastDate,"
                                + "Notes,VolunteerCategory,VolunteerSubcategory,Address,City,"
                                + "State,Zip")] ApplicationUser user)
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
                saveUser.PhoneNumber2 = user.PhoneNumber2;
                saveUser.Address = user.Address;
                saveUser.City = user.City;
                saveUser.State = user.State;
                saveUser.Zip = user.Zip;
                saveUser.Email = user.Email;
                saveUser.BeginDate = user.BeginDate;
                saveUser.LastDate = user.LastDate;
                saveUser.Notes = user.Notes;
                saveUser.VolunteerCategory = user.VolunteerCategory;
                saveUser.VolunteerSubcategory = user.VolunteerSubcategory;
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
        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult Create()
        {
            return RedirectToAction("Register", "Account");
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete"), Authorize(Roles = "Administrator,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string userName)
        {
            ApplicationUser user = (from u in _db.Users.Where(u => u.UserName == userName) select u).Single();
            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult VolunteerDatesReport()
        {
            var report = GetUsersInRolesReport();
            return View(report);
        }

        [Authorize(Roles = "Administrator,Developer")]
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
                        ws.Cell(activeRow, 7).SetValue(report.Report[i][j][6]).Style.Font.SetBold();
                    }
                    else
                    {
                        ws.Cell(activeRow, 1).SetValue(report.Report[i][j][0]);
                        ws.Cell(activeRow, 2).SetValue(report.Report[i][j][1]);
                        ws.Cell(activeRow, 3).SetValue(report.Report[i][j][2]);
                        ws.Cell(activeRow, 4).SetValue(report.Report[i][j][3]);
                        ws.Cell(activeRow, 5).SetValue(report.Report[i][j][4]);
                        ws.Cell(activeRow, 6).SetValue(report.Report[i][j][5]);
                        ws.Cell(activeRow, 7).SetValue(report.Report[i][j][6]);
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

        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult WeeklyInfoReport(DateTime? monday)
        {
            if (monday != null)
            {
            }
            else
            {
                monday = AppRoutines.GetMondaysDate(DateTime.Today);
            }

            var beginDate = (DateTime)monday;
            var view = new ReportsViewModel
            {
                BeginDate = beginDate,
                DateRangeTitle = beginDate.ToString("MM/dd/yyyy") + " - " + beginDate.AddDays(4).ToString("MM/dd/yyyy")
            };
            return View(view);
        }

        public ActionResult WeekPrevious(DateTime monday)
        {
            monday = monday.AddDays(-7);
            return RedirectToAction("WeeklyInfoReport", new { monday = monday });
        }

        public ActionResult WeekNext(DateTime monday)
        {
            monday = monday.AddDays(7);
            return RedirectToAction("WeeklyInfoReport", new { monday = monday });
        }

        public ActionResult VolunteerDatesReportToCSV()
        {
            UsersInRolesReportViewModel report = GetUsersInRolesReport();


            var sb = new StringBuilder();
            sb.Append(",," + report.Report[0][0][0] + ",," + report.Report[0][0][5]);
            sb.AppendLine();
            sb.AppendLine();

            for (var i = 1; i < report.Report.Count; i++)
            {
                for (var j = 0; j < report.Report[i].Count; j++)
                {
                    sb.Append(report.Report[i][j][0] + ',');
                    sb.Append(report.Report[i][j][1] + ',');
                    sb.Append(report.Report[i][j][2] + ',');
                    sb.Append(report.Report[i][j][3] + ',');
                    sb.Append(report.Report[i][j][4] + ',');
                    sb.Append("\"" + report.Report[i][j][5] + "\"" + ',');
                    sb.Append(report.Report[i][j][6]);
                    sb.AppendLine();
                }
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                                                      + "UserRolesReport.csv");
            response.ContentType = "text/plain";
            response.Write(sb);
            response.End();
            return null;
        }

        private UsersInRolesReportViewModel GetUsersInRolesReport()
        {
            var report = new UsersInRolesReportViewModel { Report = new List<List<string[]>>() };
            List<string[]> headerLines = new List<string[]>
            {new[] {DateTime.Today.ToShortDateString(), "", "", "", "", "Volunteer Roles and Start / End Dates", "" }};
            report.Report.Add(headerLines);
            var userList = _db.Users.OrderBy(u => u.LastName).ToList();
            var rolesList = _db.Roles.OrderBy(r => r.Name).ToList();
            var roleLookup = AppRoutines.UsersInRolesLookup();
            foreach (var role in rolesList)
            {
                var usersInRole = new List<ApplicationUser>();

                foreach (var user in userList)
                {
                    //if (AppRoutines.UserIsInRole(user.Id, role.Name))
                    if (roleLookup.Any(r => r.UserId == user.Id && r.RoleId == role.Id))
                    {
                        usersInRole.Add(user);
                    }
                }
                if (usersInRole.Count > 0)
                {
                    List<string[]> lines = new List<string[]>();
                    string str0 = role.Name;
                    if (role.Name == "OfficerOfTheDay") { str0 = "OD"; }
                    lines.Add(new[] { str0, "", "Active", "Start", "End", "Notes", "Email" });
                    foreach (var usr in usersInRole)
                    {
                        var str4 = usr.BeginDate.Year.ToString();
                        if (str4 == "1900") { str4 = ""; }
                        var str5 = usr.LastDate.Year.ToString();
                        // Has to be one year of disuse or inactive to show Ending Year REMOVED 11/28/2021
                        //if (usr.Active && (usr.LastDate > DateTime.Today.AddYears(-1) || str5 == "1900")) { str5 = ""; }
                        if (usr.Active && str5 == "1900") str5 = "";
                        if (!usr.Active && str5 == "1900")
                        {
                            str5 = str4;
                        } 
                        lines.Add(new[] { usr.FirstName, usr.LastName, usr.Active.ToString( ), str4, str5, usr.Notes, usr.Email });
                    }
                    lines.Add(new[] { "", "", "", "", "", "","" });   // Space line between Roles
                    report.Report.Add(lines);
                }
            }
            // Add "Others" when users have no roles:
            var sqlString = "SELECT DISTINCT UserId FROM AspNetUserRoles";
            var rolesUserIdList = _db.Database.SqlQuery<string>(sqlString).ToList();
            List<string[]> otherLines = new List<string[]>
            {
                new[] { "Others", "", "Active", "Start", "End", "Notes", "Email" }
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
                    if (user.Active &&(user.LastDate > DateTime.Today.AddYears(-1) || str5 == "1900")) { str5 = ""; }
                    otherLines.Add(new[] { user.FirstName, user.LastName, user.Active.ToString( ), str4, str5, user.Notes, user.Email });
                }
            }

            if (otherLines.Count > 0)
            {
                otherLines.Add(new[] { "", "", "", "", "", "", "" });   // Space between Roles
                report.Report.Add(otherLines);
            }
            return report;
        }
        
        public ActionResult ActiveVolunteerDetailsToExcel()
        {
            var db = new BHelpContext();
            var activeVolunteersList = db.Users
                .Where(u => u.Active).OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName).ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Active Volunteers");
            ws.Columns("1").Width = 10;
            ws.Cell(1, 1).SetValue("Active Volunteers").Style.Font.SetBold(true);
            ws.Cell(1, 1).Style.Alignment.WrapText = true;
            ws.Cell(1, 2).SetValue(DateTime.Today.ToShortDateString()).Style.Font.SetBold(true);
            ws.Cell(2, 1).SetValue("Last Name").Style.Font.SetBold(true);
            ws.Columns("2").Width = 15;
            ws.Cell(2, 2).SetValue("First Name").Style.Font.SetBold(true);
            ws.Columns("3").Width = 15;
            ws.Cell(2, 3).SetValue("Title").Style.Font.SetBold(true);
            ws.Columns("4").Width = 40;
            ws.Cell(2, 4).SetValue("Address").Style.Font.SetBold(true);
            ws.Cell(2, 5).SetValue("City").Style.Font.SetBold(true);
            ws.Cell(2, 6).SetValue("State").Style.Font.SetBold(true);
            ws.Cell(2, 7).SetValue("Zip Code").Style.Font.SetBold(true);
            ws.Columns("8").Width = 40;
            ws.Cell(2, 8).SetValue("Email").Style.Font.SetBold(true);
            ws.Columns("9").Width = 12;
            ws.Cell(2, 9).SetValue("Phone 1").Style.Font.SetBold(true);
            ws.Columns("10").Width = 12;
            ws.Cell(2, 10).SetValue("Phone 2").Style.Font.SetBold(true);
            ws.Cell(2, 11).SetValue("Roles").Style.Font.SetBold(true);
            ws.Cell(2, 11).SetValue("Notes").Style.Font.SetBold(true);
            
            var activeRow = 2;
            foreach (var vol in activeVolunteersList)
            {
                activeRow++;
                ws.Cell(activeRow, 1).SetValue(vol.LastName);
                ws.Cell(activeRow, 2).SetValue(vol.FirstName);
                ws.Cell(activeRow, 3).SetValue(vol.Title);
                ws.Cell(activeRow, 4).SetValue(vol.Address);
                ws.Cell(activeRow, 5).SetValue(vol.City);
                ws.Cell(activeRow, 6).SetValue(vol.State);
                ws.Cell(activeRow, 7).SetValue(vol.Zip);
                ws.Cell(activeRow, 8).SetValue(vol.Email);
                ws.Cell(activeRow, 9).SetValue(vol.PhoneNumber);
                ws.Cell(activeRow, 10).SetValue(vol.PhoneNumber2);
                var volRoles = AppRoutines.GetStringAllRolesForUser(vol.Id);
                ws.Cell(activeRow, 11).SetValue(volRoles);
                ws.Cell(activeRow, 12).SetValue(vol.Notes);
            }

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = "Active Volunteers" + DateTime.Today.ToString("MM-dd-yy") + ".xlsx" };
        }

        public ActionResult ActiveVolunteerDetailsToCSV()
        {
            var db = new BHelpContext();
            var activeVolunteersList = db.Users
                .Where(u => u.Active).OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName).ToList();
            
            var sb = new StringBuilder();
            sb.Append("Active Volunteers" + ',');
            sb.Append(DateTime.Today.ToShortDateString());
            sb.AppendLine();

            sb.Append("Last Name" + ',');
            sb.Append("First Name" + ',');
            sb.Append("Title" + ',');
            sb.Append("Address" + ',');
            sb.Append("City" + ',');
            sb.Append("State" + ',');
            sb.Append("Zip Code" + ',');
            sb.Append("Email" + ',');
            sb.Append("Phone 1" + ',');
            sb.Append("Phone 2" + ',');
            sb.Append("Roles" + ',');
            sb.Append("Notes");
            sb.AppendLine();

            foreach (var vol in activeVolunteersList)
            {
                sb.Append(vol.LastName + ',');
                sb.Append(vol.FirstName + ',');
                sb.Append("\"" + vol.Title + "\""  + ',');
                sb.Append("\"" + vol.Address + "\"" + ',');
                sb.Append(vol.City + ',');
                sb.Append(vol.State + ',');
                sb.Append(vol.Zip + ',');
                sb.Append(vol.Email + ',');
                sb.Append(vol.PhoneNumber + ',');
                sb.Append(vol.PhoneNumber2 + ',');
                sb.Append(AppRoutines.GetStringAllRolesForUser(vol.Id) + ',');
                sb.Append(vol.Notes);
                sb.AppendLine();
            }

            var response = System.Web.HttpContext.Current.Response;
            response.BufferOutput = true;
            response.Clear();
            response.ClearHeaders();
            response.ContentEncoding = Encoding.Unicode;
            response.AddHeader("content-disposition", "attachment;filename="
                   + "Active Volunteers" + DateTime.Today.ToString("MM-dd-yy") + ".csv" );
            response.ContentType = "text/plain";
            response.Write(sb);
            response.End();
            return null;
        }


        [Authorize(Roles = "Administrator,Developer")]
        public ActionResult ReturnToReportsMenu()
        {
            return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("ReportsMenu", "Deliveries");
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