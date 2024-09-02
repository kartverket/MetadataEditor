var applicationEnvironment = (applicationEnvironment === undefined) ? "" : applicationEnvironment;
var applicationVersionNumber = (applicationVersionNumber === undefined) ? "" : applicationVersionNumber;
var supportsLogin = false;
var supportsMultiCulture = false;

var authenticationData = (authenticationData === undefined) ? {} : authenticationData;
if (authenticationData !== {}) {
    supportsLogin = (authenticationData.supportsLogin === undefined) ? false : authenticationData.supportsLogin;
    authenticationData.isAuthenticated = authenticationData.isAuthenticated === undefined ? false : authenticationData.isAuthenticated;
    authenticationData.urlActionSignIn = authenticationData.urlActionSignIn === undefined ? "" : authenticationData.urlActionSignIn;
    authenticationData.urlActionSignOut = authenticationData.urlActionSignOut === undefined ? "" : authenticationData.urlActionSignOut;
    authenticationData.userName = authenticationData.userName === undefined ? "" : authenticationData.userName;
}

var cultureData = (cultureData === undefined) ? {} : cultureData;
if (cultureData !== {}) {
    supportsMultiCulture = cultureData.supportsMultiCulture === undefined ? false : cultureData.supportsMultiCulture;
    cultureData.urlSetCulture = cultureData.urlSetCulture === undefined ? "" : cultureData.urlSetCulture;
    cultureData.urlSetCultureNorwegian = cultureData.urlSetCultureNorwegian === undefined ? "" : cultureData.urlSetCultureNorwegian;
    cultureData.currentCulture = cultureData.currentCulture === undefined ? "" : cultureData.currentCulture;
}

function environmentIsProduction() {
    var productionEnvironment = "";
    return applicationEnvironment === productionEnvironment;
}

var geonorgeUrl = environmentIsProduction() ? "https://www.geonorge.no/" : "https://www.test.geonorge.no/";
if (cultureData !== {}) {
    if (cultureData.currentCulture == 'en'){
        geonorgeUrl += 'en';
    }
}

// Check if string contains parameters
function containsParameters(string) {
    return string.length && string.indexOf("?") > -1 ? true : false;
}

// Check active URL contains parameters
function urlContainsParameters() {
    return containsParameters(window.location.search);
}


/* Loading animation */
function showLoadingAnimation(loadingMessage) {
    $("#loading-animation").html(loadingMessage);
    $("#loading-animation").show();
}

function hideLoadingAnimation() {
    $("#loading-animation").html('');
    $("#loading-animation").hide();
}

function notOpeningInNewTab(event) {
    if (event.ctrlKey || event.shiftKey || event.metaKey || (event.button && event.button == 1)) {
        return false;
    } else {
        return true;
    }
}

function addDefaultLoadingAnimation(element) {
    element.addClass('show-loading-animation');
    element.data('loading-message', 'Henter innhold');
}

showLoadingAnimation('Laster innhold');
/* ----------------------------- */


$(document).ready(function() {

    // Loading animation
    hideLoadingAnimation();

    $(document).on("click", ".show-loading-animation", function(event) {
        if (notOpeningInNewTab(event)) {
            var loadingMessage = $(this).data('loading-message') !== undefined ? $(this).data('loading-message') : '';
            showLoadingAnimation(loadingMessage);
        }
    });


});

document.addEventListener("DOMContentLoaded", function (event) {
    var options = {
        disable_search_threshold: 10,
        search_contains: true
    };
    $(".chosen-select").chosen(options);
    $("[data-toggle='tooltip']").tooltip({
        trigger: 'hover',
        position: { my: "center top", at: "center bottom+10" }
    });
    $("li.has-error[data-toggle='tooltip']").tooltip("option", "position", { my: "center top", at: "center bottom+30" });
    $("li[data-toggle='tooltip']").mouseleave(function() {
        $(".ui-helper-hidden-accessible").remove();
    });

    $(".ui-tooltip-element[data-toggle='tooltip']").tooltip("option", "position", { my: "center top", at: "center bottom+25" });
    $(".ui-tooltip-element[data-toggle='tooltip']").mouseleave(function() {
        $(".ui-helper-hidden-accessible").remove();
    });

    // Get useragent
    var doc = document.documentElement;
    doc.setAttribute('data-useragent', navigator.userAgent);
});


