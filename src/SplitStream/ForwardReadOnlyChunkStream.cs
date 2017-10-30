using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace MicroKnights.IO.Streams
{
    public class ForwaredReadOnlyChunkStream : Stream
    {
        private readonly ConcurrentQueue<StreamChunk> _queueChunks;
        private readonly ManualResetEventSlim _queueWaiter = new ManualResetEventSlim(false, 10);

        private StreamChunk _chunk = null;
        private bool _finished = false;
        private int _position = 0;

        public ForwaredReadOnlyChunkStream()
        {
            _queueChunks = new ConcurrentQueue<StreamChunk>();
        }

        #region Overrides of Stream

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_finished) return 0;

            if (_chunk == null || _position == _chunk.Length)
            {
                _chunk = PopChunk(CancellationToken.None);
                if( _chunk == null || _chunk.Length == 0)
                { 
                    _finished = true;
                    return 0;
                }
                _position = 0;
            }

            // ReSharper disable once PossibleNullReferenceException
            var bufferAvailable = _chunk.Length - _position;
            if (bufferAvailable >= count)
            {
                Array.Copy(_chunk.Buffer, _position, buffer, offset, count);
                _position += count;
                return count;
            }
            Array.Copy(_chunk.Buffer, _position, buffer, offset, bufferAvailable);
            _position = 0;
            _chunk = null;
            return bufferAvailable;
        }

        public void PushChunk(StreamChunk chunk)
        {
            _queueChunks.Enqueue(chunk);
            _queueWaiter.Set();
        }

        public StreamChunk PopChunk(CancellationToken cancellationToken)
        {
            StreamChunk chunk;
            while (_queueChunks.TryDequeue(out chunk) == false)
            {
                _queueWaiter.Wait(cancellationToken);
            }
            _queueWaiter.Reset();
            return chunk;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => -1;
        public override long Position { get; set; }

        public bool IsFinised => _finished;

        #endregion

        #region Overrides of Stream

        protected override void Dispose(bool disposing)
        {
            if (disposing && IsFinised == false )
            {
                throw new ObjectDisposedException("Stream not read to end");
            }
            base.Dispose(disposing);
        }

        #endregion
}
}