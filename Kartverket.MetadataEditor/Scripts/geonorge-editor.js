
$.validator.setDefaults({
    ignore: "", /* allow validation of inivisble elements, i.e. those that are in an inactive tab*/
    highlight: function (element) {
        console.log("Running highlight!!");
        $(element).closest(".form-group").addClass("has-error");
        
        var id = $(element).closest(".tab-pane").attr('id');
        var $tabLink = $('ul.nav-tabs a[href=#' + id + ']');

        $tabLink.parent().addClass("has-error");

        var warning = $tabLink.parent().find('span.glyphicon-warning-sign');
        console.log("warning=");
        console.log(warning);
        if (warning.length == 0) {

            $tabLink.before('<span class="glyphicon glyphicon-warning-sign"></span>');
        }
        
    },
    unhighlight: function (element) {
       // $(element).closest(".form-group").removeClass("has-error");
        /*
        var id = $(element).closest(".tab-pane").attr('id');
        var $tabLink = $('ul.nav-tabs a[href=#' + id + ']');
        $tabLink.parent().removeClass("has-error");
        $tabLink.parent().find('span.glyphicon-warning-sign').remove();
        */

        console.log("unhighlight");
    }
});

$(function () {

    /* activate jquery ui datepicker */
    $.datepicker.regional['nb'] = {
        closeText: 'Lukk',
        prevText: '&#xAB;Forrige',
        nextText: 'Neste&#xBB;',
        currentText: 'I dag',
        monthNames: ['januar', 'februar', 'mars', 'april', 'mai', 'juni', 'juli', 'august', 'september', 'oktober', 'november', 'desember'],
        monthNamesShort: ['jan', 'feb', 'mar', 'apr', 'mai', 'jun', 'jul', 'aug', 'sep', 'okt', 'nov', 'des'],
        dayNamesShort: ['søn', 'man', 'tir', 'ons', 'tor', 'fre', 'lør'],
        dayNames: ['søndag', 'mandag', 'tirsdag', 'onsdag', 'torsdag', 'fredag', 'lørdag'],
        dayNamesMin: ['sø', 'ma', 'ti', 'on', 'to', 'fr', 'lø'],
        weekHeader: 'Uke',
        dateFormat: 'dd.mm.yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };
    $.datepicker.setDefaults($.datepicker.regional['nb']);

    $("input.date").datepicker({
        changeMonth: true,
        changeYear: true,
        dateFormat: 'dd.mm.yy'
    });

    jQuery.extend(jQuery.validator.messages, {
        required: "Dette feltet er påkrevd.",
        remote: "Please fix this field.",
        email: "Vennligst skriv inn en gyldig epostadresse.",
        url: "Vennligst skriv inn en gyldig nettadresse",
        date: "Vennligst skriv inn en gyldig dato.",
        dateISO: "Vennligst skriv inn en gyldig dato (ISO).",
        number: "Vennligst skriv inn et tall.",
        digits: "Vennligst skriv kun tall.",
        creditcard: "Vennligst skriv inn et gyldig kredittkortnummer.",
        equalTo: "Vennligst skriv inn samme verdi på nytt.",
        accept: "Please enter a value with a valid extension.",
        maxlength: jQuery.validator.format("Vennligst skriv inn mindre enn {0} tegn."),
        minlength: jQuery.validator.format("Vennligst skriv inn minst {0} tegn."),
        rangelength: jQuery.validator.format("Vennligst skriv inn en verdi mellom {0} og {1} tegn."),
        range: jQuery.validator.format("Vennligst skriv inn en verdi mellom {0} og {1}."),
        max: jQuery.validator.format("Vennligst skriv inn en verdi på mindre eller lik {0}."),
        min: jQuery.validator.format("Vennligst skriv inn en verdi på større eller lik {0}.")
    });

    
});
