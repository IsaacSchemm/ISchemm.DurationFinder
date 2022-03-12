using ISchemm.DurationFinder.DocumentProviders;
using ISchemm.DurationFinder.NetworkProviders;

namespace ISchemm.DurationFinder {
    public static class Providers {
        public static readonly INetworkProvider OEmbedJson = new OEmbedJsonProvider();

        public static readonly IDocumentProvider SchemaOrg = new SchemaOrgProvider();

        public static readonly INetworkProvider All = new ChainedNetworkProvider(
            OEmbedJson,
            new DocumentNetworkProvider(
                new ChainedDocumentProvider(
                    SchemaOrg)));
    }
}
