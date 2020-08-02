"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/IndHub").build();

document.addEventListener("keydown", function (event) {
    if (event.keyCode === 13) {
        if ($('.ComTxt').is(":focus")) {
            console.log("COMTXT")
            connection.invoke(
                "SaveComment",
                Number(document.activeElement.id),
                document.getElementById(document.activeElement.id).value
            ).catch(function (err) {
                return console.error(err.toString());
            });

            connection.on("LikedCom", (ComID) => {
                console.log("LikedCOM")
                var formcom = document.createElement("form");
                var divrow = document.createElement("div");
                var divcol1 = document.createElement("div");
                var divcol2 = document.createElement("div");
                var profimg = document.createElement("img");
                var spanmain = document.createElement("span");
                var spanUN = document.createElement("span");
                var DelBtnUN = document.createElement("button");
                var UnderlineUN = document.createElement("u");
                var boldUN = document.createElement("b");
                var br = document.createElement("br");
                var br2 = document.createElement("br");

                formcom.setAttribute('method', 'post')
                formcom.setAttribute('action', '/Home/Deletecomment')
                divrow.setAttribute('class', 'row m-2');
                divcol1.setAttribute('class', 'col-auto');
                divcol2.setAttribute('class', 'col p-0');
                profimg.setAttribute('class', 'rounded-circle');
                profimg.setAttribute('style', 'width:38px;height:38px');
                profimg.setAttribute('src', document.getElementById("PPic").innerHTML);
                spanmain.setAttribute('style', 'font-size : 0.95rem');
                spanUN.setAttribute('class', 'text-muted');
                DelBtnUN.setAttribute('type', 'submit');
                DelBtnUN.setAttribute('class', 'btn btn-link btn-sm p-0');
                DelBtnUN.setAttribute('formaction', '/Home/Deletecomment?ComID=' + ComID);
                UnderlineUN.setAttribute('class', 'text-danger');
                boldUN.textContent = document.getElementById("UFName").innerHTML;
                spanUN.textContent = '(' + document.getElementById("UUName").innerHTML + ')';
                UnderlineUN.textContent = 'Delete';

                spanmain.appendChild(boldUN);
                spanmain.appendChild(spanUN);
                spanmain.appendChild(br);
                divcol1.appendChild(profimg);
                divcol2.appendChild(spanmain);
                divrow.appendChild(divcol1);
                divrow.appendChild(divcol2);

                spanmain.append(document.getElementById(document.activeElement.id).value);
                spanmain.appendChild(br2);

                DelBtnUN.appendChild(UnderlineUN);
                spanmain.appendChild(DelBtnUN);

                formcom.appendChild(divrow)

                document.getElementById("Post-" + document.activeElement.id).prepend(formcom);

                document.activeElement.value = "";
            });
        }
    }
});

function PLike(PostID) {
    //console.log("PLike\nPostID: " + PostID + "\nLikes: " + document.getElementById("PLikes-" + PostID).innerHTML);
    connection.invoke("LikePost", PostID, Number(document.getElementById("PLikes-" + PostID).innerHTML)).catch(function (err) {
        return console.error(err.toString());
    });

}

connection.on("GetPostLikes", (PostId, likes, mylike) => {
    document.getElementById("PLikes-" + PostId).innerHTML = likes;
    //console.log("PLike\nPostID: " + PostId + "\nLikes: " + likes + "\nMyLike: " + mylike);
    if ("@" + mylike.substring(1) == document.getElementById("UUName").innerHTML) {
        if (mylike.charAt(0) == "1")
            document.getElementById("btnlike-" + PostId).classList.add('text-primary');
        if (mylike.charAt(0) == "0")
            document.getElementById("btnlike-" + PostId).classList.remove('text-primary');
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});