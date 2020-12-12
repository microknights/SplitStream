namespace MicroKnights.IO.Streams
{
    public class SplitStreamOptions
    {
        public int ChunkSize { get; set; }
        public bool SanityCheckOnDispose { get; set; }
        public int MaxEnqueuedChunks { get; set; }

        public SplitStreamOptions SetChunkSize(int chunkSize)
        {
            ChunkSize = chunkSize;
            return this;
        }

        public static SplitStreamOptions Default => new SplitStreamOptions
        {
            SanityCheckOnDispose = false,
            ChunkSize = ushort.MaxValue,
            MaxEnqueuedChunks = int.MaxValue
        };
    }
}