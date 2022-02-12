using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.StreamManager
{
    internal class ByteCountingStream : Stream
    {
        private readonly Stream _inner;

        public Action<int> UpdateBytesSentCallback { get; init; } = _ => { };
        public Action<int> UpdateBytesReceivedCallback { get; init; } = _ => { };
        public Action<ByteCountingStream> DisposeCallback { get; init; } = _ => { };

        public ByteCountingStream(Stream inner)
        {
            _inner = inner;
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override bool CanTimeout => _inner.CanTimeout;

        public override int ReadTimeout
        {
            get => _inner.ReadTimeout;
            set => _inner.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _inner.WriteTimeout;
            set => _inner.WriteTimeout = value;
        }

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _inner.FlushAsync(cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = _inner.Read(buffer, offset, count);
            UpdateBytesReceivedCallback?.Invoke(bytesRead);
            return bytesRead;
        }

        public override int Read(Span<byte> buffer)
        {
            var bytesRead = _inner.Read(buffer);
            UpdateBytesReceivedCallback?.Invoke(bytesRead);
            return bytesRead;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytesRead = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            UpdateBytesReceivedCallback?.Invoke(bytesRead);
            return bytesRead;
        }

        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var bytesRead = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            UpdateBytesReceivedCallback?.Invoke(bytesRead);
            return bytesRead;
        }

        public override int ReadByte()
        {
            var result = _inner.ReadByte();

            UpdateBytesReceivedCallback?.Invoke(1);

            return result;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            throw new NotSupportedException("BeginRead not supported");
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("EndRead not supported");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
            UpdateBytesSentCallback?.Invoke(count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _inner.Write(buffer);
            UpdateBytesSentCallback?.Invoke(buffer.Length); // FIXME: length?
        }

        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            UpdateBytesSentCallback?.Invoke(count);
        }

        public async override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            UpdateBytesSentCallback?.Invoke(buffer.Length); // FIXME: length?
        }

        public override void WriteByte(byte value)
        {
            _inner.WriteByte(value);
            UpdateBytesSentCallback?.Invoke(1);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            throw new NotSupportedException("BeginWrite not supported");
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("EndWrite not supported");
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await _inner.DisposeAsync().ConfigureAwait(false);
        }

        public override void Close()
        {
            _inner.Close();
            base.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            _inner.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _inner.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override string ToString()
        {
            return _inner.ToString()!;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCallback?.Invoke(this);
            }

            base.Dispose(disposing);
        }

        public override bool Equals(object? obj)
        {
            return obj is ByteCountingStream stream &&
                   EqualityComparer<Stream>.Default.Equals(_inner, stream._inner);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_inner);
        }
    }
}
