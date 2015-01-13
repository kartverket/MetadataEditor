using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Kartverket.MetadataEditor.Models
{
    public class SeoUrl
    {
        private readonly string _seoOrganization;
        private readonly string _seoTitle;

        public SeoUrl(string organization, string title)
        {
            _seoOrganization = MakeSeoFriendlyString(organization);
            _seoTitle = MakeSeoFriendlyString(title);
        }

        public string Organization
        {
            get { return _seoOrganization; }
        }

        public string Title
        {
            get { return _seoTitle; }
        }

        private string MakeSeoFriendlyString(string input)
        {
            string encodedUrl = (input ?? "").ToLower();

            // replace & with and
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // remove characters
            encodedUrl = encodedUrl.Replace("'", "");

            // replace norwegian characters
            encodedUrl = encodedUrl.Replace("å", "a").Replace("æ", "ae").Replace("ø", "o");

            // remove invalid characters
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "-");

            // remove duplicates
            encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

            // trim leading & trailing characters
            encodedUrl = encodedUrl.Trim('-');

            return encodedUrl;
        }

        public bool Matches(string organization, string title)
        {
            return _seoOrganization == organization && _seoTitle == title;
        }
    }
}