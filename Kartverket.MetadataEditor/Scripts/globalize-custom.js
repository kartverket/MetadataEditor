// customize globalize to allow validation according to norwegian methods 
// http://www.asp.net/mvc/tutorials/mvc-4/getting-started-with-aspnet-mvc4/adding-validation-to-the-model
// http://stackoverflow.com/questions/7438754/localizing-jquery-validation-with-asp-net-mvc-3
// https://gist.github.com/johnnyreilly/3651751

$(document).ready(function () {
    Globalize.culture('nb-NO');

    // parse numbers using Globalize.js
    $.validator.methods.number = function (value, element) {
        return this.optional(element) ||
            !isNaN(Globalize.parseFloat(value));
    }

    // parse dates using Globalize.js
    $.validator.methods.date = function (value, element) {
        if (!isNaN(Globalize.parseDate(value))) {
            return true;
        }
        return false;
    }

    // fix range validation
    jQuery.extend(jQuery.validator.methods, {
        range: function (value, element, param) {
            //Use the Globalization plugin to parse the value        
            var val = $.global.parseFloat(value);
            return this.optional(element) || (
                val >= param[0] && val <= param[1]);
        }
    });

});

