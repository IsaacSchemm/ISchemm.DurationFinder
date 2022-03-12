namespace ISchemm.DurationFinder {
    public static class Providers {
        public static readonly IDurationProvider All = new ChainedDurationProvider(
            new SchemaOrgDurationProvider(),
            new OpenGraphDurationProvider(),
            new OEmbedDiscoveryDurationProvider(),
            new OEmbedJsonDurationProvider(),
            new HlsPlaylistDurationProvider(),
            new HlsChunklistDurationProvider());
    }
}
