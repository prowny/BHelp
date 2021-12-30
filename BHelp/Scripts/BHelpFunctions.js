var _groupId;  // for parameter passing

function UpdateCallLogDate(callLogDate) {
    window.$.ajax({
        url: '/OD/Index',
        data: { callLogDate: callLogDate },
        type: "POST",
        success: function(data) {
            window.$("body").html(data); // to refresh the page
            //alert('Ajax hit'); 
        },
        error: function(jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function UpdateDriverLogDate(callLogDate) {
    window.$.ajax({
        url: '/Driver/Index',
        data: { logDate: callLogDate },
        type: "POST",
        success: function (data) {
            window.$("body").html(data); // to refresh the page
            //alert('Ajax hit'); 
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function SearchClients()
{
    window.$.ajax({
        url: "/OD/SearchHouseholds",
        data: { searchString: window.$("#SearchText").val() },
        type: "POST",
        success: function () {
            //var dummy = "";
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
            //var dummy = "";       
        }
    });
}

function SearchHouseholds(text) {
    window.$.ajax({
        url: "/OD/Index",
        data:
        { searchString: text },
        type: "POST",
        success: function (result) {
            // After success, update # of records found label           
            //var dummy = "";
            window.$('body').html(result);
        },
        Error: function () {
            //var dummy = "";
        }
    });
}

function GetFamilyDetails() {
    window.$.ajax({
        url: "/FamilyMembers/Edit",
        data: { Id: window.$("#client_Id").val() },
        type: "POST",
        success: function() {
            //var dummy = "";
        },
        error: function(jqxhr, status, exception) {
            alert('Exception:', exception);
            //var dummy = "";
        }
    });
}

function CountyReport() {
    window.$.ajax({
        url: "/Deliveries/CountyReport",
        data: { yy: window.$("#Year").val(), qtr: window.$("#Quarter").val() },
        type: "POST",
        success: function (data) {
            window.$("body").html(data); // to refresh the page
            //var dummy =xx;
        },
        error: function(jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function HelperReport() {
    window.$.ajax({
        url: "/Deliveries/HelperReport",
        data: { yy: window.$("#Year").val(), mm: window.$("#Month").val() },
        type: "POST",
        success: function (data) {
            window.$("body").html(data); // to refresh the page
            //var dummy =xx;
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function EditCallLog()
{
    window.$.ajax({
        url: "/Deliveries/CallLogIndividual",
        data: { id: window.$("#clientList").val() },
        type: "POST",
        success: function (data) {
            window.$("body").html(data); // to refresh the page
            //var dummy =xx;
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
            //var dummy = "";       
        }
    });
}

function flagChanges() {
    window.$.ajax({
        url: "/OD/FlagChanges",
        data: { Id: window.$("#client_Id").val() },
        type: "POST",
        success: function() {
            //var dummy = "";
        },
        error: function(jqxhr, status, exception) {
            alert('Exception:', exception);
            //var dummy = "";
        }
    });
}

function UpdateDesiredDeliveryDate(ddDate) {
    window.$.ajax({
        url: '/Deliveries/Edit',
        data:{ id: 0, desiredDeliveryDate: ddDate },
        type: "GET",
        success: function (data) {
            window.$("body").html(data);  // to refresh the page
            //alert("Ajax hit"); 
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function GetGroupMembers()
{
    _groupId = window.$(this).val();
    window.$.ajax({
        url: "/GroupMembers/GetGroupMembers",
        data: { groupId: _groupId },
        type: "GET",
        dataType: "JSON",
        success: function (data) {
            window.$("#MembersDiv").show();
            //var dummy = "";
        }
    });   // $.ajax({
}

function AddGroupMember() {
    var _clientId = window.$(this).val();
    window.$.ajax({
        url: "/GroupMembers/AddGroupMember",
        data: { groupId: _groupId, clientId:_clientId },
        type: "GET",
        dataType: "JSON",
        success: function() {
            window.$("#MembersDiv").show();
            //var dummy = "";
        }
    }); // $.ajax({
}
