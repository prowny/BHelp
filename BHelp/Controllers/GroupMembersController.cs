using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BHelp.DataAccessLayer;
using BHelp.Models;
using BHelp.ViewModels;
using ClosedXML.Excel;

namespace BHelp.Controllers
{
    [Authorize]
    public class GroupMembersController : Controller
    {
        private readonly BHelpContext db = new BHelpContext();

        // GET: GroupMembers
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Index()
        {
            var groupMembersView = new GroupMemberViewModel()
            {
                GroupNameSelectList = new List<SelectListItem>(),
                GroupMemberSelectList = new List<SelectListItem>(),
                AllClients = new List<SelectListItem>(),
                SelectedGroupId = null
            };
            var groupNamesList = db.GroupNames.OrderBy(n => n.Name).ToList();
            foreach (var gName in groupNamesList)
            {
                groupMembersView.GroupNameSelectList.Add(new SelectListItem()
                    { Text = gName.Name, Value = gName.Id.ToString(), Selected = false });
            }

            return View(groupMembersView);
        }

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Remove(int? clientId)
        {
            var _gpId = Convert.ToInt32(Session["GroupId"]);
            var memberList = db.GroupMembers.Where(g => g.NameId == _gpId).ToList();
            foreach (var mbr in memberList)
            {
                if (mbr.ClientId == clientId)
                {
                    GroupMember member = db.GroupMembers.First(m => m.NameId == _gpId
                                                                    && m.ClientId == clientId);
                    if (member != null)
                    {
                        db.GroupMembers.Remove(member);
                        db.SaveChanges();
                    }
                }
            }

            return RedirectToAction("MaintainGroupMembers", new { groupId = _gpId });
        }

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult ChangeClientSortOrder(string sortOrder)
        {
            switch (sortOrder)
            {
                case "LastName":
                    Session["ClientSortOrder"] = 0;
                    break;
                case "StreetName":
                    Session["ClientSortOrder"] = 1;
                    break;
            }

            return RedirectToAction("MaintainGroupMembers",
                new { groupId = (int)TempData["SelectedGroupId"] });
        }

        // GET: GroupMembers/Create
        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult MaintainGroupMembers(int? groupId)
        {
            if (Session["ClientSortOrder"] == null) Session["ClientSortOrder"] = 0; // deffault LastName
            var memberViewModel = new GroupMemberViewModel
            {
                SelectedGroupId = groupId,
                AllClients = new List<SelectListItem>(),
                GroupMemberSelectList = new List<SelectListItem>(),
                ClientSortOrder = (int)Session["ClientSortOrder"],
                SelectedSortOrder = "LastName"
            };

            if (memberViewModel.ClientSortOrder == 1)
            {
                memberViewModel.SelectedSortOrder = "StreetName";
            }

            if (groupId != null)
            {
                TempData["SelectedGroupId"] = groupId;
                Session["GroupId"] = groupId.ToString();
                var group = db.GroupNames.Find(groupId);
                if (group != null)
                {
                    memberViewModel.NameId = (int)groupId;
                    memberViewModel.SelectedGroupName = group.Name;
                    memberViewModel.GroupMemberSelectList = GetGroupMembers(group.Id);
                    memberViewModel.AllClients = GetAllClientsSerlectList(memberViewModel);
                }
            }

            return View(memberViewModel);
        }

        public List<SelectListItem> GetGroupMembers(int groupId)
        {
            var groupMemberSelectList = new List<SelectListItem>();
            var clientGroupMembers = db.GroupMembers
                .Where(g => g.NameId == groupId).ToList();
            foreach (var member in clientGroupMembers)
            {
                var client = db.Clients.Find(member.ClientId);
                if (client != null)
                {
                    var _text = client.LastFirstName + " " + client.StreetNumber + " ";
                    _text += client.StreetName;
                    _text += "|phone|" + client.Phone;  // insert text separator for Phone
                    var _val = client.Id.ToString();
                    groupMemberSelectList.Add(new SelectListItem()
                        { Text = _text, Value = _val, Selected = false });
                }
            }

            return groupMemberSelectList;
        }

        public List<SelectListItem> GetAllClientsSerlectList(GroupMemberViewModel view)
        {
            var existingMemberIds = string.Empty;
            foreach (var mbr in view.GroupMemberSelectList)
            {
                existingMemberIds += mbr.Value + ".";
            }

            List<Client> clientList;
            var allClientsSelectList = new List<SelectListItem>()
                { new SelectListItem { Text = @"-Select Client to Add To Group-", Value = "0" } };
            if (view.ClientSortOrder == 0) // by LastName
            {
                clientList = db.Clients.OrderBy(n => n.LastName)
                    .ThenBy(n => n.FirstName).Where(a => a.Active).ToList();
            }
            else // by StreetName
            {
                clientList = db.Clients.OrderBy(n => n.StreetName)
                    .ThenBy(n => n.StreetNumber).ThenBy(n => n.LastName)
                    .ThenBy(n => n.FirstName).Where(a => a.Active).ToList();
            }

            foreach (var client in clientList)
            {
                string text;
                if (view.ClientSortOrder == 0)
                {
                    text = client.LastName + ", " + client.FirstName + " "
                           + client.StreetNumber + " " + client.StreetName;
                }
                else
                {
                    var _padding = new string('\xA0', 40);
                    var _streetNo = (client.StreetNumber + _padding).Substring(0, 10);
                    var _streetName = (client.StreetName + _padding).Substring(0, 40);
                    text = _streetNo + _streetName + " "
                           + client.LastName + ", " + client.FirstName;
                }

                if (!existingMemberIds.Contains(client.Id.ToString()))
                {
                    allClientsSelectList.Add(new SelectListItem()
                        { Text = text, Value = client.Id.ToString(), Selected = false });
                }
            }

            return allClientsSelectList;
        }

