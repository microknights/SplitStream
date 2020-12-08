using System;
using System.Threading;
using MicroKnights.IO.Streams;

namespace MicroKnights.IO.Test
{
    public class DebuggableForwaredReadOnlyChunkStream : ForwaredReadOnlyChunkStream
    {
        private readonly int _maxSleepMilliseconds;
        private readonly Random _random = new Random((int)DateTime.Now.Ticks >> 1);
        public DebuggableForwaredReadOnlyChunkStream(int maxSleepMilliseconds) 
        {
            _maxSleepMilliseconds = maxSleepMilliseconds;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_maxSleepMilliseconds > 0)
                Thread.Sleep(_random.Next(_maxSleepMilliseconds));

            return base.Read(buffer, offset, count);
        }
    }
}