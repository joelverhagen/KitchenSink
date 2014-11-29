using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.KitchenSink.Extensions;

namespace Knapcode.KitchenSink.Support
{
    public class ChainedStream : Stream
    {
        private readonly bool _disposeOnCompletion;
        private readonly IEnumerator<Stream> _streams;
        private bool _started;
        private bool _finished;

        public ChainedStream(IEnumerable<Stream> streams, bool disposeOnCompletion = true)
        {
            Guard.ArgumentNotNull(streams, "streams");

            _started = false;
            _finished = false;
            _streams = streams.GetEnumerator();
            _disposeOnCompletion = disposeOnCompletion;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            // start the stream enumerator
            if (!_started && !(_started = _streams.MoveNext()))
            {
                return 0;
            }

            // read the streams until the desired amount is returned
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await _streams.Current.ReadAsync(buffer, offset + totalRead, count - totalRead, cancellationToken);
                if (read == 0)
                {
                    if (_disposeOnCompletion)
                    {
                        _streams.Current.Dispose();
                    }

                    if (!_streams.MoveNext())
                    {
                        return totalRead;
                    }
                }

                totalRead += read;
            }

            return totalRead;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return ReadAsync(buffer, offset, count).ToApm(callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>) asyncResult).Result;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // start the stream enumerator
            if ((!_started && !(_started = _streams.MoveNext())) || _finished)
            {
                return 0;
            }

            // read the streams until the desired amount is returned
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = _streams.Current.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0)
                {
                    if (_disposeOnCompletion)
                    {
                        _streams.Current.Dispose();
                    }

                    if (!_streams.MoveNext())
                    {
                        _finished = true;
                        return totalRead;
                    }
                }

                totalRead += read;
            }

            return totalRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}