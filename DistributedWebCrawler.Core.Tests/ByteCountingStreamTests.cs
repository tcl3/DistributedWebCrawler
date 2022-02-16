using DistributedWebCrawler.Core.Tests.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class ByteCountingStreamTests
    {
        private const string DefaultStreamContent = "TestData";

        [Theory]
        [ByteCountingStreamAutoData]
        public void LengthPropertyShouldMatchUnderlyingStream(MemoryStream innerStream)
        { 
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Equal(innerStream.Length, byteCountingStream.Length);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void CanReadPropertyShouldMatchUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Equal(innerStream.CanRead, byteCountingStream.CanRead);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void CanSeekPropertyShouldMatchUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Equal(innerStream.CanSeek, byteCountingStream.CanSeek);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void CanWritePropertyShouldMatchUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Equal(innerStream.CanWrite, byteCountingStream.CanWrite);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void CanTimeoutPropertyShouldMatchUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Equal(innerStream.CanTimeout, byteCountingStream.CanTimeout);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void PositionPropertyShouldMatchUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            var byteCount = Encoding.UTF8.GetByteCount(DefaultStreamContent);
            var buffer = new byte[byteCount];
            
            byteCountingStream.Write(buffer);

            byteCountingStream.Position = 1;

            Assert.Equal(1, innerStream.Position);
            Assert.Equal(1, byteCountingStream.Position);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void ReadTimeoutPropertyShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            // FIXME: change the inner stream to something that supports timeouts
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Throws<InvalidOperationException>(() => byteCountingStream.ReadTimeout = 1);
            Assert.Throws<InvalidOperationException>(() => _ = byteCountingStream.ReadTimeout);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void WriteTimeoutPropertyShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            // FIXME: change the inner stream to something that supports timeouts
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Throws<InvalidOperationException>(() => byteCountingStream.WriteTimeout = 1);
            Assert.Throws<InvalidOperationException>(() => _ = byteCountingStream.WriteTimeout);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void SetLengthShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            
            byteCountingStream.SetLength(10);

            Assert.Equal(10, innerStream.Length);
            Assert.Equal(10, byteCountingStream.Length);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void SeekShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);

            byteCountingStream.Seek(1, SeekOrigin.Current);

            Assert.Equal(1, innerStream.Position);
            Assert.Equal(1, byteCountingStream.Position);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        // Does ToString() actually need to delegate? Makes the debug output potentially confusing
        public void ToStringShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);
            Assert.Equal(innerStream.ToString(), byteCountingStream.ToString());
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public async Task ReadAsyncShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count;  },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];
            var bytesRead = await byteCountingStream.ReadAsync(buffer, CancellationToken.None);

            Assert.Equal(bytesRead, bytesReceived);
            Assert.Equal(expectedByteCount, bytesReceived);
            Assert.Equal(0, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public async Task ReadAsyncWithOffsetAndLengthShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];
            var bytesRead = await byteCountingStream.ReadAsync(buffer, 0, expectedByteCount);

            Assert.Equal(bytesRead, bytesReceived);
            Assert.Equal(expectedByteCount, bytesReceived);
            Assert.Equal(0, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public async Task ReadAsyncWithOffsetAndLengthAndTokenShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];
            var bytesRead = await byteCountingStream.ReadAsync(buffer, 0, expectedByteCount, CancellationToken.None);

            Assert.Equal(bytesRead, bytesReceived);
            Assert.Equal(expectedByteCount, bytesReceived);
            Assert.Equal(0, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void ReadShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];
            var bytesRead = byteCountingStream.Read(buffer);

            Assert.Equal(bytesRead, bytesReceived);
            Assert.Equal(expectedByteCount, bytesReceived);
            Assert.Equal(0, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void ReadWithOffsetAndLengthShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];
            var bytesRead = byteCountingStream.Read(buffer, 0, expectedByteCount);

            Assert.Equal(bytesRead, bytesReceived);
            Assert.Equal(expectedByteCount, bytesReceived);
            Assert.Equal(0, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void EndReadShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;
            var testTimeout = TimeSpan.FromMilliseconds(1000);

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];

            var waitHandle = new ManualResetEvent(false);
            var asyncState = "Test";
            var callback = new AsyncCallback(ar => 
            {
                Assert.Equal(ar.AsyncState, asyncState);
                var bytesRead = byteCountingStream.EndRead(ar);
                Assert.Equal(bytesRead, bytesReceived);
                Assert.Equal(expectedByteCount, bytesReceived);
                Assert.Equal(0, bytesSent);
                waitHandle.Set();
            });

            var ar = byteCountingStream.BeginRead(buffer, 0, buffer.Length, callback, asyncState);
            waitHandle.WaitOne(testTimeout);
            Assert.True(ar.IsCompleted);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void ReadByteShouldCallUpdateBytesReceivedCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var byteReceived = byteCountingStream.ReadByte();

            Assert.Equal(Encoding.UTF8.GetBytes(DefaultStreamContent).First(), byteReceived);
            Assert.Equal(1, bytesReceived);
            Assert.Equal(0, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public async Task WriteAsyncShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var buffer = Encoding.UTF8.GetBytes(DefaultStreamContent);
            await byteCountingStream.WriteAsync(buffer, CancellationToken.None);

            Assert.Equal(0, bytesReceived);
            Assert.Equal(buffer.Length, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public async Task WriteAsyncWithOffsetAndLengthShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var buffer = Encoding.UTF8.GetBytes(DefaultStreamContent);
            await byteCountingStream.WriteAsync(buffer, 0, buffer.Length);

            Assert.Equal(0, bytesReceived);
            Assert.Equal(buffer.Length, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public async Task WriteAsyncWithOffsetAndLengthAndTokenShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var buffer = Encoding.UTF8.GetBytes(DefaultStreamContent);
            await byteCountingStream.WriteAsync(buffer, 0, buffer.Length, CancellationToken.None);

            Assert.Equal(0, bytesReceived);
            Assert.Equal(buffer.Length, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void WriteShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var buffer = Encoding.UTF8.GetBytes(DefaultStreamContent);
            byteCountingStream.Write(buffer);

            Assert.Equal(0, bytesReceived);
            Assert.Equal(buffer.Length, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void WriteWithOffsetAndLengthShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var buffer = Encoding.UTF8.GetBytes(DefaultStreamContent);
            byteCountingStream.Write(buffer, 0, buffer.Length);

            Assert.Equal(0, bytesReceived);
            Assert.Equal(buffer.Length, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void WriteByteShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var buffer = Encoding.UTF8.GetBytes(DefaultStreamContent);
            byteCountingStream.WriteByte(buffer.First());

            Assert.Equal(0, bytesReceived);
            Assert.Equal(1, bytesSent);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void EndWriteShouldCallUpdateBytesSentCallback(MemoryStream innerStream)
        {
            var bytesSent = 0;
            var bytesReceived = 0;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                UpdateBytesSentCallback = count => { bytesSent += count; },
                UpdateBytesReceivedCallback = count => { bytesReceived += count; },
            };

            var expectedByteCount = innerStream.ToArray().Length;
            var buffer = new byte[expectedByteCount];
            
            var asyncState = "Test";
            object? asyncStatePassedToCallback = null;
            var waitHandle = new ManualResetEvent(false);
            var callback = new AsyncCallback(asyncResult =>
            {
                asyncStatePassedToCallback = asyncResult.AsyncState;
                waitHandle.Set();
            });

            var ar = byteCountingStream.BeginWrite(buffer, 0, buffer.Length, callback, asyncState);

            waitHandle.WaitOne();
            Assert.True(ar.IsCompleted);

            byteCountingStream.EndWrite(ar);

            Assert.Equal(asyncState, asyncStatePassedToCallback);

            Assert.Equal(expectedByteCount, bytesSent);
            Assert.Equal(0, bytesReceived);
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public void CopyToShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);

            var streamToCopyTo = new MemoryStream();
            byteCountingStream.CopyTo(streamToCopyTo);

            Assert.Equal(innerStream.ToArray(), streamToCopyTo.ToArray());
        }

        [Theory]
        [ByteCountingStreamAutoData(DefaultStreamContent)]
        public async Task CopyToAsyncShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var byteCountingStream = new ByteCountingStream(innerStream);

            var streamToCopyTo = new MemoryStream();
            await byteCountingStream.CopyToAsync(streamToCopyTo);

            Assert.Equal(innerStream.ToArray(), streamToCopyTo.ToArray());
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void DisposeShouldInvokeDisposeCallback(MemoryStream innerStream)
        {
            var disposeCalled = false;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                DisposeCallback = stream =>
                {
                    disposeCalled = true;
                },
            };
            
            byteCountingStream.Dispose();
            Assert.True(disposeCalled);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public async Task DisposeAsyncShouldInvokeDisposeCallback(MemoryStream innerStream)
        {
            var disposeCalled = false;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                DisposeCallback = stream =>
                {
                    disposeCalled = true;
                },
            };

            await byteCountingStream.DisposeAsync();
            Assert.True(disposeCalled);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void CloseShouldInvokeDisposeCallback(MemoryStream innerStream)
        {
            var disposeCalled = false;

            var byteCountingStream = new ByteCountingStream(innerStream)
            {
                DisposeCallback = stream =>
                {
                    disposeCalled = true;
                },
            };

            byteCountingStream.Close();
            Assert.True(disposeCalled);
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public void FlushShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var bytesToWrite = Encoding.UTF8.GetBytes(DefaultStreamContent);
            var bufferedStream = new BufferedStream(innerStream, bytesToWrite.Length + 1);
            var byteCountingStream = new ByteCountingStream(bufferedStream);
            byteCountingStream.Write(bytesToWrite);

            // This test relies on the fact that the buffered stream will not pass its
            // content to the underlying stream until its buffer is full or Flush() is called.
            // So we set the buffer length to be longer than the content so that a call to Flush()
            // will definately have a side-effect we can test.
            //
            // See here for more info: https://docs.microsoft.com/en-us/dotnet/api/system.io.bufferedstream.flush?view=net-6.0#remarks
            Assert.Equal(0, innerStream.Position);
            byteCountingStream.Flush();
            
            Assert.Equal(bytesToWrite.Length, innerStream.Position);
            Assert.Equal(bytesToWrite.Length, byteCountingStream.Position);            
        }

        [Theory]
        [ByteCountingStreamAutoData]
        public async Task FlushAsyncShouldDelegateToUnderlyingStream(MemoryStream innerStream)
        {
            var bytesToWrite = Encoding.UTF8.GetBytes(DefaultStreamContent);
            var bufferedStream = new BufferedStream(innerStream, bytesToWrite.Length + 1);
            var byteCountingStream = new ByteCountingStream(bufferedStream);
            await byteCountingStream.WriteAsync(bytesToWrite);

            // This test relies on the fact that the buffered stream will not pass its
            // content to the underlying stream until its buffer is full or Flush() is called.
            // So we set the buffer length to be longer than the content so that a call to Flush()
            // will definately have a side-effect we can test.
            //
            // See here for more info: https://docs.microsoft.com/en-us/dotnet/api/system.io.bufferedstream.flush?view=net-6.0#remarks
            Assert.Equal(0, innerStream.Position);
            await byteCountingStream.FlushAsync(CancellationToken.None);

            Assert.Equal(bytesToWrite.Length, innerStream.Position);
            Assert.Equal(bytesToWrite.Length, byteCountingStream.Position);
        }
    }
}
