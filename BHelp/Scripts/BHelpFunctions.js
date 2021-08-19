function UpdateCallLogDate(callLogDate) {
    $.ajax({
        url: '/OD/Index',
        data: { callLogDate: callLogDate },
        type: "POST",
        success: function(data) {
            $("body").html(data); // to refresh the page
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
            $("body").html(data); // to refresh the page
            //alert('Ajax hit'); 
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function SearchClients()
{
    $.ajax({
        url: "/OD/SearchHouseholds",
        data: { searchString: $("#SearchText").val() },
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
            $('body').html(result);
        },
        Error: function () {
            //var dummy = "";
        }
    });
}

function GetFamilyDetails() {
    $.ajax({
        url: "/FamilyMembers/Edit",
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

function CountyReport() {
    $.ajax({
        url: "/Deliveries/CountyReport",
        data: { yy: $("#Year").val(), qtr: $("#Quarter").val() },
        type: "POST",
        success: function (data) {
            $("body").html(data); // to refresh the page
            //var dummy =xx;
        },
        error: function(jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });
}

function EditCallLog()
{
    $("#callLogsDiv").hide();
    $.ajax({
        url: "/Deliveries/CallLogIndividual",
        data: { clientId: $("#clientList").val() },
        type: "POST",
        success: function (data) {
            alert(data.delivery.DeliveryList);
            $("#callLogsDiv").show();
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
            //var dummy = "";       
        }
    });
}