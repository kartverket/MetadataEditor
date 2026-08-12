using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Kartverket.MetadataEditor.Helpers
{
    /// <summary>
    /// Queues analytics events on the server and hands them to the browser through TempData,
    /// where Scripts/posthog-tracking.js captures them. Events are recorded from the action that
    /// succeeded, not from a form submit, so a failed validation does not count as a save - and
    /// TempData carries the event across the redirect the POST actions end with.
    ///
    /// Nothing user identifying goes into an event. The editor is authenticated and every page
    /// carries the user's name and email, so only the shape of what was done is captured.
    /// </summary>
    public static class TelemetryHelper
    {
        private const string EventsKey = "posthogEvents";

        /// <summary>
        /// Real search terms are short. Longer input is pasted content, which is where personal
        /// data realistically ends up.
        /// </summary>
        private const int MaxSearchTermLength = 100;

        /// <summary>Fødselsnummer and D-nummer, also written as 6 digits + space + 5.</summary>
        private static readonly Regex NationalIdentityNumber = new Regex(@"\d{6}\s?\d{5}");

        private static readonly Regex EmailAddress = new Regex(@"\S+@\S+\.\S+");
        private static readonly Regex ConsecutiveWhitespace = new Regex(@"\s+");

        /// <summary>Index of a collection entry in a model state key, as in "Distributions[3].Protocol".</summary>
        private static readonly Regex CollectionIndex = new Regex(@"\[\d+\]");

        /// <summary>
        /// Queues an event for the next page the browser renders. Several events can be queued
        /// for the same page - a delete redirects to the metadata list, which may queue an event
        /// of its own.
        /// </summary>
        public static void Capture(TempDataDictionary tempData, string eventName, IDictionary<string, object> properties = null)
        {
            if (tempData == null || string.IsNullOrWhiteSpace(eventName))
                return;

            var events = QueuedEvents(tempData);
            events.Add(new Dictionary<string, object>
            {
                { "name", eventName },
                { "properties", properties ?? new Dictionary<string, object>() }
            });

            tempData[EventsKey] = JsonConvert.SerializeObject(events);
        }

        /// <summary>
        /// The queued events as JSON, or an empty string when there are none. Reading removes
        /// them, so an event is captured once and not again on the following page.
        /// </summary>
        public static string TakeQueuedEvents(TempDataDictionary tempData)
        {
            if (tempData == null)
                return string.Empty;

            return tempData[EventsKey] as string ?? string.Empty;
        }

        /// <summary>
        /// Returns the search term normalised for analytics, or an empty string when it must not
        /// be captured. An empty result means "no term recorded", not "empty search".
        /// </summary>
        public static string SanitizeSearchTerm(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var term = ConsecutiveWhitespace.Replace(text.Trim(), " ").ToLowerInvariant();

            if (term.Length > MaxSearchTermLength)
                return string.Empty;

            if (NationalIdentityNumber.IsMatch(term) || EmailAddress.IsMatch(term))
                return string.Empty;

            return term;
        }

        /// <summary>
        /// The names of the fields that blocked a save, sorted and without duplicates. Field names
        /// are schema names and safe to record - the error messages are not, because some of them
        /// quote what the user entered. Collection indexes are collapsed, so a bad protocol on the
        /// fourth distribution reports as "DistributionsFormats[].Protocol" rather than as a value
        /// of its own; that also keeps the list bounded by the form rather than by the data.
        /// </summary>
        public static IList<string> InvalidFieldNames(ModelStateDictionary modelState)
        {
            if (modelState == null)
                return new List<string>();

            return modelState
                .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
                .Select(entry => CollectionIndex.Replace(entry.Key ?? string.Empty, "[]"))
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Peeks at what is already queued, so appending does not consume the entry.
        /// </summary>
        private static List<Dictionary<string, object>> QueuedEvents(TempDataDictionary tempData)
        {
            var json = tempData.Peek(EventsKey) as string;
            if (string.IsNullOrEmpty(json))
                return new List<Dictionary<string, object>>();

            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json)
                   ?? new List<Dictionary<string, object>>();
        }
    }
}
