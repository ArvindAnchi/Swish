var connection = new signalR.HubConnectionBuilder().withUrl("/ProfHub").build();

connection.on("GetPostLikes", (PostId, likes, mylike) => {
    console.log("GetPostLikes\nPostID: " + PostID + "\nLikes: " + likes + "\nMyLike: " + mylike);
    document.getElementById("PLikes-" + PostId).innerHTML = likes;
    if (mylike > 0)
        document.getElementById("btnlike-" + PostId).classList.add('text-primary');
    else
        document.getElementById("btnlike-" + PostId).classList.remove('text-primary');
});

document.addEventListener("keydown", function (event) {
    if ($('.ComTxt').is(":focus") && event.keyCode === 13) {
        console.log("PostID: " + document.activeElement.id + "\nText: " + document.activeElement.value);
        connection.invoke(
            "SaveComment",
            Number(document.activeElement.id),
            document.getElementById(document.activeElement.id).value
        ).catch(function (err) {
            return console.error(err.toString());
        });

        var divrow = document.createElement("div");
        var divcol1 = document.createElement("div");
        var divcol2 = document.createElement("div");
        var profimg = document.createElement("img");
        var spanmain = document.createElement("span");
        var spanUN = document.createElement("span");
        var boldUN = document.createElement("b");
        var br = document.createElement("br");

        divrow.setAttribute('class', 'row m-2');
        divcol1.setAttribute('class', 'col-auto');
        divcol2.setAttribute('class', 'col p-0');
        profimg.setAttribute('class', 'rounded-circle');
        profimg.setAttribute('style', 'width:38px;height:38px');
        profimg.setAttribute('src', document.getElementById("PPic").innerHTML);
        spanmain.setAttribute('style', 'font-size : 0.95rem');
        spanUN.setAttribute('class', 'text-muted');
        boldUN.textContent = document.getElementById("UFName").innerHTML;
        spanUN.textContent = '(' + document.getElementById("UUName").innerHTML + ')';

        spanmain.appendChild(boldUN);
        spanmain.appendChild(spanUN);
        spanmain.appendChild(br);
        divcol1.appendChild(profimg);
        divcol2.appendChild(spanmain);
        divrow.appendChild(divcol1);
        divrow.appendChild(divcol2);

        spanmain.append(document.getElementById(document.activeElement.id).value);

        document.getElementById("Post-" + document.activeElement.id).appendChild(divrow);

        document.activeElement.value = "";
        console.log("Done!");
    }
});

function PLike(PostID) {
    console.log("PLike\nPostID: " + PostID + "\nLikes: " + document.getElementById("PLikes-" + PostID).innerHTML);
    connection.invoke("LikePost", PostID, Number(document.getElementById("PLikes-" + PostID).innerHTML)).catch(function (err) {
        return console.error(err.toString());
    });

}

var page = 5;
var more = true;
var loadnew = true;
var loadmore= true;

