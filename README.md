# ISchemm.DurationFinder

This library will attempt detect the duration of a video, given a URL that
points to either a raw video resource (that you might put in a `<video>` tag)
or a supported video-sharing site.

## Usage

The DurationFinder library has two categories of "providers": implementations
of `IDocumentProvider` that pull data from an HTML page, and implementations
of `INetworkProvider` that pull data from a URL, given its content type.

Most users won't have to use anything beyond the static object `Providers.All`
and its extension function `GetDurationAsync(Uri)`. This function will make a
HEAD request to the URL to determine its content type, and then try whichever
providers are appropriate; the first duration returned by any of the available
providers will be used. If no duration can be detected, a null value will be
returned instead.

## Providers

* Implementations of **IDocumentProvider**
    * **SchemaOrgProvider** (pages with [schema.org video markup](https://developers.google.com/search/blog/2012/02/using-schemaorg-markup-for-videos), such as YouTube)
    * **ChainedDocumentProvider** (combines multiple IDocumentProviders, attempting them in sequence)
* Implementations of **INetworkProvider**
    * **OEmbedJsonProvider** (oEmbed JSON data with a non-standard `duration` field)
    * **DocumentNetworkProvider** (fetches HTML data and passes it to an IDocumentProvider)
    * **ChainedNetworkProvider** (combines multiple IDocumentProviders, attempting them in sequence)
