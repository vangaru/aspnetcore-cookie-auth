// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toggle(src) {
    let checkboxes = document.getElementsByName('selectedUsersId');
    for(let i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = src.checked;
    }
}