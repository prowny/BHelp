using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BHelp.Models;
using LumenWorks.Framework.IO.Csv;
using System.Data;

namespace BHelp
{
    public class Utilities
    {
       // private readonly BHelpContext _db = new BHelpContext();

        public static Boolean UploadClients()
        {
            //using (var context = new BHelpContext())
            //{
            //    List<ApplicationUser> userList = context.Users.ToList();
            //}

            var filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";
            //var Lines = File.ReadLines(filePath).Select(a => a.Split(';'));
            //var CSV = from line in Lines
            //          select (line.Split(',')).ToArray();
           
            //string csvData = File.ReadAllText(filePath);
            //string[] recs = csvData.Split('\n');

            //for (int i = 1; i <= recs.Length; i++)
            //{
            //    string[] fields = Regex.Split(recs[i], ",(?=(?:[^']*'[^']*')*[^']*$)");

            //    string kids = fields[11];
            //    string note = fields[13];
            //    using (var db = new BHelpContext())
            //    {
            //          Client client = new Client();
            //          client.Active = true;
            //          client.FirstName = fields[0];
            //          client.LastName = fields[1];
            //          client.StreetName = fields[2];
            //          client.StreetName = fields[3];
            //          client.City = fields[4];
            //          client.Zip = fields[5];
            //          client.Phone = fields[6];
            //          client.Notes = fields[13];
            //          db.Clients.Add(client);
            //    }
            //}

            DataTable csvtable = new DataTable();
            using (CsvReader csvReader =
                new CsvReader(new StreamReader(filePath), true))
            {
                csvtable.Load(csvReader);
            }

            foreach (DataRow row in csvtable.Rows)
            {
                foreach (DataColumn column in csvtable.Columns)
                {
                    var x = row[0];
                    var y = row[1];
                    var z = row[13];
                    System.Diagnostics.Debug.WriteLine(row[column]);
                }
                
                string dummy = "";
            }
            // open the file "data.csv" which is a CSV file with headers
            //using (CachedCsvReader csv = new
            //    CachedCsvReader(new StreamReader(filePath), true))
            //{
            //    // Field headers will automatically be used as column names
            //    myDataGrid.DataSource = csv;
            //}

            return true;
        }

    }
}