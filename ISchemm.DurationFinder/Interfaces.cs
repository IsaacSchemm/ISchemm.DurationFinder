using HtmlAgilityPack;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface INetworkProvider {
        Task<TimeSpan?> GetDurationAsync(Uri uri, ContentType contentType);
    }

    public interface IDocumentProvider {
        TimeSpan? GetDuration(HtmlDocument document);
    }
}
