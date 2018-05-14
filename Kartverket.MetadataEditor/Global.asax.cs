using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcUnobtrusiveValidatorProvider.Validators;
using Kartverket.MetadataEditor.Util;
using System.Web.Http;
using System;
using System.Reflection;
using log4net;
using Raven.Client.Document;
using Raven.Client.Embedded;
using System.Web;
using Kartverket.MetadataEditor.Models.Translations;
using Kartverket.MetadataEditor.App_Start;
using Autofac;
using System.Collections.Specialized;

namespace Kartverket.MetadataEditor
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static EmbeddableDocumentStore Store;

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyConfig.Configure(new ContainerBuilder());

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AssertThatAttribute), typeof(AssertThatValidator));

            // override standard error messages
            ClientDataTypeModelValidatorProvider.ResourceClassKey = "DefaultResources";
            DefaultModelBinder.ResourceClassKey = "DefaultResources";


            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredAttribute), typeof(MyRequiredAttributeAdapter));

            // init log4net
            log4net.Config.XmlConfigurator.Configure();

            MvcHandler.DisableMvcResponseHeader = true;

            Store = new EmbeddableDocumentStore { ConnectionStringName = "RavenDB" };
            Store.Initialize();

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            Log.Error("Application error: " + ex.Message, ex);
        }

        protected void Application_BeginRequest()
        {
            ValidateReturnUrl(Context.Request.QueryString);

            var cookie = Context.Request.Cookies["_culture"];
            if (cookie == null)
            {
                cookie = new HttpCookie("_culture", Culture.NorwegianCode);
                if (!Request.IsLocal)
                    cookie.Domain = ".geonorge.no";
                cookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                var culture = new CultureInfo(cookie.Value);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        void ValidateReturnUrl(NameValueCollection queryString)
        {
            if (queryString != null)
            {
                var returnUrl = queryString.Get("returnUrl");
                if (!string.IsNullOrEmpty(returnUrl))
                {

                    if (!returnUrl.StartsWith("/"))
                        HttpContext.Current.Response.StatusCode = 400;
                }
            }
        }
    }
}
