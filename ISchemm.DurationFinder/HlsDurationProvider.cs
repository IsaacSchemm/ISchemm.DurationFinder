﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public class HlsDurationProvider : IDurationProvider {
        private readonly IDurationProvider _chunklistProvider = new ChunklistDurationProvider();

        public async Task<TimeSpan?> GetDurationAsync(Uri originalLocation, HttpContent httpContent) {
            if (!httpContent.IsOfType("application/x-mpegURL", "application/x-vnd.apple.mpegURL", "audio/mpegURL", "audio/x-mpegURL"))
                return null;

            await httpContent.LoadIntoBufferAsync();

            string body = await httpContent.ReadAsStringAsync();
            using var sr = new StringReader(body);

            string line;
            while ((line = await sr.ReadLineAsync()) != null)
                if (!line.StartsWith("#") && line.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
                    if (Uri.TryCreate(originalLocation, line, out Uri chunklist))
                        if (await _chunklistProvider.GetDurationAsync(chunklist) is TimeSpan ts)
                            return ts;

            return null;
        }
    }
}
