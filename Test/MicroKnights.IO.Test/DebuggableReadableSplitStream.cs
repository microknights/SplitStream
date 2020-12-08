using System;
using System.IO;
using System.Threading;
using MicroKnights.IO.Streams;

namespace MicroKnights.IO.Test
{
    public class DebuggableReadableSplitStream : ReadableSplitStream
    {
        private readonly int _splitStreamMaxSleepMilliseconds;
        private readonly int _forwardReadOnlyMaxSleepMilliseconds;

        private readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public DebuggableReadableSplitStream(int splitStreamMaxSleepMilliseconds, int forwardReadOnlyMaxSleepMilliseconds, Stream stream, SplitStreamOptions options)
            : base(stream, options)
        {
            _splitStreamMaxSleepMilliseconds = splitStreamMaxSleepMilliseconds;
            _forwardReadOnlyMaxSleepMilliseconds = forwardReadOnlyMaxSleepMilliseconds;
        }

        protected override ForwaredReadOnlyChunkStream CreateForwaredReadOnlyChunkStream()
        {
            return new DebuggableForwaredReadOnlyChunkStream(_forwardReadOnlyMaxSleepMilliseconds);
        }

        protected override void DebugWaitBeforeReadingSourceStreamChunk()
        {
            if (_splitStreamMaxSleepMilliseconds > 0)
                Thread.Sleep(_random.Next(_splitStreamMaxSleepMilliseconds));
        }
    }
}