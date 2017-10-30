using System;
using System.IO;
using System.Threading;
using MicroKnights.IO.Streams;

namespace MicroKnights.IO.Test
{
    public class DebuggableReadableSplitStream : ReadableSplitStream
    {
        private readonly int _splitStreamMaxSleepMilliseconds;
        private readonly int _forwardReadOnlyNaxSleepMilliseconds;

        private readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public DebuggableReadableSplitStream(int splitStreamMaxSleepMilliseconds, int forwardReadOnlyNaxSleepMilliseconds, Stream stream, int chunkSize = UInt16.MaxValue)
            : base(stream, chunkSize)
        {
            _splitStreamMaxSleepMilliseconds = splitStreamMaxSleepMilliseconds;
            _forwardReadOnlyNaxSleepMilliseconds = forwardReadOnlyNaxSleepMilliseconds;
        }

        protected override ForwaredReadOnlyChunkStream CreateForwaredReadOnlyChunkStream()
        {
            return new DebuggableForwaredReadOnlyChunkStream(_forwardReadOnlyNaxSleepMilliseconds);
        }

        protected override void DebugWaitBeforeReadingSourceStreamChunk()
        {
            Thread.Sleep(_random.Next(_splitStreamMaxSleepMilliseconds));
        }
    }
}