        [Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult AddGroupMember(string clientId)
        {
            var gpId = Convert.ToInt32(TempData["SelectedGroupId"]);
            if (clientId != null) // Add to group unless duplicate
            {
                var iClientId = Convert.ToInt32(clientId);
                var dup = false;
                var grpMembers = db.GroupMembers
                    .Where(m => m.NameId == gpId).ToList();
                foreach (var mbr in grpMembers)
                {
                    if (mbr.ClientId == iClientId) dup = true;
                }

                if (dup == false)
                {
                    var newMember = new GroupMember()
                    {
                        ClientId = iClientId,
                        NameId = gpId
                    };
                    db.GroupMembers.Add(newMember);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("MaintainGroupMembers", new { groupId = gpId });
        }

        [HttpPost, Authorize(Roles = "Administrator,Staff,Developer")]
        public ActionResult AddGroupMember([Bind(Include = "AllClients")] GroupMemberViewModel model)
        {
            var clientId = model.ClientId;
            var gpId = Convert.ToInt32(Session["GroupId"]);
            var newMember = new GroupMember()
            {
                NameId = gpId,
                ClientId = clientId
            };
            db.GroupMembers.Add(newMember);
            db.SaveChanges();

            model = new GroupMemberViewModel()
            {
                NameId = gpId,
                GroupMemberSelectList = GetGroupMembers(gpId),
                AllClients = GetAllClientsSerlectList(model)
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateDeliveries(int nameId)
        {
            var openDeliveries = db.Deliveries.Where(s => s.Status == 0).ToList();
            var openDeliveryIds = string.Empty;
            foreach (var del in openDeliveries)
            {
                openDeliveryIds += del.ClientId + " " + del.DateDelivered;
            }

            var clientIdList = db.GroupMembers
                .Where(n => n.NameId == nameId)
                .Select(n => n.ClientId).ToList();
            foreach (var clientId in clientIdList)
            {
                var delivery = AppRoutines.NewDeliveryRecord(clientId);

                if (!openDeliveryIds.Contains(delivery.ClientId + " " + delivery.DateDelivered))
                {
                    db.Deliveries.Add(delivery);
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Deliveries");
        }
    
        public ActionResult GroupToExcel(int nameId)
        {
            var view = GetGroupReportView(nameId);
            XLWorkbook workbook = new XLWorkbook();
            IXLWorksheet ws = workbook.Worksheets.Add(view.Name  + " Member List");
            int activeRow = 1;
            ws.Cell(activeRow, 1).SetValue(view.Name + "  " + DateTime.Today.ToShortDateString());
            ws.Cell(activeRow, 1).Style.Alignment.WrapText = true;
            activeRow++;
            ws.Columns("1").Width = 12;
            ws.Cell(activeRow, 1).SetValue("Last Name");
            ws.Cell(activeRow, 2).SetValue("First Name");
            ws.Cell(activeRow, 3).SetValue("Street #");
            ws.Cell(activeRow, 4).SetValue("Street Name");
            ws.Cell(activeRow, 5).SetValue("City");
            ws.Cell(activeRow, 6).SetValue("Zip Code");
            ws.Columns("7").Width = 12;
            ws.Cell(activeRow, 7).SetValue("Phone");
            ws.Columns("8").Width = 24;
            ws.Cell(activeRow, 8).SetValue("Notes");
            foreach (var mbr in view.GroupMembersIdList)
            {
                var client = db.Clients.Find(mbr);
                if (client != null)
                {
                    activeRow++;
                    ws.Cell(activeRow, 1).SetValue(client.LastName);
                    ws.Cell(activeRow, 2).SetValue(client.FirstName);
                    ws.Cell(activeRow, 3).SetValue(client.StreetNumber);
                    ws.Cell(activeRow, 4).SetValue(client.StreetName);
                    ws.Cell(activeRow, 5).SetValue(client.City);
                    ws.Cell(activeRow, 6).SetValue(client.Zip);
                    ws.Cell(activeRow, 7).SetValue(client.Phone);
                    ws.Cell(activeRow, 7).Style.Alignment.WrapText = true;
                    ws.Cell(activeRow, 8).SetValue(client.Notes);
                    ws.Cell(activeRow, 8).Style.Alignment.WrapText = true;
                }
            }
            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            var fileName = "BH Client Group " + view.Name + DateTime.Now.ToShortDateString();
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                { FileDownloadName = fileName + ".xlsx" };
        }

        private GroupNameViewModel GetGroupReportView(int groupId)
        {
            if (groupId != 0)
            {
                var view = new GroupNameViewModel()
                {
                    Name = db.GroupNames.Where(n => n.Id == groupId)
                        .Select(n => n.Name).FirstOrDefault(),
                    GroupMembersIdList = new List<int>()
                };
                var memberList = db.GroupMembers.Where(m => m.NameId == groupId).ToList();
                foreach (var mbr in memberList)
                {
                    view.GroupMembersIdList.Add(mbr.ClientId);
                }
                return view;
            }
            return null;
        }
    }
}