var baseurl = 'http://' + window.location.host;

$(document).ready(function () {
    $(".page-wizard .page-wizard-tab").on("click", function () {
        $(".page-wizard").toggleClass("open");
    });

    var liUl = $("<li class='jose'><ul></ul></li>");

    $("<p>te</p>").appendTo($(liUl).find("ul"));
});
function addShoppingCartTooltip(elementsCount) {
    var element = $('#shopping-cart-url');
    var elementsCountText = elementsCount !== 0 ? elementsCount : 'ingen';
    var text = elementsCount == 1 ? 'Du har ' + elementsCountText + ' nedlasting i kurven din' : 'Du har ' + elementsCountText + ' nedlastinger i kurven din';
    element.attr('title', text);
    element.attr('data-original-title', text);
    element.data('toggle', 'tooltip');
    element.data('placement', 'bottom');
    element.tooltip();
}

function removeSingleItemFromArray(array, undesirableItem) {
    var index = array.indexOf(undesirableItem);
    if (index >= 0) {
        array.splice(index, 1);
    }
    return array;
}

function removeFromArray(array, undesirableItems) {
    var multiple = Array.isArray(undesirableItems);
    if (multiple) {
        undesirableItems.forEach(function (undesirableItem) {
            array = removeSingleItemFromArray(array, undesirableItem);
        });
    } else {
        array = removeSingleItemFromArray(array, undesirableItems);
    }
}

function orderItemHasMetadata(orderItemUuid){
    var orderItemHasMetadata = localStorage.getItem(orderItemUuid + ".metadata") !== null ? true : false;
    return orderItemHasMetadata;
}

function removeBrokenOrderItems() {
    var orderItems = JSON.parse(localStorage.getItem("orderItems"));
    if (orderItems !== null){
        removeFromArray(orderItems, [null, undefined, "null", {}, ""]);
        orderItems.forEach(function (orderItem) {
            if (!orderItemHasMetadata(orderItem)) {
                removeSingleItemFromArray(orderItems, orderItem);
            }
        });
        localStorage.setItem('orderItems', JSON.stringify(orderItems));
    }
}

