# ISchemm.DurationFinder

This library will attempt detect the duration of a video, given a URL that
points to either a raw video resource (that you might put in a `<video>` tag)
or a supported video-sharing site.

## Providers

* SchemaOrgDurationProvider (for pages with a `<meta itemprop="duration">` tag)
* OpenGraphDurationProvider (for pages with a `<meta property="og:video:duration">` or `<meta property="video:duration">` tag)
* OEmbedDurationProvider (for pages that provide oEmbed discovery through a `<link>` element to a JSON endpoint which contains a non-standard `duration` property)
* HlsDurationProvider (for HLS VOD playlists)
* MP4DurationProvider (for MPEG-4 Part 14 containers, such as .mp4 and .m4a - requires HTTP range request support on the remote server)
* VorbisDurationProvider (for Ogg containers - requires HTTP range request support on the remote server, and the file must contains a Vorbis stream)

Additional providers that are not included in `Providers.All` but are used internally:

* ChunklistDurationProvider (for HLS VOD chunklists)
* JsonDurationProvider (for JSON files with a numeric `duration` property)

## Support

* YouTube (SchemaOrgDurationProvider)
* SoundCloud (SchemaOrgDurationProvider)
* Vimeo (OEmbedDurationProvider)
* Twitch (OpenGraphDurationProvider)
* Dailymotion (OpenGraphDurationProvider)
* HLS (HlsDurationProvider)
* .mp4 (MP4DurationProvider)
* .m4a (MP4DurationProvider)
* .ogg (VorbisDurationProvider)
* .oga (VorbisDurationProvider)
* .ogv (VorbisDurationProvider)

## Usage

* Use `ChainedDurationProvider` to combine multiple providers (or just use the static object `Providers.All`)
* Call the extension function `GetDurationAsync(Uri)` on the resulting provider object

Each provider will check the content type of the GET response, and only
download what data is necessary. The first provider to come up with a valid
duration value will be used, and any remaining providers will be skipped. If
none of the providers come up with a result, `GetDurationAsync(Uri)` will
check for a `<link rel="canonical">` tag and try again (provided that the URL
in the tag has not been attempted yet).
