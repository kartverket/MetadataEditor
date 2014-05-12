$(function () {
    /* disable enter key to avoid accidental form submit */
    $(document).keypress(function (event) {
        if (event.which == '13') { // 13 = enter key
            event.preventDefault();
        }
    });
});