function updateShoppingCart() {
    var shoppingCartElement = $('.downloads__count');
    var orderItems = "";
    var orderItemsObj = {};
    var cookieName = "orderitems";
    var cookieValue = 0;
    var cookieDomain = ".geonorge.no";

    if (localStorage.getItem("orderItems") !== null) {
        orderItems = localStorage.getItem("orderItems");
    }
    
    if (orderItems !== "") {
        orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        if (cookieValue > 0) {
            shoppingCartElement.css("display", "block");
            shoppingCartElement.html(cookieValue);
            addShoppingCartTooltip(cookieValue);
        } else {
            shoppingCartElement.css("display", "none");
            addShoppingCartTooltip(0);
        }
        
    } else if (Cookies.get(cookieName) !== undefined && Cookies.get(cookieName) !== 0 && Cookies.get(cookieName) !== "0") {
        cookieValue = Cookies.get(cookieName);
        shoppingCartElement.css("display", "block");
        shoppingCartElement.html(cookieValue);
        addShoppingCartTooltip(cookieValue);
    } else {
        shoppingCartElement.css("display", "none");
        addShoppingCartTooltip(0);
    }
    Cookies.set(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}

function updateShoppingCartCookie() {
    var shoppingCartElement = $('.downloads__count');
    var cookieName = "orderitems";
    var cookieDomain = ".geonorge.no";
    var cookieValue = 0;
    if (localStorage.getItem("orderItems") !== null && localStorage.getItem("orderItems") != "[]") {
        var orderItems = localStorage.getItem("orderItems");
        var orderItemsObj = JSON.parse(orderItems);
        cookieValue = orderItemsObj.length;
        shoppingCartElement.html(cookieValue);
        addShoppingCartTooltip(cookieValue);
    } else {
        cookieValue = 0;
        shoppingCartElement.css("display", "none");
        addShoppingCartTooltip(cookieValue);
    }
    Cookies.set(cookieName, cookieValue, { expires: 7, path: '/', domain: cookieDomain });
}


document.addEventListener("DOMContentLoaded", function (event) {
    removeBrokenOrderItems();
    updateShoppingCart();
});

/* Content toggle */
function updateToggleLinks(element) {
  $(element).each(function() {
    $(this).toggleClass('show-content');
  });
}
$("document").ready(function() {

  updateToggleLinks($('.toggle-content'));

  $(".toggle-content").click(function() {
    var toggleClass = $(this).data('content-toggle');
    updateToggleLinks($(this));
    $("." + toggleClass).toggle();
  });

});


/* Tabs */
function activateTab(tab) {
  $(".link-tabs").ready(function() {
    tabLink = $(".link-tabs li a[data-tab='" + tab + "']");
    $(".link-tabs li.active").removeClass('active');
    tabLink.parent('li').addClass('active');
  });
}

$(".link-tabs").ready(function() {
  $(".link-tabs li a").click(function(event) {
    event.preventDefault();
    activateTab($(this).data('tab'));
    $("#tab-content").css('opacity', '.15');
    window.location.href = $(this).prop('href');
  });
});


/* Help texts */
$("document").ready(function() {
  $("a.help-text-toggle").click(function(event) {
    event.preventDefault();
    var toggleButton = $(this);
    var helpTextId = $(this).data("help-text-id");
    $("#" + helpTextId).toggle();
    if ($("#" + helpTextId).hasClass('active')) {
      $("#" + helpTextId).removeClass('active');
      toggleButton.removeClass('active');
    } else {
      $("#" + helpTextId).addClass('active');
      toggleButton.addClass('active');
    }
  });
});


/* Dynamic "add to cart" buttons*/
function updateAllCartButtons(storedOrderItems) {
  $('.add-to-cart-btn').each(function() {
    var uuid = $(this).attr('itemuuid');
    if ($.inArray(uuid, storedOrderItems) > -1) {
      $(this).addClass('added');
      $(this).attr('title', 'Allerede lagt til i kurv');
      $(this).children('.button-text').text(' Lagt i kurv');
    }
  });
}

function updateCartButton(element, storedOrderItems) {
  var uuid = $(element).attr('itemuuid');
  if ($.inArray(uuid, storedOrderItems) > -1) {
    $('.add-to-cart-btn[itemuuid="' + uuid + '"]').each(function() {
      var itemname = $(this).attr('itemname') !== undefined ? ' ' + $(this).attr('itemname') + ' ' : '';
      $(this).addClass('added');
      $(this).attr('data-original-title', 'Fjern' + itemname + 'fra kurv');
      $(this).children('.button-text').text(' Fjern fra kurv');
    });
  } else {
    $('.add-to-cart-btn[itemuuid="' + uuid + '"]').each(function() {
      var itemname = $(this).attr('itemname') !== undefined ? ' ' + $(this).attr('itemname') + ' ' : '';
      $(this).removeClass('added');
      $(this).attr('data-original-title', 'Legg til' + itemname + 'i kurv');
      $(this).children('.button-text').text(' Legg i kurv');
    });
  }
}


/* Check if href is the same as current url */
function notCurrentUrl(url) {
  if (url == window.location.href || url == window.location.pathname) {
    return true;
  } else if (url == window.location.href + "/" || url == window.location.pathname + "/") {
    return true;
  }
}


/* Loading animation for pagination */

$("document").ready(function() {
  $("ul.pagination a, ul.breadcrumbs a").each(function() {
    if (!$(this).closest('li').hasClass('active')) {
      addDefaultLoadingAnimation($(this));
    }
  });
});


/* Remove loading animation from links same as current url */
$("document").ready(function() {
  $("body").on("click", "a", function() {
    if (notCurrentUrl($(this).attr("href"))) {
      $(this).removeClass("show-loading-animation");
    }
  });
});


/* Breadcrumbs */
function disableLastBreadcrumb() {
  if ($("ul.breadcrumbs li").last().has('a').length) {
    var lastBreadcrumbText = ($("ul.breadcrumbs li").last().text());
    $("ul.breadcrumbs li").last().html(lastBreadcrumbText);
  }
}
$("document").ready(function() {
  disableLastBreadcrumb();
});


/* Alerts */
function showAlert(message, colorClass) {
  $('#feedback-alert').attr('class', 'alert alert-dismissible alert-' + colorClass);
  $('#feedback-alert .message').html($('#feedback-alert .message').html() + message + "<br/>");
  $('#feedback-alert').show();
}

function clearAlertMessage() {
  $('#feedback-alert .message').html("");
}

function hideAlert() {
  $('#feedback-alert').hide();
}

// Get URL parameters
function getParameterByName(name) {
  var url = location.search.toLowerCase();
  name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
  var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
  results = regex.exec(url);
  return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

// Remove URL parameters from string
function removeParameterByName(name, urlParameters) {
  name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
  var regex = new RegExp("[\\?&]" + name + "([^&#]*)"),
  results = regex.exec(urlParameters);
  urlParameters = urlParameters.replace(results[0], "");
  return urlParameters;
}

// Remove URL parameters from url field
function removeParameterByNameFromUrl(name) {
  var urlParameters = location.search.toLowerCase();
  urlParameters = removeParameterByName(name, urlParameters);
  var newRelativeUrl = location.pathname + urlParameters;
  window.history.replaceState({ path: newRelativeUrl }, null, newRelativeUrl);
}
$(document).on('focus', '.custom-select-list-input', function() {
  var customSelectListElement = $(this).closest('.custom-select-list');
  var dropdownElement = customSelectListElement.find('.custom-select-list-dropdown-container');
  dropdownElement.addClass('active');

  // Add automatic scroll if dropdown element is outside window
  var dropdownContentElement = dropdownElement.find('.custom-select-list-dropdown-content');
  var dropdownAdditionalOptionsElement = dropdownElement.find('.custom-select-list-dropdown-additional-options');
  var dropdownContentElementBottomPosition = dropdownContentElement.offset().top + dropdownContentElement.height();
  if (dropdownAdditionalOptionsElement.length) {
    dropdownContentElementBottomPosition += dropdownAdditionalOptionsElement.height();
  }
  var windowBottomPosition = window.innerHeight + window.scrollY;
  if (dropdownContentElementBottomPosition > windowBottomPosition) {
    var newWindowTopPosition = dropdownContentElementBottomPosition - window.innerHeight;
    window.scroll(0, newWindowTopPosition, 'smooth'); 
  }
});

$(document).on('click', '.custom-select-list-input-container', function() {
  $(this).find('.custom-select-list-input').focus();
});

function filterDropdownList(inputElement, selectListElement){
  var dropdownListElements = selectListElement.find('.custom-select-list-options');
  var filter = inputElement.val().toUpperCase();
  for (var listIndex = 0; listIndex < dropdownListElements.length; listIndex++) {
    var listItems = dropdownListElements[listIndex].getElementsByTagName('li');
    var hasResults = false;
    for (var i = 0; i < listItems.length; i++) {
      if (listItems[i].innerHTML.toUpperCase().indexOf(filter) > -1) {
        listItems[i].style.display = "";
        hasResults = true;
      } else {
        listItems[i].style.display = "none";
      }
    }

    var optionGroupNameElement = $(dropdownListElements[listIndex]).closest("div").find(".custom-select-list-option-group-name");
    if (!hasResults) {
      optionGroupNameElement.hide();
    } else {
      optionGroupNameElement.show();
    }
  }
}

$(document).on('keyup', '.custom-select-list', function(){
  var inputElement = $(this).find('.custom-select-list-input');
  filterDropdownList(inputElement, $(this));
});

function initCustomSelectList(){
  if ($('.custom-select-list').length){
    $(document).on('click', function(e){
      var insideContainer = false;
      var activeSelectListKey = null;
      $('.custom-select-list').each(function(key, selectList){
        var target = e.target;
        while (target && target.parentNode) {
          target = target.parentNode; 
          if(target && target == selectList) { 
            insideContainer = true;
            activeSelectListKey = key;
          } 

        }
      });
      $('.custom-select-list').each(function(key, selectList){
        if (key !== activeSelectListKey){
          $(this).find('.custom-select-list-dropdown-container').removeClass('active');
          var inputElement = $(this).find('.custom-select-list-input');
          inputElement.val('');
          filterDropdownList(inputElement, $(this));
        }
      });
    });
  }
}

$("document").ready(function () {
  initCustomSelectList();
});

