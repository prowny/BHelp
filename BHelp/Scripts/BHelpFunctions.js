function UpdateCallLogDate(callLogDate) {
    $.ajax({
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
    $.ajax({
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
    $.ajax({
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
    $.ajax({
        url: "/Deliveries/HelperReport",
        data: { yy: $("#Year").val(), mm: $("#Month").val() },
        type: "POST",
        success: function (data) {
            $("body").html(data); // to refresh the page
            //var dummy =xx;
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function EditCallLog()
{
    $.ajax({
        url: "/Deliveries/CallLogIndividual",
        data: { id: $("#clientList").val() },
        type: "POST",
        success: function (data) {
            $("body").html(data); // to refresh the page
            //var dummy =xx;
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
            //var dummy = "";       
        }
    });
}

function flagChanges() {
    $.ajax({
        url: "/OD/FlagChanges",
        data: { Id: $("#client_Id").val() },
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
