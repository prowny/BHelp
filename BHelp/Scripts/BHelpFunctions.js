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

function SearchClients(text)
{
    $.ajax({
        url: "/OD/SearchHouseholds",
        data: { searchString: $("#SearchText").val() },
        type: "POST",
        success: function (data) {
            var dummy = "";
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
            var dummy = "";       
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

function GetFamilyDetails(text)
{
    $.ajax({
        url: "/FamilyMembers/GetFamilyDetails",
        data: { Id: $("#client_Id").val() },
        type: "GET",
        success: function (data) {
            var dummy = "";
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
            var dummy = "";
        }
    });

}

