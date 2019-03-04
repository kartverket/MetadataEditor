using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

namespace Kartverket.MetadataEditor.Models
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
      
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (Authorize(actionContext))
            {
                return;
            }
            HandleUnauthorizedRequest(actionContext);
        }

        protected override void HandleUnauthorizedRequest(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var challengeMessage = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            throw new HttpResponseException(challengeMessage);
        }

        private bool Authorize(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            try
            {
                var apiKey = (from h in actionContext.Request.Headers where h.Key == "apikey" select h.Value.First()).FirstOrDefault();
                return apiKey == WebConfigurationManager.AppSettings["apikey"];
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}