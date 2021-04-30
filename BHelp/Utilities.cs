using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BHelp.Models;

namespace BHelp
{
    public class Utilities
    {
        //private readonly BHelpContext _db = new BHelpContext();
        public static Boolean UploadClients()
        {
            using (var context = new BHelpContext())
            {
                List<ApplicationUser> userList = context.Users.ToList();
            }

            string filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";
            var Lines = File.ReadLines(filePath).Select(a => a.Split(';'));
            //var CSV = from line in Lines
            //          select (line.Split(',')).ToArray();
            //string dummy = "";
           
            return true;
        }

    }
}