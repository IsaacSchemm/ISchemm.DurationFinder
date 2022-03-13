﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public static class Extensions {
        private static readonly string UserAgentString = "ISchemm.DurationFinder/3.0 (https://github.com/IsaacSchemm/ISchemm.DurationFinder)";

        private static readonly HttpClient _httpClient = new HttpClient();

        internal static async Task<byte[]?> GetRangeAsync(this Uri uri, long from, long to) {
            using var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.UserAgent.ParseAdd(UserAgentString);
            req.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(from, to);

            using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            if (!resp.Content.Headers.ContentRange.HasRange) return null;
            if (resp.Content.Headers.ContentRange.From != from) return null;
            if (resp.Content.Headers.ContentRange.To < to) return null;
            if (resp.Content.Headers.ContentRange.Unit != "bytes") return null;

            var stream = await resp.Content.ReadAsStreamAsync();
            var arr = new byte[to - from];
            return await stream.ReadAsync(arr, 0, arr.Length) == arr.Length
                ? arr
                : null;
        }

        internal static bool IsOfType(this HttpContent content, params string[] types) {
            foreach (string t in types)
                if (string.Equals(content.Headers.ContentType.MediaType, t, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        public static async Task<TimeSpan?> GetDurationAsync(this IDurationProvider provider, Uri initial_uri) {
            var canonical = new List<Uri> { initial_uri };

            for (int i = 0; i < canonical.Count; i++) {
                Uri uri = canonical[i];

                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.UserAgent.ParseAdd(UserAgentString);

                using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

                if (await provider.GetDurationAsync(uri, resp.Content) is TimeSpan ts)
                    return ts;

                switch (resp.Content.Headers.ContentType.MediaType) {
                    case "text/html":
                    case "application/xhtml+xml":
                        var document = new HtmlDocument();
                        string html = await resp.Content.ReadAsStringAsync();
                        document.LoadHtml(html);

                        foreach (var node in document.DocumentNode.Descendants("link"))
                            if (node.GetAttributeValue("rel", null) == "canonical")
                                if (node.GetAttributeValue("href", null) is string str)
                                    if (Uri.TryCreate(resp.Headers.Location, HtmlEntity.DeEntitize(str), out Uri new_uri))
                                        if (!canonical.Contains(new_uri))
                                            canonical.Add(new_uri);

                        break;
                }
            }

            return null;
        }
    }
}
