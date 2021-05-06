function UpdateCallLogDate(callLogDate)
{
    $.ajax({
        url: '/OD/Index',
        data: { callLogDate: callLogDate },
        type: "POST",
        success: function (data) { 
            $("body").html(data);  // to refresh the page
           //alert('Ajax hit'); 
        },
        error: function (jqxhr, status, exception) {
            alert('Exception:', exception);
        }
    });  
}