$(window).scroll(function () {
    if ($(window).scrollTop() + $(window).height() >= $(document).height() - 100) {
        if (more && loadmore) {
            connection.invoke("RetrievePosts", page, document.getElementById("UUName").innerHTML.substring(1)).then(function () {
                page += 5;
                loadnew = true;
                console.log("RetrievePosts: " + document.getElementById("UUName").innerHTML.substring(1));
            }).catch(function (err) {
                return console.error(err.toString());
            });
            loadmore = false;
            connection.on("GetPosts", (Posts) => {
                if (loadnew) {
                    console.log("GetPosts");
                    var PostsJson = JSON.parse(Posts);
                    for (var key of Object.keys(PostsJson)) {//loop through posts
                        //console.log(JSON.stringify(PostsJson[key].PostID));
                        var html = "";
                        //(@[^@](.[^ ])*?)\.
                        try {
                            html += "<div class='card mt-3'>";
                            html += "<div class='card-header'>";
                            html += "<div class='row'>";
                            html += "<div class='col-auto'>";
                            html += "<img class='rounded-circle' style='width:50px;height:50px' src='/Images/" + PostsJson[key].PosterUser.PPicPath + "' />";
                            html += "</div>";
                            html += "<div class='col p-0 m-auto'>";
                            html += "<span>";
                            html += "<b>" + PostsJson[key].PosterUser.FName + " " + PostsJson[key].PosterUser.LName + "</b>";
                            html += "<span class='text-muted'> (@" + PostsJson[key].PosterUser.UserName + ") • " + new Date(PostsJson[key].PostDt).toString("dd MMM yyyy");
                            html += "</span>";
                            html += "</span>";
                            if (document.getElementById("UUName").innerHTML.substring(1) == PostsJson[key].PosterUser.UserNam) {
                                html += "<form asp-action='DeletePost' asp-controller='Home' method='post'>";
                                html += "<div class='dropdown'>";
                                html += "<button class='dot3 btn' type='button' id='Post-" + PostsJson[key].PostID + "-dropdown' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>";
                                html += "</button>";
                                html += "<div class='dropdown-menu' aria-labelledby='Post-" + PostsJson[key].PostID + "-dropdown'>";
                                html += "<button class='dropdown-item' type='submit' asp-action='DeletePost' asp-controller='Home' asp-route-PostID='" + PostsJson[key].PostID + "'>Delete post</button>";
                                html += "</div>";
                                html += "</div>";
                                html += "</form>";
                            }
                            html += "</div>";
                            html += "</div>";
                            html += "</div>";
                            html += "<div class='card-body'>";
                            html += "<div class='row m-2'>";
                            html += "<div class='w-100'>";
                            html += "<div class='w-100 mb-3'>" + PostsJson[key].PostTxt + "</div > ";
                            html += "<div class='w-100'>";
                            html += "<div>";
                            for (var { } of Object.keys(PostsJson[key].PostImages));
                            {
                                if (PostsJson[key].PostImages.length > 0) {
                                    if (PostsJson[key].PostImages[0].IsVideo) {
                                        html += "<video style='object-fit: cover' class='w-100' src='/Images/" + PostsJson[key].PostImages[0].Image + "' alt='Alternate Text' data-toggle='modal' data-target='#ImgModal-" + PostsJson[key].PostID + "' onclick='$(\"#ImgCrsControls-" + PostsJson[key].PostID + "\").carousel(0); ' />";
                                    } else {
                                        html += "<img style='object-fit: cover' class='w-100' src='/Images/" + PostsJson[key].PostImages[0].Image + ".png' alt='Alternate Text' data-toggle='modal' data-target='#ImgModal-" + PostsJson[key].PostID + "' onclick='$(\"#ImgCrsControls-" + PostsJson[key].PostID + "\").carousel(0); ' />";
                                    }
                                    html += "</div>";
                                    if (PostsJson[key].PostImages.length > 1) {
                                        html += "<hr />";
                                        html += "<div class='row'>";
                                        html += "<div class='m-auto col-auto'><button type='button' style='background-color: rgba(128, 128, 128,.5); border: none; outline: none; width: 25px; height: 25px' onclick=\"$('#content-" + PostsJson[key].PostID + "').animate({ scrollLeft: '-=150px' }, 'fast');\" class='rounded-circle' id = 'left-button' ><</button ></div > ";
                                        html += "<div class='col p-0' id='content-" + PostsJson[key].PostID + "' style='white-space: nowrap; overflow-x: scroll'>";
                                        for (var i = 1; i < PostsJson[key].PostImages.length; i++) {
                                            if (PostsJson[key].PostImages[i].IsVideo) {
                                                html += "<video style='width: 100px; height: 100px; object-fit: cover' src='/Images/" + PostsJson[key].PostImages[i].Image + "' alt='Alternate Text' data-toggle='modal' data-target='#ImgModal-" + PostsJson[key].PostID + "' onclick='$(\"#ImgCrsControls-" + PostsJson[key].PostID + "\").carousel(" + i + ");' />";
                                            } else {
                                                html += "<img style='width: 100px; height: 100px; object-fit: cover' src='/Images/" + PostsJson[key].PostImages[i].Image + "-Thumb.png' alt='Alternate Text' data-toggle='modal' data-target='#ImgModal-" + PostsJson[key].PostID + "' onclick='$(\"#ImgCrsControls-" + PostsJson[key].PostID + "\").carousel(" + i + ");' />";
                                            }
                                        }
                                        html += "</div>";
                                        html += "<div class='m-auto col-auto'><button type='button' style='background-color: rgba(128, 128, 128,.5); border: none; outline: none; width: 25px; height: 25px' onclick=\"$('#content-" + PostsJson[key].PostID + "').animate({ scrollLeft: '+=150px' }, 'fast');\" class='rounded-circle' id = 'left-button' >></button ></div > ";
                                        html += "</div>";
                                    }
                                }
                            }
                            if (PostsJson[key].PostImages.length > 0) {
                                html += "<div class='modal fade' id='ImgModal-" + PostsJson[key].PostID + "' tabindex='-1' role='dialog' aria-labelledby='exampleModalCenterTitle' aria-hidden='true'>";
                                html += "<div class='modal-dialog modal-xl modal-dialog-centered' role='document'>";
                                html += "<div class='modal-content' style='height: 90vh'>";
                                html += "<div class='modal-body'>";
                                html += "<div id='ImgCrsControls-" + PostsJson[key].PostID + "' class='carousel slide h-100' data-ride='carousel'>";
                                html += "<div class='carousel-inner h-100 flex-column'>";
                                html += "<div class='carousel-item h-100 active'>";
                                html += "<div class='w-100' style='position: absolute; top: 50%; transform: translateY(-50%);'>";
                                if (PostsJson[key].PostImages[0].IsVideo) {
                                    html += "<video class='d-block' style='max-height: 100%; max-width: 100%; margin: auto' src='/Images/" + PostsJson[key].PostImages[0].Image + "' alt='Alternate Text'>";
                                } else {
                                    html += "<img class='d-block' style='max-height: 100%; max-width: 100%; margin: auto' src='/Images/" + PostsJson[key].PostImages[0].Image + ".png' alt='Alternate Text'>";
                                }
                                html += "</div>";
                                html += "</div>";
                                for (var pImageIndex = 1; pImageIndex < PostsJson[key].PostImages.length; pImageIndex++) {
                                    html += "<div class='carousel-item h-100'>";
                                    html += "<div class='w-100' style='position: absolute; top: 50%; transform: translateY(-50%);'>";
                                    if (PostsJson[key].PostImages[pImageIndex].IsVideo) {
                                        html += "<video controls class='d-block' style='max-height: 100%; max-width: 100%; margin: auto' src='/Images/" + PostsJson[key].PostImages[pImageIndex].Image + "' alt='Alternate Text'>";
                                    } else {
                                        html += "<img class='d-block' style='max-height: 100%; max-width: 100%; margin: auto' src='/Images/" + PostsJson[key].PostImages[pImageIndex].Image + ".png' alt='Alternate Text'>";
                                    }
                                    html += "</div>";
                                    html += "</div>";
                                }
                                html += "</div>";
                                if (PostsJson[key].PostImages.length > 1) {
                                    html += "<a class='carousel-control-prev' href='#ImgCrsControls-" + PostsJson[key].PostID + "' role='button' data-slide='prev'>";
                                    html += "<span class='carousel-control-prev-icon' aria-hidden='true'></span>";
                                    html += "<span class='sr-only'>Previous</span>";
                                    html += "</a>";
                                    html += "<a class='carousel-control-next' href='#ImgCrsControls-" + PostsJson[key].PostID + "' role='button' data-slide='next'>";
                                    html += "<span class='carousel-control-next-icon' aria-hidden='true'></span>";
                                    html += "<span class='sr-only'>Next</span>";
                                    html += "</a>";
                                }
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                                html += "</div>";
                            }
                            html += "</div>";
                            html += "</div>";
                            html += "</div>";
                            html += "</div>";
                            html += "<div class='card-header border-top border-bottom-0'>";
                            html += "<div class='row text-right'>";
                            if (PostsJson[key].mylike > 0) {
                                html += "<button id='btnlike-" + PostsJson[key].PostID + "' class='text-primary form-control w-auto ml-2' onclick='PLike(" + PostsJson[key].PostID + ")'>";
                                html += "<i class='fas fa-thumbs-up'></i> <span class='badge badge-primary' id='PLikes-" + PostsJson[key].PostID + "'>" + PostsJson[key].PLikes + "</span>";
                                html += "</button>";
                            } else {
                                html += "<button id='btnlike-" + PostsJson[key].PostID + "' class='form-control w-auto ml-2' onclick='PLike(" + PostsJson[key].PostID + ")'>";
                                html += "<i class='fas fa-thumbs-up'></i> <span class='badge badge-primary' id='PLikes-" + PostsJson[key].PostID + "'>" + PostsJson[key].PLikes + "</span>";
                                html += "</button>";
                            }
                            html += "</div>";
                            html += "</div>";
                            html += "<div class='list-group-item pl-3 pt-1 pb-1' style='border-left : none; border-right : none; border-bottom : none'>";
                            html += "<div id='Post-" + PostsJson[key].PostID + "'>";
                            for (var ckey of Object.keys(PostsJson[key].Comments)) {
                                html += "<form method='post' action='/Home/Deletecomment'>";
                                html += "<div class='row m-2'>";
                                html += "<div class='col-auto'>";
                                if (!PostsJson[key].Comments[ckey].Deleted) {
                                    html += "<img class='rounded-circle' style='width:38px;height:38px' src='/Images/" + PostsJson[key].Comments[ckey].PPicPath + "'>";
                                    html += "</div>";
                                    html += "<div class='col p-0'>";
                                    html += "<span style='font-size : 0.95rem'>";
                                    html += "<b>" + PostsJson[key].Comments[ckey].Fname + " " + PostsJson[key].Comments[ckey].Lname + "</b>";
                                    html += "<span class='text-muted'>(@" + PostsJson[key].Comments[ckey].UName + ")</span><br>" + PostsJson[key].Comments[ckey].CommentTxt;
                                    if (document.getElementById("UUName").innerHTML.substring(1) == PostsJson[key].Comments[ckey].UName) {
                                        html += "</br><button type='submit' class='btn btn-link btn-sm p-0' formaction='/Home/Deletecomment?ComID=" + PostsJson[key].Comments[ckey].ComID + "'>";
                                        html += "<u class='text-danger'>Delete</u>";
                                        html += "</button>";
                                    }
                                }
                                else {
                                    html += "<img class='rounded-circle' style='width:38px;height:38px' src='/Images/ProfilePlaceholder.png'>";
                                    html += "</div>";
                                    html += "<div class='col p-0'>";
                                    html += "<span style='font-size : 0.95rem'>";
                                    PostsJson[key].Comments[ckey].Fname = "";
                                    PostsJson[key].Comments[ckey].Lname = "";
                                    PostsJson[key].Comments[ckey].UName = "";
                                    PostsJson[key].Comments[ckey].CommentTxt = "";
                                    html += "<b class='text-muted'>[Deleted]</b>";
                                    html += "<br><i class='text-muted'>Comment was deleted</i>";
                                }
                                html += "</span>";
                                html += "</div>";
                                html += "</div>";
                                html += "</form>";
                            }
                            html += "</div>";
                            html += "<div class='row m-2'>";
                            html += "<div class='col-auto'>";
                            html += "<img class='rounded-circle' style='width:38px;height:38px' src='" + document.getElementById("PPic").src + "' />";
                            html += "</div>";
                            html += "<div class='col p-0'>";
                            html += "<input class='form-control w-100 rounded-pill ComTxt' type='text' placeholder='Enter comment here' id='" + PostsJson[key].PostID + "' />";
                            html += "</div>";
                            html += "</div>";
                            html += "</div>";
                            html += "<div class='card-header text-center'>";
                            html += "<a href='/Home/Post?postID=" + PostsJson[key].PostID + "'>View all comments</a>";
                            html += "</div>";
                            html += "</div>";
                            document.getElementById("PostDiv").innerHTML += html;
                        }
                        catch (err) {
                            console.log(err);
                        }
                        loadmore = true;
                    }

                    if (PostsJson.length < 5) {
                        more = false;
                        document.getElementById("LoadPost").style.display = "none";
                        document.getElementById("PostDiv").innerHTML += '<div class="card mt-3"><div class="card-header"><h3>End of posts</h3></div></div>';
                    }
                    else {
                        document.getElementById("LoadPost").style.display = "flex";
                    }
                    loadnew = false;
                }
            });
        }
    }
});

