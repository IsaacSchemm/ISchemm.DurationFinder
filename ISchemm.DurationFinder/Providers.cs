namespace ISchemm.DurationFinder {
    public static class Providers {
        public static readonly INetworkProvider All = new ChainedNetworkProvider(
            new OEmbedNetworkProvider(),
            new DocumentNetworkProvider(
                new ChainedDocumentProvider(
                    new SchemaOrgDocumentProvider(),
                    new OpenGraphDocumentProvider(),
                    new OEmbedDocumentProvider())));
    }
}
