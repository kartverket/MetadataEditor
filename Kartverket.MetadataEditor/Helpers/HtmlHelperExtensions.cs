﻿using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Kartverket.MetadataEditor.Models.Translations;


namespace Kartverket.MetadataEditor.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString ApplicationVersionNumber(this HtmlHelper helper)
        {
            string versionNumber = WebConfigurationManager.AppSettings["BuildVersionNumber"];
            return MvcHtmlString.Create(versionNumber);
        }
        public static string GeonorgeUrl(this HtmlHelper helper)
        {
            var url = WebConfigurationManager.AppSettings["GeonorgeUrl"];
            var culture = CultureHelper.GetCurrentCulture();
            if (culture != Culture.NorwegianCode)
                url = url + Culture.EnglishCode;

            return url;
        }
        public static string GeonorgeArtiklerUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["GeonorgeArtiklerUrl"];
        }
        public static string NorgeskartUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["NorgeskartUrl"];
        }
        public static string SecureNorgeskartUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["SecureNorgeskartUrl"];
        }

        public static string GeonorgeWebserviceUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["GeonorgeWebserviceUrl"];
        }
        public static string RegistryUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["RegistryUrl"];
        }
        public static string ObjektkatalogUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["ObjektkatalogUrl"];
        }
        public static string KartkatalogUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["KartkatalogUrl"];
        }

        public static bool SimpleMetadataEnabled(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["SimpleMetadataEnabled"] == "false" ? false : true;
        }

        public static string EnvironmentName(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["EnvironmentName"];
        }

        public static string EnvironmentNameInUrl(this HtmlHelper helper)
        {
            var environment = WebConfigurationManager.AppSettings["EnvironmentName"];

            if(environment != "")
                environment = "." + environment;

            return environment;
        }

        public static string WebmasterEmail(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["WebmasterEmail"];
        }

        public static bool SupportsMultiCulture(this HtmlHelper helper)
        {
            return Boolean.Parse(WebConfigurationManager.AppSettings["SupportsMultiCulture"]); ;
        }
    }
}