// PostHog event tracking for Metadataeditor.
// Pageviews are captured automatically by the snippet in _Layout.cshtml. Most custom events are
// queued server side by Helpers/TelemetryHelper.cs from the action that succeeded, and rendered
// into #posthog-events by _Layout.cshtml - a save that failed validation is therefore not counted.
// The two events below are captured here instead, because neither request ends in a page the
// server could attach an event to: the thumbnail upload answers with JSON, and linking metadata
// only edits the form in place.
// No personal data is sent: only what kind of metadata was worked on and how, never field values
// or user identity.

$(function () {
    if (typeof posthog === 'undefined') return;

    captureQueuedEvents();
    captureThumbnailUploads();
    captureMetadataLinking();

    // Thumbnail uploads go through jQuery.FileUpload, which uses $.ajax, so they surface as global
    // ajax events. Bound here rather than in the Vue components' done: callbacks, of which there is
    // one per upload field (see VueComponents/Edit/_Documents.cshtml and TimeAndSpace/_Coverage).
    function captureThumbnailUploads() {
        $(document).on('ajaxSuccess', function (event, jqXHR, settings, data) {
            var endpoint = thumbnailEndpoint(settings && settings.url);
            if (endpoint === null) return;

            // A rejected file type still answers 200 with status ErrorWrongContent, so the outcome
            // has to come from the body. How often editors pick an unsupported file is worth
            // knowing, so those are recorded rather than dropped.
            posthog.capture('metadataeditor_thumbnail_uploaded', {
                variant: endpoint,
                accepted: !!(data && data.status === 'OK')
            });
        });
    }

    // The upload endpoints differ in how many sizes they generate, which is worth telling apart.
    function thumbnailEndpoint(url) {
        var match = (url || '').match(/\/Metadata\/(UploadThumbnail[A-Za-z]*)/i);
        return match ? match[1] : null;
    }

    // "Koble til datasett" and the other link pickers in Views/Metadata/Edit.cshtml. The result rows
    // are built in JavaScript and carry data-link-type; a delegated handler is the only way to reach
    // them, since they do not exist when this runs.
    function captureMetadataLinking() {
        $(document).on('click', '[data-link-type]', function () {
            posthog.capture('metadataeditor_metadata_linked', {
                link_type: $(this).attr('data-link-type')
            });
        });
    }

    function captureQueuedEvents() {
        var container = document.getElementById('posthog-events');
        if (container === null) return;

        var events = parseEvents(container.getAttribute('data-events'));
        for (var i = 0; i < events.length; i++) {
            if (events[i] && events[i].name) {
                posthog.capture(events[i].name, events[i].properties || {});
            }
        }
    }

    function parseEvents(json) {
        if (!json) return [];

        try {
            var events = JSON.parse(json);
            return Array.isArray(events) ? events : [];
        } catch (e) {
            return [];
        }
    }
});
