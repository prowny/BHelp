var _latestUser_Id;
var _latestUser_Email;

function GetUserResetInfo(event) {
    var user_Id = $(this).val();
    $.ajax({
        url: "/ResetAnyPassword/GetUserInfo",
        data: { id: user_Id },
        type: "GET",
        dataType: "JSON",
        success: function (user) {
            _latestUser_Id = user_Id;
            $("#userName").text("User:" + " " + user.FirstName + " " + user.LastName);
            $("#userEmail").text(user.Email);
            _latestUser_Email = user.Email;
            $("#newPassword").text("");
            $("#EnterNewPasswordDiv").show();

            if (user.id !== null)
                $("#userPhone").text("Phone:" + " " + user.PhoneNumber);

        },
        error: function (data) {
            $("#Main").hide();
        }
    });
}  // function GetUserResetInfo(event)

function SaveNewPassword(event) {
    var x = _latestUser_Id;
    _newPassword = $("#newPassword").val();
    if (_newPassword === "") {
        return;
    }
    $.ajax({
        url: "/ResetAnyPassword/Reset",
        data: { userId: _latestUser_Id, newPassword: _newPassword },
        type: "GET",
        dataType: "JSON",
        success: function (user) {
            _latestUser_Id = user.id;
            $("#EnterNewPasswordDiv").show();
        },
        error: function (data) {
            $("#EnterNewPasswordDiv").show();
        }
    });
}  //  function SaveNewPassword(event)

function EmailToUser(event) {
    var x = _latestUser_Email;

}