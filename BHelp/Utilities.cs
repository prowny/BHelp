using BHelp.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BHelp.Models;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DataTable = System.Data.DataTable;

namespace BHelp
{
    public class Utilities
    {
        public static Boolean UploadClients()
        {
            var db = new BHelpContext();

            var filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";

            DataTable csvtable = new DataTable();
            using (CsvReader csvReader =
                new CsvReader(new StreamReader(filePath), true))
            {
                csvtable.Load(csvReader);
            }

            foreach (DataRow row in csvtable.Rows)
            {
                Client client = new Client()
                {
                    Active = true,
                    FirstName = row[0].ToString(),
                    LastName = row[1].ToString(),
                    StreetNumber = row[2].ToString(),
                    StreetName = row[3].ToString(),
                    City = row[4].ToString(),
                    Zip = row[5].ToString(),
                    Phone = row[6].ToString(),
                    Notes = row[13].ToString()
                };
                db.Clients.Add(client);
                db.SaveChanges();
                //System.Diagnostics.Debug.WriteLine(client.FirstName, client.LastName);
            }



            return true;
        }

        public static Boolean UploadAdults()
        {
            var db = new BHelpContext();

            var filePath = @"c:\TEMP\BH Food Client List-Table 1.csv";

            DataTable csvtable = new DataTable();
            using (CsvReader csvReader = new CsvReader(new StreamReader(filePath), true))
            { csvtable.Load(csvReader); }

            int i = -1;
            foreach (DataRow row in csvtable.Rows)
            {
                i ++;
                string _adults = row[10].ToString();
                List<FamilyMember> adults = new List<FamilyMember>();
                string[] adultsArray = _adults.Split(',');
                foreach (var nameAge in adultsArray)
                {
                    if (nameAge.Contains(row[0].ToString() ) && nameAge.Contains(row[1].ToString())) // First and Last Name
                    {
                        //Don't add to FamilyMembers - just capture the age & update the client DoB
                        int ageIndex = nameAge.IndexOf('/');
                        if (ageIndex > 0)
                        {
                            var years = nameAge.Substring(nameAge.IndexOf('/') +1);
                            //System.Diagnostics.Debug.WriteLine(i.ToString() + ' ' + row[0].ToString() + ' ' + row[1].ToString() + ' ' + years);
                          
                          if (int.TryParse(years, out int yy))
                          {
                              // Everybody born on April 1st
                              DateTime doB = new DateTime(2021-yy, 4, 1);
                              var original = db.Clients.Find(i);
                              if (original != null)
                              {
                                  original.DateOfBirth = doB;
                                  //db.SaveChanges();
                              }
                            }
                        }
                    }
                    else // not a client -add a family member =================
                    {
                        var years = nameAge.Substring(nameAge.IndexOf('/') + 1);
                        if (int.TryParse(years, out int yy))
                        {
                            DateTime doB = new DateTime(2021 - yy, 4, 1);
                            string fullName = nameAge.Substring(0, nameAge.IndexOf('/') - 1);
                            string firstName = fullName.Substring(0, nameAge.IndexOf(' ') - 1);
                            string lastName = fullName.Substring(nameAge.IndexOf(' ') + 1);
                            FamilyMember newAdult =new FamilyMember()
                            {
                                Active = true,
                                FirstName = firstName,
                                LastName = lastName,
                                DateOfBirth = doB
                            };
                            db.FamilyMembers.Add(newAdult);
                            //db.SaveChanges();
                        }
                    }
                }

                
                //System.Diagnostics.Debug.WriteLine(client.FirstName, client.LastName);
            }

            return true;
        }
    }
}