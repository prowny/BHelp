using System.Web.Mvc;
using BHelp.ViewModels;

namespace BHelp.Controllers
{
    public class DriverEmailController : Controller
    {
        // GET: DriverEmail
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DriverEmailDisplay(int year, int month)
        {
            //var db = new BHelpContext();
            var email = new DriverEmailViewModel
            {
                Title = "Email Drivers - "
            };
        
            var text = "Dear Drivers,\n";
            text += "Please check current driver assignments to be sure it is correct. Note the many ";
            text += "TBDs. If you are able to fill any of them, please let me know.\n\n";
            text += "IMPORTANT: You can also view the current schedule with driver contact info ";
            text += "included at bethesdahelpfd.com.\n";
            
            email.EmailText = text;
            //var startDate = DateTime.Today;
            //var endDate = DateTime.Today;

            email.HtmlContent = GetDriverSchedule(year, month);
            //var allUsers = db.Users.ToList();
            //List<ApplicationUser> recipients = new List<ApplicationUser>();
            //foreach (ApplicationUser user in allUsers)
            //{
            //    if (AppRoutines.UserIsInRole(user.Id, "Driver")
            //        || AppRoutines.UserIsInRole(user.Id, "Scheduler"))
            //    {
            //        recipients.Add(user);
            //    }
            //}

            //var newList = new List<DriverEmailRecipient>();
            //foreach (ApplicationUser user in recipients)
            //{
            //    newList.Add(new DriverEmailRecipient()
            //    { Id = user.Id, FullName = user.FullName, Email = user.Email, Checked = true });
            //}

            //TempData["Recipients"] =
            //    newList; // TempData holds complex data not passed in Redirects.                            
            //email.Recipients = newList;
            //return RedirectToAction("Index", email);


            return View(email);
        }

        private static string GetDriverSchedule(int year, int month)
        {
            //var db = new BHelpContext();
            //var scheds = db.DriverSchedules
            //    .Where(d => d.Date >= startDate && d.Date <= endDate).ToList();
            var qt = (char)34; // Quote
            var hdrStyle = " style= " + qt + "text-align:center; " + qt + " width = " + qt + "135px" + qt;
            var content = "<table border=1 style =" + qt + "border-collapse:collapse;" + qt + ">";
            content += "<tr><td" + hdrStyle + "><b>MONDAY</b></td>";
            content += "<td" + hdrStyle + "><b>TUESDAY</b></td>";
            content += "<td" + hdrStyle + "><b>WEDNESDAY</b></td>";
            content += "<td" + hdrStyle + "><b>THURSDAY</b></td>";
            content += "<td" + hdrStyle + "><b>FRIDAY</b></td></tr>";

            for (var i = 1; i < 6; i++)
            {
                content += "<tr>";
                content += "<td></td>";
                content += "<td></td>";
                content += "<td></td>";
                content += "<td></td>";
                content += "<td></td>";
                content += "</tr>";
            }

            content += "</table>";

            return (content);
        }
    }
}