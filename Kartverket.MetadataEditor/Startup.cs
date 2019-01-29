using Microsoft.Owin;
using Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.Jwt;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Owin.Security.DataHandler.Encoder;

[assembly: OwinStartup(typeof(Kartverket.MetadataEditor.Startup))]

namespace Kartverket.MetadataEditor
{
    public class Startup
    {
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        string clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];

        // RedirectUri is the URL where the user will be redirected to after they sign in.
        string redirectUri = System.Configuration.ConfigurationManager.AppSettings["RedirectUri"];

        string authority = System.Configuration.ConfigurationManager.AppSettings["Authority"];

        string clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];

        string metadataAddress = System.Configuration.ConfigurationManager.AppSettings["MetadataAddress"];

        /// <summary>
        /// Configure OWIN to use OpenIdConnect 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    MetadataAddress = metadataAddress,
                    Authority = authority,
                    RedirectUri = redirectUri,
                    ResponseType = OpenIdConnectResponseType.Code    
                }
            );

            List<string> audience = new List<string>();
            audience.Add(clientId);

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AllowedAudiences = audience,
                IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
                {
                    new SymmetricKeyIssuerSecurityKeyProvider(clientId, TextEncodings.Base64Url.Decode(clientSecret))
                }
            });
        }

        /// <summary>
        /// Handle failed authentication requests by redirecting the user to the home page with an error in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            context.HandleResponse();
            context.Response.Redirect("/?errormessage=" + context.Exception.Message);
            return Task.FromResult(0);
        }
    }
}
