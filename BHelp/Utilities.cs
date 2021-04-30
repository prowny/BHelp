using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebPages.Html;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp
{
    public class Utilities
    {
        private readonly BHelpContext _db = new BHelpContext();
        public static Boolean UploadClients()
        {
            using (var context = new BHelpContext())
            {
                List<ApplicationUser> userList = context.Users.ToList();
                string dummy = "";
            }

            return true;
        }

    }
}