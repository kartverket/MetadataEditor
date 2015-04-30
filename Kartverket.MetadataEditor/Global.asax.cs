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

namespace Kartverket.MetadataEditor
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredIfAttribute), typeof(RequiredIfValidator));
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(AssertThatAttribute), typeof(AssertThatValidator));

            // override standard error messages
            ClientDataTypeModelValidatorProvider.ResourceClassKey = "DefaultResources";
            DefaultModelBinder.ResourceClassKey = "DefaultResources";


            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(RequiredAttribute), typeof(MyRequiredAttributeAdapter));

            // setting locale
            var culture = new CultureInfo("nb-NO");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // init log4net
            log4net.Config.XmlConfigurator.Configure();

            MvcHandler.DisableMvcResponseHeader = true;

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();

            log.Error("App_Error", ex);
        }
    }
}
