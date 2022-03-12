# ISchemm.DurationFinder

This library will attempt detect the duration of a video, given a URL that
points to either a raw video resource (that you might put in a `<video>` tag)
or a supported video-sharing site.

## Providers

* SchemaOrgDurationProvider (for pages with a schema.org style `<meta itemprop="duration">` tag, such as YouTube and SoundCloud)
* OpenGraphDurationProvider (for pages with an OpenGraph style `<meta property="video:duration">` tag, such as Dailymotion)
* OEmbedDiscoveryDurationProvider (for pages that provide oEmbed discovery through a `<link>` element to a JSON endpoint)
* OEmbedJsonDurationProvider (for oEmbed JSON endpoints that provider a non-standard `duration` field, such as Vimeo)

## Support

* YouTube (SchemaOrgDurationProvider)
* SoundCloud (SchemaOrgDurationProvider)
* Vimeo (OEmbedDiscoveryDurationProvider)
* Dailymotion (OpenGraphDurationProvider)

## Usage

* Use `ChainedDurationProvider` to combine multiple providers
* Call the extension function `GetDurationAsync(Uri)` on the combined location provider

Each provider will check the content type of the GET response, and only
download what data is necessary. The first provider to come up with a valid
duration value will be used, and any remaining providers will be skipped.

Or - just use `GetDurationAsync(Uri)` on the static object `Providers.All`
and the library will do this for you.
