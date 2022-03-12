namespace ISchemm.DurationFinder {
    public static class Providers {
        public static readonly INetworkProvider All = new ChainedNetworkProvider(
            new OEmbedJsonProvider(),
            new DocumentNetworkProvider(
                new ChainedDocumentProvider(
                    new SchemaOrgProvider(),
                    new OEmbedDiscoveryProvider())));
    }
}
