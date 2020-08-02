// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function GoToSearch() {
    window.location.href = '/Home/Search?Query=' + document.getElementById("SearchTxt").value;
}

document.addEventListener("keydown", function (event) {
    if (event.keyCode === 13) {
        if ($('#SearchTxt').is(":focus")) {
            window.location.href = '/Home/Search?Query=' + document.getElementById("SearchTxt").value;
        }
    }
});