using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kartverket.MetadataEditor
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("SignIn", "SignIn", new { controller = "Home", action = "SignIn" });
            routes.MapRoute("SignOut", "SignOut", new { controller = "Home", action = "SignOut" });
            // authentication - openid connect 
            routes.MapRoute("OIDC-callback-signout", "signout-callback-oidc", new { controller = "Home", action = "SignOutCallback" });

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Metadata", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
