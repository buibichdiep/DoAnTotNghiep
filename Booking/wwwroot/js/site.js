$(function () {
    "use strict";

    function ToogleClass(element, className) {
        element.hasClass(className)
            ? element.removeClass(className)
            : element.addClass(className);
    }

    $(".info-user a img").click(function () {
        ToogleClass($(".user-menu"), "show")
    })
});