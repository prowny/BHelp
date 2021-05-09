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

function SearchClients(text) {
        $.ajax({
            url: "/OD/SearchHouseholds",
            data: { searchString: $("#SearchText").val() },
            type: "POST",
            dataType: "JSON",
            success: function (data) {
                var dummy = "";
                $("#SearchResults").show();
                $("SearchResults").html(data);
                //location.reload(true);
                //$("body").html(data);
            },
            error: function (jqxhr, status, exception) {
                alert('Exception:', exception);
                var dummy = "";       
            }
        });
    }

