using ISchemm.DurationFinder.DocumentProviders;
using ISchemm.DurationFinder.NetworkProviders;

namespace ISchemm.DurationFinder {
    public static class Providers {
        public static readonly OEmbedJsonProvider OEmbedJson = new OEmbedJsonProvider();

        public static readonly SchemaOrgProvider SchemaOrg = new SchemaOrgProvider();
        public static readonly OEmbedDiscoveryProvider OEmbedDiscovery = new OEmbedDiscoveryProvider(OEmbedJson);

        public static readonly INetworkProvider All = new ChainedNetworkProvider(
            OEmbedJson,
            new DocumentNetworkProvider(
                new ChainedDocumentProvider(
                    SchemaOrg,
                    OEmbedDiscovery)));
    }
}
