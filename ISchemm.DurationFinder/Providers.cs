namespace ISchemm.DurationFinder {
    public static class Providers {
        public static readonly IDurationProvider All = new ChainedDurationProvider(
            new OEmbedJsonDurationProvider(),
            new SchemaOrgDurationProvider(),
            new OpenGraphDurationProvider(),
            new OEmbedDiscoveryDurationProvider());
    }
}
