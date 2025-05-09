using Kartverket.MetadataEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Net;

namespace Kartverket.MetadataEditor.Views.Home
{
    public class HomeController : Controller
    {
        //public ActionResult Index()
        //{
        //    return RedirectToAction("Index", "Metadata");
        //}
        [Route("setculture/{culture}")]
        public ActionResult SetCulture(string culture, string ReturnUrl)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
            HttpCookie cookie = Request.Cookies["_culture"];

            if (cookie != null)
            {
                if (cookie.Domain != ".geonorge.no")
                {
                    HttpCookie oldCookie = new HttpCookie("_culture");
                    oldCookie.Domain = cookie.Domain;
                    oldCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(oldCookie);
                }
            }

            if (cookie != null)
            {
                cookie.Value = culture;   // update cookie value
                cookie.Expires = DateTime.Now.AddYears(1);
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
            }
            Response.Cookies.Add(cookie);

            if (!string.IsNullOrEmpty(ReturnUrl))
                return Redirect(ReturnUrl);
            else
                return RedirectToAction("Index");
        }

        public void SignIn()
        {
            var redirectUrl = Url.Action(nameof(Controllers.MetadataController.Index), "Metadata");
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void SignOut()
        {
            // Change loggedIn cookie
            var cookie = Request.Cookies["_loggedIn"];

            if (cookie != null)
            {
                cookie.Value = "false";   // update cookie value
                //cookie.SameSite = SameSiteMode.Lax;
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";

                Response.Cookies.Set(cookie);
            }
            else
            {
                cookie = new HttpCookie("_loggedIn");
                cookie.Value = "false";
                //cookie.SameSite = SameSiteMode.Lax;

                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";

                Response.Cookies.Add(cookie);
            }


            var authenticationProperties = new AuthenticationProperties {RedirectUri = WebConfigurationManager.AppSettings["GeoID:PostLogoutRedirectUri"]};

            HttpContext.GetOwinContext().Authentication.SignOut(
                authenticationProperties,
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }

        /// <summary>
        /// This is the action responding to the signout-callback-oidc route after logout at the identity provider
        /// </summary>
        /// <returns></returns>
        public ActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(Controllers.MetadataController.Index), "Metadata");
        }
    }
}