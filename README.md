# ISchemm.DurationFinder

This library will attempt detect the duration of a video, given a URL that
points to either a raw video resource (that you might put in a `<video>` tag)
or a supported video-sharing site.

## Providers

* Network providers (read from URL)
    * **OEmbedNetworkProvider** (for oEmbed JSON endpoints with a non-standard `duration` field, such as Vimeo)

* Document providers (read from HTML)
    * **SchemaOrgDocumentProvider** (for pages with a schema.org style `<meta itemprop="duration">` tag, such as YouTube and SoundCloud)
    * **OEmbedDocumentProvider** (for pages that provide oEmbed discovery through a `<link>` element to a JSON endpoint)

Recommended flow:

* Use `ChainedDocumentProvider` to combine multiple document providers
* Use `DocumentNetworkProvider` to convert the resulting document provider to a location provider
* Use `ChainedNetworkProvider` to combine it with your other location providers
* Call the extension function `GetDurationAsync(Uri)` on the combined location provider

This will perform a HEAD request to the server to determine the content type,
and pass the type to each location provider (so the actual data fetch can be]
skipped if the provider doesn't support the type). The first provider to come
up with a valid duration value will be used, and any remaining providers will
be skipped.

Or - just use `GetDurationAsync(Uri)` on the static object `Providers.All`
and the library will do this for you.
