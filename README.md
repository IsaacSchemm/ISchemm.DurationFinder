# ISchemm.DurationFinder

This library will attempt detect the duration of a video, given a URL that
points to either a raw video resource (that you might put in a `<video>` tag)
or a supported video-sharing site.

## Providers

* SchemaOrgDurationProvider (for pages with a schema.org style `<meta itemprop="duration">` tag)
* OpenGraphDurationProvider (for pages with an OpenGraph style `<meta property="video:duration">` tag)
* OEmbedDiscoveryDurationProvider (for pages that provide oEmbed discovery through a `<link>` element to a JSON endpoint)
* OEmbedJsonDurationProvider (for oEmbed JSON endpoints that include a non-standard `duration` field)
* HlsPlaylistDurationProvider (for HLS VOD playlists)
* HlsChunklistDurationProvider (for HLS VOD chunklists)
* MP4DurationProvider (for MPEG-4 Part 14 containers, such as .mp4 and .m4a - requires HTTP range request support on the remote server)

## Support

* YouTube (SchemaOrgDurationProvider)
* SoundCloud (SchemaOrgDurationProvider)
* Vimeo (OEmbedDiscoveryDurationProvider)
* Dailymotion (OpenGraphDurationProvider)
* HLS (.m3u8) (HlsPlaylistDurationProvider)
* .mp4 (MP4DurationProvider)
* .m4a (MP4DurationProvider)

## Usage

* Use `ChainedDurationProvider` to combine multiple providers (or just use the static object `Providers.All`)
* Call the extension function `GetDurationAsync(Uri)` on the resulting provider object

Each provider will check the content type of the GET response, and only
download what data is necessary. The first provider to come up with a valid
duration value will be used, and any remaining providers will be skipped. If
none of the providers come up with a result, `GetDurationAsync(Uri)` will
check for a `<link rel="canonical">` tag and try again (provided that the URL
in the tag has not been attempted yet).
