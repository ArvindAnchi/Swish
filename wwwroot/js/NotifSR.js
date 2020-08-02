"use strict";

var connectionNotif = new signalR.HubConnectionBuilder().withUrl("/NotifHub").build();

connectionNotif.on("RecieveNotifs", function (Notifications, myUName) {

    console.log("RecieveNotifs");

    var NotificationsJson = JSON.parse(Notifications);
    var html = "";

    if (NotificationsJson.length > 0) {
        for (var key of Object.keys(NotificationsJson)) {
            if (NotificationsJson[key].UserName != myUName) {
                html += "<div class='NotifCard card mb-2'>";
                html += "<div class='card-body'>";
                html += "<div class='row mb-2'>";
                html += "<div class='col-auto'>";
                html += "<img class='rounded-circle' style='width: 35px; height: 35px;' src='/Images/" + NotificationsJson[key].PPicPath + "' />";
                html += "</div>";
                html += "<div class='col p-0'>";
                html += "<label class='col-form-label'><b>" + NotificationsJson[key].FName + " " + NotificationsJson[key].LName + " (" + NotificationsJson[key].UserName + ")</b></label>";
                html += "</div>";
                html += "</div>";
                html += "Wants to add you as a friend.";
                html += "<form method='post'>";
                html += "<div class='row mt-3'>";
                html += "<div class='col'>";
                html += "<button type='submit' formaction='/Home/AddFriend?user=" + NotificationsJson[key].UserName + "&forconfirm=true&confirmed=true' class='btn w-100 btn-primary'>";
                html += "Confirm friend";
                html += "</button>";
                html += "</div>";
                html += "<div class='col'>";
                html += "<button type='submit' formaction='/Home/AddFriend?user=" + NotificationsJson[key].UserName + "&forconfirm=true&confirmed=false' class='btn w-100 btn-danger'>";
                html += "Remove notification";
                html += "</button>";
                html += "</div>";
                html += "</div>";
                html += "</form>";
                html += "</div>";
                html += "</div>";
            }
        }
        document.getElementById("NotifLoading").hidden = true;
        document.getElementById("Notif-messages").innerHTML = html;
    }
    else {
        document.getElementById("NotifLoading").hidden = true;
        document.getElementById("Notif-messages").innerHTML = "<span>No new notifications :)</span>";
    }

    console.log($("div.NotifCard").length);

    if ($("div.NotifCard").length > 0)
        $("#NotifCount").text($("div.NotifCard").length);
    else
        $("#NotifCount").text("");
});

connectionNotif.on("PopupNotifSFR", function (Message, User, id) {

    console.log("PopupNotifSFR");

    var UserJson = JSON.parse(User);
    var html = "";

    console.log(UserJson.FName);

    html += "<div role='alert' id='Toast-" + id + "' class='toast fade show ml-auto'>";
    html += "<div class='toast-header'>";
    html += "<img src='/Images/" + UserJson.PPicPath + "' class='rounded mr-2 Size15' alt='ProfilePic'><b>" + UserJson.FName + " " + UserJson.LName + " (" + UserJson.UserName + ")</b>";
    html += "<button type='button' class='ml-auto mb-1 close' data-dismiss='toast'>&times;</button>";
    html += "</div>";
    html += "<div class='toast-body'>";
    html += Message;
    html += "<form method='post'>";
    html += "<div class='row mt-3'>";
    html += "<div class='col pr-0'>";
    html += "<button type='submit' formaction='/Home/AddFriend?user=" + UserJson.UserName + "&forconfirm=true&confirmed=true' class='btn w-100 h-100 btn-primary'>";
    html += "Confirm friend";
    html += "</button>";
    html += "</div>";
    html += "<div class='col'>";
    html += "<button type='submit' formaction='/Home/AddFriend?user=" + UserJson.UserName + "&forconfirm=true&confirmed=false' class='btn w-100 h-100 btn-danger'>";
    html += "Remove notification";
    html += "</button>";
    html += "</div>";
    html += "</div>";
    html += "</form>";
    html += "</div>";
    html += "</div>";

    document.getElementById("ToastDiv").innerHTML += html;

    $("#Toast-" + id).toast({ autohide: false });
    $("#Toast-" + id).toast('show');

    setTimeout(function () {
        $("#Toast-" + id).toast('hide');
    }, 5000)

    connectionNotif.invoke("GetNotifs").catch(function (err) {
        return console.error(err.toString());
    });

});

connectionNotif.on("PopupNotifRFR", function () {

    console.log("PopupNotifRFR");

    connectionNotif.invoke("GetNotifs").catch(function (err) {
        return console.error(err.toString());
    });

});

connectionNotif.start().then(function () {
    connectionNotif.invoke("GetNotifs").catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("NotifDropDownBtn").onclick = function () {
    if (!document.getElementById("NotifDropDownMenu").classList.contains("show")) {
        connectionNotif.invoke("GetNotifs").catch(function (err) {
            return console.error(err.toString());
        });
    }
};