function AddFriend(User, UserId) {
    connection.invoke("AddFriend", User, 'false', 'false', false).then(function () {
        document.getElementById("FRLi").innerHTML = "<a class='btn btn-primary' onclick='RemoveFriend(\"" + document.getElementById("VBUser").innerHTML + "\", \"" + document.getElementById("VBUserId").innerHTML + "\")'>Friend request sent</a>";
    }).catch(function (err) {
        return console.error(err.toString());
    });

    connectionNotif.invoke("SendNotifs", UserId, document.getElementById("MUname").innerHTML + " Wants to add you as a friend.", 1).then(function () {
        console.log("SendNotifs\n" + UserId);
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

function RemoveFriend(User, UserId) {
    connection.invoke("AddFriend", User, 'false', 'false', true).then(function () {
        document.getElementById("FRLi").innerHTML = "<a class='btn btn-primary' onclick='AddFriend(\"" + document.getElementById("VBUser").innerHTML + "\", \"" + document.getElementById("VBUserId").innerHTML + "\")'>Add Friend</a>" + "<a class='btn btn-danger' href='/Home/BlockUser?User=" + document.getElementById("VBUser").innerHTML + "'>Block User</a>";
    }).catch(function (err) {
        return console.error(err.toString());
    });

    connectionNotif.invoke("SendNotifs", UserId, "", 0).then(function () {
        console.log("SendNotifs\n" + UserId);
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

connection.start().catch(function (err) {
    return console.error(err.toString());
});