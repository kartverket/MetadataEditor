using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Kartverket.Geonorge.Utilities;
using Kartverket.Geonorge.Utilities.Organization;
using GeoNorgeAPI;
using System.Reflection;
using System.Web.Http;
using System.Collections.Generic;
using System.Web.Configuration;
using Kartverket.Geonorge.Utilities.LogEntry;
using Kartverket.MetadataEditor.Models;
using Kartverket.MetadataEditor.Models.OpenData;

namespace Kartverket.MetadataEditor.App_Start
{
    public class DependencyConfig
    {
        public static void Configure(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(MvcApplication).Assembly).PropertiesAutowired();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            builder.RegisterModule(new AutofacWebTypesModule());
            ConfigureAppDependencies(builder);
            var container = builder.Build();

            // dependency resolver for MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // dependency resolver for Web API
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        // the order of component registration is significant. must wire up dependencies in other packages before types in this project.
        private static void ConfigureAppDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
            builder.RegisterType<LogEntryService>().As<ILogEntryService>().WithParameters(new List<Parameter>
            {
                new NamedParameter("logUrl", WebConfigurationManager.AppSettings["LogApi"]),
                new NamedParameter("apiKey", WebConfigurationManager.AppSettings["LogApiKey"]),
                new AutowiringParameter()
            });

            // GeoNorgeAPI
            builder.RegisterType<GeoNorge>()
                .As<IGeoNorge>()
                .WithParameters(new List<Parameter>
                {
                    new NamedParameter("geonetworkUsername", ""),
                    new NamedParameter("geonetworkPassword", ""),
                    new NamedParameter("geonetworkEndpoint", WebConfigurationManager.AppSettings["GeoNetworkUrl"])
                });

            builder.RegisterType<MetadataService>().As<IMetadataService>();
            builder.RegisterType<ReportService>().As<IReportService>();
            builder.RegisterType<ValidatorService>().As<IValidatorService>();
            builder.RegisterType<BatchService>().As<IBatchService>();
            builder.RegisterType<OpenMetadataService>().As<IOpenMetadataService>();
            builder.RegisterType<OpenMetadataFetcher>().As<IOpenMetadataFetcher>();
            builder.RegisterType<MetadataContext>().InstancePerRequest().AsSelf();
        }
    }
}