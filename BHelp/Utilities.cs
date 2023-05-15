// Utilities used by developer //

using System;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;

namespace BHelp
{
    public static class Utilities
    {
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
    }
}

