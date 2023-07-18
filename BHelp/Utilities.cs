// Utilities used by developer //

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;
using Org.BouncyCastle.Utilities;

namespace BHelp
{
    public static class Utilities
    {
        public static string ConvertPaymentsToDecimal()
        {
            using (var db = new BHelpContext())
            {
                var payments = db.AssistancePayments.ToList();
                foreach (var pymt in payments)
                {
                    var amt = $"{pymt.AmountInCents / 100}.{pymt.AmountInCents % 100:00}";
                    amt = amt.Replace(".-", ".");
                    pymt.AmountDecimal = Convert.ToDecimal(amt);
                }
                db.SaveChanges();
            }
            return null;
        }
        public static void test()
        {
            using var db = new BHelpContext();
            var startDt = new DateTime(2022, 01, 01);
            var endDt = new DateTime(2022, 03, 01);
            var recs = db.Deliveries.Where(d => d.DateDelivered >= startDt
                                                && d.DateDelivered <= endDt
                                                && d.DeliveryDateODId != null
                                                && d.Status == 1).ToList();
            var result = recs.GroupBy(x => x.DateDelivered)
                .Select(x => x.First()).ToList();
            foreach (var del in result)
            {
                if (del.DateDelivered != null)
                {
                    var addODRec = new ODSchedule()
                    {
                        Date = (DateTime)del.DateDelivered,
                        ODId = del.DeliveryDateODId
                    };
                    db.ODSchedules.Add(addODRec);
                }
            }

            recs = db.Deliveries.Where(d => d.DateDelivered >= startDt
                                            && d.DateDelivered <= endDt
                                            && d.DriverId != null
                                            && d.Status == 1).ToList();
            result = recs.GroupBy(x => x.DateDelivered)
                .Select(x => x.First()).ToList();
            foreach (var dlv in result)
            {
                if (dlv.DateDelivered != null)
                {
                    var addDrRec = new DriverSchedule()
                    {
                        Date = (DateTime)dlv.DateDelivered,
                        DriverId = dlv.DriverId
                    };
                    db.DriverSchedules.Add(addDrRec);
                }
            }

            db.SaveChanges();
        }

        public static bool CompareNoCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
        }
        
        public static void BetaClientList() //public ActionResult BetaClientList()
        {
            var db = new BHelpContext();

            var betaList = new List<Client>();
            var matchList = new List<Client>();
            var distinctList = new List<Client>();
            var missingList = new List<Client>();
            var distinctMissingList = new List<Client>();

            var matches = 0;
            var recCount = 0;
            var clientList = db.Clients.OrderBy(n => n.LastName)
                .ThenBy(f => f.FirstName).ToList();
            // Generate list names all caps
            foreach (var client in clientList)
            {
                var betaClient = client;
                betaClient.LastName = Strings.ToUpperCase(client.LastName);
                betaClient.FirstName = Strings.ToUpperCase(client.FirstName);
                betaList.Add(betaClient);
            }

            // get payments list
            //var assistanceList = new List<AssistancePayment>();
            var reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/AssistancePayments.csv");
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    var values = line.Split(',');
                    if (values[0].Length > 0) recCount++;
                    var search = betaList.FirstOrDefault(s => s.LastName == values[0]
                                                              && s.FirstName == values[1]);

                    if (search == null)
                    {
                        if (values[0] != "")
                        {
                            var missingClient = new Client()
                            {
                                LastName = values[0],
                                FirstName = values[1],
                                StreetNumber = "",
                                StreetName = values[2].Trim(),
                                City = values[3],
                                Zip = values[4]
                            };

                            if (missingClient.StreetName.Length > 0)
                            {
                                var stName = missingClient.StreetName;
                                var stNumber = "";
                                for (var i = 1; i < 6; i++)
                                {
                                    var _num = stName.Substring(0, i);
                                    if (int.TryParse(_num, out var n)) // is string numeric integer?
                                    {
                                        stNumber = stName.Substring(0, i);
                                    }
                                    Debug.Write(n); // to remove Resharper unused warnings
                                }

                                if (stNumber.Length > 0)
                                {
                                    missingClient.StreetNumber = stNumber.Trim();
                                    missingClient.StreetName =
                                        missingClient.StreetName.Substring(stNumber.Length).Trim();
                                }
                            }

                            missingList.Add(missingClient);
                        }
                    }
                    else
                    {
                        matches++;
                        matchList.Add(search);
                    }
                    //var assistanceRecord = new AssistanceViewModel(); 
                    //for (var i = 0; i < 9; i++)
                    //{
                    //    assistanceRecord.ClientId = Convert.ToInt32(values[i]);
                    //}
                }

                distinctList = matchList.GroupBy(i => i.Id)
                    .Select(s => s.FirstOrDefault()).ToList();

                distinctMissingList = missingList.GroupBy(n => new { n.LastName, n.FirstName })
                    .Select(n => n.First()).ToList();
            }

            // Insert New Clients
            foreach (var cli in distinctMissingList)
            {
                cli.Active = true;
                if (cli.StreetName.Length < 5)
                {
                    cli.StreetNumber = "10100";
                    cli.StreetName = "Old Georgetown Road";
                    cli.City = "Bethesda";
                    cli.Zip = "20814";
                }

                if (cli.Zip.Length < 5) cli.Zip = "20184";
                cli.Phone = "(none)";
                cli.Email = "(none)";
                cli.Notes = "Added fron Assistance List";
            }

            using (var context = new BHelpContext())
            {
                foreach (var item in distinctMissingList)
                {
                    context.Clients.Add(item);
                    context.SaveChanges();
                }
            }

            Debug.Write(matches); // to remove Resharper unused warnings
            Debug.Write(recCount); // to remove Resharper unused warnings
            Debug.Write(distinctList); // to remove Resharper unused warnings

            //var view = distinctMissingList;
            //var view = betaList;
            //return View(view);
        }

        public static string UpdateClientDateCreated(int clientId, DateTime date)
        {
            var db = new BHelpContext();
            var client = db.Clients.Find(clientId);
            if (client != null)
            {
                client.DateCreated = date;
                db.SaveChanges();
            }
            return String.Empty;
        }
       
    }
}

