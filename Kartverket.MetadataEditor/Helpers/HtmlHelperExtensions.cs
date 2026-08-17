using System;
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

        public static string PostHogApiKey(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["PostHog:ApiKey"];
        }

        public static string PostHogApiHost(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["PostHog:ApiHost"];
        }

        public static string PostHogUiHost(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["PostHog:UiHost"];
        }

        /// <summary>
        /// Captures a $autocapture event for every click and input in addition to the events in
        /// Scripts/posthog-tracking.js. Off unless configured - those events carry the text of
        /// whatever was clicked, and in the editor that text is metadata being written.
        /// </summary>
        public static bool PostHogAutocapture(this HtmlHelper helper)
        {
            return BoolSetting("PostHog:Autocapture", false);
        }

        /// <summary>
        /// Session replay records the rendered page. Every page in the editor is authenticated and
        /// shows the user's name and email, so this stays disabled unless configured otherwise.
        /// </summary>
        public static bool PostHogDisableSessionRecording(this HtmlHelper helper)
        {
            return BoolSetting("PostHog:DisableSessionRecording", true);
        }

        private static bool BoolSetting(string key, bool fallback)
        {
            bool value;
            return Boolean.TryParse(WebConfigurationManager.AppSettings[key], out value) ? value : fallback;
        }
    }
}