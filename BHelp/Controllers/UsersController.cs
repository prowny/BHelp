﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Net;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using System.IO;
using System.Text;
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
            var user = (from u in _db.Users
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

            if (Request.UrlReferrer == null) return View(user);
            var retUrl = Request.UrlReferrer.ToString();
            if (retUrl.Contains('?'))
            {
                var i = retUrl.IndexOf('?');
                retUrl = retUrl.Substring(0, i);
            }
            user.ReturnURL = retUrl;

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost, Authorize(Roles = "Administrator,Developer")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Active,FirstName,LastName,"
                                + "Title,PhoneNumber,PhoneNumber2,Email,BeginDate,LastDate,"
                                + "Notes,VolunteerCategory,VolunteerSubcategory,Address,City,"
                                + "State,Zip,ReturnURL")] ApplicationUser user)
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

            if (Request.UrlReferrer == null) return View(user);
            var retUrl = Request.UrlReferrer.ToString();
            if (retUrl.Contains('?'))
            {
                var i = retUrl.IndexOf('?');
                retUrl = retUrl.Substring(0, i);
            }
            user.ReturnURL = retUrl;
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(string userId, string returnUrl)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = (from u in _db.Users.Where(u => u.Id == userId) select u).Single();
            if (user == null)
            {
                return HttpNotFound();
            }

            // Check for Okay to Delete:    
            user.OKtoDelete = true;
            using var db = new BHelpContext();
            {
                var sqlString = "SELECT UserId FROM AspNetUserRoles WHERE ";
                sqlString += "UserId = '" + userId + "'";
                var success = db.Database.SqlQuery<string>(sqlString).FirstOrDefault();
                if (success != null) user.OKtoDelete = false;

                var dels = db.Deliveries.FirstOrDefault(d => d.DeliveryDateODId == userId);
                if (dels != null) user.OKtoDelete = false;
                dels = db.Deliveries.FirstOrDefault(d => d.ODId == userId);
                if (dels != null) user.OKtoDelete = false;
                dels = db.Deliveries.FirstOrDefault(d => d.DriverId == userId);
                if (dels != null) user.OKtoDelete = false;

                var ods = db.ODSchedules.FirstOrDefault(o => o.ODId == userId);
                if (ods != null) user.OKtoDelete = false;

                var drvrs = db.DriverSchedules.FirstOrDefault(d => d.DriverId == userId);
                if (drvrs != null) user.OKtoDelete = false;
                drvrs = db.DriverSchedules.FirstOrDefault(d => d.DriverId == userId);
                if (drvrs != null) user.OKtoDelete = false;
                drvrs = db.DriverSchedules.FirstOrDefault(d => d.BackupDriverId  == userId);
                if (drvrs != null) user.OKtoDelete = false;
                drvrs = db.DriverSchedules.FirstOrDefault(d => d.BackupDriver2Id == userId);
                if (drvrs != null) user.OKtoDelete = false;


                var hrs = db.VolunteerHours.FirstOrDefault(h => h.UserId == userId);
                if (hrs != null) user.OKtoDelete = false;
                hrs = db.VolunteerHours.FirstOrDefault(h => h.OriginatorUserId == userId);
                if (hrs != null) user.OKtoDelete = false;
            }

            if (user.OKtoDelete == false)
            {
                return RedirectToAction("DeleteDenied");
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
        public ActionResult DeleteConfirmed(string userId)
        {
            var user = (from u in _db.Users.Where(u => u.Id == userId) select u).Single();
            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteDenied()
        {
            return View();
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
        
        public ActionResult VolunteerDatesReportToCSV()
        {
            var report = GetUsersInRolesReport();
            
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

        //[Authorize(Roles = "Administrator,Developer")]
        //public ActionResult ReturnToReportsMenu()
        //{
        //    return User.Identity.Name.IsNullOrEmpty() ? RedirectToAction("Login", "Account") : RedirectToAction("ReportsMenu", "Deliveries");
        //}

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