using Kartverket.MetadataEditor.Helpers;
using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using System.Threading.Tasks;

namespace Kartverket.MetadataEditor.Views.Home
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return RedirectToAction("Index", "Metadata");
        }

        public void SignIn()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl },
                            OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                        OpenIdConnectAuthenticationDefaults.AuthenticationType,
                        CookieAuthenticationDefaults.AuthenticationType);
        }

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
    }
}