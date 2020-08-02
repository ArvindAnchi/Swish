"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
document.getElementById("messageInput").disabled = true;
document.getElementById("sendButton").disabled = true;
document.getElementById("ImageInput").disabled = true;

connection.on("ReceiveMessage", function (Reciever, SenderName, message, image, DateTime) {

    //console.log("SentID: " + document.getElementById("RecieverID").innerHTML +
    //"\nMyID: " + Reciever +
    //"\nMessage: " + message +
    //"\nDate: " + DateTime);

    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

    if (document.getElementById("RecieverID").innerHTML == Reciever) {

        //console.log("If");

        var div = document.createElement("div");
        var label = document.createElement("label");
        var Datep = document.createElement("p");

        label.setAttribute('class', 'p-2 m-0 reciever-bubble BreakWord');

        Datep.setAttribute('class', 'text-muted');
        Datep.setAttribute('style', 'font-size: 0.7rem;')

        div.setAttribute('class', 'mb-1');
        div.setAttribute('style', 'margin-right: 25%;');

        label.textContent = msg;
        Datep.textContent = DateTime;

        if (image != "" && image != null) {

            console.log(image);

            if (image.split('.').pop() != "mov" && image.split('.').pop() != "mp4") {
                var ChtImg = document.createElement("img");

                ChtImg.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord');
                ChtImg.setAttribute('style', 'max-width: 100%; max-height: 50vh;');

                ChtImg.src = "/Images/" + image;

                div.appendChild(ChtImg);
            }
            if (image.split('.').pop() == "mov" || image.split('.').pop() == "mp4") {
                //<video class="d-block" style="max-height: 100%; max-width: 100%; margin: auto" src="/Images/file_example_MOV_640_800kBf2aa7226-ebbe-44d6-bf3e-1c580b7dca0d.mov" alt="Alternate Text" controls=""></video>
                var ChtVid = document.createElement("video");

                ChtVid.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord');
                ChtVid.setAttribute('style', 'max-width: 100%; max-height: 50vh;');
                ChtVid.src = "/Images/" + image;
                ChtVid.setAttribute('controls', "");

                div.appendChild(ChtVid);
            }

            //var ChtImg = document.createElement("img");

            //ChtImg.setAttribute('class', 'p-2 m-0 reciever-bubble BreakWord');
            //ChtImg.setAttribute('style', 'max-width: 100%; max-height: 50vh;');

            //ChtImg.src = "/Images/" + image;

            //div.appendChild(ChtImg);

            //console.log(msg + ": " + msg != "" || msg != null);

            if (msg != "" && msg != null) {
                div.appendChild(document.createElement("br"));
            }
        }

        if (msg != "" && msg != null)
            div.appendChild(label);

        div.appendChild(document.createElement("br"));
        div.appendChild(Datep);

        document.getElementById("messagesList").appendChild(div);
    }
    else {
        //console.log("Else");

        var label = document.createElement("label");

        label.setAttribute('class', 'text-sm-left text-muted');

        label.textContent = msg;
        label.style.fontWeight = "bold";
        document.getElementById(SenderName).style.fontWeight = "bold";
        document.getElementById(Reciever).appendChild(label);

    }

    $('#MessageDiv').animate({
        scrollTop: $('#MessageDiv')[0].scrollHeight
    }, "fast");
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {

    var filename = uuidv4();
    var input = document.getElementById('ImageInput');

    if (input.files.length == 0 && document.getElementById("messageInput").value == "" && document.getElementById("messageInput").value == null)
        return;

    if (input.files.length > 0) {
        //console.log("Both upload and sendmessage")
        uploadFiles('ImageInput', filename, event);
        input.value = '';
        document.getElementById('AddImgSpn').innerHTML = ' Add Image'
        return;
    }

    if (document.getElementById("messageInput").value != "" && document.getElementById("messageInput").value != null) {
        //console.log("Only sendmessage")
        sendmessage(event);
    }

});

function UserBtnClick(Id, UserName) {

    document.getElementById("messageInput").disabled = false;
    document.getElementById("sendButton").disabled = false;
    document.getElementById("ImageInput").disabled = false;

    event.preventDefault();

    document.getElementById(Id).innerHTML = "";

    document.getElementById(UserName).style.fontWeight = "normal"

    document.getElementById('RecieverID').innerHTML = Id;

    connection.invoke("AskForMessages", Id).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById('messageInput').placeholder = 'Send message to ' + UserName;
    document.getElementById('SendGroup').disabled = false;
    //document.getElementById('messagesList').innerHTML = '';
}

function sendmessage(event, filename) {
    $('#MessageDiv').animate({
        scrollTop: $('#MessageDiv')[0].scrollHeight
    }, "fast");

    var Reciever = document.getElementById("RecieverID").innerHTML;
    var message = document.getElementById("messageInput").value;

    //console.log(filename);

    if (document.getElementById('RecieverID').innerHTML) {

        var div = document.createElement("div");
        var label = document.createElement("label");
        var Datep = document.createElement("p");

        var date = new Date();
        date.toJSON();

        label.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord active');
        label.setAttribute('style', 'color: white');

        Datep.setAttribute('class', 'text-muted');
        Datep.setAttribute('style', 'font-size: 0.7rem;')

        div.setAttribute('class', 'text-right mb-1');
        div.setAttribute('style', 'margin-left: 25%;');

        label.textContent = message;

        Datep.textContent =
            date.getHours() + ":" +
            date.getMinutes();

        if (filename != "" && filename != null) {

            console.log(filename);

            if (filename.split('.').pop() != "mov" && filename.split('.').pop() != "mp4") {
                var ChtImg = document.createElement("img");

                ChtImg.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord');
                ChtImg.setAttribute('style', 'max-width: 100%; max-height: 50vh;');

                ChtImg.src = "/Images/" + filename;

                div.appendChild(ChtImg);
            }
            if (filename.split('.').pop() == "mov" || filename.split('.').pop() == "mp4") {
                //<video class="d-block" style="max-height: 100%; max-width: 100%; margin: auto" src="/Images/file_example_MOV_640_800kBf2aa7226-ebbe-44d6-bf3e-1c580b7dca0d.mov" alt="Alternate Text" controls=""></video>
                var ChtVid = document.createElement("video");

                ChtVid.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord');
                ChtVid.setAttribute('style', 'max-width: 100%; max-height: 50vh;');
                ChtVid.src = "/Images/" + filename;
                ChtVid.setAttribute('controls', "");

                div.appendChild(ChtVid);
            }

            //var ChtImg = document.createElement("img");

            //ChtImg.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord active');
            //ChtImg.setAttribute('style', 'max-width: 100%; max-height: 50vh; color: white');

            //var input = document.getElementById('ImageInput');

            //ChtImg.src = "/Images/" + filename

            //div.appendChild(ChtImg);

            if (document.getElementById("messageInput").value != "" && document.getElementById("messageInput").value != null) {
                div.appendChild(document.createElement("br"));
            }
        }

        //console.log(document.getElementById("messageInput").value + ": " + document.getElementById("messageInput").value != "" || document.getElementById("messageInput").value != null)

        if (document.getElementById("messageInput").value != "" && document.getElementById("messageInput").value != null) {
            div.appendChild(label);
        }

        div.appendChild(document.createElement("br"));
        div.appendChild(Datep);

        document.getElementById("messagesList").appendChild(div);
    }

    connection.invoke("SendMessage", Reciever, message, filename).catch(function (err) {
        return console.error(err.toString());
    });

    document.getElementById("messageInput").value = '';

    event.preventDefault();
}

document.addEventListener("keydown", function (event) {
    if (event.keyCode === 13) {
        var filename = uuidv4();
        var input = document.getElementById('ImageInput');

        if (input.files.length == 0 && document.getElementById("messageInput").value == "" && document.getElementById("messageInput").value == null)
            return;

        if (input.files.length > 0) {
            //console.log("Both upload and sendmessage")
            uploadFiles('ImageInput', filename, event);
            input.value = '';
            document.getElementById('AddImgSpn').innerHTML = ' Add Image'
            return;
        }

        if (document.getElementById("messageInput").value != "" && document.getElementById("messageInput").value != null) {
            //console.log("Only sendmessage")
            sendmessage(event);
        }
    }
});

connection.on("GetMessagesFromUser", function (messages) {
    try {
        document.getElementById("messagesList").innerHTML = '';
        console.log(messages.length);
        for (var i = 0; i < messages.length; i++) {
            console.log(i);
            if (messages[i][1] == "Recieved") {
                console.log("Recieved");
                var msg = messages[i][0].replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                var div = document.createElement("div");
                var label = document.createElement("label");
                var Datep = document.createElement("p");

                label.setAttribute('class', 'p-2 m-0 reciever-bubble BreakWord');

                Datep.setAttribute('class', 'text-muted');
                Datep.setAttribute('style', 'font-size: 0.7rem;')

                div.setAttribute('class', 'mb-1');
                div.setAttribute('style', 'margin-right: 25%;');

                label.textContent = msg;
                Datep.textContent = messages[i][2];

                if (messages[i][3] != "" && messages[i][3] != null) {
                    //If message is Image
                    if (messages[i][3].split('.').pop() != "mov" && messages[i][3].split('.').pop() != "mp4") {
                        var ChtImg = document.createElement("img");

                        ChtImg.setAttribute('class', 'p-2 m-0 reciever-bubble BreakWord');
                        ChtImg.setAttribute('style', 'max-width: 100%; max-height: 50vh;');

                        ChtImg.src = "/Images/" + messages[i][3];

                        div.appendChild(ChtImg);
                    }
                    //If Messafe is Video
                    if (messages[i][3].split('.').pop() == "mov" || messages[i][3].split('.').pop() == "mp4") {
                        //<video class="d-block" style="max-height: 100%; max-width: 100%; margin: auto" src="/Images/file_example_MOV_640_800kBf2aa7226-ebbe-44d6-bf3e-1c580b7dca0d.mov" alt="Alternate Text" controls=""></video>
                        var ChtVid = document.createElement("video");

                        ChtVid.setAttribute('class', 'p-2 m-0 reciever-bubble BreakWord');
                        ChtVid.setAttribute('style', 'max-width: 100%; max-height: 50vh;');
                        ChtVid.src = "/Images/" + messages[i][3];
                        ChtVid.setAttribute('controls', "");

                        div.appendChild(ChtVid);
                    }
                }
                console.log("msg : " + msg)

                if (msg != "" && msg != null)
                    div.appendChild(label);

                div.appendChild(document.createElement("br"));
                div.appendChild(Datep);

                document.getElementById("messagesList").appendChild(div);
            }
            else if (messages[i][1] == "Sent") {
                console.log("Sent");
                var msg = messages[i][0].replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
                var div = document.createElement("div");
                var label = document.createElement("label");
                var Datep = document.createElement("p");

                label.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord active');
                label.setAttribute('style', 'color: white');

                Datep.setAttribute('class', 'text-muted');
                Datep.setAttribute('style', 'font-size: 0.7rem;')

                div.setAttribute('class', 'text-right mb-1');
                div.setAttribute('style', 'margin-left: 25%;');

                label.textContent = msg;
                Datep.textContent = messages[i][2];

                if (messages[i][3] != "" && messages[i][3] != null) {

                    if (messages[i][3].split('.').pop() != "mov" && messages[i][3].split('.').pop() != "mp4") {
                        var ChtImg = document.createElement("img");

                        ChtImg.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord');
                        ChtImg.setAttribute('style', 'max-width: 100%; max-height: 50vh;');

                        ChtImg.src = "/Images/" + messages[i][3];

                        div.appendChild(ChtImg);
                    }
                    if (messages[i][3].split('.').pop() == "mov" || messages[i][3].split('.').pop() == "mp4") {
                        //<video class="d-block" style="max-height: 100%; max-width: 100%; margin: auto" src="/Images/file_example_MOV_640_800kBf2aa7226-ebbe-44d6-bf3e-1c580b7dca0d.mov" alt="Alternate Text" controls=""></video>
                        var ChtVid = document.createElement("video");

                        ChtVid.setAttribute('class', 'p-2 m-0 sender-bubble BreakWord');
                        ChtVid.setAttribute('style', 'max-width: 100%; max-height: 50vh;');
                        ChtVid.src = "/Images/" + messages[i][3];
                        ChtVid.setAttribute('controls', "");

                        div.appendChild(ChtVid);
                    }

                    if (msg != "" && msg != null) {
                        div.appendChild(document.createElement("br"));
                    }
                }

                console.log("msg : " + msg)

                if (msg != "" && msg != null)
                    div.appendChild(label);

                div.appendChild(document.createElement("br"));
                div.appendChild(Datep);

                document.getElementById("messagesList").appendChild(div);
            }
        }
        $('#MessageDiv').animate({
            scrollTop: $('#MessageDiv')[0].scrollHeight
        }, "fast");
    }
    catch (ex) {
        console.log(ex)
    }
});

function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}

function uploadFiles(inputId, FileName, event) {
    var input = document.getElementById(inputId);
    var files = input.files;
    var formData = new FormData();
    var fname = FileName + "." + files[0].name.split('.').pop()

    formData.append("file", files[0]);
    formData.append("FileName", fname);

    console.log(files[0].name.split('.').pop())

    $.ajax(
        {
            url: "/Home/ChatHub",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                sendmessage(event, fname);
                //console.log("Files Uploaded!");
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
            }
        }
    );
}