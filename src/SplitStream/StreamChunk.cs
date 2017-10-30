namespace MicroKnights.IO.Streams
{
    public class StreamChunk
    {
        public StreamChunk(byte[] buffer)
        {
            Buffer = buffer;
        }

        public byte[] Buffer;
        public int Length;
    }
}