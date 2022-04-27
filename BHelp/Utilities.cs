// Utilities used by developer //

namespace BHelp
{
    public static class Utilities
    {
        public static int test()
        {
            //var a = 0;
            //var b = 0;
            //for (var i = 1; i < 26; i++)
            //{
            //    if (i < 6) {  a = 1;  b = i; }
            //    if (i > 5 && i < 11) {  a = 2;  b = i - 5; }
            //    if (i > 10 && i < 16) { a = 3;  b = i - 10; }
            //    if(i > 15 && i < 21) { a = 4;  b = i - 15; }
            //    if (i > 20) { a = 5;  b = i - 20; }
            // }
            //var z = 5 / 5 + 1 ;
            //var x = (5 % 5);
            //var y = 20 / 5;
            
            return 1;
        }
        public static void GetLatestDeliveries()
        {
            //var db = new BHelpContext();
            //var result = (from pi in db.Clients 
            //    join pu in db.Deliveries  on pi.Id equals pu.ClientId  into tpu
            //    from t in tpu.OrderByDescending( c => c.DateDelivered).Take(1)
            //    select new { pi.Id , pi.LastName, t.DateDelivered}).ToList();

            //var sql = "select c.Id, c.LastName,c.FirstName, COUNT(d.Id) as total "
            //          + "from Clients as c left join Deliveries as d on c.Id = d.ClientId "
            //          + "group by c.Id, c.LastName, c.FirstName order by total";
            //var results = db.Database.ExecuteSqlCommand(sql);

            //: 'The data reader has more than one field. Multiple fields are not valid for EDM primitive or enumeration types.'
            //var sql = "select c.LastName, c.FirstName, c.Id, latest_deliveries.DateDelivered "
            //          + "from(select ClientId, MAX(DateDelivered) as DateDelivered "
            //          + "from dbo.[Deliveries] d  where d.Status = 1 Group By ClientId) "
            //          + "as latest_deliveries "
            //          + "inner join dbo.[Clients] c on c.Id = latest_deliveries.ClientId "
            //          + "where c.Active = 1 "
            //          + "order by latest_deliveries.DateDelivered";
            //var success = db.Database.SqlQuery<string>(sql).ToList();

            //var results = (from c in db.Clients
            //    join d in db.Deliveries on c.Id equals d.ClientId into cd
            //    from t in cd.OrderBy(x => x.DateDelivered).FirstOrDefault()
            //    select new
            //    {
            //        c.Id, DateDelivered = t.DateDelivered,
            //        LastName = c.LastName, FirstName = c.FirstName
            //    }).ToList();

            //var delRecs = results
            //    .OrderBy(d => d.DateDelivered);

            //foreach (var item in results )
            //{
            //    var dt = item.DateDelivered ?? DateTime.Now ;
            //    DeliveryViewModel del = new DeliveryViewModel()
            //    {
            //        DateDeliveredString = dt .ToString("MM/dd/yyyy"),
            //        LastName = item.LastName ,
            //        FirstName =item.FirstName
            //    };
            //}

            //var clientRecIds = db.Clients.Where(a => a.Active)
            //    .Select(i => i.Id).ToList();

            //var delList = new List<DeliveryViewModel>();
            //foreach (var del in results)
            //{
            //    if (clientRecIds.Contains(del.Id))
            //    {
            //        var newDel = new DeliveryViewModel()
            //        {
            //            ClientId = del.Id,
            //            FirstName = del.FirstName 
            //        };
            //        delList.Add(newDel);
            //    }
            //}

            //var deliveryClientIds = db.Deliveries
            //    .Where(s => s.Status == 1)
            //    .GroupBy(d => d.ClientId)
            //    .Select(g => g.OrderBy(d => d.DateDelivered)
            //        .FirstOrDefault())
            //    .Select(i => i.ClientId).ToList();

            //var clientsWithDeliveries = new List<Client>();
            //var clientsWithNoDeliveries = new List<Client>();
            //foreach (var client in db.Clients)
            //{
            //    if (deliveryClientIds.Contains(client.Id))
            //    {
            //        clientsWithDeliveries.Add(client);
            //    }
            //    else
            //    {
            //        clientsWithNoDeliveries.Add(client);
            //    }
            //}

            //var dummy = "";
        }

    }
}

