using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

namespace ISchemm.DurationFinder {
    public interface IDocumentProvider {
        Task<TimeSpan?> GetDurationAsync(HtmlDocument document);
    }
}
