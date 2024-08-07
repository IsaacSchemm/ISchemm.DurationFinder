﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public static class Extensions {
        internal static readonly string UserAgentString = "ISchemm.DurationFinder/4.0 (https://github.com/IsaacSchemm/ISchemm.DurationFinder)";
        internal static readonly HttpClient HttpClient = new HttpClient();

        public static async Task<string> ReadAsStringAsync(this IDataSource dataSource) {
            using var ms = new MemoryStream(await dataSource.ReadAsync());
            using var sr = new StreamReader(ms);
            return await sr.ReadToEndAsync();
        }

        public static async Task<TimeSpan?> GetDurationAsync(this IDurationProvider provider, Uri initial_uri) {
            var canonical = new List<Uri> { initial_uri };

            for (int i = 0; i < canonical.Count; i++) {
                Uri uri = canonical[i];

                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.UserAgent.ParseAdd(UserAgentString);

                using var dataSource = new RemoteDataSource(await HttpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead));

                if (await provider.GetDurationAsync(dataSource) is TimeSpan ts)
                    return ts;

                if (dataSource.MatchesType("text/html", "application/xhtml+xml")) {
                    var document = new HtmlDocument();
                    string html = await dataSource.ReadAsStringAsync();
                    document.LoadHtml(html);

                    foreach (var node in document.DocumentNode.Descendants("link"))
                        if (node.GetAttributeValue("rel", null) == "canonical")
                            if (node.GetAttributeValue("href", null) is string str)
                                if (dataSource.TryCreateRelativeUri(HtmlEntity.DeEntitize(str), out Uri new_uri))
                                    if (!canonical.Contains(new_uri))
                                        canonical.Add(new_uri);
                }
            }

            return null;
        }
    }
}
