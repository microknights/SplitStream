using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable AccessToDisposedClosure

namespace MicroKnights.IO.Streams
{
    public class ReadableSplitStream : IDisposable
    {
        private readonly Stream _sourceStream;
        private readonly SplitStreamOptions _options;
        private readonly List<ForwaredReadOnlyChunkStream> _splitStreams = new List<ForwaredReadOnlyChunkStream>();

        private volatile bool _finished = false;
        private volatile bool _started = false;

        public ReadableSplitStream(Stream stream)
            : this(stream, SplitStreamOptions.Default)
        {
        }

        public ReadableSplitStream(Stream stream, int chunkSize)
            : this(stream, SplitStreamOptions.Default.SetChunkSize(chunkSize))
        {
        }

        public ReadableSplitStream(Stream stream, SplitStreamOptions options)
        {
            _sourceStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _options = options;
            if (options.ChunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(SplitStreamOptions.ChunkSize),options.ChunkSize,"> 0");
        }

        protected virtual ForwaredReadOnlyChunkStream CreateForwaredReadOnlyChunkStream()
        {
            return new ForwaredReadOnlyChunkStream(_options);
        }

        public Stream GetForwardReadOnlyStream()
        {
            if (_started) throw new InvalidOperationException("Data stream reading already started!");

			var stream = CreateForwaredReadOnlyChunkStream();
			_splitStreams.Add(stream);
			return stream;
        }

		public Task StartReadAhead() {
            return Task.Run(ReadAheadChunks);
		}
		
        private void ReadAheadChunks()
        {
            _started = true;

            do
            {
#if DEBUG
                DebugWaitBeforeReadingSourceStreamChunk();
#endif
                var chunk = new StreamChunk(new byte[_options.ChunkSize]);
                chunk.Length = _sourceStream.Read(chunk.Buffer, 0, chunk.Buffer.Length);
                if (chunk.Length > 0)
                {
                    PushChunkToStreams(chunk);
                }
                else
                {
                    PushChunkToStreams(null);
#if DEBUG
                    DebugWaitAfterReadingSourceStreamChunk();
#endif
                    _finished = true;
                }
            } while (_finished == false);
        }

#if DEBUG
        protected virtual void DebugWaitBeforeReadingSourceStreamChunk()
        {
        }
#endif

#if DEBUG
        protected virtual void DebugWaitAfterReadingSourceStreamChunk()
        {
            Task.Delay(10000).ConfigureAwait(false).GetAwaiter().GetResult();
        }
#endif

        private void PushChunkToStreams(StreamChunk chunk)
        {
            foreach (var stream in _splitStreams)
            {
                stream.PushChunk(chunk);
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_options.SanityCheckOnDispose && (_finished == false || _splitStreams.Any(ss=>ss.IsFinised == false)))
            {
                throw new ObjectDisposedException("Stream not read to end");
            }
        }

        #endregion
    }
}